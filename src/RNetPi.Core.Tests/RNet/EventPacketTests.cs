using RNetPi.Core.RNet;

namespace RNetPi.Core.Tests.RNet;

public class EventPacketTests
{
    [Fact]
    public void Constructor_ShouldSetMessageType()
    {
        // Act
        var packet = new EventPacket();

        // Assert
        Assert.Equal(0x05, packet.MessageType);
    }

    [Fact]
    public void GetMessageBody_ShouldCreateValidEventStructure()
    {
        // Arrange
        var packet = new EventPacket
        {
            TargetPath = new byte[] { 0x02, 0x00 },
            SourcePath = new byte[] { 0x04, 0x03 },
            EventID = 0x1234,
            EventTimestamp = 0x5678,
            EventData = 0x9ABC,
            EventPriority = 0xDE
        };

        // Act
        var body = packet.GetBuffer();

        // Assert
        Assert.True(body.Length > 10); // Should have header + event data
    }

    [Fact]
    public void FromPacket_ShouldParseValidEventPacket()
    {
        // Arrange
        var originalPacket = new RNetPacket
        {
            MessageType = 0x05,
            MessageBody = CreateTestEventPacketBody()
        };

        // Act
        var eventPacket = EventPacket.FromPacket(originalPacket);

        // Assert
        Assert.Equal(0x05, eventPacket.MessageType);
        Assert.Equal(new byte[] { 0x02, 0x00 }, eventPacket.TargetPath);
        Assert.Equal(new byte[] { 0x04, 0x03 }, eventPacket.SourcePath);
        Assert.Equal(0x1234, eventPacket.EventID);
        Assert.Equal(0x5678, eventPacket.EventTimestamp);
        Assert.Equal(0x9ABC, eventPacket.EventData);
        Assert.Equal(0xDE, eventPacket.EventPriority);
    }

    [Fact]
    public void FromPacket_ShouldThrowException_WhenWrongMessageType()
    {
        // Arrange
        var packet = new RNetPacket { MessageType = 0x00 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => EventPacket.FromPacket(packet));
    }

    [Fact]
    public void FromPacket_ShouldHandleEmptyMessageBody()
    {
        // Arrange
        var packet = new RNetPacket
        {
            MessageType = 0x05,
            MessageBody = Array.Empty<byte>()
        };

        // Act
        var eventPacket = EventPacket.FromPacket(packet);

        // Assert
        Assert.Equal(0x05, eventPacket.MessageType);
        Assert.Empty(eventPacket.TargetPath);
        Assert.Empty(eventPacket.SourcePath);
        Assert.Equal(0, eventPacket.EventID);
        Assert.Equal(0, eventPacket.EventTimestamp);
        Assert.Equal(0, eventPacket.EventData);
        Assert.Equal(0, eventPacket.EventPriority);
    }

    [Fact]
    public void CopyToPacket_ShouldCopyAllEventPacketProperties()
    {
        // Arrange
        var source = new EventPacket
        {
            MessageType = 0x05,
            TargetPath = new byte[] { 0x01, 0x02 },
            SourcePath = new byte[] { 0x03, 0x04 },
            EventID = 0x1111,
            EventTimestamp = 0x2222,
            EventData = 0x3333,
            EventPriority = 0x44
        };
        
        var target = new EventPacket();

        // Act
        source.CopyToPacket(target);

        // Assert
        Assert.Equal(source.TargetPath, target.TargetPath);
        Assert.Equal(source.SourcePath, target.SourcePath);
        Assert.Equal(source.EventID, target.EventID);
        Assert.Equal(source.EventTimestamp, target.EventTimestamp);
        Assert.Equal(source.EventData, target.EventData);
        Assert.Equal(source.EventPriority, target.EventPriority);
    }

    [Fact]
    public void CopyToPacket_ShouldThrowException_WhenTargetNotEventPacket()
    {
        // Arrange
        var source = new EventPacket { MessageType = 0x05 };
        var target = new RNetPacket { MessageType = 0x05 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => source.CopyToPacket(target));
    }

    private static byte[] CreateTestEventPacketBody()
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

        // Event fields (little endian)
        writer.Write((ushort)0x1234); // EventID
        writer.Write((ushort)0x5678); // EventTimestamp
        writer.Write((ushort)0x9ABC); // EventData
        writer.Write((byte)0xDE);     // EventPriority

        return stream.ToArray();
    }
}