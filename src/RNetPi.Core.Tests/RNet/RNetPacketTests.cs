using RNetPi.Core.RNet;

namespace RNetPi.Core.Tests.RNet;

public class RNetPacketTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var packet = new RNetPacket();

        // Assert
        Assert.Equal(0x00, packet.TargetControllerID);
        Assert.Equal(0x00, packet.TargetZoneID);
        Assert.Equal(0x7F, packet.TargetKeypadID);
        Assert.Equal(0x00, packet.SourceControllerID);
        Assert.Equal(0x00, packet.SourceZoneID);
        Assert.Equal(0x70, packet.SourceKeypadID);
        Assert.Equal(0x00, packet.MessageType);
    }

    [Fact]
    public void GetBuffer_ShouldCreateValidPacket()
    {
        // Arrange
        var packet = new RNetPacket
        {
            TargetControllerID = 0x01,
            TargetZoneID = 0x02,
            MessageType = 0x05,
            MessageBody = new byte[] { 0x10, 0x20 }
        };

        // Act
        var buffer = packet.GetBuffer();

        // Assert
        Assert.Equal(0xF0, buffer[0]); // Start byte
        Assert.Equal(0x01, buffer[1]); // Target Controller ID
        Assert.Equal(0x02, buffer[2]); // Target Zone ID
        Assert.Equal(0x7F, buffer[3]); // Target Keypad ID (default)
        Assert.Equal(0x00, buffer[4]); // Source Controller ID (default)
        Assert.Equal(0x00, buffer[5]); // Source Zone ID (default)
        Assert.Equal(0x70, buffer[6]); // Source Keypad ID (default)
        Assert.Equal(0x05, buffer[7]); // Message Type
        Assert.Equal(0x10, buffer[8]); // Message body byte 1
        Assert.Equal(0x20, buffer[9]); // Message body byte 2
        Assert.Equal(0xF7, buffer[^1]); // End byte
    }

    [Fact]
    public void FromData_ShouldParseValidPacket()
    {
        // Arrange
        var data = new byte[] 
        { 
            0xF0, // Start
            0x01, 0x02, 0x7F, 0x00, 0x00, 0x70, // Header
            0x05, // Message type
            0x10, 0x20, // Message body
            0x23, // Checksum (calculated)
            0xF7  // End
        };

        // Act
        var packet = RNetPacket.FromData(data);

        // Assert
        Assert.Equal(0x01, packet.TargetControllerID);
        Assert.Equal(0x02, packet.TargetZoneID);
        Assert.Equal(0x7F, packet.TargetKeypadID);
        Assert.Equal(0x00, packet.SourceControllerID);
        Assert.Equal(0x00, packet.SourceZoneID);
        Assert.Equal(0x70, packet.SourceKeypadID);
        Assert.Equal(0x05, packet.MessageType);
        Assert.Equal(new byte[] { 0x10, 0x20 }, packet.MessageBody);
    }

    [Fact]
    public void FromData_ShouldThrowException_WhenDataTooShort()
    {
        // Arrange
        var data = new byte[] { 0xF0, 0x01, 0x02 }; // Too short

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RNetPacket.FromData(data));
    }

    [Fact]
    public void FromData_ShouldThrowException_WhenInvalidStartByte()
    {
        // Arrange
        var data = new byte[] 
        { 
            0xFF, // Invalid start byte
            0x01, 0x02, 0x7F, 0x00, 0x00, 0x70,
            0x05,
            0x10, 0x20,
            0x23,
            0xF7
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RNetPacket.FromData(data));
    }

    [Fact]
    public void FromData_ShouldThrowException_WhenInvalidEndByte()
    {
        // Arrange
        var data = new byte[] 
        { 
            0xF0,
            0x01, 0x02, 0x7F, 0x00, 0x00, 0x70,
            0x05,
            0x10, 0x20,
            0x23,
            0xFF // Invalid end byte
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RNetPacket.FromData(data));
    }

    [Fact]
    public void CopyToPacket_ShouldCopyHeaderFields()
    {
        // Arrange
        var source = new RNetPacket
        {
            TargetControllerID = 0x01,
            TargetZoneID = 0x02,
            TargetKeypadID = 0x03,
            SourceControllerID = 0x04,
            SourceZoneID = 0x05,
            SourceKeypadID = 0x06,
            MessageType = 0x07
        };
        
        var target = new RNetPacket
        {
            MessageType = 0x07 // Same message type required
        };

        // Act
        source.CopyToPacket(target);

        // Assert
        Assert.Equal(source.TargetControllerID, target.TargetControllerID);
        Assert.Equal(source.TargetZoneID, target.TargetZoneID);
        Assert.Equal(source.TargetKeypadID, target.TargetKeypadID);
        Assert.Equal(source.SourceControllerID, target.SourceControllerID);
        Assert.Equal(source.SourceZoneID, target.SourceZoneID);
        Assert.Equal(source.SourceKeypadID, target.SourceKeypadID);
    }

    [Fact]
    public void CopyToPacket_ShouldThrowException_WhenDifferentMessageType()
    {
        // Arrange
        var source = new RNetPacket { MessageType = 0x05 };
        var target = new RNetPacket { MessageType = 0x06 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => source.CopyToPacket(target));
    }

    [Fact]
    public void RequiresHandshake_ShouldReturnFalseByDefault()
    {
        // Arrange
        var packet = new RNetPacket();

        // Act
        var result = packet.RequiresHandshake();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CausesResponseWithHandshake_ShouldReturnFalseByDefault()
    {
        // Arrange
        var packet = new RNetPacket();

        // Act
        var result = packet.CausesResponseWithHandshake();

        // Assert
        Assert.False(result);
    }
}