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
using System.Runtime.CompilerServices;
using UnrealSharp.Utils.Misc;

namespace UnrealSharp.UnrealEngine.InteropService;

/// <summary>
/// Class InteropUtils.
/// </summary>
public static partial class InteropUtils
{
    #region Shared Constants Name
    /// <summary>
    /// The unreal sharp name
    /// </summary>
    public const string UnrealSharpName = UObject.UnrealSharpName;
    #endregion

    #region Get All Types

    #region Boolean
    /// <summary>
    /// Gets the boolean.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="fieldMask">The field mask.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetBoolean(
        IntPtr address, 
        int offset,
        byte fieldMask = 0xFF
    )
    {
        Logger.Assert(address != IntPtr.Zero);

        unsafe
        {
            var ptr = (byte*)IntPtr.Add(address, offset);

            return 0 != (*ptr & fieldMask);
        }
    }

    /// <summary>
    /// Gets the boolean.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetBoolean(IntPtr address, int offset, IntPtr propertyPtr)
    {
        Logger.Assert(address != IntPtr.Zero);

        return PropertyInteropUtils.GetBoolPropertyValue(propertyPtr, IntPtr.Add(address, offset));
    }
    #endregion

    #region Numeric
    /// <summary>
    /// Gets the numeric.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>T.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetNumeric<T>(
        IntPtr address, 
        int offset
    )
        where T : struct
#if NET7_0_OR_GREATER
        , System.Numerics.INumber<T>
