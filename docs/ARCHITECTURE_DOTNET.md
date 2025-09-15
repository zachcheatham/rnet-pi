# RNET-Pi C# .NET Architecture Documentation

## Project Overview

RNET-Pi .NET is a modern C# port of the original Node.js RNET-Pi application, providing a robust proxy server that enables smart home integration with legacy Russound whole-home audio systems. The application leverages .NET 8.0 and follows clean architecture principles for maintainability, testability, and performance.

## High-Level Architecture

### Technology Stack
- **.NET 8.0**: Latest LTS version for optimal performance and modern language features
- **Clean Architecture**: Separation of concerns with distinct layers
- **Dependency Injection**: Built-in Microsoft.Extensions.DependencyInjection
- **Structured Logging**: Microsoft.Extensions.Logging with configurable providers
- **System.IO.Ports**: Native .NET serial communication
- **System.Text.Json**: High-performance JSON serialization

### Project Structure

```
src/
├── RNetPi.Core/                    # Domain models and interfaces
│   ├── Models/                     # Domain entities (Zone, Source, Configuration)
│   ├── Packets/                    # Communication packet definitions
│   └── Interfaces/                 # Service contracts
├── RNetPi.Infrastructure/          # External dependencies and hardware communication
│   └── Services/                   # Concrete implementations (RNetService, ConfigurationService)
├── RNetPi.Application/             # Business logic and orchestration
├── RNetPi.API/                     # Web API and real-time communication endpoints
└── RNetPi.Console/                 # Console application host
```

## Core Architecture Layers

### 1. Core Layer (`RNetPi.Core`)

The innermost layer containing domain models, interfaces, and business rules with no external dependencies.

#### Domain Models

**Zone Model**
```csharp
public class Zone
{
    public int ControllerID { get; }
    public int ZoneID { get; }
    public string? Name { get; private set; }
    public bool Power { get; private set; }
    public int Volume { get; private set; }
    public int Source { get; private set; }
    public bool Mute { get; private set; }
    public int MaxVolume { get; private set; } = 100;
    
    // Event-driven architecture
    public event Action<string>? NameChanged;
    public event Action<bool>? PowerChanged;
    public event Action<int>? VolumeChanged;
    // ... additional events
}
```

**Source Model**
```csharp
public class Source
{
    public int SourceID { get; }
    public string Name { get; private set; }
    public SourceType Type { get; private set; }
    
    // Smart integration features
    public List<ZoneReference> AutoOnZones { get; }
    public bool AutoOff { get; set; }
    
    // Media metadata
    public string? MediaTitle { get; private set; }
    public string? MediaArtist { get; private set; }
    public bool MediaPlaying { get; private set; }
}
```

#### Service Interfaces

**IRNetService Interface**
```csharp
public interface IRNetService
{
    bool IsConnected { get; }
    
    Task<bool> ConnectAsync();
    Task DisconnectAsync();
    
    Zone? GetZone(int controllerID, int zoneID);
    Source? GetSource(int sourceID);
    
    IEnumerable<Zone> GetAllZones();
    IEnumerable<Source> GetAllSources();
    
    Zone CreateZone(int controllerID, int zoneID, string name);
    Source CreateSource(int sourceID, string name, SourceType type);
    
    void SetAllPower(bool power);
    void SetAllMute(bool mute, int fadeTime = 0);
    
    event EventHandler? Connected;
    event EventHandler? Disconnected;
    event EventHandler<Exception>? Error;
}
```

### 2. Infrastructure Layer (`RNetPi.Infrastructure`)

Handles external concerns like serial communication, file persistence, and hardware integration.

#### RNet Communication Service

```csharp
public class RNetService : IRNetService, IDisposable
{
    private readonly ILogger<RNetService> _logger;
    private readonly IConfigurationService _configService;
    private readonly ConcurrentDictionary<(int, int), Zone> _zones;
    private readonly ConcurrentDictionary<int, Source> _sources;
    private SerialPort? _serialPort;
    
    public async Task<bool> ConnectAsync()
    {
        // Serial port configuration: 19200 baud, 8N1
        _serialPort = new SerialPort(devicePath, 19200, Parity.None, 8, StopBits.One);
        _serialPort.DataReceived += OnDataReceived;
        _serialPort.ErrorReceived += OnErrorReceived;
        _serialPort.Open();
        
        // Request initial zone information
        await RequestAllZoneInfoAsync();
        return true;
    }
}
```

#### Configuration Management

