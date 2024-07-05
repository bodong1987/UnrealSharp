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
using System.Diagnostics.CodeAnalysis;

namespace UnrealSharp.UnrealEngine;

/// <summary>
/// Class ULocalPlayer.
/// Implements the <see cref="UnrealSharp.UnrealEngine.UPlayer" />
/// </summary>
/// <seealso cref="UnrealSharp.UnrealEngine.UPlayer" />
public partial class ULocalPlayer
{
    /// <summary>
    /// Gets the subsystem.
    /// </summary>
    /// <param name="contextObject">The context object.</param>
    /// <param name="class">The class.</param>
    /// <returns>System.Nullable&lt;ULocalPlayerSubsystem&gt;.</returns>
    public static ULocalPlayerSubsystem? GetSubsystem(UObject? contextObject, TSubclassOf<ULocalPlayerSubsystem> @class)
    {
        return USubsystemBlueprintLibrary.GetLocalPlayerSubsystem(contextObject, @class);
    }

    /// <summary>
    /// Gets the subsystem.
    /// </summary>
    /// <param name="class">The class.</param>
    /// <returns>System.Nullable&lt;ULocalPlayerSubsystem&gt;.</returns>
    public ULocalPlayerSubsystem? GetSubsystem(TSubclassOf<ULocalPlayerSubsystem> @class)
    {
        return GetSubsystem(this, @class);
    }

    /// <summary>
    /// Gets the subsystem.
    /// </summary>
    /// <typeparam name="TLocalPlayerSubsystemType">The type of the t local player subsystem type.</typeparam>
    /// <returns>System.Nullable&lt;TLocalPlayerSubsystemType&gt;.</returns>
    public TLocalPlayerSubsystemType? GetSubsystem<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] TLocalPlayerSubsystemType>()
        where TLocalPlayerSubsystemType : ULocalPlayerSubsystem
    {
        var subsystem = GetSubsystem(new TSubclassOf<ULocalPlayerSubsystem>(UClass.GetClassOf<TLocalPlayerSubsystemType>()));

        return subsystem as TLocalPlayerSubsystemType;
    }
}