# RNET-Pi Component Analysis

## Core Component Breakdown

This document provides a detailed analysis of each major component in the RNET-Pi system, including their responsibilities, interfaces, dependencies, and implementation details.

## 1. Application Orchestrator (`app.js`)

### Purpose
Central coordinator that wires together all system components and manages the application lifecycle.

### Key Responsibilities
- **System Initialization**: Creates and configures all major subsystems
- **Event Routing**: Coordinates communication between components
- **State Synchronization**: Ensures all clients receive consistent state updates
- **Lifecycle Management**: Handles startup, shutdown, and error conditions

### Component Dependencies
```javascript
// Core infrastructure
const Server = require("./server/Server")
const RNet = require("./rnet/rnet");
const config = require("./configuration");

// Smart integrations
const GoogleCastIntegration = require("./smart-integration/GoogleCastIntegration");
const WebHookServer = require("./webHookServer");
const Updater = require("./updater");

// 40+ packet classes for client-server communication
```

### Key Interfaces

#### Event Handling Pattern
```javascript
// Server events
server.on("client_connected", (client) => {
    // Send complete state sync to new client
    // Activate auto-updates if first client
});

server.on("packet", (client, packet) => {
    // Route packet to appropriate handler based on packet.getID()
});

// RNet events  
rNet.on("volume", (zone, volume) => {
    // Broadcast volume change to all clients
    server.broadcast(new PacketS2CZoneVolume(zone.getControllerID(), zone.getZoneID(), volume));
});
```

#### Packet Routing Logic
```javascript
switch (packet.getID()) {
    case PacketC2SZoneVolume.ID:
        const zone = rNet.getZone(packet.getControllerID(), packet.getZoneID());
        if (zone != null)
            zone.setVolume(packet.getVolume());
        break;
    // Handle 14 other packet types...
}
```

### Critical Features
- **Complete State Sync**: New clients receive full system state
- **Graceful Error Handling**: Logs warnings for unknown zones/sources
- **Auto-Update Management**: Optimizes serial traffic based on client count
- **Broadcast Efficiency**: Single event triggers updates to all clients

---

## 2. RNet Communication Layer

### 2.1 Core RNet Class (`rnet/rnet.js`)

#### Purpose
Primary interface to Russound hardware via RS-232 serial communication.

#### Key Responsibilities
- **Serial Protocol Management**: Handle low-level serial communication
- **Packet Parsing**: Convert binary data to/from structured packets
- **Device State Management**: Maintain authoritative state for zones and sources
- **Configuration Persistence**: Save/restore zone and source configurations

#### Serial Communication Implementation
```javascript
class RNet extends EventEmitter {
    connect() {
        this._serialPort = new SerialPort(this._device, {
            baudRate: 19200,
        })
        .on("open", () => {
            this._connected = true;
            this.emit("connected");
            this.requestAllZoneInfo(true);
        })
        .on("data", (data) => {this._handleData(data)});
    }
    
    _handleData(data) {
        // Packet boundary detection and validation
        // Checksum verification
        // Packet type identification and routing
    }
}
```

#### State Management Architecture
```javascript
// Internal data structures
this._zones = [];      // 3D array: [controller][zone]
this._sources = [];    // 1D array: [sourceID]
this._autoUpdating = false;
this._connected = false;
this._packetQueue = [];
```

#### Configuration Persistence
- **Sources**: `sources.json` - name, type, Cast integration settings
- **Zones**: `zones.json` - name, controller/zone mapping, custom parameters
- **Auto-recovery**: Restores state on application restart

### 2.2 Zone Management (`rnet/zone.js`)

#### Purpose
Represents individual audio zones with full state management and control capabilities.

#### Core State Properties
```javascript
class Zone extends EventEmitter {
    constructor(rnet, ctrllrID, zoneID) {
        // Identity
        this._ctrllrID = ctrllrID;
        this._zoneID = zoneID;
        this._name = null;
        
        // Audio state
        this._power = false;
        this._volume = 0;
        this._source = 0;
        this._mute = false;
        this._maxVolume = 100;
        
        // Audio parameters (9 parameters per zone)
        this._parameters = [
            0,      // Bass             -10 - +10
            0,      // Treble           -10 - +10
            false,  // Loudness
            0,      // Balance          -10 - +10
            0,      // Turn on Volume   0 - 100
            0,      // Background Color 0 - 2
            false,  // Do Not Disturb
            0,      // Party Mode       0 - 2
            false,  // Front AV Enable
        ];
    }
}
```

#### Advanced Features

##### Volume Animation System
```javascript
setVolume(volume, fadeTime = 0) {
    if (fadeTime > 0) {
        // Smooth volume transitions using amator library
        this._volumeAnimation = animate(this._volume, volume, {
            duration: fadeTime,
            step: (value) => {
                this._volume = Math.round(value);
                this._sendVolumeCommand();
            }
        });
    } else {
        this._volume = volume;
        this._sendVolumeCommand();
    }
}
```

