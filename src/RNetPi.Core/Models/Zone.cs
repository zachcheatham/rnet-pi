using System;
using System.Text.Json;
using System.Threading.Tasks;
using RNetPi.Core.Constants;

namespace RNetPi.Core.Models;

public class Zone
{
    private readonly Action<Zone, RNet.RequestDataPacket>? _sendDataCallback;
    private readonly Action<Zone, RNet.RequestParameterPacket>? _sendParameterCallback;
    private readonly Action<Zone, RNet.DisplayMessagePacket>? _sendDisplayCallback;

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
    public event Action<bool, bool>? PowerChanged; // power, rNetTriggered
    public event Action<int, bool>? VolumeChanged; // volume, rNetTriggered
    public event Action<int, bool>? SourceChanged; // source, rNetTriggered
    public event Action<bool>? MuteChanged;
    public event Action<int>? MaxVolumeChanged;
    public event Action<int, object, bool>? ParameterChanged; // parameterID, value, rNetTriggered

    // Backward compatibility events
    public event Action<bool>? PowerChangedSimple;
    public event Action<int>? VolumeChangedSimple;
    public event Action<int>? SourceChangedSimple;
    public event Action<int, object>? ParameterChangedSimple;

    public Zone(int controllerID, int zoneID)
        : this(controllerID, zoneID, null, null, null)
    {
    }

    public enum Parameter
    {
        Bass = 0,
        Treble = 1,
        Loudness = 2,
        Balance = 3,
        TurnOnVolume = 4,
        BackgroundColor = 5,
        DoNotDisturb = 6,
        PartyMode = 7,
        FrontAVEnable = 8
    }

    public Zone(int controllerID, int zoneID,
        Action<Zone, RNet.RequestDataPacket>? sendDataCallback = null,
        Action<Zone, RNet.RequestParameterPacket>? sendParameterCallback = null,
        Action<Zone, RNet.DisplayMessagePacket>? sendDisplayCallback = null)
    {
        ControllerID = controllerID;
        ZoneID = zoneID;
        _sendDataCallback = sendDataCallback;
        _sendParameterCallback = sendParameterCallback;
        _sendDisplayCallback = sendDisplayCallback;
        
        // Initialize parameters with default values
        _parameters[(int)Parameter.Bass] = 0;      // Bass             -10 - +10
        _parameters[(int)Parameter.Treble] = 0;      // Treble           -10 - +10
        _parameters[(int)Parameter.Loudness] = false;  // Loudness
        _parameters[(int)Parameter.Balance] = 0;      // Balance          -10 - +10
        _parameters[(int)Parameter.TurnOnVolume] = 50;      // Turn on Volume   0 - 100
        _parameters[(int)Parameter.BackgroundColor] = 0;      // Background Color 0 - 2
        _parameters[(int)Parameter.DoNotDisturb] = false;  // Do Not Disturb
        _parameters[(int)Parameter.PartyMode] = 0;      // Party Mode       0 - 2
        _parameters[(int)Parameter.FrontAVEnable] = false;  // Front AV Enable
    }

    public void SetName(string name)
    {
        if (Name != name)
        {
            Name = name;
            NameChanged?.Invoke(name);
        }
    }

    public void SetPower(bool power, bool rNetTriggered = false)
    {
        if (Power != power)
        {
            Power = power;
            PowerChanged?.Invoke(power, rNetTriggered);
            PowerChangedSimple?.Invoke(power); // Backward compatibility

            // Clear mute state when power changes
            if (Mute)
            {
                Mute = false;
                PreMuteVolume = 0;
                MuteChanged?.Invoke(false);
            }

            // Request info update after powering on
            if (power)
            {
                // Delay slightly to allow RNet to process
                Task.Delay(1000).ContinueWith(_ => RequestInfo());
            }
        }
    }

    public void SetVolume(int volume, bool rNetTriggered = false, bool forMute = false)
    {
        volume = Math.Clamp(volume, 0, 100);
        
        if (Volume != volume)
        {
            // Clear mute if volume changed and not for mute operation
            if (Mute && !forMute)
            {
                Mute = false;
                PreMuteVolume = 0;
                MuteChanged?.Invoke(false);
            }

            // Enforce max volume limit
            if (volume > MaxVolume)
            {
                volume = MaxVolume;
                rNetTriggered = false; // Override RNet triggering since we're limiting
            }

            Volume = volume;
            VolumeChanged?.Invoke(volume, rNetTriggered);
            VolumeChangedSimple?.Invoke(volume); // Backward compatibility
        }
    }

