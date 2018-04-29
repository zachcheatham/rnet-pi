const EventEmitter = require("events");
const CastClient = require("castv2").Client;
const mdns = require("mdns-js");

const Source = require("../rnet/source");

var requestID = 1;

class GoogleCastIntegration {
    constructor(rNet) {
        this._rNet = rNet;
        this._sources = [];
        this._castMonitor = new GoogleCastMonitor();

        let sources = rNet.getSourcesByType(Source.TYPE_GOOGLE_CAST);
        for (let source of sources) {
            this.integrateSource(source);
        }

        this._rNet.on("new-source", (source) => {
            if (source.getType() == Source.TYPE_GOOGLE_CAST) {
                this.integrateSource(source);
            }
        });

        this._rNet.on("source-name", (source, name, oldName) => {
            if (source.getType() == Source.TYPE_GOOGLE_CAST) {
                this.removeSource(source, oldName);
                this.integrateSource(source);
            }
        });

        this._rNet.on("source-type", (source, type) => {
            if (type == Source.TYPE_GOOGLE_CAST) {
                this.integrateSource(source);
            }
            else {
                this.removeSource(source);
            }
        });

        this._rNet.on("source-deleted", (sourceID) => {
            this._castMonitor.unregisterCast(source.getName());
        });

        this._castMonitor.on("application", (castName, application) => {
            if (castName in this._sources) {
                const source = this._rNet.getSource(this._sources[castName].id);
                if (application) {
                    source._onPower(true);
                    source.setDescriptiveText(application, 0);
                }
                else {
                    source._onPower(false);
                    source.setDescriptiveText(null, 0);
                }
            }
        });

        this._castMonitor.on("media", (castName, title, artist, artworkURL) => {
            if (castName in this._sources) {
                const source = this._rNet.getSource(this._sources[castName].id);
                source.setMediaMetadata(title, artist, artworkURL);
            }
        });

        this._castMonitor.on("playing", (castName, playing) => {
            if (castName in this._sources) {
                const source = this._rNet.getSource(this._sources[castName].id);
                source.setMediaPlayState(playing);
            }
        });

        console.info("[Google Cast] Ready.");
    }

    integrateSource(source) {
        const sourceInfo = {
            "id": source.getSourceID()
        }

        const castName = source.getName();

        // Bridge source control
        sourceInfo.controlListener = (operation, rNetTriggered) => {
            switch (operation) {
                case Source.CONTROL_PLAY:
                    this._castMonitor.play(castName);
                    break;
                case Source.CONTROL_PAUSE:
                    this._castMonitor.pause(castName)
                    break;
                case Source.CONTROL_STOP:
                    this._castMonitor.stop(castName);
                    break;
                case Source.CONTROL_NEXT:
                    this._castMonitor.skipTrack(castName);
                    break;
                case Source.CONTROL_PREV:
                    this._castMonitor.rewindTrack(castName);
                    break;
            }
        }
        source.on("control", sourceInfo.controlListener);

        this._castMonitor.registerCast(castName);
        this._sources[castName] = sourceInfo;
    }

    removeSource(source, byName=false) {
        let name = (byName ? byName : source.getName());
        const sourceInfo = this._sources[name];
        source.removeListener("control", sourceInfo.controlListener);
        delete this._sources[name];

        this._castMonitor.unregisterCast(name);
    }

    stop() {
        for (let sourceInfo of Object.values(this._sources)) {
            let source = this._rNet.getSource(sourceInfo.id);
            if (source) {
                source.removeListener("control", sourceInfo.controlListener);
                this._castMonitor.unregisterCast(source.getName());
            }
            else {
                console.warn("WARNING: Unable to find source during cast integration shutdown!");
            }
        }
        this._sources = [];
        this._castMonitor.removeAllListeners();
        this._castMonitor = null;
        console.info("[Google Cast] Finished.");
    }
}

class GoogleCastMonitor extends EventEmitter {
    constructor() {
        super();
        this._knownCasts = {};
        this._registeredCasts = {};
    }

    start() {
        this._browser = mdns.createBrowser("_googlecast._tcp");
        this._browser.on("ready", () => {
            this._browser.discover();
        });
        this._browser.on("update", (service) => {
            if (service.txt && service.addresses) {
                let id = null;
                let name = null;
                for (let txtRecord of service.txt) {
                    if (txtRecord.includes("fn=")) {
                        name = txtRecord.replace("fn=", "");
                    }
                    else if (txtRecord.includes("id=")) {
                        id = txtRecord.replace("id=", "");
                    }
                }
                if (id && name) {
                    if (id in this._knownCasts) {
                        if (this._knownCasts[id].name != name) {
                            this._disconnectCast(this._knownCasts[id].name);
                        }
                        else if (this._knownCasts[id].address != service.addresses[0]) {
                            this._disconnectCast(name);
                            this._connectCast(name);
                        }
                    }
                    else {
                        this._knownCasts[id] = {
                            "name": name,
                            "address": service.addresses[0]
                        }

                        if (name in this._registeredCasts) {
                            this._connectCast(name);
                        }
                    }
                }
            }
        });

        console.info("[Google Cast Monitor] Started.");
    }

