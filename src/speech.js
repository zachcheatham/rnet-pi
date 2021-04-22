const say = require("say")

class Speach {
  constructor(rNet, audioSource) {
      this._rNet = rNet;
  }

  speak(ctrllrID, zoneID, text) {
      savedSources = {}
      if (ctrllrID == -1) {
          for (let ctrllrID = 0; ctrllrID < this._rNet.getControllersSize(); ctrllrID++) {
              for (let zoneID = 0; zoneID < this._rNet.getZonesSize(ctrllrID); zoneID++) {
                  let zone = this._rNet.getZone(ctrllrID, zoneID);
                  if (!(ctrllrID in savedSources)) {
                      savedSources[ctrllrID] = {}
                  }
                  if (zone && zone.getSourceID() == this._id && zone.getPower()) {
                      return true;
                  }
              }
          }
      }


      say.speak(text);
  }
}
