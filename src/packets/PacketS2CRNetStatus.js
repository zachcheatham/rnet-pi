const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x02
 * rNet Status
 * Sends the rNet Connection Status
 */
class PacketS2CRNetStatus extends PacketS2C {
    constructor(status) {
        super();

        this._buffer.writeUInt8(status ? 0x01 : 0x00);
    }

    getID() {
        return 0x02;
    }
}

module.exports = PacketS2CRNetStatus;
