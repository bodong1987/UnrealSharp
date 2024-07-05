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
namespace UnrealSharp.UnrealEngine;

/// <summary>
/// Class IEnhancedInputSubsystemInterfaceExtensions.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IEnhancedInputSubsystemInterfaceExtensions
{
    /// <summary>
    /// Adds the mapping context.
    /// </summary>
    /// <param name="interface">The interface.</param>
    /// <param name="mappingContext">The mapping context.</param>
    /// <param name="priority">The priority.</param>
    public static void AddMappingContext(this IEnhancedInputSubsystemInterface @interface, UInputMappingContext? mappingContext, int priority = 0)
    {
        var options = new FModifyContextOptions();

        @interface.AddMappingContext(mappingContext, priority, ref options);
    }
}