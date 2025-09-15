using RNetPi.Core.RNet;

namespace RNetPi.Core.Tests.RNet;

public class PacketBuilderTests
{
    [Fact]
    public void Build_ShouldReturnNull_WhenInvalidBuffer()
    {
        // Arrange
        var invalidBuffer = new byte[] { 0x00, 0x01 }; // Too short

        // Act
        var result = PacketBuilder.Build(invalidBuffer);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Build_ShouldReturnDataPacket_WhenMessageTypeIs0x00()
    {
        // Arrange
        var buffer = CreateValidRNetPacketBuffer(0x00);

        // Act
        var result = PacketBuilder.Build(buffer);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<DataPacket>(result);
    }

    [Fact]
    public void Build_ShouldReturnEventPacket_WhenMessageTypeIs0x05()
    {
        // Arrange
        var buffer = CreateValidRNetPacketBuffer(0x05);

        // Act
        var result = PacketBuilder.Build(buffer);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<EventPacket>(result);
    }

    [Fact]
    public void Build_ShouldReturnRenderedDisplayMessagePacket_WhenMessageTypeIs0x06()
    {
        // Arrange
        var buffer = CreateValidRNetPacketBuffer(0x06);

        // Act
        var result = PacketBuilder.Build(buffer);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<RenderedDisplayMessagePacket>(result);
    }

    [Fact]
    public void Build_ShouldReturnZoneInfoPacket_WhenDataPacketMatchesZoneInfoPattern()
    {
        // Arrange
        var buffer = CreateZoneInfoPacketBuffer();

        // Act
        var result = PacketBuilder.Build(buffer);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ZoneInfoPacket>(result);
    }

    [Fact]
    public void Build_ShouldReturnZonePowerPacket_WhenDataPacketMatchesZonePowerPattern()
    {
        // Arrange
        var buffer = CreateZonePowerPacketBuffer();

        // Act
        var result = PacketBuilder.Build(buffer);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ZonePowerPacket>(result);
    }

    [Fact]
    public void Build_ShouldReturnZoneSourcePacket_WhenDataPacketMatchesZoneSourcePattern()
    {
        // Arrange
        var buffer = CreateZoneSourcePacketBuffer();

        // Act
        var result = PacketBuilder.Build(buffer);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ZoneSourcePacket>(result);
    }

    [Fact]
    public void Build_ShouldReturnZoneVolumePacket_WhenDataPacketMatchesZoneVolumePattern()
    {
        // Arrange
        var buffer = CreateZoneVolumePacketBuffer();

        // Act
        var result = PacketBuilder.Build(buffer);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ZoneVolumePacket>(result);
    }

    [Fact]
    public void Build_ShouldReturnKeypadEventPacket_WhenEventPacketMatchesKeypadPattern()
    {
        // Arrange
        var buffer = CreateKeypadEventPacketBuffer();

        // Act
        var result = PacketBuilder.Build(buffer);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<KeypadEventPacket>(result);
    }

    [Fact]
    public void Build_ShouldReturnNull_WhenUnknownMessageType()
    {
        // Arrange
        var buffer = CreateValidRNetPacketBuffer(0xFF); // Unknown message type

        // Act
        var result = PacketBuilder.Build(buffer);

        // Assert
        Assert.Null(result);
    }

    private static byte[] CreateValidRNetPacketBuffer(byte messageType)
    {
        return new byte[]
        {
            0xF0, // Start
            0x00, 0x00, 0x7F, 0x00, 0x00, 0x70, // Header
            messageType, // Message type
            0x00, 0x00, // Minimal body
            0x7C, // Checksum (approximate)
            0xF7  // End
        };
    }

    private static byte[] CreateZoneInfoPacketBuffer()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // RNet packet header
        writer.Write((byte)0xF0); // Start
        writer.Write((byte)0x00); // Target Controller ID
        writer.Write((byte)0x00); // Target Zone ID
        writer.Write((byte)0x7F); // Target Keypad ID
        writer.Write((byte)0x01); // Source Controller ID
        writer.Write((byte)0x02); // Source Zone ID
        writer.Write((byte)0x70); // Source Keypad ID
        writer.Write((byte)0x00); // Message type (DataPacket)

        // DataPacket body with zone info pattern
        writer.Write((byte)0); // Target path length
        writer.Write((byte)4); // Source path length
        writer.Write((byte)0x02); // Source path[0] - Root Menu
        writer.Write((byte)0x00); // Source path[1] - Run Mode
        writer.Write((byte)0x01); // Source path[2] - Controller ID
        writer.Write((byte)0x07); // Source path[3] - Zone Info

        writer.Write((ushort)0); // Packet number
        writer.Write((ushort)1); // Packet count
        writer.Write((ushort)10); // Data length
        
        // Zone info data
        writer.Write(new byte[10]); // Dummy zone info data

        writer.Write((byte)0x7C); // Checksum (approximate)
        writer.Write((byte)0xF7); // End

        return stream.ToArray();
    }

