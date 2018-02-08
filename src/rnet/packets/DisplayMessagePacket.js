const DataPacket = require("./DataPacket");
const RNetPacket = require("./RNetPacket");
const SmartBuffer = require("smart-buffer").SmartBuffer;

class DisplayMessagePacket extends DataPacket {
    constructor(ctrllrID, zoneID, alignment, flashTime, message) {
        super();

        this.targetControllerID = ctrllrID;
        this.targetZoneID = zoneID;
        this.targetKeypadID = RNetPacket.KEYPAD_ALL_IN_ZONE;

        this.targetPath = [0x01, 0x01] // Standard Interface -> Display

        const buffer = new SmartBuffer();
        buffer.writeUInt8(alignment);
        buffer.writeUInt16LE(flashTime);
        for (let i = 0; i < 12; i++) {
            if (i < message.length) {
                buffer.writeUInt8(message.charCodeAt(i));
            }
            else {
                buffer.writeUInt8(0x00);
            }
        }
        buffer.writeUInt8(0x00);
        this.data = buffer.toBuffer();
    }
}

DisplayMessagePacket.fromPacket = function(rNetPacket) {
    if (dataPacket instanceof DataPacket) {
        const displayMessagePacket = new DisplayMessagePacket();
        dataPacket.copyToPacket(displayMessagePacket);
        return displayMessagePacket;
    }
    else {
        throw new TypeError("Cannot create DisplayMessagePacket with anything other than a DataPacket");
    }
}

DisplayMessagePacket.ALIGN_CENTER = 0x00;
DisplayMessagePacket.ALIGN_LEFT = 0x01;

module.exports = DisplayMessagePacket;
