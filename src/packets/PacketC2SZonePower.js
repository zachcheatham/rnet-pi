const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x08
 * Zone Power
 * Turns a Zone On/Off
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 *      (Unsigned Char) On/Off State
 */
class PacketC2SZonePower extends PacketC2S {
    parseData() {
        this.ctrllrID = this._buffer.readUInt8();
        this.zoneID = this._buffer.readUInt8();
        this.power = (this._buffer.readUInt8() == 1);
    }
}

module.exports = PacketC2SZonePower;
