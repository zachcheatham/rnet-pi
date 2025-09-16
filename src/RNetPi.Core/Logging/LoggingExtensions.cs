using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace RNetPi.Core.Logging;

public static class LoggingExtensions
{
    /// <summary>
    /// Adds enhanced console logging with timestamp, class name, function name, and thread ID
    /// </summary>
    public static ILoggingBuilder AddEnhancedConsole(this ILoggingBuilder builder)
    {
        builder.AddConsole(options =>
        {
            options.FormatterName = "enhanced";
        });
        
        builder.AddConsoleFormatter<EnhancedConsoleFormatter, EnhancedConsoleFormatterOptions>();
        
        return builder;
    }

    /// <summary>
    /// Helper method to log hex data with packet information
    /// </summary>
    public static void LogDataPacket(this ILogger logger, LogLevel logLevel, string direction, string packetType, byte[] data, string? additionalMessage = null)
    {
        if (!logger.IsEnabled(logLevel))
            return;

        var hexData = Convert.ToHexString(data);
        var message = additionalMessage != null 
            ? $"{direction} packet {packetType} {additionalMessage}: {hexData}"
            : $"{direction} packet {packetType}: {hexData}";
            
        logger.Log(logLevel, message);
    }

    /// <summary>
    /// Helper method to log sent data packets
    /// </summary>
    public static void LogSentPacket(this ILogger logger, string packetType, byte[] data, string? additionalMessage = null)
    {
        logger.LogDataPacket(LogLevel.Debug, "Sent", packetType, data, additionalMessage);
    }

    /// <summary>
    /// Helper method to log received data packets
    /// </summary>
    public static void LogReceivedPacket(this ILogger logger, string packetType, byte[] data, string? additionalMessage = null)
    {
        logger.LogDataPacket(LogLevel.Debug, "Received", packetType, data, additionalMessage);
    }
}