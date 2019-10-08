const PacketS2C = require("./PacketS2C");

/**
 * Server -> Client
 * ID = 0x36
 * Media metadata
 * Sends info about the current playing track to clients
 * Data:
 *      (Unsigned Char) SourceID
 *      (Short) Time
 *      (String) Text
 */
class PacketS2CMediaMetadata extends PacketS2C {
    constructor(sourceID, title, artist, artworkURL) {
        super();

        this._buffer.writeUInt8(sourceID);
        this._buffer.writeStringNT(title ? title : "");
        this._buffer.writeStringNT(artist ? artist : "");
        this._buffer.writeStringNT(artworkURL ? artworkURL : "");
    }

    getID() {
        return 0x36;
    }
}

module.exports = PacketS2CMediaMetadata;
