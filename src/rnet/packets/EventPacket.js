const Packet = require("./Packet");

module.exports = class EventPacket extends Packet {
    constructor() {
        super();

        if (new.target === EventPacket) {
            throw new TypeError("Cannot initiate a EventPacket object. DataPacket is meant to be extended.");
        }

        if (this.getEventID === undefined) {
            throw new TypeError("EventPacket subclasses must implement getEventID()");
        }

        if (this.getEventTimestamp === undefined) {
            throw new TypeError("EventPacket subclasses must implement getEventTimestamp()");
        }

        if (this.getEventData === undefined) {
            throw new TypeError("EventPacket subclasses must implement getEventData()");
        }

        if (this.getEventPriority === undefined) {
            throw new TypeError("EventPacket subclasses must implement getEventPriority()");
        }
    }

    getMessageType(data) {
        return 0x05;
    }

    writePacketBody(buffer) {
        buffer.writeUInt16LE(this.getEventID());
        buffer.writeUInt16LE(this.getEventTimestamp());
        buffer.writeUInt16LE(this.getEventData());
        buffer.writeUInt8(this.getEventPriority());
    }
}
