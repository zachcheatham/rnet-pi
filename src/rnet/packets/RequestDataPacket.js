const RNetPacket = require("./RNetPacket");
const SmartBuffer = require("smart-buffer").SmartBuffer;

class RequestDataPacket extends RNetPacket {
    constructor(controllerID, zoneID, dataType) {
        super();

        this.targetControllerID = controllerID;
        this.messageType = 0x01;
        this.sourcePath = [];
        this.targetPath = [
            0x02,
            0x00,
            zoneID,
            dataType
        ]
    }

    getMessageBody() {
        const buffer = new SmartBuffer();

        buffer.writeUInt8(this.targetPath.length);
        for (let i = 0; i < this.targetPath.length; i++) {
            buffer.writeUInt8(this.targetPath[i])
        }

        buffer.writeUInt8(this.sourcePath.length);
        for (var i = 0; i < this.sourcePath.length; i++) {
            buffer.writeUInt8(this.sourcePath[i])
        }

        // Request Data Type
        // NOTE Assuming this is always 0;
        buffer.writeUInt8(0x00);

        return buffer.toBuffer();
    }

    causesResponseWithHandshake() {
        return true;
    }
}

RequestDataPacket.DATA_TYPE = {
    ZONE_INFO: 0x07,
    ZONE_POWER: 0x06,
    ZONE_SOURCE: 0x02,
    ZONE_VOLUME: 0x01
}

module.exports = RequestDataPacket;
