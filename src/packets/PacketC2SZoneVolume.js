const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x09
 * Zone Volume
 * Sets a zone's volume
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 *      (Unsigned Char) Volume
 */
class PacketC2SZoneVolume extends PacketC2S {
    parseData() {
        this.ctrllrID = this._buffer.readUInt8();
        this.zoneID = this._buffer.readUInt8();
        this.volume = this._buffer.readUInt8();
    }
}

module.exports = PacketC2SZoneVolume;
