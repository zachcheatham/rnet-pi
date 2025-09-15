namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x64
/// Zone Max Volume
/// Sets a zone's max volume
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
///     (Unsigned Char) Max Volume
/// </summary>
public class PacketC2SZoneMaxVolume : PacketC2S
{
    public const byte ID = 0x64;

    public byte ControllerID { get; private set; }
    public byte ZoneID { get; private set; }
    public byte MaxVolume { get; private set; }

    public PacketC2SZoneMaxVolume(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        ControllerID = Reader.ReadByte();
        ZoneID = Reader.ReadByte();
        MaxVolume = Reader.ReadByte();
    }
}