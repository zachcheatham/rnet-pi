# RNET-Pi Raspberry Pi Deployment Guide

This guide provides complete instructions for deploying RNET-Pi on specific Raspberry Pi models with their optimized builds.

## Supported Raspberry Pi Models

### Raspberry Pi 2 Model B Rev 1.1
- **OS**: Raspbian 12 - Bookworm (32-bit)
- **Architecture**: ARMv7 (32-bit)
- **Runtime Identifier**: `linux-arm`
- **Memory**: 1GB RAM
- **CPU**: 900MHz quad-core ARM Cortex-A7

### Raspberry Pi 5 
- **OS**: Raspberry Pi OS 64-bit
- **Architecture**: ARMv8 (64-bit)
- **Runtime Identifier**: `linux-arm64`
- **Memory**: 4GB/8GB RAM
- **CPU**: 2.4GHz quad-core ARM Cortex-A76

## Prerequisites

### System Requirements

**For both models:**
- Fresh installation of the respective OS
- Internet connectivity for initial setup
- USB-to-RS232 adapter for Russound hardware connection
- SSH access (recommended) or direct console access

**Minimum free disk space:**
- Standard deployment: 200MB
- Self-contained deployment: 500MB

### Hardware Setup

1. **Serial Adapter Connection**
   - Connect USB-to-RS232 adapter to Raspberry Pi USB port
   - Connect RS-232 end to Russound system's automation port
   - Note the device path (usually `/dev/ttyUSB0` or `/dev/ttyAMA0`)

2. **Network Configuration**
   - Ensure Raspberry Pi has internet access
   - Configure static IP (recommended for stable access)

## Build Process

### Option 1: Using Pre-built Packages (Recommended)

If you have access to pre-built deployment packages:

1. Download the appropriate package for your Raspberry Pi model
2. Extract to a temporary directory
3. Follow the installation steps in the package README

### Option 2: Building from Source

#### On Development Machine

**Prerequisites on build machine:**
- .NET 8.0 SDK
- Git

**Build commands:**

```bash
# Clone repository
git clone https://github.com/mmackelprang/rnet-pi.git
cd rnet-pi

# Build for Raspberry Pi 2
make build-pi2

# Build for Raspberry Pi 5
make build-pi5

# Build for both architectures
make build-all

# Build with self-contained runtime (no .NET runtime required on Pi)
make build-pi2-sc
make build-pi5-sc
```

**Alternative using scripts directly:**

```bash
# Raspberry Pi 2
./scripts/build-raspberry-pi.sh --arch pi2

# Raspberry Pi 5
./scripts/build-raspberry-pi.sh --arch pi5

# Both architectures
./scripts/build-raspberry-pi.sh --arch all

# Self-contained builds
./scripts/build-raspberry-pi.sh --arch pi2 --self-contained
./scripts/build-raspberry-pi.sh --arch pi5 --self-contained
```

The build output will be in the `dist/` directory:
```
dist/
├── pi2/          # Raspberry Pi 2 builds
│   ├── RNetPi.API/
│   ├── RNetPi.Console/
│   ├── scripts/
│   └── README.md
└── pi5/          # Raspberry Pi 5 builds
    ├── RNetPi.API/
    ├── RNetPi.Console/
    ├── scripts/
    └── README.md
```

## Deployment Instructions

### Raspberry Pi 2 Model B Rev 1.1 (Raspbian 12 - Bookworm)

#### 1. Prepare the Raspberry Pi

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install .NET 8.0 Runtime (32-bit ARM)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --runtime aspnetcore --architecture arm

# Add .NET to PATH
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

# Verify installation
dotnet --version
```

#### 2. Transfer and Install Application

```bash
# Copy deployment package to Pi (adjust paths as needed)
scp -r dist/pi2 pi@your-pi-ip:/home/pi/rnet-pi-deployment

# SSH into Pi
ssh pi@your-pi-ip

