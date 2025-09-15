using System;
using System.Collections.Generic;

namespace RNetPi.Core.Models;

public enum SourceType
{
    Generic = 0,
    GoogleCast = 1,
    Sonos = 2
}

public enum SourceControl
{
    Play,
    Pause,
    Stop,
    Next,
    Previous
}

public class Source
{
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
    public event Action<string>? NameChanged;
    public event Action<SourceType>? TypeChanged;
    public event Action<string>? MediaTitleChanged;
    public event Action<string>? MediaArtistChanged;
    public event Action<string>? MediaArtworkURLChanged;
    public event Action<bool>? MediaPlayingChanged;
    public event Action<string>? DescriptiveTextChanged;
    public event Action<SourceControl>? ControlRequested;

    public Source(int sourceID, string name, SourceType type)
    {
        SourceID = sourceID;
        Name = name;
        Type = type;
    }

    public void SetName(string name)
    {
        if (Name != name)
        {
            var oldName = Name;
            Name = name;
            NameChanged?.Invoke(name);
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

    public void SetMediaTitle(string? title)
    {
        if (MediaTitle != title)
        {
            MediaTitle = title;
            MediaTitleChanged?.Invoke(title ?? string.Empty);
        }
    }

    public void SetMediaArtist(string? artist)
    {
        if (MediaArtist != artist)
        {
            MediaArtist = artist;
            MediaArtistChanged?.Invoke(artist ?? string.Empty);
        }
    }

    public void SetMediaArtworkURL(string? artworkURL)
    {
        if (MediaArtworkURL != artworkURL)
        {
            MediaArtworkURL = artworkURL;
            MediaArtworkURL = artworkURL;
            MediaArtworkURLChanged?.Invoke(artworkURL ?? string.Empty);
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

    public void SetDescriptiveText(string? text)
    {
        if (DescriptiveText != text)
        {
            DescriptiveText = text;
            DescriptiveTextChanged?.Invoke(text ?? string.Empty);
        }
    }

    public void Control(SourceControl operation)
    {
        ControlRequested?.Invoke(operation);
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
}

public record ZoneReference(int ControllerID, int ZoneID);