#endif
    {
        Logger.Assert(address != IntPtr.Zero);

        unsafe
        {
            // T is value type, so this is safe                
#pragma warning disable CS8500 // This gets the address of a managed type, gets its size, or declares a pointer to it
            var ptr = (T*)IntPtr.Add(address, offset);
#pragma warning restore CS8500 // This gets the address of a managed type, gets its size, or declares a pointer to it                

            return *ptr;
        }
    }
    #endregion

    #region Enum
    /// <summary>
    /// Gets the enum.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="enumSize">Size of the enum.</param>
    /// <returns>T.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetEnum<T>(
        IntPtr address, 
        int offset, 
        int enumSize
    ) where T : unmanaged, Enum
    {
        Logger.Assert(address != IntPtr.Zero);
          
        unsafe
        {
            if(enumSize == sizeof(T))
            {
                var ptr = (T*)IntPtr.Add(address, offset);

                return *ptr;
            }

            switch (enumSize)
            {
                case sizeof(byte):
                {
                    var value = *(byte*)IntPtr.Add(address, offset);

                    return (T)Enum.ToObject(typeof(T), value);
                }
                case sizeof(int):
                {
                    var value = *(int*)IntPtr.Add(address, offset);

                    return (T)Enum.ToObject(typeof(T), value);
                }
                case sizeof(long):
                {
                    var value = *(long*)IntPtr.Add(address, offset);

                    return (T)Enum.ToObject(typeof(T), value);
                }
                case sizeof(short):
                {
                    var value = *(short*)IntPtr.Add(address, offset);

                    return (T)Enum.ToObject(typeof(T), value);
                }
            }

            return default;
        }
    }
    #endregion

    #region String
    /// <summary>
    /// Gets the string.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>System.Nullable&lt;System.String&gt;.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? GetString(
        IntPtr address,
        int offset
    )
    {
        Logger.Assert(address != IntPtr.Zero);

        var value = address != IntPtr.Zero ? StringInteropUtils.GetStringFromUnrealString(IntPtr.Add(address, offset)) : null;

        return value;
    }
    #endregion

    #region StringView
    /// <summary>
    /// Gets the string view.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>FStringView.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FStringView GetStringView(
        IntPtr address, 
        int offset
    )
    {
        Logger.Assert(address != IntPtr.Zero);

        return new FStringView(IntPtr.Add(address, offset));
    }
    #endregion

    #region Name
    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>FName.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FName GetName(
        IntPtr address, 
        int offset
    )
    {
        Logger.Assert(address != IntPtr.Zero);

        return FName.FromNative(address, offset);
    }
    #endregion

    #region Text
    /// <summary>
    /// Gets the text.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>FText.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FText GetText(
        IntPtr address,
        int offset
    )
    {
        Logger.Assert(address != IntPtr.Zero);

        return FText.FromNative(address, offset);
    }
    #endregion

    #region Object
    /// <summary>
    /// Gets the object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="objectPointer">The object pointer.</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public static T? GetObjectByNativePointer<T>(IntPtr objectPointer) where T : class, IUObjectInterface
    {
        if (objectPointer == IntPtr.Zero)
        {
            return null;
        }

        var result = ObjectInteropUtils.GetCSharpObjectOfUnrealObject(objectPointer);

        return result as T;
    }

    /// <summary>
    /// Gets the object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public static T? GetObject<T>(
        IntPtr address, 
        int offset
    ) where T : class, IUObjectInterface
    {
        Logger.Assert(address != IntPtr.Zero);

        if (address == IntPtr.Zero)
        {
            return null;
        }

        IntPtr objectPointer;

        unsafe
        {
            var ptr = (IntPtr*)IntPtr.Add(address, offset);
            objectPointer = *ptr;
        }

        return GetObjectByNativePointer<T>(objectPointer);
    }
    #endregion

    #region Class
    /// <summary>
    /// Gets the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>System.Nullable&lt;UClass&gt;.</returns>
    public static UClass? GetClass(
        IntPtr address, 
        int offset
    )
    {
        Logger.Assert(address != IntPtr.Zero);

        if (address == IntPtr.Zero)
        {
            return null;
        }

        unsafe
        {
            var ptr = (IntPtr*)IntPtr.Add(address, offset);

            return *ptr != IntPtr.Zero ? new UClass(*ptr) : null;
        }
    }

    /// <summary>
    /// Gets the class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>TSubclassOf&lt;T&gt;.</returns>
    public static TSubclassOf<T> GetClass<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(
        IntPtr address, 
        int offset
    )
        where T : class, IUObjectInterface
    {
        var classPtr = GetClass( 
            address, 
            offset
        );

        return new TSubclassOf<T>(classPtr);
    }
    #endregion

    #region Soft Object
    /// <summary>
    /// Gets the soft object PTR.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>System.Nullable&lt;TSoftObjectPtr&lt;T&gt;&gt;.</returns>
    public static TSoftObjectPtr<T>? GetSoftObjectPtr<T>(
        IntPtr address,
        int offset
    ) where T : UObject
    {
        Logger.Assert(address != IntPtr.Zero);

        if (address == IntPtr.Zero)
        {
            return null;
        }

        IntPtr objectPointer;

        unsafe
        {
            var ptr = (IntPtr*)IntPtr.Add(address, offset);
            objectPointer = *ptr;
        }

        return objectPointer == IntPtr.Zero ? null : new TSoftObjectPtr<T>(objectPointer);
    }
    #endregion

    #region Soft Class
    /// <summary>
    /// Gets the soft class PTR.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>System.Nullable&lt;TSoftClassPtr&lt;T&gt;&gt;.</returns>
    public static TSoftClassPtr<T>? GetSoftClassPtr<T>(
        IntPtr address,
        int offset
    ) where T : UObject
    {
        Logger.Assert(address != IntPtr.Zero);

        if (address == IntPtr.Zero)
        {
            return null;
        }

        IntPtr objectPointer;

        unsafe
        {
            var ptr = (IntPtr*)IntPtr.Add(address, offset);
            objectPointer = *ptr;
        }

        return objectPointer == IntPtr.Zero ? null : new TSoftClassPtr<T>(objectPointer);
    }
    #endregion

    #region Struct
    /// <summary>
    /// Gets the structure unsafe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>T.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetStructUnsafe<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(IntPtr address, int offset) where T : struct
    {
        Logger.Assert(address != IntPtr.Zero);

        MetaInteropUtils.CheckStructFastAccessSafety<T>();

        unsafe
        {
            // T must be struct value, so it is safe here
#pragma warning disable CS8500 // This gets the address of a managed type, gets its size, or declares a pointer to it
            var ptr = (T*)IntPtr.Add(address, offset);
#pragma warning restore CS8500 // This gets the address of a managed type, gets its size, or declares a pointer to it

            return *ptr;
        }
    }
    #endregion

    #endregion

    #region Set All Types

    #region Boolean
    /// <summary>
    /// Sets the boolean.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="fieldMask">The field mask.</param>
    /// <param name="value">if set to <c>true</c> [value].</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBoolean(
        IntPtr address, 
        int offset,
        byte fieldMask,
        bool value          
    )
    {
        Logger.Assert(address != IntPtr.Zero);

        unsafe
        {
            var ptr = (byte*)IntPtr.Add(address, offset);

            if(value)
            {
                *ptr = (byte)(*ptr | fieldMask);
            }
            else
            {
                // ReSharper disable once ArrangeRedundantParentheses
                *ptr = (byte)(*ptr & (~fieldMask));
            }
        }
    }

    /// <summary>
    /// Sets the boolean.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="value">if set to <c>true</c> [value].</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBoolean(
        IntPtr address, 
        int offset,
        bool value
    )
    {
        Logger.Assert(address != IntPtr.Zero);

        unsafe
        {
            var ptr = (bool*)IntPtr.Add(address, offset);

            *ptr = value;
        }
    }

    /// <summary>
    /// Sets the boolean.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="value">if set to <c>true</c> [value].</param>
    /// <param name="boolPropertyPtr">The bool property PTR.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBoolean(IntPtr address, int offset, bool value, IntPtr boolPropertyPtr)
    {
        Logger.Assert(address != IntPtr.Zero);

        PropertyInteropUtils.SetBoolPropertyValue(boolPropertyPtr, IntPtr.Add(address, offset), value);
    }
    #endregion

    #region Numeric
    /// <summary>
    /// Sets the numeric.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="value">The value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetNumeric<T>(
        IntPtr address, 
        int offset,
        T value
    ) where T : struct
