const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x01
 * Name request
 * Asks client for a device name. Should only be used when a new client connects.
 */
class PacketS2CName extends PacketS2C {
    getID() {
        return 0x01;
    }
}

module.exports = PacketS2CName;
