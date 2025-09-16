const EventPacket = require("./EventPacket");

class SetVolumePacket extends EventPacket {
    constructor(controllerID, zoneID, volume) {
        super();

        if (volume < 0 || volume > 100) {
            throw new Error("Volume out of range (0-100) when constructing SetVolumePacket");
        }

        this.targetPath = [0x02, 0x00];
        this.targetControllerID = controllerID;
        this.eventID = 0xDE;
        this.eventTimestamp = Math.floor(volume / 2);  // Translate our range 0-100 to 0-50
        this.eventData = zoneID;
        this.eventPriority = 1;

    }

    getControllerID() {
        return this.targetControllerID;
    }

    getZoneID() {
        return this.eventData;
    }

    getVolume() {
        return this.eventTimestamp * 2;
    }
}

SetVolumePacket.fromPacket = function(eventpacket) {
    if (eventPacket instanceof EventPacket) {
        const powerPacket = new SetPowerPacket();
        eventPacket.copyToPacket(powerPacket);
        return powerPacket;
    }
    else {
        throw new TypeError("Cannot create SetPowerPacket from anything other than an EventPacket");
    }
}

module.exports = SetVolumePacket;
