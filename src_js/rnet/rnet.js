const EventEmitter = require("events");
const fs = require("fs");
const SerialPort = require("serialport");
const SmartBuffer = require("smart-buffer").SmartBuffer;

const ExtraZoneParam = require("./extraZoneParam");
const HandshakePacket = require("./packets/HandshakePacket");
const KeypadEventPacket = require("./packets/KeypadEventPacket")
const PacketBuilder = require("./packets/PacketBuilder");
const RenderedDisplayMessagePacket = require("./packets/RenderedDisplayMessagePacket");
const RequestDataPacket = require("./packets/RequestDataPacket");
const SetAllPowerPacket = require("./packets/SetAllPowerPacket");
const SetParameterPacket = require("./packets/SetParameterPacket");
const SetPowerPacket = require("./packets/SetPowerPacket");
const SetSourcePacket = require("./packets/SetSourcePacket");
const SetVolumePacket = require("./packets/SetVolumePacket");
const SourceDescriptiveTextPacket = require("./packets/SourceDescriptiveTextPacket");
const ZoneInfoPacket = require("./packets/ZoneInfoPacket");
const ZoneParameterPacket = require("./packets/ZoneParameterPacket");
const ZonePowerPacket = require("./packets/ZonePowerPacket");
const ZoneSourcePacket = require("./packets/ZoneSourcePacket");
const ZoneVolumePacket = require("./packets/ZoneVolumePacket");
const Source = require("./source");
const Zone = require("./zone");

class RNet extends EventEmitter {
    constructor(device) {
        super();

        this._device = device
        this._zones = [];
        this._sources = [];
        this._autoUpdating = false;
        this._connected = false;
        this._waitingForHandshake = false;
        this._packetQueue = [];
        this._allMuted = false;

        this.readConfiguration();
        this.writeConfiguration();
    }

    connect() {
        // TODO Automatically continue to try to connect
        // TODO This autodetect usb serial
        this._serialPort = new SerialPort(this._device, {
            baudRate: 19200,
        })
        .on("open", () => {
            this._connected = true;
            this.emit("connected");
            this.requestAllZoneInfo(true);
        })
        .on("close", () => {
            // TODO Start auto-reconnect
            this._connected = false;
            this.emit("disconnected");
        })
        .on("error", (error) => {
            this.emit("error", error);
        })
        .on("data", (data) => {this._handleData(data)});
    }

    disconnect() {
        if (this._serialPort !== undefined)
            this._serialPort.close();
    }

    readConfiguration() {
        var sourceFile;
        try {
            sourceFile = fs.readFileSync("sources.json");
        }
        catch (e) {}

        if (sourceFile && sourceFile.length > 0) {
            let sources = JSON.parse(sourceFile);
            for (let sourceID = 0; sourceID < sources.length; sourceID++) {
                if (sources[sourceID] != null) {
                    let sourceData = sources[sourceID];
                    let source = this.createSource(sourceID, sourceData.name, ("cast" in sourceData) && Source.TYPE_GOOGLE_CAST || sourceData.type, false);
                    if ("auto_on_zones" in sourceData) {
                        source._autoOnZones = sourceData.auto_on_zones;
                    }
                    if ("auto_off" in sourceData) {
                        source._autoOff = sourceData.auto_off;
                    }
                    if ("override_name" in sourceData) {
                        source._overrideName = sourceData.override_name;
                    }
                }
            }
        }

        var zonesFile = "";
        try {
            zonesFile = fs.readFileSync("zones.json");
        }
        catch (e) {}

        if (zonesFile.length > 0) {
            var zones = JSON.parse(zonesFile);
            for (var ctrllrID = 0; ctrllrID < zones.length; ctrllrID++) {
                if (zones[ctrllrID] != null) {
                    for (var zoneID = 0; zoneID < zones[ctrllrID].length; zoneID++) {
                        if (zones[ctrllrID][zoneID] != null) {
                            let zoneData = zones[ctrllrID][zoneID];
                            let zone = this.createZone(ctrllrID, zoneID, zoneData.name, false);
                            if ("maxvol" in zoneData) {
                                zone.setMaxVolume(zoneData.maxvol, false)
                            }
                        }
                    }
                }
            }
        }
    }

