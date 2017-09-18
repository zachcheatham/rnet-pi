const vorpal = require("vorpal")();

const Server = require("./server");
const RNet = require("./rnet/rnet");
const PacketS2CRNetStatus = require("./packets/PacketS2CRNetStatus");
const parameterIDToString = require("./rnet/parameterIDToString");

console.log("RNet Proxy v1.0.0");
console.log("By Zach Cheatham");
console.log("");

// Modify console.log to include timestamps
require("console-stamp")(console, "HH:MM:ss");

const server = new Server(3000)
const rNet = new RNet();

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
    console.log("Client %s connected.", client.getName());
})
.on("client_disconnect", function(client) {
    console.log("Client %s disconnected.", client.getName());
})
.on("packet", function(client, packet) {

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
    console.info(
        "Controller #%d zone #%d (%s) created.",
        zone.getControllerID(),
        zone.getZoneID(),
        zone.getName()
    );
})
.on("zone-name", (zone, name) => {
    console.info(
        "Controller #%d zone #%d renamed to %s.",
        zone.getControllerID(),
        zone.getZoneID(),
        name
    );
})
.on("zone-deleted", (ctrllrID, zoneID) => {
    console.info(
        "Controller #%d zone #%d deleted.",
        ctrllrID,
        zoneID
    );
})
.on("new-source", (sourceID) => {
    console.info(
        "Source #%d (%s) created.",
        sourceID,
        rNet.getSourceName(sourceID)
    );
})
.on("source-name", (sourceID) => {
    console.info(
        "Source #%d renamed to %s.",
        sourceID,
        rNet.getSourceName(sourceID)
    );
})
.on("source-deleted", (sourceID) => {
    console.info(
        "Source #%d deleted.",
        sourceID
    );
})
.on("power", (zone, powered) => {
    console.info(
        "Controller #%d zone #%d (%s) power set to %s",
        zone.getControllerID(),
        zone.getZoneID(),
        zone.getName(),
        powered
    );
})
.on("volume", (zone, volume) => {
    console.info(
        "Controller #%d zone #%d (%s) volume set to %d",
        zone.getControllerID(),
        zone.getZoneID(),
        zone.getName(),
        volume
    );
})
.on("source", (zone, sourceID) => {
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

/**
 * Source data
 */
vorpal
.command("list sources", "Lists sources.")
.action(function(args, callback) {
    const sources = rNet.getSources();
    for (var i = 0; i < sources.length; i++) {
        if (sources[i]) {
            console.log("%d\t%s", i, sources[i].name);
        }
    }
    callback();
});
vorpal
.command("create source <id> <name>", "Creates a new source.")
.action(function(args, callback) {
    if (!rNet.createSource(args.id, args.name)) {
        console.error("Source #%d already exists as %s", args.id, rNet.getSourceName(args.id));
    }
    callback();
});
vorpal
.command("rename source <id> <name>", "Renames a source.")
.action(function(args, callback) {
    if (!rNet.renameSource(args.id, args.name)) {
        console.error("Source #%d doesn't exist", args.id);
    }
    callback();
});
vorpal
.command("delete source <id>", "Deletes a source.")
.action(function(args, callback) {
    rNet.deleteSource(args.id);
    callback();
});

/**
 * Zone data
 */
vorpal
.command("list zones", "Lists zones.")
.action(function(args, callback) {
    console.log("CTRL\tZONE\tNAME")
    const zones = rNet.getZones();
    for (var c = 0; c < zones.length; c++) {
        if (zones[c]) {
            for (var z = 0; z < zones[c].length; z++) {
                if (zones[c][z]) {
                    console.log("%d\t%d\t%s", c, z, zones[c][z].getName());
                }
            }
        }
    }
    callback();
});
vorpal
.command("create zone <cid> <id> <name>", "Creates a new zone.")
.action(function(args, callback) {
    if (!rNet.createZone(args.cid, args.id, args.name)) {
        console.error(
            "Controller #%d zone #%d already exists as %s",
            args.cid,
            args.id,
            rNet.getZone(args.cid, args.id).getName()
        );
    }
    callback();
});
vorpal
.command("rename zone <cid> <id> <name>", "Renames a zone.")
.action(function(args, callback) {
    const zone = rNet.getZone(args.cid, args.id);
    if (!zone) {
        console.error(
            "Controller #%d zone #%d doesn't exist",
            args.cid,
            args.id
        );
    }
    else {
        zone.setName(args.name);
    }

    callback();
});
vorpal
.command("delete zone <cid> <id>", "Deletes a zone.")
.action(function(args, callback) {
    if (!rNet.deleteZone(args.cid, args.id)) {
        console.error(
            "Controller #%d zone #%d doesn't exist",
            args.cid,
            args.id
        );
    }

    callback();
});

/**
 * Zone commands
 */
vorpal
.command("set power all <power>", "Set all zone on/off")
.action(function(args, callback) {
    rNet.setAllPower(args.power == "true");

    callback();
});

vorpal
.command("set power <cid> <id> <power>", "Set zone power")
.action(function(args, callback) {
    const zone = rNet.getZone(args.cid, args.id);
    if (!zone) {
        console.error(
            "Controller #%d zone #%d doesn't exist",
            args.cid,
            args.id
        );
    }
    else {
        zone.setPower(args.power == "true");
    }

    callback();
});
vorpal
.command("set volume <cid> <id> <volume>", "Set zone volume.")
.action(function(args, callback) {
    const zone = rNet.getZone(args.cid, args.id);
    if (!zone) {
        console.error(
            "Controller #%d zone #%d doesn't exist",
            args.id,
            args.cid
        );
    }
    else {
        zone.setVolume(parseInt(args.volume));
    }

    callback();
});
vorpal
.command("set source <cid> <id> <sourceID>", "Set zone source")
.action(function(args, callback) {
    const zone = rNet.getZone(args.cid, args.id);
    if (!zone) {
        console.error(
            "Controller #%d zone #%d doesn't exist",
            args.cid,
            args.id
        );
    }
    else {
        zone.setSourceID(parseInt(args.sourceID));
    }

    callback();
});
vorpal
.command("request all info", "Requests info update on all zones")
.action(function(args, callback) {
    rNet.requestAllZoneInfo();
    callback();
});

vorpal
.command("request info <cid> <id>", "Requests info update on zone")
.action(function(args, callback) {
    const zone = rNet.getZone(args.cid, args.id);
    if (!zone) {
        console.error(
            "Controller #%d zone #%d doesn't exist",
            args.cid,
            args.id
        );
    }
    else {
        zone.requestInfo();
    }

    callback();
});

vorpal.delimiter(">").show();
