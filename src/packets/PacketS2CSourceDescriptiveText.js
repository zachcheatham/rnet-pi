const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x35
 * Source Descriptive Text
 * Data:
 *      (Unsigned Char) SourceID
 *      (Short) Time
 *      (String) Text
 */
class PacketS2CSourceDescriptiveText extends PacketS2C {
    constructor(sourceID, time, text) {
        super();

        this._buffer.writeUInt8(sourceID);
        this._buffer.writeUInt16LE(time);
        this._buffer.writeStringNT(text);
    }

    getID() {
        return 0x35;
    }
}

module.exports = PacketS2CSourceDescriptiveText;
