const fs = require("fs");

var data = {};

module.exports = {
    read: function() {
        var contents = "";
        try {
            contents = fs.readFileSync("config.json");
        }
        catch (e) {}

        if (contents.length > 0) {
            data = JSON.parse(contents);
        }
        else {
            data = {
                serverName: "Untitled RNet Controller",
                serverHost: false,
                serverPort: 3000,
                serialDevice: "/dev/tty-usbserial1"
            }
        }
    },
    write: function() {
        fs.writeFileSync("config.json", JSON.stringify(data));
    },
    get: function(key) {
        return data[key];
    }
}
