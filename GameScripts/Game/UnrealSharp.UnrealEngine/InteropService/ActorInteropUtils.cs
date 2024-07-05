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
// ReSharper disable MemberHidesStaticFromOuterClass

namespace UnrealSharp.UnrealEngine.InteropService;

/// <summary>
/// Class ActorInteropUtils.
/// </summary>
public static unsafe class ActorInteropUtils
{
    #region Interop Function Pointers
        
    /// <summary>
    /// Class InteropFunctionPointers
    /// Since mono does not support setting delegate* unmanaged type fields directly through reflection,
    /// Therefore we cannot directly declare delegate* unmanaged fields and set them through reflection
    /// So we use this method to set it indirectly, first save the external function pointer to these IntPtr,
    /// and then solve it through forced type conversion when calling.Although this is a bit inconvenient,
    /// there is currently no other way unless Mono supports it in the future.
    /// ReSharper disable once CommentTypo
    /// @reference check here: https://github.com/dotnet/runtime/blob/main/src/mono/mono/metadata/icall.c#L2134  ves_icall_RuntimeFieldInfo_SetValueInternal
    /// </summary>
    private static class InteropFunctionPointers
    {
#pragma warning disable CS0649 // The compiler detected an uninitialized private or internal field declaration that is never assigned a value. [We use reflection to bind all fields of this class]            
        public static readonly IntPtr GetActorWorld;            
        public static readonly IntPtr GetActorGameInstance;            
        public static readonly IntPtr SpawnActorByTransform;
        public static readonly IntPtr SpawnActor;
#pragma warning restore CS0649

        /// <summary>
        /// static constructor
        /// </summary>
        static InteropFunctionPointers()
        {
            InteropFunctions.BindInteropFunctionPointers(typeof(InteropFunctionPointers));
        }
    }
    #endregion

    /// <summary>
    /// Gets the world.
    /// </summary>
    /// <param name="actor">The actor.</param>
    /// <returns>UnrealSharp.UnrealEngine.UWorld?.</returns>
    public static UWorld? GetWorld(AActor actor)
    {
        if(!actor.IsBindingToUnreal)
        {
            return null;
        }

        var value = ((delegate* unmanaged[Cdecl]<IntPtr, FCSharpObjectMarshalValue>)InteropFunctionPointers.GetActorWorld)(actor.GetNativePtr());

        return ObjectInteropUtils.MarshalObject<UWorld>(value);
    }

    /// <summary>
    /// Gets the game instance.
    /// </summary>
    /// <param name="actor">The actor.</param>
    /// <returns>UnrealSharp.UnrealEngine.UGameInstance?.</returns>
    public static UGameInstance? GetGameInstance(AActor actor)
    {
        if (!actor.IsBindingToUnreal)
        {
            return null;
        }

        var value = ((delegate* unmanaged[Cdecl]<IntPtr, FCSharpObjectMarshalValue>)InteropFunctionPointers.GetActorGameInstance)(actor.GetNativePtr());

        return ObjectInteropUtils.MarshalObject<UGameInstance>(value);
    }

    /// <summary>
    /// Spawns the actor.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="class">The class.</param>
    /// <param name="transform">The transform.</param>
    /// <returns>UnrealSharp.UnrealEngine.AActor?.</returns>
    public static AActor? SpawnActor(UWorld world, UClass? @class, ref FTransform transform)
    {
        if(!world.IsBindingToUnreal)
        {
            return null;
        }

        Logger.EnsureNotNull(@class);

        var stackTransform = transform;

        var value = ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, FTransform*, int, FCSharpObjectMarshalValue>)InteropFunctionPointers.SpawnActorByTransform)(world.GetNativePtr(), @class.GetNativePtr(), &stackTransform, sizeof(FTransform));

        return ObjectInteropUtils.MarshalObject<AActor>(value);
    }

    /// <summary>
    /// Spawns the actor.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="class">The class.</param>
    /// <param name="location">The location.</param>
    /// <param name="rotation">The rotation.</param>
    /// <returns>UnrealSharp.UnrealEngine.AActor?.</returns>
    public static AActor? SpawnActor(UWorld world, UClass? @class, FVector location, FRotator rotation)
    {
        if (!world.IsBindingToUnreal)
        {
            return null;
        }

        Logger.EnsureNotNull(@class);

        var localLocation = location;
        var localRotation = rotation;

        var value = ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, FVector*, FRotator*, FCSharpObjectMarshalValue>)InteropFunctionPointers.SpawnActorByTransform)(world.GetNativePtr(), @class.GetNativePtr(), &localLocation, &localRotation);

        return ObjectInteropUtils.MarshalObject<AActor>(value);
    }
}