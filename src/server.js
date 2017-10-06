const EventEmitter = require("events");
const net = require("net");
const mdns = require("mdns");

const Client = require("./client");

class Server extends EventEmitter {
    constructor(name, host, port) {
        super();

        this._name = name;
        this._port = port;
        this._clients = [];

        if (!host) {
            this._host = "0.0.0.0";
        }
        else {
            this._host = host;
        }

        this._server = net.createServer();

        this._server.on("error", (err) => {
            this.emit("error", err);
        });

        this._server.on("connection", (conn) => {
            this._handleConnection(conn);
        });
    }

    start() {
        this._server.listen(this._port, this._host, () => {
            this._service = mdns.createAdvertisement(mdns.tcp("rnet"), this._port, {name: this._name});
            this._service.start();

            this.emit("start");
        });
    }

    broadcast(packet) {
        //console.info("DEBUG: Sending packet " + packet.constructor.name + " to everyone");
        const buffer = packet.getBuffer();
        for (let client of this._clients) {
            client.sendBuffer(buffer);
        }
    }

    stop() {
        this._service.destroy();
    }

    getAddress() {
        const addr = this._server.address();
        return addr.address + ":" + addr.port;
    }

    getClientCount() {
        return this._clients.length;
    }

    _handleConnection(conn) {
        const client = new Client(conn)
        .once("close", () => {
            if (client.isSubscribed()) {
                this.emit("client_disconnect", client);

                let i = this._clients.indexOf(client);
                this._clients.splice(i, 1);
            }
        })
        .once("subscribed", () => {
            // Ready to tell the world!
            this.emit("client_connected", client);
            this._clients.push(client);
        })
        .on("packet", (packet) => {
            this.emit("packet", client, packet);
        });
    }
}

module.exports = Server;
