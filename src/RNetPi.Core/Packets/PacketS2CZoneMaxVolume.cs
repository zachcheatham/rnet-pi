namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x64
/// Zone Max Volume
/// Sends a zone's current max volume
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
///     (Unsigned Char) Max Volume
/// </summary>
public class PacketS2CZoneMaxVolume : PacketS2C
{
    public const byte ID = 0x64;

    public PacketS2CZoneMaxVolume(byte controllerID, byte zoneID, byte maxVolume)
    {
        Writer.Write(controllerID);
        Writer.Write(zoneID);
        Writer.Write(maxVolume);
    }

    public override byte GetID() => ID;
}