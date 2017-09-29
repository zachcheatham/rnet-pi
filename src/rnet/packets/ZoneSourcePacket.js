const DataPacket = require("./DataPacket");

class ZoneVolumePacket extends DataPacket {
    requiresHandshake() {
        return true;
    }

    getControllerID() {
        return this.sourceControllerID;
    }

    getZoneID() {
        return this.sourcePath[2];
    }

    getVolume() {
        return this.data.readUInt8(0) * 2;
    }
}

ZoneVolumePacket.fromPacket = function(dataPacket) {
    if (dataPacket instanceof DataPacket) {
        const zoneVolumePacket = new ZoneVolumePacket();
        dataPacket.copyToPacket(zoneVolumePacket);
        return zoneVolumePacket;
    }
    else {
        throw new TypeError("Cannot create ZoneVolumePacket with anything other than a DataPacket");
    }
}

module.exports = ZoneVolumePacket;
