const Server = require("./server");
const RNet = require("./rnet/rnet");
const config = require("./configuration");

const PacketC2SAllPower = require("./packets/PacketC2SAllPower");
const PacketC2SDeleteSource = require("./packets/PacketC2SDeleteSource");
const PacketC2SDeleteZone = require("./packets/PacketC2SDeleteZone");
const PacketC2SSourceName = require("./packets/PacketC2SSourceName");
const PacketC2SZoneName = require("./packets/PacketC2SZoneName");
const PacketC2SZoneParameter = require("./packets/PacketC2SZoneParameter");
const PacketC2SZonePower = require("./packets/PacketC2SZonePower");
const PacketC2SZoneSource = require("./packets/PacketC2SZoneSource");
const PacketC2SZoneVolume = require("./packets/PacketC2SZoneVolume");
const PacketS2CRNetStatus = require("./packets/PacketS2CRNetStatus");
const PacketC2SMute = require("./packets/PacketC2SMute");
const PacketS2CSourceName = require("./packets/PacketS2CSourceName");
const PacketS2CSourceDeleted = require("./packets/PacketS2CSourceDeleted");
const PacketS2CZoneName = require("./packets/PacketS2CZoneName");
const PacketS2CZoneDeleted = require("./packets/PacketS2CZoneDeleted");
const PacketS2CZoneIndex = require("./packets/PacketS2CZoneIndex");
const PacketS2CZoneParameter = require("./packets/PacketS2CZoneParameter");
const PacketS2CZonePower = require("./packets/PacketS2CZonePower");
const PacketS2CZoneSource = require("./packets/PacketS2CZoneSource");
const PacketS2CZoneVolume = require("./packets/PacketS2CZoneVolume");
const parameterIDToString = require("./rnet/parameterIDToString");

// Modify console.log to include timestamps
require("console-stamp")(console, "HH:MM:ss");

config.read();
config.write();

const server = new Server(config.get("serverName"), config.get("serverHost"), config.get("serverPort"));
const rNet = new RNet(config.get("serialDevice"));

// Setup server
server.once("start", function() {
    console.info("Server listening on %s", server.getAddress());
    console.log("Connecting to RNet...")
    rNet.connect();
})
.once("error", function(error) {
    console.error("Server error: %s", error.message);
    process.exit(1);
})
.on("client_connected", function(client) {
    console.log("Client %s connected.", client.getAddress());

    let zones = [];
    for (let ctrllrID = 0; ctrllrID < rNet.getControllersSize(); ctrllrID++) {
        for (let zoneID = 0; zoneID < rNet.getZonesSize(ctrllrID); zoneID++) {
            if (rNet.getZone(ctrllrID, zoneID) != null) {
                zones.push([ctrllrID, zoneID]);
            }
        }
    }
    client.send(new PacketS2CZoneIndex(zones))

    client.send(new PacketS2CRNetStatus(rNet.isConnected()));

    for (let sourceID = 0; sourceID < rNet.getSourcesSize(); sourceID++) {
        if (rNet.sourceExists(sourceID)) {
            client.send(new PacketS2CSourceName(sourceID, rNet.getSourceName(sourceID)));
        }
    }

    for (let ctrllrID = 0; ctrllrID < rNet.getControllersSize(); ctrllrID++) {
        for (let zoneID = 0; zoneID < rNet.getZonesSize(ctrllrID); zoneID++) {
            const zone = rNet.getZone(ctrllrID, zoneID);

            if (zone != null) {
                client.send(new PacketS2CZoneName(ctrllrID, zoneID, zone.getName()));
                client.send(new PacketS2CZonePower(ctrllrID, zoneID, zone.getPower()));
                client.send(new PacketS2CZoneVolume(ctrllrID, zoneID, zone.getVolume()));
                client.send(new PacketS2CZoneSource(ctrllrID, zoneID, zone.getSourceID()));
                for (let i = 0; i < 9; i++) {
                    client.send(new PacketS2CZoneParameter(ctrllrID, zoneID, i, zone.getParameter(i)));
                }
            }
        }
    }

    if (server.getClientCount() == 0) { // First client connected
        rNet.requestAllZoneInfo();
    }
    rNet.setAutoUpdate(true);
})
.on("client_disconnect", function(client) {
    console.log("Client %s disconnected.", client.getAddress());

    if (server.getClientCount() - 1 == 0) {
        rNet.setAutoUpdate(false);
    }
})
.on("packet", function(client, packet) {
    switch (packet.getID())
    {
        case PacketC2SAllPower.ID:
        {
            rNet.setAllPower(packet.getPowered());
            break;
        }
        case PacketC2SDeleteZone.ID:
        {
            rNet.deleteZone(packet.getControllerID(), packet.getZoneID());
            break;
        }
        case PacketC2SDeleteSource.ID:
        {
            rNet.deleteSource(packet.getSourceID());
            break;
        }
        case PacketC2SSourceName.ID:
        {
            if (rNet.sourceExists(packet.getSourceID()))
                rNet.renameSource(packet.getSourceID(), packet.getName());
            else
                rNet.createSource(packet.getSourceID(), packet.getName());
            break;
        }
        case PacketC2SZoneName.ID:
        {
            const zone = rNet.getZone(packet.getControllerID(), packet.getZoneID());
            if (zone)
                zone.setName(packet.getName());
            else
                rNet.createZone(packet.getControllerID(), packet.getZoneID(), packet.getName());
            break;
        }
        case PacketC2SZoneParameter.ID:
        {
            const zone = rNet.getZone(packet.getControllerID(), packet.getZoneID());
            if (zone != null)
                zone.setParameter(packet.getParameterID(), packet.getParameterValue());
            else
                console.warn("Received request to set parameter of unknown zone %d-%d", packet.getControllerID(), packet.getZoneID());
            break;
        }
        case PacketC2SZonePower.ID:
        {
            const zone = rNet.getZone(packet.getControllerID(), packet.getZoneID());
            if (zone != null)
                zone.setPower(packet.getPowered());
            else
                console.warn("Received request to set power of unknown zone %d-%d", packet.getControllerID(), packet.getZoneID());
            break;
        }
        case PacketC2SZoneSource.ID:
        {
            const zone = rNet.getZone(packet.getControllerID(), packet.getZoneID());
            if (zone != null)
                zone.setSourceID(packet.getSourceID());
            else
                console.warn("Received request to set source of unknown zone %d-%d", packet.getControllerID(), packet.getZoneID());
            break;
        }
        case PacketC2SZoneVolume.ID:
        {
            const zone = rNet.getZone(packet.getControllerID(), packet.getZoneID());
            if (zone != null)
                zone.setVolume(packet.getVolume());
            else
                console.warn("Received request to set volume of unknown zone %d-%d", packet.getControllerID(), packet.getZoneID());
            break;
        }
        case PacketC2SMute.ID:
        {
            if (!packet.getControllerID()) {
                if (packet.getMuteState() == PacketC2SMute.MUTE_TOGGLE) {
                    rNet.setAllMute(!rNet.getAllMute(), packet.getFadeTime());
                }
                else {
                    rNet.setAllMute(packet.getMuteState == 0x01, packet.getFadeTime());
                }
            }
            else {
                const zone = rNet.getZone(packet.getControllerID(), packet.getZoneID());
                if (zone != null) {
                    if (packet.getMuteState() == PacketC2SMute.MUTE_TOGGLE) {
                        zone.setMute(!zone.getMuted(), packet.getFadeTime());
                    }
                    else {
                        zone.setMute(zone.getMuted() == 0x01, packet.getFadeTime());
                    }
                }
                else
                    console.warn("Received request to set mute of unknown zone %d-%d", packet.getControllerID(), packet.getZoneID());
            }
        }
    }
});

