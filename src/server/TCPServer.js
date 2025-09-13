const EventEmitter = require("events");
const net = require("net");

const Client = require("./TCPClient");

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
            let bonjour = null;
            try {
                let bonjour = require("bonjour-service");

                console.log(`Publishing Bonjour Service: ${this._name} - rnet - ${this._port}`);
                this._bonjour = new bonjour.Bonjour();
                this._service = this._bonjour.publish({name: this._name, type: "rnet", port: this._port});
            }
            catch (e) {
                console.warn("Bonjour Unavaiable. Remotes won't be able to automatically find this controller.")
            }

            this.emit("start");
        });
    }

    broadcastBuffer(buffer) {
        for (let client of this._clients) {
            client.sendBuffer(buffer);
        }
    }

    stop(callback) {
        this._service.stop();
        for (let client of this._clients) {
            client.disconnect();
        }
        this._server.close(() => {
            console.info("Server stopped.")
            callback();
        });
    }

    getName() {
        return this._name;
    }

    setName(name) {
        if (name != this._name) {
            this._name = name;

            if (this._service != null) {
                this._service.stop();
                this._service = this._bonjour.publish({name: this._name, type: "rnet", port: this._port});
            }
        }
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
            this._clients.push(client);
            this.emit("client_connected", client);
        })
        .on("packet", (packet) => {
            this.emit("packet", client, packet);
        });
    }
}

module.exports = Server;
