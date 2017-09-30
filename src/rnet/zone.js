const EventEmitter = require("events");

const ExtraZoneParam = require("./extraZoneParam");
const RequestDataPacket = require("./packets/RequestDataPacket");
const RequestParameterPacket = require("./packets/RequestParameterPacket");

class Zone extends EventEmitter {
    constructor(rnet, ctrllrID, zoneID) {
        super()

        this._rNet = rnet;
        this._zoneID = zoneID;
        this._ctrllrID = ctrllrID;

        this._name = null;
        this._power = false;
        this._volume = 0;
        this._source = 0;

        this._parameters = [
            0,      // Bass             -10 - +10
            0,      // Treble           -10 - +10
            false,  // Loudness
            0,      // Balance          -10 - +10
            0,      // Turn on Volume   0 - 100
            0,      // Background Color 0 - 2
            false,  // Do Not Disturb
            0,      // Party Mode       0 - 2
            false,  // Front AV Enable
        ]
    }

    getControllerID() {
        return this._ctrllrID;
    }

    getZoneID() {
        return this._zoneID;
    }

    getName() {
        return this._name;
    }

    setName(name) {
        this._name = name;
        this.emit("name", name);
    }

    getPower() {
        return this._power;
    }

    setPower(powered, rNetTriggered=false) {
        if (powered != this._power) {
            this._power = powered;
            this.emit("power", powered, rNetTriggered);

            if (powered) {
                setTimeout(() => {
                    this.requestInfo();
                }, 1000);
            }
        }
        return true;
    }

    getVolume() {
        return this._volume;
    }

    setVolume(volume, rNetTriggered=false) {
        if (volume >= 0 && volume <= 100) {
            if (volume != this._volume) {
                this._volume = volume;
                this.emit("volume", volume, rNetTriggered);
            }

            return true;
        }
        else {
            return false;
        }
    }

    getSourceID() {
        return this._source;
    }

    setSourceID(id, rNetTriggered=false) {
        if (rNetTriggered || this._rNet.sourceExists(id)) {
            if (this._source != id) {
                this._source = id;
                this.emit("source", id, rNetTriggered);
            }
            return true;
        }
        else {
            return false;
        }
    }

    getParameter(parameterID) {
        if (parameterID >= 0 && parameterID <= 8) {
            return this._parameters[parameterID];
        }
        else {
            return null;
        }
    }

    setParameter(parameterID, value, rNetTriggered=false) {
        if (parameterID >= 0 && parameterID <= 8) {
            // Validate parameter
            switch (parameterID) {
                case 0:
                case 1:
                case 3:
                    if (value < -10 || value > 10)
                        return false;
                    break;
                case 4:
                    if (value < 0 || value > 100)
                        return false;
                    break;
                case 5:
                case 7:
                    if (value < 0 || value > 2)
                        return false;
                    break;
            }
            if (this._parameters[parameterID] != value) {
                this._parameters[parameterID] = value;
                this.emit("parameter", parameterID, value, rNetTriggered)
            }
        }
        else {
            return false;
        }
    }

    requestInfo() {
        this._rNet.sendData(new RequestDataPacket(this._ctrllrID, this._zoneID, RequestDataPacket.DATA_TYPE.ZONE_INFO));
    }

    requestBasicInfo() {
        this._rNet.sendData(new RequestDataPacket(this._ctrllrID, this._zoneID, RequestDataPacket.DATA_TYPE.ZONE_POWER));
        this._rNet.sendData(new RequestDataPacket(this._ctrllrID, this._zoneID, RequestDataPacket.DATA_TYPE.ZONE_VOLUME));
        this._rNet.sendData(new RequestDataPacket(this._ctrllrID, this._zoneID, RequestDataPacket.DATA_TYPE.ZONE_SOURCE));
    }

    requestPowered() {
        this._rNet.sendData(new RequestDataPacket(this._ctrllrID, this._zoneID, RequestDataPacket.DATA_TYPE.ZONE_POWER));
    }
}

module.exports = Zone;
