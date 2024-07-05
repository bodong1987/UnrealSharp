using UnrealSharp.Utils.Extensions.IO;
using UnrealSharp.Utils.Extensions;
using System.Diagnostics;

namespace UnrealSharp.Utils.Misc;

/// <summary>
/// Enum LoggerLevel
/// </summary>
public enum LoggerLevel
{
    /// <summary>
    /// The verbose
    /// </summary>
    Verbose = -10,

    /// <summary>
    /// The information
    /// </summary>
    Information = -9,

    /// <summary>
    /// The warning
    /// </summary>
    Warning = -8,

    /// <summary>
    /// The error
    /// </summary>
    Error = -7,

    /// <summary>
    /// The total
    /// </summary>
    Total = -5
}

/// <summary>
/// Interface ILoggerObserver
/// </summary>
public interface ILoggerObserver
{
    /// <summary>
    /// Called when [receive].
    /// May called from any thread...
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="tag">The tag.</param>
    /// <param name="message">The message.</param>
    // ReSharper disable once UnusedMemberInSuper.Global
    void OnEvent(LoggerLevel level, string? tag, string message);
}

/// <summary>
/// Class SystemLoggerObserver.
/// Implements the <see cref="UnrealSharp.Utils.Misc.ILoggerObserver" />
/// </summary>
/// <seealso cref="UnrealSharp.Utils.Misc.ILoggerObserver" />
public class SystemLoggerObserver : ILoggerObserver
{
    /// <summary>
    /// Called when [receive].
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="tag">The tag.</param>
    /// <param name="message">The message.</param>
    public void OnEvent(LoggerLevel level, string? tag, string message)
    {
        Logger.Log(level, tag, message);
    }
}

/// <summary>
/// Class DebuggerLoggerObserver.
/// Implements the <see cref="UnrealSharp.Utils.Misc.ILoggerObserver" />
/// </summary>
/// <seealso cref="UnrealSharp.Utils.Misc.ILoggerObserver" />
public class DebuggerLoggerObserver : ILoggerObserver
{
    /// <summary>
    /// Called when [receive].
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="tag">The tag.</param>
    /// <param name="message">The message.</param>
    public void OnEvent(LoggerLevel level, string? tag, string message)
    {            
        Debug.WriteLine(message);

        var oldColor = Console.ForegroundColor;

        switch (level)
        {
            case LoggerLevel.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                break;
            case LoggerLevel.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message);
                break;
            case LoggerLevel.Verbose:
            case LoggerLevel.Information:
            case LoggerLevel.Total:
            default:
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message);
                break;
        }

        Console.ForegroundColor = oldColor;
    }
}

/// <summary>
/// Class LoggerFileObserver.
/// Implements the <see cref="UnrealSharp.Utils.Misc.ILoggerObserver" />
/// Implements the <see cref="IDisposable" />
/// </summary>
/// <seealso cref="UnrealSharp.Utils.Misc.ILoggerObserver" />
/// <seealso cref="IDisposable" />
public sealed class LoggerFileObserver : ILoggerObserver, IDisposable
{
    /// <summary>
    /// The log stream
    /// </summary>
    private FileStream? _logStream;

    /// <summary>
    /// The file path
    /// </summary>
    public readonly string FilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerFileObserver"/> class.
    /// </summary>
    public LoggerFileObserver() :
        this(GenDefaultLogFileName())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerFileObserver" /> class.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="autoCleanOutdatedLogs">The automatic clean outdated logs.</param>
    public LoggerFileObserver(string path, bool autoCleanOutdatedLogs = true)
    {
        FilePath = path;

        var dir = path.GetDirectoryPath();

        if (!dir.IsDirectoryExists())
        {
            Directory.CreateDirectory(dir);
        }
        else
        {
            try
            {
                if(autoCleanOutdatedLogs)
                {
                    // delete all old files
                    var files = Directory.GetFiles(dir, "*.*");

                    // save 7 days logs...
                    var outDateFlag = DateTime.Now.AddDays(-7);
                    foreach (var f in files)
                    {
                        var fi = new FileInfo(f);

                        if (fi.CreationTime < outDateFlag)
                        {
                            try
                            {
                                fi.Delete();
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                }                    
            }
            catch
            {
                // ignored
            }
        }

        _logStream = new FileStream(
            FilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read
        );
    }

    /// <summary>
    /// Gens the log filename.
    /// </summary>
    /// <returns>System.String.</returns>
    private static string GenDefaultLogFileName()
    {
        var basePath = AppContext.BaseDirectory;
        var logDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(basePath)!, "../../../../../Saved/Logs"));

        var logFilePath = Path.Combine(logDir, basePath + "-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture) + ".log");

        return logFilePath;
    }


    /// <summary>
    /// Called when [receive].
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="tag">The tag.</param>
    /// <param name="message">The message.</param>
    public void OnEvent(LoggerLevel level, string? tag, string message)
    {
        if (_logStream != null && message.IsNotNullOrEmpty())
        {
            lock (_logStream)
            {
                var time = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]", System.Globalization.CultureInfo.InvariantCulture);
                var detailTag = tag.IsNotNullOrEmpty() ? $"[{tag}]" : "";

                var bytes = System.Text.Encoding.UTF8.GetBytes($"{time}{detailTag}{message.Trim()}{Environment.NewLine}");
                _logStream.Write(bytes, 0, bytes.Length);
                _logStream.Flush();
            }
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        if (_logStream != null)
        {
            _logStream.Flush();
            _logStream.Dispose();
            _logStream = null;
        }
    }
}