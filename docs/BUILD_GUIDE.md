# RNET-Pi Build Guide

This guide covers building RNET-Pi for different Raspberry Pi architectures.

## Prerequisites

- .NET 8.0 SDK
- Git
- Linux, macOS, or Windows development environment

## Supported Build Targets

| Target | Device | OS | Architecture | Runtime ID |
|--------|--------|----|--------------|-----------:|
| `pi2` | Raspberry Pi 2 Model B Rev 1.1 | Raspbian 12 - Bookworm | ARMv7 (32-bit) | `linux-arm` |
| `pi5` | Raspberry Pi 5 | 64-bit OS | ARMv8 (64-bit) | `linux-arm64` |
| `all` | Both above | - | Multi-arch | Both |

## Quick Start

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
```

## Build Methods

### Method 1: Using Makefile (Recommended)

```bash
# Standard builds (require .NET runtime on target)
make build-pi2          # Raspberry Pi 2
make build-pi5          # Raspberry Pi 5  
make build-all          # Both architectures

# Self-contained builds (include .NET runtime)
make build-pi2-sc       # Raspberry Pi 2 + runtime
make build-pi5-sc       # Raspberry Pi 5 + runtime
make build-all-sc       # Both + runtime

# Development builds
make build              # Local development (Any CPU)
make test               # Run tests
make clean              # Clean build artifacts
```

### Method 2: Using Build Scripts Directly

```bash
# Basic usage
./scripts/build-raspberry-pi.sh --arch pi2
./scripts/build-raspberry-pi.sh --arch pi5
./scripts/build-raspberry-pi.sh --arch all

# With options
./scripts/build-raspberry-pi.sh --arch pi2 --config Debug
./scripts/build-raspberry-pi.sh --arch pi5 --self-contained
./scripts/build-raspberry-pi.sh --arch all --output custom-dist
```

**Script Options:**
- `--arch` : Target architecture (pi2, pi5, all)
- `--config` : Build configuration (Debug, Release) [default: Release]
- `--output` : Output directory [default: ./dist]
- `--self-contained` : Include .NET runtime in output
- `--help` : Show help message

### Method 3: Using .NET CLI Directly

```bash
# Restore dependencies
dotnet restore

# Build for Raspberry Pi 2 (linux-arm)
dotnet publish src/RNetPi.Console -c Release -r linux-arm -o dist/pi2/RNetPi.Console
dotnet publish src/RNetPi.API -c Release -r linux-arm -o dist/pi2/RNetPi.API

# Build for Raspberry Pi 5 (linux-arm64)
dotnet publish src/RNetPi.Console -c Release -r linux-arm64 -o dist/pi5/RNetPi.Console
dotnet publish src/RNetPi.API -c Release -r linux-arm64 -o dist/pi5/RNetPi.API
```

## Build Output

Builds are placed in the `dist/` directory:

```
dist/
├── pi2/                    # Raspberry Pi 2 builds
│   ├── RNetPi.API/        # Web API component
│   ├── RNetPi.Console/    # Console application  
│   ├── scripts/           # Installation scripts
│   └── README.md          # Deployment instructions
└── pi5/                   # Raspberry Pi 5 builds
    ├── RNetPi.API/        # Web API component
    ├── RNetPi.Console/    # Console application
    ├── scripts/           # Installation scripts
    └── README.md          # Deployment instructions
```

Each build includes:
- **Application binaries** for the target architecture
- **Installation scripts** for automated deployment
- **Deployment info** JSON with build metadata
- **Architecture-specific README** with setup instructions

## Build Verification

### Test Build Quality

```bash
# Run tests
make test

# Check build output
ls -la dist/pi2/RNetPi.Console/
ls -la dist/pi5/RNetPi.Console/

# Verify deployment info
cat dist/pi2/RNetPi.Console/deployment-info.json
cat dist/pi5/RNetPi.Console/deployment-info.json
```

### Validate Architecture

```bash
# Check runtime identifier in deployment info
grep runtimeIdentifier dist/pi2/RNetPi.Console/deployment-info.json
grep runtimeIdentifier dist/pi5/RNetPi.Console/deployment-info.json

# Verify binary architecture (on Linux)
file dist/pi2/RNetPi.Console/RNetPi.Console.dll
file dist/pi5/RNetPi.Console/RNetPi.Console.dll
```

## Development Workflow

### Standard Development Cycle

```bash
# Quick development build and test
make dev

# Build specific architecture for testing
make debug-pi2
make debug-pi5

# Clean and rebuild
make clean
make build-all
```

### Continuous Integration

Example CI pipeline steps:

```yaml
- name: Restore dependencies
  run: dotnet restore

- name: Run tests  
  run: dotnet test --verbosity normal

- name: Build all architectures
  run: make build-all

- name: Archive artifacts
  uses: actions/upload-artifact@v3
  with:
    name: rnet-pi-builds
    path: dist/
```

## Troubleshooting

### Common Build Issues

**Missing .NET SDK:**
```bash
# Install .NET 8.0 SDK
# Linux/macOS
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0

# Windows  
# Download from https://dotnet.microsoft.com/download/dotnet/8.0
```

**Build script permission denied:**
```bash
chmod +x scripts/build-raspberry-pi.sh
```

**Disk space issues:**
```bash
# Clean previous builds
make clean

# Check available space
df -h
```

**Package restore failures:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore with verbose logging
dotnet restore --verbosity detailed
```

### Build Optimization

**Faster builds:**
```bash
# Skip self-contained for development
make build-pi2

# Build single architecture
make build-pi5

# Parallel builds (adjust -m value based on CPU cores)
make -j4 build-all
```

**Smaller deployment packages:**
```bash
# Standard builds (smaller, require .NET runtime on target)
make build-all

# Trimmed builds (experimental)
dotnet publish src/RNetPi.Console -c Release -r linux-arm -p:PublishTrimmed=true
```

## Next Steps

After building:

1. **For deployment**: See [Raspberry Pi Deployment Guide](RASPBERRY_PI_DEPLOYMENT.md)
2. **For development**: Use `make build` for local testing
3. **For CI/CD**: Integrate build scripts into your pipeline

## Build Configuration

The build system uses:
- **Directory.Build.props**: Centralized project configuration
- **Architecture-specific constants**: `RASPBERRY_PI_2`, `RASPBERRY_PI_5`
- **Shared package versions**: Consistent dependency management
- **Runtime identifiers**: Proper platform targeting