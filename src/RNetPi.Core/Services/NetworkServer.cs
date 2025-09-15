using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RNetPi.Core.Packets;

namespace RNetPi.Core.Services;

/// <summary>
/// Configuration for the network server
/// </summary>
public class NetworkServerConfig
{
    public string Name { get; set; } = "RNet-Pi";
    public string Host { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 4000;
    public string? WebHost { get; set; }
    public int? WebPort { get; set; }
}

/// <summary>
/// Main network server that coordinates TCP and WebSocket servers
/// </summary>
public class NetworkServer : IDisposable
{
    private readonly ILogger<NetworkServer> _logger;
    private readonly NetworkServerConfig _config;
    
    private TcpNetworkServer? _tcpServer;
    private WebSocketNetworkServer? _webSocketServer;
    private bool _disposed = false;

    /// <summary>
    /// Event fired when the server starts
    /// </summary>
    public event EventHandler? Started;

    /// <summary>
    /// Event fired when a client connects
    /// </summary>
    public event EventHandler<NetworkClient>? ClientConnected;

    /// <summary>
    /// Event fired when a client disconnects
    /// </summary>
    public event EventHandler<NetworkClient>? ClientDisconnected;

    /// <summary>
    /// Event fired when a packet is received
    /// </summary>
    public event EventHandler<(NetworkClient Client, PacketC2S Packet)>? PacketReceived;

    /// <summary>
    /// Event fired when an error occurs
    /// </summary>
    public event EventHandler<Exception>? Error;

    /// <summary>
    /// Gets the server name
    /// </summary>
    public string Name => _config.Name;

    /// <summary>
    /// Gets the total number of connected clients
    /// </summary>
    public int ClientCount => (_tcpServer?.ClientCount ?? 0) + (_webSocketServer?.ClientCount ?? 0);

    public NetworkServer(NetworkServerConfig config, ILogger<NetworkServer> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Starts the network server (TCP and optionally WebSocket)
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    public async Task StartAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(NetworkServer));

        try
        {
            // Start TCP server
            _tcpServer = new TcpNetworkServer(_config.Name, _config.Host, _config.Port, 
                new TcpServerLoggerWrapper(_logger));
            
            SetupTcpServerEvents();
            await _tcpServer.StartAsync();

            // Start WebSocket server if configured
            if (_config.WebPort.HasValue)
            {
                _webSocketServer = new WebSocketNetworkServer(_config.WebHost ?? _config.Host, 
                    _config.WebPort.Value, new WebSocketServerLoggerWrapper(_logger));
                
                SetupWebSocketServerEvents();
                await _webSocketServer.StartAsync();
            }

            _logger.LogInformation("Network server started successfully");
            Started?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start network server");
            Error?.Invoke(this, ex);
            throw;
        }
    }

    /// <summary>
    /// Stops the network server
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    public async Task StopAsync()
    {
        if (_disposed) return;

        try
        {
            var tasks = new List<Task>();

            if (_tcpServer != null)
            {
                tasks.Add(_tcpServer.StopAsync());
            }

            if (_webSocketServer != null)
            {
                tasks.Add(_webSocketServer.StopAsync());
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }

            _logger.LogInformation("Network server stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping network server");
            Error?.Invoke(this, ex);
        }
    }

    /// <summary>
    /// Broadcasts a packet to all connected clients
    /// </summary>
    /// <param name="packet">The packet to broadcast</param>
    /// <returns>Task representing the async operation</returns>
    public async Task BroadcastAsync(PacketS2C packet)
    {
        var tasks = new List<Task>();

        if (_tcpServer != null)
        {
            tasks.Add(_tcpServer.BroadcastPacketAsync(packet));
        }

        if (_webSocketServer != null)
        {
            tasks.Add(_webSocketServer.BroadcastPacketAsync(packet));
        }

        if (tasks.Count > 0)
        {
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting packet");
                Error?.Invoke(this, ex);
            }
        }
    }

    /// <summary>
    /// Sets the server name and updates it across all servers
    /// </summary>
    /// <param name="name">The new server name</param>
    public void SetName(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        _config.Name = name;
        // Note: TCP server name update would require Bonjour service republishing
        _logger.LogInformation("Server name updated to: {Name}", name);
    }

    /// <summary>
    /// Gets the TCP server address
    /// </summary>
    /// <returns>TCP server address or null if not started</returns>
    public string? GetTcpAddress()
    {
        return _tcpServer?.GetAddress();
    }

    /// <summary>
    /// Gets the WebSocket server address
    /// </summary>
    /// <returns>WebSocket server address or null if not started</returns>
    public string? GetWebSocketAddress()
    {
        return _webSocketServer?.GetAddress();
    }

    private void SetupTcpServerEvents()
    {
        if (_tcpServer == null) return;

        _tcpServer.ClientConnected += (sender, client) =>
        {
            _logger.LogDebug("TCP client connected: {Address}", client.GetAddress());
            ClientConnected?.Invoke(this, client);
        };

        _tcpServer.ClientDisconnected += (sender, client) =>
        {
            _logger.LogDebug("TCP client disconnected: {Address}", client.GetAddress());
            ClientDisconnected?.Invoke(this, client);
        };

        _tcpServer.PacketReceived += (sender, data) =>
        {
            PacketReceived?.Invoke(this, (data.Client, data.Packet));
        };

        _tcpServer.Error += (sender, ex) =>
        {
            _logger.LogError(ex, "TCP server error");
            Error?.Invoke(this, ex);
        };
    }

    private void SetupWebSocketServerEvents()
    {
        if (_webSocketServer == null) return;

        _webSocketServer.ClientConnected += (sender, client) =>
        {
            _logger.LogDebug("WebSocket client connected: {Address}", client.GetAddress());
            ClientConnected?.Invoke(this, client);
        };

        _webSocketServer.ClientDisconnected += (sender, client) =>
        {
            _logger.LogDebug("WebSocket client disconnected: {Address}", client.GetAddress());
            ClientDisconnected?.Invoke(this, client);
        };

        _webSocketServer.PacketReceived += (sender, data) =>
        {
            PacketReceived?.Invoke(this, (data.Client, data.Packet));
        };

        _webSocketServer.Error += (sender, ex) =>
        {
            _logger.LogError(ex, "WebSocket server error");
            Error?.Invoke(this, ex);
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        StopAsync().Wait(TimeSpan.FromSeconds(10));
        
        _tcpServer?.Dispose();
        _webSocketServer?.Dispose();
    }
}

/// <summary>
/// Logger wrapper for TCP server
/// </summary>
internal class TcpServerLoggerWrapper : ILogger<TcpNetworkServer>
{
    private readonly ILogger<NetworkServer> _logger;

    public TcpServerLoggerWrapper(ILogger<NetworkServer> logger)
    {
        _logger = logger;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _logger.Log(logLevel, eventId, state, exception, (s, e) => $"[TCP] {formatter(s, e)}");
    }
}

/// <summary>
/// Logger wrapper for WebSocket server
/// </summary>
internal class WebSocketServerLoggerWrapper : ILogger<WebSocketNetworkServer>
{
    private readonly ILogger<NetworkServer> _logger;

    public WebSocketServerLoggerWrapper(ILogger<NetworkServer> logger)
    {
        _logger = logger;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _logger.Log(logLevel, eventId, state, exception, (s, e) => $"[WebSocket] {formatter(s, e)}");
    }
}