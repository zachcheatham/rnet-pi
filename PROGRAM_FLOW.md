# RNET-Pi Program Flow Documentation

## Application Startup Sequence

### 1. Bootstrap Phase (`app.js` lines 1-50)
```javascript
// Module imports and initialization
const config = require("./configuration");
require("console-stamp")(console, "HH:MM:ss");

// Configuration loading and validation
config.read();
config.write();
```

**Flow Details:**
1. Load all required modules and dependencies
2. Apply console timestamp formatting
3. Read existing configuration or create defaults
4. Write configuration back to ensure file consistency

### 2. Server Initialization (`app.js` lines 52-63)
```javascript
const server = new Server({
    name: config.get("serverName"),
    host: config.get("serverHost"),
    port: config.get("serverPort"),
    webHost: config.get("webHost"),
    webPort: config.get("webPort")
});

const rNet = new RNet(config.get("serialDevice"));
const webHookServer = new WebHookServer(config.get("serverPort")+1, config.get("webHookPassword"), rNet);
const googleCastIntegration = new GoogleCastIntegration(rNet);
```

**Component Creation Order:**
1. **Server**: Creates TCP and WebSocket servers
2. **RNet**: Initializes serial communication (not yet connected)
3. **WebHookServer**: Sets up HTTP endpoints for external automation
4. **GoogleCastIntegration**: Prepares Cast device monitoring

### 3. Event Handler Registration (`app.js` lines 65-456)

#### Server Event Handlers
- **client_connected**: Synchronizes new clients with current system state
- **client_disconnect**: Manages auto-update behavior based on client count
- **packet**: Routes incoming commands to appropriate handlers

#### RNet Event Handlers
- **connected/disconnected**: Manages serial connection state
- **new-zone/zone-deleted**: Handles zone lifecycle
- **new-source/source-deleted**: Manages source configuration
- **State change events**: Broadcasts updates to all clients

### 4. Service Startup (`app.js` lines 457-472)
```javascript
// Serial connection (unless in simulation mode)
if (!config.get("simulate")) {
    console.info("Connecting to RNet...");
    rNet.connect();
} else {
    console.info("Simulation mode. Will not attempt to open serial connection.")
}

// Network services
console.info("Starting Server...");
server.start();
webHookServer.start();
```

**Startup Order:**
1. Attempt RNet serial connection (if not simulating)
2. Start TCP and WebSocket servers
3. Start webhook HTTP server
4. Begin update checking process

## Client Connection Flow

### New Client Connection Sequence

```
Client Connection Request
         ↓
   Connection Accepted
         ↓
   Client State Sync
         ↓
   Auto-Update Activation
         ↓
   Real-time Updates Begin
```

#### Detailed Client Sync Process (`app.js` lines 69-133)

1. **Zone Index Transmission**
   ```javascript
   let zones = [];
   for (let ctrllrID = 0; ctrllrID < rNet.getControllersSize(); ctrllrID++) {
       for (let zoneID = 0; zoneID < rNet.getZonesSize(ctrllrID); zoneID++) {
           if (rNet.getZone(ctrllrID, zoneID) != null) {
               zones.push([ctrllrID, zoneID]);
           }
       }
   }
   client.send(new PacketS2CZoneIndex(zones));
   ```

2. **System Properties**
   - Server name
   - Current version
   - Serial connection status

3. **Source Information**
   - All configured sources
   - Media metadata (if available)
   - Descriptive text
   - Playback state

4. **Zone State Synchronization**
   - Zone names
   - Power states
   - Volume levels
   - Source assignments
   - All 9 parameters per zone
   - Maximum volume limits
   - Mute states

5. **Auto-Update Activation**
   ```javascript
   if (server.getClientCount() == 0) { // First client connected
       rNet.requestAllZoneInfo();
   }
   rNet.setAutoUpdate(true);
   ```

## Command Processing Flow

### Inbound Command Processing

```
Client Sends Packet
        ↓
   Packet Received
        ↓
   Packet Type Identified
        ↓
   Command Validation
        ↓
   RNet Command Execution
        ↓
   State Update Broadcast
```

