# RNET-Pi C# .NET Deployment Guide

## Overview

This guide covers deployment options for the RNET-Pi .NET application, from development environments to production deployments on various platforms including Raspberry Pi, Docker containers, and cloud services.

## Prerequisites

### System Requirements
- **.NET 8.0 Runtime**: Latest LTS version
- **Serial Port Access**: USB-to-RS232 adapter for Russound hardware
- **Network Connectivity**: For client connections and smart device integration
- **Minimum Hardware**:
  - RAM: 256MB (512MB recommended)
  - Storage: 100MB available space
  - CPU: ARM32v7+ (Raspberry Pi 2+) or x64

### Supported Platforms
- **Linux**: Ubuntu 20.04+, Debian 11+, Raspberry Pi OS
- **Windows**: Windows 10+, Windows Server 2019+
- **macOS**: macOS 10.15+ (development/testing only)
- **Docker**: Linux containers with .NET 8.0 runtime

## Development Environment Setup

### 1. Install .NET SDK

**Linux (Ubuntu/Debian)**:
```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

**Windows**:
Download and install from: https://dotnet.microsoft.com/download/dotnet/8.0

**macOS**:
```bash
# Using Homebrew
brew install dotnet
```

### 2. Clone and Build

```bash
# Clone repository
git clone https://github.com/your-org/rnet-pi.git
cd rnet-pi

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests (if available)
dotnet test

# Run console application
dotnet run --project src/RNetPi.Console
```

### 3. Development Configuration

Create `config.json` for development:
```json
{
  "ServerName": "Development RNet Controller",
  "ServerHost": "localhost",
  "ServerPort": 3000,
  "WebHost": "localhost", 
  "WebPort": 8080,
  "SerialDevice": "/dev/ttyUSB0",
  "WebHookPassword": "dev-password",
  "Simulate": true
}
```

## Production Deployment

### Option 1: Raspberry Pi Deployment

#### 1.1 Prepare Raspberry Pi

**Install Raspberry Pi OS Lite**:
```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install .NET runtime (ARM32 for Pi 2/3, ARM64 for Pi 4)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --runtime aspnetcore
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

# Install additional dependencies
sudo apt install -y udev
```

#### 1.2 Deploy Application

```bash
# Create application directory
sudo mkdir -p /opt/rnet-pi
sudo chown pi:pi /opt/rnet-pi

# Publish application for ARM
dotnet publish src/RNetPi.Console -c Release -r linux-arm --self-contained false -o /opt/rnet-pi

# Set permissions
chmod +x /opt/rnet-pi/RNetPi.Console
```

#### 1.3 Configure Serial Access

```bash
# Add user to dialout group for serial port access
sudo usermod -a -G dialout pi

# Create udev rule for consistent device naming
sudo tee /etc/udev/rules.d/99-rnet-serial.rules > /dev/null << 'EOF'
SUBSYSTEM=="tty", ATTRS{idVendor}=="0403", ATTRS{idProduct}=="6001", SYMLINK+="rnet-serial"
EOF

# Reload udev rules
sudo udevadm control --reload-rules
```

#### 1.4 Create systemd Service

```bash
# Create service file
sudo tee /etc/systemd/system/rnet-pi.service > /dev/null << 'EOF'
[Unit]
Description=RNET-Pi Audio Controller
After=network.target
StartLimitIntervalSec=0

[Service]
Type=simple
Restart=always
RestartSec=5
User=pi
ExecStart=/opt/rnet-pi/RNetPi.Console
WorkingDirectory=/opt/rnet-pi
Environment=DOTNET_ROOT=/home/pi/.dotnet
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
EOF

# Enable and start service
sudo systemctl daemon-reload
sudo systemctl enable rnet-pi
sudo systemctl start rnet-pi

# Check status
sudo systemctl status rnet-pi
```

#### 1.5 Production Configuration

Create `/opt/rnet-pi/config.json`:
```json
{
  "ServerName": "Living Room Audio Controller",
  "ServerHost": null,
  "ServerPort": 3000,
  "WebHost": null,
  "WebPort": 8080,
  "SerialDevice": "/dev/rnet-serial",
  "WebHookPassword": "your-secure-password-here",
  "Simulate": false
}
```

### Option 2: Docker Deployment

#### 2.1 Create Dockerfile

```dockerfile
# Multi-stage build for optimized container
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/RNetPi.Console/RNetPi.Console.csproj", "RNetPi.Console/"]
COPY ["src/RNetPi.Core/RNetPi.Core.csproj", "RNetPi.Core/"]
COPY ["src/RNetPi.Infrastructure/RNetPi.Infrastructure.csproj", "RNetPi.Infrastructure/"]
COPY ["src/RNetPi.Application/RNetPi.Application.csproj", "RNetPi.Application/"]

# Restore dependencies
RUN dotnet restore "RNetPi.Console/RNetPi.Console.csproj"

# Copy source code
COPY src/ .

# Build and publish
RUN dotnet publish "RNetPi.Console/RNetPi.Console.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

