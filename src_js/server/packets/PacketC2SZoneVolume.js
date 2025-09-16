const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x09
 * Zone Volume
 * Sets a zone's volume
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 *      (Unsigned Char) Volume
 */
class PacketC2SZoneVolume extends PacketC2S {
    parseData() {
        this._ctrllrID = this._buffer.readUInt8();
        this._zoneID = this._buffer.readUInt8();
        this._volume = this._buffer.readUInt8();
    }

    getControllerID() {
        return this._ctrllrID;
    }

    getZoneID() {
        return this._zoneID;
    }

    getVolume() {
        return this._volume;
    }

    getID() {
        return PacketC2SZoneVolume.ID;
    }
}

PacketC2SZoneVolume.ID = 0x09;

module.exports = PacketC2SZoneVolume;
