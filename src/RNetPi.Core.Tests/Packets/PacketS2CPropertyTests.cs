using RNetPi.Core.Constants;
using RNetPi.Core.Packets;

namespace RNetPi.Core.Tests.Packets;

public class PacketS2CPropertyTests
{
    [Fact]
    public void Constructor_ShouldCreateValidPacket_ForBooleanProperty()
    {
        // Arrange
        byte propertyID = Properties.WebServerEnabled;
        bool value = true;

        // Act
        var packet = new PacketS2CProperty(propertyID, value);
        var buffer = packet.GetBuffer();

        // Assert
        Assert.Equal(PacketS2CProperty.ID, packet.GetID());
        Assert.Equal(0x02, packet.GetID());
        
        // Check the buffer contains the expected data
        Assert.Equal(0x02, buffer[0]); // Packet ID
        Assert.Equal(2, buffer[1]);    // Packet length
        Assert.Equal(propertyID, buffer[2]); // Property ID
        Assert.Equal(1, buffer[3]);    // Boolean value as byte
    }

    [Fact]
    public void Constructor_ShouldCreateValidPacket_ForStringProperty()
    {
        // Arrange
        byte propertyID = Properties.Name;
        string value = "Test Name";

        // Act
        var packet = new PacketS2CProperty(propertyID, value);
        var buffer = packet.GetBuffer();

        // Assert
        Assert.Equal(PacketS2CProperty.ID, packet.GetID());
        
        // Check the buffer contains the expected data
        Assert.Equal(0x02, buffer[0]); // Packet ID
        Assert.Equal(propertyID, buffer[2]); // Property ID
        
        // Check that the string is properly null-terminated
        var stringStart = 3;
        var stringBytes = buffer[stringStart..^1]; // Exclude the null terminator
        var decodedString = System.Text.Encoding.UTF8.GetString(stringBytes);
        Assert.Equal(value, decodedString);
        Assert.Equal(0, buffer[^1]); // Last byte should be null terminator
    }

    [Fact]
    public void Constructor_ShouldHandleFalseBooleanValue()
    {
        // Arrange
        byte propertyID = Properties.SerialConnected;
        bool value = false;

        // Act
        var packet = new PacketS2CProperty(propertyID, value);
        var buffer = packet.GetBuffer();

        // Assert
        Assert.Equal(propertyID, buffer[2]); // Property ID
        Assert.Equal(0, buffer[3]);    // Boolean false as byte
    }

    [Fact]
    public void GetID_ShouldReturn0x02()
    {
        // Arrange
        var packet = new PacketS2CProperty(Properties.Name, "test");

        // Act
        var id = packet.GetID();

        // Assert
        Assert.Equal(0x02, id);
    }
}