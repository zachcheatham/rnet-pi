using RNetPi.Core.Models;

namespace RNetPi.Core.Interfaces;

public interface IConfigurationService
{
    Configuration Configuration { get; }
    
    Task LoadAsync();
    Task SaveAsync();
    
    void SetProperty(string key, object value);
    T GetProperty<T>(string key);
}