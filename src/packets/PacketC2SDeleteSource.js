const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x07
 * Delete Source
 * Deletes a Source
 * Data:
 *      (Unsigned Char) Source ID
 */
class PacketC2SDeleteSource extends PacketC2S {
    parseData() {
        this.sourceID = this._buffer.readUInt8();
    }
}

module.exports = PacketC2SDeleteSource;
