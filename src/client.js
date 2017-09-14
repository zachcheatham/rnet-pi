const EventEmitter = require("events");

const createPacket = require("./packets/createPacket");
const PacketC2S = require("./packets/PacketC2S");
const PacketC2SName = require("./packets/PacketC2SName");
const PacketS2CName = require("./packets/PacketS2CName");

class Client extends EventEmitter {
    constructor(connection) {
        super();

        this._connection = connection;

        this._connection.on("data", (data) => {
            this._recvData(data);
        });

        this._connection.once("close", () => {
            this._connectionClosed();
        });
    }

    requestName() {
        this.send(new PacketS2CName());
    }

    send(packet) {
        console.info("DEBUG: Sending packet " + packet.constructor.name + " to " + this.getName());
        this._connection.write(packet.getBuffer());
    }

    getName() {
        return this._name || this.getAddress();
    }

    getAddress() {
        return this._connection.remoteAddress;
    }

    isNamed() {
        return this._name !== undefined;
    }

    _recvData(data) {
        const packet = createPacket(data);

        if (packet !== undefined) {
            console.info("DEBUG: Recieved packet " + packet.constructor.name + " from " + this.getName());

            if (packet instanceof PacketC2SName) {
                this._name = packet.name;
                this.emit("named");
            }
            else {
                this.emit("packet", packet);
            }
        }
        else {
            console.warn("Recieved bad packet from " + this.getName());
        }
    }

    _connectionClosed() {
        this.emit("close");
    }
}

module.exports = Client;
