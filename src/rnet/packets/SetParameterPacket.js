const DataPacket = require("./DataPacket");

const parameterIDToString = require("../parameterIDToString");

module.exports = class SetParameterPacket extends DataPacket {
    constructor(controllerID, zoneID, parameterID, value) {
        super();

        this._controllerID = controllerID;
        this._zoneID = zoneID;
        this._parameterID = parameterID;

        if (parameterID < 0 || parameterID > 8) {
            throw new Error("Unknown parameter ID (" + parameterID + ") while constructing SetParameterPacket");
        }

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

        this._value = value;
    }

    getTargetPath() {
        return [
            0x02, // Root Menu
            0x00, // Run Mode
            this._zoneID,
            0x00, // User Menu
            this._parameterID
        ]
    }

    getData() {
        return [
            this._value
        ];
    }
}
