using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RNetPi.Core.Interfaces;
using RNetPi.Core.Models;
using RNetPi.Core.RNet;

namespace RNetPi.Core.Services;

/// <summary>
/// Enhanced RNet service implementation using the ported packet infrastructure
/// </summary>
public class EnhancedRNetService : IRNetService, IDisposable
{
    private readonly ILogger<EnhancedRNetService> _logger;
    private readonly IConfigurationService _configService;
    private readonly ConcurrentDictionary<(int controllerID, int zoneID), Zone> _zones;
    private readonly ConcurrentDictionary<int, Source> _sources;
    private readonly Queue<RNetPacket> _packetQueue = new();
    private readonly object _packetQueueLock = new();
    
    private SerialPort? _serialPort;
    private bool _connected = false;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _processingTask;

    public bool IsConnected => _connected;
    public bool AllMuted { get; private set; } = false;

    // Events from IRNetService
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;
    public event EventHandler<Exception>? Error;

    // Additional events for enhanced functionality
    public event EventHandler<Zone>? ZoneAdded;
    public event EventHandler<Zone>? ZoneRemoved;
    public event EventHandler<Source>? SourceAdded;
    public event EventHandler<Source>? SourceRemoved;
    public event EventHandler<KeypadEventPacket>? KeypadEvent;
    public event EventHandler<string>? DisplayMessage;

