const Packet = require("./Packet");

module.exports = class HandshakePacket extends Packet {
    constructor(type=2) {
        super();

        this._type = type;
    }

    getMessageType(data) {
        return 0x02;
    }

    writePacketBody(buffer) {
        buffer.writeUInt8(this._type);
    }
}
