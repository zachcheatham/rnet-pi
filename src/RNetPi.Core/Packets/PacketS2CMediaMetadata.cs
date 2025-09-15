namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x36
/// Media Metadata
/// Sends info about the current playing track to clients
/// Data:
///     (Unsigned Char) SourceID
///     (String) Title
///     (String) Artist
///     (String) Artwork URL
/// </summary>
public class PacketS2CMediaMetadata : PacketS2C
{
    public const byte ID = 0x36;

    public PacketS2CMediaMetadata(byte sourceID, string? title, string? artist, string? artworkURL)
    {
        Writer.Write(sourceID);
        WriteNullTerminatedString(title ?? string.Empty);
        WriteNullTerminatedString(artist ?? string.Empty);
        WriteNullTerminatedString(artworkURL ?? string.Empty);
    }

    public override byte GetID() => ID;
}