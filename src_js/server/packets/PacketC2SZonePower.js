const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x08
 * Zone Power
 * Turns a Zone On/Off
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 *      (Unsigned Char) On/Off State
 */
class PacketC2SZonePower extends PacketC2S {
    parseData() {
        this._ctrllrID = this._buffer.readUInt8();
        this._zoneID = this._buffer.readUInt8();
        this._power = (this._buffer.readUInt8() == 1);
    }

    getControllerID() {
        return this._ctrllrID;
    }

    getZoneID() {
        return this._zoneID;
    }

    getPowered() {
        return this._power;
    }

    getID() {
        return PacketC2SZonePower.ID;
    }
}

PacketC2SZonePower.ID = 0x08;

module.exports = PacketC2SZonePower;
