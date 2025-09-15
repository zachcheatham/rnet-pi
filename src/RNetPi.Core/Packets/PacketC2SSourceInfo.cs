namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x06
/// Source Info
/// Update source info
/// Data:
///     (Unsigned Char) Source ID
///     (NT String) New Source Name
///     (Unsigned Char) Source Type ID [OPTIONAL]
/// </summary>
public class PacketC2SSourceInfo : PacketC2S
{
    public const byte ID = 0x06;

    public byte SourceID { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public byte SourceTypeID { get; private set; }

    public PacketC2SSourceInfo(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        SourceID = Reader.ReadByte();
        Name = ReadNullTerminatedString();
        
        if (Reader.BaseStream.Position < Reader.BaseStream.Length)
        {
            SourceTypeID = Reader.ReadByte();
        }
        else
        {
            SourceTypeID = 0;
        }
    }
}