using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RNetPi.Core.Packets;

namespace RNetPi.Core.Services;

/// <summary>
/// WebSocket server for handling RNet client connections using HttpListener
/// </summary>
public class WebSocketNetworkServer : IDisposable
{
    private readonly ILogger<WebSocketNetworkServer> _logger;
    private readonly IPAddress _bindAddress;
    private readonly int _port;
    
    private readonly ConcurrentDictionary<string, WebSocketNetworkClient> _clients;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private HttpListener? _httpListener;
    private Task? _listenerTask;
    private bool _disposed = false;

    /// <summary>
    /// Event fired when the server starts listening
    /// </summary>
    public event EventHandler? Started;

    /// <summary>
    /// Event fired when a client connects and subscribes
    /// </summary>
    public event EventHandler<WebSocketNetworkClient>? ClientConnected;

    /// <summary>
    /// Event fired when a client disconnects
    /// </summary>
    public event EventHandler<WebSocketNetworkClient>? ClientDisconnected;

    /// <summary>
    /// Event fired when a packet is received from a client
    /// </summary>
    public event EventHandler<(WebSocketNetworkClient Client, PacketC2S Packet)>? PacketReceived;

    /// <summary>
    /// Event fired when an error occurs
    /// </summary>
    public event EventHandler<Exception>? Error;

    /// <summary>
    /// Gets the number of connected clients
    /// </summary>
    public int ClientCount => _clients.Count;

    public WebSocketNetworkServer(string? host, int port, ILogger<WebSocketNetworkServer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _port = port;
        _clients = new ConcurrentDictionary<string, WebSocketNetworkClient>();
        _cancellationTokenSource = new CancellationTokenSource();

        // Parse bind address
        if (string.IsNullOrEmpty(host) || host == "0.0.0.0")
        {
            _bindAddress = IPAddress.Any;
        }
        else if (!IPAddress.TryParse(host, out var parsedAddress))
        {
            throw new ArgumentException($"Invalid host address: {host}", nameof(host));
        }
        else
        {
            _bindAddress = parsedAddress;
        }
    }

