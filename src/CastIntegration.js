const Source = require("./rnet/source");
const patchCastMonitor = require("./util/patch").castMonitor;

class CastIntegration {
    constructor(rNet, config) {
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

            let automationConfig = config.get("cast_automation");
            if (automationConfig != null) {
                if (device.name in automationConfig) {
                    device.triggerZones = automationConfig[device.name].zones;
                    device.idleTimeout = automationConfig[device.name].timeout * 1000;
                }
            }
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
                device.triggeredZones = false;

                device.monitor.on("powerState", (stateName) => {
                    let powered = stateName == "on";
                    if (powered != device.lastState) {
                        console.info("[Cast] \"%s\" power set to %s", device.name, powered);
                        // Cast powered on
                        if (powered) {
                            // Interrupt the timer for power down
                            if ("idleTimer" in device) {
                                clearTimeout(device.idleTimer);
                                delete device.idleTimer;
                            }

                            // Only turn on trigger zones if no other zone is playing it
                            if ("triggerZones" in device && !this.rNet.zonePlayingSource(device.sourceID)) {
                                // Turn on the default zones
                                for (i in device.triggerZones) {
                                    let zone = this.rNet.getZone(device.triggerZones[i][0], device.triggerZones[i][1]);
                                    zone.setPower(true);
                                }
                                device.triggeredZones = true;
                            }
                        }
                        // Cast powered off
                        else {
                            let source = this.rNet.getSource(device.sourceID);
                            source.setDescriptiveText(null);
                            source.setMediaMetadata(null, null, null);

                            if (device.triggeredZones) {
                                // Wait the configured time to shut off zones
                                device.idleTimer = setTimeout(() => {
                                    // Shut off all zones running the cast source
                                    for (let zone in this.rNet.getZonesPlayingSource)
                                    for (let ctrllrID = 0; ctrllrID < this.rNet.getControllersSize(); ctrllrID++) {
                                        for (let zoneID = 0; zoneID < this.rNet.getZonesSize(ctrllrID); zoneID++) {
                                            let zone = this.rNet.getZone(ctrllrID, zoneID);
                                            if (zone != null && zone.getSourceID() == device.sourceID) {
                                                zone.setPower(false);
                                            }
                                        }
                                    }
                                    device.triggeredZones = false;
                                }, device.idleTimeout);
                            }
                        }
                    }
                    device.lastState = powered;
                })
                .on("media", (media) => {
                    // Only turn on trigger zones if no other zone is playing it
                    if (!this.rNet.zonePlayingSource(device.sourceID)) {
                        // Turn on the default zones
                        for (i in device.triggerZones) {
                            let zone = this.rNet.getZone(device.triggerZones[i][0], device.triggerZones[i][1]);
                            zone.setPower(true);
                        }
                        device.triggeredZones = true;
                    }

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
