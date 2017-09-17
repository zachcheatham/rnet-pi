const RNetPacket = require("./RNetPacket");
const SmartBuffer = require("smart-buffer").SmartBuffer;

class DataPacket extends RNetPacket {
    constructor() {
        super();

        this.messageType = 0x00;
        this.sourcePath = [];
        this.packetNumber = 0;
        this.packetCount = 1;
    }

    getMessageBody() {
        const buffer = new SmartBuffer();

        if (this.targetPath !== undefined) {
            buffer.writeUInt8(this.targetPath.length);
            for (let i = 0; i < this.targetPath.length; i++) {
                buffer.writeUInt8(this.targetPath[i])
            }
        }
        else {
            throw new Error("this.targetPath was not set")
        }

        buffer.writeUInt8(this.sourcePath.length);
        for (var i = 0; i < this.sourcePath.length; i++) {
            buffer.writeUInt8(this.sourcePath[i])
        }

        buffer.writeUInt16LE(this.packetNumber);
        buffer.writeUInt16LE(this.packetCount);

        if (this.data !== undefined) {
            buffer.writeUInt16LE(this.data.length);
            buffer.writeBuffer(this.data);
        }
        else {
            throw new Error("this.data was not set");
        }

        return buffer.toBuffer();
    }

    copyToPacket(packet) {
        if (packet.messageType == this.messageType) {
            packet.targetControllerID = this.targetControllerID;
            packet.targetZoneID = this.targetZoneID;
            packet.targetKeypadID = this.targetKeypadID;
            packet.sourceControllerID = this.sourceControllerID;
            packet.sourceZoneID = this.sourceZoneID;
            packet.sourceKeypadID = this.sourceKeypadID;

            packet.targetPath = this.targetPath;
            packet.sourcePath = this.sourcePath;
            packet.packetNumber = this.packetNumber;
            packet.packetCount = this.packetCount;
            packet.data = this.data;
        }
        else {
            throw new Error("Attempted to copy values to packet with different messageType");
        }
    }
}

DataPacket.fromPacket = function(rNetPacket) {
    if (rNetPacket instanceof RNetPacket) {
        const dataPacket = new DataPacket();
        rNetPacket.copyToPacket(dataPacket);

        const buffer = new SmartBuffer(rNetPacket.messageBody);
        dataPacket.targetPath = [];
        {
            let length = buffer.readUInt8();
            for (let i = 0; i < length; i++) {
                dataPacket.targetPath[i] = buffer.readUInt8();
            }
        }
        dataPacket.sourcePath = [];
        {
            let length = buffer.readUInt8();
            for (let i = 0; i < length; i++) {
                dataPacket.sourcePath[i] = buffer.readUInt8();
            }
        }
        dataPacket.packetNumber = buffer.readUInt16LE();
        dataPacket.packetCount = buffer.readUInt16LE();
        const dataLength = buffer.readUInt16LE();
        dataPacket.data = buffer.readBuffer(buffer.remaining() - 2);
        if (dataPacket.data.length != dataLength) {
            return false;
        }
        return dataPacket;
    }
    else {
        throw new TypeError("Cannot create DataPacket with anything other than RNetPacket");
    }
}

module.exports = DataPacket;
