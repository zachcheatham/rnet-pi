namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x65
/// Zone Mute
/// Sends a zone's current mute state
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
///     (Unsigned Char) Muted
/// </summary>
public class PacketS2CZoneMute : PacketS2C
{
    public const byte ID = 0x65;

    public PacketS2CZoneMute(byte controllerID, byte zoneID, bool muted)
    {
        Writer.Write(controllerID);
        Writer.Write(zoneID);
        Writer.Write((byte)(muted ? 1 : 0));
    }

    public override byte GetID() => ID;
}