const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x0A
 * Zone Source
 * Sets a zone's source
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 *      (Unsigned Char) Source ID
 */
class PacketC2SZoneSource extends PacketC2S {
    parseData() {
        this._ctrllrID = this._buffer.readUInt8();
        this._zoneID = this._buffer.readUInt8();
        this._sourceID = this._buffer.readUInt8();
    }

    getControllerID() {
        return this._ctrllrID;
    }

    getZoneID() {
        return this._zoneID;
    }

    getSourceID() {
        return this._sourceID;
    }

    getID() {
        return PacketC2SZoneSource.ID;
    }
}

PacketC2SZoneSource.ID = 0x0A;

module.exports = PacketC2SZoneSource;
