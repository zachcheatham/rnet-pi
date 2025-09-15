using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RNetPi.Core.Constants;
using RNetPi.Core.Interfaces;
using RNetPi.Core.Models;
using RNetPi.Core.RNet;

namespace RNetPi.Core.Services;

/// <summary>
/// Simple RNet controller implementation porting core functionality from rnet.js
/// </summary>
public class SimpleRNetController : IRNetController, IDisposable
{
    private readonly ILogger<SimpleRNetController> _logger;
    private readonly ConcurrentDictionary<(int controllerID, int zoneID), Zone> _zones = new();
    private readonly ConcurrentDictionary<int, Source> _sources = new();
    
    private SerialPort? _serialPort;
    private bool _connected = false;
    private bool _allMuted = false;
    private Timer? _autoUpdateTimer;

    public bool IsConnected => _connected;
    public bool AllMuted => _allMuted;

    // Events from IRNetController
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;
    public event EventHandler<Exception>? Error;
    public event EventHandler<Zone>? ZoneAdded;
    public event EventHandler<Zone>? ZoneRemoved;
    public event EventHandler<Source>? SourceAdded;
    public event EventHandler<Source>? SourceRemoved;
    public event EventHandler<(Zone Zone, bool Power)>? ZonePowerChanged;
    public event EventHandler<(Zone Zone, int Volume)>? ZoneVolumeChanged;
    public event EventHandler<(Zone Zone, int Source)>? ZoneSourceChanged;
    public event EventHandler<(Zone Zone, bool Mute)>? ZoneMuteChanged;
    public event EventHandler<(Zone Zone, int ParameterID, object Value)>? ZoneParameterChanged;
    public event EventHandler<(Source Source, string Name)>? SourceNameChanged;
    public event EventHandler<(Source Source, string? Title, string? Artist, string? ArtworkURL)>? SourceMediaMetadata;
    public event EventHandler<(Source Source, bool Playing)>? SourceMediaPlaying;
    public event EventHandler<(Source Source, string? Text, int FlashTime)>? SourceDescriptiveText;

