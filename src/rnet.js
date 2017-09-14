const EventEmitter = require("events");

const zones = [];
const sources = [];



class RNet extends EventEmitter {
    constructor() {
        super();

        this.zones = [];
        this.sources = [];
    }

    connect() {
        // TODO: Connect to rNET over serial
    }

    setAutoUpdate(enabled) {

    }

    setPower(ctrllrID, zoneID, power, extra_data) {
        // TODO: Get Zone Obj
        // TODO: Check if power differs
        // TODO: Pass Extra Data to volume change event
    }

    setVolume(ctrllrID, zoneID, volume, extra_data) {
        // TODO: Get Zone Obj
        // TODO: Check if volume differs
        // TODO: Pass Extra Data to volume change event
    }

    readConfiguration() {

    }

    writeConfiguration() {

    }
}

RNet.EXTRA_ZONE_PARAM = {
    BASS: 0,
    TREBLE: 1,
    LOUDNESS: 2,
    BALANCE: 3,
    TURN_ON_VOLUME: 4,
    BACKGROUND_COLOR: 5,
    DO_NOT_DISTURB: 6,
    PARTY_MODE: 7,
    FRONT_AV_ENABLE: 8
}


module.exports = RNet;
