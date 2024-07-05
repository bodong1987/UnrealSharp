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
// ReSharper disable MemberHidesStaticFromOuterClass
namespace UnrealSharp.UnrealEngine.InteropService;

/// <summary>
/// Class PropertyInteropUtils.
/// </summary>
public static unsafe class PropertyInteropUtils
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
        /// <summary>
        /// The get property offset
        /// </summary>
        public static readonly IntPtr GetPropertyOffset;
        /// <summary>
        /// The get property size
        /// </summary>
        public static readonly IntPtr GetPropertySize;
        /// <summary>
        /// The initialize property data
        /// </summary>
        public static readonly IntPtr InitializePropertyData;
        /// <summary>
        /// The un initialize property data
        /// </summary>
        public static readonly IntPtr UnInitializePropertyData;
        /// <summary>
        /// The get property cast flags
        /// </summary>
        public static readonly IntPtr GetPropertyCastFlags;
        /// <summary>
        /// The get property inner field
        /// </summary>
        public static readonly IntPtr GetPropertyInnerField;

        /// <summary>
        /// The set property value in container
        /// </summary>
        public static readonly IntPtr SetPropertyValueInContainer;

        /// <summary>
        /// The get property value in container
        /// </summary>
        public static readonly IntPtr GetPropertyValueInContainer;

        /// <summary>
        /// The set bool property value
        /// compatible with old version engine
        /// </summary>
        public static readonly IntPtr SetBoolPropertyValue;

        /// <summary>
        /// The get bool property value
        /// compatible with old version engine
        /// </summary>
        public static readonly IntPtr GetBoolPropertyValue;
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
    /// Gets the property offset.
    /// </summary>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <returns>int.</returns>
    public static int GetPropertyOffset(IntPtr propertyPtr)
    {
        return ((delegate* unmanaged[Cdecl]<IntPtr, int>)InteropFunctionPointers.GetPropertyOffset)(propertyPtr);
    }

    /// <summary>
    /// Gets the size of the property.
    /// </summary>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <returns>int.</returns>
    public static int GetPropertySize(IntPtr propertyPtr)
    {
        return ((delegate* unmanaged[Cdecl]<IntPtr, int>)InteropFunctionPointers.GetPropertySize)(propertyPtr);
    }

    /// <summary>
    /// Initializes the property data.
    /// </summary>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="memPtr">The memory PTR.</param>
    public static void InitializePropertyData(IntPtr propertyPtr, IntPtr memPtr)
    {
        ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void>)InteropFunctionPointers.InitializePropertyData)(propertyPtr, memPtr);
    }

    /// <summary>
    /// uninitialize property data.
    /// </summary>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="memPtr">The memory PTR.</param>
    public static void UnInitializePropertyData(IntPtr propertyPtr, IntPtr memPtr)
    {
        ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void>)InteropFunctionPointers.UnInitializePropertyData)(propertyPtr, memPtr);
    }

    /// <summary>
    /// Gets the property cast flags.
    /// used to fast check FProperty type
    /// </summary>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <returns>ulong.</returns>
    public static ulong GetPropertyCastFlags(IntPtr propertyPtr)
    {
        return ((delegate* unmanaged[Cdecl]<IntPtr, ulong>)InteropFunctionPointers.GetPropertyCastFlags)(propertyPtr);
    }

    /// <summary>
    /// Gets the property inner field.
    /// if FProperty is FObjectProperty/FStructProperty/FEnumProperty, get the UField of this property
    /// </summary>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <returns>nint.</returns>
    public static IntPtr GetPropertyInnerField(IntPtr propertyPtr)
    {
        return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetPropertyInnerField)(propertyPtr);
    }

    /// <summary>
    /// Sets the property value in container.
    /// </summary>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="containerPtr">The container PTR.</param>
    /// <param name="valuePtr">The value PTR.</param>
    public static void SetPropertyValueInContainer(IntPtr propertyPtr, IntPtr containerPtr, IntPtr valuePtr)
    {
        ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void>)InteropFunctionPointers.SetPropertyValueInContainer)(propertyPtr, containerPtr, valuePtr);
    }

    /// <summary>
    /// Gets the property value in container.
    /// </summary>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="containerPtr">The container PTR.</param>
    /// <param name="outValuePtr">The out value PTR.</param>
    public static void GetPropertyValueInContainer(IntPtr propertyPtr, IntPtr containerPtr, IntPtr outValuePtr)
    {
        ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void>)InteropFunctionPointers.GetPropertyValueInContainer)(propertyPtr, containerPtr, outValuePtr);
    }

    /// <summary>
    /// Sets the bool property value.
    /// </summary>
    /// <param name="boolPropertyPtr">The bool property PTR.</param>
    /// <param name="targetAddress">The target address.</param>
    /// <param name="value">The value.</param>
    public static void SetBoolPropertyValue(IntPtr boolPropertyPtr, IntPtr targetAddress, bool value)
    {
        ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, bool, void>)InteropFunctionPointers.SetBoolPropertyValue)(boolPropertyPtr, targetAddress, value);
    }

    /// <summary>
    /// Gets the bool property value.
    /// </summary>
    /// <param name="boolPropertyPtr">The bool property PTR.</param>
    /// <param name="targetAddress">The target address.</param>
    /// <returns>bool.</returns>
    public static bool GetBoolPropertyValue(IntPtr boolPropertyPtr, IntPtr targetAddress)
    {
        return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, bool>)InteropFunctionPointers.GetBoolPropertyValue)(boolPropertyPtr, targetAddress);
    }
}

/// <summary>
/// Class ScopedPropertyTempVariable.
/// Mainly used in containers to dynamically build temporary areas for objects and manage related memory
/// Implements the <see cref="System.IDisposable" />
/// </summary>
/// <seealso cref="System.IDisposable" />
public sealed class ScopedPropertyTempVariable : IDisposable
{
    /// <summary>
    /// The property PTR
    /// </summary>
    public readonly IntPtr PropertyPtr;
    /// <summary>
    /// The variable PTR
    /// </summary>
    public readonly IntPtr VariablePtr;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScopedPropertyTempVariable"/> class.
    /// </summary>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="variablePtr">The variable PTR.</param>
    public ScopedPropertyTempVariable(IntPtr propertyPtr, IntPtr variablePtr)
    {
        PropertyPtr = propertyPtr;
        VariablePtr = variablePtr;

        PropertyInteropUtils.InitializePropertyData(PropertyPtr, variablePtr);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScopedPropertyTempVariable"/> class.
    /// </summary>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="bytes">The bytes.</param>
    public unsafe ScopedPropertyTempVariable(IntPtr propertyPtr, byte* bytes) :
        this(propertyPtr, new IntPtr(bytes))
    {            
    }

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
        PropertyInteropUtils.UnInitializePropertyData(PropertyPtr, VariablePtr);
    }
}