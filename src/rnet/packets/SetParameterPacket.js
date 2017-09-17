const DataPacket = require("./DataPacket");

const parameterIDToString = require("../parameterIDToString");

module.exports = class SetParameterPacket extends DataPacket {
    constructor(controllerID, zoneID, parameterID, value) {
        super();

        if (parameterID < 0 || parameterID > 8) {
            throw new Error("Unknown parameter ID (" + parameterID + ") while constructing SetParameterPacket");
        }

        this.targetPath = [
            0x02, // Root Menu
            0x00, // Run Mode
            zoneID,
            0x00, // User Menu
            parameterID
        ]

        this.targetControllerID = controllerID;

        switch (parameterID) {
            case 0:
            case 1:
            case 3:
                if (value < -10 || value > 10)
                    throw new Error("Parameter \"" + parameterIDToString(parameterID) + "\" out of range (-10 - +10) while constructing SetParameterPacket");
                else
                    value += 10; // Add 10 for rNet
                break;
            case 4:
                if (value < 0 || value > 100)
                    throw new Error("Parameter \"" + parameterIDToString(parameterID) + "\" out of range (0 - 100) while constructing SetParameterPacket");
                else {
                    value = Math.floor(value / 2);
                }
                break;
            case 5:
            case 7:
                if (value < 0 || value > 2)
                    throw new Error("Parameter \"" + parameterIDToString(parameterID) + "\" out of range (0 - 2) while constructing SetParameterPacket");
                break;
            default:
                if (type(value) != type(true))
                    throw new Error("Parameter \"" + parameterIDToString(parameterID) + "\" not true nor false when constructing SetParameterPacket");
                else
                    value = value ? 1 : 0;
        }

        this.data = Buffer.alloc(1);
        this.data.writeUInt8(value, 0);
    }

    getControllerID() {
        return this.targetControllerID;
    }

    getZoneID() {
        return this.targetPath[2];
    }

    getParameterID() {
        return this.targetPath[4]
    }

    getValue() {
        var value = this.data.readUInt8(0);

        switch (this.getParameterID) {
            case 0:
            case 1:
            case 3:
                return value -= 10;
            case 4:
                return value * 2;
            case 5:
            case 7:
                return value;
            default:
                return value == 1;
        }
    }
}
