/*
    MIT License

    Copyright (c) 2024 UnrealSharp

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

    Project URL: https://github.com/bodong1987/UnrealSharp
*/
using UnrealSharp.Utils.Misc;

namespace UnrealSharp.UnrealEngine.Main;

/// <summary>
/// Class UnrealLoggerObserver.
/// Implements the <see cref="ILoggerObserver" />
/// </summary>
/// <seealso cref="ILoggerObserver" />
internal class UnrealLoggerObserver : ILoggerObserver
{
    /// <summary>
    /// Called when [receive].
    /// May called from any thread...
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="tag">The tag.</param>
    /// <param name="message">The message.</param>
    public void OnEvent(LoggerLevel level, string? tag, string message)
    {
        UnrealSharpEntry.LogMessage(level, message);
    }
}