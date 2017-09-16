const RequestDataPacket = require("./RequestDataPacket");

module.exports = class RequestAllZoneInfoPacket extends RequestDataPacket {
    constructor(controllerID, zoneID) {
        super();

        this._controllerID = controllerID;
        this._zoneID = zoneID;
    }

    getTargetPath() {
        return [
            0x02, // Root Menu
            0x00, // Run Mode
            this._zoneID, // Zone ID
            0x07 // Zone Info
        ]
    }

    getRequestType() {
        return 0x00; // Request Data
    }
}