#### Command Routing (`app.js` lines 142-312)

The system processes 15 different command types:

1. **PacketC2SAllPower**: System-wide power control
2. **PacketC2SDeleteZone/Source**: Configuration management
3. **PacketC2SMute**: Global and zone-specific muting
4. **PacketC2SProperty**: System property updates
5. **PacketC2SSourceControl**: Media transport controls
6. **PacketC2SSourceInfo**: Source configuration
7. **PacketC2SZone*** commands: Zone-specific operations

#### Example: Volume Change Flow
```javascript
case PacketC2SZoneVolume.ID:
{
    const zone = rNet.getZone(packet.getControllerID(), packet.getZoneID());
    if (zone != null)
        zone.setVolume(packet.getVolume());
    else
        console.warn("Received request to set volume of unknown zone %d-%d", 
                    packet.getControllerID(), packet.getZoneID());
    break;
}
```

### Outbound State Propagation

```
Hardware State Change
        ↓
   RNet Event Emission
        ↓
   Event Handler Processing
        ↓
   Packet Creation
        ↓
   Broadcast to All Clients
```

#### Example: Volume Change Propagation
```javascript
.on("volume", (zone, volume) => {
    server.broadcast(new PacketS2CZoneVolume(zone.getControllerID(), zone.getZoneID(), volume));
    console.info(
        "Controller #%d zone #%d (%s) volume set to %d",
        zone.getControllerID(),
        zone.getZoneID(),
        zone.getName(),
        volume
    );
})
```

## RNet Communication Flow

### Serial Communication Lifecycle

```
Application Start
        ↓
   Serial Port Open
        ↓
   Handshake Protocol
        ↓
   Zone Discovery
        ↓
   Continuous Monitoring
```

#### Connection Establishment (`rnet.js` lines 43-63)

```javascript
connect() {
    this._serialPort = new SerialPort(this._device, {
        baudRate: 19200,
    })
    .on("open", () => {
        this._connected = true;
        this.emit("connected");
        this.requestAllZoneInfo(true);
    })
    .on("close", () => {
        this._connected = false;
        this.emit("disconnected");
    })
    .on("error", (error) => {
        this.emit("error", error);
    })
    .on("data", (data) => {this._handleData(data)});
}
```

#### Data Processing Pipeline

```
Raw Serial Data
        ↓
   Packet Boundary Detection
        ↓
   Checksum Validation
        ↓
   Packet Type Identification
        ↓
   Data Extraction
        ↓
   State Update
        ↓
   Event Emission
```

### Zone and Source Management Flow

#### Zone Creation Process
```javascript
// From client request
case PacketC2SZoneName.ID:
{
    const zone = rNet.getZone(packet.getControllerID(), packet.getZoneID());
    if (zone)
        zone.setName(packet.getName());
    else
        rNet.createZone(packet.getControllerID(), packet.getZoneID(), packet.getName());
    break;
}
```

#### Source Integration Flow
```javascript
case PacketC2SSourceInfo.ID:
{
    let source = rNet.getSource(packet.getSourceID());
    if (source != null)
    {
        source.setName(packet.getName());
        source.setType(packet.getSourceTypeID());
    }
    else
        rNet.createSource(packet.getSourceID(), packet.getName(), packet.getSourceTypeID());
    break;
}
```

## Smart Integration Flow

### Google Cast Integration Lifecycle

```
Cast Integration Start
        ↓
   mDNS Device Discovery
        ↓
   Source Association
        ↓
   Session Monitoring
        ↓
   Media State Sync
```

#### Cast Device Discovery Process (`GoogleCastIntegration.js`)

1. **Service Discovery**
   ```javascript
   this._bonjour.find({type: 'googlecast'}, (service) => {
       this.handleDeviceFound(service);
   });
   ```

2. **Device Connection**
   ```javascript
   this._client.connect(device.host, () => {
       this._client.launch(MediaReceiver, (err, player) => {
           this.monitorPlayer(player, sourceName);
       });
   });
   ```

