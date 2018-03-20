const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x7D
 * Update
 * Request to start an update
 */
class PacketC2SUpdate extends PacketC2S {
    parseData() {}

    getID() {
        return PacketC2SUpdate.ID;
    }
}

PacketC2SUpdate.ID = 0x7D;

module.exports = PacketC2SUpdate;