    writeConfiguration() {
        this.writeSources();
        this.writeZones();
    }

    writeSources() {
        const sources = [];
        for (var sourceID = 0; sourceID < this._sources.length; sourceID++) {
            if (this._sources[sourceID] == null) {
                sources[sourceID] = null;
            }
            else {
                let source = this._sources[sourceID];
                sources[sourceID] = {
                    name: source.getName(),
                    type: source.getType()
                }

                if (source._autoOff) {
                    sources[sourceID].auto_off = true;
                }
                if (source._autoOnZones.length > 0) {
                    sources[sourceID].auto_on_zones = source._autoOnZones;
                }
                if (source._overrideName) {
                    sources[sourceID].override_name = true;
                }
            }
        }

        fs.writeFileSync("sources.json", JSON.stringify(sources));
    }

    writeZones() {
        const zones = [];
        for (var ctrllrID = 0; ctrllrID < this._zones.length; ctrllrID++) {
            if (this._zones[ctrllrID] == null) {
                zones[ctrllrID] = null;
            }
            else {
                zones[ctrllrID] = [];
                for (var zoneID = 0; zoneID < this._zones[ctrllrID].length; zoneID++) {
                    if (this._zones[ctrllrID][zoneID] == null) {
                        zones[ctrllrID][zoneID] = null;
                    }
                    else {
                        zones[ctrllrID][zoneID] = {
                            name: this._zones[ctrllrID][zoneID].getName(),
                        }

                        if (this._zones[ctrllrID][zoneID].getMaxVolume() < 100) {
                            zones[ctrllrID][zoneID].maxvol = this._zones[ctrllrID][zoneID].getMaxVolume();
                        }
                    }
                }
            }
        }

        fs.writeFileSync("zones.json", JSON.stringify(zones));
    }

    createZone(ctrllrID, zoneID, name, writeConfig=true) {
        if (this._zones[ctrllrID] == null || this._zones[ctrllrID][zoneID] == null)
        {
            const zone = new Zone(this, ctrllrID, zoneID);
            zone.setName(name);

            if (!this._zones[ctrllrID]) {
                this._zones[ctrllrID] = [];
            }
            this._zones[ctrllrID][zoneID] = zone;

            if (writeConfig) {
                this.writeConfiguration();
            }

            zone.on("name", (name) => {
                this.emit("zone-name", zone, name);
                this.writeConfiguration();
            })
            .on("power", (powered, rNetTriggered) => {
                if (!rNetTriggered) {
                    this.sendData(
                        new SetPowerPacket(
                            zone.getControllerID(),
                            zone.getZoneID(),
                            powered
                        )
                    );
                }

                if (powered) {
                    let source = this.getSource(zone.getSourceID());
                    if (source) {
                        if (source.getDescriptiveText() != null) {
                            if (!source.isDescriptionFromRNet()) {
                                this.sendData(new SourceDescriptiveTextPacket(source.getSourceID(), 0, source.getDescriptiveText()));
                            }
                        }
                        else if (source.getOverrideName()) {
                            this.sendData(new SourceDescriptiveTextPacket(source.getSourceID(), 0, source.getName()));
                        }
                    }
                }

                this.emit("power", zone, powered);
            })
            .on("volume", (volume, rNetTriggered) => {
                if (!rNetTriggered) {
                    this.sendData(
                        new SetVolumePacket(
                            zone.getControllerID(),
                            zone.getZoneID(),
                            volume
                        )
                    );
                }
                this.emit("volume", zone, volume);
            })
            .on("max-volume", (maxVolume) => {
                this.emit("max-volume", zone, maxVolume);
            })
            .on("mute", (muting) => {
                this.emit("mute", zone, muting);
            })
            .on("source", (sourceID, rNetTriggered) => {
                if (!rNetTriggered) {
                    this.sendData(
                        new SetSourcePacket(
                            zone.getControllerID(),
                            zone.getZoneID(),
                            sourceID
                        )
                    );
                }

                let source = this.getSource(sourceID);
                if (source) {
                    if (source.getDescriptiveText() != null) {
                        if (!source.isDescriptionFromRNet()) {
                            this.sendData(new SourceDescriptiveTextPacket(sourceID, 0, source.getDescriptiveText()));
                        }
                    }
                    else if (source.getOverrideName()) {
                        this.sendData(new SourceDescriptiveTextPacket(sourceID, 0, source.getName()));
                    }
                }

                this.emit("source", zone, sourceID);
            })
            .on("parameter", (parameterID, value, rNetTriggered) => {
                if (!rNetTriggered) {
                    this.sendData(
                        new SetParameterPacket(
                            zone.getControllerID(),
                            zone.getZoneID(),
                            parameterID,
                            value
                        )
                    );
                }
                this.emit("parameter", zone, parameterID, value);
            });

            this.emit("new-zone", zone);
            return zone;
        }
        return false;
    }

