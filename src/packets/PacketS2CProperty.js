const PacketS2C = require("./PacketS2C");
const Property = require("./Property");

/**
 * Server -> Client
 * ID = 0x02
 * Property
 * Sends the client a controller property value
 */
class PacketS2CProperty extends PacketS2C {
    constructor(property, value) {
        super();

        this._buffer.writeUInt8(property);
        switch (property)
        {
        case Property.PROPERTY_SERIAL_CONNECTED:
        case Property.PROPERTY_WEB_SERVER_ENABLED:
            this._buffer.writeUInt8(value === true ? 1 : 0);
            break;
        case Property.PROPERTY_NAME:
        case Property.PROPERTY_VERSION:
            this._buffer.writeStringNT(value);
            break;
        }
    }

    getID() {
        return 0x02;
    }
}

module.exports = PacketS2CProperty;
