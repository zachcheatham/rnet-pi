const Packet = require("./Packet");

module.exports = class RequestDataPacket extends Packet {
    constructor() {
        super();

        if (new.target === RequestDataPacket) {
            throw new TypeError("Cannot initiate a RequestDataPacket object. RequestDataPacket is meant to be extended.");
        }

        if (this.getRequestType === undefined) {
            throw new TypeError("RequestDataPacket subclasses must implement getRequestType()");
        }
    }

    getMessageType(data) {
        return 0x01;
    }

    writePacketBody(buffer) {
        buffer.writeUInt8(this.getRequestType());
    }
}
