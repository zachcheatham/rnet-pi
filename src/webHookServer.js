const EventEmitter = require("events");
const express = require("express");

class WebHookServer extends EventEmitter {
    constructor(port, password, rNet) {
        super();

        if (!password) {
            console.warn("To use enable Web Hooks, set \"webHookPassword\" in config.json");
            return;
        }

        this._port = port;
        this._app = express();

        this._app.use(function(req, res, next) {
            if (req.query.pass != password) {
                res.sendStatus(401);
                console.warn("[Web Hook] Bad password in request.");
            }
            else {
                next();
            }
        });

        this._app.use(function(req, res, next) {
            if (!rNet.isConnected()) {
                res.type("txt").status(503).send("RNet not connected.");
            }
            else {
                next();
            }
        });

        this._app.put("/on", function(req, res) {
            req.zone.setAllPower(true);
            res.sendStatus(200);
        });

        this._app.put("/off", function(req, res) {
            req.zone.setAllPower(false);
            res.sendStatus(200);
        });

        this._app.put("/mute", function(req, res) {
            req.zone.setAllMute(true, 1000);
            res.sendStatus(200);
        });

        this._app.put("/unmute", function(req, res) {
            req.zone.setAllMute(false, 1000);
            res.sendStatus(200);
        });

        this._app.use("/:zone/*", function(req, res, next) {
            const zone = rNet.findZoneByName(req.params.zone);
            if (zone) {
                req.zone = zone;
                next();
            }
            else {
                console.warn("[Web Hook] Unknown zone " + req.params.zone + ".");
                res.sendStatus(404);
            }
        })

        this._app.put("/:zone/volume/:volume", function(req, res) {
            req.zone.setVolume(Math.floor(parseInt(req.params.volume) / 2) * 2);
            res.sendStatus(200);
        });

        this._app.put("/:zone/source/:source", function(req, res) {
            const sourceID = rNet.findSourceIDByName(req.params.name);
            if (sourceID !== false) {
                req.zone.setSourceID(sourceID);
                res.sendStatus(200);
            }
            else {
                res.sendStatus(404);
            }
        });

        this._app.put("/:zone/mute", function(req, res) {
            req.zone.setMute(true, 1000);
            res.sendStatus(200);
        });

        this._app.put("/:zone/unmute", function(req, res) {
            req.zone.setMute(false, 1000);
            res.sendStatus(200);
        });

        this._app.put("/:zone/on", function(req, res) {
            req.zone.setPower(true);
            res.sendStatus(200);
        });

        this._app.put("/:zone/off", function(req, res) {
            req.zone.setPower(false);
            res.sendStatus(200);
        });
    }

    start() {
        if (this._app) {
            this._app.listen(this._port);
            console.info("Web hook server running on port " + this._port);
        }
    }
}

module.exports = WebHookServer;