# Navigate to deployment directory
cd /home/pi/rnet-pi-deployment

# Run installation script
chmod +x scripts/install.sh
./scripts/install.sh
```

#### 3. Configuration for Pi 2

```bash
# Create/edit configuration
sudo nano /opt/rnet-pi/RNetPi.Console/config.json
```

Sample configuration:
```json
{
  "serverName": "Pi2 RNet Controller",
  "serverHost": null,
  "serverPort": 3000,
  "webHost": null,
  "webPort": 8080,
  "serialDevice": "/dev/ttyUSB0",
  "webHookPassword": "your-secure-password",
  "simulate": false
}
```

#### 4. Start and Verify Service

```bash
# Start service
sudo systemctl start rnet-pi

# Enable auto-start
sudo systemctl enable rnet-pi

# Check status
sudo systemctl status rnet-pi

# View logs
sudo journalctl -u rnet-pi -f
```

### Raspberry Pi 5 (64-bit OS)

#### 1. Prepare the Raspberry Pi 5

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install .NET 8.0 Runtime (64-bit ARM)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --runtime aspnetcore --architecture arm64

# Add .NET to PATH
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

# Verify installation
dotnet --version
```

#### 2. Transfer and Install Application

```bash
# Copy deployment package to Pi 5
scp -r dist/pi5 pi@your-pi5-ip:/home/pi/rnet-pi-deployment

# SSH into Pi 5
ssh pi@your-pi5-ip

# Navigate to deployment directory
cd /home/pi/rnet-pi-deployment

# Run installation script
chmod +x scripts/install.sh
./scripts/install.sh
```

#### 3. Configuration for Pi 5

```bash
# Create/edit configuration
sudo nano /opt/rnet-pi/RNetPi.Console/config.json
```

Sample configuration optimized for Pi 5:
```json
{
  "serverName": "Pi5 RNet Controller",
  "serverHost": null,
  "serverPort": 3000,
  "webHost": null,
  "webPort": 8080,
  "serialDevice": "/dev/ttyUSB0",
  "webHookPassword": "your-secure-password",
  "simulate": false,
  "logging": {
    "logLevel": {
      "default": "Information",
      "RNetPi": "Debug"
    }
  }
}
```

#### 4. Start and Verify Service

```bash
# Start service
sudo systemctl start rnet-pi

# Enable auto-start
sudo systemctl enable rnet-pi

# Check status
sudo systemctl status rnet-pi

# View logs
sudo journalctl -u rnet-pi -f
```

## Performance Optimization

### Raspberry Pi 2 Optimizations

**Memory Management:**
```bash
# Increase swap space if needed
sudo dphys-swapfile swapoff
sudo nano /etc/dphys-swapfile
# Set CONF_SWAPSIZE=512
sudo dphys-swapfile setup
sudo dphys-swapfile swapon
```

**CPU Governor:**
```bash
# Set performance governor for consistent performance
echo 'performance' | sudo tee /sys/devices/system/cpu/cpu*/cpufreq/scaling_governor
```

### Raspberry Pi 5 Optimizations

**Enable all CPU cores:**
```bash
# Pi 5 typically has all cores enabled by default, but verify:
cat /proc/cpuinfo | grep processor
```

**Memory Configuration:**
```bash
# Optional: Increase GPU memory split for headless operation
sudo raspi-config
# Advanced Options -> Memory Split -> Set to 16 (minimum for headless)
```

## Troubleshooting

### Common Issues and Solutions

#### Serial Port Access Denied
```bash
# Check device exists
ls -la /dev/ttyUSB* /dev/ttyAMA*

# Add user to dialout group
sudo usermod -a -G dialout pi

# Check group membership
groups pi

# Reboot if group addition doesn't take effect immediately
sudo reboot
```

#### Service Fails to Start
```bash
# Check detailed logs
sudo journalctl -u rnet-pi --no-pager

# Test manual startup
sudo -u pi dotnet /opt/rnet-pi/RNetPi.Console/RNetPi.Console.dll

# Check .NET runtime
dotnet --info
```

