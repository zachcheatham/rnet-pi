const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x06
 * Zone Name
 * Sends sources' names
 * Data:
 *      (Unsigned Char) SourceID
 *      (String) Name
 */
class PacketS2CSourceName extends PacketS2C {
    constructor(sourceID, name) {
        super();

        this._buffer.writeUInt8(sourceID);
        this._buffer.writeStringNT(name);
    }

    getID() {
        return 0x06;
    }
}

module.exports = PacketS2CSourceName;
