#!/bin/bash

# RNET-Pi Build Script for Raspberry Pi Architectures
# This script builds the C# components for different Raspberry Pi models

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
BUILD_DIR="$ROOT_DIR/dist"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Build RNET-Pi for Raspberry Pi architectures"
    echo ""
    echo "Options:"
    echo "  -a, --arch ARCH    Target architecture (pi2, pi5, all)"
    echo "                     pi2: Raspberry Pi 2 Model B (linux-arm)"
    echo "                     pi5: Raspberry Pi 5 (linux-arm64)"
    echo "                     all: Build for both architectures"
    echo "  -c, --config CFG   Build configuration (Debug, Release) [default: Release]"
    echo "  -o, --output DIR   Output directory [default: ./dist]"
    echo "  -s, --self-contained  Create self-contained deployment"
    echo "  -h, --help         Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 --arch pi2                # Build for Raspberry Pi 2"
    echo "  $0 --arch pi5 --config Debug # Build for Raspberry Pi 5 (Debug)"
    echo "  $0 --arch all --self-contained # Build for both with runtime included"
}

# Default values
ARCH=""
CONFIG="Release"
OUTPUT_DIR="$BUILD_DIR"
SELF_CONTAINED=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -a|--arch)
            ARCH="$2"
            shift 2
            ;;
        -c|--config)
            CONFIG="$2"
            shift 2
            ;;
        -o|--output)
            OUTPUT_DIR="$2"
            shift 2
            ;;
        -s|--self-contained)
            SELF_CONTAINED=true
            shift
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            usage
            exit 1
            ;;
    esac
done

# Validate architecture
if [[ -z "$ARCH" ]]; then
    print_error "Architecture must be specified"
    usage
    exit 1
fi

if [[ "$ARCH" != "pi2" && "$ARCH" != "pi5" && "$ARCH" != "all" ]]; then
    print_error "Invalid architecture: $ARCH"
    usage
    exit 1
fi

# Map architectures to runtime identifiers
declare -A ARCH_MAP
ARCH_MAP["pi2"]="linux-arm"
ARCH_MAP["pi5"]="linux-arm64"

# Projects to build
PROJECTS=(
    "src/RNetPi.API"
    "src/RNetPi.Console"
)

build_for_arch() {
    local target_arch=$1
    local runtime_id=${ARCH_MAP[$target_arch]}
    local arch_output_dir="$OUTPUT_DIR/$target_arch"
    
    print_status "Building for $target_arch (Runtime: $runtime_id)"
    
    # Create output directory
    mkdir -p "$arch_output_dir"
    
    # Build each project
    for project in "${PROJECTS[@]}"; do
        local project_name=$(basename "$project")
        local project_output="$arch_output_dir/$project_name"
        
        print_status "Building $project_name for $target_arch..."
        
        # Build arguments
        local build_args=(
            "$ROOT_DIR/$project"
            --configuration "$CONFIG"
            --runtime "$runtime_id"
            --output "$project_output"
        )
        
        if [[ "$SELF_CONTAINED" == "true" ]]; then
            build_args+=(--self-contained true)
        else
            build_args+=(--self-contained false)
        fi
        
        # Execute build
        if dotnet publish "${build_args[@]}"; then
            print_success "Built $project_name for $target_arch"
            
            # Create deployment info file
            cat > "$project_output/deployment-info.json" << EOF
{
  "project": "$project_name",
  "architecture": "$target_arch",
  "runtimeIdentifier": "$runtime_id",
  "configuration": "$CONFIG",
  "selfContained": $SELF_CONTAINED,
  "buildDate": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "targetDevice": "$(get_device_description $target_arch)"
}
EOF
        else
            print_error "Failed to build $project_name for $target_arch"
            return 1
        fi
    done
    
    print_success "Completed build for $target_arch"
}

get_device_description() {
    case $1 in
        pi2)
            echo "Raspberry Pi 2 Model B Rev 1.1, OS Version: Raspbian 12 - Bookworm"
            ;;
        pi5)
            echo "Raspberry Pi 5 with 64-bit OS"
            ;;
    esac
}

