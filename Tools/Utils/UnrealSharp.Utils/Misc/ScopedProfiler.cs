using System.Diagnostics;

namespace UnrealSharp.Utils.Misc
{
    /// <summary>
    /// Class ScopedProfiler.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class ScopedProfiler : IDisposable
    {
        Stopwatch watch = new Stopwatch();
        string Tag;
        /// <summary>
        /// Initializes a new instance of the <see cref="ScopedProfiler"/> class.
        /// </summary>
        public ScopedProfiler(string tag)
        {
            Tag = tag;
            watch.Start();
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Logger.Log("{0} Done, {1}", Tag, watch.Elapsed);

            watch.Stop();
        }

        /// <summary>
        /// Logs the specified FMT.
        /// </summary>
        /// <param name="fmt">The FMT.</param>
        /// <param name="args">The arguments.</param>
        public void Log(string fmt, params object[] args)
        {
            string text;

            if (args != null)
            {
                text = string.Format(fmt, args);
            }
            else
            {
                text = fmt;
            }

            Logger.Log("{0} {1} {2}", Tag, text, watch.Elapsed);
        }
    }
}