    stop() {
        this._browser.stop();
        this._browser = undefined;
        console.info("[Google Cast Monitor] Stopped.");
    }

    registerCast(castName) {
        if (!(castName in this._registeredCasts)) {
            this._registeredCasts[castName] = false;
            if (Object.keys(this._registeredCasts).length == 1) {
                this.start();
            }
            else {
                this._connectCast(castName);
            }
        }
    }

    unregisterCast(castName) {
        if (castName in this._registeredCasts) {
            this._disconnectCast(castName);
            delete this._registeredCasts[castName];
            if (Object.keys(this._registeredCasts).length == 0) {
                this.stop();
            }
        }
    }

    _connectCast(castName) {
        console.info("[Google Cast Monitor] Connected to", castName);
        if (castName in this._registeredCasts) {
            if (!this._registeredCasts[castName]) {
                for (let castInfo of Object.values(this._knownCasts)) {
                    if (castInfo.name === castName) {
                        let cast = new Cast(castInfo.address);

                        cast.on("application", (application) => {
                            this.emit("application", castName, application);
                        });
                        cast.on("playing", (playing) => {
                            this.emit("playing", castName, playing);
                        });
                        cast.on("media", (title, artist, artworkURL) => {
                            this.emit("media", castName, title, artist, artworkURL);
                        });

                        this._registeredCasts[castName] = cast;
                        break;
                    }
                }
            }
        }
    }

    _disconnectCast(castName) {
        if (castName in this._registeredCasts) {
            if (this._registeredCasts[castName]) {
                this._registeredCasts[castName].removeAllListeners();
                this._registeredCasts[castName].close();
                console.info("[Google Cast Monitor] Disconnected from", castName);
            }
        }
    }

    play(castName) {
        if (castName in this._registeredCasts) {
            const cast = this._registeredCasts[castName];
            if (cast && cast._mediaConnection && !cast._playing) {
                cast._mediaConnection.send({
                    type: "PLAY",
                    mediaSessionId: cast._mediaSessionId,
                    requestId: requestID++,
                });
            }
        }
    }

    pause(castName) {
        if (castName in this._registeredCasts) {
            const cast = this._registeredCasts[castName];
            if (cast && cast._mediaConnection && cast._playing) {
                cast._mediaConnection.send({
                    type: "PAUSE",
                    mediaSessionId: cast._mediaSessionId,
                    requestId: requestID++,
                });
            }
        }
    }

    stop(castName) {
        if (castName in this._registeredCasts) {
            const cast = this._registeredCasts[castName];
            if (cast && cast._mediaConnection) {
                cast._mediaConnection.send({
                    type: "STOP",
                    mediaSessionId: cast._mediaSessionId,
                    requestId: requestID++,
                });
            }
        }
    }

    skipTrack(castName) {
        if (castName in this._registeredCasts) {
            const cast = this._registeredCasts[castName];
            if (cast && cast._mediaConnection && cast._duration > 0) {
                cast._mediaConnection.send({
                    type: "SEEK",
                    currentTime: cast._duration,
                    mediaSessionId: cast._mediaSessionId,
                    requestId: requestID++,
                });
            }
        }
    }

    rewindTrack(castName) {
        if (castName in this._registeredCasts) {
            const cast = this._registeredCasts[castName];
            if (cast && cast._mediaConnection && cast._duration > 0) {
                cast._mediaConnection.send({
                    type: "SEEK",
                    currentTime: 0,
                    mediaSessionId: cast._mediaSessionId,
                    requestId: requestID++,
                });
            }
        }
    }
}

class Cast extends EventEmitter {
    constructor(address) {
        super();
        this._address = address;
        this._active = false;
        this._playing = false;
        this._application = null;
        this._title = null;
        this._artist = null;
        this._artworkUrl = null;
        this.connect();
    }

