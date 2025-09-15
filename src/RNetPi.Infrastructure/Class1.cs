using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RNetPi.Core.Interfaces;
using RNetPi.Core.Models;

namespace RNetPi.Infrastructure.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly string _configFilePath;
    private Configuration _configuration;

    public Configuration Configuration => _configuration;

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;
        _configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
        _configuration = new Configuration();
    }

    public async Task LoadAsync()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                var json = await File.ReadAllTextAsync(_configFilePath);
                var config = JsonSerializer.Deserialize<Configuration>(json);
                if (config != null)
                {
                    _configuration = config;
                    _logger.LogInformation("Configuration loaded from {FilePath}", _configFilePath);
                }
            }
            else
            {
                // Create default configuration if file doesn't exist
                await SaveAsync();
                _logger.LogInformation("Created default configuration at {FilePath}", _configFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration from {FilePath}", _configFilePath);
            // Use default configuration
            _configuration = new Configuration();
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_configuration, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_configFilePath, json);
            _logger.LogInformation("Configuration saved to {FilePath}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration to {FilePath}", _configFilePath);
        }
    }

    public void SetProperty(string key, object value)
    {
        var property = typeof(Configuration).GetProperty(key);
        if (property != null && property.CanWrite)
        {
            property.SetValue(_configuration, value);
        }
        else
        {
            _logger.LogWarning("Unknown configuration property: {Key}", key);
        }
    }

    public T GetProperty<T>(string key)
    {
        var property = typeof(Configuration).GetProperty(key);
        if (property != null && property.CanRead)
        {
            var value = property.GetValue(_configuration);
            if (value is T typedValue)
                return typedValue;
            
            // Try to convert
            try
            {
                return (T)Convert.ChangeType(value, typeof(T))!;
            }
            catch
            {
                return default(T)!;
            }
        }
        
        return default(T)!;
    }
}
