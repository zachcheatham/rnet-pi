const EventEmitter = require("events");

const EXTRA_ZONE_PARAM = require("./extraZoneParam");

class RNet extends EventEmitter {
    constructor() {
        super();

        this.zones = [];
        this.sources = [];
    }

    connect() {
        // TODO: Connect to rNET over serial
    }

    disconnect() {

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
