using System;
using System.Threading.Tasks;

namespace RNetPi.Core.Interfaces;

public interface IUpdateService
{
    string CurrentVersion { get; }
    
    Task<(string? latestVersion, string currentVersion)> CheckForUpdatesAsync();
    Task<bool> UpdateAsync();
    
    event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;
}

public class UpdateAvailableEventArgs : EventArgs
{
    public string LatestVersion { get; }
    public string CurrentVersion { get; }

    public UpdateAvailableEventArgs(string latestVersion, string currentVersion)
    {
        LatestVersion = latestVersion;
        CurrentVersion = currentVersion;
    }
}