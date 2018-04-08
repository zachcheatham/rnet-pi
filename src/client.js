const EventEmitter = require("events");

const SmartBuffer = require("smart-buffer").SmartBuffer;

const createPacket = require("./packets/createPacket");
const PacketC2S = require("./packets/PacketC2S");
const PacketC2SIntent = require("./packets/PacketC2SIntent");
const PacketS2CZoneIndex = require("./packets/PacketS2CZoneIndex");

class Client extends EventEmitter {
    constructor(connection) {
        super();

        this._intent = false;
        this._pendingBuffer = SmartBuffer.fromSize(255);
        this._pendingBytesRemaining = false;
        this._pendingPacketType = false;

        this._connection = connection;

        this._connection.on("data", (data) => {
            this._recvData(data);
        });

        this._connection.on("error", (error) => {
        })

        this._connection.once("close", () => {
            this._connectionClosed();
        });
    }

    send(packet) {
        //console.info("DEBUG: Sending packet " + packet.constructor.name + " to " + this.getAddress());
        this._connection.write(packet.getBuffer());
    }

    sendBuffer(buffer) {
        this._connection.write(buffer);
    }

    getAddress() {
        return this._connection.remoteAddress;
    }

    isValid() {
        return this._intent !== false;
    }

    isSubscribed() {
        return this._intent == 0x02;
    }

    _recvData(data) {
        const incomingBuffer = SmartBuffer.fromBuffer(data);

        while (incomingBuffer.remaining() > 0) {
            if (this._pendingBytesRemaining === false) {
                this._pendingPacketType = incomingBuffer.readUInt8();
                this._pendingBytesRemaining = incomingBuffer.readUInt8();
            }
            else {
                let bytesToRead;

                if (this._pendingBytesRemaining > incomingBuffer.remaining()) {
                    bytesToRead = incomingBuffer.remaining();
                }
                else {
                    bytesToRead = this._pendingBytesRemaining;
                }

                this._pendingBuffer.writeBuffer(incomingBuffer.readBuffer(bytesToRead));
                this._pendingBytesRemaining -= bytesToRead;
            }

            if (this._pendingBytesRemaining == 0) {
                this._handlePacket(this._pendingPacketType, this._pendingBuffer.toBuffer());
                this._pendingBytesRemaining = false;
                this._pendingPacketType = false;
                this._pendingBuffer = SmartBuffer.fromSize(255);
            }
        }
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

    _connectionClosed() {
        this.emit("close");
    }
}

module.exports = Client;
