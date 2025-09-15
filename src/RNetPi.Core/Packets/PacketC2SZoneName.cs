namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x04
/// Zone Name
/// Renames a zone
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
///     (NT String) New Zone Name
/// </summary>
public class PacketC2SZoneName : PacketC2S
{
    public const byte ID = 0x04;

    public byte ControllerID { get; private set; }
    public byte ZoneID { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public PacketC2SZoneName(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        ControllerID = Reader.ReadByte();
        ZoneID = Reader.ReadByte();
        Name = ReadNullTerminatedString();
    }
}