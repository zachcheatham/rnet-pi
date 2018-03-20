const { exec } = require("child_process")
const packageJSON = require("../package.json");

const UPDATE_CHECK_FREQUENCY = 86400;

var lastUpdateCheck = 0;
var updateAvaiable = false;
var latestPackageJSON = null;
var updating = false;

function time() {
    return Math.floor(new Date() / 1000);
}

module.exports = {
    currentVersion: packageJSON.version,
    checkForUpdates: function(callback) {
        if (updateAvaiable !== false) {
            callback(latestPackageJSON.version, packageJSON.version);
        }
        else if (time() - lastUpdateCheck >= UPDATE_CHECK_FREQUENCY) {
            console.info("Checking for updates...");
            exec("git fetch", (err, stdout, stderr) => {
                if (err === null) {
                    exec("git show origin/master:package.json", (err, stdout, stderr) => {
                        if (err === null) {
                            let remotePackageJSON = JSON.parse(stdout);
                            lastUpdateCheck = time();
                            if (remotePackageJSON.version != packageJSON.version) {
                                updateAvaiable = true;
                                latestPackageJSON = remotePackageJSON;
                                console.info("Update %s -> %s avaiable!", packageJSON.version, remotePackageJSON.version);
                                callback(remotePackageJSON.version, packageJSON.version)
                            }
                            else {
                                console.info("No updates are available.");
                            }
                        }
                        else {
                            console.warn("Failed to check for updates: git show failed: %s", stderr);
                        }
                    });
                }
                else {
                    console.warn("Failed to check for updates: git fetch failed: %s", stderr);
                }
            });
        }
    },
    update: function() {
        if (updateAvaiable !== false) {
            if (!updating) {
                updating = true;
                exec("git pull", (err, stdout, stderr) => {
                    if (err === null) {
                        if (stdout.indexOf("Updating") > -1) {
                            // TODO check if package dependencies changed and run npm install
                            console.info("Update complete. Exiting...");
                            process.exit();
                        }
                        else {
                            console.warn("Update failed: git pull failed:\n%s", stdout);
                        }
                    }
                    else {
                        console.warn("Update failed: git pull failed: %s", stderr);
                        updating = false;
                    }
                });
            }
        }
        else {
            console.warn("update() called while update is not avaiable.");
        }
    }
}
