const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x06
 * Source Info
 * Update source info
 * Data:
 *      (Unsigned Char) Source ID
 *      (NT String) New Source Name
 *      (Unsigned Char) Source Type ID [OPTIONAL]
 */
class PacketC2SSourceInfo extends PacketC2S {
    parseData() {
        this._sourceID = this._buffer.readUInt8();
        this._name = this._buffer.readStringNT();
        if (this._buffer.remaining() > 0)
            this._sourceTypeID = this._buffer.readUInt8();
        else
            this._sourceTypeID = 0;
    }

    getSourceID() {
        return this._sourceID;
    }

    getName() {
        return this._name;
    }

    getSourceTypeID() {
        return this._sourceTypeID;
    }

    getID() {
        return PacketC2SSourceInfo.ID;
    }
}

PacketC2SSourceInfo.ID = 0x06;

module.exports = PacketC2SSourceInfo;
