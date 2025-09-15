using RNetPi.Core.Packets;

namespace RNetPi.Core.Tests.Packets;

public class PacketC2SSourceInfoTests
{
    [Fact]
    public void Constructor_ShouldParseDataWithSourceType()
    {
        // Arrange
        var sourceID = (byte)5;
        var name = "Test Source";
        var sourceTypeID = (byte)3;
        var nameBytes = System.Text.Encoding.UTF8.GetBytes(name);
        
        var data = new List<byte>();
        data.Add(sourceID);
        data.AddRange(nameBytes);
        data.Add(0); // null terminator
        data.Add(sourceTypeID);

        // Act
        var packet = new PacketC2SSourceInfo(data.ToArray());

        // Assert
        Assert.Equal(PacketC2SSourceInfo.ID, packet.GetID());
        Assert.Equal(sourceID, packet.SourceID);
        Assert.Equal(name, packet.Name);
        Assert.Equal(sourceTypeID, packet.SourceTypeID);
    }

    [Fact]
    public void Constructor_ShouldParseDataWithoutSourceType()
    {
        // Arrange
        var sourceID = (byte)5;
        var name = "Test Source";
        var nameBytes = System.Text.Encoding.UTF8.GetBytes(name);
        
        var data = new List<byte>();
        data.Add(sourceID);
        data.AddRange(nameBytes);
        data.Add(0); // null terminator
        // No source type ID

        // Act
        var packet = new PacketC2SSourceInfo(data.ToArray());

        // Assert
        Assert.Equal(PacketC2SSourceInfo.ID, packet.GetID());
        Assert.Equal(sourceID, packet.SourceID);
        Assert.Equal(name, packet.Name);
        Assert.Equal(0, packet.SourceTypeID); // Default value
    }

    [Fact]
    public void GetID_ShouldReturn0x06()
    {
        // Arrange
        var data = new byte[] { 1, 0 }; // Minimal data
        var packet = new PacketC2SSourceInfo(data);

        // Act
        var id = packet.GetID();

        // Assert
        Assert.Equal(0x06, id);
    }
}