    private static byte[] CreateZonePowerPacketBuffer()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // RNet packet header
        writer.Write((byte)0xF0); // Start
        writer.Write((byte)0x00); // Target Controller ID
        writer.Write((byte)0x00); // Target Zone ID
        writer.Write((byte)0x7F); // Target Keypad ID
        writer.Write((byte)0x01); // Source Controller ID
        writer.Write((byte)0x02); // Source Zone ID
        writer.Write((byte)0x70); // Source Keypad ID
        writer.Write((byte)0x00); // Message type (DataPacket)

        // DataPacket body with zone power pattern
        writer.Write((byte)0); // Target path length
        writer.Write((byte)4); // Source path length
        writer.Write((byte)0x02); // Source path[0] - Root Menu
        writer.Write((byte)0x00); // Source path[1] - Run Mode
        writer.Write((byte)0x01); // Source path[2] - Controller ID
        writer.Write((byte)0x06); // Source path[3] - Zone Power

        writer.Write((ushort)0); // Packet number
        writer.Write((ushort)1); // Packet count
        writer.Write((ushort)1); // Data length
        writer.Write((byte)1); // Power on

        writer.Write((byte)0x7C); // Checksum (approximate)
        writer.Write((byte)0xF7); // End

        return stream.ToArray();
    }

    private static byte[] CreateZoneSourcePacketBuffer()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // RNet packet header
        writer.Write((byte)0xF0); // Start
        writer.Write((byte)0x00); // Target Controller ID
        writer.Write((byte)0x00); // Target Zone ID
        writer.Write((byte)0x7F); // Target Keypad ID
        writer.Write((byte)0x01); // Source Controller ID
        writer.Write((byte)0x02); // Source Zone ID
        writer.Write((byte)0x70); // Source Keypad ID
        writer.Write((byte)0x00); // Message type (DataPacket)

        // DataPacket body with zone source pattern
        writer.Write((byte)0); // Target path length
        writer.Write((byte)4); // Source path length
        writer.Write((byte)0x02); // Source path[0] - Root Menu
        writer.Write((byte)0x00); // Source path[1] - Run Mode
        writer.Write((byte)0x01); // Source path[2] - Controller ID
        writer.Write((byte)0x02); // Source path[3] - Zone Source

        writer.Write((ushort)0); // Packet number
        writer.Write((ushort)1); // Packet count
        writer.Write((ushort)1); // Data length
        writer.Write((byte)3); // Source ID

        writer.Write((byte)0x7C); // Checksum (approximate)
        writer.Write((byte)0xF7); // End

        return stream.ToArray();
    }

    private static byte[] CreateZoneVolumePacketBuffer()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // RNet packet header
        writer.Write((byte)0xF0); // Start
        writer.Write((byte)0x00); // Target Controller ID
        writer.Write((byte)0x00); // Target Zone ID
        writer.Write((byte)0x7F); // Target Keypad ID
        writer.Write((byte)0x01); // Source Controller ID
        writer.Write((byte)0x02); // Source Zone ID
        writer.Write((byte)0x70); // Source Keypad ID
        writer.Write((byte)0x00); // Message type (DataPacket)

        // DataPacket body with zone volume pattern
        writer.Write((byte)0); // Target path length
        writer.Write((byte)4); // Source path length
        writer.Write((byte)0x02); // Source path[0] - Root Menu
        writer.Write((byte)0x00); // Source path[1] - Run Mode
        writer.Write((byte)0x01); // Source path[2] - Controller ID
        writer.Write((byte)0x01); // Source path[3] - Zone Volume

        writer.Write((ushort)0); // Packet number
        writer.Write((ushort)1); // Packet count
        writer.Write((ushort)1); // Data length
        writer.Write((byte)25); // Volume (50 when multiplied by 2)

        writer.Write((byte)0x7C); // Checksum (approximate)
        writer.Write((byte)0xF7); // End

        return stream.ToArray();
    }

    private static byte[] CreateKeypadEventPacketBuffer()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // RNet packet header
        writer.Write((byte)0xF0); // Start
        writer.Write((byte)0x00); // Target Controller ID
        writer.Write((byte)0x00); // Target Zone ID
        writer.Write((byte)0x7F); // Target Keypad ID
        writer.Write((byte)0x01); // Source Controller ID
        writer.Write((byte)0x02); // Source Zone ID
        writer.Write((byte)0x70); // Source Keypad ID
        writer.Write((byte)0x05); // Message type (EventPacket)

        // EventPacket body with keypad pattern
        writer.Write((byte)0); // Target path length
        writer.Write((byte)2); // Source path length
        writer.Write((byte)0x04); // Source path[0]
        writer.Write((byte)0x03); // Source path[1]

        writer.Write((ushort)0x20); // Event ID (valid keypad key)
        writer.Write((ushort)0); // Event timestamp
        writer.Write((ushort)0); // Event data
        writer.Write((byte)1); // Event priority

        writer.Write((byte)0x7C); // Checksum (approximate)
        writer.Write((byte)0xF7); // End

        return stream.ToArray();
    }
}