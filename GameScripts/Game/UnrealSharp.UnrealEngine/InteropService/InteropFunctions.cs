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
using UnrealSharp.UnrealEngine.Main;
using UnrealSharp.Utils.Misc;

namespace UnrealSharp.UnrealEngine.InteropService;

/// <summary>
/// Class InteropFunctions.
/// </summary>
public static unsafe class InteropFunctions
{
    /// <summary>
    /// The native instance
    /// </summary>
    public static readonly IntPtr NativeInstance;

    /// <summary>
    /// static constructor
    /// </summary>
    static InteropFunctions()
    {
        var interopInfo = UnrealSharpEntry.InteropFunctionInfo;
        NativeInstance = new IntPtr(interopInfo.NativeInteropFunctionsPtr);

        Logger.Ensure<Exception>(interopInfo.GetUnrealInteropFunctionPointerFunc != null, "Failed bind Main interop function : GetUnrealInteropFunctionPointer");
                        
        GetUnrealInteropFunctionPointer = (delegate* unmanaged[Cdecl]<IntPtr, string, IntPtr>)interopInfo.GetUnrealInteropFunctionPointerFunc;
        ValidateUnrealSharpBuildInfo = (delegate* unmanaged[Cdecl] < FUnrealSharpBuildInfo *, void>)GetUnrealInteropFunctionPointer(NativeInstance, "ValidateUnrealSharpBuildInfo");

        Logger.Ensure<Exception>(ValidateUnrealSharpBuildInfo != null, "Failed find interop function:ValidateUnrealSharpBuildInfo");
    }

    #region Internal Methods

    /// <summary>
    /// The get function pointer index
    /// </summary>
    public static readonly delegate* unmanaged[Cdecl]<IntPtr, string, IntPtr> GetUnrealInteropFunctionPointer;

    /// <summary>
    /// The validate unreal sharp build information
    /// </summary>
    public static readonly delegate* unmanaged[Cdecl]<FUnrealSharpBuildInfo*, void> ValidateUnrealSharpBuildInfo;
    #endregion

    #region Binding Help Utils
    /// <summary>
    /// Binds the interop function pointers.
    /// </summary>
    /// <param name="type">The type.</param>
    public static void BindInteropFunctionPointers([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields|DynamicallyAccessedMemberTypes.NonPublicFields)]Type type)
    {
        foreach (var field in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
        {
            if (field.FieldType != typeof(IntPtr))
            {
                continue;
            }
            
            var name = field.Name;
            var funcPointer = GetUnrealInteropFunctionPointer(NativeInstance, name);
            Logger.Ensure<Exception>(funcPointer != IntPtr.Zero, "Failed bind interop function {0} in class {1}", name, type.FullName!);
                                                            
            field.SetValue(null, funcPointer);

            Logger.LogD("Bind {0}:{1} at 0x{2:x}", type.FullName!, name, funcPointer);
        }
    }

    /// <summary>
    /// Binds the interop function pointer.
    /// bind single function pointer
    /// </summary>
    /// <param name="pointer">The pointer.</param>
    /// <param name="interopMethodName">Name of the interop method.</param>
    /// <param name="typeName">Name of the type.</param>
    public static void BindInteropFunctionPointer(ref IntPtr pointer, string interopMethodName, string typeName)
    {
        var funcPointer = GetUnrealInteropFunctionPointer(NativeInstance, interopMethodName);
        Logger.Ensure<Exception>(funcPointer != IntPtr.Zero, "Failed bind interop function {0} in class {1}", interopMethodName, typeName);

        pointer = funcPointer;
    }
    #endregion
}