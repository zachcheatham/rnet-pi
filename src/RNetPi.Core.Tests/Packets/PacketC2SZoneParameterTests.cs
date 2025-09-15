using RNetPi.Core.Packets;
using RNetPi.Core.Utilities;

namespace RNetPi.Core.Tests.Packets;

public class PacketC2SZoneParameterTests
{
    [Fact]
    public void Constructor_ShouldParseSignedParameter()
    {
        // Arrange - Bass parameter (ID 0) with value -5
        byte[] data = [1, 2, 0, 251]; // Controller: 1, Zone: 2, Parameter: 0 (Bass), Value: -5 (251 as unsigned)

        // Act
        var packet = new PacketC2SZoneParameter(data);

        // Assert
        Assert.Equal(PacketC2SZoneParameter.ID, packet.GetID());
        Assert.Equal(1, packet.ControllerID);
        Assert.Equal(2, packet.ZoneID);
        Assert.Equal(0, packet.ParameterID);
        Assert.Equal((sbyte)-5, packet.ParameterValue);
    }

    [Fact]
    public void Constructor_ShouldParseBooleanParameter()
    {
        // Arrange - Loudness parameter (ID 2) with value true
        byte[] data = [1, 2, 2, 1]; // Controller: 1, Zone: 2, Parameter: 2 (Loudness), Value: 1 (true)

        // Act
        var packet = new PacketC2SZoneParameter(data);

        // Assert
        Assert.Equal(PacketC2SZoneParameter.ID, packet.GetID());
        Assert.Equal(1, packet.ControllerID);
        Assert.Equal(2, packet.ZoneID);
        Assert.Equal(2, packet.ParameterID);
        Assert.Equal(true, packet.ParameterValue);
    }

    [Fact]
    public void Constructor_ShouldParseUnsignedParameter()
    {
        // Arrange - Turn on Volume parameter (ID 4) with value 50
        byte[] data = [1, 2, 4, 50]; // Controller: 1, Zone: 2, Parameter: 4 (Turn on Volume), Value: 50

        // Act
        var packet = new PacketC2SZoneParameter(data);

        // Assert
        Assert.Equal(PacketC2SZoneParameter.ID, packet.GetID());
        Assert.Equal(1, packet.ControllerID);
        Assert.Equal(2, packet.ZoneID);
        Assert.Equal(4, packet.ParameterID);
        Assert.Equal((byte)50, packet.ParameterValue);
    }

    [Fact]
    public void GetID_ShouldReturn0x0B()
    {
        // Arrange
        byte[] data = [0, 0, 0, 0];
        var packet = new PacketC2SZoneParameter(data);

        // Act
        var id = packet.GetID();

        // Assert
        Assert.Equal(0x0B, id);
    }
}