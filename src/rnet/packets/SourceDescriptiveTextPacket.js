const DataPacket = require("./DataPacket");
const RNetPacket = require("./RNetPacket");
const SmartBuffer = require("smart-buffer").SmartBuffer;

class SourceDescriptiveTextPacket extends DataPacket {
    constructor(sourceID, flashTime, message) {
        super();

        this.targetControllerID = RNetPacket.CONTROLLER_ALL_KEYPADS;
        this.targetKeypadID = RNetPacket.KEYPAD_ALL_ON_SOURCE;
        this.sourceKeypadID = RNetPacket.KEYPAD_CONTROLLER;

        this.targetPath = [0x01, 0x01] // Standard Interface -> Display

        const buffer = new SmartBuffer();
        buffer.writeUInt8(0x10 | sourceID);
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

SourceDescriptiveTextPacket.fromPacket = function(dataPacket) {
    if (dataPacket instanceof DataPacket) {
        const sourceDescriptiveTextPacket = new SourceDescriptiveTextPacket();
        dataPacket.copyToPacket(sourceDescriptiveTextPacket);
        return sourceDescriptiveTextPacket;
    }
    else {
        throw new TypeError("Cannot create SourceDescriptiveTextPacket with anything other than a DataPacket");
    }
}

module.exports = SourceDescriptiveTextPacket;
