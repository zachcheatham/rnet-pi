const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x05
 * Delete Zone
 * Deletes a Zone
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 */
class PacketC2SDeleteZone extends PacketC2S {
    parseData() {
        this._ctrllrID = this._buffer.readUInt8();
        this._zoneID = this._buffer.readUInt8();
    }

    getControllerID() {
        return this._ctrllrID;
    }

    getZoneID() {
        return this._zoneID;
    }

    getID() {
        return PacketC2SDeleteZone.ID;
    }
}

PacketC2SDeleteZone.ID = 0x05;

module.exports = PacketC2SDeleteZone;
