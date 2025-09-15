namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x01
/// Intent
/// Announces to the server the client's intent to connect
/// </summary>
public class PacketC2SIntent : PacketC2S
{
    public const byte ID = 0x01;

    public PacketC2SIntent(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        // No data to parse for intent packet
    }
}