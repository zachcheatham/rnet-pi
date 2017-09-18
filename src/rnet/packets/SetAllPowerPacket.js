const EventPacket = require("./EventPacket");

module.exports = class SetAllPowerPacket extends EventPacket {
    constructor(power) {
        super();

        this.targetPath = [0x02, 0x00];
        this.targetControllerID = 0x7E;
        this.eventID = 0xDD;
        this.eventData = 0x00;
        this.eventTimestamp = (power === true ? 1 : 0) << 8;
        this.eventPriority = 1;
    }
}
