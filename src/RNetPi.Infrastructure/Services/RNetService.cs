using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RNetPi.Core.Interfaces;
using RNetPi.Core.Models;

namespace RNetPi.Infrastructure.Services;

public class RNetService : IRNetService, IDisposable
{
    private readonly ILogger<RNetService> _logger;
    private readonly IConfigurationService _configService;
    private readonly ConcurrentDictionary<(int controllerID, int zoneID), Zone> _zones;
    private readonly ConcurrentDictionary<int, Source> _sources;
    private SerialPort? _serialPort;
    private bool _connected = false;
    private bool _autoUpdating = false;
    private readonly Queue<byte[]> _packetQueue = new();
    private readonly string _zonesFilePath;
    private readonly string _sourcesFilePath;

    public bool IsConnected => _connected;

    public event EventHandler? Connected;
    public event EventHandler? Disconnected; 
    public event EventHandler<Exception>? Error;

    public RNetService(ILogger<RNetService> logger, IConfigurationService configService)
    {
        _logger = logger;
        _configService = configService;
        _zones = new ConcurrentDictionary<(int, int), Zone>();
        _sources = new ConcurrentDictionary<int, Source>();
        
        var baseDirectory = Directory.GetCurrentDirectory();
        _zonesFilePath = Path.Combine(baseDirectory, "zones.json");
        _sourcesFilePath = Path.Combine(baseDirectory, "sources.json");
        
        LoadPersistedDataAsync().ConfigureAwait(false);
    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            if (_configService.Configuration.Simulate)
            {
                _logger.LogInformation("Simulation mode enabled - not opening serial connection");
                _connected = true;
                Connected?.Invoke(this, EventArgs.Empty);
                return true;
            }

            var devicePath = _configService.Configuration.SerialDevice;
            _logger.LogInformation("Connecting to RNet on device: {Device}", devicePath);

            _serialPort = new SerialPort(devicePath, 19200, Parity.None, 8, StopBits.One);
            _serialPort.DataReceived += OnDataReceived;
            _serialPort.ErrorReceived += OnErrorReceived;

            _serialPort.Open();
            _connected = true;
            
            _logger.LogInformation("Connected to RNet on {Device}", devicePath);
            Connected?.Invoke(this, EventArgs.Empty);
            
            // Request all zone info on connection
            await RequestAllZoneInfoAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RNet device");
            Error?.Invoke(this, ex);
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_serialPort != null)
        {
            try
            {
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while disconnecting from serial port");
            }
        }

