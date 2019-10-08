const Client = require("./Client");
const createPacket = require("./packets/createPacket");
const PacketC2S = require("./packets/PacketC2S");
const PacketS2CZoneIndex = require("./packets/PacketS2CZoneIndex");

class WSClient extends Client {
    constructor(connection) {
        super();
        this._connection = connection;

        this._connection.on("message", (message) => {
            if (message.type == "utf8") {
                console.warn("[WebSocket] Unsubscribed client at " + this.getAddress() + " attempted to send us an unaccepted message type.")
                connection.close(1008, null);
            }
            else if (message.type == "binary") {
                const buffer = message.binaryData;
                const packetType = buffer.readUInt8();
                this._handlePacket(packetType, buffer.slice(2));
            }
        });

        this._connection.on("error", (error) => {
        })

        this._connection.once("close", () => {
            this.emit("close");
        });
    }

    send(packet) {
        //console.info("DEBUG: Sending packet " + packet.constructor.name + " to " + this.getAddress());
        this._connection.sendBytes(packet.getBuffer());
    }

    sendBuffer(buffer) {
        this._connection.sendBytes(buffer);
    }

    getAddress() {
        return this._connection.remoteAddress;
    }

    disconnect() {
        this._connection.end();
    }
}

module.exports = WSClient;
