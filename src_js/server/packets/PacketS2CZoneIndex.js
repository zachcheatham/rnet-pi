const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x03
 * Zone Index
 * Informs client of all the existing zones when it connects
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 */
class PacketS2CZoneIndex extends PacketS2C {
    constructor(zones) {
        super();
        for (let i = 0; i < zones.length; i++) {
            this._buffer.writeUInt8(zones[i][0]);
            this._buffer.writeUInt8(zones[i][1]);
        }
    }

    getID() {
        return 0x03;
    }
}

module.exports = PacketS2CZoneIndex;
