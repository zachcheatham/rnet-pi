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
        this._sourceID = this._buffer.readUInt8();
    }

    getSourceID() {
        return this._sourceID;
    }

    getID() {
        return PacketC2SDeleteSource.ID;
    }
}

PacketC2SDeleteSource.ID = 0x07;

module.exports = PacketC2SDeleteSource;
