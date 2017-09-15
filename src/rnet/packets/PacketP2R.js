const SmartBuffer = require("smart-buffer").SmartBuffer;

module.exports = class PacketP2R {
    constructor() {
        if (new.target === PacketP2R) {
            throw new TypeError("Cannot initiate a PacketP2R object. PacketP2R is meant to be extended.");
        }

        if (this.getMessageType === undefined) {
            throw new TypeError("PacketP2R subclasses must implement getMessageType()");
        }

        if (this.getEventIDLoByte === undefined) {
            throw new TypeError("PacketP2R subclasses must implement getEventIDLoByte()");
        }
    }

    getBuffer() {
        const buffer = new SmartBuffer();
        buffer.writeUInt8(0xF0); // Start of message
        buffer.writeUInt8(this.getTargetControllerID());
        buffer.writeUInt8(this.getTargetZoneID());
        buffer.writeUInt8(this.getTargetKeypadID());
        buffer.writeUInt8(this.getSourceControllerID()); // Source controller ID
        buffer.writeUInt8(this.getSourceZoneID());
        buffer.writeUInt8(this.getSourceKeypadID());
        buffer.writeUInt8(this.getMessageType());
        {
            let targetPath = this.getTargetPath();
            buffer.writeUInt8(targetPath.length);
            for (let i = 0; i < targetPath.length; i++) {
                buffer.writeUInt8(targetPath[i])
            }
        }
        {
            let sourcePath = this.getSourcePath();
            buffer.writeUInt8(sourcePath.length);
            for (let i = 0; i < sourcePath.length; i++) {
                buffer.writeUInt8(sourcePath[i])
            }
        }

        if (this.invertEventIDLo()) {
            buffer.writeUInt8(0xF1);
        }

        buffer.writeUInt8(this.getEventIDLoByte());
        buffer.writeUInt8(0x00); // Event ID Hi Byte



        return this._buffer.toBuffer();
    }

    getTargetControllerID() {
        return 0x00;
    }

    getTargetZoneID() {
        return 0x00;
    }

    getTargetKeypadID() {
        return 0x7F; // The controller is the target
    }

    getSourceControllerID() {
        return 0x00;
    }

    getSourceZoneID() {
        return 0x00;
    }

    getSourceKeypadID() {
        return 0x70;
    }

    getTargetPath() {
        return [0x02, 0x00];
    }

    getSourcePath() {
        return [];
    }

    invertEventIDLo()
        return false;
    }
}
