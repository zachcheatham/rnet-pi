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
        this._sourceID = this._buffer.readUInt8();
        this._operation = this._buffer.readUInt8();
    }

    getSourceID() {
        return this._sourceID;
    }

    getOperation() {
        return this._operation;
    }

    getID() {
        return PacketC2SSourceControl.ID;
    }
}

PacketC2SSourceControl.ID = 0x32;

module.exports = PacketC2SSourceControl;
