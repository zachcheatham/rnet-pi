const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x0C
 * All Power
 * Sets on/off state of all zones
 * Data:
 *      (Unsigned Char) On/Off
 */
class PacketC2SAllPower extends PacketC2S {
    parseData() {
        this._power = (this._buffer.readUInt8() == 1);
    }

    getPowered() {
        return this._power;
    }

    getID() {
        return PacketC2SAllPower.ID;
    }
}

PacketC2SAllPower.ID = 0x0C;

module.exports = PacketC2SAllPower;
