namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x04
/// Zone Name
/// Sends a zone's current name
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
///     (String) Zone Name
/// </summary>
public class PacketS2CZoneName : PacketS2C
{
    public const byte ID = 0x04;

    public PacketS2CZoneName(byte controllerID, byte zoneID, string name)
    {
        Writer.Write(controllerID);
        Writer.Write(zoneID);
        WriteNullTerminatedString(name);
    }

    public override byte GetID() => ID;
}