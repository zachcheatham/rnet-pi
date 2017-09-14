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
            this._buffer.writeUInt8(value);
        }
    }

    getID() {
        return 0x0B;
    }
}

module.exports = PacketS2CZoneParameter;
