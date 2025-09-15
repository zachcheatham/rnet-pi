using Microsoft.AspNetCore.Mvc;
using RNetPi.Core.Interfaces;
using RNetPi.Core.Models;

namespace RNetPi.API.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebHookController : ControllerBase
{
    private readonly ILogger<WebHookController> _logger;
    private readonly IConfigurationService _configurationService;
    private readonly IRNetController _rnetController;

    public WebHookController(
        ILogger<WebHookController> logger,
        IConfigurationService configurationService,
        IRNetController rnetController)
    {
        _logger = logger;
        _configurationService = configurationService;
        _rnetController = rnetController;
    }

    private IActionResult ValidateWebHook()
    {
        var password = _configurationService.Configuration.WebHookPassword;
        if (string.IsNullOrEmpty(password))
        {
            _logger.LogWarning("[Web Hook] Web hooks are disabled - no password configured");
            return BadRequest("Web hooks are disabled");
        }

        var requestPassword = HttpContext.Request.Query["pass"].FirstOrDefault();
        if (requestPassword != password)
        {
            _logger.LogWarning("[Web Hook] Bad password in request");
            return Unauthorized("Invalid password");
        }

        if (!_rnetController.IsConnected)
        {
            return StatusCode(503, "RNet not connected");
        }

        return Ok();
    }

    private Zone? FindZoneByName(string zoneName)
    {
        return _rnetController.FindZoneByName(zoneName);
    }

    [HttpPut("on")]
    public async Task<IActionResult> AllOn()
    {
        var validation = ValidateWebHook();
        if (validation is not OkResult)
            return validation;

        try
        {
            await _rnetController.SetAllPowerAsync(true);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Web Hook] Failed to turn all zones on");
            return StatusCode(500, "Failed to execute command");
        }
    }

    [HttpPut("off")]
    public async Task<IActionResult> AllOff()
    {
        var validation = ValidateWebHook();
        if (validation is not OkResult)
            return validation;

        try
        {
            await _rnetController.SetAllPowerAsync(false);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Web Hook] Failed to turn all zones off");
            return StatusCode(500, "Failed to execute command");
        }
    }

    [HttpPut("mute")]
    public async Task<IActionResult> AllMute()
    {
        var validation = ValidateWebHook();
        if (validation is not OkResult)
            return validation;

        try
        {
            await _rnetController.SetAllMuteAsync(true, 1000);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Web Hook] Failed to mute all zones");
            return StatusCode(500, "Failed to execute command");
        }
    }

    [HttpPut("unmute")]
    public async Task<IActionResult> AllUnmute()
    {
        var validation = ValidateWebHook();
        if (validation is not OkResult)
            return validation;

        try
        {
            await _rnetController.SetAllMuteAsync(false, 1000);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Web Hook] Failed to unmute all zones");
            return StatusCode(500, "Failed to execute command");
        }
    }

    [HttpPut("{zoneName}/volume/{volume:int}")]
    public async Task<IActionResult> SetZoneVolume(string zoneName, int volume)
    {
        var validation = ValidateWebHook();
        if (validation is not OkResult)
            return validation;

        var zone = FindZoneByName(zoneName);
        if (zone == null)
        {
            _logger.LogWarning("[Web Hook] Unknown zone {ZoneName}", zoneName);
            return NotFound($"Zone '{zoneName}' not found");
        }

        try
        {
            // Ensure even volume levels as per original JavaScript code
            var adjustedVolume = Math.Floor(volume / 2.0) * 2;
            zone.SetVolume((int)adjustedVolume);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Web Hook] Failed to set volume for zone {ZoneName}", zoneName);
            return StatusCode(500, "Failed to execute command");
        }
    }

    [HttpPut("{zoneName}/source/{sourceName}")]
    public async Task<IActionResult> SetZoneSource(string zoneName, string sourceName)
    {
        var validation = ValidateWebHook();
        if (validation is not OkResult)
            return validation;

        var zone = FindZoneByName(zoneName);
        if (zone == null)
        {
            _logger.LogWarning("[Web Hook] Unknown zone {ZoneName}", zoneName);
            return NotFound($"Zone '{zoneName}' not found");
        }

        try
        {
            var source = _rnetController.FindSourceByName(sourceName);
            if (source == null)
            {
                _logger.LogWarning("[Web Hook] Unknown source {SourceName}", sourceName);
                return NotFound($"Source '{sourceName}' not found");
            }
            
            zone.SetSource(source.SourceID);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Web Hook] Failed to set source for zone {ZoneName}", zoneName);
            return StatusCode(500, "Failed to execute command");
        }
    }

    [HttpPut("{zoneName}/mute")]
    public async Task<IActionResult> MuteZone(string zoneName)
    {
        var validation = ValidateWebHook();
        if (validation is not OkResult)
            return validation;

        var zone = FindZoneByName(zoneName);
        if (zone == null)
        {
            _logger.LogWarning("[Web Hook] Unknown zone {ZoneName}", zoneName);
            return NotFound($"Zone '{zoneName}' not found");
        }

        try
        {
            zone.SetMute(true, 1000);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Web Hook] Failed to mute zone {ZoneName}", zoneName);
            return StatusCode(500, "Failed to execute command");
        }
    }

    [HttpPut("{zoneName}/unmute")]
    public async Task<IActionResult> UnmuteZone(string zoneName)
    {
        var validation = ValidateWebHook();
        if (validation is not OkResult)
            return validation;

        var zone = FindZoneByName(zoneName);
        if (zone == null)
        {
            _logger.LogWarning("[Web Hook] Unknown zone {ZoneName}", zoneName);
            return NotFound($"Zone '{zoneName}' not found");
        }

        try
        {
            zone.SetMute(false, 1000);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Web Hook] Failed to unmute zone {ZoneName}", zoneName);
            return StatusCode(500, "Failed to execute command");
        }
    }

    [HttpPut("{zoneName}/on")]
    public async Task<IActionResult> TurnZoneOn(string zoneName)
    {
        var validation = ValidateWebHook();
        if (validation is not OkResult)
            return validation;

        var zone = FindZoneByName(zoneName);
        if (zone == null)
        {
            _logger.LogWarning("[Web Hook] Unknown zone {ZoneName}", zoneName);
            return NotFound($"Zone '{zoneName}' not found");
        }

        try
        {
            zone.SetPower(true);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Web Hook] Failed to turn on zone {ZoneName}", zoneName);
            return StatusCode(500, "Failed to execute command");
        }
    }

    [HttpPut("{zoneName}/off")]
    public async Task<IActionResult> TurnZoneOff(string zoneName)
    {
        var validation = ValidateWebHook();
        if (validation is not OkResult)
            return validation;

        var zone = FindZoneByName(zoneName);
        if (zone == null)
        {
            _logger.LogWarning("[Web Hook] Unknown zone {ZoneName}", zoneName);
            return NotFound($"Zone '{zoneName}' not found");
        }

        try
        {
            zone.SetPower(false);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Web Hook] Failed to turn off zone {ZoneName}", zoneName);
            return StatusCode(500, "Failed to execute command");
        }
    }
}