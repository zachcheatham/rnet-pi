const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x09
 * Zone Volume
 * Sends a zone's current volume
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unisgned Char) Zone ID
 *      (Unsigned Char) Volume
 */
class PacketS2CZoneVolume extends PacketS2C {
    constructor(ctrlrID, zoneID, volume) {
        super();

        this._buffer.writeUInt8(ctrlrID);
        this._buffer.writeUInt8(zoneID);
        this._buffer.writeUInt8(volume);
    }

    getID() {
        return 0x09;
    }
}

module.exports = PacketS2CZoneVolume;
