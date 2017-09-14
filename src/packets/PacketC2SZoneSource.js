const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x0A
 * Zone Source
 * Sets a zone's source
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 *      (Unsigned Char) Source ID
 */
class PacketC2SZoneSource extends PacketC2S {
    parseData() {
        this.ctrllrID = this._buffer.readUInt8();
        this.zoneID = this._buffer.readUInt8();
        this.sourceID = this._buffer.readUInt8();
    }
}

module.exports = PacketC2SZoneSource;
