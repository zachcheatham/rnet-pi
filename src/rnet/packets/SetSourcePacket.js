const EventPacket = require("./EventPacket");

class SetSourcePacket extends EventPacket {
    constructor(controllerID, zoneID, sourceID) {
        super();

        this.targetPath = [0x00, 0x00];
        this.targetControllerID = controllerID;
        this.sourceZoneID = zoneID;
        this.eventID = 0xC1;
        this.eventData = sourceID;
        this.eventTimestamp = 0;
        this.eventPriority = 1;
    }

    getControllerID() {
        return this.targetControllerID;
    }

    getZoneID() {
        return this.sourceZoneID;
    }

    getSourceID() {
        return this.eventData;
    }
}

SetSourcePacket.fromPacket = function(eventpacket) {
    if (eventPacket instanceof EventPacket) {
        const sourcePacket = new SetSourcePacket();
        eventPacket.copyToPacket(sourcePacket);
        return sourcePacket;
    }
    else {
        throw new TypeError("Cannot create SetSourcePacket from anything other than an EventPacket");
    }
}

module.exports = SetSourcePacket;
