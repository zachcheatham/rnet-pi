namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x0A
/// Zone Source
/// Sends a zone's current source
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
///     (Unsigned Char) Source ID
/// </summary>
public class PacketS2CZoneSource : PacketS2C
{
    public const byte ID = 0x0A;

    public PacketS2CZoneSource(byte controllerID, byte zoneID, byte sourceID)
    {
        Writer.Write(controllerID);
        Writer.Write(zoneID);
        Writer.Write(sourceID);
    }

    public override byte GetID() => ID;
}