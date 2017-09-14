const EventEmitter = require("events");
const net = require("net");

const Client = require("./client");

class Server extends EventEmitter {
    constructor(port) {
        super();

        this._port = port;
        this._server = net.createServer();

        this._server.on("error", (err) => {
            this.emit("error", err);
        });

        this._server.on("connection", (conn) => {
            this._handleConnection(conn);
        });
    }

    start() {
        this._server.listen(this._port, () => {
            this.emit("start");
        });
    }

    broadcast() {

    }

    stop() {

    }

    getAddress() {
        const addr = this._server.address();
        return addr.address + addr.port;
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
