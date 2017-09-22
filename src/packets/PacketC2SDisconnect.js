const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x03
 * Disconnect
 * Informs the server of an official disconnect (Not sure if I need this yet.)
 */
class PacketC2SDisconnect extends PacketC2S {
    parseData() {}

    getID() {
        return PacketC2SDisconnect.ID;
    }
}

PacketC2SDisconnect.ID = 0x03;

module.exports = PacketC2SDisconnect;
