const EventEmitter = require("events");
const animate = require("amator");

const ExtraZoneParam = require("./extraZoneParam");
const DisplayMessagePacket = require("./packets/DisplayMessagePacket");
const RequestDataPacket = require("./packets/RequestDataPacket");
const RequestParameterPacket = require("./packets/RequestParameterPacket");

class Zone extends EventEmitter {
    constructor(rnet, ctrllrID, zoneID) {
        super();

        this._rNet = rnet;
        this._zoneID = zoneID;
        this._ctrllrID = ctrllrID;

        this._name = null;
        this._power = false;
        this._volume = 0;
        this._source = 0;
        this._mute = false;
        this._preMuteVolume = 0;
        this._maxVolume = 100;

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

            if (this._muted) {
                this._muted = false;
                this._preMuteVolume = 0;
            }

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

    setVolume(volume, rNetTriggered=false, forMute=false) {
        if (volume >= 0 && volume <= 100) {
            if (volume != this._volume) {
                if (this._mute && !forMute) {
                    this._mute = false;
                    this._preMuteVolume = 0;
                }

                if (volume > this._maxVolume) {
                    volume = this._maxVolume;
                    rNetTriggered = false; // Set rNetTriggered to false since rnet can go over our max
                }

                this._volume = volume;
                this.emit("volume", volume, rNetTriggered);
            }

            return true;
        }
        else {
            return false;
        }
    }

    getMaxVolume() {
        return this._maxVolume;
    }

    setMaxVolume(maxVolume, save=true) {
        if (maxVolume >= 0 && maxVolume <= 100)
        {
            if (maxVolume != this._maxVolume) {
                this._maxVolume = maxVolume;
                if (this._volume > this._maxVolume) {
                    this.setVolume(this._maxVolume);
                }

                this.emit("max-volume", maxVolume);

                if (save) {
                    this._rNet.writeZones();
                }
            }

            return true;
        }
        else {
            return false;
        }
    }

    getMuted() {
        return this._mute;
    }

    setMute(muted, fadeTime=0) {
        if (muted != this._mute) {
            this._mute = muted;

            if (fadeTime == 0) {
                if (muted) {
                    this._preMuteVolume = this.getVolume();
                    this.setVolume(0, false, true);
                }
                else {
                    this.setVolume(this._preMuteVolume, false, true);
                    this._preMuteVolume = 0;
                }
            }
            else {
                if (muted) {
                    this._preMuteVolume = this.getVolume();
                    animate({v: this._preMuteVolume / 2}, {v: 0}, {
                        duration: fadeTime,
                        step: (fo) => {
                            const vol = Math.round(fo.v) * 2;
                            this.setVolume(vol, false, true);
                        }
                    });
                }
                else {
                    animate({v: 0}, {v: this._preMuteVolume / 2}, {
                        duration: fadeTime,
                        step: (fo) => {
                            const vol = Math.round(fo.v) * 2;
                            this.setVolume(vol, false, true);
                        }
                    });
                }
            }
        }
    }

    getSourceID() {
        return this._source;
    }

    setSourceID(id, rNetTriggered=false) {
        if (rNetTriggered || this._rNet.getSource(id) != null) {
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
        this._rNet.sendData(new RequestParameterPacket(this._ctrllrID, this._zoneID, ExtraZoneParam.TURN_ON_VOLUME));
    }

    requestBasicInfo() {
        this._rNet.sendData(new RequestDataPacket(this._ctrllrID, this._zoneID, RequestDataPacket.DATA_TYPE.ZONE_POWER));
        this._rNet.sendData(new RequestDataPacket(this._ctrllrID, this._zoneID, RequestDataPacket.DATA_TYPE.ZONE_VOLUME));
        this._rNet.sendData(new RequestDataPacket(this._ctrllrID, this._zoneID, RequestDataPacket.DATA_TYPE.ZONE_SOURCE));
    }

    requestPowered() {
        this._rNet.sendData(new RequestDataPacket(this._ctrllrID, this._zoneID, RequestDataPacket.DATA_TYPE.ZONE_POWER));
    }

    displayMessage(message, flashTime=0, alignment=DisplayMessagePacket.ALIGN_LEFT) {
        this._rNet.sendData(new DisplayMessagePacket(this._ctrllrID, this._zoneID, alignment, flashTime, message));
    }
}

module.exports = Zone;
