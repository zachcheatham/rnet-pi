namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x0A
/// Zone Source
/// Sets a zone's source
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
///     (Unsigned Char) Source ID
/// </summary>
public class PacketC2SZoneSource : PacketC2S
{
    public const byte ID = 0x0A;

    public byte ControllerID { get; private set; }
    public byte ZoneID { get; private set; }
    public byte SourceID { get; private set; }

    public PacketC2SZoneSource(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        ControllerID = Reader.ReadByte();
        ZoneID = Reader.ReadByte();
        SourceID = Reader.ReadByte();
    }
}