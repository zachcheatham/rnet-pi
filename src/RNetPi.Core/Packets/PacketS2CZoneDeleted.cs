namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x05
/// Zone Deleted
/// Informs client of a deleted zone
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
/// </summary>
public class PacketS2CZoneDeleted : PacketS2C
{
    public const byte ID = 0x05;

    public PacketS2CZoneDeleted(byte controllerID, byte zoneID)
    {
        Writer.Write(controllerID);
        Writer.Write(zoneID);
    }

    public override byte GetID() => ID;
}