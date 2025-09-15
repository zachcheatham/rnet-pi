namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x33
/// Request Source Properties
/// Data:
///     (Unsigned Char) Source ID
/// </summary>
public class PacketC2SRequestSourceProperties : PacketC2S
{
    public const byte ID = 0x33;

    public byte SourceID { get; private set; }

    public PacketC2SRequestSourceProperties(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        SourceID = Reader.ReadByte();
    }
}