    deleteZone(ctrllrID, zoneID) {
        if (this._zones[ctrllrID] && this._zones[ctrllrID][zoneID])
        {
            this._zones[ctrllrID][zoneID].removeAllListeners();

            delete this._zones[ctrllrID][zoneID];
            if (this._zones[ctrllrID].length == 0) {
                delete this._zones[ctrllrID];
            }

            this.writeConfiguration();
            this.emit("zone-deleted", ctrllrID, zoneID);
            return true;
        }
        return false;
    }

    getZone(ctrllrID, zoneID) {
        if (!this._zones[ctrllrID]) {
            return null;
        }
        else {
            return this._zones[ctrllrID][zoneID];
        }
    }

    findZoneByName(name) {
        name = name.toUpperCase();
        for (let ctrllrID = 0; ctrllrID < this.getControllersSize(); ctrllrID++) {
            for (let zoneID = 0; zoneID < this.getZonesSize(ctrllrID); zoneID++) {
                let zone = this.getZone(ctrllrID, zoneID);
                if (zone && zone.getName().toUpperCase() === name) {
                    return zone;
                }
            }
        }

        return false;
    }

    getControllersSize() {
        return this._zones.length;
    }

    getZonesSize(ctrllrID) {
        if (this._zones[ctrllrID] != null) {
            return this._zones[ctrllrID].length;
        }
        else {
            return 0;
        }
    }

    createSource(sourceID, name, type, writeConfig=true) {
        if (!this._sources[sourceID]) {
            let source = new Source(this, sourceID, name, type && type || Source.TYPE_GENERIC);
            this._sources[sourceID] = source;

            if (writeConfig) {
                this.writeSources();
            }

            source.on("name", (name, oldName) => {
                this.emit("source-name", source, name, oldName);
                this.writeSources();
            })
            .on("type", (type) => {
                this.emit("source-type", source, type);
                this.writeSources();
            })
            .on("media-metadata", (title, artist, artworkURL) => {
                this.emit("media-metadata", source, title, artist, artworkURL);
                console.info("Source #%d (%s) is now playing %s by %s", sourceID, name, title, artist);
            })
            .on("media-playing", (playing) => {
                this.emit("media-playing", source, playing);
                console.info("Source #%d (%s) play state changed to %s", sourceID, name, playing);
            })
            .on("descriptive-text", (message, flashTime, rNetTriggered) => {
                if (!rNetTriggered) {
                    this.sendData(new SourceDescriptiveTextPacket(sourceID, flashTime, message));
                }
                this.emit("descriptive-text", source, flashTime, message);
                console.info("Source #%d (%s) published descriptive text: %s", sourceID, name, message);
            })
            .on("control", (operation, rNetTriggered) => {
                if (!rNetTriggered && !source.networkControlled()) {
                    let zones = source.getZones();
                    if (zones.length > 0) {
                        let key = false;
                        switch (operation) {
                        case Source.CONTROL_NEXT:
                            key = KeypadEventPacket.NEXT;
                            break;
                        case Source.CONTROL_PREV:
                            key = KeypadEventPacket.PREVIOUS;
                            break;
                        case Source.CONTROL_STOP:
                            key = KeypadEventPacket.STOP;
                            break;
                        case Source.CONTROL_PLAY:
                            key = KeypadEventPacket.PLAY;
                            break;
                        case Source.CONTROL_PAUSE:
                            key = KeypadEventPacket.PAUSE;
                            break;
                        case Source.CONTROL_PLUS:
                            key = KeypadEventPacket.PLUS;
                            break;
                        case Source.CONTROL_MINUS:
                            key = KeypadEventPacket.MINUS;
                            break;
                        }

                        this.sendData(new KeypadEventPacket(zones[0].getControllerID(), zones[0].getZoneID(), key));
                    }
                }
            })
            .on("override-name", () => {
                this.sendData(new SourceDescriptiveTextPacket(sourceID, 0, source.getName()));
            });

            this.emit("new-source", source);
            return source;
        }
        return false;
    }

