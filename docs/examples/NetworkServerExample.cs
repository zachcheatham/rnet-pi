using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RNetPi.Core.Packets;
using RNetPi.Core.Services;

namespace RNetPi.Examples;

/// <summary>
/// Example demonstrating how to use the RNet network server
/// </summary>
public class NetworkServerExample
{
    private readonly ILogger<NetworkServerExample> _logger;
    private NetworkServer? _networkServer;

    public NetworkServerExample(ILogger<NetworkServerExample> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Runs the network server example
    /// </summary>
    public async Task RunAsync()
    {
        _logger.LogInformation("Starting RNet Network Server Example");

        try
        {
            // Configure the network server
            var config = new NetworkServerConfig
            {
                Name = "RNet-Pi Example",
                Host = "0.0.0.0",
                Port = 4000,
                WebHost = "0.0.0.0",
                WebPort = 4001
            };

            // Create logger for network server
            var networkLogger = LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<NetworkServer>();

            // Create and configure the network server
            _networkServer = new NetworkServer(config, networkLogger);

            // Subscribe to server events
            SetupEventHandlers();

            // Start the server
            await _networkServer.StartAsync();

            _logger.LogInformation("Network server started successfully");
            _logger.LogInformation("TCP server address: {TcpAddress}", _networkServer.GetTcpAddress());
            _logger.LogInformation("WebSocket server address: {WebSocketAddress}", _networkServer.GetWebSocketAddress());

            // Keep the server running
            _logger.LogInformation("Press Ctrl+C to stop the server");
            await WaitForCancellationAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running network server example");
        }
        finally
        {
            if (_networkServer != null)
            {
                await _networkServer.StopAsync();
                _networkServer.Dispose();
            }
        }
    }

    private void SetupEventHandlers()
    {
        if (_networkServer == null) return;

        _networkServer.Started += (sender, e) =>
        {
            _logger.LogInformation("Network server started event fired");
        };

        _networkServer.ClientConnected += (sender, client) =>
        {
            _logger.LogInformation("Client connected: {Address}", client.GetAddress());
        };

        _networkServer.ClientDisconnected += (sender, client) =>
        {
            _logger.LogInformation("Client disconnected: {Address}", client.GetAddress());
        };

        _networkServer.PacketReceived += async (sender, data) =>
        {
            await HandlePacketReceived(data.Client, data.Packet);
        };

        _networkServer.Error += (sender, ex) =>
        {
            _logger.LogError(ex, "Network server error");
        };
    }

    private async Task HandlePacketReceived(NetworkClient client, PacketC2S packet)
    {
        _logger.LogInformation("Received packet {PacketType} from {Address}", 
            packet.GetType().Name, client.GetAddress());

        try
        {
            switch (packet)
            {
                case PacketC2SZonePower zonePower:
                    await HandleZonePowerPacket(client, zonePower);
                    break;

                case PacketC2SZoneVolume zoneVolume:
                    await HandleZoneVolumePacket(client, zoneVolume);
                    break;

                case PacketC2SZoneSource zoneSource:
                    await HandleZoneSourcePacket(client, zoneSource);
                    break;

                case PacketC2SAllPower allPower:
                    await HandleAllPowerPacket(client, allPower);
                    break;

                default:
                    _logger.LogDebug("Unhandled packet type: {PacketType}", packet.GetType().Name);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling packet {PacketType} from {Address}", 
                packet.GetType().Name, client.GetAddress());
        }
    }

    private async Task HandleZonePowerPacket(NetworkClient client, PacketC2SZonePower packet)
    {
        _logger.LogInformation("Zone power command: Controller {ControllerID}, Zone {ZoneID}, Power {Power}",
            packet.GetControllerID(), packet.GetZoneID(), packet.GetPower());

        // Simulate processing and send response
        var response = new PacketS2CZonePower();
        // Set response data based on packet...
        
        await BroadcastResponse(response);
    }

    private async Task HandleZoneVolumePacket(NetworkClient client, PacketC2SZoneVolume packet)
    {
        _logger.LogInformation("Zone volume command: Controller {ControllerID}, Zone {ZoneID}, Volume {Volume}",
            packet.GetControllerID(), packet.GetZoneID(), packet.GetVolume());

        // Simulate processing and send response
        var response = new PacketS2CZoneVolume();
        // Set response data based on packet...
        
        await BroadcastResponse(response);
    }

    private async Task HandleZoneSourcePacket(NetworkClient client, PacketC2SZoneSource packet)
    {
        _logger.LogInformation("Zone source command: Controller {ControllerID}, Zone {ZoneID}, Source {SourceID}",
            packet.GetControllerID(), packet.GetZoneID(), packet.GetSourceID());

        // Simulate processing and send response
        var response = new PacketS2CZoneSource();
        // Set response data based on packet...
        
        await BroadcastResponse(response);
    }

    private async Task HandleAllPowerPacket(NetworkClient client, PacketC2SAllPower packet)
    {
        _logger.LogInformation("All power command: Power {Power}", packet.GetPower());

        // Simulate processing - would typically update all zones
        // Then broadcast updates to all clients
        await BroadcastSystemUpdate();
    }

    private async Task BroadcastResponse(PacketS2C response)
    {
        if (_networkServer != null)
        {
            await _networkServer.BroadcastAsync(response);
            _logger.LogDebug("Broadcasted response: {PacketType}", response.GetType().Name);
        }
    }

    private async Task BroadcastSystemUpdate()
    {
        if (_networkServer == null) return;

        // Example: Broadcast zone updates to all connected clients
        // In a real implementation, this would get actual zone data

        for (int zoneId = 1; zoneId <= 4; zoneId++)
        {
            var powerResponse = new PacketS2CZonePower();
            // Set zone data...
            await _networkServer.BroadcastAsync(powerResponse);

            var volumeResponse = new PacketS2CZoneVolume();
            // Set zone data...
            await _networkServer.BroadcastAsync(volumeResponse);
        }

        _logger.LogDebug("Broadcasted system update to all clients");
    }

    private async Task WaitForCancellationAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            tcs.SetResult(true);
        };

        await tcs.Task;
    }
}

/// <summary>
/// Program entry point for the network server example
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        // Configure logging
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        
        var logger = loggerFactory.CreateLogger<NetworkServerExample>();

        // Run the example
        var example = new NetworkServerExample(logger);
        await example.RunAsync();
    }
}