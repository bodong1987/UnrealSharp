﻿/*
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
using UnrealSharp.Utils.CommandLine;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace UnrealSharp.UnrealEngine.Main;

/// <summary>
/// Class UnrealSharpStartOptions.
/// </summary>
public class UnrealSharpStartOptions
{
    /// <summary>
    /// Gets or sets the application command line.
    /// </summary>
    /// <value>The application command line.</value>
    [Option("app")]
    public string? AppCommandLine { get; set; }

    /// <summary>
    /// Gets or sets the project directory.
    /// </summary>
    /// <value>The project directory.</value>
    [Option("projectDir")]
    public string? ProjectDirectory { get; set; }

    /// <summary>
    /// Gets or sets the unreal sharp intermediate directory.
    /// </summary>
    /// <value>The unreal sharp intermediate directory.</value>
    [Option("unrealsharpIntermediateDir")]
    public string? UnrealSharpIntermediateDirectory { get; set; }
}