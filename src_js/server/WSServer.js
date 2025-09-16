const EventEmitter = require("events");
const http = require("http");
const WebSocketServer = require("websocket").server;

const Client = require("./WSClient");

class WSServer extends EventEmitter {
    constructor(host, port) {
        super();

        this._port = port;
        this._clients = [];

        if (!host)
            this._host = "0.0.0.0";
        else
            this._host = host;

        this._httpServer = http.createServer();

        this._server = new WebSocketServer({
            httpServer: this._httpServer
        });

        this._server.on("request", (request) => {
            this._handleConnection(request);
        })
    }

    start() {
        let self = this;
        this._httpServer.listen(this._port, this._host, function() {
            self.emit("start");
        })
    }

    broadcastBuffer(buffer) {
        for (let client of this._clients) {
            client.sendBuffer(buffer);
        }
    }

    stop(callback) {
        console.warning("We haven't implemented WSServer.stop()");
    }

    getAddress() {
        return this._host + ":" + this._port;
    }

    getClientCount() {
        return this._clients.length;
    }

    _handleConnection(request) {
        const connection = request.accept(null, request.origin);

        const client = new Client(connection)
        .once("close", () => {
            if (client.isSubscribed()) {
                this.emit("client_disconnect", client);

                let i = this._clients.indexOf(client);
                this._clients.splice(i, 1);
            }
        })
        .once("subscribed", () => {
            this._clients.push(client);
            this.emit("client_connected", client);
        })
        .on("packet", (packet) => {
            this.emit("packet", client, packet);
        });
    }


}

module.exports = WSServer;
