using System;

namespace RNetPi.Core.Models;

public class Zone
{
    public int ControllerID { get; }
    public int ZoneID { get; }
    public string? Name { get; private set; }
    public bool Power { get; private set; }
    public int Volume { get; private set; }
    public int Source { get; private set; }
    public bool Mute { get; private set; }
    public int PreMuteVolume { get; private set; }
    public int MaxVolume { get; private set; } = 100;

    // Zone parameters: Bass, Treble, Loudness, Balance, TurnOnVolume, BackgroundColor, DoNotDisturb, PartyMode, FrontAVEnable
    private readonly object[] _parameters = new object[9];

    // Events
    public event Action<string>? NameChanged;
    public event Action<bool>? PowerChanged;
    public event Action<int>? VolumeChanged;
    public event Action<int>? SourceChanged;
    public event Action<bool>? MuteChanged;
    public event Action<int, object>? ParameterChanged;

    public Zone(int controllerID, int zoneID)
    {
        ControllerID = controllerID;
        ZoneID = zoneID;
        
        // Initialize parameters with default values
        _parameters[0] = 0;      // Bass             -10 - +10
        _parameters[1] = 0;      // Treble           -10 - +10
        _parameters[2] = false;  // Loudness
        _parameters[3] = 0;      // Balance          -10 - +10
        _parameters[4] = 0;      // Turn on Volume   0 - 100
        _parameters[5] = 0;      // Background Color 0 - 2
        _parameters[6] = false;  // Do Not Disturb
        _parameters[7] = 0;      // Party Mode       0 - 2
        _parameters[8] = false;  // Front AV Enable
    }

    public void SetName(string name)
    {
        if (Name != name)
        {
            Name = name;
            NameChanged?.Invoke(name);
        }
    }

    public void SetPower(bool power)
    {
        if (Power != power)
        {
            Power = power;
            PowerChanged?.Invoke(power);
        }
    }

    public void SetVolume(int volume)
    {
        if (Volume != volume)
        {
            Volume = Math.Clamp(volume, 0, MaxVolume);
            VolumeChanged?.Invoke(Volume);
        }
    }

    public void SetSource(int source)
    {
        if (Source != source)
        {
            Source = source;
            SourceChanged?.Invoke(source);
        }
    }

    public void SetMute(bool mute)
    {
        if (Mute != mute)
        {
            if (mute)
            {
                PreMuteVolume = Volume;
            }
            Mute = mute;
            MuteChanged?.Invoke(mute);
        }
    }

    public void SetMaxVolume(int maxVolume)
    {
        MaxVolume = Math.Clamp(maxVolume, 1, 100);
        if (Volume > MaxVolume)
        {
            SetVolume(MaxVolume);
        }
    }

    public object GetParameter(int parameterID)
    {
        if (parameterID < 0 || parameterID >= _parameters.Length)
            throw new ArgumentOutOfRangeException(nameof(parameterID));
        
        return _parameters[parameterID];
    }

    public void SetParameter(int parameterID, object value)
    {
        if (parameterID < 0 || parameterID >= _parameters.Length)
            throw new ArgumentOutOfRangeException(nameof(parameterID));

        // Validate parameter values based on type and range
        var validatedValue = ValidateParameterValue(parameterID, value);
        
        if (!_parameters[parameterID].Equals(validatedValue))
        {
            _parameters[parameterID] = validatedValue;
            ParameterChanged?.Invoke(parameterID, validatedValue);
        }
    }

    private object ValidateParameterValue(int parameterID, object value)
    {
        return parameterID switch
        {
            0 or 1 or 3 => Math.Clamp(Convert.ToInt32(value), -10, 10), // Bass, Treble, Balance
            4 => Math.Clamp(Convert.ToInt32(value), 0, 100), // Turn on Volume
            5 or 7 => Math.Clamp(Convert.ToInt32(value), 0, 2), // Background Color, Party Mode
            2 or 6 or 8 => Convert.ToBoolean(value), // Loudness, Do Not Disturb, Front AV Enable
            _ => value
        };
    }
}