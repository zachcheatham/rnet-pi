const EventEmitter = require("events");


const createPacket = require("./packets/createPacket");
const PacketC2S = require("./packets/PacketC2S");
const PacketC2SIntent = require("./packets/PacketC2SIntent");
const PacketS2CZoneIndex = require("./packets/PacketS2CZoneIndex");

class Client extends EventEmitter {
    constructor() {
        super();

        if (this.constructor === Client) {
            throw new Error("Client is an abstract class!");
        }

        this._intent = false;
    }

    send(packet) {
    }

    sendBuffer(buffer) {
    }

    getAddress() {
    }

    disconnect() {
    }

    isValid() {
        return this._intent !== false;
    }

    isSubscribed() {
        return this._intent == 0x02;
    }

    _handlePacket(packetType, data) {
        const packet = createPacket(packetType, data);

        if (packet !== undefined) {
            //console.info("DEBUG: Recieved packet " + packet.constructor.name + " from " + this.getAddress());

            if (packet.getID() == PacketC2SIntent.ID) {
                this._intent = packet.getIntent();
                if (this._intent == 0x02) { // Subscribe mode
                    this.emit("subscribed");
                }
            }
            else if (this.isValid()) {
                this.emit("packet", packet);
            }
        }
        else {
            console.warn("Recieved bad packet from " + this.getAddress());
        }
    }
}

module.exports = Client;
