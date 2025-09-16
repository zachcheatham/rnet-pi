using Microsoft.AspNetCore.Mvc;
using RNetPi.Core.Interfaces;
using RNetPi.Core.Models;

namespace RNetPi.API.Controllers;

[ApiController]
[Route("api/webhooks")]
[Produces("application/json")]
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

    /// <summary>
    /// Turn all zones on
    /// </summary>
    /// <remarks>Powers on all audio zones in the RNet system. Requires 'pass' query parameter with webhook password.</remarks>
    /// <returns>Success or error message</returns>
    /// <response code="200">All zones turned on successfully</response>
    /// <response code="400">Web hooks are disabled - no password configured</response>
    /// <response code="401">Invalid password</response>
    /// <response code="500">Failed to execute command</response>
    /// <response code="503">RNet not connected</response>
    [HttpPut("on")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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

    /// <summary>
    /// Turn all zones off
    /// </summary>
    /// <remarks>Powers off all audio zones in the RNet system. Requires 'pass' query parameter with webhook password.</remarks>
    /// <returns>Success or error message</returns>
    /// <response code="200">All zones turned off successfully</response>
    /// <response code="400">Web hooks are disabled - no password configured</response>
    /// <response code="401">Invalid password</response>
    /// <response code="500">Failed to execute command</response>
    /// <response code="503">RNet not connected</response>
    [HttpPut("off")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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

    /// <summary>
    /// Mute all zones
    /// </summary>
    /// <remarks>Mutes all audio zones in the RNet system. Requires 'pass' query parameter with webhook password.</remarks>
    /// <returns>Success or error message</returns>
    /// <response code="200">All zones muted successfully</response>
    /// <response code="400">Web hooks are disabled - no password configured</response>
    /// <response code="401">Invalid password</response>
    /// <response code="500">Failed to execute command</response>
    /// <response code="503">RNet not connected</response>
    [HttpPut("mute")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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

    /// <summary>
    /// Unmute all zones
    /// </summary>
    /// <remarks>Unmutes all audio zones in the RNet system. Requires 'pass' query parameter with webhook password.</remarks>
    /// <returns>Success or error message</returns>
    /// <response code="200">All zones unmuted successfully</response>
    /// <response code="400">Web hooks are disabled - no password configured</response>
    /// <response code="401">Invalid password</response>
    /// <response code="500">Failed to execute command</response>
    /// <response code="503">RNet not connected</response>
    [HttpPut("unmute")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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

    /// <summary>
    /// Set zone volume
    /// </summary>
    /// <remarks>Sets the volume level for a specific audio zone. Volume levels are automatically adjusted to even numbers (0-100). Requires 'pass' query parameter with webhook password.</remarks>
    /// <param name="zoneName">Name of the audio zone</param>
    /// <param name="volume">Volume level (0-100, will be adjusted to even numbers)</param>
    /// <returns>Success or error message</returns>
    /// <response code="200">Zone volume set successfully</response>
    /// <response code="400">Web hooks are disabled - no password configured</response>
    /// <response code="401">Invalid password</response>
    /// <response code="404">Zone not found</response>
    /// <response code="500">Failed to execute command</response>
    /// <response code="503">RNet not connected</response>
    [HttpPut("{zoneName}/volume/{volume:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult SetZoneVolume(string zoneName, int volume)
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

    /// <summary>
    /// Set zone source
    /// </summary>
    /// <remarks>Changes the audio source for a specific zone. Requires 'pass' query parameter with webhook password.</remarks>
    /// <param name="zoneName">Name of the audio zone</param>
    /// <param name="sourceName">Name of the audio source</param>
    /// <returns>Success or error message</returns>
    /// <response code="200">Zone source set successfully</response>
    /// <response code="400">Web hooks are disabled - no password configured</response>
    /// <response code="401">Invalid password</response>
    /// <response code="404">Zone or source not found</response>
    /// <response code="500">Failed to execute command</response>
    /// <response code="503">RNet not connected</response>
    [HttpPut("{zoneName}/source/{sourceName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult SetZoneSource(string zoneName, string sourceName)
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

    /// <summary>
    /// Mute zone
    /// </summary>
    /// <remarks>Mutes a specific audio zone. Requires 'pass' query parameter with webhook password.</remarks>
    /// <param name="zoneName">Name of the audio zone</param>
    /// <returns>Success or error message</returns>
    /// <response code="200">Zone muted successfully</response>
    /// <response code="400">Web hooks are disabled - no password configured</response>
    /// <response code="401">Invalid password</response>
    /// <response code="404">Zone not found</response>
    /// <response code="500">Failed to execute command</response>
    /// <response code="503">RNet not connected</response>
    [HttpPut("{zoneName}/mute")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult MuteZone(string zoneName)
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

    /// <summary>
    /// Unmute zone
    /// </summary>
    /// <remarks>Unmutes a specific audio zone. Requires 'pass' query parameter with webhook password.</remarks>
    /// <param name="zoneName">Name of the audio zone</param>
    /// <returns>Success or error message</returns>
    /// <response code="200">Zone unmuted successfully</response>
    /// <response code="400">Web hooks are disabled - no password configured</response>
    /// <response code="401">Invalid password</response>
    /// <response code="404">Zone not found</response>
    /// <response code="500">Failed to execute command</response>
    /// <response code="503">RNet not connected</response>
    [HttpPut("{zoneName}/unmute")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult UnmuteZone(string zoneName)
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

    /// <summary>
    /// Turn zone on
    /// </summary>
    /// <remarks>Powers on a specific audio zone. Requires 'pass' query parameter with webhook password.</remarks>
    /// <param name="zoneName">Name of the audio zone</param>
    /// <returns>Success or error message</returns>
    /// <response code="200">Zone turned on successfully</response>
    /// <response code="400">Web hooks are disabled - no password configured</response>
    /// <response code="401">Invalid password</response>
    /// <response code="404">Zone not found</response>
    /// <response code="500">Failed to execute command</response>
    /// <response code="503">RNet not connected</response>
    [HttpPut("{zoneName}/on")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult TurnZoneOn(string zoneName)
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

    /// <summary>
    /// Turn zone off
    /// </summary>
    /// <remarks>Powers off a specific audio zone. Requires 'pass' query parameter with webhook password.</remarks>
    /// <param name="zoneName">Name of the audio zone</param>
    /// <returns>Success or error message</returns>
    /// <response code="200">Zone turned off successfully</response>
    /// <response code="400">Web hooks are disabled - no password configured</response>
    /// <response code="401">Invalid password</response>
    /// <response code="404">Zone not found</response>
    /// <response code="500">Failed to execute command</response>
    /// <response code="503">RNet not connected</response>
    [HttpPut("{zoneName}/off")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult TurnZoneOff(string zoneName)
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