##### Parameter Validation
```javascript
setParameter(parameterID, value) {
    // Validate parameter ID (0-8)
    if (parameterID < 0 || parameterID >= this._parameters.length) {
        console.warn("Invalid parameter ID: %d", parameterID);
        return;
    }
    
    // Type-specific validation
    switch (parameterID) {
        case 0: case 1: case 3: // Bass, Treble, Balance
            value = Math.max(-10, Math.min(10, value));
            break;
        case 4: // Turn on Volume
            value = Math.max(0, Math.min(100, value));
            break;
        // Additional validation logic...
    }
}
```

#### Event-Driven Architecture
```javascript
// Zone emits events for all state changes
setPower(powered) {
    if (this._power !== powered) {
        this._power = powered;
        this.emit("power", powered);
        this._sendPowerCommand();
    }
}
```

### 2.3 Source Management (`rnet/source.js`)

#### Purpose
Manages audio sources with smart device integration capabilities.

#### Source Types
```javascript
// Predefined source types
static TYPE_GENERIC = 0;
static TYPE_GOOGLE_CAST = 1;
static TYPE_SONOS = 2; // Future implementation
```

#### Smart Integration Features
```javascript
class Source extends EventEmitter {
    constructor(rNet, sourceID, name, type) {
        // Core properties
        this._sourceID = sourceID;
        this._name = name;
        this._type = type;
        
        // Media metadata
        this._mediaTitle = "";
        this._mediaArtist = "";
        this._mediaArtworkURL = "";
        this._mediaPlayState = false;
        
        // Smart features
        this._autoOnZones = [];    // Zones to auto-activate
        this._autoOff = false;     // Auto-deactivate zones
        this._overrideName = "";   // Display name override
        this._descriptiveText = "";
    }
}
```

#### Media Control Interface
```javascript
control(operation) {
    switch (operation) {
        case Source.CONTROL_PLAY:
        case Source.CONTROL_PAUSE:
        case Source.CONTROL_STOP:
        case Source.CONTROL_NEXT:
        case Source.CONTROL_PREVIOUS:
            // Forward to smart integration layer
            this.emit("control", operation);
            break;
    }
}
```

### 2.4 Packet System (`rnet/packets/`)

#### Architecture Overview
Binary protocol implementation with structured packet types for different operations.

#### Base Packet Classes
```javascript
// Base packet with common functionality
class RNetPacket {
    constructor() {
        this._data = new SmartBuffer();
        this._checksum = 0;
    }
    
    // Checksum calculation for data integrity
    calculateChecksum() {
        let checksum = 0;
        for (let byte of this._data.toBuffer()) {
            checksum ^= byte;
        }
        return checksum;
    }
}

// Specialized packet types
class DataPacket extends RNetPacket { /* ... */ }
class EventPacket extends RNetPacket { /* ... */ }
class HandshakePacket extends RNetPacket { /* ... */ }
```

#### Packet Processing Pipeline
```javascript
// Packet creation and transmission
sendPacket(packet) {
    const buffer = packet.build();
    this._serialPort.write(buffer);
}

// Packet parsing and validation
_parsePacket(data) {
    const packet = PacketBuilder.fromBuffer(data);
    if (packet.validateChecksum()) {
        this._routePacket(packet);
    }
}
```

---

## 3. Server Infrastructure

### 3.1 Multi-Protocol Server (`server/Server.js`)

#### Purpose
Unified server interface supporting multiple client protocols simultaneously.

#### Protocol Support
```javascript
class Server extends EventEmitter {
    constructor(config) {
        // TCP server for native mobile apps
        this._tcpServer = new TCPServer(config.name, config.host, config.port);
        
        // WebSocket server for web clients
        if (config.webPort) {
            this._wsServer = new WSServer(config.webHost, config.webPort);
        }
        
        // Unified event handling for both protocols
        this._setupEventForwarding();
    }
}
```

#### Event Unification
```javascript
_setupEventForwarding() {
    [this._tcpServer, this._wsServer].forEach(server => {
        if (server) {
            server.on("client_connected", (client) => {
                this.emit("client_connected", client);
            });
            // Forward all events through unified interface
        }
    });
}
```

### 3.2 Client Abstraction Layer

