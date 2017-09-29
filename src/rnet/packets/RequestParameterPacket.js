const RNetPacket = require("./RNetPacket");
const SmartBuffer = require("smart-buffer").SmartBuffer;

class RequestParameterPacket extends RNetPacket {
    constructor(controllerID, zoneID, parameterID) {
        super();

        this.targetPath = [
            0x02,
            0x00,
            zoneID,
            0x00,
            parameterID
        ]
    }
}
