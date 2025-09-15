using RNetPi.Core.RNet;

namespace RNetPi.Core.Tests.RNet;

public class SetPowerPacketTests
{
    [Fact]
    public void Constructor_ShouldSetCorrectProperties_WhenPowerIsTrue()
    {
        // Arrange
        byte controllerID = 0x01;
        byte zoneID = 0x02;
        bool power = true;

        // Act
        var packet = new SetPowerPacket(controllerID, zoneID, power);

        // Assert
        Assert.Equal(0x05, packet.MessageType); // Event packet
        Assert.Equal(new byte[] { 0x02, 0x00 }, packet.TargetPath);
        Assert.Equal(controllerID, packet.TargetControllerID);
        Assert.Equal(0xDD, packet.EventID); // Power on
        Assert.Equal(0, packet.EventTimestamp);
        Assert.Equal(zoneID, packet.EventData);
        Assert.Equal(1, packet.EventPriority);
    }

    [Fact]
    public void Constructor_ShouldSetCorrectProperties_WhenPowerIsFalse()
    {
        // Arrange
        byte controllerID = 0x01;
        byte zoneID = 0x02;
        bool power = false;

        // Act
        var packet = new SetPowerPacket(controllerID, zoneID, power);

        // Assert
        Assert.Equal(0x05, packet.MessageType); // Event packet
        Assert.Equal(new byte[] { 0x02, 0x00 }, packet.TargetPath);
        Assert.Equal(controllerID, packet.TargetControllerID);
        Assert.Equal(0xDC, packet.EventID); // Power off
        Assert.Equal(0, packet.EventTimestamp);
        Assert.Equal(zoneID, packet.EventData);
        Assert.Equal(1, packet.EventPriority);
    }

    [Fact]
    public void GetControllerID_ShouldReturnTargetControllerID()
    {
        // Arrange
        var packet = new SetPowerPacket(0x05, 0x02, true);

        // Act
        var result = packet.GetControllerID();

        // Assert
        Assert.Equal(0x05, result);
    }

    [Fact]
    public void GetZoneID_ShouldReturnEventData()
    {
        // Arrange
        var packet = new SetPowerPacket(0x01, 0x07, true);

        // Act
        var result = packet.GetZoneID();

        // Assert
        Assert.Equal(0x07, result);
    }

    [Fact]
    public void GetPower_ShouldReturnTrue_WhenEventIDIs0xDD()
    {
        // Arrange
        var packet = new SetPowerPacket(0x01, 0x02, true);

        // Act
        var result = packet.GetPower();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetPower_ShouldReturnFalse_WhenEventIDIs0xDC()
    {
        // Arrange
        var packet = new SetPowerPacket(0x01, 0x02, false);

        // Act
        var result = packet.GetPower();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetBuffer_ShouldCreateValidPacket()
    {
        // Arrange
        var packet = new SetPowerPacket(0x01, 0x02, true);

        // Act
        var buffer = packet.GetBuffer();

        // Assert
        Assert.True(buffer.Length > 10); // Should have complete packet structure
        Assert.Equal(0xF0, buffer[0]); // Start byte
        Assert.Equal(0x01, buffer[1]); // Target Controller ID
        Assert.Equal(0xF7, buffer[^1]); // End byte
    }
}