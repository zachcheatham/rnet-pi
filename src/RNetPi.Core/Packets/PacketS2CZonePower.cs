namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x08
/// Zone Power
/// Sends a zone's current power state
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
///     (Unsigned Char) Power State
/// </summary>
public class PacketS2CZonePower : PacketS2C
{
    public const byte ID = 0x08;

    public PacketS2CZonePower(byte controllerID, byte zoneID, bool powered)
    {
        Writer.Write(controllerID);
        Writer.Write(zoneID);
        Writer.Write((byte)(powered ? 1 : 0));
    }

    public override byte GetID() => ID;
}