    public EnhancedRNetService(ILogger<EnhancedRNetService> logger, IConfigurationService configService)
    {
        _logger = logger;
        _configService = configService;
        _zones = new ConcurrentDictionary<(int, int), Zone>();
        _sources = new ConcurrentDictionary<int, Source>();

        LoadConfiguration();
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

            var device = _configService.Configuration.SerialDevice ?? "/dev/ttyUSB0";
            _logger.LogInformation("Connecting to RNet device: {Device}", device);

            _serialPort = new SerialPort(device, 19200)
            {
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };

            _serialPort.DataReceived += OnDataReceived;
            _serialPort.ErrorReceived += OnErrorReceived;

            _serialPort.Open();
            _connected = true;

            // Start processing task
            _cancellationTokenSource = new CancellationTokenSource();
            _processingTask = ProcessPacketsAsync(_cancellationTokenSource.Token);

            _logger.LogInformation("Successfully connected to RNet device");
            Connected?.Invoke(this, EventArgs.Empty);

            // Send initial handshake if needed
            await SendHandshakeAsync();

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

            // Cancel processing task
            _cancellationTokenSource?.Cancel();
            if (_processingTask != null)
            {
                await _processingTask;
            }

            _serialPort?.Close();
            _serialPort?.Dispose();
            _serialPort = null;

            _logger.LogInformation("Disconnected from RNet device");
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnect");
            Error?.Invoke(this, ex);
        }
    }

    public Zone? GetZone(int controllerID, int zoneID)
    {
        _zones.TryGetValue((controllerID, zoneID), out var zone);
        return zone;
    }

    public Source? GetSource(int sourceID)
    {
        _sources.TryGetValue(sourceID, out var source);
        return source;
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

        // Subscribe to zone events
        zone.NameChanged += (newName) => SaveConfiguration();
        zone.PowerChangedSimple += (power) => SaveConfiguration();
        zone.VolumeChangedSimple += (volume) => SaveConfiguration();
        zone.SourceChangedSimple += (source) => SaveConfiguration();
        zone.MuteChanged += (mute) => SaveConfiguration();
        zone.ParameterChangedSimple += (paramId, value) => SaveConfiguration();

        _zones[(controllerID, zoneID)] = zone;
        SaveConfiguration();

        _logger.LogInformation("Created zone {ZoneID} on controller {ControllerID}: {Name}", 
            zoneID, controllerID, name);
        ZoneAdded?.Invoke(this, zone);

        return zone;
    }

    public Source CreateSource(int sourceID, string name, SourceType type)
    {
        var source = new Source(sourceID, name, type);

        // Subscribe to source events
        source.NameChanged += (newName, oldName) => SaveConfiguration();
        source.TypeChanged += (newType) => SaveConfiguration();
        source.MediaTitleChanged += (title) => { /* Handle media metadata */ };
        source.MediaArtistChanged += (artist) => { /* Handle media metadata */ };
        source.MediaPlayingChanged += (playing) => { /* Handle playback state */ };
        source.DescriptiveTextChanged += (text, flashTime) => { /* Handle descriptive text */ };

        _sources[sourceID] = source;
        SaveConfiguration();

        _logger.LogInformation("Created source {SourceID}: {Name} ({Type})", sourceID, name, type);
        SourceAdded?.Invoke(this, source);

        return source;
    }

    public void DeleteZone(int controllerID, int zoneID)
    {
        if (_zones.TryRemove((controllerID, zoneID), out var zone))
        {
            SaveConfiguration();
            _logger.LogInformation("Deleted zone {ZoneID} on controller {ControllerID}", zoneID, controllerID);
            ZoneRemoved?.Invoke(this, zone);
        }
    }

    public void DeleteSource(int sourceID)
    {
        if (_sources.TryRemove(sourceID, out var source))
        {
            SaveConfiguration();
            _logger.LogInformation("Deleted source {SourceID}", sourceID);
            SourceRemoved?.Invoke(this, source);
        }
    }

    public void SetAllPower(bool power)
    {
        try
        {
            var packet = new SetAllPowerPacket(power);
            EnqueuePacket(packet);
            _logger.LogInformation("Set all power to {Power}", power);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set all power");
            Error?.Invoke(this, ex);
        }
    }

    public void SetAllMute(bool mute, int fadeTime = 0)
    {
        try
        {
            // In a real implementation, this would need to send mute commands to all zones
            // For now, we'll track the state
            AllMuted = mute;
            _logger.LogInformation("Set all mute to {Mute} with fade time {FadeTime}ms", mute, fadeTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set all mute");
            Error?.Invoke(this, ex);
        }
    }

    // Enhanced functionality methods
    public void SetZonePower(int controllerID, int zoneID, bool power)
    {
        try
        {
            var packet = new SetPowerPacket((byte)controllerID, (byte)zoneID, power);
            EnqueuePacket(packet);

            // Update local zone state
            var zone = GetZone(controllerID, zoneID);
            zone?.SetPower(power);

            _logger.LogInformation("Set zone {ZoneID} power to {Power}", zoneID, power);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set zone power");
            Error?.Invoke(this, ex);
        }
    }

    public void SetZoneVolume(int controllerID, int zoneID, int volume)
    {
        try
        {
            var packet = new SetVolumePacket((byte)controllerID, (byte)zoneID, volume);
            EnqueuePacket(packet);

            // Update local zone state
            var zone = GetZone(controllerID, zoneID);
            zone?.SetVolume(volume);

            _logger.LogInformation("Set zone {ZoneID} volume to {Volume}", zoneID, volume);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set zone volume");
            Error?.Invoke(this, ex);
        }
    }

    public void SetZoneSource(int controllerID, int zoneID, int sourceID)
    {
        try
        {
            var packet = new SetSourcePacket((byte)controllerID, (byte)zoneID, (byte)sourceID);
            EnqueuePacket(packet);

            // Update local zone state
            var zone = GetZone(controllerID, zoneID);
            zone?.SetSource(sourceID);

            _logger.LogInformation("Set zone {ZoneID} source to {SourceID}", zoneID, sourceID);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set zone source");
            Error?.Invoke(this, ex);
        }
    }

    public void SetZoneParameter(int controllerID, int zoneID, int parameterID, byte value)
    {
        try
        {
            var packet = new SetParameterPacket((byte)controllerID, (byte)zoneID, (byte)parameterID, value);
            EnqueuePacket(packet);

            // Update local zone state
            var zone = GetZone(controllerID, zoneID);
            zone?.SetParameter(parameterID, value);

            _logger.LogInformation("Set zone {ZoneID} parameter {ParameterID} to {Value}", 
                zoneID, parameterID, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set zone parameter");
            Error?.Invoke(this, ex);
        }
    }

    public void RequestZoneInfo(int controllerID, int zoneID)
    {
        try
        {
            var packet = RequestDataPacket.CreateZoneInfoRequest((byte)controllerID, (byte)zoneID);
            EnqueuePacket(packet);
            _logger.LogDebug("Requested zone info for zone {ZoneID}", zoneID);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to request zone info");
            Error?.Invoke(this, ex);
        }
    }

    public void SendDisplayMessage(int controllerID, int zoneID, string message, byte displayTime = 5)
    {
        try
        {
            var packet = new DisplayMessagePacket((byte)controllerID, (byte)zoneID, DisplayMessagePacket.Alignment.Left, (byte)displayTime, message);
            EnqueuePacket(packet);
            _logger.LogInformation("Sent display message to zone {ZoneID}: {Message}", zoneID, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send display message");
            Error?.Invoke(this, ex);
        }
    }

    private void EnqueuePacket(RNetPacket packet)
    {
        lock (_packetQueueLock)
        {
            _packetQueue.Enqueue(packet);
        }
    }

    private async Task SendHandshakeAsync()
    {
        if (!_connected) return;

        try
        {
            var handshakePacket = new HandshakePacket(0x00, 0x01);
            await SendPacketAsync(handshakePacket);
            _logger.LogDebug("Sent handshake packet");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send handshake");
            Error?.Invoke(this, ex);
        }
    }

    private async Task SendPacketAsync(RNetPacket packet)
    {
        if (!_connected || _serialPort == null) return;

        try
        {
            var buffer = packet.GetBuffer();
            await _serialPort.BaseStream.WriteAsync(buffer, 0, buffer.Length);
            await _serialPort.BaseStream.FlushAsync();
            
            _logger.LogTrace("Sent packet: {PacketType} ({Size} bytes)", 
                packet.GetType().Name, buffer.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send packet");
            Error?.Invoke(this, ex);
        }
    }

    private async Task ProcessPacketsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _connected)
        {
            try
            {
                RNetPacket? packet = null;
                lock (_packetQueueLock)
                {
                    if (_packetQueue.Count > 0)
                    {
                        packet = _packetQueue.Dequeue();
                    }
                }

                if (packet != null)
                {
                    await SendPacketAsync(packet);
                }

                await Task.Delay(10, cancellationToken); // Small delay to prevent tight loop
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in packet processing loop");
                Error?.Invoke(this, ex);
            }
        }
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (_serialPort == null) return;

        try
        {
            var bytesToRead = _serialPort.BytesToRead;
            if (bytesToRead == 0) return;

            var buffer = new byte[bytesToRead];
            _serialPort.Read(buffer, 0, bytesToRead);

            // Process received data - this would need packet parsing logic
            ProcessReceivedData(buffer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing received data");
            Error?.Invoke(this, ex);
        }
    }

    private void ProcessReceivedData(byte[] buffer)
    {
        try
        {
            // Use PacketBuilder to parse the received data
            var packet = PacketBuilder.Build(buffer);
            if (packet == null)
            {
                _logger.LogWarning("Received unrecognized packet");
                return;
            }

            _logger.LogTrace("Received packet: {PacketType}", packet.GetType().Name);

            // Handle different packet types
            switch (packet)
            {
                case ZoneInfoPacket zoneInfo:
                    HandleZoneInfoPacket(zoneInfo);
                    break;
                case ZonePowerPacket zonePower:
                    HandleZonePowerPacket(zonePower);
                    break;
                case ZoneVolumePacket zoneVolume:
                    HandleZoneVolumePacket(zoneVolume);
                    break;
                case ZoneSourcePacket zoneSource:
                    HandleZoneSourcePacket(zoneSource);
                    break;
                case ZoneParameterPacket zoneParameter:
                    HandleZoneParameterPacket(zoneParameter);
                    break;
                case KeypadEventPacket keypadEvent:
                    HandleKeypadEventPacket(keypadEvent);
                    break;
                case RenderedDisplayMessagePacket displayMessage:
                    HandleDisplayMessagePacket(displayMessage);
                    break;
                case SourceDescriptiveTextPacket sourceText:
                    HandleSourceDescriptiveTextPacket(sourceText);
                    break;
                case HandshakePacket handshake:
                    HandleHandshakePacket(handshake);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling received packet");
            Error?.Invoke(this, ex);
        }
    }

    private void HandleZoneInfoPacket(ZoneInfoPacket packet)
    {
        var controllerID = packet.GetControllerID();
        var zoneID = packet.GetZoneID();
        
        var zone = GetZone(controllerID, zoneID);
        if (zone != null)
        {
            zone.SetPower(packet.GetPower());
            zone.SetVolume(packet.GetVolume());
            zone.SetSource(packet.GetSourceID());
            zone.SetParameter(0, packet.GetBassLevel() + 10); // Bass
            zone.SetParameter(1, packet.GetTrebleLevel() + 10); // Treble
            zone.SetParameter(2, packet.GetLoudness()); // Loudness
            zone.SetParameter(3, packet.GetBalance() + 10); // Balance
            zone.SetParameter(7, packet.GetPartyMode()); // Party Mode
            zone.SetParameter(6, packet.GetDoNotDisturbMode() != 0); // Do Not Disturb
        }
    }

    private void HandleZonePowerPacket(ZonePowerPacket packet)
    {
        var zone = GetZone(packet.GetControllerID(), packet.GetZoneID());
        zone?.SetPower(packet.GetPower());
    }

    private void HandleZoneVolumePacket(ZoneVolumePacket packet)
    {
        var zone = GetZone(packet.GetControllerID(), packet.GetZoneID());
        zone?.SetVolume(packet.GetVolume());
    }

    private void HandleZoneSourcePacket(ZoneSourcePacket packet)
    {
        var zone = GetZone(packet.GetControllerID(), packet.GetZoneID());
        zone?.SetSource(packet.GetSourceID());
    }

    private void HandleZoneParameterPacket(ZoneParameterPacket packet)
    {
        var zone = GetZone(packet.GetControllerID(), packet.GetZoneID());
        zone?.SetParameter(packet.GetParameterID(), packet.GetParameterValue());
    }

    private void HandleKeypadEventPacket(KeypadEventPacket packet)
    {
        _logger.LogInformation("Keypad event: Controller {ControllerID}, Zone {ZoneID}, Key {KeyID}",
            packet.GetControllerID(), packet.GetZoneID(), packet.GetKeyID());
        KeypadEvent?.Invoke(this, packet);
    }

    private void HandleDisplayMessagePacket(RenderedDisplayMessagePacket packet)
    {
        // Extract message text from display data if possible
        var message = System.Text.Encoding.UTF8.GetString(packet.DisplayData);
        _logger.LogInformation("Display message from Controller {ControllerID}, Zone {ZoneID}: {Message}",
            packet.GetControllerID(), packet.GetZoneID(), message);
        DisplayMessage?.Invoke(this, message);
    }

    private void HandleSourceDescriptiveTextPacket(SourceDescriptiveTextPacket packet)
    {
        var source = GetSource(packet.GetSourceID());
        if (source != null)
        {
            source.SetDescriptiveText(packet.GetDescriptiveText());
        }
    }

    private void HandleHandshakePacket(HandshakePacket packet)
    {
        _logger.LogDebug("Received handshake response");
    }

    private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
        _logger.LogError("Serial port error: {Error}", e.EventType);
        Error?.Invoke(this, new Exception($"Serial port error: {e.EventType}"));
    }

    private void LoadConfiguration()
    {
        // TODO: Implement configuration loading from JSON files
        // This would load zones.json and sources.json like in the JavaScript implementation
    }

    private void SaveConfiguration()
    {
        // TODO: Implement configuration saving to JSON files
        // This would save zones.json and sources.json like in the JavaScript implementation
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
        DisconnectAsync().Wait();
        _cancellationTokenSource?.Dispose();
    }
}