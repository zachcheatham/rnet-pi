using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using RNetPi.Core.Packets;
using RNetPi.Core.Services;
using Xunit;

namespace RNetPi.Core.Tests.Services;

public class PacketFactoryTests
{
    [Theory]
    [InlineData(0x01, typeof(PacketC2SIntent))]
    [InlineData(0x03, typeof(PacketC2SDisconnect))]
    public void CreatePacket_ValidPacketType_ReturnsCorrectPacketType(byte packetType, Type expectedType)
    {
        // Arrange
        var data = new byte[] { 0x02 }; // Intent data for subscribe

        // Act
        var packet = PacketFactory.CreatePacket(packetType, data);

        // Assert
        Assert.NotNull(packet);
        Assert.IsType(expectedType, packet);
        Assert.Equal(packetType, packet.GetID());
    }

    [Fact]
    public void CreatePacket_UnknownPacketType_ReturnsNull()
    {
        // Arrange
        byte unknownPacketType = 0xFF;
        var data = new byte[] { 0x00 };

        // Act
        var packet = PacketFactory.CreatePacket(unknownPacketType, data);

        // Assert
        Assert.Null(packet);
    }

    [Fact]
    public void CreatePacket_IntentPacket_ParsesCorrectly()
    {
        // Arrange
        byte packetType = 0x01;
        var data = new byte[] { 0x02 }; // Subscribe intent

        // Act
        var packet = PacketFactory.CreatePacket(packetType, data);

        // Assert
        Assert.NotNull(packet);
        Assert.IsType<PacketC2SIntent>(packet);
        var intentPacket = (PacketC2SIntent)packet;
        Assert.Equal(0x02, intentPacket.GetIntent());
    }
}

public class NetworkClientTests
{
    private class TestNetworkClient : NetworkClient
    {
        public string TestAddress { get; set; } = "127.0.0.1:1234";
        public bool SendPacketCalled { get; private set; }
        public bool SendBufferCalled { get; private set; }
        public bool DisconnectCalled { get; private set; }

        public override string GetAddress() => TestAddress;

        public override Task SendPacketAsync(PacketS2C packet)
        {
            SendPacketCalled = true;
            return Task.CompletedTask;
        }

        public override Task SendBufferAsync(byte[] buffer)
        {
            SendBufferCalled = true;
            return Task.CompletedTask;
        }

        public override Task DisconnectAsync()
        {
            DisconnectCalled = true;
            return Task.CompletedTask;
        }

        public void SimulatePacketReceived(byte packetType, byte[] data)
        {
            HandlePacket(packetType, data);
        }
    }

    [Fact]
    public void InitialState_IsCorrect()
    {
        // Arrange & Act
        var client = new TestNetworkClient();

        // Assert
        Assert.Equal(ClientIntent.None, client.Intent);
        Assert.False(client.IsValid);
        Assert.False(client.IsSubscribed);
    }

    [Fact]
    public void HandleIntentPacket_Subscribe_UpdatesStateAndFiresEvent()
    {
        // Arrange
        var client = new TestNetworkClient();
        var subscribedEventFired = false;
        client.Subscribed += (sender, e) => subscribedEventFired = true;

        var intentData = new byte[] { 0x02 }; // Subscribe intent

        // Act
        client.SimulatePacketReceived(0x01, intentData);

        // Assert
        Assert.Equal(ClientIntent.Subscribe, client.Intent);
        Assert.True(client.IsValid);
        Assert.True(client.IsSubscribed);
        Assert.True(subscribedEventFired);
    }

    [Fact]
    public void HandleValidPacket_WhenClientIsValid_FiresPacketReceivedEvent()
    {
        // Arrange
        var client = new TestNetworkClient();
        PacketC2S? receivedPacket = null;
        client.PacketReceived += (sender, packet) => receivedPacket = packet;

        // First set client to valid state
        client.SimulatePacketReceived(0x01, new byte[] { 0x02 });

        // Act - Send a different packet
        client.SimulatePacketReceived(0x02, new byte[] { 0x00 });

        // Assert
        Assert.NotNull(receivedPacket);
        Assert.IsType<PacketC2SProperty>(receivedPacket);
    }

    [Fact]
    public void HandleValidPacket_WhenClientIsNotValid_DoesNotFirePacketReceivedEvent()
    {
        // Arrange
        var client = new TestNetworkClient();
        var packetReceivedEventFired = false;
        client.PacketReceived += (sender, packet) => packetReceivedEventFired = true;

        // Act - Send packet without setting client to valid state first
        client.SimulatePacketReceived(0x02, new byte[] { 0x00 });

        // Assert
        Assert.False(packetReceivedEventFired);
    }
}

public class NetworkServerConfigTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var config = new NetworkServerConfig();

        // Assert
        Assert.Equal("RNet-Pi", config.Name);
        Assert.Equal("0.0.0.0", config.Host);
        Assert.Equal(4000, config.Port);
        Assert.Null(config.WebHost);
        Assert.Null(config.WebPort);
    }
}

public class NetworkServerTests
{
    private readonly Mock<ILogger<NetworkServer>> _mockLogger;
    private readonly NetworkServerConfig _config;

    public NetworkServerTests()
    {
        _mockLogger = new Mock<ILogger<NetworkServer>>();
        _config = new NetworkServerConfig
        {
            Name = "Test Server",
            Host = "127.0.0.1",
            Port = 14000 // Use a different port to avoid conflicts
        };
    }

    [Fact]
    public void Constructor_ValidParameters_SetsProperties()
    {
        // Act
        var server = new NetworkServer(_config, _mockLogger.Object);

        // Assert
        Assert.Equal("Test Server", server.Name);
        Assert.Equal(0, server.ClientCount);
    }

    [Fact]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NetworkServer(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NetworkServer(_config, null!));
    }

    [Fact]
    public void SetName_ValidName_UpdatesName()
    {
        // Arrange
        var server = new NetworkServer(_config, _mockLogger.Object);

        // Act
        server.SetName("New Name");

        // Assert
        Assert.Equal("New Name", server.Name);
    }

    [Fact]
    public void SetName_EmptyName_DoesNotUpdateName()
    {
        // Arrange
        var server = new NetworkServer(_config, _mockLogger.Object);
        var originalName = server.Name;

        // Act
        server.SetName("");

        // Assert
        Assert.Equal(originalName, server.Name);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        // Arrange
        var server = new NetworkServer(_config, _mockLogger.Object);

        // Act & Assert
        server.Dispose(); // Should not throw
    }
}