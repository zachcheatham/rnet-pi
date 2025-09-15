using RNetPi.Core.Packets;

namespace RNetPi.Core.Tests.Packets;

public class PacketC2SZoneVolumeTests
{
    [Fact]
    public void Constructor_ShouldParseDataCorrectly()
    {
        // Arrange
        byte[] data = [1, 2, 50]; // Controller ID: 1, Zone ID: 2, Volume: 50

        // Act
        var packet = new PacketC2SZoneVolume(data);

        // Assert
        Assert.Equal(PacketC2SZoneVolume.ID, packet.GetID());
        Assert.Equal(1, packet.ControllerID);
        Assert.Equal(2, packet.ZoneID);
        Assert.Equal(50, packet.Volume);
    }

    [Fact]
    public void GetID_ShouldReturn0x09()
    {
        // Arrange
        byte[] data = [0, 0, 0];
        var packet = new PacketC2SZoneVolume(data);

        // Act
        var id = packet.GetID();

        // Assert
        Assert.Equal(0x09, id);
    }
}