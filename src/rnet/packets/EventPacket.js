const RNetPacket = require("./RNetPacket");
const SmartBuffer = require("smart-buffer").SmartBuffer;

class EventPacket extends RNetPacket {
    constructor() {
        super();
        this.messageType = 0x05;
        this.sourcePath = [];
    }

    getMessageBody() {
        const buffer = new SmartBuffer();

        if (this.targetPath !== undefined) {
            buffer.writeUInt8(this.targetPath.length);
            for (let i = 0; i < this.targetPath.length; i++) {
                buffer.writeUInt8(this.targetPath[i])
            }
        }
        else {
            throw new Error("this.targetPath was not set")
        }

        buffer.writeUInt8(this.sourcePath.length);
        for (var i = 0; i < this.sourcePath.length; i++) {
            buffer.writeUInt8(this.sourcePath[i])
        }

        if (this.eventID !== undefined) {
            this.writeWithInvertUInt16LE(buffer, this.eventID);
        }
        else {
            throw new Error("this.eventID has not been set.")
        }

        if (this.eventTimestamp !== undefined) {
            this.writeWithInvertUInt16LE(buffer, this.eventTimestamp);
        }
        else {
            throw new Error("this.eventTimestamp has not been set.");
        }

        if (this.eventData !== undefined) {
            this.writeWithInvertUInt16LE(buffer, this.eventData);
        }
        else {
            throw new Error("this.eventData has not been set.")
        }

        if (this.eventPriority !== undefined) {
            this.writeWithInvertUInt8(buffer, this.eventPriority);
        }
        else {
            throw new Error("this.eventPriority has not been set.");
        }

        return buffer.toBuffer();
    }

    copyToPacket(packet) {
        if (packet.messageType == this.messageType) {
            if (packet.eventID == this.eventID) {
                packet.targetControllerID = this.targetControllerID;
                packet.targetZoneID = this.targetZoneID;
                packet.targetKeypadID = this.targetKeypadID;
                packet.sourceControllerID = this.sourceControllerID;
                packet.sourceZoneID = this.sourceZoneID;
                packet.sourceKeypadID = this.sourceKeypadID;

                packet.targetPath = this.targetPath;
                packet.sourcePath = this.sourcePath;
                packet.eventID = this.eventID;
                packet.eventTimestamp = this.eventTimestamp;
                packet.eventData = this.eventData;
                packet.eventPriority = this.eventPriority;
            }
            else {
                throw new Error("Attempted to copy values to packet with different eventID");
            }
        }
        else {
            throw new Error("Attempted to copy values to packet with different messageType");
        }
    }
}

EventPacket.fromPacket = function(rNetPacket) {
    if (rNetPacket instanceof RNetPacket) {
        const eventPacket = new EventPacket();
        rNetPacket.copyToPacket(eventPacket);

        const buffer = new SmartBuffer(rNetPacket.messageBody);
        eventPacket.targetPath = [];
        {
            let length = buffer.readUInt8();
            for (let i = 0; i < length; i++) {
                eventPacket.targetPath[i] = buffer.readUInt8();
            }
        }
        eventPacket.sourcePath = [];
        {
            let length = buffer.readUInt8();
            for (let i = 0; i < length; i++) {
                eventPacket.sourcePath[i] = buffer.readUInt8();
            }
        }
        eventPacket.eventID = buffer.readUInt16LE();
        eventPacket.eventTimestamp = buffer.readUInt16LE();
        eventPacket.eventData = buffer.readUInt16LE();
        eventPacket.eventPriority = buffer.readUInt8();(0, 0, true);

        return eventPacket;
    }
    else {
        throw new TypeError("Cannot create EventPacket with anything other than RNetPacket");
    }
}

module.exports = EventPacket;
