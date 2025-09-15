using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RNetPi.Core.Interfaces;

namespace RNetPi.Infrastructure.Services;

public class UpdateService : IUpdateService
{
    private readonly ILogger<UpdateService> _logger;
    private const int UPDATE_CHECK_FREQUENCY = 86400; // 24 hours in seconds
    
    private DateTime _lastUpdateCheck = DateTime.MinValue;
    private bool _updateAvailable = false;
    private string? _latestVersion = null;
    private bool _updating = false;

    public string CurrentVersion { get; }
    
    public event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

    public UpdateService(ILogger<UpdateService> logger)
    {
        _logger = logger;
        CurrentVersion = GetCurrentVersion();
    }

    private string GetCurrentVersion()
    {
        // Try to get version from assembly
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        if (version != null)
        {
            return version.ToString();
        }

        // Fallback: try to read from package.json-like file or use default
        try
        {
            var versionFilePath = Path.Combine(Directory.GetCurrentDirectory(), "version.txt");
            if (File.Exists(versionFilePath))
            {
                return File.ReadAllText(versionFilePath).Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read version file");
        }

        return "1.0.0"; // Default version
    }

    public async Task<(string? latestVersion, string currentVersion)> CheckForUpdatesAsync()
    {
        if (_updateAvailable && _latestVersion != null)
        {
            return (_latestVersion, CurrentVersion);
        }

        var timeSinceLastCheck = DateTime.UtcNow - _lastUpdateCheck;
        if (timeSinceLastCheck.TotalSeconds >= UPDATE_CHECK_FREQUENCY)
        {
            _logger.LogInformation("Checking for updates...");
            
            try
            {
                // Run git fetch
                var fetchResult = await RunGitCommandAsync("fetch");
                if (fetchResult.success)
                {
                    // Get remote version information
                    var showResult = await RunGitCommandAsync("show origin/master:version.txt");
                    if (showResult.success && !string.IsNullOrWhiteSpace(showResult.output))
                    {
                        var remoteVersion = showResult.output.Trim();
                        _lastUpdateCheck = DateTime.UtcNow;
                        
                        if (remoteVersion != CurrentVersion)
                        {
                            _updateAvailable = true;
                            _latestVersion = remoteVersion;
                            _logger.LogInformation("Update {CurrentVersion} -> {LatestVersion} available!", CurrentVersion, remoteVersion);
                            
                            UpdateAvailable?.Invoke(this, new UpdateAvailableEventArgs(remoteVersion, CurrentVersion));
                            return (remoteVersion, CurrentVersion);
                        }
                        else
                        {
                            _logger.LogInformation("No updates are available.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Failed to check for updates: git show failed: {Output}", showResult.output);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to check for updates: git fetch failed: {Output}", fetchResult.output);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check for updates");
            }
        }

        return (null, CurrentVersion);
    }

    public async Task<bool> UpdateAsync()
    {
        if (!_updateAvailable)
        {
            _logger.LogWarning("Update() called while update is not available.");
            return false;
        }

        if (_updating)
        {
            _logger.LogWarning("Update already in progress.");
            return false;
        }

        _updating = true;
        try
        {
            _logger.LogInformation("Starting update process...");
            
            var pullResult = await RunGitCommandAsync("pull");
            if (pullResult.success)
            {
                if (pullResult.output.Contains("Updating"))
                {
                    _logger.LogInformation("Update complete. Application should restart.");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Update failed: git pull did not indicate changes:\n{Output}", pullResult.output);
                }
            }
            else
            {
                _logger.LogWarning("Update failed: git pull failed: {Output}", pullResult.output);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update failed with exception");
        }
        finally
        {
            _updating = false;
        }

        return false;
    }

    private async Task<(bool success, string output)> RunGitCommandAsync(string arguments)
    {
        try
        {
            using var process = new Process();
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();
            
            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode == 0)
            {
                return (true, output);
            }
            else
            {
                return (false, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run git command: {Arguments}", arguments);
            return (false, ex.Message);
        }
    }
}