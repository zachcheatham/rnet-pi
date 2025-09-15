namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x07
/// Source Deleted
/// Informs client of a deleted source
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
/// </summary>
public class PacketS2CSourceDeleted : PacketS2C
{
    public const byte ID = 0x07;

    public PacketS2CSourceDeleted(byte controllerID, byte zoneID)
    {
        Writer.Write(controllerID);
        Writer.Write(zoneID);
    }

    public override byte GetID() => ID;
}