namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x65
/// Zone Mute
/// Mute / Unmute a Zone
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
///     (Unsigned Char) Muting
/// </summary>
public class PacketC2SZoneMute : PacketC2S
{
    public const byte ID = 0x65;

    public byte ControllerID { get; private set; }
    public byte ZoneID { get; private set; }
    public bool Muted { get; private set; }

    public PacketC2SZoneMute(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        ControllerID = Reader.ReadByte();
        ZoneID = Reader.ReadByte();
        Muted = Reader.ReadByte() == 1;
    }
}