    deleteSource(sourceID) {
        this._sources[sourceID].removeAllListeners();
        delete this._sources[sourceID];

        let lastNonNull = false;
        for (let i = this._sources.length - 1; i >= 0; i--)
            if (this._sources[i]) {
                lastNonNull = i;
                break;
            }
        this._sources.splice(lastNonNull+1, this._sources.length - lastNonNull + 1)

        this.writeSources();
        this.emit("source-deleted", sourceID);
    }

    getSource(sourceID) {
        if (this._sources[sourceID]) {
            return this._sources[sourceID];
        }
        else {
            return null;
        }
    }

    findSourceByName(name) {
        name = name.toUpperCase();
        for (let sourceID = 0; sourceID < this._sources.length; sourceID++) {
            if (this._sources[sourceID] != null && this._sources[sourceID].getName().toUpperCase() == name) {
                return this._sources[sourceID];
            }
        }

        return false;
    }

    getSourcesSize() {
        return this._sources.length;
    }

    getSources() {
        return this._sources;
    }

    getSourcesByType(type) {
        let sources = [];
        for (let sourceID = 0; sourceID < this._sources.length; sourceID++) {
            if (this._sources[sourceID] != null && this._sources[sourceID].getType() == type) {
                sources.push(this._sources[sourceID]);
            }
        }
        return sources;
    }

    setAutoUpdate(enabled) {
        if (this._autoUpdating != enabled) {
            this._autoUpdating = enabled;

            //console.log("DEBUG: RNet auto-update set to " + enabled);

            if (enabled) {
                this._autoUpdateInterval = setInterval(() => {
                    this.requestAllZoneInfo();
                }, 30000);
            }
            else {
                clearInterval(this._autoUpdateInterval);
                this._autoUpdateInterval = undefined;
            }
        }
    }

    requestAllZoneInfo(forceAll=false) {
        for (var ctrllrID in this._zones) {
            for (var zoneID in this._zones[ctrllrID]) {
                this._zones[ctrllrID][zoneID].requestInfo();
            }
        }
    }

    setAllPower(power) {
        this.sendData(new SetAllPowerPacket(power));

        setTimeout(() => {
            this.requestAllZoneInfo(true);
        }, 1000);
    }

    setAllMute(muted, fadeTime=0) {
        this._allMuted = muted;
        for (let ctrllrID = 0; ctrllrID < this.getControllersSize(); ctrllrID++) {
            for (let zoneID = 0; zoneID < this.getZonesSize(ctrllrID); zoneID++) {
                const zone = this.getZone(ctrllrID, zoneID);

                if (zone != null && zone.getPower()) {
                    zone.setMute(muted, fadeTime);
                }
            }
        }
    }

    getAllMute() {
        return this._allMuted;
    }

    isConnected() {
        return this._connected;
    }

