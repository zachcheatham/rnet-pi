const CastDeviceMonitor = require("castv2-device-monitor").DeviceMonitor
const Source = require("./rnet/source");

class CaseIntegration {
    constructor(rNet, config) {
        this.rNet = rNet;
        this._castDevices = rNet.getCastSources();

        let automationConfig = config.get("cast_automation");

        for (let deviceName in automationConfig) {
            for (let i in this._castDevices) {
                const source = this.rNet.getSource(this._castDevices[i].sourceID);
                source.on("control", (operation, rNetTriggered) => {
                    const mon = this._castDevices[i].monitor;
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
                    }
                })

                if (this._castDevices[i].name == deviceName) {
                    this._castDevices[i].triggerZones = automationConfig[deviceName].zones;
                    this._castDevices[i].idleTimeout = automationConfig[deviceName].timeout * 1000;
                    break;
                }
            }
        }

        console.info("Google Cast integration enabled.");
    }

    start() {
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
                console.log("[Cast] \"%s\" is now playing %s by %s", device.name, media.title, media.artist);

                // Only turn on trigger zones if no other zone is playing it
                if (!this.rNet.zonePlayingSource(device.sourceID)) {
                    // Turn on the default zones
                    for (i in device.triggerZones) {
                        let zone = this.rNet.getZone(device.triggerZones[i][0], device.triggerZones[i][1]);
                        zone.setPower(true);
                    }
                    device.triggeredZones = true;
                }

                // TODO Temporary descriptive text of track
            })
            .on("application", (application) => {
                console.log("[Cast] \"%s\" is now running %s", device.name, application);
                let source = this.rNet.getSource(device.sourceID);
                source.setDescriptiveText(application);
            });

            console.info("[Cast] Monitoring \"%s\"", device.name);
        }
    }

    stop() {
        for (let i in this._castDevices) {
            this._castDevices[i].monitor.close();
        }
    }
}

module.exports = CaseIntegration