        _connected = false;
        Disconnected?.Invoke(this, EventArgs.Empty);
        await Task.CompletedTask;
    }

    public Zone? GetZone(int controllerID, int zoneID)
    {
        return _zones.TryGetValue((controllerID, zoneID), out var zone) ? zone : null;
    }

    public Source? GetSource(int sourceID)
    {
        return _sources.TryGetValue(sourceID, out var source) ? source : null;
    }

    public IEnumerable<Zone> GetAllZones()
    {
        return _zones.Values;
    }

    public IEnumerable<Source> GetAllSources()
    {
        return _sources.Values;
    }

    public Zone CreateZone(int controllerID, int zoneID, string name)
    {
        var zone = new Zone(controllerID, zoneID);
        zone.SetName(name);
        
        // Subscribe to zone events for persistence and broadcasting
        zone.NameChanged += (name) => SaveZonesAsync().ConfigureAwait(false);
        zone.PowerChangedSimple += (power) => _logger.LogDebug("Zone {Controller}-{Zone} power: {Power}", controllerID, zoneID, power);
        zone.VolumeChangedSimple += (volume) => _logger.LogDebug("Zone {Controller}-{Zone} volume: {Volume}", controllerID, zoneID, volume);
        
        _zones.TryAdd((controllerID, zoneID), zone);
        _logger.LogInformation("Created zone {Controller}-{Zone}: {Name}", controllerID, zoneID, name);
        
        SaveZonesAsync().ConfigureAwait(false);
        return zone;
    }

    public Source CreateSource(int sourceID, string name, SourceType type)
    {
        var source = new Source(sourceID, name, type);
        
        // Subscribe to source events for persistence and broadcasting
        source.NameChanged += (name, oldName) => SaveSourcesAsync().ConfigureAwait(false);
        source.TypeChanged += (type) => SaveSourcesAsync().ConfigureAwait(false);
        
        _sources.TryAdd(sourceID, source);
        _logger.LogInformation("Created source {SourceID}: {Name} (Type: {Type})", sourceID, name, type);
        
        SaveSourcesAsync().ConfigureAwait(false);
        return source;
    }

    public void DeleteZone(int controllerID, int zoneID)
    {
        if (_zones.TryRemove((controllerID, zoneID), out var zone))
        {
            _logger.LogInformation("Deleted zone {Controller}-{Zone}: {Name}", controllerID, zoneID, zone.Name);
            SaveZonesAsync().ConfigureAwait(false);
        }
    }

    public void DeleteSource(int sourceID)
    {
        if (_sources.TryRemove(sourceID, out var source))
        {
            _logger.LogInformation("Deleted source {SourceID}: {Name}", sourceID, source.Name);
            SaveSourcesAsync().ConfigureAwait(false);
        }
    }

    public void SetAllPower(bool power)
    {
        _logger.LogInformation("Setting all zones power to: {Power}", power);
        
        foreach (var zone in _zones.Values)
        {
            zone.SetPower(power);
        }
        
        // TODO: Send RNet packet to hardware
    }

    public void SetAllMute(bool mute, int fadeTime = 0)
    {
        _logger.LogInformation("Setting all zones mute to: {Mute} (fade: {FadeTime}ms)", mute, fadeTime);
        
        foreach (var zone in _zones.Values)
        {
            zone.SetMute(mute);
        }
        
        // TODO: Send RNet packet to hardware
    }

    private async Task RequestAllZoneInfoAsync()
    {
        // TODO: Implement RNet packet to request zone information
        _logger.LogDebug("Requesting all zone information from RNet device");
        await Task.CompletedTask;
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (_serialPort == null) return;
        
        try
        {
            int bytesToRead = _serialPort.BytesToRead;
            byte[] buffer = new byte[bytesToRead];
            _serialPort.Read(buffer, 0, bytesToRead);
            
            // TODO: Implement packet parsing and handling
            _logger.LogTrace("Received {ByteCount} bytes from RNet", bytesToRead);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing received data from RNet");
            Error?.Invoke(this, ex);
        }
    }

    private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
        _logger.LogError("Serial port error: {Error}", e.EventType);
        var exception = new InvalidOperationException($"Serial port error: {e.EventType}");
        Error?.Invoke(this, exception);
    }

    private async Task LoadPersistedDataAsync()
    {
        await LoadSourcesAsync();
        await LoadZonesAsync();
    }

    private async Task LoadSourcesAsync()
    {
        try
        {
            if (File.Exists(_sourcesFilePath))
            {
                var json = await File.ReadAllTextAsync(_sourcesFilePath);
                var sources = JsonSerializer.Deserialize<SourceData[]>(json);
                
                if (sources != null)
                {
                    for (int i = 0; i < sources.Length; i++)
                    {
                        var sourceData = sources[i];
                        if (sourceData != null)
                        {
                            var source = CreateSource(i, sourceData.Name, sourceData.Type);
                            if (sourceData.AutoOnZones != null)
                            {
                                foreach (var zoneRef in sourceData.AutoOnZones)
                                {
                                    source.AddAutoOnZone(zoneRef.ControllerID, zoneRef.ZoneID);
                                }
                            }
                            source.AutoOff = sourceData.AutoOff;
                            source.OverrideName = sourceData.OverrideName;
                        }
                    }
                }
                
                _logger.LogInformation("Loaded {Count} sources from {FilePath}", _sources.Count, _sourcesFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load sources from {FilePath}", _sourcesFilePath);
        }
    }

    private async Task SaveSourcesAsync()
    {
        try
        {
            var sourcesArray = new SourceData[_sources.Keys.Max() + 1];
            
            foreach (var kvp in _sources)
            {
                var source = kvp.Value;
                sourcesArray[source.SourceID] = new SourceData
                {
                    Name = source.Name,
                    Type = source.Type,
                    AutoOnZones = source.AutoOnZones.ToArray(),
                    AutoOff = source.AutoOff,
                    OverrideName = source.OverrideName
                };
            }
            
            var json = JsonSerializer.Serialize(sourcesArray, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            await File.WriteAllTextAsync(_sourcesFilePath, json);
            _logger.LogDebug("Saved sources to {FilePath}", _sourcesFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save sources to {FilePath}", _sourcesFilePath);
        }
    }

    private async Task LoadZonesAsync()
    {
        try
        {
            if (File.Exists(_zonesFilePath))
            {
                var json = await File.ReadAllTextAsync(_zonesFilePath);
                var zones = JsonSerializer.Deserialize<ZoneData[]>(json);
                
                if (zones != null)
                {
                    foreach (var zoneData in zones)
                    {
                        if (zoneData != null)
                        {
                            var zone = CreateZone(zoneData.ControllerID, zoneData.ZoneID, zoneData.Name);
                            zone.SetMaxVolume(zoneData.MaxVolume);
                            
                            for (int i = 0; i < zoneData.Parameters.Length && i < 9; i++)
                            {
                                zone.SetParameter(i, zoneData.Parameters[i]);
                            }
                        }
                    }
                }
                
                _logger.LogInformation("Loaded {Count} zones from {FilePath}", _zones.Count, _zonesFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load zones from {FilePath}", _zonesFilePath);
        }
    }

    private async Task SaveZonesAsync()
    {
        try
        {
            var zonesList = new List<ZoneData>();
            
            foreach (var zone in _zones.Values)
            {
                var parameters = new object[9];
                for (int i = 0; i < 9; i++)
                {
                    parameters[i] = zone.GetParameter(i);
                }
                
                zonesList.Add(new ZoneData
                {
                    ControllerID = zone.ControllerID,
                    ZoneID = zone.ZoneID,
                    Name = zone.Name ?? string.Empty,
                    MaxVolume = zone.MaxVolume,
                    Parameters = parameters
                });
            }
            
            var json = JsonSerializer.Serialize(zonesList, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            await File.WriteAllTextAsync(_zonesFilePath, json);
            _logger.LogDebug("Saved zones to {FilePath}", _zonesFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save zones to {FilePath}", _zonesFilePath);
        }
    }

    public async Task SetAllPowerAsync(bool power)
    {
        await Task.Run(() => SetAllPower(power));
    }

    public async Task SetAllMuteAsync(bool mute, int fadeTime = 0)
    {
        await Task.Run(() => SetAllMute(mute, fadeTime));
    }

    public void Dispose()
    {
        DisconnectAsync().ConfigureAwait(false);
    }

    // Data classes for serialization
    private class SourceData
    {
        public string Name { get; set; } = string.Empty;
        public SourceType Type { get; set; }
        public ZoneReference[]? AutoOnZones { get; set; }
        public bool AutoOff { get; set; }
        public bool OverrideName { get; set; }
    }

    private class ZoneData
    {
        public int ControllerID { get; set; }
        public int ZoneID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MaxVolume { get; set; } = 100;
        public object[] Parameters { get; set; } = new object[9];
    }
}