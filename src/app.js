const colors = require("colors");
const Server = require("./server");
const RNet = require("./rnet/rnet")

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
});

// Start server
console.log("Starting Server...");
server.start();
