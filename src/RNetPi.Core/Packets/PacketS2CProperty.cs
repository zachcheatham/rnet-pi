using RNetPi.Core.Constants;

namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x02
/// Property
/// Sends the client a controller property value
/// Data:
///     (Unsigned Char) Property ID
///     (Variable) Property Value
/// </summary>
public class PacketS2CProperty : PacketS2C
{
    public const byte ID = 0x02;

    public PacketS2CProperty(byte property, object? value)
    {
        Writer.Write(property);
        switch (property)
        {
            case Properties.SerialConnected:
            case Properties.WebServerEnabled:
                Writer.Write((byte)((value is bool boolValue && boolValue) ? 1 : 0));
                break;
            case Properties.Name:
            case Properties.Version:
                WriteNullTerminatedString(value?.ToString() ?? string.Empty);
                break;
        }
    }

    public override byte GetID() => ID;
}