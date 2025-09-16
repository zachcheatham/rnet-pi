const SmartBuffer = require("smart-buffer").SmartBuffer;

const Client = require("./Client");
const createPacket = require("./packets/createPacket");
const PacketC2S = require("./packets/PacketC2S");
const PacketC2SIntent = require("./packets/PacketC2SIntent");
const PacketS2CZoneIndex = require("./packets/PacketS2CZoneIndex");

class TCPClient extends Client {
    constructor(connection) {
        super();

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
            this.emit("close");
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

    disconnect() {
        this._connection.end();
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
}

module.exports = TCPClient;
