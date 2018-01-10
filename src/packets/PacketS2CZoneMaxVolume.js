const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x64
 * Zone Max Volume
 * Informs client of a zone's max volume
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 *      (Unsigned Char) Volume
 */
class PacketS2CZoneMaxVolume extends PacketS2C {
    constructor(ctrlrID, zoneID, maxVolume) {
        super();
        this._buffer.writeUInt8(ctrllrID);
        this._buffer.writeUInt8(zoneID);
        this._buffer.writeUInt8(maxVolume);
    }

    getID() {
        return 0x64;
    }
}

module.exports = PacketS2CZoneMaxVolume;
