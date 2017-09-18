const EventEmitter = require("events");
const fs = require("fs");
const SerialPort = require("serialport");
const SmartBuffer = require("smart-buffer").SmartBuffer;

const ExtraZoneParam = require("./extraZoneParam");
const HandshakePacket = require("./packets/HandshakePacket");
const PacketBuilder = require("./packets/PacketBuilder");
const RequestDataPacket = require("./packets/RequestDataPacket");
const SetAllPowerPacket = require("./packets/SetAllPowerPacket");
const SetParameterPacket = require("./packets/SetParameterPacket");
const SetPowerPacket = require("./packets/SetPowerPacket");
const SetSourcePacket = require("./packets/SetSourcePacket");
const SetVolumePacket = require("./packets/SetVolumePacket");
const ZoneInfoPacket = require("./packets/ZoneInfoPacket");
const Zone = require("./zone");

class RNet extends EventEmitter {
    constructor() {
        super();

        this._zones = [];
        this._sources = [];
        this._autoUpdating = false;

        this.readConfiguration();
        this.writeConfiguration();
    }

    connect() {
        // TODO Automatically continue to try to connect
        // TODO This autodetect usb serial
        this._serialPort = new SerialPort("/dev/tty-usbserial1", {
            baudRate: 19200,
            autoOpen: false // TODO Remove after dry debugging
        })
        .on("open", () => {
            this.emit("connected");
            this.requestAllZoneInfo();
        })
        .on("close", () => {
            // TODO Start auto-reconnect
            this.emit("disconnected");
        })
        .on("error", (error) => {
            this.emit("error", error);
        })
        .on("data", (data) => {this._handleData(data)});
    }

    disconnect() {
        this._serialPort.close();
    }

    readConfiguration() {
        var sourceFile;
        try {
            sourceFile = fs.readFileSync("sources.json");
        }
        catch (e) {}

        if (sourceFile && sourceFile.length > 0) {
            this._sources = JSON.parse(sourceFile);
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
                            this.createZone(ctrllrID, zoneID, zones[ctrllrID][zoneID], false);
                        }
                    }
                }
            }
        }
    }

    writeConfiguration() {
        fs.writeFileSync("sources.json", JSON.stringify(this._sources));

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
                        zones[ctrllrID][zoneID] = this._zones[ctrllrID][zoneID].getName();
                    }
                }
            }
        }
        fs.writeFileSync("zones.json", JSON.stringify(zones));
    }

    createZone(ctrllrID, zoneID, name, writeConfig=true) {
        // TODO Prevent overriding existing zone
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
        return true;
    }

    deleteZone(ctrllrID, zoneID) {
        // TODO Check if zone exists
        this._zones[ctrllrID][zoneID].removeAllListeners();

        delete this._zones[ctrllrID][zoneID];
        if (this._zones[ctrllrID].length == 0) {
            delete this._zones[ctrllrID];
        }

        this.writeConfiguration();
        this.emit("zone-deleted", ctrllrID, zoneID);
        return true;
    }

    getZone(ctrllrID, zoneID) {
        if (!this._zones[ctrllrID]) {
            return null;
        }
        else {
            return this._zones[ctrllrID][zoneID];
        }
    }

    getZones() {
        return this._zones;
    }

    createSource(sourceID, name) {
        if (!this._sources[sourceID]) {
            this._sources[sourceID] = {name: name};
            this.writeConfiguration();
            this.emit("new-source", sourceID);
            return true;
        }
        return false;
    }

    renameSource(sourceID, name) {
        if (this._sources[sourceID]) {
            this._sources[sourceID].name = name;
            this.writeConfiguration();
            this.emit("source-name", sourceID);
            return true;
        }
        return false;
    }

    deleteSource(sourceID) {
        delete this._sources[sourceID];
        this.writeConfiguration();
        this.emit("source-deleted", sourceID);
    }

    getSourceName(sourceID) {
        if (this._sources[sourceID]) {
            return this._sources[sourceID].name;
        }
        else {
            return "Unknown";
        }
    }

    sourceExists(sourceID) {
        return (this._sources[sourceID] !== undefined)
    }

    getSources() {
        return this._sources;
    }

    setAutoUpdate(enabled) {
        if (this._autoUpdating != enabled) {
            this._autoUpdating = enabled;

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

    requestAllZoneInfo() {
        for (var ctrllrID in this._zones) {
            for (var zoneID in this._zones[ctrllrID]) {
                this.sendData(new RequestDataPacket(ctrllrID, zoneID, RequestDataPacket.DATA_TYPE.ZONE_INFO));
            }
        }
    }

    setAllPower(power) {
        this.sendData(new SetAllPowerPacket(power));

        setTimeout(() => {
            this.requestAllZoneInfo();
        }, 5000);
    }

    sendData(packet) {
        this._serialPort.write(packet.getBuffer());
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
                console.warn("Received ZoneInfo for unknown zone (%d.%d)", packet.getControllerID(), packet.getZoneID());
            }
        }
    }
}

module.exports = RNet;
