const Source = require("./rnet/source");
const patchCastMonitor = require("./util/patch").castMonitor;

class CastIntegration {
    constructor(rNet) {
        this.rNet = rNet;
        this._castDevices = rNet.getCastSources();

        for (let i in this._castDevices)
        {
            const device = this._castDevices[i];

            // Bridge source control
            const source = this.rNet.getSource(device.sourceID);
            source.on("control", (operation, rNetTriggered) => {
                const mon = device.monitor;
                switch (operation) {
                    case Source.CONTROL_PLAY:
                        mon.playDevice();
                        break;
                    case Source.CONTROL_PAUSE:
                        mon.pauseDevice();
                        break;
                    case Source.CONTROL_STOP:
                        mon.stopDevice();
                        break;
                    case Source.CONTROL_NEXT:
                        if (mon.skipDevice) {
                            mon.skipDevice();
                        }
                        else {
                            console.warn("Cast Monitor hasn't been patched with skip and rewind.");
                        }
                        break;
                    case Source.CONTROL_PREV:
                        if (mon.rewindDevice) {
                            mon.rewindDevice();
                        }
                        else {
                            console.warn("Cast Monitor hasn't been patched with skip and rewind.");
                        }
                        break;
                }
            });
        }

        console.info("Google Cast integration enabled.");
    }

    start() {
        patchCastMonitor(() => {
            const CastDeviceMonitor = require("castv2-device-monitor").DeviceMonitor

            for (let i in this._castDevices) {
                let device = this._castDevices[i];
                device.monitor = new CastDeviceMonitor(device.name);
                device.lastState = false;

                device.monitor.on("powerState", (stateName) => {
                    let powered = stateName == "on";
                    if (powered != device.lastState) {
                        console.info("[Cast] \"%s\" power set to %s", device.name, powered);
                        // Cast powered on
                        if (powered) {
                            source._onPower(true);
                        }
                        // Cast powered off
                        else {
                            let source = this.rNet.getSource(device.sourceID);
                            source._onPower(false);
                            source.setDescriptiveText(null);
                            source.setMediaMetadata(null, null, null);
                        }
                    }
                    device.lastState = powered;
                })
                .on("media", (media) => {
                    // We call _onPower here too because powerState is unreliable
                    source._onPower(true);

                    let artworkURL = null;
                    if (media.images.length > 0)
                        artworkURL = media.images[0].url;

                    let source = this.rNet.getSource(device.sourceID);
                    source.setMediaMetadata(media.title, media.artist, artworkURL);
                    source.setDescriptiveText(device.monitor.application);

                    // TODO Temporary descriptive text of track
                });

                console.info("[Cast] Connected to \"%s\"", device.name);
            }
        });
    }

    stop() {
        for (let i in this._castDevices) {
            this._castDevices[i].monitor.close();
        }
    }
}

module.exports = CastIntegration
