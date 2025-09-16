using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RNetPi.Core.Packets;

namespace RNetPi.Core.Services;

/// <summary>
/// TCP server for handling RNet client connections
/// </summary>
public class TcpNetworkServer : IDisposable
{
    private readonly ILogger<TcpNetworkServer> _logger;
    private readonly string _serverName;
    private readonly IPAddress _bindAddress;
    private readonly int _port;
    
    private TcpListener? _listener;
    private readonly ConcurrentDictionary<string, TcpNetworkClient> _clients;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task? _acceptTask;
    private bool _disposed = false;

    /// <summary>
    /// Event fired when the server starts listening
    /// </summary>
    public event EventHandler? Started;

    /// <summary>
    /// Event fired when a client connects and subscribes
    /// </summary>
    public event EventHandler<TcpNetworkClient>? ClientConnected;

    /// <summary>
    /// Event fired when a client disconnects
    /// </summary>
    public event EventHandler<TcpNetworkClient>? ClientDisconnected;

    /// <summary>
    /// Event fired when a packet is received from a client
    /// </summary>
    public event EventHandler<(TcpNetworkClient Client, PacketC2S Packet)>? PacketReceived;

    /// <summary>
    /// Event fired when an error occurs
    /// </summary>
    public event EventHandler<Exception>? Error;

    /// <summary>
    /// Gets the server name
    /// </summary>
    public string ServerName => _serverName;

    /// <summary>
    /// Gets the number of connected clients
    /// </summary>
    public int ClientCount => _clients.Count;

    public TcpNetworkServer(string serverName, string? host, int port, ILogger<TcpNetworkServer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serverName = serverName ?? throw new ArgumentNullException(nameof(serverName));
        _port = port;
        _clients = new ConcurrentDictionary<string, TcpNetworkClient>();
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
    /// Starts the TCP server
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    public async Task StartAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(TcpNetworkServer));

        try
        {
            _listener = new TcpListener(_bindAddress, _port);
            _listener.Start();

            _logger.LogInformation("TCP Server listening on {Address}:{Port}", _bindAddress, _port);

            // Try to publish Bonjour/Zeroconf service if available
            await PublishBonjourServiceAsync();

            // Start accepting connections
            _acceptTask = AcceptConnectionsAsync(_cancellationTokenSource.Token);

            Started?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start TCP server");
            Error?.Invoke(this, ex);
            throw;
        }
    }

    /// <summary>
    /// Stops the TCP server
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    public async Task StopAsync()
    {
        if (_disposed) return;

        try
        {
            _cancellationTokenSource.Cancel();

            // Stop listening for new connections
            _listener?.Stop();

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

            // Wait for accept task to complete
            if (_acceptTask != null)
            {
                try
                {
                    await _acceptTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected
                }
            }

            _logger.LogInformation("TCP Server stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping TCP server");
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
                _logger.LogError(ex, "Error broadcasting to clients");
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
        if (_listener?.LocalEndpoint is IPEndPoint endpoint)
        {
            return $"{endpoint.Address}:{endpoint.Port}";
        }
        return $"{_bindAddress}:{_port}";
    }

    private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _listener != null)
        {
            try
            {
                var tcpClient = await _listener.AcceptTcpClientAsync();
                
                var networkClient = new TcpNetworkClient(tcpClient, 
                    new TcpClientLoggerWrapper(_logger));
                    
                HandleNewClient(networkClient);
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
                    _logger.LogError(ex, "Error accepting TCP connection");
                    Error?.Invoke(this, ex);
                }
            }
        }
    }

    private void HandleNewClient(TcpNetworkClient client)
    {
        var clientAddress = client.GetAddress();
        _logger.LogDebug("New TCP connection from {Address}", clientAddress);

        // Set up client event handlers
        client.Subscribed += (sender, e) =>
        {
            _clients[clientAddress] = client;
            _logger.LogInformation("Client {Address} subscribed", clientAddress);
            ClientConnected?.Invoke(this, client);
        };

        client.Disconnected += (sender, e) =>
        {
            if (_clients.TryRemove(clientAddress, out var removedClient) && 
                removedClient.IsSubscribed)
            {
                _logger.LogInformation("Client {Address} disconnected", clientAddress);
                ClientDisconnected?.Invoke(this, client);
            }
            client.Dispose();
        };

        client.PacketReceived += (sender, packet) =>
        {
            PacketReceived?.Invoke(this, (client, packet));
        };
    }

    private async Task PublishBonjourServiceAsync()
    {
        try
        {
            // Try to use Bonjour/Zeroconf if available
            // This would require a NuGet package like Zeroconf or similar
            _logger.LogInformation("Publishing Bonjour service: {Name} - rnet - {Port}", 
                _serverName, _port);
            
            // TODO: Implement Bonjour service publication
            // For now, just log that we would publish the service
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Bonjour unavailable. Remotes won't be able to automatically find this controller");
        }
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
        
        _cancellationTokenSource?.Dispose();
        _listener = null;
    }
}

/// <summary>
/// Logger wrapper for TCP client
/// </summary>
internal class TcpClientLoggerWrapper : ILogger<TcpNetworkClient>
{
    private readonly ILogger<TcpNetworkServer> _logger;

    public TcpClientLoggerWrapper(ILogger<TcpNetworkServer> logger)
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