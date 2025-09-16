const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x05
 * Zone Deleted
 * Informs client of a deleted zone
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 */
class PacketS2CZoneDeleted extends PacketS2C {
    constructor(ctrlrID, zoneID) {
        super();

        this._buffer.writeUInt8(ctrlrID);
        this._buffer.writeUInt8(zoneID);
    }

    getID() {
        return 0x05;
    }
}

module.exports = PacketS2CZoneDeleted;
