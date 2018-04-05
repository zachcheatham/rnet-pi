const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x32
 * Source Control
 * Control source media
 * Data:
 *      (Unsigned Char) Source ID
 *      (Unsigned Char) Button ID
 */
class PacketC2SSourceControl extends PacketC2S {
    parseData() {
        this._ctrllrID = this._buffer.readUInt8();
        this._zoneID = this._buffer.readUInt8();
        this._keyID = this._buffer.readUInt8();
    }

    getSourceID() {
        return this._sourceID;
    }

    getKeyID() {
        return this._keyID;
    }

    getID() {
        return PacketC2SSourceControl.ID;
    }
}

PacketC2SSourceControl.ID = 0x32;

module.exports = PacketC2SSourceControl;
