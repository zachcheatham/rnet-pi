const PacketC2S = require("./PacketC2S");
const SourceProperty = require("../../rnet/SourceProperty");

/**
 * Client -> Server
 * ID = 0x34
 * Source Property
 * Change source properties
 */
class PacketC2SSourceProperty extends PacketC2S {
    parseData() {
        this._sourceID = this._buffer.readUInt8();
        this._propertyID = this._buffer.readUInt8();
        switch (this._propertyID) {
        case SourceProperty.PROPERTY_AUTO_OFF:
        case SourceProperty.PROPERTY_OVERRIDE_NAME:
            this._propertyValue = this._buffer.readUInt8() == 0x01;
            break;
        case SourceProperty.PROPERTY_AUTO_ON_ZONES:
            this._propertyValue = [];
            while (this._buffer.remaining() > 0) {
                this._propertyValue.push([this._buffer.readUInt8(), this._buffer.readUInt8()]);
            }
            break;
        default:
            this._propertyValue = false;
        }
    }

    getSourceID() {
        return this._sourceID;
    }

    getPropertyID() {
        return this._propertyID;
    }

    getPropertyValue() {
        return this._propertyValue;
    }

    getID() {
        return PacketC2SSourceProperty.ID;
    }
}

PacketC2SSourceProperty.ID = 0x34;

module.exports = PacketC2SSourceProperty;
