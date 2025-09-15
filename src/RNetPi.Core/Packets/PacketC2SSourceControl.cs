namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x32
/// Source Control
/// Control source media
/// Data:
///     (Unsigned Char) Source ID
///     (Unsigned Char) Button ID
/// </summary>
public class PacketC2SSourceControl : PacketC2S
{
    public const byte ID = 0x32;

    public byte SourceID { get; private set; }
    public byte Operation { get; private set; }

    public PacketC2SSourceControl(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        SourceID = Reader.ReadByte();
        Operation = Reader.ReadByte();
    }
}