const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x7D
 * Update Available
 * Notifies the client that theres an update available
 * Data:
 *      (Unsigned Char) Update Version
 */
class PacketS2CUpdateAvailable extends PacketS2C {
    constructor(version) {
        super();
        this._buffer.writeStringNT(version);
    }

    getID() {
        return 0x7D;
    }
}

module.exports = PacketS2CUpdateAvailable;
