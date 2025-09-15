namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x0D
/// Mute
/// Data:
///     (Unsigned Char) Mute state
///     (Unsigned Short) Fade Time
///     (Optional) (Unsigned Char) Controller ID
///     (Optional) (Unsigned Char) Zone ID
/// </summary>
public class PacketC2SMute : PacketC2S
{
    public const byte ID = 0x0D;
    
    public const byte MuteOff = 0x00;
    public const byte MuteOn = 0x01;
    public const byte MuteToggle = 0x02;

    public byte MuteState { get; private set; }
    public ushort FadeTime { get; private set; }
    public byte? ControllerID { get; private set; }
    public byte? ZoneID { get; private set; }

    public PacketC2SMute(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        MuteState = Reader.ReadByte();
        FadeTime = Reader.ReadUInt16();
        
        // Check if there's remaining data for controller and zone ID
        if (Reader.BaseStream.Length - Reader.BaseStream.Position > 1)
        {
            ControllerID = Reader.ReadByte();
            ZoneID = Reader.ReadByte();
        }
    }
}