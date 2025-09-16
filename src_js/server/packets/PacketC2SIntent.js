const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x01
 * Intent
 * Let's us know what the client plans on doing
 */
class PacketC2SIntent extends PacketC2S {
    parseData() {
        this._intent = this._buffer.readUInt8();
    }

    getIntent() {
        return this._intent;
    }

    getID() {
        return PacketC2SIntent.ID;
    }
}

PacketC2SIntent.ID = 0x01;

module.exports = PacketC2SIntent;