    connect() {
        if (!this._active) {
            this._active = true;
            this._client = new CastClient();

            this._client.on("error", (error) => {
                this.close();
                this._reconnectTimer = setTimeout(() => this.connect(), 5000);
            });

            this._client.connect(this._address, () => {
                this._connection = this._client.createChannel("sender-0", "receiver-0", "urn:x-cast:com.google.cast.tp.connection", "JSON");
                this._heartbeat = this._client.createChannel("sender-0", "receiver-0", "urn:x-cast:com.google.cast.heartbeat", "JSON");
                this._receiver = this._client.createChannel("sender-0", "receiver-0", "urn:x-cast:com.google.cast.receiver", "JSON");

                this._connection.send({type: "CONNECT"});
                this._connection.on("message", (data) => {
                    if (data.type === "CLOSE") {
                        this.close(false);
                        this._reconnectTimer = setTimeout(() => this.connect(), 5000);
                    }
                });

                this._heartbeatInterval = setInterval(() => {
                    this._heartbeat.send({type: "PING"});
                }, 5000);

                this._receiver.send({
                    type: "GET_STATUS",
                    requestId: requestID++,
                });
                this._receiver.on("message", (data) => {
                    if (data.status.applications) {
                        if (!data.status.applications[0].isIdleScreen) {
                            let applicationName = data.status.applications[0].displayName;
                            let transportID = data.status.applications[0].transportId;

                            if (this._application != applicationName) {
                                this._application = applicationName;
                                this.emit("application", this._application);
                            }

                            if (transportID !== this._transportID) {
                                if (this._mediaStateConnection) {
                                    this._closeMediaConnection();
                                }
                                this._openMediaConnection(transportID);
                            }
                        }
                        else {
                            if (this._application) {
                                this._application = null;
                                this.emit("application", null);
                            }
                            this._transportID = null;
                            if (this._mediaStatusConnection) {
                                this._closeMediaConnection();
                            }
                        }
                    }
                })
            });
        }
    }

    close(stopReconnect=true) {
        if (this._active) {
            this._active = false;
            clearInterval(this._heartbeatInterval);
            clearTimeout(this._reconnectTimer);
            this._closeMediaConnection();
            this._transportID = null;
            this._connection.close();
            this._connection.removeAllListeners();
            this._connection = null;
            this._heartbeat.close();
            this._heartbeat.removeAllListeners();
            this._heartbeat = null;
            this._receiver.close();
            this._receiver.removeAllListeners();
            this._receiver = null;
            this._client.removeAllListeners();
            this._client.close();
        }
    }

    _openMediaConnection(transportID) {
        if (!this._mediaStateConnection) {
            this._transportID = transportID;
            this._mediaStateConnection = this._client.createChannel("client-17558", transportID, "urn:x-cast:com.google.cast.tp.connection", "JSON");
            this._mediaConnection = this._client.createChannel("client-17558", transportID, "urn:x-cast:com.google.cast.media", "JSON");

            this._mediaStateConnection.send({type: "CONNECT"});
            this._mediaStateConnection.on("message", (data) => {
                if (data.type === "CLOSE") {
                    this._closeMediaConnection();
                }
            });

            this._mediaConnection.send({
                "type": "GET_STATUS",
                requestId: requestID++
            });
            this._mediaConnection.on("message", (message) => {
                if (message.type === "MEDIA_STATUS") {
                    let status = message.status[0];
                    if (status) {
                        this._mediaSessionId = status.mediaSessionId;
                        let playing = (status.playerState == "PLAYING" || status.playerState == "BUFFERING");

                        if (this._playing != playing) {
                            this._playing = playing;
                            this.emit("playing", playing);
                        }

                        if (status.media && status.media.metadata) {
                            let title = null;
                            let artist = null;
                            let artworkURL = null;

                            if (status.media.metadata.title)
                                title = status.media.metadata.title;
                            if (status.media.metadata.artist)
                                artist = status.media.metadata.artist;
                            if (status.media.metadata.images)
                                artworkURL = status.media.metadata.images[0].url;
                            this._duration = status.media.metadata.duration;

                            if (this._title != title || this._artist != artist || this._artworkUrl != artworkURL) {
                                this._title = title;
                                this._artist = artist;
                                this._artworkUrl = artworkURL;
                                this.emit("media", title, artist, artworkURL);
                            }
                        }
                    }
                }
            });
        }
        else {
            console.warn("PROGRAMMING ERROR: ATTEMPTED TO OPEN MEDIA CONNECTION TWICE");
        }
    }

    _closeMediaConnection() {
        if (this._mediaStateConnection) {
            this._mediaConnection.close();
            this._mediaConnection.removeAllListeners();
            this._mediaConnection = null;
            this._mediaStateConnection.close();
            this._mediaStateConnection.removeAllListeners();
            this._mediaStateConnection = null;
            this._mediaControlConnection = null;
            this._mediaSessionId = null;
            this._duration = 0;

            if (this._application) {
                this._application = null;
                this.emit("application", null);
            }
            if (this._title || this._artist || this._artworkUrl) {
                this._title = null;
                this._artist = null;
                this._artworkUrl = null;
                this.emit("media", null, null, null);
            }
            if (this._playing) {
                this._playing = false;
                this.emit("playing", false);
            }
        }
    }
}

module.exports = GoogleCastIntegration;
