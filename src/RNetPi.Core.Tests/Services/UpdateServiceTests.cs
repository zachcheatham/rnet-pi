using Microsoft.Extensions.Logging;
using Moq;
using RNetPi.Infrastructure.Services;
using Xunit;

namespace RNetPi.Core.Tests.Services;

public class UpdateServiceTests
{
    private readonly Mock<ILogger<UpdateService>> _mockLogger;
    private readonly UpdateService _updateService;

    public UpdateServiceTests()
    {
        _mockLogger = new Mock<ILogger<UpdateService>>();
        _updateService = new UpdateService(_mockLogger.Object);
    }

    [Fact]
    public void CurrentVersion_ShouldReturnValidVersion()
    {
        // Act
        var version = _updateService.CurrentVersion;

        // Assert
        Assert.NotNull(version);
        Assert.NotEmpty(version);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WhenUpdateNotAvailable_ShouldReturnCurrentVersion()
    {
        // Act
        var (latestVersion, currentVersion) = await _updateService.CheckForUpdatesAsync();

        // Assert
        Assert.Equal(_updateService.CurrentVersion, currentVersion);
        // latestVersion may be null if no updates are available
    }

    [Fact]
    public async Task UpdateAsync_WhenNoUpdateAvailable_ShouldReturnFalse()
    {
        // Act
        var result = await _updateService.UpdateAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UpdateAvailable_Event_ShouldBeDefinedProperly()
    {
        // Arrange
        var eventFired = false;
        _updateService.UpdateAvailable += (sender, args) => eventFired = true;

        // This test just verifies the event can be subscribed to
        // Actual event firing would require mocking git operations
        Assert.False(eventFired); // Should be false initially
    }
}