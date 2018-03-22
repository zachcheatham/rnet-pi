const PacketC2SAllPower = require("./PacketC2SAllPower");
const PacketC2SDeleteSource = require("./PacketC2SDeleteSource");
const PacketC2SDeleteZone = require("./PacketC2SDeleteZone");
const PacketC2SDisconnect = require("./PacketC2SDisconnect");
const PacketC2SUpdate = require("./PacketC2SUpdate");
const PacketC2SIntent = require("./PacketC2SIntent");
const PacketC2SProperty = require("./PacketC2SProperty");
const PacketC2SMute = require("./PacketC2SMute");
const PacketC2SSourceName = require("./PacketC2SSourceName");
const PacketC2SZoneName = require("./PacketC2SZoneName");
const PacketC2SZoneParameter = require("./PacketC2SZoneParameter");
const PacketC2SZonePower = require("./PacketC2SZonePower");
const PacketC2SZoneSource = require("./PacketC2SZoneSource");
const PacketC2SZoneMaxVolume = require("./PacketC2SZoneMaxVolume");
const PacketC2SZoneVolume = require("./PacketC2SZoneVolume");

module.exports = function(packetType, data) {
    switch (packetType) {
        case 0x01:
            return new PacketC2SIntent(data);
        case 0x02:
            return new PacketC2SProperty(data);
        case 0x03:
            return new PacketC2SDisconnect(data);
        case 0x04:
            return new PacketC2SZoneName(data);
        case 0x05:
            return new PacketC2SDeleteZone(data);
        case 0x06:
            return new PacketC2SSourceName(data);
        case 0x07:
            return new PacketC2SDeleteSource(data);
        case 0x08:
            return new PacketC2SZonePower(data);
        case 0x09:
            return new PacketC2SZoneVolume(data);
        case 0x0A:
            return new PacketC2SZoneSource(data);
        case 0x0B:
            return new PacketC2SZoneParameter(data);
        case 0x0C:
            return new PacketC2SAllPower(data);
        case 0x0D:
            return new PacketC2SMute(data);
        case 0x64:
            return new PacketC2SZoneMaxVolume(data);
        case 0x7D:
            return new PacketC2SUpdate(data);
        default:
            return undefined;
    }
}
