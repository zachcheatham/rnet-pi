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
        this._sourceID = this._buffer.readUInt8();
        this._name = this._buffer.readStringNT();
    }

    getSourceID() {
        return this._sourceID;
    }

    getName() {
        return this._name;
    }

    getID() {
        return PacketC2SSourceName.ID;
    }
}

PacketC2SSourceName.ID = 0x06;

module.exports = PacketC2SSourceName;
