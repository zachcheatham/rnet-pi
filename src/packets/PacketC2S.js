const SmartBuffer = require("smart-buffer").SmartBuffer;

class PacketC2S {
    constructor(data) {
        if (new.target === PacketC2S) {
            throw new TypeError("Cannot initiate a PacketC2S object. PacketC2S is meant to be extended.");
        }

        if (this.parseData === undefined) {
            throw new TypeError("PacketC2S subclasses must implement parseData()");
        }

        this._buffer = SmartBuffer.fromBuffer(data);
        this.parseData();
    }

    getID() {
        return this.id;
    }
}

module.exports = PacketC2S;
