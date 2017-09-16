const Packet = require("./Packet");

module.exports = class DataPacket extends Packet {
    constructor() {
        super();

        if (new.target === DataPacket) {
            throw new TypeError("Cannot initiate a DataPacket object. DataPacket is meant to be extended.");
        }

        if (this.getData === undefined) {
            throw new TypeError("DataPacket subclasses must implement getData()");
        }
    }

    getMessageType(data) {
        return 0x00;
    }

    writePacketBody(buffer) {
        buffer.writeUInt16LE(0); // Packet 1
        buffer.writeUInt16LE(1); // Of 1
        // NOTE There is no use of split packets in the documentation
        //      but there's a chance that it works on longer
        //      display string packets.

        let data = this.getData();
        buffer.writeUInt16LE(data.length); // Following data length
        for (var i = 0; i < data.length; i++) {
            buffer.writeUInt8(data[i]);
        }
    }
}
