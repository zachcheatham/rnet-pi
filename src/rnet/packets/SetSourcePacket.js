const EventPacket = require("./EventPacket");

module.exports = class SetSourcePacket extends EventPacket {
    constructor(controllerID, zoneID, sourceID) {
        super();

        this._controllerID = controllerID;
        this._zoneID = zoneID;
        this._sourceID = sourceID;
    }

    getTargetControllerID() {
        return this._controllerID;
    }

    getSourceZoneID() {
        return this._zoneID;
    }

    getTargetPath() {
        return [
            0x00, // Root Menu
            0x00 // Run Mode
        ]
    }

    getEventID() {
        return 0xC1;
    }

    getEventTimestamp() {
        return 0x00;
    }

    getEventData() {
        return this._sourceID;
    }

    getEventPriority() {
        return 0x01;
    }
}