#### Protocol-Agnostic Client Interface
```javascript
// Base client class
class Client extends EventEmitter {
    constructor() {
        this._connected = false;
        this._address = "";
    }
    
    // Abstract methods implemented by protocol-specific clients
    send(packet) { throw new Error("Must implement send()"); }
    disconnect() { throw new Error("Must implement disconnect()"); }
    getAddress() { return this._address; }
}

// Protocol-specific implementations
class TCPClient extends Client {
    send(packet) {
        if (this._socket && this._connected) {
            const buffer = packet.build();
            this._socket.write(buffer);
        }
    }
}

class WSClient extends Client {
    send(packet) {
        if (this._connection && this._connected) {
            const data = packet.serialize();
            this._connection.sendUTF(data);
        }
    }
}
```

### 3.3 Packet Processing System (`server/packets/`)

#### Bidirectional Packet Architecture

##### Client-to-Server Packets (C2S)
```javascript
// Base class for client commands
class PacketC2S {
    static ID = 0; // Unique packet identifier
    
    constructor() {
        this._data = new SmartBuffer();
    }
    
    getID() { return this.constructor.ID; }
    
    // Abstract methods for packet parsing
    parse(data) { throw new Error("Must implement parse()"); }
}

// Example: Zone volume control
class PacketC2SZoneVolume extends PacketC2S {
    static ID = 0x08;
    
    parse(data) {
        this._controllerID = data.readUInt8();
        this._zoneID = data.readUInt8();
        this._volume = data.readUInt8();
    }
    
    getControllerID() { return this._controllerID; }
    getZoneID() { return this._zoneID; }
    getVolume() { return this._volume; }
}
```

##### Server-to-Client Packets (S2C)
```javascript
// Base class for server notifications
class PacketS2C {
    static ID = 0; // Unique packet identifier
    
    constructor() {
        this._data = new SmartBuffer();
    }
    
    // Abstract methods for packet building
    build() { throw new Error("Must implement build()"); }
    serialize() { throw new Error("Must implement serialize()"); }
}

// Example: Zone volume notification
class PacketS2CZoneVolume extends PacketS2C {
    static ID = 0x08;
    
    constructor(controllerID, zoneID, volume) {
        super();
        this._controllerID = controllerID;
        this._zoneID = zoneID;
        this._volume = volume;
    }
    
    build() {
        this._data.writeUInt8(this.constructor.ID);
        this._data.writeUInt8(this._controllerID);
        this._data.writeUInt8(this._zoneID);
        this._data.writeUInt8(this._volume);
        return this._data.toBuffer();
    }
}
```

#### Packet Factory Pattern
```javascript
// Dynamic packet creation based on ID
function createPacket(id, data) {
    const packetMap = {
        0x01: PacketC2SZonePower,
        0x02: PacketC2SZoneVolume,
        0x03: PacketC2SZoneSource,
        // ... 40+ packet types
    };
    
    const PacketClass = packetMap[id];
    if (PacketClass) {
        const packet = new PacketClass();
        packet.parse(data);
        return packet;
    }
    
    throw new Error(`Unknown packet ID: 0x${id.toString(16)}`);
}
```

---

## 4. Smart Integration Layer

### 4.1 Google Cast Integration (`smart-integration/GoogleCastIntegration.js`)

#### Purpose
Bridges Google Cast devices with Russound zones for seamless smart home integration.

#### Core Architecture
```javascript
class GoogleCastIntegration {
    constructor(rNet) {
        this._rNet = rNet;
        this._sources = new Map(); // Source name -> Cast device mapping
        this._castMonitor = new GoogleCastMonitor();
        
        // Initialize existing Cast sources
        let sources = rNet.getSourcesByType(Source.TYPE_GOOGLE_CAST);
        for (let source of sources) {
            this.integrateSource(source);
        }
    }
}
```

#### Device Discovery and Management
```javascript
class GoogleCastMonitor extends EventEmitter {
    constructor() {
        this._devices = new Map();
        this._bonjour = bonjour();
        
        // Discover Cast devices via mDNS
        this._bonjour.find({type: 'googlecast'}, (service) => {
            this.handleDeviceFound(service);
        });
    }
    
    handleDeviceFound(service) {
        const device = {
            name: service.name,
            host: service.referer?.address || service.addresses?.[0],
            port: service.port,
            txt: service.txt
        };
        
        this._devices.set(service.name, device);
        this.emit("device-found", device);
    }
}
```

#### Media Session Monitoring
```javascript
integrateSource(source) {
    const deviceName = source.getName();
    const device = this._castMonitor.getDevice(deviceName);
    
    if (device) {
        this.connectToDevice(device, source);
    }
}

connectToDevice(device, source) {
    const client = new CastClient();
    
    client.connect(device.host, () => {
        client.launch('CC1AD845', (err, player) => {
            if (!err) {
                this.monitorPlayer(player, source);
            }
        });
    });
}

monitorPlayer(player, source) {
    player.on('status', (status) => {
        if (status.media) {
            // Update source with media metadata
            source.setMediaTitle(status.media.metadata.title);
            source.setMediaArtist(status.media.metadata.artist);
            source.setMediaArtworkURL(status.media.metadata.artwork);
            
            // Handle auto-on zones
            if (status.playerState === 'PLAYING' && source.getAutoOnZones().length > 0) {
                this.activateAutoOnZones(source);
            }
        }
    });
}
```