    /// <summary>
    /// Starts the WebSocket server
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    public async Task StartAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(WebSocketNetworkServer));

        try
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"http://{(_bindAddress.Equals(IPAddress.Any) ? "+" : _bindAddress)}:{_port}/");

            _httpListener.Start();
            _logger.LogInformation("WebSocket Server listening on {Address}:{Port}", _bindAddress, _port);

            _listenerTask = HandleHttpRequestsAsync(_cancellationTokenSource.Token);
            
            Started?.Invoke(this, EventArgs.Empty);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start WebSocket server");
            Error?.Invoke(this, ex);
            throw;
        }
    }

    /// <summary>
    /// Stops the WebSocket server
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    public async Task StopAsync()
    {
        if (_disposed) return;

        try
        {
            _cancellationTokenSource.Cancel();

            // Stop the HTTP listener
            _httpListener?.Stop();

            // Disconnect all clients
            var disconnectTasks = new List<Task>();
            foreach (var client in _clients.Values)
            {
                disconnectTasks.Add(client.DisconnectAsync());
            }
            
            if (disconnectTasks.Count > 0)
            {
                await Task.WhenAll(disconnectTasks);
            }

            // Wait for listener task to complete
            if (_listenerTask != null)
            {
                try
                {
                    await _listenerTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected
                }
            }

            _logger.LogInformation("WebSocket Server stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping WebSocket server");
            Error?.Invoke(this, ex);
        }
    }

    /// <summary>
    /// Broadcasts a packet to all connected and subscribed clients
    /// </summary>
    /// <param name="packet">The packet to broadcast</param>
    /// <returns>Task representing the async operation</returns>
    public async Task BroadcastPacketAsync(PacketS2C packet)
    {
        var buffer = packet.GetBuffer();
        await BroadcastBufferAsync(buffer);
    }

    /// <summary>
    /// Broadcasts a buffer to all connected and subscribed clients
    /// </summary>
    /// <param name="buffer">The buffer to broadcast</param>
    /// <returns>Task representing the async operation</returns>
    public async Task BroadcastBufferAsync(byte[] buffer)
    {
        var tasks = new List<Task>();
        
        foreach (var client in _clients.Values)
        {
            if (client.IsSubscribed)
            {
                tasks.Add(client.SendBufferAsync(buffer));
            }
        }

        if (tasks.Count > 0)
        {
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting to WebSocket clients");
                Error?.Invoke(this, ex);
            }
        }
    }

    /// <summary>
    /// Gets the server's listening address
    /// </summary>
    /// <returns>String representation of the server address</returns>
    public string GetAddress()
    {
        return $"{_bindAddress}:{_port}";
    }

    private async Task HandleHttpRequestsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _httpListener != null && _httpListener.IsListening)
        {
            try
            {
                var context = await _httpListener.GetContextAsync();
                _ = Task.Run(() => HandleHttpContextAsync(context), cancellationToken);
            }
            catch (HttpListenerException)
            {
                // Expected when stopping
                break;
            }
            catch (ObjectDisposedException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error handling HTTP request");
                    Error?.Invoke(this, ex);
                }
            }
        }
    }

    private async Task HandleHttpContextAsync(HttpListenerContext context)
    {
        try
        {
            if (context.Request.IsWebSocketRequest)
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                var remoteAddress = context.Request.RemoteEndPoint?.Address?.ToString() ?? "Unknown";
                
                _logger.LogDebug("New WebSocket connection from {Address}", remoteAddress);

                var networkClient = new WebSocketNetworkClient(webSocketContext.WebSocket, remoteAddress, 
                    new WebSocketClientLoggerWrapper(_logger));
                    
                HandleNewClient(networkClient);

                // Keep the connection alive until it closes
                while (webSocketContext.WebSocket.State == WebSocketState.Open && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(1000, _cancellationTokenSource.Token);
                }
            }
            else
            {
                context.Response.StatusCode = 400;
                var message = Encoding.UTF8.GetBytes("WebSocket request expected");
                await context.Response.OutputStream.WriteAsync(message, 0, message.Length);
                context.Response.Close();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling WebSocket request");
            context.Response.StatusCode = 500;
            context.Response.Close();
        }
    }

    private void HandleNewClient(WebSocketNetworkClient client)
    {
        var clientAddress = client.GetAddress();

        // Set up client event handlers
        client.Subscribed += (sender, e) =>
        {
            _clients[clientAddress] = client;
            _logger.LogInformation("WebSocket client {Address} subscribed", clientAddress);
            ClientConnected?.Invoke(this, client);
        };

        client.Disconnected += (sender, e) =>
        {
            if (_clients.TryRemove(clientAddress, out var removedClient) && 
                removedClient.IsSubscribed)
            {
                _logger.LogInformation("WebSocket client {Address} disconnected", clientAddress);
                ClientDisconnected?.Invoke(this, client);
            }
            client.Dispose();
        };

        client.PacketReceived += (sender, packet) =>
        {
            PacketReceived?.Invoke(this, (client, packet));
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        StopAsync().Wait(TimeSpan.FromSeconds(5));
        
        foreach (var client in _clients.Values)
        {
            client.Dispose();
        }
        
        _httpListener?.Close();
        _cancellationTokenSource?.Dispose();
    }
}

/// <summary>
/// Logger wrapper for WebSocket client
/// </summary>
internal class WebSocketClientLoggerWrapper : ILogger<WebSocketNetworkClient>
{
    private readonly ILogger<WebSocketNetworkServer> _logger;

    public WebSocketClientLoggerWrapper(ILogger<WebSocketNetworkServer> logger)
    {
        _logger = logger;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _logger.Log(logLevel, eventId, state, exception, (s, e) => $"[Client] {formatter(s, e)}");
    }
}