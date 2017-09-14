const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x01
 * Zone Name
 * Renames a zone
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 *      (NT String) New Zone Name
 */
class PacketC2SZoneName extends PacketC2S {
    parseData() {
        this.ctrllrID = this._buffer.readUInt8();
        this.zoneID = this._buffer.readUInt8();
        this.name = this._buffer.readStringNT();
    }
}

module.exports = PacketC2SZoneName;
