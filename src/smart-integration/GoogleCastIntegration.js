const Source = require("../rnet/source");
const patchCastMonitor = require("../util/patch").castMonitor;
var CastDeviceMonitor = null;

class GoogleCastIntegration {
    constructor(rNet) {
        this._rNet = rNet;
        this._pendingSources = [];
        this._devices = [];
        this._settingUp = false;

        let sources = rNet.getSourcesByType(Source.TYPE_GOOGLE_CAST);
        for (let i in sources) {
            this._pendingSources.push(sources[i].getSourceID());
        }

        if (this._pendingSources.length > 0) {
            this.setup()
        }

        this._rNet.on("new-source", (source) => {
            if (source.getType() == Source.TYPE_GOOGLE_CAST) {
                if (CastDeviceMonitor) {
                    this.integrateSource(source.getSourceID());
                }
                else {
                    this._pendingSources.push(source.getSourceID());
                    this.setup();
                }
            }
        });

        this._rNet.on("source-name", (sourceID, name) => {
            let source = this._rNet.getSource(sourceID);
            if (source.getType() == Source.TYPE_GOOGLE_CAST) {
                if (CastDeviceMonitor) {
                    this.seperateSource(sourceID);
                    this.integrateSource(sourceID);
                }
            }
        });

        this._rNet.on("source-type", (sourceID, type) => {
            if (type == Source.TYPE_GOOGLE_CAST) {
                if (CastDeviceMonitor) {
                    this.integrateSource(sourceID);
                }
                else {
                    pendingSources.add(sourceID);
                    setup();
                }
            }
            else {
                this.seperateSource(sourceID);
            }
        });

        this._rNet.on("source-deleted", (sourceID) => {
            this.seperateSource(sourceID);
        });

        console.info("Google Cast integration ready.");
    }

    setup() {
        if (!this._settingUp && !CastDeviceMonitor) {
            this._settingUp = true;
            patchCastMonitor(() => {
                CastDeviceMonitor = require("castv2-device-monitor").DeviceMonitor;
                for (let i in this._pendingSources) {
                    this.integrateSource(this._pendingSources[i]);
                }
                this._settingUp = false;
            });
        }
    }

    integrateSource(sourceID) {
        let source = this._rNet.getSource(sourceID);
        if (source !== null && source.getType() == Source.TYPE_GOOGLE_CAST) {
            const device = {
                "sourceID": sourceID,
                "name": source.getName(),
                "monitor": new CastDeviceMonitor(source.getName()),
                "lastState": false
            }

            device.monitor.on("powerState", (stateName) => {
                let powered = stateName == "on";
                if (powered != device.lastState) {
                    console.info("[Cast] \"%s\" power set to %s", device.name, powered);

                    let source = this._rNet.getSource(device.sourceID);
                    // Cast powered on
                    if (powered) {
                        source._onPower(true);
                    }
                    // Cast powered off
                    else {
                        let source = this._rNet.getSource(device.sourceID);
                        source._onPower(false);
                        source.setDescriptiveText(null);
                        source.setMediaMetadata(null, null, null);
                    }
                }
                device.lastState = powered;
            })
            .on("media", (media) => {
                let source = this._rNet.getSource(device.sourceID);

                // We call _onPower here too because powerState is unreliable
                source._onPower(true);

                let artworkURL = null;
                if (media.images.length > 0)
                    artworkURL = media.images[0].url;

                source.setMediaMetadata(media.title, media.artist, artworkURL);
                source.setDescriptiveText(device.monitor.application);

                // TODO Temporary descriptive text of track
            });

            // Bridge source control
            device.controlListener = (operation, rNetTriggered) => {
                switch (operation) {
                    case Source.CONTROL_PLAY:
                        device.monitor.playDevice();
                        break;
                    case Source.CONTROL_PAUSE:
                        device.monitor.pauseDevice();
                        break;
                    case Source.CONTROL_STOP:
                        device.monitor.stopDevice();
                        break;
                    case Source.CONTROL_NEXT:
                        if (device.monitor.skipDevice) {
                            device.monitor.skipDevice();
                        }
                        else {
                            console.warn("Cast Monitor hasn't been patched with skip and rewind.");
                        }
                        break;
                    case Source.CONTROL_PREV:
                        if (device.monitor.rewindDevice) {
                            device.monitor.rewindDevice();
                        }
                        else {
                            console.warn("Cast Monitor hasn't been patched with skip and rewind.");
                        }
                        break;
                }
            }
            source.on("control", device.controlListener);

            this._devices.push(device);
            console.info("[Google Cast] Now monitoring \"%s\"", device.name);
        }
        else {
            console.info("[Google Cast] Source #%d was removed or changed during Cast Integration initialization.", sourceID);
        }
    }

    seperateSource(sourceID) {
        for (let i in this._devices) {
            let device = this._devices[i];
            if (device.sourceID == sourceID) {
                let source = this._rNet.getSource(device.sourceID);
                if (source) {
                    source.removeListener("control", device.controlListener);
                }
                if (device.monitor.clientConnection) {
                    device.monitor.clientConnection.close();
                }
                this._devices.splice(i, 1);

                console.info("[Google Cast] No longer monitoring \"%s\"", device.name);
                break;
            }
        }
    }

    stop() {
        for (let i in this._devices) {
            let device = this._devices[i];
            let source = this._rNet.getSource(device.sourceID);
            if (source) {
                source.removeListener("control", device.controlListener);
            }
            if (device.monitor.clientConnection) {
                device.monitor.clientConnection.close();
            }
        }

        this._devices = [];

        console.info("[Google Cast] Stopped.");
    }
}

module.exports = GoogleCastIntegration;
