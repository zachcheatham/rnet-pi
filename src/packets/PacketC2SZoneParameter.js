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
        this.ctrllrID = this._buffer.readUInt8();
        this.zoneID = this._buffer.readUInt8();
        this.parameterID = this._buffer.readUInt8();

        if (parameterIsSigned(parameterID)) {
            this.value = this._buffer.readInt8();
        }
        else {
            this.value = this._buffer.readUInt8();
        }
    }
}

module.exports = PacketC2SParameter;
