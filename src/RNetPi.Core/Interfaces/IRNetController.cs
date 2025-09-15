using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RNetPi.Core.Models;
using RNetPi.Core.RNet;

namespace RNetPi.Core.Interfaces;

/// <summary>
/// Interface for the main RNet service functionality
/// Corresponds to the rnet.js class
/// </summary>
public interface IRNetController
{
    // Connection status
    bool IsConnected { get; }
    bool AllMuted { get; }

    // Events
    event EventHandler? Connected;
    event EventHandler? Disconnected;
    event EventHandler<Exception>? Error;
    event EventHandler<Zone>? ZoneAdded;
    event EventHandler<Zone>? ZoneRemoved;
    event EventHandler<Source>? SourceAdded;
    event EventHandler<Source>? SourceRemoved;
    event EventHandler<(Zone Zone, bool Power)>? ZonePowerChanged;
    event EventHandler<(Zone Zone, int Volume)>? ZoneVolumeChanged;
    event EventHandler<(Zone Zone, int Source)>? ZoneSourceChanged;
    event EventHandler<(Zone Zone, bool Mute)>? ZoneMuteChanged;
    event EventHandler<(Zone Zone, int ParameterID, object Value)>? ZoneParameterChanged;
    event EventHandler<(Source Source, string Name)>? SourceNameChanged;
    event EventHandler<(Source Source, string? Title, string? Artist, string? ArtworkURL)>? SourceMediaMetadata;
    event EventHandler<(Source Source, bool Playing)>? SourceMediaPlaying;
    event EventHandler<(Source Source, string? Text, int FlashTime)>? SourceDescriptiveText;

    // Connection management
    Task<bool> ConnectAsync(string? device = null);
    Task DisconnectAsync();

    // Zone management
    Zone CreateZone(int controllerID, int zoneID, string name);
    bool DeleteZone(int controllerID, int zoneID);
    Zone? GetZone(int controllerID, int zoneID);
    Zone? FindZoneByName(string name);
    int GetControllersSize();
    int GetZonesSize(int controllerID);

    // Source management
    Source CreateSource(int sourceID, string name, SourceType type);
    bool DeleteSource(int sourceID);
    Source? GetSource(int sourceID);
    Source? FindSourceByName(string name);
    int GetSourcesSize();
    IReadOnlyList<Source> GetSources();
    IReadOnlyList<Source> GetSourcesByType(SourceType type);

    // Zone control
    Task RequestAllZoneInfoAsync(bool forceAll = false);
    Task SetAllPowerAsync(bool power);
    Task SetAllMuteAsync(bool muted, int fadeTime = 0);

    // Configuration management
    Task LoadConfigurationAsync();
    Task SaveConfigurationAsync();

    // Auto update
    void SetAutoUpdate(bool enabled);

    // Data sending
    Task SendDataAsync(RNetPacket packet);
}