create_deployment_package() {
    local target_arch=$1
    local arch_output_dir="$OUTPUT_DIR/$target_arch"
    
    print_status "Creating deployment package for $target_arch..."
    
    # Create deployment scripts
    local scripts_dir="$arch_output_dir/scripts"
    mkdir -p "$scripts_dir"
    
    # Create installation script
    cat > "$scripts_dir/install.sh" << 'EOF'
#!/bin/bash

# RNET-Pi Installation Script
# Automatically installs RNET-Pi for Raspberry Pi

set -e

INSTALL_DIR="/opt/rnet-pi"
SERVICE_NAME="rnet-pi"
USER="pi"

print_status() {
    echo "[INFO] $1"
}

print_success() {
    echo "[SUCCESS] $1"
}

print_error() {
    echo "[ERROR] $1"
}

# Check if running as root
if [[ $EUID -eq 0 ]]; then
   print_error "This script should not be run as root. Please run as the pi user."
   exit 1
fi

# Create installation directory
print_status "Creating installation directory..."
sudo mkdir -p "$INSTALL_DIR"
sudo chown "$USER:$USER" "$INSTALL_DIR"

# Copy files
print_status "Installing application files..."
cp -r ./* "$INSTALL_DIR/"

# Set executable permissions
chmod +x "$INSTALL_DIR"/RNetPi.*/RNetPi.*

# Add user to dialout group for serial access
print_status "Configuring serial port access..."
sudo usermod -a -G dialout "$USER"

# Create systemd service
print_status "Creating systemd service..."
sudo tee "/etc/systemd/system/${SERVICE_NAME}.service" > /dev/null << EOSERVICE
[Unit]
Description=RNET-Pi Audio Controller
After=network.target
StartLimitIntervalSec=0

[Service]
Type=simple
Restart=always
RestartSec=5
User=$USER
ExecStart=$INSTALL_DIR/RNetPi.Console/RNetPi.Console
WorkingDirectory=$INSTALL_DIR/RNetPi.Console
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
EOSERVICE

# Reload systemd and enable service
sudo systemctl daemon-reload
sudo systemctl enable "$SERVICE_NAME"

print_success "Installation completed!"
print_status "To start the service: sudo systemctl start $SERVICE_NAME"
print_status "To check status: sudo systemctl status $SERVICE_NAME"
print_status "To view logs: sudo journalctl -u $SERVICE_NAME -f"

EOF

    chmod +x "$scripts_dir/install.sh"
    
    # Create uninstall script
    cat > "$scripts_dir/uninstall.sh" << 'EOF'
#!/bin/bash

# RNET-Pi Uninstallation Script

set -e

INSTALL_DIR="/opt/rnet-pi"
SERVICE_NAME="rnet-pi"

print_status() {
    echo "[INFO] $1"
}

print_success() {
    echo "[SUCCESS] $1"
}

# Stop and disable service
print_status "Stopping and disabling service..."
sudo systemctl stop "$SERVICE_NAME" 2>/dev/null || true
sudo systemctl disable "$SERVICE_NAME" 2>/dev/null || true

# Remove service file
sudo rm -f "/etc/systemd/system/${SERVICE_NAME}.service"
sudo systemctl daemon-reload

# Remove installation directory
print_status "Removing application files..."
sudo rm -rf "$INSTALL_DIR"

print_success "Uninstallation completed!"

EOF

    chmod +x "$scripts_dir/uninstall.sh"
    
    # Create README for this architecture
    cat > "$arch_output_dir/README.md" << EOF
# RNET-Pi Deployment Package

## Target Device
$(get_device_description $target_arch)

## Architecture Details
- **Runtime Identifier**: ${ARCH_MAP[$target_arch]}
- **Configuration**: $CONFIG
- **Self-Contained**: $SELF_CONTAINED
- **Build Date**: $(date -u +"%Y-%m-%dT%H:%M:%SZ")

## Installation

1. Copy this entire directory to your Raspberry Pi
2. Run the installation script:
   \`\`\`bash
   cd /path/to/deployment/package
   chmod +x scripts/install.sh
   ./scripts/install.sh
   \`\`\`

## Quick Start

1. Configure your serial device in \`/opt/rnet-pi/RNetPi.Console/config.json\`
2. Start the service: \`sudo systemctl start rnet-pi\`
3. Check status: \`sudo systemctl status rnet-pi\`

## Components

- **RNetPi.API**: Web API and Swagger documentation
- **RNetPi.Console**: Console application for headless operation

## Uninstallation

To remove RNET-Pi:
\`\`\`bash
./scripts/uninstall.sh
\`\`\`

For more information, see the main documentation at: https://github.com/mmackelprang/rnet-pi
EOF

    print_success "Created deployment package for $target_arch"
}

# Main execution
main() {
    print_status "RNET-Pi Build Script"
    print_status "Configuration: $CONFIG"
    print_status "Output Directory: $OUTPUT_DIR"
    print_status "Self-Contained: $SELF_CONTAINED"
    
    # Clean output directory
    if [[ -d "$OUTPUT_DIR" ]]; then
        print_status "Cleaning output directory..."
        rm -rf "$OUTPUT_DIR"
    fi
    mkdir -p "$OUTPUT_DIR"
    
    # Change to root directory
    cd "$ROOT_DIR"
    
    # Restore dependencies
    print_status "Restoring dependencies..."
    dotnet restore
    
    # Build based on architecture
    if [[ "$ARCH" == "all" ]]; then
        for arch in "pi2" "pi5"; do
            build_for_arch "$arch"
            create_deployment_package "$arch"
        done
    else
        build_for_arch "$ARCH"
        create_deployment_package "$ARCH"
    fi
    
    print_success "Build completed successfully!"
    print_status "Output directory: $OUTPUT_DIR"
    
    # Show directory structure
    print_status "Build output:"
    find "$OUTPUT_DIR" -type f -name "*.dll" -o -name "*.exe" -o -name "deployment-info.json" | sort
}

# Execute main function
main "$@"