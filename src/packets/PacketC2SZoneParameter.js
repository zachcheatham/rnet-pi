const PacketC2S = require("./PacketC2S");
const parameterIsSigned = require("./parameterIsSigned");

/**
 * Client -> Server
 * ID = 0x0B
 * Zone Parameter
 * Sets extra parameters in a zone
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unsigned Char) Zone ID
 *      (Unsigned Char) Parameter ID
 *      (Unsigned/Signed Char) Parameter Value
 */
class PacketC2SParameter extends PacketC2S {
    parseData() {
        this._ctrllrID = this._buffer.readUInt8();
        this._zoneID = this._buffer.readUInt8();
        this._parameterID = this._buffer.readUInt8();

        if (parameterIsSigned(this._parameterID)) {
            this._value = this._buffer.readInt8();
        }
        else {
            switch (this._parameterID)
            {
                case 2: // Loudness
                case 6: // Do Not Disturb
                case 8: // Front A/V Enable
                    this._value = (this._buffer.readUInt8() == 1);
                    console.log(this._value);
                    break;
                default:
                    this._value = this._buffer.readUInt8();
                    break;
            }
        }
    }

    getControllerID() {
        return this._ctrllrID;
    }

    getZoneID() {
        return this._zoneID;
    }

    getParameterID() {
        return this._parameterID;
    }

    getParameterValue() {
        return this._value;
    }

    getID() {
        return PacketC2SParameter.ID;
    }
}

PacketC2SParameter.ID = 0x0B;

module.exports = PacketC2SParameter;
