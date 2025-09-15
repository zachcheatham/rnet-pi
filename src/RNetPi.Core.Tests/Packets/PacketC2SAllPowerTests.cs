using RNetPi.Core.Packets;

namespace RNetPi.Core.Tests.Packets;

public class PacketC2SAllPowerTests
{
    [Fact]
    public void Constructor_ShouldParseData_WhenPowerIsOn()
    {
        // Arrange
        byte[] data = [1]; // Power on

        // Act
        var packet = new PacketC2SAllPower(data);

        // Assert
        Assert.Equal(PacketC2SAllPower.ID, packet.GetID());
        Assert.True(packet.Powered);
    }

    [Fact]
    public void Constructor_ShouldParseData_WhenPowerIsOff()
    {
        // Arrange
        byte[] data = [0]; // Power off

        // Act
        var packet = new PacketC2SAllPower(data);

        // Assert
        Assert.Equal(PacketC2SAllPower.ID, packet.GetID());
        Assert.False(packet.Powered);
    }

    [Fact]
    public void GetID_ShouldReturn0x0C()
    {
        // Arrange
        byte[] data = [0];
        var packet = new PacketC2SAllPower(data);

        // Act
        var id = packet.GetID();

        // Assert
        Assert.Equal(0x0C, id);
    }
}