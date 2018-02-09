const EventEmitter = require("events");

class Source extends EventEmitter {
    constructor(rnet, sourceID, name, type) {
        super();

        this._rNet = rnet;

        this._id = sourceID;
        this._name = name;
        this._type = type;

        this._descriptiveText = null;
        this._descriptiveTextFromRNet = true;
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

    isCast() {
        return this._type == Source.TYPE_CAST;
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
}

Source.TYPE_GENERIC = "generic";
Source.TYPE_CAST = "cast";

module.exports = Source;
