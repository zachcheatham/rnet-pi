# RNet Library Usage Examples

This document provides comprehensive examples of how to use the ported RNet C# library.

## Basic Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RNetPi.Core.Interfaces;
using RNetPi.Core.Services;
using RNetPi.Core.Models;

// Set up dependency injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddSingleton<IConfigurationService, ConfigurationService>();
services.AddSingleton<EnhancedRNetService>();

var serviceProvider = services.BuildServiceProvider();
var rnetService = serviceProvider.GetRequiredService<EnhancedRNetService>();
```

## Connection Management

### Connecting to RNet Device

```csharp
// Connect to the RNet device
var connected = await rnetService.ConnectAsync();
if (connected)
{
    Console.WriteLine("Successfully connected to RNet device");
}
else
{
    Console.WriteLine("Failed to connect to RNet device");
}
```

### Event Handling

```csharp
// Subscribe to connection events
rnetService.Connected += (sender, e) => Console.WriteLine("RNet connected");
rnetService.Disconnected += (sender, e) => Console.WriteLine("RNet disconnected");
rnetService.Error += (sender, ex) => Console.WriteLine($"RNet error: {ex.Message}");

// Subscribe to zone and source events
rnetService.ZoneAdded += (sender, zone) => 
    Console.WriteLine($"Zone added: {zone.Name} (Controller {zone.ControllerID}, Zone {zone.ZoneID})");

rnetService.SourceAdded += (sender, source) => 
    Console.WriteLine($"Source added: {source.Name} (ID {source.SourceID})");

// Subscribe to keypad events
rnetService.KeypadEvent += (sender, keypadEvent) =>
{
    Console.WriteLine($"Keypad event: Controller {keypadEvent.GetControllerID()}, " +
                     $"Zone {keypadEvent.GetZoneID()}, Key {keypadEvent.GetKeyID()}");
};
```

## Zone Management

### Creating and Managing Zones

```csharp
// Create a new zone
var livingRoomZone = rnetService.CreateZone(0, 1, "Living Room");

// Set zone properties
livingRoomZone.SetMaxVolume(80); // Limit volume to 80%

// Subscribe to zone events
livingRoomZone.PowerChanged += power => 
    Console.WriteLine($"Living Room power: {(power ? "ON" : "OFF")}");

livingRoomZone.VolumeChanged += volume => 
    Console.WriteLine($"Living Room volume: {volume}%");

livingRoomZone.SourceChanged += sourceID => 
    Console.WriteLine($"Living Room source changed to: {sourceID}");
```

### Controlling Zones

```csharp
// Power control
rnetService.SetZonePower(0, 1, true);  // Turn on zone 1
rnetService.SetZonePower(0, 1, false); // Turn off zone 1

// Volume control
rnetService.SetZoneVolume(0, 1, 50); // Set volume to 50%

// Source selection
rnetService.SetZoneSource(0, 1, 3); // Set source to source ID 3

// Parameter adjustment (Bass, Treble, etc.)
rnetService.SetZoneParameter(0, 1, 0, 15); // Set bass to +5 (15 - 10)
rnetService.SetZoneParameter(0, 1, 1, 12); // Set treble to +2 (12 - 10)
rnetService.SetZoneParameter(0, 1, 2, 1);  // Enable loudness
```

### Requesting Zone Information

```csharp
// Request current zone information
rnetService.RequestZoneInfo(0, 1);

// The response will be handled automatically and update the zone object
var zone = rnetService.GetZone(0, 1);
if (zone != null)
{
    Console.WriteLine($"Zone: {zone.Name}");
    Console.WriteLine($"Power: {zone.Power}");
    Console.WriteLine($"Volume: {zone.Volume}%");
    Console.WriteLine($"Source: {zone.Source}");
    Console.WriteLine($"Bass: {(int)zone.GetParameter(0) - 10}");
    Console.WriteLine($"Treble: {(int)zone.GetParameter(1) - 10}");
}
```

## Source Management

### Creating and Managing Sources

```csharp
// Create different types of sources
var cableBoxSource = rnetService.CreateSource(1, "Cable Box", SourceType.Generic);
var chromecastSource = rnetService.CreateSource(2, "Chromecast", SourceType.GoogleCast);
var sonosSource = rnetService.CreateSource(3, "Sonos", SourceType.Sonos);

// Configure source properties
cableBoxSource.AutoOff = true; // Auto-off when not in use
cableBoxSource.AddAutoOnZone(0, 1); // Auto-on for living room

// Subscribe to source events
chromecastSource.MediaTitleChanged += title => 
    Console.WriteLine($"Now playing: {title}");

chromecastSource.MediaArtistChanged += artist => 
    Console.WriteLine($"Artist: {artist}");

chromecastSource.MediaPlayingChanged += playing => 
    Console.WriteLine($"Playback state: {(playing ? "Playing" : "Stopped")}");
```

### Source Control

```csharp
// Control source playback (for supported source types)
chromecastSource.Control(SourceControl.Play);
chromecastSource.Control(SourceControl.Pause);
chromecastSource.Control(SourceControl.Next);
chromecastSource.Control(SourceControl.Previous);
```

## Global System Control

### All Zones Power Control

```csharp
// Turn all zones on or off
rnetService.SetAllPower(true);  // Turn on all zones
rnetService.SetAllPower(false); // Turn off all zones

