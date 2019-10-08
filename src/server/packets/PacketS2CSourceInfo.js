const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x06
 * Zone Name
 * Sends source information
 * Data:
 *      (Unsigned Char) SourceID
 *      (String) Name
 *      (Unsigned Char) Source Type ID
 */
class PacketS2CSourceInfo extends PacketS2C {
    constructor(sourceID, name, sourceTypeID) {
        super();

        this._buffer.writeUInt8(sourceID);
        this._buffer.writeStringNT(name);
        this._buffer.writeUInt8(sourceTypeID);
    }

    getID() {
        return 0x06;
    }
}

module.exports = PacketS2CSourceInfo;
