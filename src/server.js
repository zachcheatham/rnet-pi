const EventEmitter = require("events");
const net = require("net");
const mdns = require("mdns");
const ip = require("ip");

const Client = require("./client");

class Server extends EventEmitter {
    constructor(name, host, port) {
        super();

        this._name = name;
        this._port = port;

        if (!host) {
            this._host = ip.address();
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

    broadcast() {

    }

    stop() {
        this._service.destroy();
    }

    getAddress() {
        const addr = this._server.address();
        return addr.address + ":" + addr.port;
    }

    _handleConnection(conn) {
        const client = new Client(conn)
        .once("close", () => {
            if (client.isNamed()) {
                this.emit("client_disconnect", client);
            }
        })
        .once("named", () => {
            // Ready to tell the world!
            this.emit("client_connected", client);
        });

        client.requestName();
    }
}

module.exports = Server;
