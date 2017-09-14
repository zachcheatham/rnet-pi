const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x05
 * Delete Zone
 * Deletes a Zone
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 */
class PacketC2SDeleteZone extends PacketC2S {
    parseData() {
        this.ctrllrID = this._buffer.readUInt8();
        this.zoneID = this._buffer.readUInt8();
    }
}

module.exports = PacketC2SDeleteZone;
