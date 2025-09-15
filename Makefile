# RNET-Pi Makefile for Raspberry Pi builds

.PHONY: help build build-pi2 build-pi5 build-all clean test restore

# Default target
help:
	@echo "RNET-Pi Build System"
	@echo ""
	@echo "Available targets:"
	@echo "  help        Show this help message"
	@echo "  restore     Restore NuGet packages"
	@echo "  build       Build for local development (Any CPU)"
	@echo "  test        Run all tests"
	@echo "  build-pi2   Build for Raspberry Pi 2 (linux-arm)"
	@echo "  build-pi5   Build for Raspberry Pi 5 (linux-arm64)"
	@echo "  build-all   Build for all Raspberry Pi architectures"
	@echo "  clean       Clean build artifacts"
	@echo ""
	@echo "Advanced targets:"
	@echo "  build-pi2-sc     Build for Pi 2 with self-contained runtime"
	@echo "  build-pi5-sc     Build for Pi 5 with self-contained runtime"
	@echo "  build-all-sc     Build for all architectures with self-contained runtime"
	@echo ""
	@echo "Examples:"
	@echo "  make build-pi2"
	@echo "  make build-all"
	@echo "  make test"

# Restore NuGet packages
restore:
	@echo "Restoring NuGet packages..."
	@dotnet restore

# Build for local development
build:
	@echo "Building for local development..."
	@dotnet build --configuration Release

# Run tests
test:
	@echo "Running tests..."
	@dotnet test --verbosity minimal

# Build for Raspberry Pi 2
build-pi2:
	@echo "Building for Raspberry Pi 2..."
	@./scripts/build-raspberry-pi.sh --arch pi2

# Build for Raspberry Pi 5
build-pi5:
	@echo "Building for Raspberry Pi 5..."
	@./scripts/build-raspberry-pi.sh --arch pi5

# Build for all Raspberry Pi architectures
build-all:
	@echo "Building for all Raspberry Pi architectures..."
	@./scripts/build-raspberry-pi.sh --arch all

# Build for Raspberry Pi 2 with self-contained runtime
build-pi2-sc:
	@echo "Building for Raspberry Pi 2 (self-contained)..."
	@./scripts/build-raspberry-pi.sh --arch pi2 --self-contained

# Build for Raspberry Pi 5 with self-contained runtime
build-pi5-sc:
	@echo "Building for Raspberry Pi 5 (self-contained)..."
	@./scripts/build-raspberry-pi.sh --arch pi5 --self-contained

# Build for all architectures with self-contained runtime
build-all-sc:
	@echo "Building for all architectures (self-contained)..."
	@./scripts/build-raspberry-pi.sh --arch all --self-contained

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	@dotnet clean
	@rm -rf dist/
	@echo "Clean complete."

# Debug build for Pi 2
debug-pi2:
	@echo "Building debug version for Raspberry Pi 2..."
	@./scripts/build-raspberry-pi.sh --arch pi2 --config Debug

# Debug build for Pi 5
debug-pi5:
	@echo "Building debug version for Raspberry Pi 5..."
	@./scripts/build-raspberry-pi.sh --arch pi5 --config Debug

# Quick development cycle: clean, restore, build, test
dev: clean restore build test
	@echo "Development build complete."