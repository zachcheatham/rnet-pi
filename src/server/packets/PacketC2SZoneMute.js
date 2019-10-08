const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x65
 * Zone Mute
 * Mute / Unmute a Zone
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 *      (Unsigned Char) Muting
 */
class PacketC2SZoneMute extends PacketC2S {
    parseData() {
        this._ctrllrID = this._buffer.readUInt8();
        this._zoneID = this._buffer.readUInt8();
        this._muted = (this._buffer.readUInt8() == 1);
    }

    getControllerID() {
        return this._ctrllrID;
    }

    getZoneID() {
        return this._zoneID;
    }

    getMuted() {
        return this._muted;
    }

    getID() {
        return PacketC2SZoneMute.ID;
    }
}

PacketC2SZoneMute.ID = 0x65;

module.exports = PacketC2SZoneMute;
