const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x01
 * Zone Name
 * Renames a zone
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 *      (NT String) New Zone Name
 */
class PacketC2SZoneName extends PacketC2S {
    parseData() {
        this._ctrllrID = this._buffer.readUInt8();
        this._zoneID = this._buffer.readUInt8();
        this._name = this._buffer.readStringNT();
    }

    getControllerID() {
        return this._ctrllrID;
    }

    getZoneID() {
        return this._zoneID;
    }

    getName() {
        return this._name;
    }

    getID() {
        return PacketC2SZoneName.ID;
    }
}

PacketC2SZoneName.ID = 0x01;

module.exports = PacketC2SZoneName;
