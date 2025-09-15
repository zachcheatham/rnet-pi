using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RNetPi.Core.Interfaces;
using RNetPi.Core.Models;
using RNetPi.Core.Services;

namespace RNetPi.Examples.ConsoleApp;

/// <summary>
/// Simple console application demonstrating basic RNet functionality
/// </summary>
internal class Program
{
    private static EnhancedRNetService? _rnetService;

    static async Task Main(string[] args)
    {
        Console.WriteLine("RNet Console Example");
        Console.WriteLine("===================");

        // Set up dependency injection
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add configuration service with simulation mode
        services.AddSingleton<IConfigurationService>(provider =>
        {
            var config = new Configuration
            {
                Simulate = true, // Use simulation mode for this example
                SerialDevice = "/dev/ttyUSB0"
            };
            return new ConfigurationService(config);
        });

        services.AddSingleton<EnhancedRNetService>();

        var serviceProvider = services.BuildServiceProvider();
        _rnetService = serviceProvider.GetRequiredService<EnhancedRNetService>();

        // Subscribe to events
        SetupEventHandlers();

        try
        {
            // Connect to RNet
            Console.WriteLine("Connecting to RNet device...");
            var connected = await _rnetService.ConnectAsync();

            if (!connected)
            {
                Console.WriteLine("Failed to connect to RNet device");
                return;
            }

            Console.WriteLine("Connected successfully!");

            // Create sample zones and sources
            await SetupSampleConfiguration();

            // Demonstrate basic functionality
            await DemonstrateBasicFunctionality();

            // Interactive menu
            await RunInteractiveMenu();
        }
        finally
        {
            await _rnetService.DisconnectAsync();
            _rnetService.Dispose();
        }
    }

    private static void SetupEventHandlers()
    {
        if (_rnetService == null) return;

        _rnetService.Connected += (_, _) => Console.WriteLine("✓ RNet Connected");
        _rnetService.Disconnected += (_, _) => Console.WriteLine("✗ RNet Disconnected");
        _rnetService.Error += (_, ex) => Console.WriteLine($"⚠ RNet Error: {ex.Message}");

        _rnetService.ZoneAdded += (_, zone) =>
            Console.WriteLine($"+ Zone Added: {zone.Name} (Controller {zone.ControllerID}, Zone {zone.ZoneID})");

        _rnetService.SourceAdded += (_, source) =>
            Console.WriteLine($"+ Source Added: {source.Name} (ID {source.SourceID}, Type: {source.Type})");

        _rnetService.KeypadEvent += (_, keypadEvent) =>
            Console.WriteLine($"🎹 Keypad Event: Controller {keypadEvent.GetControllerID()}, " +
                            $"Zone {keypadEvent.GetZoneID()}, Key 0x{keypadEvent.GetKeyID():X2}");

        _rnetService.DisplayMessage += (_, message) =>
            Console.WriteLine($"📺 Display Message: {message}");
    }

    private static async Task SetupSampleConfiguration()
    {
        if (_rnetService == null) return;

        Console.WriteLine("\nSetting up sample configuration...");

        // Create zones
        var livingRoom = _rnetService.CreateZone(0, 1, "Living Room");
        var kitchen = _rnetService.CreateZone(0, 2, "Kitchen");
        var bedroom = _rnetService.CreateZone(0, 3, "Master Bedroom");

        // Configure zone properties
        livingRoom.SetMaxVolume(90);
        kitchen.SetMaxVolume(80);
        bedroom.SetMaxVolume(70);

        // Create sources
        var cableBox = _rnetService.CreateSource(1, "Cable Box", SourceType.Generic);
        var chromecast = _rnetService.CreateSource(2, "Chromecast", SourceType.GoogleCast);
        var sonos = _rnetService.CreateSource(3, "Sonos", SourceType.Sonos);

        // Configure source properties
        cableBox.AutoOff = true;
        cableBox.AddAutoOnZone(0, 1); // Auto-on for living room

        Console.WriteLine("Configuration complete!");
        await Task.Delay(500); // Brief pause for effect
    }