# Install required packages for serial communication
RUN apt-get update && apt-get install -y \
    udev \
    && rm -rf /var/lib/apt/lists/*

# Copy application
COPY --from=build /app/publish .

# Create non-root user
RUN useradd -m -s /bin/bash rnetuser
USER rnetuser

ENTRYPOINT ["dotnet", "RNetPi.Console.dll"]
```

#### 2.2 Build and Run Container

```bash
# Build image
docker build -t rnet-pi:latest .

# Run with serial device access
docker run -d \
  --name rnet-pi \
  --device=/dev/ttyUSB0:/dev/ttyUSB0 \
  --restart unless-stopped \
  -p 3000:3000 \
  -p 8080:8080 \
  -v /opt/rnet-pi/config:/app/config \
  -v /opt/rnet-pi/data:/app/data \
  rnet-pi:latest
```

#### 2.3 Docker Compose Configuration

```yaml
version: '3.8'

services:
  rnet-pi:
    build: .
    container_name: rnet-pi
    restart: unless-stopped
    devices:
      - "/dev/ttyUSB0:/dev/ttyUSB0"
    ports:
      - "3000:3000"
      - "8080:8080"
    volumes:
      - ./config:/app/config
      - ./data:/app/data
      - ./logs:/app/logs
    environment:
      - DOTNET_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
```

### Option 3: Windows Service Deployment

#### 3.1 Install as Windows Service

```powershell
# Publish application
dotnet publish src\RNetPi.Console -c Release -r win-x64 --self-contained true -o C:\RNetPi

# Install as Windows Service using sc command
sc.exe create "RNetPi" binpath="C:\RNetPi\RNetPi.Console.exe" start=auto

# Start service
sc.exe start "RNetPi"
```

#### 3.2 Alternative: Using Windows Service Template

Modify `Program.cs` to support Windows Service hosting:

```csharp
public static async Task Main(string[] args)
{
    var host = Host.CreateDefaultBuilder(args)
        .UseWindowsService() // Enable Windows Service support
        .ConfigureServices((context, services) =>
        {
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IRNetService, RNetService>();
            services.AddHostedService<RNetBackgroundService>();
        })
        .Build();

    await host.RunAsync();
}
```

## Monitoring and Maintenance

### 1. Logging Configuration

Configure structured logging in `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "RNetPi": "Debug"
    },
    "Console": {
      "TimestampFormat": "yyyy-MM-dd HH:mm:ss "
    },
    "File": {
      "Path": "logs/rnet-pi-.log",
      "RollingInterval": "Day",
      "RetainedFileCountLimit": 30
    }
  }
}
```

### 2. Health Checks

Monitor application health:
```bash
# Check service status (systemd)
sudo systemctl status rnet-pi

# View logs
sudo journalctl -u rnet-pi -f

# Check application logs
tail -f /opt/rnet-pi/logs/rnet-pi-$(date +%Y%m%d).log
```

### 3. Performance Monitoring

Monitor system resources:
```bash
# CPU and memory usage
htop

# Disk usage
df -h

# Network connections
ss -tulnp | grep :3000
```

### 4. Backup and Recovery

**Configuration Backup**:
```bash
# Backup configuration and data
sudo tar -czf rnet-pi-backup-$(date +%Y%m%d).tar.gz \
  /opt/rnet-pi/config.json \
  /opt/rnet-pi/zones.json \
  /opt/rnet-pi/sources.json

# Store backup securely
sudo mv rnet-pi-backup-*.tar.gz /backup/location/
```

**Recovery Process**:
```bash
# Stop service
sudo systemctl stop rnet-pi

# Restore from backup
sudo tar -xzf rnet-pi-backup-YYYYMMDD.tar.gz -C /

# Start service
sudo systemctl start rnet-pi
```

## Security Considerations

### 1. Network Security

**Firewall Configuration**:
```bash
# Allow only necessary ports
sudo ufw allow 3000/tcp  # RNet API
sudo ufw allow 8080/tcp  # Web interface
sudo ufw enable
```

**SSL/TLS Configuration**:
- Use reverse proxy (nginx/Apache) for HTTPS termination
- Generate SSL certificates with Let's Encrypt
- Configure HTTP to HTTPS redirection

### 2. Access Control

**Service Account**:
```bash
# Create dedicated service account
sudo useradd -r -s /bin/false rnet-service
sudo chown -R rnet-service:rnet-service /opt/rnet-pi

# Update service file to use service account
sudo systemctl edit rnet-pi
```

**File Permissions**:
```bash
# Secure configuration files
sudo chmod 600 /opt/rnet-pi/config.json
sudo chmod 644 /opt/rnet-pi/*.json
```

### 3. Update Management

**Automated Updates**:
```bash
#!/bin/bash
# Update script: /opt/rnet-pi/update.sh

# Stop service
sudo systemctl stop rnet-pi

# Backup current version
sudo cp -r /opt/rnet-pi /opt/rnet-pi.backup

# Deploy new version
sudo dotnet publish /path/to/source -c Release -o /opt/rnet-pi

# Start service
sudo systemctl start rnet-pi

# Verify health
sleep 10
if sudo systemctl is-active --quiet rnet-pi; then
    echo "Update successful"
    sudo rm -rf /opt/rnet-pi.backup
else
    echo "Update failed, rolling back"
    sudo systemctl stop rnet-pi
    sudo rm -rf /opt/rnet-pi
    sudo mv /opt/rnet-pi.backup /opt/rnet-pi
    sudo systemctl start rnet-pi
fi
```

## Troubleshooting

### Common Issues

**Serial Port Access Denied**:
```bash
# Check device permissions
ls -l /dev/ttyUSB*

# Add user to dialout group
sudo usermod -a -G dialout $USER

# Reboot or logout/login
```

**Service Won't Start**:
```bash
# Check service logs
sudo journalctl -u rnet-pi -n 50

# Check .NET runtime
dotnet --info

# Verify permissions
sudo -u pi dotnet /opt/rnet-pi/RNetPi.Console.dll
```

**High CPU Usage**:
- Check for infinite loops in packet processing
- Monitor serial port data rates
- Review logging verbosity settings

**Memory Leaks**:
- Enable GC logging: `DOTNET_GCStress=1`
- Monitor with dotnet-counters: `dotnet-counters monitor --process-id <pid>`
- Review IDisposable implementations

This deployment guide provides comprehensive instructions for deploying the RNET-Pi .NET application across various environments while maintaining security, reliability, and maintainability.