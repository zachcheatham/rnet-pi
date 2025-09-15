# RNet Network Server Documentation

This document describes the ported network server functionality that allows remote clients to connect and control RNet systems over TCP and WebSocket connections.

## Overview

The network server provides a bridge between RNet hardware and remote client applications. It supports both TCP and WebSocket connections, allowing various types of clients to connect and send control commands.

## Architecture

The network server is built with the following components:

### Core Classes

- **`NetworkServer`**: Main coordinator class that manages both TCP and WebSocket servers
- **`TcpNetworkServer`**: Handles TCP client connections using standard sockets
- **`WebSocketNetworkServer`**: Handles WebSocket client connections using HttpListener
- **`NetworkClient`**: Abstract base class for client connections
- **`TcpNetworkClient`**: TCP implementation of network client
- **`WebSocketNetworkClient`**: WebSocket implementation of network client
- **`PacketFactory`**: Factory for creating packet instances from raw data

### Configuration

```csharp
var config = new NetworkServerConfig
{
    Name = "RNet-Pi",           // Server name for Bonjour/mDNS discovery
    Host = "0.0.0.0",           // Bind address (0.0.0.0 for all interfaces)
    Port = 4000,                // TCP port
    WebHost = "0.0.0.0",        // WebSocket bind address (optional)
    WebPort = 4001              // WebSocket port (optional)
};
```

## Usage

### Basic Setup

```csharp
using Microsoft.Extensions.Logging;
using RNetPi.Core.Services;

// Create logger
var logger = LoggerFactory.Create(builder => builder.AddConsole())
    .CreateLogger<NetworkServer>();

// Configure server
var config = new NetworkServerConfig
{
    Name = "My RNet Controller",
    Host = "0.0.0.0",
    Port = 4000,
    WebPort = 4001
};

// Create and start server
var server = new NetworkServer(config, logger);
await server.StartAsync();
```

### Handling Events

```csharp
// Client connection events
server.ClientConnected += (sender, client) =>
{
    Console.WriteLine($"Client connected: {client.GetAddress()}");
};

server.ClientDisconnected += (sender, client) =>
{
    Console.WriteLine($"Client disconnected: {client.GetAddress()}");
};

// Packet processing
server.PacketReceived += async (sender, data) =>
{
    var (client, packet) = data;
    await HandlePacket(client, packet);
};
```

### Processing Packets

```csharp
private async Task HandlePacket(NetworkClient client, PacketC2S packet)
{
    switch (packet)
    {
        case PacketC2SZonePower zonePower:
            // Handle zone power command
            Console.WriteLine($"Zone {zonePower.GetZoneID()} power: {zonePower.GetPower()}");
            
            // Send response to all clients
            var response = new PacketS2CZonePower();
            // Configure response...
            await server.BroadcastAsync(response);
            break;
            
        case PacketC2SZoneVolume zoneVolume:
            // Handle zone volume command
            Console.WriteLine($"Zone {zoneVolume.GetZoneID()} volume: {zoneVolume.GetVolume()}");
            break;
            
        // Handle other packet types...
    }
}
```

### Broadcasting Updates

```csharp
// Broadcast to all connected clients
var zoneUpdate = new PacketS2CZoneVolume();
// Configure packet...
await server.BroadcastAsync(zoneUpdate);
```

## Client Connection Flow

1. **Initial Connection**: Client connects via TCP or WebSocket
2. **Intent Declaration**: Client sends `PacketC2SIntent` with subscribe intent (0x02)
3. **Subscription**: Server marks client as subscribed and starts forwarding packets
4. **Communication**: Bidirectional packet exchange
5. **Disconnection**: Client disconnects or server closes connection

## Packet Types Supported

The packet factory supports creating the following client-to-server packet types:

| ID   | Packet Type | Description |
|------|-------------|-------------|
| 0x01 | PacketC2SIntent | Client intent declaration |
| 0x02 | PacketC2SProperty | Property update |
| 0x03 | PacketC2SDisconnect | Disconnect request |
| 0x04 | PacketC2SZoneName | Zone name change |
| 0x05 | PacketC2SDeleteZone | Delete zone |
| 0x06 | PacketC2SSourceInfo | Source information |
| 0x07 | PacketC2SDeleteSource | Delete source |
| 0x08 | PacketC2SZonePower | Zone power control |
| 0x09 | PacketC2SZoneVolume | Zone volume control |
| 0x0A | PacketC2SZoneSource | Zone source selection |
| 0x0B | PacketC2SZoneParameter | Zone parameter change |
| 0x0C | PacketC2SAllPower | All zones power control |
| 0x0D | PacketC2SMute | Mute control |
| 0x32 | PacketC2SSourceControl | Source control |
| 0x33 | PacketC2SRequestSourceProperties | Request source properties |
| 0x34 | PacketC2SSourceProperty | Source property update |
| 0x64 | PacketC2SZoneMaxVolume | Zone maximum volume |
| 0x65 | PacketC2SZoneMute | Zone mute control |
| 0x7D | PacketC2SUpdate | Update request |

## Threading and Async Patterns

The network server uses modern C# async/await patterns throughout:

- All I/O operations are asynchronous
- Event handlers can be async
- Proper cancellation token support
- Thread-safe client collections

## Error Handling

The server provides comprehensive error handling:

```csharp
server.Error += (sender, exception) =>
{
    logger.LogError(exception, "Network server error");
    // Handle error...
};
```

Common error scenarios:
- Port already in use
- Network interface unavailable
- Client disconnection during operation
- Malformed packets

## Security Considerations

- The server accepts connections from any client
- No authentication is performed by default
- Consider using firewall rules to restrict access
- WebSocket connections use standard HTTP upgrade mechanism

## Performance

- Supports multiple concurrent clients
- Efficient binary packet protocol
- Minimal memory allocations during operation
- Configurable buffer sizes

## Integration with RNet Service

The network server integrates with the existing RNet service infrastructure:

```csharp
// In your RNet service
private async Task OnRNetPacketReceived(RNetPacket packet)
{
    // Convert RNet packet to network packet
    var networkPacket = ConvertToNetworkPacket(packet);
    
    // Broadcast to all connected clients
    await networkServer.BroadcastAsync(networkPacket);
}
```

## Testing

Unit tests are provided for:
- Packet factory functionality
- Client state management
- Server configuration
- Event handling

Run tests with:
```bash
dotnet test
```

## Troubleshooting

### Common Issues

1. **Port already in use**: Change the port numbers in configuration
2. **Permission denied**: Run with elevated privileges or use ports > 1024
3. **Clients not connecting**: Check firewall settings and network configuration
4. **WebSocket handshake failures**: Ensure proper HTTP upgrade headers

### Debugging

Enable debug logging to see detailed packet information:

```csharp
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

This will show:
- Client connection/disconnection events
- Packet reception and transmission
- Error details

## Migration from JavaScript

The C# implementation maintains compatibility with the original JavaScript protocol:

- Same packet structure and IDs
- Compatible binary format
- Same TCP and WebSocket behavior
- Equivalent event handling patterns

Existing JavaScript clients should work without modification.