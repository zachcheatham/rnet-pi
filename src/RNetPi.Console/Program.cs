using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RNetPi.Core.Interfaces;
using RNetPi.Infrastructure.Services;
using RNetPi.Core.Models;
using RNetPi.Core.Logging;

namespace RNetPi.Console;

public class Program
{
    public static async Task Main(string[] args)
    {
        System.Console.WriteLine("RNET-Pi .NET Console Application");
        System.Console.WriteLine("================================");

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register services
                services.AddSingleton<IConfigurationService, ConfigurationService>();
                services.AddSingleton<IRNetService, RNetService>();
                services.AddSingleton<RNetApplication>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddEnhancedConsole();
                logging.SetMinimumLevel(LogLevel.Debug); // Set to Debug to see packet details
            })
            .Build();

        try
        {
            var app = host.Services.GetRequiredService<RNetApplication>();
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Application error: {ex.Message}");
            System.Console.WriteLine($"Details: {ex}");
        }
        finally
        {
            await host.StopAsync();
        }
    }
}

public class RNetApplication
{
    private readonly ILogger<RNetApplication> _logger;
    private readonly IConfigurationService _configService;
    private readonly IRNetService _rnetService;

    public RNetApplication(
        ILogger<RNetApplication> logger,
        IConfigurationService configService,
        IRNetService rnetService)
    {
        _logger = logger;
        _configService = configService;
        _rnetService = rnetService;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting RNET-Pi application...");

        // Load configuration
        await _configService.LoadAsync();
        _logger.LogInformation("Server: {ServerName}", _configService.Configuration.ServerName);
        _logger.LogInformation("Serial Device: {Device}", _configService.Configuration.SerialDevice);
        _logger.LogInformation("Simulation Mode: {Simulate}", _configService.Configuration.Simulate);

        // Connect to RNet
        var connected = await _rnetService.ConnectAsync();
        if (!connected)
        {
            _logger.LogError("Failed to connect to RNet device");
            return;
        }

        // Create default zones and sources for demonstration
        await CreateDefaultData();

        // Wait for user input
        System.Console.WriteLine("\nPress 'q' to quit, 'z' to list zones, 's' to list sources:");
        
        while (true)
        {
            var key = System.Console.ReadKey(true);
            
            switch (key.KeyChar)
            {
                case 'q':
                case 'Q':
                    _logger.LogInformation("Shutting down...");
                    await _rnetService.DisconnectAsync();
                    return;
                
                case 'z':
                case 'Z':
                    ListZones();
                    break;
                
                case 's':
                case 'S':
                    ListSources();
                    break;
                
                case 't':
                case 'T':
                    await TestFunctionality();
                    break;
                
                default:
                    System.Console.WriteLine("Press 'q' to quit, 'z' to list zones, 's' to list sources, 't' to test functionality");
                    break;
            }
        }
    }

    private async Task CreateDefaultData()
    {
        // Create default zones if none exist
        if (!_rnetService.GetAllZones().Any())
        {
            _logger.LogInformation("Creating default zones...");
            _rnetService.CreateZone(0, 0, "Screen Porch");
            _rnetService.CreateZone(0, 1, "Master Bedroom");
            _rnetService.CreateZone(0, 2, "Dining Room");
            _rnetService.CreateZone(0, 3, "Kitchen");
            _rnetService.CreateZone(0, 4, "Living Room");
            _rnetService.CreateZone(0, 5, "Bonus Room");
        }

        // Create default sources if none exist
        if (!_rnetService.GetAllSources().Any())
        {
            _logger.LogInformation("Creating default sources...");
            _rnetService.CreateSource(0, "AudioCast1", SourceType.GoogleCast);
            _rnetService.CreateSource(1, "AudioCast2", SourceType.GoogleCast);
            _rnetService.CreateSource(2, "Pi Audio Input", SourceType.Computer);
        }

        await Task.CompletedTask;
    }

    private void ListZones()
    {
        var zones = _rnetService.GetAllZones().ToList();
        System.Console.WriteLine($"\nZones ({zones.Count}):");
        
        foreach (var zone in zones)
        {
            System.Console.WriteLine($"  [{zone.ControllerID}-{zone.ZoneID}] {zone.Name} - " +
                                   $"Power: {zone.Power}, Volume: {zone.Volume}, Source: {zone.Source}, Mute: {zone.Mute}");
        }
        System.Console.WriteLine();
    }

    private void ListSources()
    {
        var sources = _rnetService.GetAllSources().ToList();
        System.Console.WriteLine($"\nSources ({sources.Count}):");
        
        foreach (var source in sources)
        {
            System.Console.WriteLine($"  [{source.SourceID}] {source.Name} - Type: {source.Type}");
            if (source.AutoOnZones.Any())
            {
                System.Console.WriteLine($"      Auto-on zones: {string.Join(", ", source.AutoOnZones.Select(z => $"{z.ControllerID}-{z.ZoneID}"))}");
            }
        }
        System.Console.WriteLine();
    }

    private async Task TestFunctionality()
    {
        _logger.LogInformation("Testing zone control functionality...");
        
        var livingRoom = _rnetService.GetZone(0, 4);
        if (livingRoom != null)
        {
            System.Console.WriteLine("Testing Living Room zone:");
            
            // Test power
            livingRoom.SetPower(true);
            System.Console.WriteLine($"  Power on - Current power: {livingRoom.Power}");
            
            // Test volume
            livingRoom.SetVolume(25);
            System.Console.WriteLine($"  Set volume to 25 - Current volume: {livingRoom.Volume}");
            
            // Test source
            livingRoom.SetSource(1);
            System.Console.WriteLine($"  Set source to 1 - Current source: {livingRoom.Source}");
            
            // Test parameters
            livingRoom.SetParameter(0, 5); // Bass
            livingRoom.SetParameter(1, -2); // Treble
            System.Console.WriteLine($"  Set bass to 5, treble to -2");
            System.Console.WriteLine($"  Bass: {livingRoom.GetParameter(0)}, Treble: {livingRoom.GetParameter(1)}");
        }

        // Test global functions
        System.Console.WriteLine("\nTesting global functions:");
        _rnetService.SetAllPower(true);
        System.Console.WriteLine("  All zones powered on");
        
        await Task.Delay(1000);
        
        _rnetService.SetAllMute(true);
        System.Console.WriteLine("  All zones muted");

        System.Console.WriteLine("\nTesting complete!");
        await Task.CompletedTask;
    }
}
