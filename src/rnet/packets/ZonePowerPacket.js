const DataPacket = require("./DataPacket");

class ZonePowerPacket extends DataPacket {
    requiresHandshake() {
        return true;
    }

    getControllerID() {
        return this.sourceControllerID;
    }

    getZoneID() {
        return this.sourcePath[2];
    }

    getPower() {
        return this.data.readUInt8(0) == 1;
    }
}

ZonePowerPacket.fromPacket = function(dataPacket) {
    if (dataPacket instanceof DataPacket) {
        const zonePowerPacket = new ZonePowerPacket();
        dataPacket.copyToPacket(zonePowerPacket);
        return zonePowerPacket;
    }
    else {
        throw new TypeError("Cannot create ZonePowerPacket with anything other than a DataPacket");
    }
}

module.exports = ZonePowerPacket;
