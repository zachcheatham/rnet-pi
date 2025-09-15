namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x03
/// Disconnect
/// Disconnects from the server
/// </summary>
public class PacketC2SDisconnect : PacketC2S
{
    public const byte ID = 0x03;

    public PacketC2SDisconnect(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        // No data to parse for disconnect packet
    }
}