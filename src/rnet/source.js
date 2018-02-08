const EventEmitter = require("events");

class Source extends EventEmitter {
    constructor(rnet, sourceID, name, type) {
        super();

        this._rNet = rnet;

        this._id = sourceID;
        this._name = name;
        this._type = type;

        this._currentDisplay = null;
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

    setDisplay(displayMessage) {
        this._currentDisplay = displayMessage;
        this.emit("display-message", displayMessage);
    }

    getDisplay() {
        return this._currentDisplay;
    }
}

Source.TYPE_GENERIC = "generic";
Source.TYPE_CAST = "cast";

module.exports = Source;
