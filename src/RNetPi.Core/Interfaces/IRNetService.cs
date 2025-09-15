using RNetPi.Core.Models;

namespace RNetPi.Core.Interfaces;

public interface IRNetService
{
    bool IsConnected { get; }
    
    Task<bool> ConnectAsync();
    Task DisconnectAsync();
    
    Zone? GetZone(int controllerID, int zoneID);
    Source? GetSource(int sourceID);
    
    IEnumerable<Zone> GetAllZones();
    IEnumerable<Source> GetAllSources();
    
    Zone CreateZone(int controllerID, int zoneID, string name);
    Source CreateSource(int sourceID, string name, SourceType type);
    
    void DeleteZone(int controllerID, int zoneID);
    void DeleteSource(int sourceID);
    
    void SetAllPower(bool power);
    void SetAllMute(bool mute, int fadeTime = 0);
    
    event EventHandler? Connected;
    event EventHandler? Disconnected;
    event EventHandler<Exception>? Error;
}