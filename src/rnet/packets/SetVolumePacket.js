const EventPacket = require("./EventPacket");

module.exports = class SetVolumePacket extends EventPacket {
    constructor(controllerID, zoneID, volume) {
        super();

        this._controllerID = controllerID;
        this._zoneID = zoneID;

        if (volume < 0 || volume > 50) {
            throw new Error("Volume out of range (0-50) when constructing SetVolumePacket");
            return;
        }

        this._volume = volume;
    }

    getTargetPath() {
        return [
            0x02, // Root Menu
            0x00 // Run Mode
        ]
    }

    getEventID() {
        return 0xDE;
    }

    getEventTimestamp() {
        return this._volume;
    }

    getEventData() {
        return this._zoneID;
    }

    getEventPriority() {
        return 0x01;
    }
}
