namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x0C
/// All Power
/// Sets on/off state of all zones
/// Data:
///     (Unsigned Char) On/Off
/// </summary>
public class PacketC2SAllPower : PacketC2S
{
    public const byte ID = 0x0C;

    public bool Powered { get; private set; }

    public PacketC2SAllPower(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        Powered = Reader.ReadByte() == 1;
    }
}