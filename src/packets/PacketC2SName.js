const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x01
 * Name request
 * Contains the device name
 */
class PacketC2SName extends PacketC2S {
    parseData() {
        this.name = this._buffer.readStringNT();
    }
}

module.exports = PacketC2SName;
