using System;
using System.Collections.Generic;
using System.Linq;

namespace RNetPi.Core.Models;

public enum SourceType
{
    Generic = 0,
    Airplay = 1,
    Bluray = 2,
    Cable = 3,
    CD = 4,
    Computer = 5,
    DVD = 6,
    GoogleCast = 7,
    InternetRadio = 8,
    iPod = 9,
    MediaServer = 10,
    MP3 = 11,
    OTA = 12,
    Phono = 13,
    Radio = 14,
    Satellite = 15,
    SatelliteRadio = 16,
    Sonos = 17,
    Tape = 18,
    VCR = 19
}

public enum SourceControl
{
    Next = 0,
    Previous = 1,
    Stop = 2,
    Play = 3,
    Pause = 4,
    Plus = 5,
    Minus = 6
}

public class Source
{
    private readonly Func<int, int, Zone?> _getZone;
    private readonly Func<int> _getControllersSize;
    private readonly Func<int, int> _getZonesSize;

    public int SourceID { get; }
    public string Name { get; private set; } = string.Empty;
    public SourceType Type { get; private set; }
    public List<ZoneReference> AutoOnZones { get; } = new();
    public bool AutoOff { get; set; }
    public bool OverrideName { get; set; }

    // Media metadata
    public string? MediaTitle { get; private set; }
    public string? MediaArtist { get; private set; }
    public string? MediaArtworkURL { get; private set; }
    public bool MediaPlaying { get; private set; }

    // Descriptive text
    public string? DescriptiveText { get; private set; }
    public bool DescriptiveTextFromRNet { get; set; } = true;

    // Events
    public event Action<string, string>? NameChanged; // name, oldName
    public event Action<SourceType>? TypeChanged;
    public event Action<string?, string?, string?>? MediaMetadataChanged; // title, artist, artworkURL
    public event Action<bool>? MediaPlayingChanged;
    public event Action<string?, int>? DescriptiveTextChanged; // message, flashTime
    public event Action<SourceControl>? ControlRequested;
    public event Action? OverrideNameRequested;

    public Source(int sourceID, string name, SourceType type,
        Func<int, int, Zone?> getZone,
        Func<int> getControllersSize,
        Func<int, int> getZonesSize)
    {
        SourceID = sourceID;
        Name = name;
        Type = type;
        _getZone = getZone;
        _getControllersSize = getControllersSize;
        _getZonesSize = getZonesSize;
    }

    public void SetName(string name)
    {
        if (Name != name)
        {
            var oldName = Name;
            Name = name;
            NameChanged?.Invoke(name, oldName);
        }
    }

    public void SetType(SourceType type)
    {
        if (Type != type)
        {
            Type = type;
            TypeChanged?.Invoke(type);
        }
    }

    public void SetMediaMetadata(string? title, string? artist, string? artworkURL)
    {
        if (MediaTitle != title || MediaArtist != artist || MediaArtworkURL != artworkURL)
        {
            MediaTitle = title;
            MediaArtist = artist;
            MediaArtworkURL = artworkURL;
            MediaMetadataChanged?.Invoke(title, artist, artworkURL);
        }
    }

    public void SetMediaPlaying(bool playing)
    {
        if (MediaPlaying != playing)
        {
            MediaPlaying = playing;
            MediaPlayingChanged?.Invoke(playing);
        }
    }

    public void SetDescriptiveText(string? message, int flashTime = 0)
    {
        if (flashTime == 0)
        {
            DescriptiveText = message;
        }

        if (message == null)
        {
            message = Name;
        }

        DescriptiveTextChanged?.Invoke(message, flashTime);
    }

    public void Control(SourceControl operation)
    {
        ControlRequested?.Invoke(operation);
        
        // Update play state for non-network controlled sources
        if (!IsNetworkControlled())
        {
            switch (operation)
            {
                case SourceControl.Play:
                    SetMediaPlaying(true);
                    break;
                case SourceControl.Pause:
                case SourceControl.Stop:
                    SetMediaPlaying(false);
                    break;
            }
        }
    }

    public bool IsInUse()
    {
        for (int controllerID = 0; controllerID < _getControllersSize(); controllerID++)
        {
            for (int zoneID = 0; zoneID < _getZonesSize(controllerID); zoneID++)
            {
                var zone = _getZone(controllerID, zoneID);
                if (zone?.Source == SourceID && zone.Power)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public List<Zone> GetZones()
    {
        var zones = new List<Zone>();
        
        for (int controllerID = 0; controllerID < _getControllersSize(); controllerID++)
        {
            for (int zoneID = 0; zoneID < _getZonesSize(controllerID); zoneID++)
            {
                var zone = _getZone(controllerID, zoneID);
                if (zone?.Source == SourceID && zone.Power)
                {
                    zones.Add(zone);
                }
            }
        }
        
        return zones;
    }

    public bool IsNetworkControlled()
    {
        return Type switch
        {
            SourceType.GoogleCast => true,
            SourceType.Sonos => true,
            _ => false
        };
    }

    public void AddAutoOnZone(int controllerID, int zoneID)
    {
        var zoneRef = new ZoneReference(controllerID, zoneID);
        if (!AutoOnZones.Contains(zoneRef))
        {
            AutoOnZones.Add(zoneRef);
        }
    }

    public void RemoveAutoOnZone(int controllerID, int zoneID)
    {
        var zoneRef = new ZoneReference(controllerID, zoneID);
        AutoOnZones.Remove(zoneRef);
    }

    /// <summary>
    /// Handles power events for smart integration (auto on/off)
    /// </summary>
    public void OnPowerChanged(bool powered)
    {
        if (powered)
        {
            if (AutoOnZones.Count > 0 && !IsInUse())
            {
                foreach (var zoneRef in AutoOnZones)
                {
                    var zone = _getZone(zoneRef.ControllerID, zoneRef.ZoneID);
                    if (zone != null)
                    {
                        zone.SetPower(true);
                        zone.SetSource(SourceID);
                    }
                }
            }
        }
        else if (AutoOff)
        {
            var zones = GetZones();
            foreach (var zone in zones)
            {
                zone.SetPower(false);
            }
        }
    }

    public void RequestOverrideName()
    {
        if (OverrideName && DescriptiveText == null)
        {
            OverrideNameRequested?.Invoke();
        }
    }
}

public record ZoneReference(int ControllerID, int ZoneID);