    public SimpleRNetController(ILogger<SimpleRNetController> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ConnectAsync(string? device = null)
    {
        try
        {
            device ??= "/dev/ttyUSB0";
            _logger.LogInformation("Connecting to RNet device: {Device}", device);

            _serialPort = new SerialPort(device, 19200);
            _serialPort.Open();
            _connected = true;

            _logger.LogInformation("Successfully connected to RNet device");
            Connected?.Invoke(this, EventArgs.Empty);
            
            await LoadConfigurationAsync();
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
        try
        {
            _connected = false;
            _serialPort?.Close();
            _serialPort?.Dispose();
            _serialPort = null;
            
            _autoUpdateTimer?.Dispose();
            _autoUpdateTimer = null;

            _logger.LogInformation("Disconnected from RNet device");
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnect");
            Error?.Invoke(this, ex);
        }
    }

    public Zone CreateZone(int controllerID, int zoneID, string name)
    {
        var key = (controllerID, zoneID);
        if (_zones.TryGetValue(key, out var existingZone))
        {
            if (existingZone.Name != name)
            {
                existingZone.SetName(name);
                SaveConfigurationAsync();
            }
            return existingZone;
        }

        var zone = new Zone(controllerID, zoneID);
        zone.SetName(name);

        // Subscribe to events
        zone.NameChanged += _ => SaveConfigurationAsync();
        zone.PowerChangedSimple += power => ZonePowerChanged?.Invoke(this, (zone, power));
        zone.VolumeChangedSimple += volume => ZoneVolumeChanged?.Invoke(this, (zone, volume));
        zone.SourceChangedSimple += source => ZoneSourceChanged?.Invoke(this, (zone, source));
        zone.MuteChanged += mute => ZoneMuteChanged?.Invoke(this, (zone, mute));
        zone.ParameterChangedSimple += (paramId, value) => ZoneParameterChanged?.Invoke(this, (zone, paramId, value));

        _zones[key] = zone;
        _logger.LogInformation("Created zone {ControllerID}-{ZoneID}: {Name}", controllerID, zoneID, name);
        ZoneAdded?.Invoke(this, zone);
        SaveConfigurationAsync();

        return zone;
    }

    public bool DeleteZone(int controllerID, int zoneID)
    {
        var key = (controllerID, zoneID);
        if (_zones.TryRemove(key, out var zone))
        {
            _logger.LogInformation("Deleted zone {ControllerID}-{ZoneID}", controllerID, zoneID);
            ZoneRemoved?.Invoke(this, zone);
            SaveConfigurationAsync();
            return true;
        }
        return false;
    }

    public Zone? GetZone(int controllerID, int zoneID)
    {
        return _zones.TryGetValue((controllerID, zoneID), out var zone) ? zone : null;
    }

    public Zone? FindZoneByName(string name)
    {
        var upperName = name.ToUpperInvariant();
        return _zones.Values.FirstOrDefault(z => z.Name?.ToUpperInvariant() == upperName);
    }

    public int GetControllersSize()
    {
        return _zones.Keys.Select(k => k.controllerID).DefaultIfEmpty(-1).Max() + 1;
    }

    public int GetZonesSize(int controllerID)
    {
        return _zones.Keys.Where(k => k.controllerID == controllerID)
                          .Select(k => k.zoneID)
                          .DefaultIfEmpty(-1)
                          .Max() + 1;
    }

    public Source CreateSource(int sourceID, string name, SourceType type)
    {
        if (_sources.TryGetValue(sourceID, out var existingSource))
        {
            if (existingSource.Name != name || existingSource.Type != type)
            {
                existingSource.SetName(name);
                existingSource.SetType(type);
                SaveConfigurationAsync();
            }
            return existingSource;
        }

        var source = new Source(sourceID, name, type, GetZone, GetControllersSize, GetZonesSize);

        // Subscribe to events
        source.NameChanged += (newName, _) => {
            SourceNameChanged?.Invoke(this, (source, newName));
            SaveConfigurationAsync();
        };
        source.TypeChanged += _ => SaveConfigurationAsync();
        source.MediaMetadataChanged += (title, artist, artworkURL) => 
            SourceMediaMetadata?.Invoke(this, (source, title, artist, artworkURL));
        source.MediaPlayingChanged += playing => 
            SourceMediaPlaying?.Invoke(this, (source, playing));
        source.DescriptiveTextChanged += (message, flashTime) => 
            SourceDescriptiveText?.Invoke(this, (source, message, flashTime));

        _sources[sourceID] = source;
        _logger.LogInformation("Created source {SourceID}: {Name} ({Type})", sourceID, name, type);
        SourceAdded?.Invoke(this, source);
        SaveConfigurationAsync();

        return source;
    }

    public bool DeleteSource(int sourceID)
    {
        if (_sources.TryRemove(sourceID, out var source))
        {
            _logger.LogInformation("Deleted source {SourceID}", sourceID);
            SourceRemoved?.Invoke(this, source);
            SaveConfigurationAsync();
            return true;
        }
        return false;
    }

    public Source? GetSource(int sourceID)
    {
        return _sources.TryGetValue(sourceID, out var source) ? source : null;
    }

    public Source? FindSourceByName(string name)
    {
        var upperName = name.ToUpperInvariant();
        return _sources.Values.FirstOrDefault(s => s.Name.ToUpperInvariant() == upperName);
    }

    public int GetSourcesSize() => _sources.Count;
    public IReadOnlyList<Source> GetSources() => _sources.Values.ToList().AsReadOnly();
    public IReadOnlyList<Source> GetSourcesByType(SourceType type) => 
        _sources.Values.Where(s => s.Type == type).ToList().AsReadOnly();

    public async Task RequestAllZoneInfoAsync(bool forceAll = false)
    {
        foreach (var zone in _zones.Values)
        {
            zone.RequestInfo();
        }
    }

    public async Task SetAllPowerAsync(bool power)
    {
        foreach (var zone in _zones.Values)
        {
            zone.SetPower(power);
        }
    }

    public async Task SetAllMuteAsync(bool muted, int fadeTime = 0)
    {
        _allMuted = muted;
        foreach (var zone in _zones.Values.Where(z => z.Power))
        {
            zone.SetMute(muted, fadeTime);
        }
    }

    public void SetAutoUpdate(bool enabled)
    {
        if (enabled)
        {
            _autoUpdateTimer = new Timer(_ => RequestAllZoneInfoAsync(), 
                null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }
        else
        {
            _autoUpdateTimer?.Dispose();
            _autoUpdateTimer = null;
        }
    }

    public async Task SendDataAsync(RNetPacket packet)
    {
        if (_serialPort?.IsOpen == true)
        {
            await _serialPort.BaseStream.WriteAsync(packet.GetBuffer());
            _logger.LogDebug("Sent packet {PacketType} to RNet", packet.GetType().Name);
        }
    }

    public async Task LoadConfigurationAsync()
    {
        await LoadSourcesAsync();
        await LoadZonesAsync();
    }

    public async Task SaveConfigurationAsync()
    {
        try
        {
            await SaveSourcesAsync();
            await SaveZonesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration");
        }
    }

    private async Task LoadSourcesAsync()
    {
        try
        {
            if (!File.Exists("sources.json")) return;

            var json = await File.ReadAllTextAsync("sources.json");
            var sourcesArray = JsonSerializer.Deserialize<JsonElement[]>(json);

            for (int sourceID = 0; sourceID < sourcesArray.Length; sourceID++)
            {
                if (sourcesArray[sourceID].ValueKind != JsonValueKind.Null)
                {
                    var sourceData = sourcesArray[sourceID];
                    var name = sourceData.GetProperty("name").GetString() ?? $"Source {sourceID}";
                    var type = SourceType.Generic;
                    
                    if (sourceData.TryGetProperty("type", out var typeElement))
                    {
                        type = (SourceType)typeElement.GetInt32();
                    }

                    CreateSource(sourceID, name, type);
                }
            }
            _logger.LogInformation("Loaded {Count} sources from configuration", _sources.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load sources configuration");
        }
    }

    private async Task LoadZonesAsync()
    {
        try
        {
            if (!File.Exists("zones.json")) return;

            var json = await File.ReadAllTextAsync("zones.json");
            var controllersArray = JsonSerializer.Deserialize<JsonElement[][]>(json);

            for (int controllerID = 0; controllerID < controllersArray.Length; controllerID++)
            {
                if (controllersArray[controllerID] != null)
                {
                    var zonesArray = controllersArray[controllerID];
                    for (int zoneID = 0; zoneID < zonesArray.Length; zoneID++)
                    {
                        if (zonesArray[zoneID].ValueKind != JsonValueKind.Null)
                        {
                            var zoneData = zonesArray[zoneID];
                            var name = zoneData.GetProperty("name").GetString() ?? $"Zone {controllerID}-{zoneID}";
                            
                            var zone = CreateZone(controllerID, zoneID, name);

                            if (zoneData.TryGetProperty("maxvol", out var maxVolElement))
                            {
                                zone.SetMaxVolume(maxVolElement.GetInt32(), false);
                            }
                        }
                    }
                }
            }
            _logger.LogInformation("Loaded {Count} zones from configuration", _zones.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load zones configuration");
        }
    }

    private async Task SaveSourcesAsync()
    {
        var maxSourceID = _sources.Keys.DefaultIfEmpty(-1).Max();
        var sourcesArray = new object?[maxSourceID + 1];

        foreach (var kvp in _sources)
        {
            sourcesArray[kvp.Key] = new { name = kvp.Value.Name, type = (int)kvp.Value.Type };
        }

        var json = JsonSerializer.Serialize(sourcesArray, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("sources.json", json);
    }

    private async Task SaveZonesAsync()
    {
        var maxControllerID = _zones.Keys.Select(k => k.controllerID).DefaultIfEmpty(-1).Max();
        var controllersArray = new object?[maxControllerID + 1];

        for (int controllerID = 0; controllerID <= maxControllerID; controllerID++)
        {
            var zonesForController = _zones.Where(kvp => kvp.Key.controllerID == controllerID).ToList();
            if (zonesForController.Any())
            {
                var maxZoneID = zonesForController.Select(kvp => kvp.Key.zoneID).Max();
                var zonesArray = new object?[maxZoneID + 1];

                foreach (var kvp in zonesForController)
                {
                    var zone = kvp.Value;
                    zonesArray[kvp.Key.zoneID] = new { name = zone.Name };
                }

                controllersArray[controllerID] = zonesArray;
            }
        }

        var json = JsonSerializer.Serialize(controllersArray, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("zones.json", json);
    }

    public void Dispose()
    {
        DisconnectAsync().Wait();
        _autoUpdateTimer?.Dispose();
    }
}