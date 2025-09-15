# RNET-Pi Build Script for Raspberry Pi Architectures (PowerShell)
# This script builds the C# components for different Raspberry Pi models

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("pi2", "pi5", "all")]
    [string]$Architecture,
    
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter()]
    [string]$OutputDirectory = "dist",
    
    [Parameter()]
    [switch]$SelfContained,
    
    [Parameter()]
    [switch]$Help
)

if ($Help) {
    Write-Host @"
RNET-Pi Build Script for Raspberry Pi Architectures

USAGE:
    .\build-raspberry-pi.ps1 -Architecture <ARCH> [OPTIONS]

PARAMETERS:
    -Architecture <ARCH>    Target architecture (pi2, pi5, all)
                           pi2: Raspberry Pi 2 Model B (linux-arm)
                           pi5: Raspberry Pi 5 (linux-arm64)
                           all: Build for both architectures
    
    -Configuration <CFG>    Build configuration (Debug, Release) [default: Release]
    -OutputDirectory <DIR>  Output directory [default: .\dist]
    -SelfContained         Create self-contained deployment
    -Help                  Show this help message

EXAMPLES:
    .\build-raspberry-pi.ps1 -Architecture pi2
    .\build-raspberry-pi.ps1 -Architecture pi5 -Configuration Debug
    .\build-raspberry-pi.ps1 -Architecture all -SelfContained
"@
    exit 0
}

# Script directory and paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$BuildDir = Join-Path $RootDir $OutputDirectory

# Architecture mapping
$ArchMap = @{
    "pi2" = "linux-arm"
    "pi5" = "linux-arm64"
}

# Projects to build
$Projects = @(
    "src\RNetPi.API"
    "src\RNetPi.Console"
)

function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Get-DeviceDescription {
    param([string]$Arch)
    
    switch ($Arch) {
        "pi2" { return "Raspberry Pi 2 Model B Rev 1.1, OS Version: Raspbian 12 - Bookworm" }
        "pi5" { return "Raspberry Pi 5 with 64-bit OS" }
    }
}

function Build-ForArchitecture {
    param(
        [string]$TargetArch,
        [string]$RuntimeId,
        [string]$ArchOutputDir
    )
    
    Write-Status "Building for $TargetArch (Runtime: $RuntimeId)"
    
    # Create output directory
    New-Item -ItemType Directory -Path $ArchOutputDir -Force | Out-Null
    
    # Build each project
    foreach ($project in $Projects) {
        $projectName = Split-Path -Leaf $project
        $projectOutput = Join-Path $ArchOutputDir $projectName
        
        Write-Status "Building $projectName for $TargetArch..."
        
        # Build arguments
        $buildArgs = @(
            "publish"
            (Join-Path $RootDir $project)
            "--configuration"
            $Configuration
            "--runtime"
            $RuntimeId
            "--output"
            $projectOutput
        )
        
        if ($SelfContained) {
            $buildArgs += "--self-contained", "true"
        } else {
            $buildArgs += "--self-contained", "false"
        }
        
        # Execute build
        $result = & dotnet @buildArgs
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Built $projectName for $TargetArch"
            
            # Create deployment info file
            $deploymentInfo = @{
                project = $projectName
                architecture = $TargetArch
                runtimeIdentifier = $RuntimeId
                configuration = $Configuration
                selfContained = $SelfContained.IsPresent
                buildDate = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
                targetDevice = Get-DeviceDescription $TargetArch
            }
            
            $deploymentInfo | ConvertTo-Json -Depth 2 | Out-File -FilePath (Join-Path $projectOutput "deployment-info.json") -Encoding UTF8
        } else {
            Write-Error "Failed to build $projectName for $TargetArch"
            return $false
        }
    }
    
    Write-Success "Completed build for $TargetArch"
    return $true
}

function Create-DeploymentPackage {
    param(
        [string]$TargetArch,
        [string]$ArchOutputDir
    )
    
    Write-Status "Creating deployment package for $TargetArch..."
    
    # Create deployment scripts directory
    $scriptsDir = Join-Path $ArchOutputDir "scripts"
    New-Item -ItemType Directory -Path $scriptsDir -Force | Out-Null
    
    # Create README for this architecture
    $readmeContent = @"
# RNET-Pi Deployment Package

## Target Device
$(Get-DeviceDescription $TargetArch)

## Architecture Details
- **Runtime Identifier**: $($ArchMap[$TargetArch])
- **Configuration**: $Configuration
- **Self-Contained**: $($SelfContained.IsPresent)
- **Build Date**: $((Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"))

## Installation

1. Copy this entire directory to your Raspberry Pi
2. Run the installation script:
   ``````bash
   cd /path/to/deployment/package
   chmod +x scripts/install.sh
   ./scripts/install.sh
   ``````

## Quick Start

1. Configure your serial device in `/opt/rnet-pi/RNetPi.Console/config.json`
2. Start the service: `sudo systemctl start rnet-pi`
3. Check status: `sudo systemctl status rnet-pi`

## Components

- **RNetPi.API**: Web API and Swagger documentation
- **RNetPi.Console**: Console application for headless operation

## Uninstallation

To remove RNET-Pi:
``````bash
./scripts/uninstall.sh
``````

For more information, see the main documentation at: https://github.com/mmackelprang/rnet-pi
"@

    $readmeContent | Out-File -FilePath (Join-Path $ArchOutputDir "README.md") -Encoding UTF8
    
    Write-Success "Created deployment package for $TargetArch"
}

# Main execution
function Main {
    Write-Status "RNET-Pi Build Script"
    Write-Status "Configuration: $Configuration"
    Write-Status "Output Directory: $BuildDir"
    Write-Status "Self-Contained: $($SelfContained.IsPresent)"
    
    # Clean output directory
    if (Test-Path $BuildDir) {
        Write-Status "Cleaning output directory..."
        Remove-Item -Path $BuildDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $BuildDir -Force | Out-Null
    
    # Change to root directory
    Set-Location $RootDir
    
    # Restore dependencies
    Write-Status "Restoring dependencies..."
    & dotnet restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to restore dependencies"
        exit 1
    }
    
    # Build based on architecture
    $success = $true
    if ($Architecture -eq "all") {
        foreach ($arch in @("pi2", "pi5")) {
            $runtimeId = $ArchMap[$arch]
            $archOutputDir = Join-Path $BuildDir $arch
            
            $result = Build-ForArchitecture -TargetArch $arch -RuntimeId $runtimeId -ArchOutputDir $archOutputDir
            if ($result) {
                Create-DeploymentPackage -TargetArch $arch -ArchOutputDir $archOutputDir
            } else {
                $success = $false
            }
        }
    } else {
        $runtimeId = $ArchMap[$Architecture]
        $archOutputDir = Join-Path $BuildDir $Architecture
        
        $result = Build-ForArchitecture -TargetArch $Architecture -RuntimeId $runtimeId -ArchOutputDir $archOutputDir
        if ($result) {
            Create-DeploymentPackage -TargetArch $Architecture -ArchOutputDir $archOutputDir
        } else {
            $success = $false
        }
    }
    
    if ($success) {
        Write-Success "Build completed successfully!"
        Write-Status "Output directory: $BuildDir"
        
        # Show build output
        Write-Status "Build output:"
        Get-ChildItem -Path $BuildDir -Recurse -Include "*.dll", "*.exe", "deployment-info.json" | 
            Sort-Object FullName | 
            ForEach-Object { Write-Host "  $($_.FullName.Replace($BuildDir, '.'))" }
    } else {
        Write-Error "Build failed!"
        exit 1
    }
}

# Execute main function
Main