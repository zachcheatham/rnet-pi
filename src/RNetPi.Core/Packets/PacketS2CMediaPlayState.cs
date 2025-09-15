namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x37
/// Media Play State
/// Sends media play state of a source
/// Data:
///     (Unsigned Char) SourceID
///     (Unsigned Char) Playing / Paused
/// </summary>
public class PacketS2CMediaPlayState : PacketS2C
{
    public const byte ID = 0x37;

    public PacketS2CMediaPlayState(byte sourceID, bool isPlaying)
    {
        Writer.Write(sourceID);
        Writer.Write((byte)(isPlaying ? 0x01 : 0x00));
    }

    public override byte GetID() => ID;
}