const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x65
 * Zone Mute
 * Sends a zone's mute state
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unisgned Char) Zone ID
 *      (Unsigned Char) Muting
 */
class PacketS2CZoneMute extends PacketS2C {
    constructor(ctrlrID, zoneID, muted) {
        super();

        this._buffer.writeUInt8(ctrlrID);
        this._buffer.writeUInt8(zoneID);
        this._buffer.writeUInt8(muted ? 0x01 : 0x00);
    }

    getID() {
        return 0x65;
    }
}

module.exports = PacketS2CZoneMute;
