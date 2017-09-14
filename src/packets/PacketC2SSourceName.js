const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x06
 * Source Name
 * Renames a source
 * Data:
 *      (Unsigned Char) Source ID
 *      (NT String) New Source Name
 */
class PacketC2SSourceName extends PacketC2S {
    parseData() {
        this.sourceID = this._buffer.readUInt8();
        this.name = this._buffer.readStringNT();
    }
}

module.exports = PacketC2SSourceName;
