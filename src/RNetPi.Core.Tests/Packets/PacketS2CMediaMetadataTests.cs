using RNetPi.Core.Packets;

namespace RNetPi.Core.Tests.Packets;

public class PacketS2CMediaMetadataTests
{
    [Fact]
    public void Constructor_ShouldCreateValidPacket_WithAllFields()
    {
        // Arrange
        byte sourceID = 5;
        string title = "Test Song";
        string artist = "Test Artist";
        string artworkURL = "http://example.com/artwork.jpg";

        // Act
        var packet = new PacketS2CMediaMetadata(sourceID, title, artist, artworkURL);
        var buffer = packet.GetBuffer();

        // Assert
        Assert.Equal(PacketS2CMediaMetadata.ID, packet.GetID());
        Assert.Equal(0x36, packet.GetID());
        
        // Verify packet structure
        Assert.Equal(0x36, buffer[0]); // Packet ID
        Assert.Equal(sourceID, buffer[2]); // Source ID (after length byte)
        
        // Check that the buffer contains the null-terminated strings
        var bufferString = System.Text.Encoding.UTF8.GetString(buffer[3..]);
        Assert.Contains(title, bufferString);
        Assert.Contains(artist, bufferString);
        Assert.Contains(artworkURL, bufferString);
    }

    [Fact]
    public void Constructor_ShouldHandleNullValues()
    {
        // Arrange
        byte sourceID = 5;

        // Act
        var packet = new PacketS2CMediaMetadata(sourceID, null, null, null);
        var buffer = packet.GetBuffer();

        // Assert
        Assert.Equal(PacketS2CMediaMetadata.ID, packet.GetID());
        Assert.Equal(sourceID, buffer[2]); // Source ID
        
        // Should contain empty strings (null terminators only)
        // Buffer[3] to buffer[5] should be null terminators for the three empty strings
        Assert.Equal(0, buffer[3]); // First null terminator
        Assert.Equal(0, buffer[4]); // Second null terminator
        Assert.Equal(0, buffer[5]); // Third null terminator
    }

    [Fact]
    public void GetID_ShouldReturn0x36()
    {
        // Arrange
        var packet = new PacketS2CMediaMetadata(0, "", "", "");

        // Act
        var id = packet.GetID();

        // Assert
        Assert.Equal(0x36, id);
    }
}