// Mute all zones
rnetService.SetAllMute(true, 1000); // Mute with 1 second fade
rnetService.SetAllMute(false, 500); // Unmute with 0.5 second fade
```

### Display Messages

```csharp
// Send display message to a specific zone
rnetService.SendDisplayMessage(0, 1, "Welcome Home!", 10); // Show for 10 seconds

// Send announcement to all zones
foreach (var zone in rnetService.GetAllZones())
{
    rnetService.SendDisplayMessage(zone.ControllerID, zone.ZoneID, "System Update", 5);
}
```

## Advanced Packet Handling

### Custom Packet Creation

```csharp
using RNetPi.Core.RNet;

// Create custom packets directly
var volumePacket = new SetVolumePacket(0, 1, 75); // Controller 0, Zone 1, Volume 75%
var powerPacket = new SetPowerPacket(0, 2, true);  // Controller 0, Zone 2, Power On

// Get packet buffer for transmission
var volumeBuffer = volumePacket.GetBuffer();
var powerBuffer = powerPacket.GetBuffer();

// Use PacketBuilder to parse received data
byte[] receivedData = GetDataFromRNetDevice(); // Your method to get data
var parsedPacket = PacketBuilder.Build(receivedData);

if (parsedPacket is ZoneInfoPacket zoneInfo)
{
    Console.WriteLine($"Received zone info for zone {zoneInfo.GetZoneID()}:");
    Console.WriteLine($"  Power: {zoneInfo.GetPower()}");
    Console.WriteLine($"  Volume: {zoneInfo.GetVolume()}%");
    Console.WriteLine($"  Source: {zoneInfo.GetSourceID()}");
}
```

### Request/Response Pattern

```csharp
// Request specific data
var request = RequestDataPacket.CreateZoneInfoRequest(0, 1);
var requestBuffer = request.GetBuffer();

// Send request through your serial connection
SendToSerialPort(requestBuffer);

// Response will be received and handled automatically by the service
```

## Error Handling and Logging

```csharp
// Configure logging for detailed debugging
services.AddLogging(builder => 
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug); // Enable debug logging
});

// Handle errors gracefully
rnetService.Error += (sender, exception) =>
{
    Console.WriteLine($"RNet Error: {exception.Message}");
    
    // Implement retry logic or error recovery
    if (exception.Message.Contains("serial"))
    {
        // Try to reconnect after serial errors
        Task.Run(async () =>
        {
            await Task.Delay(5000); // Wait 5 seconds
            await rnetService.ConnectAsync();
        });
    }
};
```

## Performance Optimization

### Batch Operations

```csharp
// When making multiple changes, batch them to avoid flooding the RNet bus
var zones = rnetService.GetAllZones().ToList();

foreach (var zone in zones)
{
    rnetService.SetZonePower(zone.ControllerID, zone.ZoneID, true);
    await Task.Delay(100); // Small delay between commands
}
```

### Efficient Zone Monitoring

```csharp
// Request zone info for all zones periodically
var timer = new Timer(async _ =>
{
    foreach (var zone in rnetService.GetAllZones())
    {
        rnetService.RequestZoneInfo(zone.ControllerID, zone.ZoneID);
        await Task.Delay(50); // Stagger requests
    }
}, null, TimeSpan.Zero, TimeSpan.FromSeconds(30)); // Every 30 seconds
```

## Integration Examples

### Home Automation Integration

```csharp
// Example: Integration with motion sensor
public class MotionSensorIntegration
{
    private readonly EnhancedRNetService _rnetService;
    
    public MotionSensorIntegration(EnhancedRNetService rnetService)
    {
        _rnetService = rnetService;
    }
    
    public void OnMotionDetected(string roomName)
    {
        var zone = _rnetService.GetAllZones()
            .FirstOrDefault(z => z.Name?.Equals(roomName, StringComparison.OrdinalIgnoreCase) == true);
            
        if (zone != null)
        {
            _rnetService.SetZonePower(zone.ControllerID, zone.ZoneID, true);
            _rnetService.SetZoneVolume(zone.ControllerID, zone.ZoneID, 25); // Welcome volume
            _rnetService.SendDisplayMessage(zone.ControllerID, zone.ZoneID, "Welcome!", 3);
        }
    }
}
```

### Music Synchronization

```csharp
// Example: Synchronize music across multiple zones
public void PlayMusicInMultipleZones(int sourceID, params string[] zoneNames)
{
    var targetZones = rnetService.GetAllZones()
        .Where(z => zoneNames.Contains(z.Name))
        .ToList();
        
    foreach (var zone in targetZones)
    {
        rnetService.SetZonePower(zone.ControllerID, zone.ZoneID, true);
        rnetService.SetZoneSource(zone.ControllerID, zone.ZoneID, sourceID);
        rnetService.SetZoneVolume(zone.ControllerID, zone.ZoneID, 40);
    }
}
```

## Cleanup and Disposal

```csharp
// Always dispose of the service when done
using var rnetService = serviceProvider.GetRequiredService<EnhancedRNetService>();

// Your code here...

// Service will be automatically disposed and disconnected
```

## Testing and Simulation

```csharp
// For testing without hardware, use simulation mode
var config = new Configuration { Simulate = true };
services.AddSingleton<IConfigurationService>(new ConfigurationService(config));

// The service will operate in simulation mode without requiring actual hardware
```