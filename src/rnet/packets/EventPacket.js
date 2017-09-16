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
        this.writeWithInvertUInt16LE(buffer, this.getEventID());
        this.writeWithInvertUInt16LE(buffer, this.getEventTimestamp());
        this.writeWithInvertUInt16LE(buffer, this.getEventData());
        this.writeWithInvertUInt8(buffer, this.getEventPriority());
    }
}
