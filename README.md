RNET Pi
===
Using the RS-232 "automation" port on older Russound whole home audio systems, we can control them using a low-power computer such as a Raspberry Pi via a USB to serial adapter in order to retrofit modern day "smart" capabilites. RNET-Pi is a Node.JS server created to act as a proxy between smart devices and these legacy audio systems.

Features
---
- Front-end Android app -- Use your mobile phone or tablet to control your Russound system. ([Google Play](https://play.google.com/store/apps/details?id=me.zachcheatham.rnetremote))
- IFTTT support -- Allows the ability to automate your system using IFTT or utilize assistants such as Google Home or Alexa.
- Volume limit -- Individually limit zones to a maximum volume.
- Chromecast Audio Integration
  - Display currently playing media on wall plate displays.
  - Control Chromecast using existing wall plates.
  - (Configurable) Automatically activate zones and switch to appropriate source when Chromecast begins playing media.
  - (Configurable) Automaticallly turn off zones using a Cast device when media is no longer being played.

### Planned Features
 - Sonos Connect support.
 - Direct integration with Alexa and Google Home opposed to using IFTTT.
 - Web interface

### Supported Systems
In theory, this *should* work with the CAS44, CAA66, CAM6.6, and CAV6.6, but has only been tested with the CAV6.6. If you run into any issues with other devices, feel free to open an issue. The more support, the better.

Installation
---
##### Required Hardware
- [Raspberry Pi](https://www.raspberrypi.org/) or similar device running Linux
*This software most likely will work on Windows or macOS, but it's only been tested on Linux*
- Male USB to male RS-232 adapter ([Amazon](https://www.amazon.com/TRENDnet-Converter-Installation-Universal-TU-S9/dp/B0007T27H8)) *Not a specific recommendation, just an example.*
##### Download and Install
1. Verify your Raspberry Pi is up to date by running:
`sudo apt update && sudo apt upgrade`
2. Install [Node.JS](https://nodejs.org/en/):
`sudo apt install nodejs`
3. Install [forever-service](https://github.com/zapty/forever-service) in order to have RNET Pi run automatically at boot:
`sudo npm install -g forever-service`
4. Download RNET Pi:
`git clone https://gitlab.com/zachcheatham/rnet-pi.git`
`cd rnet-pi`
5. Download and install required libraries:
`npm install`
6. Install RNET Pi to a service for autostarting at boot:
`sudo forever-service install -s ./src/app.js rnet-pi`
##### Configuration
1. Run the server once to generate a config file
`npm start` *Wait for startup to complete* `^C`
2. Determine the device path of the serial adapter.
*The adapter should not be connected at this point!*
   1. Get a current listing of devices:
   `ls /dev/`
   3. Connect the RS232 adapter to the Russound device's serial port and the Pi's USB port.
   4. Get another listing of devices:
   `ls /dev/`
   5. Compare results to determine the newly connected adapter. For example, my adapter is `/dev/tty-usbserial1`
3. Open the configuration file for editing:
`nano config.json`
*These are low level config options that you shouldn't have to ever edit again.*
4. Replace the `serialDevice` property by replacing the existing value `/dev/tty-usbserial1` with the `/dev/` path you determined in step two. There's a good chance your adapter will by the same path.
5. [Advanced Users] Set the address and port you want the server to bind to here. If you don't know why you would change these, you can leave them alone.
6. Save and exit the configuration file by pressing `CTRL+O` followed by `CTRL+X`
##### Start the server
1. If you want to be sure the server starts up successfully, run `npm start` to run the server in your current console. This will close when you log out.
2. If you see `Connected to RNet!` in the terminal, everything is probably working normally. You can now exit `CTRL+C` so we can start the server as a service.
3. Start the server as a service:
`sudo systemctl start rnet-pi`
##### Setup the Zones and Sources
The RNET RS-232 protocol has no zone naming, method of determining which zones and sources have physical connections, or method to retrieve the names of sources. All of that is up to you. Before you can start using this system, you must connect to this newly created server using the [RNET Remote](https://play.google.com/store/apps/details?id=me.zachcheatham.rnetremote) app and add zones and sources.