const SmartBuffer = require("smart-buffer").SmartBuffer;

class PacketS2C {
    constructor() {
        if (new.target === PacketS2C) {
            throw new TypeError("Cannot initiate a PacketS2C object. Packet is meant to be extended.");
        }

        if (this.getID === undefined) {
            throw new TypeError("PacketS2C subclasses must implement getID()");
        }

        this._buffer = new SmartBuffer();
        this._buffer.writeUInt8(this.getID());
    }

    getBuffer() {
        // TODO Only allow this to happen once
        this._buffer.writeUInt8(this._buffer.length - 1, 1);
        return this._buffer.toBuffer();
    }
}

module.exports = PacketS2C;
