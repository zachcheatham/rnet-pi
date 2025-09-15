namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x08
/// Zone Power
/// Turns a Zone On/Off
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
///     (Unsigned Char) On/Off State
/// </summary>
public class PacketC2SZonePower : PacketC2S
{
    public const byte ID = 0x08;

    public byte ControllerID { get; private set; }
    public byte ZoneID { get; private set; }
    public bool Powered { get; private set; }

    public PacketC2SZonePower(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        ControllerID = Reader.ReadByte();
        ZoneID = Reader.ReadByte();
        Powered = Reader.ReadByte() == 1;
    }
}