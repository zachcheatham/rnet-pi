using RNetPi.Core.RNet;

namespace RNetPi.Core.Tests.RNet;

public class SetVolumePacketTests
{
    [Fact]
    public void Constructor_ShouldSetCorrectProperties()
    {
        // Arrange
        byte controllerID = 0x01;
        byte zoneID = 0x02;
        int volume = 50;

        // Act
        var packet = new SetVolumePacket(controllerID, zoneID, volume);

        // Assert
        Assert.Equal(0x05, packet.MessageType); // Event packet
        Assert.Equal(new byte[] { 0x02, 0x00 }, packet.TargetPath);
        Assert.Equal(controllerID, packet.TargetControllerID);
        Assert.Equal(0xDE, packet.EventID);
        Assert.Equal(25, packet.EventTimestamp); // 50 / 2
        Assert.Equal(zoneID, packet.EventData);
        Assert.Equal(1, packet.EventPriority);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenVolumeIsNegative()
    {
        // Arrange
        byte controllerID = 0x01;
        byte zoneID = 0x02;
        int volume = -1;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new SetVolumePacket(controllerID, zoneID, volume));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenVolumeIsGreaterThan100()
    {
        // Arrange
        byte controllerID = 0x01;
        byte zoneID = 0x02;
        int volume = 101;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new SetVolumePacket(controllerID, zoneID, volume));
    }

    [Fact]
    public void GetControllerID_ShouldReturnTargetControllerID()
    {
        // Arrange
        var packet = new SetVolumePacket(0x05, 0x02, 50);

        // Act
        var result = packet.GetControllerID();

        // Assert
        Assert.Equal(0x05, result);
    }

    [Fact]
    public void GetZoneID_ShouldReturnEventData()
    {
        // Arrange
        var packet = new SetVolumePacket(0x01, 0x07, 50);

        // Act
        var result = packet.GetZoneID();

        // Assert
        Assert.Equal(0x07, result);
    }

    [Fact]
    public void GetVolume_ShouldReturnEventTimestampTimesTwo()
    {
        // Arrange
        var packet = new SetVolumePacket(0x01, 0x02, 50);

        // Act
        var result = packet.GetVolume();

        // Assert
        Assert.Equal(50, result); // 25 * 2
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 2)]
    [InlineData(50, 50)]
    [InlineData(99, 98)]
    [InlineData(100, 100)]
    public void VolumeConversion_ShouldWorkCorrectly(int inputVolume, int expectedVolume)
    {
        // Arrange
        var packet = new SetVolumePacket(0x01, 0x02, inputVolume);

        // Act
        var result = packet.GetVolume();

        // Assert
        Assert.Equal(expectedVolume, result);
    }

    [Fact]
    public void GetBuffer_ShouldCreateValidPacket()
    {
        // Arrange
        var packet = new SetVolumePacket(0x01, 0x02, 50);

        // Act
        var buffer = packet.GetBuffer();

        // Assert
        Assert.True(buffer.Length > 10); // Should have complete packet structure
        Assert.Equal(0xF0, buffer[0]); // Start byte
        Assert.Equal(0x01, buffer[1]); // Target Controller ID
        Assert.Equal(0xF7, buffer[^1]); // End byte
    }
}