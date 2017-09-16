const EventEmitter = require("events");
const fs = require("fs");
const SerialPort = require("serialport");

const EXTRA_ZONE_PARAM = require("./extraZoneParam");
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
        })
        .on("close", () => {
            // TODO Start auto-reconnect
            this.emit("disconnected");
        })
        .on("error", (error) => {
            this.emit("error", error);
        })
        .on("data", this._handleData);
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

        if (sourceFile.length > 0) {
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

    createZone(ctrllrID, zoneID, name, writeConfig=false) {
        const zone = new Zone(ctrllrID, zoneID);
        zone.setName(name);

        if (!this._zones[ctrllrID]) {
            this._zones[ctrllrID] = [];
        }
        this._zones[ctrllrID][zoneID] = zone;

        if (writeConfig) {
            this.writeConfiguration();
        }
    }

    deleteZone(ctrllrID, zoneID) {
        delete this._zones[ctrllrID][zoneID];
        if (this._zones[ctrllrID].length == 0) {
            delete this._zones[ctrllrID];
        }

        this.writeConfiguration();
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

    getZone(ctrllrID, zoneID) {
        return this._zones[ctrllrID][zoneID];
    }

    requestAllZoneInfo() {
        for (var ctrllrID in this._zones) {
            for (var zoneID in this._zones[ctrllrID]) {
                var zone = this._zones[ctrllrID][zoneID];
                this.sendData(new RequestAllZoneInfoPacket(zone.getControllerID(), zone.getZoneID()).getBuffer());
            }
        }
    }

    sendData(packet) {
        this._serialPort.write(packet.getBuffer());
    }

    _handleData(data) {
        // TODO Construct some kind of packet :/
        console.log(data);
    }
}

module.exports = RNet;
