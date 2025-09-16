const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x64
 * Zone Max Volume
 * Sets a zone's max volume
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 *      (Unsigned Char) Max Volume
 */
class PacketC2SZoneMaxVolume extends PacketC2S {
    parseData() {
        this._ctrllrID = this._buffer.readUInt8();
        this._zoneID = this._buffer.readUInt8();
        this._maxVolume = this._buffer.readUInt8();
    }

    getControllerID() {
        return this._ctrllrID;
    }

    getZoneID() {
        return this._zoneID;
    }

    getMaxVolume() {
        return this._maxVolume;
    }

    getID() {
        return PacketC2SZoneMaxVolume.ID;
    }
}

PacketC2SZoneMaxVolume.ID = 0x64;

module.exports = PacketC2SZoneMaxVolume;
