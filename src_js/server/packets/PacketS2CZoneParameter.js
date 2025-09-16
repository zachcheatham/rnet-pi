const PacketS2C = require("./PacketS2C");
const parameterIsSigned = require("./parameterIsSigned")

/**
 * Server -> Client
 * ID = 0x0B
 * Zone Parameter
 * Sends extra parameter values
 * Data:
 *      (Unsigned Char) Controller ID
 *      (Unisgned Char) Zone ID
 *      (Unsigned Char) Paramter ID
 *      (Unsigned/Signed Char) Paramter Value
 */

class PacketS2CZoneParameter extends PacketS2C {
    constructor(ctrlrID, zoneID, paramterID, value) {
        super();

        this._buffer.writeUInt8(ctrlrID);
        this._buffer.writeUInt8(zoneID);
        this._buffer.writeUInt8(paramterID);

        if (parameterIsSigned(paramterID)) {
            this._buffer.writeInt8(value);
        }
        else {
            switch (this._parameterID)
            {
                case 2: // Loudness
                case 6: // Do Not Disturb
                case 8: // Front A/V Enable
                    this._buffer.writeUInt8(value === true ? 1 : 0);
                    break;
                default:
                    this._buffer.writeUInt8(value);
                    break;
            }
        }
    }

    getID() {
        return 0x0B;
    }
}

module.exports = PacketS2CZoneParameter;
