using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace UnrealSharp.Utils.Misc;

/// <summary>
/// Delegate LoggerEventHandler
/// </summary>
/// <param name="level">The level.</param>
/// <param name="tag">The tag.</param>
/// <param name="message">The message.</param>
public delegate void LoggerEventHandler(LoggerLevel level, string? tag, string message);

/// <summary>
/// Class Logger.
/// </summary>
public static class Logger
{
    /// <summary>
    /// Occurs when [on system log event].
    /// </summary>
    public static event LoggerEventHandler? OnSystemLogEvent;

    /// <summary>
    /// Gets or sets the level.
    /// </summary>
    /// <value>The level.</value>
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public static LoggerLevel Level { get; set; } = LoggerLevel.Information;

    /// <summary>
    /// Gets a value indicating whether this instance is verbose enabled.
    /// </summary>
    /// <value><c>true</c> if this instance is verbose enabled; otherwise, <c>false</c>.</value>
    public static bool IsVerboseEnabled => Level <= LoggerLevel.Verbose;

    /// <summary>
    /// Gets a value indicating whether this instance is common information enabled.
    /// </summary>
    /// <value><c>true</c> if this instance is common information enabled; otherwise, <c>false</c>.</value>
    public static bool IsCommonInformationEnabled => Level <= LoggerLevel.Information;

    /// <summary>
    /// Gets a value indicating whether this instance is warning enabled.
    /// </summary>
    /// <value><c>true</c> if this instance is warning enabled; otherwise, <c>false</c>.</value>
    public static bool IsWarningEnabled => Level <= LoggerLevel.Warning;

    /// <summary>
    /// Gets a value indicating whether this instance is error enabled.
    /// </summary>
    /// <value><c>true</c> if this instance is error enabled; otherwise, <c>false</c>.</value>
    public static bool IsErrorEnabled => Level <= LoggerLevel.Error;

    static Logger()
    {
        try
        {
            var debuggerReceiver = new DebuggerLoggerObserver();

            OnSystemLogEvent += (s, t, e) => 
            { 
                debuggerReceiver.OnEvent(s, t, e);
            };

            if (Environment.CommandLine.Contains("--verbose"))
            {
                Level = LoggerLevel.Verbose;                    
            }

            if(IsVerboseEnabled)
            {
                var builder = new StringBuilder();
                builder.AppendLine("Environment Variables:");

                var caches = new List<DictionaryEntry>();
                caches.AddRange(Environment.GetEnvironmentVariables().Cast<DictionaryEntry>());
                caches.Sort((x, y) => Comparer<string>.Default.Compare(x.Key.ToString(), y.Key.ToString()));

                foreach (var i in caches)
                {
                    builder.AppendLine($"    {i.Key} = {i.Value ?? ""}");
                }

                debuggerReceiver.OnEvent(LoggerLevel.Verbose, "", builder.ToString());
            }
        }
        catch(Exception e)
        {
            LogError(e.Message);
        }
    }

    /// <summary>
    /// Logs the specified level.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="tag">The tag.</param>
    /// <param name="message">The message.</param>
    public static void Log(LoggerLevel level, string? tag, string message)
    {
        if(level >= Level)
        {
            OnSystemLogEvent?.Invoke(level, tag, message);
        }            
    }

    /// <summary>
    /// Logs the error.
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    public static void LogError(string format, params object?[]? args)
    {
        Log(LoggerLevel.Error, null, args is { Length: > 0 } ? string.Format(format, args) : format);
    }

    /// <summary>
    /// Logs the warning.
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    public static void LogWarning(string format, params object?[]? args)
    {
        if(!IsWarningEnabled)
        {
            return;
        }

        Log(LoggerLevel.Warning, null, args is { Length: > 0 } ? string.Format(format, args) : format);
    }

    /// <summary>
    /// Logs the d.
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    [Conditional("DEBUG")]
    public static void LogD(string format, params object?[]? args)
    {
        if (!IsCommonInformationEnabled)
        {
            return;
        }

        Log(LoggerLevel.Information, null, args is { Length: > 0 } ? string.Format(format, args) : format);
    }

    /// <summary>
    /// Logs the specified format.
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    public static void Log(string format, params object?[]? args)
    {
        if(!IsCommonInformationEnabled)
        {
            return;
        }

        Log(LoggerLevel.Information, null, args is { Length: > 0 } ? string.Format(format, args) : format);
    }