rNet.on("connected", () => {
    server.broadcast(new PacketS2CRNetStatus(true));
    console.log("Connected to RNet!");
})
.on("disconnected", () => {
    server.broadcast(new PacketS2CRNetStatus(false));
    console.log("Disconnected from RNet")
})
.on("error", (error) => {
    console.error("RNet Error: %s", error.message);
    process.exit(2);
})
.on("new-zone", (zone) => {
    server.broadcast(new PacketS2CZoneName(zone.getControllerID(), zone.getZoneID(), zone.getName()));
    console.info(
        "Controller #%d zone #%d (%s) created.",
        zone.getControllerID(),
        zone.getZoneID(),
        zone.getName()
    );
})
.on("zone-name", (zone, name) => {
    server.broadcast(new PacketS2CZoneName(zone.getControllerID(), zone.getZoneID(), name));
    console.info(
        "Controller #%d zone #%d renamed to %s.",
        zone.getControllerID(),
        zone.getZoneID(),
        name
    );
})
.on("zone-deleted", (ctrllrID, zoneID) => {
    server.broadcast(new PacketS2CZoneDeleted(ctrllrID, zoneID));
    console.info(
        "Controller #%d zone #%d deleted.",
        ctrllrID,
        zoneID
    );
})
.on("new-source", (sourceID) => {
    server.broadcast(new PacketS2CSourceName(sourceID, rNet.getSourceName(sourceID)));
    console.info(
        "Source #%d (%s) created.",
        sourceID,
        rNet.getSourceName(sourceID)
    );
})
.on("source-name", (sourceID) => {
    server.broadcast(new PacketS2CSourceName(sourceID, rNet.getSourceName(sourceID)));
    console.info(
        "Source #%d renamed to %s.",
        sourceID,
        rNet.getSourceName(sourceID)
    );
})
.on("source-deleted", (sourceID) => {
    server.broadcast(new PacketS2CSourceDeleted(sourceID));
    console.info(
        "Source #%d deleted.",
        sourceID
    );
})
.on("power", (zone, powered) => {
    server.broadcast(new PacketS2CZonePower(zone.getControllerID(), zone.getZoneID(), powered));
    console.info(
        "Controller #%d zone #%d (%s) power set to %s",
        zone.getControllerID(),
        zone.getZoneID(),
        zone.getName(),
        powered
    );
})
.on("volume", (zone, volume) => {
    server.broadcast(new PacketS2CZoneVolume(zone.getControllerID(), zone.getZoneID(), volume));
    console.info(
        "Controller #%d zone #%d (%s) volume set to %d",
        zone.getControllerID(),
        zone.getZoneID(),
        zone.getName(),
        volume
    );
})
.on("source", (zone, sourceID) => {
    server.broadcast(new PacketS2CZoneSource(zone.getControllerID(), zone.getZoneID(), sourceID));
    console.info(
        "Controller #%d zone #%d (%s) source set to #%d (%s)",
        zone.getControllerID(),
        zone.getZoneID(),
        zone.getName(),
        sourceID,
        rNet.getSourceName(sourceID)
    );
})
.on("parameter", (zone, parameterID, value) => {
    server.broadcast(new PacketS2CZoneParameter(zone.getControllerID(), zone.getZoneID(), parameterID, value));
    console.info(
        "Controller #%d Zone #%d (%s) parameter %d (%s) to %s",
        zone.getControllerID(),
        zone.getZoneID(),
        zone.getName(),
        parameterID,
        parameterIDToString(parameterID),
        value
    );
})

// Start server
console.log("Starting Server...");
server.start();
