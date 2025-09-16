const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x0A
 * Zone Source
 * Sends a zone's current volume
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unisgned Char) Zone ID
 *      (Unsigned Char) Volume
 */
class PacketS2CZoneSource extends PacketS2C {
    constructor(ctrlrID, zoneID, sourceID) {
        super();

        this._buffer.writeUInt8(ctrlrID);
        this._buffer.writeUInt8(zoneID);
        this._buffer.writeUInt8(sourceID);
    }

    getID() {
        return 0x0A;
    }
}

module.exports = PacketS2CZoneSource;
