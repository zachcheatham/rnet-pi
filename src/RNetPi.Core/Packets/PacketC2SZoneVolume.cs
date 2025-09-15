namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x09
/// Zone Volume
/// Sets a zone's volume
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
///     (Unsigned Char) Volume
/// </summary>
public class PacketC2SZoneVolume : PacketC2S
{
    public const byte ID = 0x09;

    public byte ControllerID { get; private set; }
    public byte ZoneID { get; private set; }
    public byte Volume { get; private set; }

    public PacketC2SZoneVolume(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        ControllerID = Reader.ReadByte();
        ZoneID = Reader.ReadByte();
        Volume = Reader.ReadByte();
    }
}