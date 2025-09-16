const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x37
 * Media play state
 * Sends media play state of a source
 * Data:
 *      (Unsigned Char) SourceID
 *      (Unsigned Char) Playing / Paused
 */
class PacketS2CMediaPlayState extends PacketS2C {
    constructor(sourceID, isPlaying) {
        super();

        this._buffer.writeUInt8(sourceID);
        this._buffer.writeUInt8(isPlaying ? 0x01 : 0x00);
    }

    getID() {
        return 0x37;
    }
}

module.exports = PacketS2CMediaPlayState;
