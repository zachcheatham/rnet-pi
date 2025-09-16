using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace RNetPi.Core.Logging;

public sealed class EnhancedConsoleFormatter : ConsoleFormatter
{
    private readonly EnhancedConsoleFormatterOptions _options;

    public EnhancedConsoleFormatter(IOptionsMonitor<EnhancedConsoleFormatterOptions> options)
        : base("enhanced")
    {
        _options = options.CurrentValue;
    }

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        if (message is null)
        {
            return;
        }

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        var threadId = Environment.CurrentManagedThreadId;
        var logLevel = logEntry.LogLevel.ToString().ToUpperInvariant();
        
        // Extract class and method name from category
        var category = logEntry.Category;
        var className = GetClassName(category);
        var methodName = GetCallingMethodName();

        var logBuilder = new StringBuilder();
        logBuilder.Append($"[{timestamp}] ");
        logBuilder.Append($"[TID:{threadId:D3}] ");
        logBuilder.Append($"[{logLevel}] ");
        logBuilder.Append($"[{className}::{methodName}] ");
        logBuilder.Append(message);

        if (logEntry.Exception != null)
        {
            logBuilder.AppendLine();
            logBuilder.Append(logEntry.Exception.ToString());
        }

        textWriter.WriteLine(logBuilder.ToString());
    }

    private static string GetClassName(string category)
    {
        // Extract class name from category (e.g., "RNetPi.Core.Services.SimpleRNetController" -> "SimpleRNetController")
        var lastDotIndex = category.LastIndexOf('.');
        return lastDotIndex >= 0 ? category[(lastDotIndex + 1)..] : category;
    }

    private static string GetCallingMethodName()
    {
        var stackTrace = new StackTrace();
        var frames = stackTrace.GetFrames();

        // Skip logging framework frames and find the first user code frame
        for (int i = 0; i < frames.Length; i++)
        {
            var method = frames[i].GetMethod();
            if (method?.DeclaringType != null)
            {
                var declaringType = method.DeclaringType;
                var typeName = declaringType.FullName ?? declaringType.Name;

                // Skip Microsoft.Extensions.Logging frames and our formatter
                if (!typeName.StartsWith("Microsoft.Extensions.Logging") &&
                    !typeName.StartsWith("RNetPi.Core.Logging") &&
                    !typeName.Contains("Logger") &&
                    method.Name != "GetCallingMethodName")
                {
                    return method.Name;
                }
            }
        }

        return "Unknown";
    }
}

public class EnhancedConsoleFormatterOptions : ConsoleFormatterOptions
{
    // Additional options can be added here if needed
}