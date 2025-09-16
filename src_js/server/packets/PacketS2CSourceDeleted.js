const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x07
 * Source Deleted
 * Informs client of a deleted source
 * Data:
 *      (Unsigned Char) Source ID
 */
class PacketS2CSourceDeleted extends PacketS2C {
    constructor(ctrlrID, zoneID) {
        super();

        this._buffer.writeUInt8(ctrlrID);
        this._buffer.writeUInt8(zoneID);
    }

    getID() {
        return 0x07;
    }
}

module.exports = PacketS2CSourceDeleted;
