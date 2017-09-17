const SmartBuffer = require("smart-buffer").SmartBuffer;

const BYTE_START_MESSAGE = 0xF0;
const BYTE_END_MESSAGE = 0xF7;

class RNetPacket {
    constructor() {
        this.targetControllerID = 0x00;
        this.targetZoneID = 0x00;
        this.targetKeypadID = 0x7F;
        this.sourceControllerID = 0x00;
        this.sourceZoneID = 0x00;
        this.sourceKeypadID = 0x70;
    }

    getBuffer() {
        const buffer = new SmartBuffer();
        buffer.writeUInt8(BYTE_START_MESSAGE);
        buffer.writeUInt8(this.targetControllerID);
        buffer.writeUInt8(this.targetZoneID);
        buffer.writeUInt8(this.targetKeypadID);
        buffer.writeUInt8(this.sourceControllerID);
        buffer.writeUInt8(this.sourceZoneID);
        buffer.writeUInt8(this.sourceKeypadID);
        buffer.writeUInt8(this.messageType);

        buffer.writeBuffer(this.getMessageBody())

        buffer.writeUInt8(this.calculateChecksum(buffer));
        buffer.writeUInt8(BYTE_END_MESSAGE);

        return buffer.toBuffer();
    }

    getMessageBody() {
        // Subclasses must handle getMessageBody() in order to
        // write inverted packets
        throw new Error("getMessageBody() not implemented.");
    }

    calculateChecksum(buffer) {
        const totalBytes = buffer.length;
        var byteSum = 0;

        buffer.moveTo(0);
        for (var i = 0; i < totalBytes; i++) {
            byteSum += buffer.readUInt8();
        }

        byteSum += totalBytes;
        byteSum = byteSum & 0x007F;

        if (byteSum > 127) {
            console.warn("Checksum is true byte! It happened not sure if I'm supposed to handle it. If whatever you tried to do didn't work, it means I do.")
        }

        return byteSum;
    }

    writeWithInvertUInt16LE(buffer, value) {
        var b = [
            value & 0x00FF,
            (value & 0xFF00) >> 8
        ];

        if (b[0] > 127) {
            buffer.writeUInt8(0xF1); // Inve // End of message;rt signal
            buffer.writeUInt8(~ b[0] & 0xFF); // Invert
        }
        else {
            buffer.writeUInt8(b[0]);
        }

        buffer.writeUInt8(b[1]);
    }

    writeWithInvertUInt8(buffer, value) {
        var b = value & 0x00FF

        if (b > 127) {
            buffer.writeUInt8(0xF1); // Invert signal
            buffer.writeUInt8(~ b & 0xFF); // Invert
        }
        else {
            buffer.writeUInt8(b);
        }
    }

    copyToPacket(packet) {
        if (packet.messageType == this.messageType) {
            packet.targetControllerID = this.targetControllerID;
            packet.targetZoneID = this.targetZoneID;
            packet.targetKeypadID = this.targetKeypadID;
            packet.sourceControllerID = this.sourceControllerID;
            packet.sourceZoneID = this.sourceZoneID;
            packet.sourceKeypadID = this.sourceKeypadID;
        }
        else {
            throw new Error("Attempted to copy values to packet with different messageType");
        }
    }
}

RNetPacket.fromData = function(data) {
    const packet = new RNetPacket();
    const buffer = SmartBuffer.fromBuffer(data);
    if (buffer.readUInt8() != BYTE_START_MESSAGE) {
        throw new Error("RNetPacket data didn't begin with BYTE_START_MESSAGE.")
    }

    packet.targetControllerID = buffer.readUInt8();
    packet.targetZoneID = buffer.readUInt8();
    packet.targetKeypadID = buffer.readUInt8();
    packet.sourceControllerID = readUInt8();
    packet.sourceZoneID = readUInt8();
    packet.sourceKeypadID = readUInt8();
    packet.messageType = readUInt8();
    packet.messageBody = buffer.readBuffer(buffer.remaining() - 2);
    buffer.readUInt8() // checksum TODO we really should check it.
    if (buffer.readUInt8() != BYTE_END_MESSAGE) {
        throw new Error("RNetPacket data didn't end with BYTE_END_MESSAGE")
    }
}

module.exports = RNetPacket;
