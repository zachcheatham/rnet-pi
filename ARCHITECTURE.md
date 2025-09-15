# RNET-Pi Architecture Documentation

## Project Overview

RNET-Pi is a Node.js-based proxy server that enables modern smart home integration with legacy Russound whole-home audio systems. The application bridges the RS-232 serial "automation" port on older Russound systems (CAS44, CAA66, CAM6.6, CAV6.6) with contemporary smart devices and services.

## High-Level Architecture

### System Components

```
┌─────────────────────────────────────────────────────────────────┐
│                        RNET-Pi Server                          │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │   TCP Server    │  │   WebSocket     │  │   Web Hook      │  │
│  │   (Port 3000)   │  │   Server        │  │   Server        │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │   Packet        │  │   Client        │  │   Property      │  │
│  │   System        │  │   Management    │  │   Management    │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │   RNet Core     │  │   Zone/Source   │  │   Smart         │  │
│  │   Communication │  │   Management    │  │   Integration   │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │   Serial Port   │  │   Configuration │  │   Update        │  │
│  │   Interface     │  │   Management    │  │   Management    │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
            │                                              │
            │ RS-232                                       │ Network
            ▼                                              ▼
   ┌─────────────────┐                           ┌─────────────────┐
   │   Russound      │                           │   Mobile Apps   │
   │   Audio System  │                           │   Smart Devices │
   │   (CAV6.6, etc) │                           │   IFTTT, etc    │
   └─────────────────┘                           └─────────────────┘
```

## Core Components Analysis

### 1. Application Entry Point (`src/app.js`)
- **Purpose**: Main application orchestrator and event handler
- **Responsibilities**:
  - Initializes all subsystems (Server, RNet, GoogleCast, WebHook)
  - Configures packet routing and event handling
  - Manages client connections and broadcasts
  - Handles graceful shutdown procedures
- **Key Features**:
  - Extensive packet handling (40+ packet types)
  - Event-driven architecture with EventEmitter pattern
  - Real-time client synchronization
  - Auto-update checking mechanism

### 2. RNet Communication Layer (`src/rnet/`)

#### Core RNet Class (`rnet.js`)
- **Purpose**: Primary interface to Russound hardware via RS-232
- **Key Capabilities**:
  - Serial port communication at 19200 baud
  - Packet parsing and validation
  - Zone and source state management
  - Auto-discovery and configuration persistence
  - Handshake protocol implementation

#### Zone Management (`zone.js`)
- **Purpose**: Represents individual audio zones
- **State Management**:
  - Power, volume, mute, source selection
  - Audio parameters (bass, treble, balance, etc.)
  - Maximum volume limits
  - Fade animations for volume changes
- **Features**:
  - 9 configurable parameters per zone
  - Parameter validation and bounds checking
  - Event emission for state changes

#### Source Management (`source.js`)
- **Purpose**: Represents audio sources (inputs)
- **Capabilities**:
  - Source naming and type classification
  - Media metadata handling (title, artist, artwork)
  - Smart device integration (Google Cast)
  - Auto-on/auto-off zone management
  - Descriptive text display support

#### Packet System (`src/rnet/packets/`)
- **Architecture**: Command/response pattern with binary protocol
- **Packet Types**:
  - Data packets (zone info, parameters, volume)
  - Control packets (power, source selection)
  - Display packets (text rendering)
  - Event packets (keypad interactions)
- **Features**:
  - Checksumming for data integrity
  - Variable-length packet support
  - Automated packet building and parsing

### 3. Server Infrastructure (`src/server/`)

#### Multi-Protocol Server (`Server.js`)
- **Purpose**: Unified interface for multiple communication protocols
- **Protocols Supported**:
  - TCP for native mobile applications
  - WebSocket for web-based clients
- **Client Management**:
  - Connection tracking and state synchronization
  - Broadcast messaging to all clients
  - Per-client packet queuing

#### Client Abstraction (`Client.js`, `TCPClient.js`, `WSClient.js`)
- **Purpose**: Protocol-agnostic client representation
- **Features**:
  - Unified send/receive interface
  - Connection state management
  - Packet serialization/deserialization
  - Error handling and recovery

#### Packet Processing (`src/server/packets/`)
- **Architecture**: Bidirectional packet system
- **Client-to-Server (C2S) Packets**:
  - Control commands (power, volume, source)
  - Configuration updates (zone names, parameters)
  - System commands (updates, property changes)
- **Server-to-Client (S2C) Packets**:
  - State notifications (volume, power, source changes)
  - Media metadata updates
  - System status information

### 4. Smart Device Integration (`src/smart-integration/`)

#### Google Cast Integration (`GoogleCastIntegration.js`)
- **Purpose**: Bridges Google Cast devices with Russound zones
- **Capabilities**:
  - Chromecast discovery via mDNS/Bonjour
  - Media session monitoring
  - Automatic zone activation/deactivation
  - Metadata display on Russound keypads
- **Integration Features**:
  - Volume synchronization
  - Play/pause/stop control via keypads
  - Source switching automation

### 5. Configuration and Persistence

#### Configuration Management (`configuration.js`)
- **File-based storage**: `config.json`
- **Settings**:
  - Server identification and network configuration
  - Serial device path
  - Web hook authentication
  - Simulation mode flag

#### Zone/Source Persistence
- **Separate storage files**: `zones.json`, `sources.json`
- **Persistent data**:
  - Zone names and custom settings
  - Source configurations and smart device associations
  - Maximum volume limits
  - Auto-on/auto-off settings

### 6. Additional Services

