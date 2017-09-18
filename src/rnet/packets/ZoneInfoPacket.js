const DataPacket = require("./DataPacket");

class ZoneInfoPacket extends DataPacket {
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

    getSourceID() {
        return this.data.readUInt8(1);
    }

    getVolume() {
        return this.data.readUInt8(2) * 2;
    }

    getBassLevel() {
        return this.data.readUInt8(3) - 10;
    }

    getTrebleLevel() {
        return this.data.readUInt8(4) - 10;
    }

    getLoudness() {
        return this.data.readUInt8(5) == 1;
    }

    getBalance() {
        return this.data.readUInt8(6) - 10;
    }

    getPartyMode() {
        return this.data.readUInt8(8);
    }

    getDoNotDisturbMode() {
        return this.data.readUInt8(9);
    }
}

ZoneInfoPacket.fromPacket = function(dataPacket) {
    if (dataPacket instanceof DataPacket) {
        const zoneInfoPacket = new ZoneInfoPacket();
        dataPacket.copyToPacket(zoneInfoPacket);
        return zoneInfoPacket;
    }
    else {
        throw new TypeError("Cannot create ZoneInfoPacket with anything other than a DataPacket");
    }
}

module.exports = ZoneInfoPacket;