#if NET7_0_OR_GREATER
        , System.Numerics.INumber<T>
#endif
    {
        Logger.Assert(address != IntPtr.Zero);

        unsafe
        {
            // T is value type, so this is safe
#pragma warning disable CS8500 // This gets the address of a managed type, gets its size, or declares a pointer to it
            var ptr = (T*)IntPtr.Add(address, offset);
#pragma warning restore CS8500 // This gets the address of a managed type, gets its size, or declares a pointer to it

            *ptr = value;
        }
    }
    #endregion

    #region Enum
    /// <summary>
    /// Sets the enum.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="enumSize">Size of the enum.</param>
    /// <param name="value">The value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetEnum<T>(
        IntPtr address, 
        int offset,
        int enumSize,
        T value
    ) where T : unmanaged, Enum
    {
        Logger.Assert(address != IntPtr.Zero);

        unsafe
        {
            if (enumSize == sizeof(T))
            {
                *(T*)IntPtr.Add(address, offset) = value;
            }
            else switch (enumSize)
            {
                case sizeof(byte):
                    *(byte*)IntPtr.Add(address, offset) = Convert.ToByte(value);
                    break;
                case sizeof(int):
                    *(int*)IntPtr.Add(address, offset) = Convert.ToInt32(value);
                    break;
                case sizeof(long):
                    *(long*)IntPtr.Add(address, offset) = Convert.ToInt64(value);
                    break;
                case sizeof(short):
                    *(short*)IntPtr.Add(address, offset) = Convert.ToInt16(value);
                    break;
            }
        }
    }
    #endregion

    #region String
    /// <summary>
    /// Sets the string.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="value">The value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetString(
        IntPtr address, 
        int offset,
        string? value
    )
    {
        Logger.Assert(address != IntPtr.Zero);

        StringInteropUtils.SetUnrealString(IntPtr.Add(address, offset), value);
    }
    #endregion

    #region StringView
    /// <summary>
    /// Sets the string view.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="view">The view.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetStringView(
        IntPtr address, 
        int offset,
        FStringView view
    )
    {
        Logger.Assert(address != IntPtr.Zero);

        FStringView.ToNative(address, offset, ref view);
    }
    #endregion

    #region Name
    /// <summary>
    /// Sets the name.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="name">The name.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetName(
        IntPtr address, 
        int offset, 
        FName name
    )
    {
        Logger.Assert(address != IntPtr.Zero);

        FName.ToNative(address, offset, ref name);
    }
    #endregion

    #region Text
    /// <summary>
    /// Sets the text.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="text">The text.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetText(
        IntPtr address, 
        int offset,
        FText text
    )
    {
        Logger.Assert(address != IntPtr.Zero);

        FText.ToNative(address, offset, ref text);
    }
    #endregion

    #region Object
    /// <summary>
    /// Sets the object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="value">The value.</param>
    public static void SetObject<T>(
        IntPtr address, 
        int offset,
        T? value
    ) where T : IUObjectInterface
    {
        Logger.Assert(address != IntPtr.Zero);

        unsafe
        {
            *(IntPtr*)IntPtr.Add(address, offset) = value != null ? value.GetNativePtr() : IntPtr.Zero;
        }
    }
    #endregion

    #region Class
    /// <summary>
    /// Sets the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="value">The value.</param>
    public static void SetClass(
        IntPtr address, 
        int offset,
        UClass? value
    )
    {
        Logger.Assert(address != IntPtr.Zero);

        unsafe
        {
            var ptr = (IntPtr*)IntPtr.Add(address, offset);

            *ptr = value != null ? value.GetNativePtr() : IntPtr.Zero;
        }
    }

    /// <summary>
    /// Sets the class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="value">The value.</param>
    public static void SetClass<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(
        IntPtr address, 
        int offset,
        TSubclassOf<T> value
    )
        where T : class, IUObjectInterface
    {
        unsafe
        {
            var ptr = (IntPtr*)IntPtr.Add(address, offset);

            *ptr =  value.GetNativePtr();
        }
    }
    #endregion

    #region Soft Object
    /// <summary>
    /// Sets the soft object PTR.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="value">The value.</param>
    public static void SetSoftObjectPtr<T>(
        IntPtr address,
        int offset,
        TSoftObjectPtr<T>? value
    ) where T : UObject
    {
        Logger.Assert(address != IntPtr.Zero);

        IntPtr objectPointer;

        unsafe
        {
            var ptr = (IntPtr*)IntPtr.Add(address, offset);
            objectPointer = *ptr;
        }

        if (objectPointer == IntPtr.Zero)
        {
            return;
        }

        SoftObjectPtrInteropUtils.CopySoftObjectPtr(objectPointer, value?.GetNativePtr() ?? IntPtr.Zero);
    }
    #endregion

    #region Soft Class
    /// <summary>
    /// Sets the soft class PTR.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="value">The value.</param>
    public static void SetSoftClassPtr<T>(
        IntPtr address,
        int offset,
        TSoftClassPtr<T>? value
    ) where T : UObject
    {
        Logger.Assert(address != IntPtr.Zero);

        IntPtr objectPointer;

        unsafe
        {
            var ptr = (IntPtr*)IntPtr.Add(address, offset);
            objectPointer = *ptr;
        }

        if (objectPointer == IntPtr.Zero)
        {
            return;
        }

        // The C++ version of TSoftClassPtr is composed of TSoftObjectPtr.
        // The memory layout and size of the two are exactly the same, so it is safe to directly use the interface to copy TSoftObjectPtr.
        // The C++ side will also have corresponding static assertions, which will not compile when the requirements are not met.
        SoftObjectPtrInteropUtils.CopySoftObjectPtr(objectPointer, value?.GetNativePtr() ?? IntPtr.Zero);
    }
    #endregion

    #region Struct
    /// <summary>
    /// Sets the structure unsafe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="value">The value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetStructUnsafe<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(IntPtr address, int offset, ref T value) where T : struct
    {
        Logger.Assert(address != IntPtr.Zero);
            
        MetaInteropUtils.CheckStructFastAccessSafety<T>();

        unsafe
        {
            // T must be struct value, so it is safe here
#pragma warning disable CS8500 // This gets the address of a managed type, gets its size, or declares a pointer to it
            var ptr = (T*)IntPtr.Add(address, offset);
#pragma warning restore CS8500 // This gets the address of a managed type, gets its size, or declares a pointer to it

            *ptr = value;
        }
    }
    #endregion

    #endregion
}