    private static async Task DemonstrateBasicFunctionality()
    {
        if (_rnetService == null) return;

        Console.WriteLine("\nDemonstrating basic functionality...");

        // Turn on all zones with different volumes
        Console.WriteLine("Turning on all zones...");
        _rnetService.SetZonePower(0, 1, true);
        _rnetService.SetZoneVolume(0, 1, 40);
        await Task.Delay(200);

        _rnetService.SetZonePower(0, 2, true);
        _rnetService.SetZoneVolume(0, 2, 30);
        await Task.Delay(200);

        _rnetService.SetZonePower(0, 3, true);
        _rnetService.SetZoneVolume(0, 3, 25);
        await Task.Delay(500);

        // Set different sources
        Console.WriteLine("Setting different sources...");
        _rnetService.SetZoneSource(0, 1, 2); // Living Room -> Chromecast
        await Task.Delay(200);
        _rnetService.SetZoneSource(0, 2, 3); // Kitchen -> Sonos
        await Task.Delay(200);
        _rnetService.SetZoneSource(0, 3, 1); // Bedroom -> Cable Box
        await Task.Delay(500);

        // Send welcome messages
        Console.WriteLine("Sending welcome messages...");
        _rnetService.SendDisplayMessage(0, 1, "Welcome to Living Room!", 3);
        _rnetService.SendDisplayMessage(0, 2, "Kitchen Audio Active", 3);
        _rnetService.SendDisplayMessage(0, 3, "Good Evening", 3);

        await Task.Delay(1000);
        Console.WriteLine("Basic demonstration complete!");
    }

