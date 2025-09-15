namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x35
/// Source Descriptive Text
/// Data:
///     (Unsigned Char) SourceID
///     (Short) Time
///     (String) Text
/// </summary>
public class PacketS2CSourceDescriptiveText : PacketS2C
{
    public const byte ID = 0x35;

    public PacketS2CSourceDescriptiveText(byte sourceID, ushort time, string text)
    {
        Writer.Write(sourceID);
        Writer.Write(time);
        WriteNullTerminatedString(text);
    }

    public override byte GetID() => ID;
}