3. **Media State Monitoring**
   - Volume synchronization
   - Playback state tracking
   - Metadata extraction and display

#### Zone Auto-Activation Flow

```
Cast Media Starts
        ↓
   Source Detection
        ↓
   Auto-On Zone Check
        ↓
   Zone Power/Source Set
        ↓
   Metadata Display
```

## Web Hook Processing Flow

### HTTP Request Processing

```
External HTTP Request
        ↓
   Authentication Check
        ↓
   RNet Connection Verify
        ↓
   Command Execution
        ↓
   Response Generation
```

#### Request Authentication
```javascript
this._app.use(function(req, res, next) {
    if (req.query.pass != password) {
        res.sendStatus(401);
        console.warn("[Web Hook] Bad password in request.");
    }
    else {
        next();
    }
});
```

#### Command Mapping
- `PUT /on` → `rNet.setAllPower(true)`
- `PUT /off` → `rNet.setAllPower(false)`
- `PUT /mute` → `rNet.setAllMute(true, 1000)`
- `PUT /volume/:level` → Zone volume setting

## Configuration Management Flow

### Configuration Lifecycle

```
Application Start
        ↓
   Read config.json
        ↓
   Apply Defaults
        ↓
   Write Back Config
        ↓
   Runtime Updates
        ↓
   Persist Changes
```

#### Dynamic Configuration Updates
```javascript
case PacketC2SProperty.ID:
{
    switch (packet.getProperty()) {
        case Property.PROPERTY_NAME:
            server.setName(packet.getValue());
            server.broadcast(new PacketS2CProperty(Property.PROPERTY_NAME, packet.getValue()));
            config.set("serverName", packet.getValue());
            config.write();
            break;
    }
    break;
}
```

### Persistent State Management

#### Zone/Source Persistence
- **zones.json**: Zone configurations and custom settings
- **sources.json**: Source definitions and smart device associations

#### State Recovery on Restart
```javascript
readConfiguration() {
    // Load sources from sources.json
    if (sourceFile && sourceFile.length > 0) {
        let sources = JSON.parse(sourceFile);
        for (let sourceID = 0; sourceID < sources.length; sourceID++) {
            if (sources[sourceID] != null) {
                let sourceData = sources[sourceID];
                let source = this.createSource(sourceID, sourceData.name, sourceData.type, false);
                // Restore additional properties
            }
        }
    }
    
    // Load zones from zones.json
    // Similar process for zone restoration
}
```

## Error Handling and Recovery Flow

### Connection Error Handling

```
Serial Error Detected
        ↓
   Error Event Emission
        ↓
   Client Notification
        ↓
   Application Termination
```

#### Critical Error Response
```javascript
.on("error", (error) => {
    console.error("RNet Error: %s", error.message);
    process.exit(2);
})
```

### Graceful Shutdown Flow

```
SIGINT Signal Received
        ↓
   Server Stop Initiated
        ↓
   Client Disconnections
        ↓
   Service Cleanup
        ↓
   Serial Disconnection
        ↓
   Process Exit
```

#### Shutdown Implementation
```javascript
process.on('SIGINT', function() {
    console.info("Shutting down...");
    server.stop(() => {
        webHookServer.stop();
        googleCastIntegration.stop();
        rNet.disconnect();
        console.info("Goodbye.");
        process.exit();
    });
});
```

## Performance and Timing Considerations

### Auto-Update Management
- **Activation**: When first client connects
- **Deactivation**: When last client disconnects
- **Purpose**: Reduce serial traffic when no clients are monitoring

### Animation and Transitions
- **Volume changes**: Smooth fade animations using amator library
- **Zone muting**: Configurable fade times
- **Performance**: Non-blocking animations prevent UI freeze

### Memory Management
- **In-memory state**: All zone/source state cached for performance
- **Persistence**: Periodic writes to JSON files
- **Cleanup**: Automatic cleanup on zone/source deletion

This program flow documentation provides a comprehensive understanding of how data moves through the system, from initial startup through real-time operation, enabling effective analysis for porting to other platforms.