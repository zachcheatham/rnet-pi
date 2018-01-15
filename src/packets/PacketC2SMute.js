const PacketC2S = require("./PacketC2S");

/**
 * Client -> Server
 * ID = 0x0D
 * Mute
 * Data:
 *      (Unsigned Char) Mute state
 *      (Unsigned Short) Fade Time
 *      (Optional) (Unsigned Char) Controller ID
 *      (Optional) (Unsigned Char) Zone ID
 */
class PacketC2SMute extends PacketC2S {
    parseData() {
        this._muteState = this._buffer.readUInt8();
        this._fadeTime = this._buffer.readUInt16LE();
        this._controllerID = false;
        this._zoneID = false;

        if (this._buffer.remaining() > 1) {
            this._controllerID = this._buffer.readUInt8();
            this._zoneID = this._buffer.readUInt8();
        }
    }

    getMuteState() {
        return this._muteState;
    }

    getFadeTime() {
        return this._fadeTime;
    }

    getControllerID() {
        return this._controllerID;
    }

    getZoneID() {
        return this._zoneID;
    }

    getID() {
        return PacketC2SMute.ID;
    }
}

PacketC2SMute.ID = 0x0D;
PacketC2SMute.MUTE_OFF = 0x00;
PacketC2SMute.MUTE_ON = 0x01;
PacketC2SMute.MUTE_TOGGLE = 0x02;

module.exports = PacketC2SMute;