    /// <summary>
    /// Logs the verbose.
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    public static void LogVerbose(string format, params object?[]? args)
    {
        if(!IsVerboseEnabled)
        {
            return;
        }

        Log(LoggerLevel.Verbose, null, args is { Length: > 0 } ? string.Format(format, args) : format);
    }

    // <summary>
    /// <summary>
    /// Asserts the specified condition.
    /// </summary>
    /// <param name="condition">if set to <c>true</c> [condition].</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    /// <font color="red">Badly formed XML comment.</font>
    [Conditional("DEBUG")]
    public static void Assert(bool condition, string format, params object?[]? args)
    {
        if (condition)
        {
            return;
        }

        var sb = new StringBuilder();

        if(args is { Length: > 0 })
        {
            sb.Append("Assertion:");
            sb.AppendLine(string.Format(format, args));
            sb.Append(Environment.StackTrace);

            LogError(sb.ToString());
        }
        else
        {
            sb.Append("Assertion:");
            sb.AppendLine(format);
            sb.Append(Environment.StackTrace);

            LogError(sb.ToString());
        }            

        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }
        else
        {
            throw new Exception($"Assertion:{sb}");
        }
    }

    /// <summary>
    /// Asserts the specified b condition.
    /// </summary>
    /// <param name="condition">if set to <c>true</c> [condition].</param>
    [Conditional("DEBUG")]
    public static void Assert(bool condition)
    {
        if (condition)
        {
            return;
        }

        LogError($"Assertion:{Environment.NewLine}{Environment.StackTrace}");

        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }
        else
        {
            throw new Exception("Assertion!!!");
        }
    }

    /// <summary>
    /// Ensures the specified condition.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="condition">if set to <c>true</c> [condition].</param>        
    public static void Ensure<
#if !NETSTANDARD2_1
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        T>(bool condition)
    {
        if(!condition)
        {
            LogError("Ensure Failure<{0}>",typeof(T).Name);

            var exp = Activator.CreateInstance(typeof(T)) as Exception;
            throw exp!;
        }
    }

    /// <summary>
    /// Ensures the specified condition.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="condition">if set to <c>true</c> [condition].</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>        
    public static void Ensure<
#if !NETSTANDARD2_1
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        T>(bool condition, string format, params object?[]? args)
        where T : Exception, new()
    {
        if (!condition)
        {
            if(args is { Length: > 0 })
            {
                var message = string.Format(format, args);

                LogError("Ensure Failure<{1}>:{0}", message, typeof(T).Name);

                var exp = Activator.CreateInstance(typeof(T), message) as Exception;
                throw exp!;
            }
            else
            {
                LogError("Ensure Failure<{1}>:{0}", format, typeof(T).Name);

                var exp = Activator.CreateInstance(typeof(T), format) as Exception;
                throw exp!;
            }
        }
    }

    /// <summary>
    /// Ensures the not null.
    /// </summary>
    /// <typeparam name="TExceptionType">The type of the t exception type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>        
    public static void EnsureNotNull<
#if !NETSTANDARD2_1
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        TExceptionType>([NotNull] object? value, string format, params object?[]? args)             
        where TExceptionType : class, new()
    {
        if(value == null)
        {
            if (args is { Length: > 0 })
            {
                var message = string.Format(format, args);

                LogError("Ensure Failure<{1}>:{0}", message, typeof(TExceptionType).Name);

                var exp = Activator.CreateInstance(typeof(TExceptionType), message) as Exception;
                throw exp!;
            }
            else
            {
                LogError("Ensure Failure<{1}>:{0}", format, typeof(TExceptionType).Name);

                var exp = Activator.CreateInstance(typeof(TExceptionType), format) as Exception;
                throw exp!;
            }
        }
    }

    /// <summary>
    /// Ensures the not null.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    public static void EnsureNotNull([NotNull] object? value, string format, params object?[]? args)
    {
        EnsureNotNull<Exception>(value, format, args);
    }

    /// <summary>
    /// Ensures the not null.
    /// </summary>
    /// <param name="value">The value.</param>
    public static void EnsureNotNull([NotNull] object? value)
    {
        EnsureNotNull<Exception>(value, "condition can not be null!");
    }
}