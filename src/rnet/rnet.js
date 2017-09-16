const EventEmitter = require("events");
const SerialPort = require("serialport");

const EXTRA_ZONE_PARAM = require("./extraZoneParam");

class RNet extends EventEmitter {
    constructor() {
        super();

        this.zones = [];
        this.sources = [];
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

    setAutoUpdate(enabled) {

    }

    getZone(ctrllrID, zoneID) {

    }

    displayMessage(message) {

    }

    readConfiguration() {

    }

    writeConfiguration() {

    }
}

module.exports = RNet;
