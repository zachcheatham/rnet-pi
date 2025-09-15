namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x7D
/// Update
/// Requests a software update
/// </summary>
public class PacketC2SUpdate : PacketC2S
{
    public const byte ID = 0x7D;

    public PacketC2SUpdate(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        // No data to parse for update packet
    }
}