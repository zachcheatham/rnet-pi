using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RNetPi.Core.Packets;
using RNetPi.Core.Logging;

namespace RNetPi.Core.Services;

/// <summary>
/// WebSocket implementation of the network client
/// </summary>
public class WebSocketNetworkClient : NetworkClient, IDisposable
{
    private readonly WebSocket _webSocket;
    private readonly string _remoteAddress;
    private readonly CancellationTokenSource _cancellationTokenSource;
    
    private bool _disposed = false;

    public WebSocketNetworkClient(WebSocket webSocket, string remoteAddress, ILogger<WebSocketNetworkClient>? logger = null)
    {
        _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
        _remoteAddress = remoteAddress ?? "Unknown";
        base._logger = logger; // Set the base class logger
        _cancellationTokenSource = new CancellationTokenSource();

        // Start receiving data
        _ = Task.Run(ReceiveDataAsync);
    }

    public override string GetAddress()
    {
        return _remoteAddress;
    }

    public override async Task SendPacketAsync(PacketS2C packet)
    {
        if (_disposed || _webSocket.State != WebSocketState.Open) return;

        try
        {
            var buffer = packet.GetBuffer();
            await _webSocket.SendAsync(
                new ArraySegment<byte>(buffer), 
                WebSocketMessageType.Binary, 
                true, 
                _cancellationTokenSource.Token);
            
            _logger?.LogSentPacket(packet.GetType().Name, buffer, $"to {GetAddress()} ({buffer.Length} bytes)");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send packet to {Address}", GetAddress());
            throw;
        }
    }

    public override async Task SendBufferAsync(byte[] buffer)
    {
        if (_disposed || _webSocket.State != WebSocketState.Open) return;

        try
        {
            await _webSocket.SendAsync(
                new ArraySegment<byte>(buffer), 
                WebSocketMessageType.Binary, 
                true, 
                _cancellationTokenSource.Token);
            
            _logger?.LogSentPacket("RawBuffer", buffer, $"to {GetAddress()} ({buffer.Length} bytes)");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send buffer to {Address}", GetAddress());
            throw;
        }
    }

    public override async Task DisconnectAsync()
    {
        if (_disposed) return;

        try
        {
            _cancellationTokenSource.Cancel();
            
            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, 
                    "Server initiated close", 
                    CancellationToken.None);
            }
            
            _logger?.LogDebug("Disconnected WebSocket client {Address}", GetAddress());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error disconnecting WebSocket client {Address}", GetAddress());
        }

        OnDisconnected();
    }

    private async Task ReceiveDataAsync()
    {
        var buffer = new byte[1024];
        
        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested && 
                   _webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), 
                    _cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    // We don't accept text messages, close the connection
                    _logger?.LogWarning("WebSocket client at {Address} attempted to send unaccepted text message", GetAddress());
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.InvalidMessageType,
                        "Text messages not supported",
                        CancellationToken.None);
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    if (result.Count >= 2)
                    {
                        var packetType = buffer[0];
                        // Skip the length byte at position 1
                        var dataLength = result.Count - 2;
                        var packetData = new byte[dataLength];
                        Array.Copy(buffer, 2, packetData, 0, dataLength);
                        
                        HandlePacket(packetType, packetData);
                    }
                    else
                    {
                        _logger?.LogWarning("Received malformed WebSocket binary message from {Address}", GetAddress());
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (WebSocketException ex)
        {
            _logger?.LogDebug(ex, "WebSocket connection closed for {Address}", GetAddress());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error receiving data from WebSocket client {Address}", GetAddress());
        }
        finally
        {
            OnDisconnected();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _cancellationTokenSource.Cancel();
        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}