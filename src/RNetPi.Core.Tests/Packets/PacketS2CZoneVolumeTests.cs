using RNetPi.Core.Packets;

namespace RNetPi.Core.Tests.Packets;

public class PacketS2CZoneVolumeTests
{
    [Fact]
    public void Constructor_ShouldCreateValidPacket()
    {
        // Arrange
        byte controllerID = 1;
        byte zoneID = 2;
        byte volume = 75;

        // Act
        var packet = new PacketS2CZoneVolume(controllerID, zoneID, volume);
        var buffer = packet.GetBuffer();

        // Assert
        Assert.Equal(PacketS2CZoneVolume.ID, packet.GetID());
        Assert.Equal(0x09, packet.GetID());
        
        // Check the buffer contains the expected data
        Assert.Equal(0x09, buffer[0]); // Packet ID
        Assert.Equal(3, buffer[1]);    // Packet length (excluding ID and length)
        Assert.Equal(controllerID, buffer[2]);
        Assert.Equal(zoneID, buffer[3]);
        Assert.Equal(volume, buffer[4]);
    }

    [Fact]
    public void GetID_ShouldReturn0x09()
    {
        // Arrange
        var packet = new PacketS2CZoneVolume(0, 0, 0);

        // Act
        var id = packet.GetID();

        // Assert
        Assert.Equal(0x09, id);
    }
}