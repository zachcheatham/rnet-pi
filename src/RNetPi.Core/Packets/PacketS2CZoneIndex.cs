namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x03
/// Zone Index
/// Informs client of all the existing zones when it connects
/// Data:
///     (Array of) (Unsigned Char) Controller ID
///     (Array of) (Unsigned Char) Zone ID
/// </summary>
public class PacketS2CZoneIndex : PacketS2C
{
    public const byte ID = 0x03;

    public PacketS2CZoneIndex(IEnumerable<(byte ControllerID, byte ZoneID)> zones)
    {
        foreach (var (controllerID, zoneID) in zones)
        {
            Writer.Write(controllerID);
            Writer.Write(zoneID);
        }
    }

    public override byte GetID() => ID;
}