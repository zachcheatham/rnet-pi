const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x04
 * Zone Name
 * Sends a zone's name
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unisgned Char) Zone ID
 *      (String) Name
 */
class PacketS2CZoneName extends PacketS2C {
    constructor(ctrlrID, zoneID, name) {
        super();

        this._buffer.writeUInt8(ctrlrID);
        this._buffer.writeUInt8(zoneID);
        this._buffer.writeStringNT(name);
    }

    getID() {
        return 0x04;
    }
}

module.exports = PacketS2CZoneName;
