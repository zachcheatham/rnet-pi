const colors = require("colors");
const Server = require("./server");



console.log("RNet Proxy v1.0.0");
console.log("By Zach Cheatham");
console.log("");

// Modify console.log to include timestamps
require("console-stamp")(console, "HH:MM:ss");

// Setup server
const server = new Server(3000)
.once("start", function() {
    console.log("Server listening on %s", server.getAddress());
    startRNet();
})
.once("error", function(error) {
    console.log("Server error: %s", error.message);
    process.exit(1);
})
.on("client_connected", function(client) {
    console.log("Client %s connected.", client.getName());
})
.on("client_disconnect", function(client) {
    console.log("Client %s disconnected.", client.getName());
});

// Start server
console.log("Starting Server...");
server.start();

// Function to start rNet once server is ready
const startRNet = function() {
    console.log("Connecting to rNET device...");
}
