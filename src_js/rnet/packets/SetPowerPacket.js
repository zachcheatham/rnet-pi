const EventPacket = require("./EventPacket");

class SetPowerPacket extends EventPacket {
    constructor(controllerID, zoneID, power) {
        super();

        this.targetPath = [0x02, 0x00];
        this.targetControllerID = controllerID;
        this.eventID = 0xDC;
        this.eventData = zoneID;
        this.eventTimestamp = power === true ? 1 : 0;
        this.eventPriority = 1;
    }

    getControllerID() {
        return this.targetControllerID;
    }

    getZoneID() {
        return this.eventData;
    }

    getPower() {
        return this.eventTimestamp == 1;
    }
}

SetPowerPacket.fromPacket = function(eventpacket) {
    if (eventPacket instanceof EventPacket) {
        const powerPacket = new SetPowerPacket();
        eventPacket.copyToPacket(powerPacket);
        return powerPacket;
    }
    else {
        throw new TypeError("Cannot create SetPowerPacket from anything other than an EventPacket");
    }
}

module.exports = SetPowerPacket;
