namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x05
/// Delete Zone
/// Deletes a Zone
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
/// </summary>
public class PacketC2SDeleteZone : PacketC2S
{
    public const byte ID = 0x05;

    public byte ControllerID { get; private set; }
    public byte ZoneID { get; private set; }

    public PacketC2SDeleteZone(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        ControllerID = Reader.ReadByte();
        ZoneID = Reader.ReadByte();
    }
}