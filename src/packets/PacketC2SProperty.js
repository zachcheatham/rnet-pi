const PacketC2S = require("./PacketC2S");
const Property = require("../Property");

/**
 * Client -> Server
 * ID = 0x34
 * Property
 */
class PacketC2SProperty extends PacketC2S {
    parseData() {
        this._property = this._buffer.readUInt8();
        switch(this._property) {
            case Property.PROPERTY_WEB_SERVER_ENABLED:
                break;
            case Property.PROPERTY_NAME:
                this._value = this._buffer.readStringNT();
                break;
        }
    }

    getProperty() {
        return this._property;
    }

    getValue() {
        return this._value;
    }

    getID() {
        return PacketC2SProperty.ID;
    }
}

PacketC2SProperty.ID = 0x34;

module.exports = PacketC2SProperty;
