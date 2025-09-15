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

## C# .NET Installation (Recommended)

The RNet-Pi project has been ported to modern C# .NET 8.0 for improved performance, better cross-platform support, and enhanced maintainability. This is the recommended deployment method for new installations.

### Supported Raspberry Pi Models

- **Raspberry Pi 2 Model B Rev 1.1** - Raspbian 12 (Bookworm) - 32-bit ARM
- **Raspberry Pi 5** - 64-bit OS - 64-bit ARM
- Other Linux ARM devices (generic linux-arm/linux-arm64 builds)

### Quick Start

For detailed deployment instructions specific to your Raspberry Pi model, see:
📖 **[Raspberry Pi Deployment Guide](docs/RASPBERRY_PI_DEPLOYMENT.md)**

### Building from Source

#### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

#### Build Commands

```bash
# Clone repository
git clone https://github.com/mmackelprang/rnet-pi.git
cd rnet-pi

# Build for specific Raspberry Pi models
make build-pi2    # Raspberry Pi 2 Model B Rev 1.1
make build-pi5    # Raspberry Pi 5 (64-bit)
make build-all    # Both architectures

# Alternative: Use build scripts directly
./scripts/build-raspberry-pi.sh --arch pi2
./scripts/build-raspberry-pi.sh --arch pi5
./scripts/build-raspberry-pi.sh --arch all

# Self-contained builds (includes .NET runtime)
make build-pi2-sc
make build-pi5-sc
```

#### Available Build Targets

| Target | Description | Runtime ID |
|--------|-------------|------------|
| `pi2` | Raspberry Pi 2 Model B Rev 1.1, Raspbian 12 - Bookworm | `linux-arm` |
| `pi5` | Raspberry Pi 5 with 64-bit OS | `linux-arm64` |
| `all` | Build for both architectures | Both |

Build artifacts are generated in the `dist/` directory with complete deployment packages including installation scripts.

### Prerequisites
- [Raspberry Pi](https://www.raspberrypi.org/) or compatible Linux device
- .NET 8.0 Runtime (automatically configured by deployment scripts)
- Male USB to male RS-232 adapter

### Installation Steps

**For Raspberry Pi 2 Model B Rev 1.1 or Raspberry Pi 5, use the optimized deployment packages:**

See the **[Raspberry Pi Deployment Guide](docs/RASPBERRY_PI_DEPLOYMENT.md)** for complete instructions.

**For generic Linux installations or development:**

#### 1. Update Your System
```bash
sudo apt update && sudo apt upgrade -y
```

#### 2. Install .NET 8.0
```bash
# Add Microsoft package repository
curl -sSL https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
sudo apt-add-repository https://packages.microsoft.com/debian/11/prod

# Install .NET 8.0 Runtime (or SDK for development)
sudo apt update
sudo apt install -y dotnet-runtime-8.0

# Verify installation
dotnet --version
```

#### 3. Clone and Build RNet-Pi
```bash
# Clone the repository
git clone https://github.com/mmackelprang/rnet-pi.git
cd rnet-pi

# Build the application
dotnet build --configuration Release

# Publish for deployment
dotnet publish src/RNetPi.API/RNetPi.API.csproj -c Release -o /opt/rnet-pi
```

#### 4. Configure Serial Device
Determine your USB-to-RS232 adapter device path:
```bash
# Before connecting adapter
ls /dev/tty*

# Connect the adapter and list again
ls /dev/tty*

# Look for new device (typically /dev/ttyUSB0 or /dev/tty-usbserial1)
```

#### 5. Create Configuration
Create initial configuration file:
```bash
sudo mkdir -p /etc/rnet-pi
sudo tee /etc/rnet-pi/config.json > /dev/null <<EOF
{
  "serverName": "RNet Controller",
  "serverHost": null,
  "serverPort": 3000,
  "webHost": null,
  "webPort": null,
  "serialDevice": "/dev/ttyUSB0",
  "webHookPassword": "your-secure-password-here",
  "simulate": false
}
EOF
```

#### 6. Create Systemd Service
Create a systemd service for automatic startup:
```bash
sudo tee /etc/systemd/system/rnet-pi.service > /dev/null <<EOF
[Unit]
Description=RNet-Pi Audio Controller
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /opt/rnet-pi/RNetPi.API.dll
Restart=always
RestartSec=5
User=pi
Group=pi
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:3000
WorkingDirectory=/opt/rnet-pi

# Logging
StandardOutput=journal
StandardError=journal
SyslogIdentifier=rnet-pi

[Install]
WantedBy=multi-user.target
EOF
```

#### 7. Start and Enable Service
```bash
# Set correct permissions
sudo chown -R pi:pi /opt/rnet-pi
sudo chmod +x /opt/rnet-pi/RNetPi.API

# Enable and start the service
sudo systemctl daemon-reload
sudo systemctl enable rnet-pi
sudo systemctl start rnet-pi

# Check status
sudo systemctl status rnet-pi

# View logs
sudo journalctl -u rnet-pi -f
```

### Configuration

The application uses `config.json` for configuration. Key settings include:

- **serverName**: Display name for your RNet controller
- **serverPort**: Port for the main API (default: 3000)
- **serialDevice**: Path to your USB-to-RS232 adapter
- **webHookPassword**: Password for webhook API access
- **simulate**: Set to true for testing without hardware

### API Endpoints

The C# version exposes RESTful API endpoints:

#### Global Controls
- `PUT /api/webhooks/on?pass=yourpassword` - Turn all zones on
- `PUT /api/webhooks/off?pass=yourpassword` - Turn all zones off
- `PUT /api/webhooks/mute?pass=yourpassword` - Mute all zones
- `PUT /api/webhooks/unmute?pass=yourpassword` - Unmute all zones

#### Zone-Specific Controls
- `PUT /api/webhooks/{zoneName}/on?pass=yourpassword` - Turn specific zone on
- `PUT /api/webhooks/{zoneName}/off?pass=yourpassword` - Turn specific zone off
- `PUT /api/webhooks/{zoneName}/volume/{level}?pass=yourpassword` - Set volume (0-100)
- `PUT /api/webhooks/{zoneName}/mute?pass=yourpassword` - Mute specific zone
- `PUT /api/webhooks/{zoneName}/unmute?pass=yourpassword` - Unmute specific zone
- `PUT /api/webhooks/{zoneName}/source/{sourceName}?pass=yourpassword` - Set source

### Swagger API Documentation

The C# version includes Swagger documentation available at:
`http://your-pi-ip:3000/swagger`

### Troubleshooting

#### View Service Logs
```bash
# Real-time logs
sudo journalctl -u rnet-pi -f

# Recent logs
sudo journalctl -u rnet-pi --since "1 hour ago"
```

#### Check Serial Device Permissions
```bash
# Add user to dialout group for serial access
sudo usermod -a -G dialout pi

# Check device permissions
ls -la /dev/ttyUSB0
```

#### Test Configuration
```bash
# Test configuration loading
sudo -u pi dotnet /opt/rnet-pi/RNetPi.API.dll --urls="http://localhost:3001"
```

#### Service Management
```bash
# Restart service
sudo systemctl restart rnet-pi

# Stop service
sudo systemctl stop rnet-pi

# Disable autostart
sudo systemctl disable rnet-pi
```