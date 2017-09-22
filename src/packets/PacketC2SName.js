const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x01
 * Name request
 * Contains the device name
 */
class PacketC2SName extends PacketC2S {
    parseData() {
        this._name = this._buffer.readStringNT();
    }

    getName() {
        return this._name;
    }

    getID() {
        return PacketC2SName.ID;
    }
}

PacketC2SName.ID = 0x01;

module.exports = PacketC2SName;