    sendData(packet, queueLoop=false) {
        if (this._serialPort !== undefined) {
            if (packet instanceof HandshakePacket) {
                this._serialPort.write(packet.getBuffer());
                //console.log("DEBUG: Sent handshake packet to RNet.");

                if (!this._waitingForHandshake) {
                    console.warn("Unexpectedly sent a handshake packet!");
                }
                else {
                    this._waitingForHandshake = false;
                    clearTimeout(this._waitingForHandshakeTimeout);
                }

                if (this._packetQueue.length > 0) {
                    this.sendData(this._packetQueue.shift(), true);
                }
            }
            else if (this._waitingForHandshake || (!queueLoop && this._packetQueue.length > 0)) {
                //console.log("DEBUG: Queued packet while expecting to perform a handshake.");
                this._packetQueue.push(packet);
            }
            else {
                this._serialPort.write(packet.getBuffer());
                //console.log("DEBUG: Sent packet " + packet.constructor.name + " to RNet.");

                if (packet.causesResponseWithHandshake()) {
                    //console.log("DEBUG: Now expecting to perform handshake.");
                    this._waitingForHandshake = true;
                    this._waitingForHandshakeTimeout = setTimeout(() => {
                        console.warn("Waited for expected handshake for too long. Continuing...");
                        this._waitingForHandshake = false;
                        if (this._packetQueue.length > 0) {
                            this.sendData(this._packetQueue.shift(), true);
                        }
                    }, 3000);
                }

                if (!this._waitingForHandshake && this._packetQueue.length > 0) {
                    this.sendData(this._packetQueue.shift(), true);
                }
            }
        }
    }

    _handleData(data) {
        for (var b of data) {
            if (this._invertNextPacket) {
                b = ~ b[0] & 0xFF
                this._invertNextPacket = false;
            }

            if (b == 0xF0) {
                if (this._pendingPacket !== undefined) {
                    console.warn("Received START_MESSAGE_BYTE before recieving a END_MESSAGE_BYTE from RNet");
                    delete this._pendingPacket;
                    this._pendingPacket = undefined;
                }
                this._pendingPacket = new SmartBuffer();
                this._pendingPacket.writeUInt8(b);
            }
            else if (b == 0xF7) {
                if (this._pendingPacket !== undefined) {
                    this._pendingPacket.writeUInt8(b);
                    const buffer = this._pendingPacket.toBuffer();
                    delete this._pendingPacket;
                    this._pendingPacket = undefined;
                    setImmediate(() => {
                        const packet = PacketBuilder.build(buffer);
                        if (packet) {
                            this._receivedRNetPacket(packet);
                        }
                    });
                }
                else {
                    console.warn("Received packet from RNet without start of new message.");
                }
            }
            else if (b == 0xF1) {
                if (this._pendingPacket !== undefined) {
                    this._invertNextPacket = true;
                }
                else {
                    console.warn("Received packet from RNet without start of new message.");
                }
            }
            else {
                if (this._pendingPacket !== undefined) {
                    this._pendingPacket.writeUInt8(b);
                }
                else {
                    console.warn("Received packet from RNet without start of new message.");
                }
            }
        }
    }

