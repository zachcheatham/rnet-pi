const RNetPacket = require("./RNetPacket");
const DataPacket = require("./DataPacket");
const EventPacket = require("./EventPacket");

module.exports = {
    build: function(buffer) {
        var packet = RNetPacket.fromData(buffer);
        switch (packet.messageType) {
            case 0x00:
                packet = DataPacket.fromPacket(packet);
                break;
            case 0x01:
                console.log("DEBUG: Received a RequestDataPacket from RNet");
                return false; // Don't care about these
            case 0x02:
                console.log("DEBUG: Received a HandshakePacket from RNet");
                return false; // Don't care about these
            case 0x05:
                packet = EventPacket.fromPacket(packet);
                break;
        }

        // TODO: Break down these more
        return packet;
    }
}
