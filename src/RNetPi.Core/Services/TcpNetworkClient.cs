using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RNetPi.Core.Packets;
using RNetPi.Core.Logging;

namespace RNetPi.Core.Services;

/// <summary>
/// TCP implementation of the network client
/// </summary>
public class TcpNetworkClient : NetworkClient, IDisposable
{
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;
    private readonly CancellationTokenSource _cancellationTokenSource;
    
    private readonly byte[] _pendingBuffer = new byte[255];
    private int _pendingBytesRemaining = 0;
    private byte _pendingPacketType = 0;
    private int _pendingBufferIndex = 0;
    
    private bool _disposed = false;

    public TcpNetworkClient(TcpClient tcpClient, ILogger<TcpNetworkClient>? logger = null)
    {
        _tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
        _stream = _tcpClient.GetStream();
        base._logger = logger; // Set the base class logger
        _cancellationTokenSource = new CancellationTokenSource();

        // Start receiving data
        _ = Task.Run(ReceiveDataAsync);
    }

    public override string GetAddress()
    {
        try
        {
            return _tcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    public override async Task SendPacketAsync(PacketS2C packet)
    {
        if (_disposed || !_tcpClient.Connected) return;

        try
        {
            var buffer = packet.GetBuffer();
            await _stream.WriteAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
            await _stream.FlushAsync(_cancellationTokenSource.Token);
            
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
        if (_disposed || !_tcpClient.Connected) return;

        try
        {
            await _stream.WriteAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
            await _stream.FlushAsync(_cancellationTokenSource.Token);
            
            _logger?.LogTrace("Sent buffer to {Address} ({Size} bytes)", GetAddress(), buffer.Length);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send buffer to {Address}", GetAddress());
            throw;
        }
    }

    public override Task DisconnectAsync()
    {
        if (_disposed) return Task.CompletedTask;

        try
        {
            _cancellationTokenSource.Cancel();
            _tcpClient.Close();
            _logger?.LogDebug("Disconnected client {Address}", GetAddress());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error disconnecting client {Address}", GetAddress());
        }

        OnDisconnected();
        return Task.CompletedTask;
    }

    private async Task ReceiveDataAsync()
    {
        var buffer = new byte[1024];
        
        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested && _tcpClient.Connected)
            {
                var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
                if (bytesRead == 0)
                {
                    // Connection closed
                    break;
                }

                ProcessReceivedData(buffer, bytesRead);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error receiving data from {Address}", GetAddress());
        }
        finally
        {
            OnDisconnected();
        }
    }

    private void ProcessReceivedData(byte[] incomingData, int length)
    {
        int offset = 0;

        while (offset < length)
        {
            if (_pendingBytesRemaining == 0)
            {
                // Read packet header (type and length)
                if (offset < length)
                {
                    _pendingPacketType = incomingData[offset++];
                }
                if (offset < length)
                {
                    _pendingBytesRemaining = incomingData[offset++];
                    _pendingBufferIndex = 0;
                }
            }
            else
            {
                // Read packet data
                int bytesToRead = Math.Min(_pendingBytesRemaining, length - offset);
                Array.Copy(incomingData, offset, _pendingBuffer, _pendingBufferIndex, bytesToRead);
                
                offset += bytesToRead;
                _pendingBufferIndex += bytesToRead;
                _pendingBytesRemaining -= bytesToRead;
            }

            // If we have a complete packet, process it
            if (_pendingBytesRemaining == 0 && _pendingBufferIndex > 0)
            {
                var packetData = new byte[_pendingBufferIndex];
                Array.Copy(_pendingBuffer, packetData, _pendingBufferIndex);
                
                HandlePacket(_pendingPacketType, packetData);
                
                // Reset for next packet
                _pendingBufferIndex = 0;
                _pendingPacketType = 0;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _cancellationTokenSource.Cancel();
        _stream?.Dispose();
        _tcpClient?.Close();
        _cancellationTokenSource?.Dispose();
    }
}