const PacketS2C = require("./PacketS2C");
const SourceProperty = require("../rnet/SourceProperty");

/**
 * Server -> Client
 * ID = 0x34
 * Update Available
 * Notifies the client that theres an update available
 * Data:
 *      (Unsigned Char) Update Version
 */
class PacketS2CSourceProperty extends PacketS2C {
    constructor(sourceID, propertyID, propertyValue) {
        super();
        this._buffer.writeUInt8(sourceID);
        this._buffer.writeUInt8(propertyID);
        switch (propertyID) {
            case SourceProperty.PROPERTY_AUTO_OFF:
                this._buffer.writeUInt8(propertyValue === true ? 0x01 : 0x00);
                break;
            case SourceProperty.PROPERTY_AUTO_ON_ZONES:
                for (zoneID of propertyValue) {
                    this._buffer.writeUInt8(zoneID[0]);
                    this._buffer.writeUInt8(zoneID[1]);
                }
                break;
        }
    }

    getID() {
        return 0x34;
    }
}

module.exports = PacketS2CSourceProperty;