    _receivedRNetPacket(packet) {
        //console.log("DEBUG: Received packet " + packet.constructor.name + " from RNet.");

        if (packet.requiresHandshake()) {
            this.sendData(new HandshakePacket(packet.sourceControllerID, 2));
        }

        if (packet instanceof ZoneInfoPacket) {
            const zone = this.getZone(packet.getControllerID(), packet.getZoneID());
            if (zone) {
                zone.setPower(packet.getPower(), true);
                zone.setSourceID(packet.getSourceID(), true);
                zone.setVolume(packet.getVolume(), true);
                zone.setParameter(ExtraZoneParam.BASS, packet.getBassLevel(), true);
                zone.setParameter(ExtraZoneParam.TREBLE, packet.getTrebleLevel(), true);
                zone.setParameter(ExtraZoneParam.LOUDNESS, packet.getLoudness(), true);
                zone.setParameter(ExtraZoneParam.BALANCE, packet.getBalance(), true);
                zone.setParameter(ExtraZoneParam.PARTY_MODE, packet.getPartyMode(), true);
                zone.setParameter(ExtraZoneParam.DO_NOT_DISTURB, packet.getDoNotDisturbMode(), true);
            }
            else {
                console.warn("Received ZoneInfoPacket for unknown zone %d-%d", packet.getControllerID(), packet.getZoneID());
            }
        }
        else if (packet instanceof ZonePowerPacket) {
            const zone = this.getZone(packet.getControllerID(), packet.getZoneID());
            if (zone) {
                zone.setPower(packet.getPower(), true);
            }
            else {
                console.warn("Received ZonePowerPacket for unknown zone %d-%d", packet.getControllerID(), packet.getZoneID());
            }
        }
        else if (packet instanceof ZoneSourcePacket) {
            const zone = this.getZone(packet.getControllerID(), packet.getZoneID());
            if (zone) {
                zone.setSourceID(packet.getSourceID(), true);
            }
            else {
                console.warn("Received ZoneSourcePacket for unknown zone %d-%d", packet.getControllerID(), packet.getZoneID());
            }
        }
        else if (packet instanceof ZoneVolumePacket) {
            const zone = this.getZone(packet.getControllerID(), packet.getZoneID());
            if (zone) {
                zone.setVolume(packet.getVolume(), true);
            }
            else {
                console.warn("Received ZoneVolumePacket for unknown zone %d-%d", packet.getControllerID(), packet.getZoneID());
            }
        }
        else if (packet instanceof ZoneParameterPacket) {
            const zone = this.getZone(packet.getControllerID(), packet.getZoneID());
            if (zone) {
                zone.setParameter(packet.getParameterID(), packet.getValue(), true);
            }
            else {
                console.warn("Received ZoneParameterPacket for unknown zone %d-%d", packet.getControllerID(), packet.getZoneID());
            }
        }
        else if (packet instanceof RenderedDisplayMessagePacket) {
            /*console.log("RenderedDisplayMessage:")
            console.log("Target: %d -> %d -> %d", packet.targetControllerID, packet.targetZoneID, packet.targetKeypadID);
            console.log("Source: %d -> %d -> %d", packet.sourceControllerID, packet.sourceZoneID, packet.sourceKeypadID)
            console.log("Render Type: %d", packet.renderType);
            console.log("Flash Time: %d", packet.flashTime);
            console.log("Value Low: %d", packet.getLowValue());
            console.log("Value High: %d", packet.getHighValue());
            console.log("Short Value: %d", packet.getShortValue());*/

            switch (packet.getRenderType()) {
                case RenderedDisplayMessagePacket.TYPE_SOURCE_NAME:
                    this.getZone(packet.targetControllerID, packet.targetZoneID).setSourceID(packet.getHighValue(), true);
                    break;
                case RenderedDisplayMessagePacket.TYPE_VOLUME:
                    this.getZone(packet.targetControllerID, packet.targetZoneID).setVolume(packet.getLowValue() * 2, true);
                    break;
            }
        }
        else if (packet instanceof KeypadEventPacket) {
            const zone = this.getZone(packet.sourceControllerID, packet.sourceZoneID);
            const source = this.getSource(zone.getSourceID());
            if (zone != null) {
                switch (packet.getKey()) {
                    case KeypadEventPacket.KEYS.POWER:
                        zone.setPower(!zone.getPower(), true);
                        return;
                }

                if (source != null) {
                    switch (packet.getKey()) {
                        case KeypadEventPacket.KEYS.NEXT:
                            source.control(Source.CONTROL_NEXT, true);
                            break;
                        case KeypadEventPacket.KEYS.PREVIOUS:
                            source.control(Source.CONTROL_PREV, true);
                            break;
                        case KeypadEventPacket.KEYS.PLUS:
                            source.control(Source.CONTROL_PLUS, true);
                            break;
                        case KeypadEventPacket.KEYS.MINUS:
                            source.control(Source.CONTROL_MINUS, true);
                            break;
                        case KeypadEventPacket.KEYS.STOP:
                            source.control(Source.CONTROL_STOP, true);
                            break;
                        case KeypadEventPacket.KEYS.PAUSE:
                            source.control(Source.CONTROL_PAUSE, true);
                            break;
                        case KeypadEventPacket.KEYS.PLAY:
                            source.control(Source.CONTROL_PLAY, true);
                            break;
                    }
                }
            }
            else {
                console.warn("Received keypad event from unknown Zone (%d-%d)", packet.sourceControllerID, packet.sourceZoneID);
            }
        }
    }
}

module.exports = RNet;
