const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x08
 * Zone Power
 * Sends a zone's power status
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unisgned Char) Zone ID
 *      (Unsigned Char) On/Off
 */
class PacketS2CZonePower extends PacketS2C {
    constructor(ctrlrID, zoneID, power) {
        super();

        this._buffer.writeUInt8(ctrlrID);
        this._buffer.writeUInt8(zoneID);
        this._buffer.writeUInt8(power ? 0x01 : 0x00);
    }

    getID() {
        return 0x08;
    }
}

module.exports = PacketS2CZonePower;
