const Packet = require("./Packet");

class DataPacket extends Packet {
    constructor() {
        super();

        if (new.target === DataPacket) {
            throw new TypeError("Cannot initiate a DataPacket object. DataPacket is meant to be extended.");
        }

        if (this.usesMultiPacket === undefined) {
            throw new TypeError("DataPacket subclasses must implement usesMultiPacket");
        }

        if (this.getPacketCount === undefined) {
            throw new TypeError("DataPacket subclasses that use multipacket must implement getPacketCount()");
        }

        if (this.getData === undefined) {
            throw new TypeError("DataPacket subclasses must implement getData");
        }
    }

    getMessageType(data) {
        return 0x00;
    }

    writePacketBody(buffer, packetNumber=0) {
        buffer.writeUInt16LE(packetNumber);
        buffer.writeUInt16LE(this.getPacketCount());

        let data = this.getData(packetNumber);
        if (data.length > DataPacket.DATA_SIZE) {
            throw new Error("DataPacket subclass returned too long of a byte array.");
            return;
        }

        buffer.writeUInt16LE(data.length);
        for (var i = 0; i < DataPacket.DATA_SIZE, i++) {
            if (data[i]) {
                buffer.writeUInt8(data[i]);
            }
            else {
                buffer.writeUInt8(0x00);
            }
        }
    }
}

DataPacket.DATA_SIZE = 16;
