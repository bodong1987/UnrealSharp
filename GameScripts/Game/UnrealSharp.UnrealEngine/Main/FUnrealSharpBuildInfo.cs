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

using System.Runtime.InteropServices;

namespace UnrealSharp.UnrealEngine.Main;

/// <summary>
/// Enum EUnrealSharpPlatform
/// please sync with C++
/// </summary>
public enum EUnrealSharpPlatform : byte
{
    /// <summary>
    /// The windows
    /// </summary>
    Windows,
    /// <summary>
    /// The mac
    /// </summary>
    Mac,
    /// <summary>
    /// The linux
    /// </summary>
    Linux,
    /// <summary>
    /// The ios
    /// </summary>
    // ReSharper disable once InconsistentNaming
    IOS,
    /// <summary>
    /// The android
    /// </summary>
    Android
}

/// <summary>
/// Enum EUnrealSharpBuildConfiguration
/// SyncWith C++
/// </summary>
public enum EUnrealSharpBuildConfiguration : byte
{
    /// <summary>
    /// The debug
    /// </summary>
    Debug,
    /// <summary>
    /// The release
    /// </summary>
    Release
}

/// <summary>
/// Struct FUnrealSharpBuildInfo
/// Sync with C++
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct FUnrealSharpBuildInfo
{
    /// <summary>
    /// The platform
    /// </summary>
    public EUnrealSharpPlatform Platform;
    /// <summary>
    /// The configuration
    /// </summary>
    public EUnrealSharpBuildConfiguration Configuration;
    /// <summary>
    /// The b with editor
    /// </summary>
    public bool bWithEditor;

    /// <summary>
    /// Gets this instance.
    /// </summary>
    /// <returns>FUnrealSharpBuildInfo.</returns>
    public static FUnrealSharpBuildInfo Get()
    {
        var result = new FUnrealSharpBuildInfo();

#if WITH_EDITOR
        result.bWithEditor = true;
#else
            result.bWithEditor = false;
#endif

#if PLATFORM_WINDOWS
        result.Platform = EUnrealSharpPlatform.Windows;
#elif PLATFORM_MAC
            result.Platform = EUnrealSharpPlatform.Mac;
#elif PLATFORM_LINUX
            result.Platform = EUnrealSharpPlatform.Linux;
#elif PLATFORM_IOS
            result.Platform = EUnrealSharpPlatform.IOS;
#elif PLATFORM_ANDROID
            result.Platform = EUnrealSharpPlatform.Android;
#else
#error "Unsupported platform"
#endif

#if DEBUG
        result.Configuration = EUnrealSharpBuildConfiguration.Debug;
#elif NDEBUG
            result.Configuration = EUnrealSharpBuildConfiguration.Release;
#else
#error "You should add DEBUG or NDEBUG for your build configuration."
#endif

        return result;
    }
}