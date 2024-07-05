using System.Diagnostics;

namespace UnrealSharp.Utils.Misc;

/// <summary>
/// Class ScopedProfiler.
/// Implements the <see cref="System.IDisposable" />
/// </summary>
/// <seealso cref="System.IDisposable" />
public sealed class ScopedProfiler : IDisposable
{
    private readonly Stopwatch _watch = new();
    private readonly string _tag;
        
    /// <summary>
    /// Initializes a new instance of the <see cref="ScopedProfiler"/> class.
    /// </summary>
    public ScopedProfiler(string tag)
    {
        _tag = tag;
        _watch.Start();
    }

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
        Logger.Log("{0} Done, {1}", _tag, _watch.Elapsed);

        _watch.Stop();
    }

    /// <summary>
    /// Logs the specified FMT.
    /// </summary>
    /// <param name="fmt">The FMT.</param>
    /// <param name="args">The arguments.</param>
    public void Log(string fmt, params object?[]? args)
    {
        var text = args != null ? string.Format(fmt, args) : fmt;

        Logger.Log("{0} {1} {2}", _tag, text, _watch.Elapsed);
    }
}