const EventEmitter = require("events");

const TCPServer = require("./TCPServer");
const WSServer = require("./WSServer");

class Server extends EventEmitter {
    constructor(config) {
        super();

        const self = this

        this._tcpServer = new TCPServer(config.name, config.host, config.port);

        this._tcpServer.once("start", function() {
            console.info("[TCP Server] listening on %s", this.getAddress());
        })
        .on("error", function(err) {
            self.emit("error", err);
        })
        .on("client_connected", function(client) {
            self.emit("client_connected", client);
        })
        .on("client_disconnect", function(client) {
            self.emit("client_disconnect", client);
        })
        .on("packet", function(client, packet) {
            self.emit("packet", client, packet);
        });

        if (config.webPort) {
            this._wsServer = new WSServer(config.webHost, config.webPort);

            this._wsServer.once("start", function() {
                console.info("[Web Server] listening on %s", this.getAddress());
            })
            .on("client_connected", (client) => {
                this.emit("client_connected", client);
            })
            .on("client_disconnect", (client) => {
                this.emit("client_disconnect", client);
            })
            .on("packet", function(client, packet) {
                self.emit("packet", client, packet);
            })
            .on("error", function(err) {
                self.emit("error", err);
            });
        }
    }

    start() {
        this._tcpServer.start();
        if (this._wsServer) this._wsServer.start();
    }

    broadcast(packet) {
        const buffer = packet.getBuffer();

        this._tcpServer.broadcastBuffer(buffer);
        if (this._wsServer) {
            this._wsServer.broadcastBuffer(buffer);
        }
    }

    stop(callback) {
        let otherStopped = true; // TODO:
        this._tcpServer.stop(function() {
            console.log("[TCP Server] Stopped.")

            if (otherStopped) {
                callback();
            }
            else {
                otherStopped = true;
            }
        });
    }

    getName() {
        return this._tcpServer.getName();
    }

    setName(name) {
        this._tcpServer.setName(name);
    }

    getTCPAddress() {
        return this._tcpServer.getAddress();
    }

    getWSAddress() {
        return this._wsServer.getAddress();
    }

    getClientCount() {
        return this._tcpServer.getClientCount() + this._wsServer ? this._wsServer.getClientCount() : 0;
    }
}

module.exports = Server;
