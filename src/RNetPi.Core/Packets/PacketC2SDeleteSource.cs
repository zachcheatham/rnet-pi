namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x07
/// Delete Source
/// Deletes a Source
/// Data:
///     (Unsigned Char) Source ID
/// </summary>
public class PacketC2SDeleteSource : PacketC2S
{
    public const byte ID = 0x07;

    public byte SourceID { get; private set; }

    public PacketC2SDeleteSource(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        SourceID = Reader.ReadByte();
    }
}