    public void SetSource(int source, bool rNetTriggered = false)
    {
        if (Source != source)
        {
            Source = source;
            SourceChanged?.Invoke(source, rNetTriggered);
            SourceChangedSimple?.Invoke(source); // Backward compatibility
        }
    }

    public void SetMute(bool mute, int fadeTimeMs = 0)
    {
        if (Mute != mute)
        {
            Mute = mute;

            if (fadeTimeMs == 0)
            {
                if (mute)
                {
                    PreMuteVolume = Volume;
                    SetVolume(0, false, true);
                }
                else
                {
                    SetVolume(PreMuteVolume, false, true);
                    PreMuteVolume = 0;
                }
            }
            else
            {
                // Note: C# implementation doesn't include fade animation like JS amator
                // This would require additional threading or timer implementation
                if (mute)
                {
                    PreMuteVolume = Volume;
                    SetVolume(0, false, true);
                }
                else
                {
                    SetVolume(PreMuteVolume, false, true);
                    PreMuteVolume = 0;
                }
            }

            MuteChanged?.Invoke(mute);
        }
    }

    public void SetMaxVolume(int maxVolume, bool save = true)
    {
        maxVolume = Math.Clamp(maxVolume, 1, 100);
        
        if (MaxVolume != maxVolume)
        {
            MaxVolume = maxVolume;
            
            // Adjust current volume if it exceeds new max
            if (Volume > MaxVolume)
            {
                SetVolume(MaxVolume);
            }

            MaxVolumeChanged?.Invoke(maxVolume);
        }
    }

    public object GetParameter(int parameterID)
    {
        if (parameterID < 0 || parameterID >= _parameters.Length)
            throw new ArgumentOutOfRangeException(nameof(parameterID));
        
        return _parameters[parameterID];
    }

    public void SetParameter(int parameterID, object value, bool rNetTriggered = false)
    {
        if (parameterID < 0 || parameterID >= _parameters.Length)
            throw new ArgumentOutOfRangeException(nameof(parameterID));

        // Validate parameter values based on type and range
        var validatedValue = ValidateParameterValue(parameterID, value);
        
        if (!_parameters[parameterID].Equals(validatedValue))
        {
            _parameters[parameterID] = validatedValue;
            ParameterChanged?.Invoke(parameterID, validatedValue, rNetTriggered);
            ParameterChangedSimple?.Invoke(parameterID, validatedValue); // Backward compatibility
        }
    }

    public void RequestInfo()
    {
        if (_sendDataCallback != null && _sendParameterCallback != null)
        {
            _sendDataCallback(this, new RNet.RequestDataPacket((byte)ControllerID, (byte)ZoneID, RNet.RequestDataPacket.DataType.ZoneInfo));
            _sendParameterCallback(this, new RNet.RequestParameterPacket((byte)ControllerID, (byte)ZoneID, (byte)ZoneParameters.TurnOnVolume));
        }
    }

    public void RequestBasicInfo()
    {
        if (_sendDataCallback != null)
        {
            _sendDataCallback(this, new RNet.RequestDataPacket((byte)ControllerID, (byte)ZoneID, RNet.RequestDataPacket.DataType.ZonePower));
            _sendDataCallback(this, new RNet.RequestDataPacket((byte)ControllerID, (byte)ZoneID, RNet.RequestDataPacket.DataType.ZoneVolume));
            _sendDataCallback(this, new RNet.RequestDataPacket((byte)ControllerID, (byte)ZoneID, RNet.RequestDataPacket.DataType.ZoneSource));
        }
    }

    public void RequestPower()
    {
        if (_sendDataCallback != null)
        {
            _sendDataCallback(this, new RNet.RequestDataPacket((byte)ControllerID, (byte)ZoneID, RNet.RequestDataPacket.DataType.ZonePower));
        }
    }

    public void DisplayMessage(string message, int flashTime = 0, RNet.DisplayMessagePacket.Alignment alignment = RNet.DisplayMessagePacket.Alignment.Left)
    {
        if (_sendDisplayCallback != null)
        {
            _sendDisplayCallback(this, new RNet.DisplayMessagePacket((byte)ControllerID, (byte)ZoneID, alignment, (byte)flashTime, message));
        }
    }

    private object ValidateParameterValue(int parameterID, object value)
    {
        // Convert JsonElement to appropriate type
        if (value is JsonElement jsonElement)
        {
            value = jsonElement.ValueKind switch
            {
                JsonValueKind.Number => jsonElement.GetInt32(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => jsonElement.GetString() ?? string.Empty,
                _ => value
            };
        }

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