#### Zone Auto-Activation
```javascript
activateAutoOnZones(source) {
    const autoOnZones = source.getAutoOnZones();
    
    for (let zoneRef of autoOnZones) {
        const zone = this._rNet.getZone(zoneRef.controller, zoneRef.zone);
        if (zone) {
            zone.setPower(true);
            zone.setSourceID(source.getSourceID());
        }
    }
}
```

---

## 5. Supporting Services

### 5.1 Configuration Management (`configuration.js`)

#### Purpose
Centralized configuration management with file-based persistence.

#### Implementation
```javascript
module.exports = {
    read: function() {
        try {
            const contents = fs.readFileSync("config.json");
            data = JSON.parse(contents);
        } catch (e) {
            // Apply default configuration
            data = {
                serverName: "Untitled RNet Controller",
                serverHost: false,
                serverPort: 3000,
                serialDevice: "/dev/tty-usbserial1",
                webHookPassword: ""
            };
        }
    },
    
    write: function() {
        fs.writeFileSync("config.json", JSON.stringify(data));
    },
    
    get: function(key) { return data[key]; },
    set: function(key, value) { data[key] = value; }
};
```

### 5.2 Web Hook Server (`webHookServer.js`)

#### Purpose
HTTP API for external automation systems (IFTTT, custom scripts, etc.).

#### Security and Middleware
```javascript
class WebHookServer extends EventEmitter {
    constructor(port, password, rNet) {
        this._app = express();
        
        // Password authentication middleware
        this._app.use((req, res, next) => {
            if (req.query.pass != password) {
                res.sendStatus(401);
                console.warn("[Web Hook] Bad password in request.");
            } else {
                next();
            }
        });
        
        // RNet connection verification
        this._app.use((req, res, next) => {
            if (!rNet.isConnected()) {
                res.type("txt").status(503).send("RNet not connected.");
            } else {
                next();
            }
        });
    }
}
```

#### RESTful Endpoints
```javascript
// System-wide controls
this._app.put("/on", (req, res) => {
    rNet.setAllPower(true);
    res.sendStatus(200);
});

this._app.put("/off", (req, res) => {
    rNet.setAllPower(false);
    res.sendStatus(200);
});

// Zone-specific controls with parameter parsing
this._app.put("/volume/:controller/:zone/:volume", (req, res) => {
    const zone = rNet.getZone(
        parseInt(req.params.controller),
        parseInt(req.params.zone)
    );
    
    if (zone) {
        zone.setVolume(parseInt(req.params.volume));
        res.sendStatus(200);
    } else {
        res.sendStatus(404);
    }
});
```

### 5.3 Update Management (`updater.js`)

#### Purpose
Automatic software update checking and notification system.

#### Version Checking
```javascript
class Updater {
    static currentVersion = "1.1.1";
    
    static checkForUpdates(callback) {
        // Check remote repository for newer versions
        // Compare semantic versions
        // Notify callback of available updates
    }
    
    static update() {
        // Coordinate git pull and npm install
        // Handle service restart
    }
}
```

---

## Component Interaction Patterns

### 1. Event-Driven Communication
All major components use EventEmitter for loose coupling:
- RNet emits hardware state changes
- Server broadcasts to all clients
- Smart integrations monitor external devices
- Components can subscribe to relevant events without tight coupling

### 2. State Synchronization
```
Hardware State Change → RNet Event → Server Broadcast → All Clients Updated
```

### 3. Command Routing
```
Client Command → Server → Packet Router → RNet → Hardware Command
```

### 4. Configuration Management
```
Runtime Change → Config Update → File Persistence → Startup Recovery
```

### 5. Error Propagation
```
Low-Level Error → Component Event → Application Handler → User Notification
```

## Performance Characteristics

### Memory Usage
- **In-memory state**: All zone/source data cached for performance
- **Minimal persistence**: Only configuration data written to disk
- **Event cleanup**: Proper listener management prevents memory leaks

### Network Efficiency
- **State-based updates**: Only changes are broadcast
- **Connection management**: Auto-update optimization based on client count
- **Protocol efficiency**: Binary protocols for mobile, JSON for web

### Serial Communication
- **Optimized polling**: Requests only when clients are connected
- **Queue management**: Prevents command flooding
- **Error recovery**: Graceful handling of communication failures

This component analysis provides the foundation for understanding how to replicate each piece of functionality in a C# .NET environment while maintaining the same architectural principles and interaction patterns.