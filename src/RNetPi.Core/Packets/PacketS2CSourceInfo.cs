namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x06
/// Source Info
/// Sends source information
/// Data:
///     (Unsigned Char) SourceID
///     (String) Name
///     (Unsigned Char) Source Type ID
/// </summary>
public class PacketS2CSourceInfo : PacketS2C
{
    public const byte ID = 0x06;

    public PacketS2CSourceInfo(byte sourceID, string name, byte sourceTypeID)
    {
        Writer.Write(sourceID);
        WriteNullTerminatedString(name);
        Writer.Write(sourceTypeID);
    }

    public override byte GetID() => ID;
}