#### Performance Issues on Pi 2
```bash
# Monitor system resources
htop
iotop
free -h

# Check for thermal throttling
vcgencmd measure_temp
vcgencmd get_throttled

# Ensure adequate cooling
```

#### Network Connectivity Issues
```bash
# Test API endpoint
curl http://localhost:3000/health

# Check firewall
sudo ufw status

# Test from external machine
curl http://pi-ip-address:3000/health
```

### Serial Device Troubleshooting

#### Identify Serial Device
```bash
# Before connecting adapter
ls /dev/tty*

# After connecting adapter
ls /dev/tty*

# Check USB devices
lsusb

# Monitor kernel messages
dmesg | tail -20
```

#### Test Serial Communication
```bash
# Install serial tools
sudo apt install minicom

# Test basic connectivity (replace with your device)
sudo minicom -D /dev/ttyUSB0 -b 19200

# Exit minicom: Ctrl+A, then X
```

## Monitoring and Maintenance

### Health Checks

```bash
# Service status
sudo systemctl is-active rnet-pi

# Process monitoring
ps aux | grep RNetPi

# Port listening
ss -tulnp | grep :3000

# Application logs
tail -f /opt/rnet-pi/logs/rnet-pi-*.log
```

### Updates

```bash
# Stop service
sudo systemctl stop rnet-pi

# Backup current installation
sudo cp -r /opt/rnet-pi /opt/rnet-pi.backup.$(date +%Y%m%d)

# Deploy new version (follow deployment steps with new package)

# Start service
sudo systemctl start rnet-pi

# Verify
sudo systemctl status rnet-pi
```

### Automated Monitoring Script

Create `/home/pi/monitor-rnet.sh`:
```bash
#!/bin/bash
# RNET-Pi monitoring script

SERVICE="rnet-pi"
LOG_FILE="/var/log/rnet-monitor.log"

if ! systemctl is-active --quiet $SERVICE; then
    echo "$(date): $SERVICE is not running, attempting restart" >> $LOG_FILE
    systemctl start $SERVICE
    sleep 10
    if systemctl is-active --quiet $SERVICE; then
        echo "$(date): $SERVICE restarted successfully" >> $LOG_FILE
    else
        echo "$(date): Failed to restart $SERVICE" >> $LOG_FILE
    fi
else
    echo "$(date): $SERVICE is running normally" >> $LOG_FILE
fi
```

Add to crontab:
```bash
# Check every 5 minutes
crontab -e
# Add: */5 * * * * /home/pi/monitor-rnet.sh
```

## Security Considerations

### Network Security
```bash
# Configure firewall
sudo ufw allow 22/tcp      # SSH
sudo ufw allow 3000/tcp    # RNET-Pi API
sudo ufw allow 8080/tcp    # Web interface (if used)
sudo ufw enable
```

### Service Security
```bash
# Secure configuration file
sudo chmod 600 /opt/rnet-pi/RNetPi.Console/config.json
sudo chown pi:pi /opt/rnet-pi/RNetPi.Console/config.json

# Regular security updates
sudo apt update && sudo apt upgrade -y
```

## Performance Comparison

| Feature | Raspberry Pi 2 | Raspberry Pi 5 |
|---------|----------------|----------------|
| Boot Time | ~45 seconds | ~25 seconds |
| Service Start | ~15 seconds | ~5 seconds |
| API Response | ~100ms | ~30ms |
| Memory Usage | ~150MB | ~120MB |
| Concurrent Connections | 5-10 | 20+ |

## Support

For issues specific to this deployment:

1. Check the troubleshooting section above
2. Review logs: `sudo journalctl -u rnet-pi -f`
3. Verify configuration: `/opt/rnet-pi/RNetPi.Console/config.json`
4. Test connectivity: `curl http://localhost:3000/health`

For general RNET-Pi support, see the main repository documentation.