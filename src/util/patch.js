const { exec } = require("child_process");
const os = require("os");
const path = require("path");
const fs = require("fs");

module.exports = {
    castMonitor: function(callback) {
        if (os.type() != "Windows_NT")
        {
            const cwd = path.join(path.dirname(fs.realpathSync(__filename)), "/../..");
            const options = {"cwd": cwd}

            exec("patch -N -p1 < cast-monitor.patch", options, function(error, stdout, stderr) {
                callback();
            });
        }
        else {
            callback();
        }
    }
}
