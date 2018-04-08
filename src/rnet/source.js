const EventEmitter = require("events");

class Source extends EventEmitter {
    constructor(rnet, sourceID, name, type) {
        super();

        this._rNet = rnet;

        this._id = sourceID;
        this._name = name;
        this._type = type;

        this._autoOnZones = [];
        this._autoOff = false;

        this._mediaMetadataTitle = null;
        this._mediaMetadataArtist = null;
        this._mediaMetadataArtworkURL = null;

        this._descriptiveText = null;
        this._descriptiveTextFromRNet = true;
    }

    inUse() {
        for (let ctrllrID = 0; ctrllrID < this._rNet.getControllersSize(); ctrllrID++) {
            for (let zoneID = 0; zoneID < this._rNet.getZonesSize(ctrllrID); zoneID++) {
                let zone = this._rNet.getZone(ctrllrID, zoneID);
                if (zone.getSourceID() == this._id && zone.getPower()) {
                    return true;
                }
            }
        }

        return false;
    }

    getZones() {
        let zones = [];

        for (let ctrllrID = 0; ctrllrID < this._rNet.getControllersSize(); ctrllrID++) {
            for (let zoneID = 0; zoneID < this._rNet.getZonesSize(ctrllrID); zoneID++) {
                let zone = this._rNet.getZone(ctrllrID, zoneID);
                if (zone.getSourceID() == this._id && zone.getPower()) {
                    zones.push(zone);
                }
            }
        }

        return zones;
    }

    getSourceID() {
        return this._id;
    }

    getName() {
        return this._name;
    }

    setName(name) {
        this._name = name;
        this.emit("name", name);
    }

    setType(type) {
        this._type = type;
        this.emit("type", type);
    }

    getType() {
        return this._type;
    }

    isCast() {
        return this._type == Source.TYPE_GOOGLE_CAST;
    }

    /*
     * Media Metedata is used for smart sources
     * such as a chromecast or sonos device
     */

    setMediaMetadata(title, artist, artworkURL) {
        this._mediaMetadataTitle = title;
        this._mediaMetadataArtist = artist;
        this._mediaMetadataArtworkURL = artworkURL;

        this.emit("media-metadata", title, artist, artworkURL);
    }

    getMediaTitle() {
        return this._mediaMetadataTitle;
    }

    getMediaArtist() {
        return this._mediaMetadataArtist;
    }

    getMediaArtworkURL() {
        return this._mediaMetadataArtworkURL;
    }

    setDescriptiveText(message, flashTime=0, rNetTriggered=false) {
        if (flashTime == 0) {
            this._descriptiveText = message;
            this._descriptiveTextFromRNet = rNetTriggered;
        }

        if (message == null) {
            message = this.getName();
        }

        this.emit("descriptive-text", message, flashTime, rNetTriggered);
    }

    getDescriptiveText() {
        return this._descriptiveText;
    }

    isDescriptionFromRNet() {
        return this._descriptiveTextFromRNet;
    }

    control(operation, srcCtrllr, srcZone, rNetTriggered=false) {
        this.emit("control", operation, srcCtrllr, srcZone, rNetTriggered);
    }

    setZoneAutoOff(autoOff) {
        this._autoOff = autoOff;
    }

    setAutoOnZones(zones) {
        this._autoOnZones = zones;
    }

    // Called by smart device integration to toggle auto on/off
    _onPower(powered) {
        if (powered) {
            if (!this.isUse()) {
                for (let id in this._autoOnZones) {
                    let zone = this._rNet.getZone(id[0], id[1]);
                    zone.setPower(true);
                }
            }
        }
        else {
            for (let zone in this.getZones()) {
                zone.setPower(false);
            }
        }
    }
}

Source.TYPE_GENERIC = 0;
Source.TYPE_AIRPLAY = 1;
Source.TYPE_BLURAY = 2;
Source.TYPE_CABLE = 3;
Source.TYPE_CD = 4;
Source.TYPE_COMPUTER = 5;
Source.TYPE_DVD = 6;
Source.TYPE_GOOGLE_CAST = 7;
Source.TYPE_INTERNET_RADIO = 8;
Source.TYPE_IPOD = 9;
Source.TYPE_MEDIA_SERVER = 10;
Source.TYPE_MP3 = 11;
Source.TYPE_OTA = 12;
Source.TYPE_PHONO = 13;
Source.TYPE_RADIO = 14;
Source.TYPE_SATELITE = 15;
Source.TYPE_SATELITE_RADIO = 16;
Source.TYPE_SONOS = 17;
Source.TYPE_TAPE = 18;
Source.TYPE_VCR = 19;

Source.CONTROL_NEXT = 0;
Source.CONTROL_PREV = 1;
Source.CONTROL_STOP = 2;
Source.CONTROL_PLAY = 3;
Source.CONTROL_PAUSE = 4;
Source.CONTROL_PLUS = 5;
Source.CONTROL_MINUS = 6;

module.exports = Source;
