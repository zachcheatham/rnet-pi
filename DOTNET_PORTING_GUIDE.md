# RNET-Pi to C# .NET Porting Guide

## Overview

This document provides a comprehensive guide for porting the RNET-Pi Node.js application to a modern C# .NET project. The guide maintains the existing architecture while leveraging .NET's strengths and modern development practices.

## Target Architecture

### .NET Technology Stack
- **.NET 8.0+**: Latest LTS version for best performance and features
- **ASP.NET Core**: For web server and API endpoints
- **SignalR**: For real-time client communication (replaces WebSocket)
- **System.IO.Ports**: For serial communication
- **Entity Framework Core**: For configuration persistence (optional upgrade)
- **Serilog**: For structured logging
- **AutoMapper**: For object mapping
- **MediatR**: For CQRS pattern implementation

### Project Structure
```
RNetPi.sln
├── src/
│   ├── RNetPi.Core/                    # Domain models and interfaces
│   ├── RNetPi.Infrastructure/          # Hardware communication and external services
│   ├── RNetPi.Application/             # Business logic and services
│   ├── RNetPi.API/                     # Web API and SignalR hubs
│   └── RNetPi.Console/                 # Console application host
├── tests/
│   ├── RNetPi.Core.Tests/
│   ├── RNetPi.Infrastructure.Tests/
│   └── RNetPi.Application.Tests/
└── docs/
    ├── architecture.md
    └── deployment.md
```

## Phase 1: Project Setup and Infrastructure

### 1.1 Create Solution Structure

```bash
# Create solution and projects
dotnet new sln -n RNetPi
dotnet new classlib -n RNetPi.Core -f net8.0
dotnet new classlib -n RNetPi.Infrastructure -f net8.0
dotnet new classlib -n RNetPi.Application -f net8.0
dotnet new webapi -n RNetPi.API -f net8.0
dotnet new console -n RNetPi.Console -f net8.0

# Add projects to solution
dotnet sln add src/RNetPi.Core/RNetPi.Core.csproj
dotnet sln add src/RNetPi.Infrastructure/RNetPi.Infrastructure.csproj
dotnet sln add src/RNetPi.Application/RNetPi.Application.csproj
dotnet sln add src/RNetPi.API/RNetPi.API.csproj
dotnet sln add src/RNetPi.Console/RNetPi.Console.csproj

# Setup project references
dotnet add src/RNetPi.Infrastructure/RNetPi.Infrastructure.csproj reference src/RNetPi.Core/RNetPi.Core.csproj
dotnet add src/RNetPi.Application/RNetPi.Application.csproj reference src/RNetPi.Core/RNetPi.Core.csproj
dotnet add src/RNetPi.API/RNetPi.API.csproj reference src/RNetPi.Application/RNetPi.Application.csproj
dotnet add src/RNetPi.API/RNetPi.API.csproj reference src/RNetPi.Infrastructure/RNetPi.Infrastructure.csproj
dotnet add src/RNetPi.Console/RNetPi.Console.csproj reference src/RNetPi.API/RNetPi.API.csproj
```

This comprehensive porting guide provides a roadmap for migrating the RNET-Pi project to modern C# .NET while maintaining all existing functionality and improving performance, maintainability, and deployment options.
