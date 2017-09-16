const EventPacket = require("./EventPacket");

module.exports = class SetPowerPacket extends EventPacket {
    constructor(controllerID, zoneID, power) {
        super();

        this._controllerID = controllerID;
        this._zoneID = zoneID;
        this._power = power ? 1 : 0;
    }

    getTargetControllerID() {
        return this._controllerID;
    }

    getTargetPath() {
        return [
            0x02, // Root Menu
            0x00 // Run Mode
        ]
    }

    getEventID() {
        return 0xDC;
    }

    getEventTimestamp() {
        return this._power;
    }

    getEventData() {
        return this._zoneID;
    }

    getEventPriority() {
        return 0x01;
    }
}
