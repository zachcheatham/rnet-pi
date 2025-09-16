using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RNetPi.API.Controllers;
using RNetPi.Core.Interfaces;
using RNetPi.Core.Models;
using Xunit;

namespace RNetPi.Core.Tests.Controllers;

public class WebHookControllerTests
{
    private readonly Mock<ILogger<WebHookController>> _mockLogger;
    private readonly Mock<IConfigurationService> _mockConfigService;
    private readonly Mock<IRNetController> _mockRNetController;
    private readonly WebHookController _controller;
    private readonly Configuration _testConfig;

    public WebHookControllerTests()
    {
        _mockLogger = new Mock<ILogger<WebHookController>>();
        _mockConfigService = new Mock<IConfigurationService>();
        _mockRNetController = new Mock<IRNetController>();
        
        _testConfig = new Configuration
        {
            WebHookPassword = "test-password"
        };
        
        _mockConfigService.Setup(x => x.Configuration).Returns(_testConfig);
        _mockRNetController.Setup(x => x.IsConnected).Returns(true);

        _controller = new WebHookController(
            _mockLogger.Object,
            _mockConfigService.Object,
            _mockRNetController.Object);

        // Setup HTTP context for password query parameter
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?pass=test-password");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task AllOn_WithValidPassword_ShouldReturnOk()
    {
        // Arrange
        _mockRNetController.Setup(x => x.SetAllPowerAsync(true))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AllOn();

        // Assert
        Assert.IsType<OkResult>(result);
        _mockRNetController.Verify(x => x.SetAllPowerAsync(true), Times.Once);
    }

    [Fact]
    public async Task AllOff_WithValidPassword_ShouldReturnOk()
    {
        // Arrange
        _mockRNetController.Setup(x => x.SetAllPowerAsync(false))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AllOff();

        // Assert
        Assert.IsType<OkResult>(result);
        _mockRNetController.Verify(x => x.SetAllPowerAsync(false), Times.Once);
    }

    [Fact]
    public async Task AllMute_WithValidPassword_ShouldReturnOk()
    {
        // Arrange
        _mockRNetController.Setup(x => x.SetAllMuteAsync(true, 1000))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AllMute();

        // Assert
        Assert.IsType<OkResult>(result);
        _mockRNetController.Verify(x => x.SetAllMuteAsync(true, 1000), Times.Once);
    }

    [Fact]
    public async Task AllUnmute_WithValidPassword_ShouldReturnOk()
    {
        // Arrange
        _mockRNetController.Setup(x => x.SetAllMuteAsync(false, 1000))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AllUnmute();

        // Assert
        Assert.IsType<OkResult>(result);
        _mockRNetController.Verify(x => x.SetAllMuteAsync(false, 1000), Times.Once);
    }

    [Fact]
    public async Task AllOn_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?pass=wrong-password");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _controller.AllOn();

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task AllOn_WithNoPassword_ShouldReturnBadRequest()
    {
        // Arrange
        _testConfig.WebHookPassword = "";
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _controller.AllOn();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AllOn_WhenRNetNotConnected_ShouldReturnServiceUnavailable()
    {
        // Arrange
        _mockRNetController.Setup(x => x.IsConnected).Returns(false);

        // Act
        var result = await _controller.AllOn();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, statusCodeResult.StatusCode);
    }

    [Fact]
    public void SetZoneVolume_WithUnknownZone_ShouldReturnNotFound()
    {
        // Arrange
        _mockRNetController.Setup(x => x.FindZoneByName("unknown-zone"))
            .Returns((Zone?)null);

        // Act
        var result = _controller.SetZoneVolume("unknown-zone", 50);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}