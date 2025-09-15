using RNetPi.Core.RNet;

namespace RNetPi.Core.Tests.RNet;

public class DataPacketTests
{
    [Fact]
    public void Constructor_ShouldSetMessageType()
    {
        // Act
        var packet = new DataPacket();

        // Assert
        Assert.Equal(0x00, packet.MessageType);
    }

    [Fact]
    public void GetMessageBody_ShouldCreateValidDataStructure()
    {
        // Arrange
        var packet = new DataPacket
        {
            TargetPath = new byte[] { 0x02, 0x00 },
            SourcePath = new byte[] { 0x04, 0x03 },
            PacketNumber = 1,
            PacketCount = 2,
            Data = new byte[] { 0x10, 0x20, 0x30 }
        };

        // Act
        var body = packet.GetBuffer();

        // Assert
        // Should contain the structured data format
        Assert.True(body.Length > 10); // Should have header + data
    }

    [Fact]
    public void FromPacket_ShouldParseValidDataPacket()
    {
        // Arrange
        var originalPacket = new RNetPacket
        {
            MessageType = 0x00,
            MessageBody = CreateTestDataPacketBody()
        };

        // Act
        var dataPacket = DataPacket.FromPacket(originalPacket);

        // Assert
        Assert.Equal(0x00, dataPacket.MessageType);
        Assert.Equal(new byte[] { 0x02, 0x00 }, dataPacket.TargetPath);
        Assert.Equal(new byte[] { 0x04, 0x03 }, dataPacket.SourcePath);
        Assert.Equal(1, dataPacket.PacketNumber);
        Assert.Equal(2, dataPacket.PacketCount);
        Assert.Equal(new byte[] { 0x10, 0x20 }, dataPacket.Data);
    }

    [Fact]
    public void FromPacket_ShouldThrowException_WhenWrongMessageType()
    {
        // Arrange
        var packet = new RNetPacket { MessageType = 0x05 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => DataPacket.FromPacket(packet));
    }

    [Fact]
    public void FromPacket_ShouldHandleEmptyMessageBody()
    {
        // Arrange
        var packet = new RNetPacket
        {
            MessageType = 0x00,
            MessageBody = Array.Empty<byte>()
        };

        // Act
        var dataPacket = DataPacket.FromPacket(packet);

        // Assert
        Assert.Equal(0x00, dataPacket.MessageType);
        Assert.Empty(dataPacket.TargetPath);
        Assert.Empty(dataPacket.SourcePath);
        Assert.Equal(0, dataPacket.PacketNumber);
        Assert.Equal(1, dataPacket.PacketCount);
        Assert.Empty(dataPacket.Data);
    }

    [Fact]
    public void CopyToPacket_ShouldCopyAllDataPacketProperties()
    {
        // Arrange
        var source = new DataPacket
        {
            MessageType = 0x00,
            TargetPath = new byte[] { 0x01, 0x02 },
            SourcePath = new byte[] { 0x03, 0x04 },
            PacketNumber = 5,
            PacketCount = 10,
            Data = new byte[] { 0x11, 0x22 }
        };
        
        var target = new DataPacket();

        // Act
        source.CopyToPacket(target);

        // Assert
        Assert.Equal(source.TargetPath, target.TargetPath);
        Assert.Equal(source.SourcePath, target.SourcePath);
        Assert.Equal(source.PacketNumber, target.PacketNumber);
        Assert.Equal(source.PacketCount, target.PacketCount);
        Assert.Equal(source.Data, target.Data);
    }

    [Fact]
    public void CopyToPacket_ShouldThrowException_WhenTargetNotDataPacket()
    {
        // Arrange
        var source = new DataPacket { MessageType = 0x00 };
        var target = new RNetPacket { MessageType = 0x00 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => source.CopyToPacket(target));
    }

    private static byte[] CreateTestDataPacketBody()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Target path
        writer.Write((byte)2); // Length
        writer.Write((byte)0x02);
        writer.Write((byte)0x00);

        // Source path
        writer.Write((byte)2); // Length
        writer.Write((byte)0x04);
        writer.Write((byte)0x03);

        // Packet number and count
        writer.Write((ushort)1);
        writer.Write((ushort)2);

        // Data
        writer.Write((ushort)2); // Data length
        writer.Write((byte)0x10);
        writer.Write((byte)0x20);

        return stream.ToArray();
    }
}