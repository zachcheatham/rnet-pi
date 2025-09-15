using RNetPi.Core.Constants;

namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x02
/// Property
/// </summary>
public class PacketC2SProperty : PacketC2S
{
    public const byte ID = 0x02;

    public byte Property { get; private set; }
    public object? Value { get; private set; }

    public PacketC2SProperty(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        Property = Reader.ReadByte();
        switch (Property)
        {
            case Properties.WebServerEnabled:
                // No additional data
                break;
            case Properties.Name:
                Value = ReadNullTerminatedString();
                break;
            default:
                // Handle unknown properties gracefully
                break;
        }
    }
}