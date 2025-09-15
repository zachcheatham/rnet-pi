using RNetPi.Core.RNet;

namespace RNetPi.Core.Tests.RNet;

public class ZoneInfoPacketTests
{
    [Fact]
    public void Constructor_ShouldSetMessageType()
    {
        // Act
        var packet = new ZoneInfoPacket();

        // Assert
        Assert.Equal(0x00, packet.MessageType);
    }

    [Fact]
    public void RequiresHandshake_ShouldReturnTrue()
    {
        // Arrange
        var packet = new ZoneInfoPacket();

        // Act
        var result = packet.RequiresHandshake();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetControllerID_ShouldReturnSourceControllerID()
    {
        // Arrange
        var packet = new ZoneInfoPacket
        {
            SourceControllerID = 0x05
        };

        // Act
        var result = packet.GetControllerID();

        // Assert
        Assert.Equal(0x05, result);
    }

    [Fact]
    public void GetZoneID_ShouldReturnSourcePathIndex2()
    {
        // Arrange
        var packet = new ZoneInfoPacket
        {
            SourcePath = new byte[] { 0x02, 0x00, 0x03, 0x07 }
        };

        // Act
        var result = packet.GetZoneID();

        // Assert
        Assert.Equal(0x03, result);
    }

    [Fact]
    public void GetZoneID_ShouldReturn0_WhenSourcePathTooShort()
    {
        // Arrange
        var packet = new ZoneInfoPacket
        {
            SourcePath = new byte[] { 0x02 }
        };

        // Act
        var result = packet.GetZoneID();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetPower_ShouldReturnTrue_WhenDataFirstByteIs1()
    {
        // Arrange
        var packet = new ZoneInfoPacket
        {
            Data = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        // Act
        var result = packet.GetPower();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetPower_ShouldReturnFalse_WhenDataFirstByteIs0()
    {
        // Arrange
        var packet = new ZoneInfoPacket
        {
            Data = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        // Act
        var result = packet.GetPower();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetSourceID_ShouldReturnSecondDataByte()
    {
        // Arrange
        var packet = new ZoneInfoPacket
        {
            Data = new byte[] { 1, 5, 25, 15, 15, 1, 10, 0, 0, 0 }
        };

        // Act
        var result = packet.GetSourceID();

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void GetVolume_ShouldReturnThirdDataByteTimesTwo()
    {
        // Arrange
        var packet = new ZoneInfoPacket
        {
            Data = new byte[] { 1, 5, 25, 15, 15, 1, 10, 0, 0, 0 }
        };

        // Act
        var result = packet.GetVolume();

        // Assert
        Assert.Equal(50, result); // 25 * 2
    }

    [Fact]
    public void GetBassLevel_ShouldReturnFourthDataByteMinus10()
    {
        // Arrange
        var packet = new ZoneInfoPacket
        {
            Data = new byte[] { 1, 5, 25, 15, 15, 1, 10, 0, 0, 0 }
        };

        // Act
        var result = packet.GetBassLevel();

        // Assert
        Assert.Equal(5, result); // 15 - 10
    }

    [Fact]
    public void GetTrebleLevel_ShouldReturnFifthDataByteMinus10()
    {
        // Arrange
        var packet = new ZoneInfoPacket
        {
            Data = new byte[] { 1, 5, 25, 15, 15, 1, 10, 0, 0, 0 }
        };

        // Act
        var result = packet.GetTrebleLevel();

        // Assert
        Assert.Equal(5, result); // 15 - 10
    }

    [Fact]
    public void GetLoudness_ShouldReturnTrue_WhenSixthDataByteIs1()
    {
        // Arrange
        var packet = new ZoneInfoPacket
        {
            Data = new byte[] { 1, 5, 25, 15, 15, 1, 10, 0, 0, 0 }
        };

        // Act
        var result = packet.GetLoudness();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetBalance_ShouldReturnSeventhDataByteMinus10()
    {
        // Arrange
        var packet = new ZoneInfoPacket
        {
            Data = new byte[] { 1, 5, 25, 15, 15, 1, 10, 0, 0, 0 }
        };

        // Act
        var result = packet.GetBalance();

        // Assert
        Assert.Equal(0, result); // 10 - 10
    }

    [Fact]
    public void GetPartyMode_ShouldReturnNinthDataByte()
    {
        // Arrange
        var packet = new ZoneInfoPacket
        {
            Data = new byte[] { 1, 5, 25, 15, 15, 1, 10, 0, 2, 0 }
        };

        // Act
        var result = packet.GetPartyMode();

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void GetDoNotDisturbMode_ShouldReturnTenthDataByte()
    {
        // Arrange
        var packet = new ZoneInfoPacket
        {
            Data = new byte[] { 1, 5, 25, 15, 15, 1, 10, 0, 2, 1 }
        };

        // Act
        var result = packet.GetDoNotDisturbMode();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetMethods_ShouldReturnDefaults_WhenDataArrayTooShort()
    {
        // Arrange
        var packet = new ZoneInfoPacket
        {
            Data = new byte[] { 1 } // Only one byte
        };

        // Act & Assert
        Assert.True(packet.GetPower());
        Assert.Equal(0, packet.GetSourceID());
        Assert.Equal(0, packet.GetVolume());
        Assert.Equal(0, packet.GetBassLevel()); // Default when no data
        Assert.Equal(0, packet.GetTrebleLevel()); // Default when no data
        Assert.False(packet.GetLoudness());
        Assert.Equal(0, packet.GetBalance()); // Default when no data
        Assert.Equal(0, packet.GetPartyMode());
        Assert.Equal(0, packet.GetDoNotDisturbMode());
    }

    [Fact]
    public void FromPacket_ShouldCreateZoneInfoPacket()
    {
        // Arrange
        var dataPacket = new DataPacket
        {
            MessageType = 0x00,
            SourceControllerID = 0x01,
            SourcePath = new byte[] { 0x02, 0x00, 0x03, 0x07 },
            Data = new byte[] { 1, 5, 25, 15, 15, 1, 10, 0, 2, 1 }
        };

        // Act
        var zoneInfoPacket = ZoneInfoPacket.FromPacket(dataPacket);

        // Assert
        Assert.Equal(0x01, zoneInfoPacket.GetControllerID());
        Assert.Equal(0x03, zoneInfoPacket.GetZoneID());
        Assert.True(zoneInfoPacket.GetPower());
        Assert.Equal(5, zoneInfoPacket.GetSourceID());
        Assert.Equal(50, zoneInfoPacket.GetVolume());
        Assert.Equal(5, zoneInfoPacket.GetBassLevel());
        Assert.Equal(5, zoneInfoPacket.GetTrebleLevel());
        Assert.True(zoneInfoPacket.GetLoudness());
        Assert.Equal(0, zoneInfoPacket.GetBalance());
        Assert.Equal(2, zoneInfoPacket.GetPartyMode());
        Assert.Equal(1, zoneInfoPacket.GetDoNotDisturbMode());
    }

    [Fact]
    public void FromPacket_ShouldThrowException_WhenWrongMessageType()
    {
        // Arrange
        var dataPacket = new DataPacket { MessageType = 0x05 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ZoneInfoPacket.FromPacket(dataPacket));
    }
}