    private static async Task RunInteractiveMenu()
    {
        if (_rnetService == null) return;

        while (true)
        {
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("RNet Interactive Menu");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine("1. Show Zone Status");
            Console.WriteLine("2. Show Source Status");
            Console.WriteLine("3. Control Zone Power");
            Console.WriteLine("4. Control Zone Volume");
            Console.WriteLine("5. Set Zone Source");
            Console.WriteLine("6. Send Display Message");
            Console.WriteLine("7. All Zones Power Control");
            Console.WriteLine("8. Request Zone Information");
            Console.WriteLine("9. Exit");
            Console.Write("\nSelect option (1-9): ");

            var input = Console.ReadLine();
            Console.WriteLine();

            switch (input)
            {
                case "1":
                    ShowZoneStatus();
                    break;
                case "2":
                    ShowSourceStatus();
                    break;
                case "3":
                    await ControlZonePower();
                    break;
                case "4":
                    await ControlZoneVolume();
                    break;
                case "5":
                    await SetZoneSource();
                    break;
                case "6":
                    await SendDisplayMessage();
                    break;
                case "7":
                    await AllZonesPowerControl();
                    break;
                case "8":
                    await RequestZoneInformation();
                    break;
                case "9":
                    Console.WriteLine("Goodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }

    private static void ShowZoneStatus()
    {
        if (_rnetService == null) return;

        Console.WriteLine("Zone Status:");
        Console.WriteLine(new string('-', 60));

        foreach (var zone in _rnetService.GetAllZones())
        {
            Console.WriteLine($"Zone: {zone.Name}");
            Console.WriteLine($"  Controller: {zone.ControllerID}, Zone ID: {zone.ZoneID}");
            Console.WriteLine($"  Power: {(zone.Power ? "ON" : "OFF")}");
            Console.WriteLine($"  Volume: {zone.Volume}% (Max: {zone.MaxVolume}%)");
            Console.WriteLine($"  Source: {zone.Source}");
            Console.WriteLine($"  Mute: {(zone.Mute ? "YES" : "NO")}");
            Console.WriteLine($"  Bass: {(int)zone.GetParameter(0) - 10}");
            Console.WriteLine($"  Treble: {(int)zone.GetParameter(1) - 10}");
            Console.WriteLine($"  Loudness: {((bool)zone.GetParameter(2) ? "ON" : "OFF")}");
            Console.WriteLine();
        }
    }

    private static void ShowSourceStatus()
    {
        if (_rnetService == null) return;

        Console.WriteLine("Source Status:");
        Console.WriteLine(new string('-', 60));

        foreach (var source in _rnetService.GetAllSources())
        {
            Console.WriteLine($"Source: {source.Name} (ID: {source.SourceID})");
            Console.WriteLine($"  Type: {source.Type}");
            Console.WriteLine($"  Auto Off: {(source.AutoOff ? "YES" : "NO")}");
            Console.WriteLine($"  Auto On Zones: {source.AutoOnZones.Count}");
            
            if (!string.IsNullOrEmpty(source.MediaTitle))
            {
                Console.WriteLine($"  Now Playing: {source.MediaTitle}");
                if (!string.IsNullOrEmpty(source.MediaArtist))
                    Console.WriteLine($"  Artist: {source.MediaArtist}");
                Console.WriteLine($"  Status: {(source.MediaPlaying ? "Playing" : "Stopped")}");
            }

            if (!string.IsNullOrEmpty(source.DescriptiveText))
                Console.WriteLine($"  Description: {source.DescriptiveText}");

            Console.WriteLine();
        }
    }

    private static async Task ControlZonePower()
    {
        if (_rnetService == null) return;

        var zones = _rnetService.GetAllZones().ToList();
        if (!zones.Any())
        {
            Console.WriteLine("No zones available.");
            return;
        }

        Console.WriteLine("Select zone:");
        for (int i = 0; i < zones.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {zones[i].Name} (Currently: {(zones[i].Power ? "ON" : "OFF")})");
        }

        Console.Write("Zone number: ");
        if (int.TryParse(Console.ReadLine(), out int zoneIndex) && 
            zoneIndex > 0 && zoneIndex <= zones.Count)
        {
            var zone = zones[zoneIndex - 1];
            Console.Write($"Turn {zone.Name} [O]n or [F]f? ");
            var powerChoice = Console.ReadLine()?.ToUpper();

            if (powerChoice == "O")
            {
                _rnetService.SetZonePower(zone.ControllerID, zone.ZoneID, true);
                Console.WriteLine($"Turning on {zone.Name}");
            }
            else if (powerChoice == "F")
            {
                _rnetService.SetZonePower(zone.ControllerID, zone.ZoneID, false);
                Console.WriteLine($"Turning off {zone.Name}");
            }
            else
            {
                Console.WriteLine("Invalid choice.");
            }
        }
        else
        {
            Console.WriteLine("Invalid zone number.");
        }
    }

    private static async Task ControlZoneVolume()
    {
        if (_rnetService == null) return;

        var zones = _rnetService.GetAllZones().ToList();
        if (!zones.Any())
        {
            Console.WriteLine("No zones available.");
            return;
        }

        Console.WriteLine("Select zone:");
        for (int i = 0; i < zones.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {zones[i].Name} (Current: {zones[i].Volume}%)");
        }

        Console.Write("Zone number: ");
        if (int.TryParse(Console.ReadLine(), out int zoneIndex) && 
            zoneIndex > 0 && zoneIndex <= zones.Count)
        {
            var zone = zones[zoneIndex - 1];
            Console.Write($"Enter volume for {zone.Name} (0-{zone.MaxVolume}): ");
            if (int.TryParse(Console.ReadLine(), out int volume) && 
                volume >= 0 && volume <= zone.MaxVolume)
            {
                _rnetService.SetZoneVolume(zone.ControllerID, zone.ZoneID, volume);
                Console.WriteLine($"Setting {zone.Name} volume to {volume}%");
            }
            else
            {
                Console.WriteLine("Invalid volume level.");
            }
        }
        else
        {
            Console.WriteLine("Invalid zone number.");
        }
    }

    private static async Task SetZoneSource()
    {
        if (_rnetService == null) return;

        var zones = _rnetService.GetAllZones().ToList();
        var sources = _rnetService.GetAllSources().ToList();

        if (!zones.Any())
        {
            Console.WriteLine("No zones available.");
            return;
        }

        if (!sources.Any())
        {
            Console.WriteLine("No sources available.");
            return;
        }

        Console.WriteLine("Select zone:");
        for (int i = 0; i < zones.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {zones[i].Name}");
        }

        Console.Write("Zone number: ");
        if (int.TryParse(Console.ReadLine(), out int zoneIndex) && 
            zoneIndex > 0 && zoneIndex <= zones.Count)
        {
            var zone = zones[zoneIndex - 1];

            Console.WriteLine("Select source:");
            for (int i = 0; i < sources.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {sources[i].Name} (ID: {sources[i].SourceID})");
            }

            Console.Write("Source number: ");
            if (int.TryParse(Console.ReadLine(), out int sourceIndex) && 
                sourceIndex > 0 && sourceIndex <= sources.Count)
            {
                var source = sources[sourceIndex - 1];
                _rnetService.SetZoneSource(zone.ControllerID, zone.ZoneID, source.SourceID);
                Console.WriteLine($"Setting {zone.Name} source to {source.Name}");
            }
            else
            {
                Console.WriteLine("Invalid source number.");
            }
        }
        else
        {
            Console.WriteLine("Invalid zone number.");
        }
    }

    private static async Task SendDisplayMessage()
    {
        if (_rnetService == null) return;

        var zones = _rnetService.GetAllZones().ToList();
        if (!zones.Any())
        {
            Console.WriteLine("No zones available.");
            return;
        }

        Console.WriteLine("Select zone:");
        for (int i = 0; i < zones.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {zones[i].Name}");
        }

        Console.Write("Zone number (0 for all zones): ");
        var input = Console.ReadLine();

        Console.Write("Enter message: ");
        var message = Console.ReadLine();

        Console.Write("Display time in seconds (default 5): ");
        if (!byte.TryParse(Console.ReadLine(), out byte displayTime))
            displayTime = 5;

        if (string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine("Message cannot be empty.");
            return;
        }

        if (input == "0")
        {
            // Send to all zones
            foreach (var zone in zones)
            {
                _rnetService.SendDisplayMessage(zone.ControllerID, zone.ZoneID, message, displayTime);
            }
            Console.WriteLine($"Sent message to all zones: {message}");
        }
        else if (int.TryParse(input, out int zoneIndex) && 
                 zoneIndex > 0 && zoneIndex <= zones.Count)
        {
            var zone = zones[zoneIndex - 1];
            _rnetService.SendDisplayMessage(zone.ControllerID, zone.ZoneID, message, displayTime);
            Console.WriteLine($"Sent message to {zone.Name}: {message}");
        }
        else
        {
            Console.WriteLine("Invalid zone number.");
        }
    }

    private static async Task AllZonesPowerControl()
    {
        if (_rnetService == null) return;

        Console.Write("Turn all zones [O]n or [F]f? ");
        var choice = Console.ReadLine()?.ToUpper();

        if (choice == "O")
        {
            _rnetService.SetAllPower(true);
            Console.WriteLine("Turning on all zones");
        }
        else if (choice == "F")
        {
            _rnetService.SetAllPower(false);
            Console.WriteLine("Turning off all zones");
        }
        else
        {
            Console.WriteLine("Invalid choice.");
        }
    }

    private static async Task RequestZoneInformation()
    {
        if (_rnetService == null) return;

        var zones = _rnetService.GetAllZones().ToList();
        if (!zones.Any())
        {
            Console.WriteLine("No zones available.");
            return;
        }

        Console.WriteLine("Requesting current information for all zones...");

        foreach (var zone in zones)
        {
            _rnetService.RequestZoneInfo(zone.ControllerID, zone.ZoneID);
            await Task.Delay(100); // Small delay between requests
        }

        Console.WriteLine("Zone information requests sent. Current state will be updated automatically.");
    }
}