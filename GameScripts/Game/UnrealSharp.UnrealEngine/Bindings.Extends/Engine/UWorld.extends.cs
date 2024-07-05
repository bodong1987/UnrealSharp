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
using UnrealSharp.UnrealEngine.InteropService;

namespace UnrealSharp.UnrealEngine;

partial class UWorld
{
    /// <summary>
    /// Spawns the actor.
    /// </summary>
    /// <param name="class">The class.</param>
    /// <param name="transform">The transform.</param>
    /// <returns>System.Nullable&lt;AActor&gt;.</returns>
    public AActor? SpawnActor(UClass? @class, ref FTransform transform)
    {
        return ActorInteropUtils.SpawnActor(this, @class, ref transform);
    }

    /// <summary>
    /// Spawns the actor.
    /// </summary>
    /// <param name="class">The class.</param>
    /// <param name="location">The location.</param>
    /// <param name="rotation">The rotation.</param>
    /// <returns>System.Nullable&lt;AActor&gt;.</returns>
    public AActor? SpawnActor(UClass? @class, FVector location, FRotator rotation)
    {
        return ActorInteropUtils.SpawnActor(this, @class, location, rotation);
    }

    /// <summary>
    /// Spawns the actor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transform">The transform.</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public T? SpawnActor<T>(ref FTransform transform) where T : AActor
    {
        var result = SpawnActor(UClass.GetClassOf<T>(), ref transform);

        return result as T;
    }

    /// <summary>
    /// Spawns the actor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="location">The location.</param>
    /// <param name="rotation">The rotation.</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public T? SpawnActor<T>(FVector location, FRotator rotation) where T : AActor
    {
        var result = SpawnActor(UClass.GetClassOf<T>(), location, rotation);

        return result as T;
    }
}