#### Web Hook Server (`webHookServer.js`)
- **Purpose**: IFTTT and external automation integration
- **Endpoints**:
  - Power control (`/on`, `/off`)
  - Volume control (`/volume`, `/mute`, `/unmute`)
  - Zone-specific operations
- **Security**: Password-based authentication

#### Update Management (`updater.js`)
- **Purpose**: Automatic software update checking
- **Features**:
  - Version comparison
  - Update availability notifications
  - Graceful update process

## Communication Protocols

### RS-232 Serial Protocol
- **Physical Layer**: USB-to-Serial adapter
- **Parameters**: 19200 baud, 8N1
- **Protocol**: Proprietary Russound RNet binary format
- **Packet Structure**:
  ```
  [Start] [Length] [Target] [Source] [Data] [Checksum]
  ```

### Network Protocols
- **TCP**: Custom binary protocol for mobile applications
- **WebSocket**: For web-based clients and future browser integration
- **HTTP**: RESTful endpoints for webhook integration

## Data Flow Architecture

### Inbound Data Flow
```
Serial Port → RNet Parser → Packet Decoder → Event Emitter → 
Server Broadcast → Client Transmission → Mobile/Web Apps
```

### Outbound Command Flow
```
Mobile/Web Apps → Network Protocol → Server → Packet Router → 
RNet Command Builder → Serial Transmission → Russound Hardware
```

### State Synchronization
```
Hardware State Change → RNet Events → Server Broadcast → 
All Connected Clients Updated Simultaneously
```

## Event-Driven Architecture

The system uses Node.js EventEmitter extensively for loose coupling:

### RNet Events
- `connected`, `disconnected`, `error`
- `new-zone`, `zone-name`, `zone-deleted`
- `new-source`, `source-name`, `source-deleted`
- `power`, `volume`, `mute`, `source`, `parameter`
- `media-metadata`, `media-playing`, `descriptive-text`

### Server Events
- `client_connected`, `client_disconnect`
- `packet` (for all incoming client packets)
- `error`, `start`

### Integration Events
- Google Cast device discovery and session monitoring
- Media state changes
- Zone automation triggers

## Scalability and Performance

### Design Strengths
- **Event-driven**: Non-blocking I/O for high concurrency
- **Modular architecture**: Separation of concerns
- **Protocol abstraction**: Easy addition of new client types
- **Smart caching**: In-memory state management

### Current Limitations
- **Single-threaded**: Node.js event loop limitations
- **No clustering**: Single process architecture
- **Limited error recovery**: Serial connection failures require restart
- **Memory usage**: All state held in memory

## Security Considerations

### Current Security Features
- **Web hook authentication**: Password-based access
- **Network binding**: Configurable host/port restrictions
- **Input validation**: Packet parameter bounds checking

### Security Gaps
- **No encryption**: Plain text protocols
- **No user authentication**: Open access to TCP/WebSocket
- **No rate limiting**: Potential DoS vulnerability
- **No audit logging**: Limited security monitoring

## Technology Dependencies

### Core Dependencies
- **Node.js Runtime**: JavaScript execution environment
- **SerialPort**: Hardware serial communication
- **Express**: HTTP server framework
- **WebSocket**: Real-time communication
- **Smart-Buffer**: Binary data manipulation

### Smart Integration Dependencies
- **castv2-client**: Google Cast communication
- **bonjour-service**: mDNS device discovery
- **amator**: Animation library for smooth transitions

## File Structure Overview

```
src/
├── app.js                          # Main application entry point
├── configuration.js                # Configuration management
├── Property.js                     # System property definitions
├── updater.js                      # Update management
├── webHookServer.js               # IFTTT/webhook integration
├── rnet/                          # Russound protocol implementation
│   ├── rnet.js                    # Core RNet communication
│   ├── zone.js                    # Zone state management
│   ├── source.js                  # Source management
│   ├── SourceProperty.js          # Source property definitions
│   ├── extraZoneParam.js          # Additional zone parameters
│   ├── parameterIDToString.js     # Parameter name mapping
│   └── packets/                   # RNet packet definitions
│       ├── RNetPacket.js          # Base packet class
│       ├── DataPacket.js          # Data transfer packets
│       ├── HandshakePacket.js     # Connection establishment
│       └── [20+ packet types]     # Specific packet implementations
├── server/                        # Network server implementation
│   ├── Server.js                  # Multi-protocol server
│   ├── Client.js                  # Client abstraction
│   ├── TCPServer.js              # TCP server implementation
│   ├── TCPClient.js              # TCP client handling
│   ├── WSServer.js               # WebSocket server
│   ├── WSClient.js               # WebSocket client handling
│   └── packets/                   # Client-server packets
│       ├── createPacket.js        # Packet factory
│       ├── PacketC2S.js          # Client-to-server base
│       ├── PacketS2C.js          # Server-to-client base
│       └── [40+ packet types]     # Bidirectional packet types
└── smart-integration/             # Smart device integrations
    └── GoogleCastIntegration.js   # Chromecast integration
```

## Deployment Architecture

### Target Environment
- **Primary Platform**: Raspberry Pi (ARM Linux)
- **Supported Platforms**: Linux, Windows, macOS (with Node.js)
- **Hardware Requirements**:
  - USB-to-RS232 adapter
  - Network connectivity
  - Minimal compute resources (suitable for Pi Zero)

### Service Integration
- **Systemd service**: Auto-start on boot
- **Forever-service**: Process monitoring and restart
- **Bonjour/mDNS**: Network service discovery
- **Update mechanism**: Automated software updates

This architecture provides a robust, extensible foundation for bridging legacy audio systems with modern smart home ecosystems while maintaining real-time responsiveness and supporting multiple concurrent clients.