```csharp
public class ConfigurationService : IConfigurationService
{
    private Configuration _configuration;
    private readonly string _configFilePath;
    
    public async Task LoadAsync()
    {
        if (File.Exists(_configFilePath))
        {
            var json = await File.ReadAllTextAsync(_configFilePath);
            _configuration = JsonSerializer.Deserialize<Configuration>(json);
        }
        else
        {
            // Create default configuration
            _configuration = new Configuration();
            await SaveAsync();
        }
    }
}
```

### 3. Application Layer (`RNetPi.Application`)

Contains business logic, command handling, and orchestration services.

### 4. API Layer (`RNetPi.API`)

Provides REST API endpoints and real-time communication via SignalR.

### 5. Console Host (`RNetPi.Console`)

Demonstration application showing core functionality and service integration.

## Event-Driven Architecture

The C# implementation maintains the event-driven nature of the original Node.js version while leveraging .NET's strong typing and performance characteristics.

### Zone Events
```csharp
zone.PowerChanged += (power) => {
    // Broadcast power change to connected clients
    // Log state change
    // Persist configuration if needed
};

zone.VolumeChanged += (volume) => {
    // Update UI clients
    // Apply volume limits
    // Handle fade animations
};
```

### Service Events
```csharp
rnetService.Connected += (sender, args) => {
    // Start client broadcasts
    // Request initial state
    // Enable auto-updates
};

rnetService.Error += (sender, exception) => {
    // Log error details
    // Attempt reconnection
    // Notify administrators
};
```

## Configuration and Persistence

### JSON-Based Configuration
```json
{
  "ServerName": "Living Room Audio Controller",
  "ServerHost": null,
  "ServerPort": 3000,
  "WebHost": null,
  "WebPort": 8080,
  "SerialDevice": "/dev/ttyUSB0",
  "WebHookPassword": "secure_password",
  "Simulate": false
}
```

### Zone and Source Persistence
- **zones.json**: Zone configurations, names, and custom parameters
- **sources.json**: Source definitions and smart device associations
- **Automatic persistence**: Changes are automatically saved to disk

## Packet System

The packet system maintains compatibility with the original RNet protocol while providing type-safe implementations.

### Base Packet Classes
```csharp
public abstract class PacketC2S : IDisposable
{
    protected BinaryReader Reader { get; }
    
    public abstract byte GetID();
    protected abstract void ParseData();
}

public abstract class PacketS2C : IDisposable
{
    protected BinaryWriter Writer { get; }
    
    public abstract byte GetID();
    public virtual byte[] GetBuffer();
}
```

### Example Packet Implementation
```csharp
public class PacketC2SZoneVolume : PacketC2S
{
    public const byte ID = 0x09;
    
    public byte ControllerID { get; private set; }
    public byte ZoneID { get; private set; }
    public byte Volume { get; private set; }
    
    protected override void ParseData()
    {
        ControllerID = Reader.ReadByte();
        ZoneID = Reader.ReadByte();
        Volume = Reader.ReadByte();
    }
}
```

## Performance and Scalability

### Concurrency
- **Thread-safe collections**: ConcurrentDictionary for zone and source management
- **Async/await patterns**: Non-blocking I/O operations throughout
- **Event-driven updates**: Minimal CPU usage during idle periods

### Memory Management
- **IDisposable pattern**: Proper resource cleanup for serial ports and file handles
- **Structured logging**: Efficient log message formatting with string interpolation
- **Lazy loading**: Configuration and persistence loaded on-demand

### Error Handling
- **Graceful degradation**: Simulation mode when hardware unavailable
- **Comprehensive logging**: Detailed error information with context
- **Automatic recovery**: Reconnection logic for serial communication failures

## Advantages Over Node.js Version

### Type Safety
- **Compile-time checking**: Eliminates entire classes of runtime errors
- **IntelliSense support**: Better development experience and maintainability
- **Null reference analysis**: Reduced null pointer exceptions

### Performance
- **Native compilation**: Better startup time and memory usage
- **Optimized JSON**: System.Text.Json provides superior performance
- **Async I/O**: Truly asynchronous operations without event loop blocking

### Ecosystem Integration
- **Windows Services**: Native support for running as Windows service
- **Docker containers**: Optimized .NET runtime containers
- **APM integration**: Native support for Application Performance Monitoring
- **Health checks**: Built-in health check endpoints for monitoring

### Development Experience
- **Debugging**: Superior debugging tools in Visual Studio/VS Code
- **Testing**: Rich testing framework ecosystem
- **Dependency injection**: Built-in container with lifetime management
- **Configuration**: Strongly-typed configuration with validation

This C# .NET architecture provides a solid foundation for scaling the RNET-Pi application while maintaining all the functionality of the original Node.js implementation with improved performance, type safety, and maintainability.