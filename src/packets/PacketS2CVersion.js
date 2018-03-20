const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x7F
 * Version
 * Let's the client know the current version
 */
class PacketS2CVersion extends PacketS2C {
    constructor(version) {
        super();
        this._buffer.writeStringNT(version);
    }

    getID() {
        return 0x7F;
    }
}

module.exports = PacketS2CVersion;
