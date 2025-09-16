const EventPacket = require("./EventPacket");

class KeypadEventPacket extends EventPacket {
    constructor(controllerID, zoneID, key) {
        super();

        this.targetPath = [0x02, 0x00];
        this.targetControllerID = controllerID;
        this.sourceZoneID = zoneID
        this.eventID = key;
        this.eventData = 0;
        this.eventTimestamp = 0;
        this.eventPriority = 1;
    }

    getControllerID() {
        return this.targetControllerID;
    }

    getZoneID() {
        return this.sourceZoneID;
    }

    getKey() {
        return this.eventID;
    }
}

KeypadEventPacket.fromPacket = function(eventPacket) {
    if (eventPacket instanceof EventPacket) {
        const keypadEventPacket = new KeypadEventPacket();
        eventPacket.copyToPacket(keypadEventPacket);
        return keypadEventPacket;
    }
    else {
        throw new TypeError("Cannot create KeypadEventPacket from anything other than an EventPacket");
    }
}

KeypadEventPacket.KEYS = {
    SETUP_BUTTON: 0x64,
    PREVIOUS: 0x67,
    NEXT: 0x68,
    PLUS: 0x69,
    MINUS: 0x6A,
    SOURCE: 0x6B,
    POWER: 0x6C,
    STOP: 0x6D,
    PAUSE: 0x6E,
    FAVORITE_1: 0x6F,
    FAVORITE_2: 0x70,
    PLAY: 0x73
}

module.exports = KeypadEventPacket;
