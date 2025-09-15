namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x09
/// Zone Volume
/// Sends a zone's current volume
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
///     (Unsigned Char) Volume
/// </summary>
public class PacketS2CZoneVolume : PacketS2C
{
    public const byte ID = 0x09;

    public PacketS2CZoneVolume(byte controllerID, byte zoneID, byte volume)
    {
        Writer.Write(controllerID);
        Writer.Write(zoneID);
        Writer.Write(volume);
    }

    public override byte GetID() => ID;
}