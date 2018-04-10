const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x32
 * Request Properties
 */
class PacketC2SRequestSourceProperties extends PacketC2S {
    parseData() {
        this._sourceID = this._buffer.readUInt8();
    }

    getSourceID() {
        return this._value;
    }

    getID() {
        return PacketC2SRequestSourceProperties.ID;
    }
}

PacketC2SRequestSourceProperties.ID = 0x32;

module.exports = PacketC2SRequestSourceProperties;
