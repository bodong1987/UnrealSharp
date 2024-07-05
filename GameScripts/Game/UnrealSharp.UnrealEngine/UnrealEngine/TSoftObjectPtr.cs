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
using UnrealSharp.Utils.Misc;

namespace UnrealSharp.UnrealEngine;

#region Interface
/// <summary>
/// Interface ISoftObjectPtr
/// </summary>
public interface ISoftObjectPtr
{
    /// <summary>
    /// Resets from.
    /// </summary>
    /// <param name="other">The other.</param>
    void ResetFrom(ISoftObjectPtr other);

    /// <summary>
    /// Writes to.
    /// </summary>
    /// <param name="addressOfNativeSoftObjectPtr">The address of native soft object PTR.</param>
    void WriteTo(IntPtr addressOfNativeSoftObjectPtr);
}

/// <summary>
/// Interface ISoftClassPtr
/// </summary>
public interface ISoftClassPtr
{
    /// <summary>
    /// Resets from.
    /// </summary>
    /// <param name="other">The other.</param>
    void ResetFrom(ISoftClassPtr other);

    /// <summary>
    /// Writes to.
    /// </summary>
    /// <param name="addressOfNativeSoftClassPtr">The address of native soft class PTR.</param>
    void WriteTo(IntPtr addressOfNativeSoftClassPtr);
}

#endregion

#region TSoftObjectPtr
/// <summary>
/// Class TSoftObjectPtr.
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
public class TSoftObjectPtr<T> : ISoftObjectPtr
    where T : UObject
{
    /// <summary>
    /// The native PTR
    /// </summary>
    private readonly IntPtr _nativePtr;

    /// <summary>
    /// The cached pointer
    /// </summary>
    private IntPtr _cachedPointer;

    /// <summary>
    /// The cached value
    /// </summary>
    private T? _cachedValue;

    /// <summary>
    /// The constructor
    /// </summary>
    private static readonly IntelligentCacheUtils.ConstructDelegateType<T> Constructor = address => InteropUtils.GetObject<T>(address, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="TSoftObjectPtr{T}" /> class.
    /// </summary>
    /// <param name="nativePtr">The native PTR.</param>
    public TSoftObjectPtr(IntPtr nativePtr)
    {
        _nativePtr = nativePtr;
    }

    /// <summary>
    /// Gets the native PTR.
    /// </summary>
    /// <returns>IntPtr.</returns>
    public IntPtr GetNativePtr()
    {
        return _nativePtr;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is binding to unreal.
    /// </summary>
    /// <value><c>true</c> if this instance is binding to unreal; otherwise, <c>false</c>.</value>
    public bool IsBindingToUnreal => _nativePtr != IntPtr.Zero;

    /// <summary>
    /// Reset the lazy pointer back to the null state
    /// </summary>
    public void Reset()
    {
        if(IsBindingToUnreal)
        {
            SoftObjectPtrInteropUtils.ResetSoftObjectPtr(_nativePtr);
        }
    }

    /// <summary>
    /// Resets the weak ptr only, call this when ObjectId may change
    /// </summary>
    public void ResetWeakPtr()
    {
        if (IsBindingToUnreal)
        {
            SoftObjectPtrInteropUtils.ResetSoftObjectPtrWeakPtr(_nativePtr);
        }
    }

    /// <summary>
    /// Test if this does not point to a live UObject, but may in the future
    /// return true if this does not point to a real object, but could possibly
    /// </summary>
    /// <returns><c>true</c> if this instance is pending; otherwise, <c>false</c>.</returns>
    public bool IsPending()
    {
        return IsBindingToUnreal && SoftObjectPtrInteropUtils.IsSoftObjectPtrPending(_nativePtr);
    }

    /// <summary>
    /// Test if this points to a live UObject
    /// </summary>
    /// <returns>return true if Get() would return a valid non-null pointer</returns>
    public bool IsValid()
    {
        return IsBindingToUnreal && SoftObjectPtrInteropUtils.IsSoftObjectPtrValid(_nativePtr);
    }

    /// <summary>
    /// Slightly different !IsValid(), returns true if this used to point to a UObject, but doesn't anymore and has not been assigned or reset in mean time.
    /// </summary>
    /// <returns>return true if this used to point at a real object but no longer does.</returns>
    public bool IsStale()
    {
        return IsBindingToUnreal && SoftObjectPtrInteropUtils.IsSoftObjectPtrStale(_nativePtr);
    }

    /// <summary>
    /// Test if this can never point to a live UObject
    /// </summary>
    /// <returns>return true if this is explicitly pointing to no object</returns>
    public bool IsNull()
    {
        return IsBindingToUnreal && SoftObjectPtrInteropUtils.IsSoftObjectPtrNull(_nativePtr);
    }

    /// <summary>
    /// Dereference the pointer, which may cause it to become valid again. Will not try to load pending outside of game thread
    /// </summary>
    /// <returns>return nullptr if this object is gone or the pointer was null, otherwise a valid UObject pointer</returns>
    public T? Get()
    {
        if(!IsBindingToUnreal)
        {
            return null;
        }

        var unrealObjectPtr = SoftObjectPtrInteropUtils.GetUnrealObjectPointerOfSoftObjectPtr(_nativePtr);

        unsafe
        {
            return IntelligentCacheUtils.GetPointerBasedValue(ref _cachedPointer, ref _cachedValue, new IntPtr(&unrealObjectPtr), Constructor);
        }            
    }

    /// <summary>
    /// Dereference the lazy pointer, which may cause it to become valid again. Will not try to load pending outside of game thread
    /// </summary>
    /// <param name="evenIfPendingKill">if this is true, pending kill objects are considered valid</param>
    /// <returns>return null if this object is gone or the lazy pointer was null, otherwise a valid UObject pointer</returns>
    public UObject? Get(bool evenIfPendingKill)
    {
        if(!IsBindingToUnreal)
        {                
            return null;
        }

        var unrealObjectPtr = SoftObjectPtrInteropUtils.GetUnrealObjectPointerOfSoftObjectPtrEx(_nativePtr, evenIfPendingKill);

        unsafe
        {
            return IntelligentCacheUtils.GetPointerBasedValue(ref _cachedPointer, ref _cachedValue, new IntPtr(&unrealObjectPtr), Constructor);
        }
    }

    /// <summary>
    /// Synchronously load (if necessary) and return the asset object represented by this asset ptr
    /// </summary>
    /// <returns>System.Nullable&lt;UObject&gt;.</returns>
    public UObject? LoadSynchronous()
    {
        if(!IsBindingToUnreal)
        {
            return null;
        }

        var unrealObjectPtr = SoftObjectPtrInteropUtils.LoadSynchronousSoftObjectPtr(_nativePtr);

        unsafe
        {
            return IntelligentCacheUtils.GetPointerBasedValue(ref _cachedPointer, ref _cachedValue, new IntPtr(&unrealObjectPtr), Constructor);
        }
    }

    /// <summary>
    /// Returns the StringObjectPath that is wrapped by this TSoftObjectPtr
    /// </summary>
    /// <returns>FSoftObjectPath.</returns>
    public FSoftObjectPath ToSoftObjectPath()
    {
        if(!IsBindingToUnreal)
        {
            return default;
        }

        var pathPtr = SoftObjectPtrInteropUtils.GetObjectIdPointerOfSoftObjectPtr(_nativePtr);

        return FSoftObjectPath.FromNative(pathPtr, 0);
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return ToSoftObjectPath().ToString()!;
    }

    /// <summary>
    /// Copies from.
    /// </summary>
    /// <param name="other">The other.</param>
    public void CopyFrom(TSoftObjectPtr<T>? other)
    {
        if(!IsBindingToUnreal)
        {
            return;
        }

        SoftObjectPtrInteropUtils.CopySoftObjectPtr(_nativePtr, other?._nativePtr ?? IntPtr.Zero);
    }

    /// <summary>
    /// Resets from.
    /// </summary>
    /// <param name="other">The other.</param>
    public void ResetFrom(ISoftObjectPtr other)
    {
        if(other is TSoftObjectPtr<T> ptr)
        {
            CopyFrom(ptr);
        }
    }

    /// <summary>
    /// Writes to.
    /// </summary>
    /// <param name="addressOfNativeSoftObjectPtr">The address of native soft object PTR.</param>
    public void WriteTo(IntPtr addressOfNativeSoftObjectPtr)
    {
        SoftObjectPtrInteropUtils.CopySoftObjectPtr(addressOfNativeSoftObjectPtr, _nativePtr);
    }
}
#endregion

#region TSoftObjectPtrCache
/// <summary>
/// Struct TSoftObjectPtrCache
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
public struct TSoftObjectPtrCache<T> where T : UObject
{
    /// <summary>
    /// The cached address
    /// </summary>
    private IntPtr _cachedAddress;

    /// <summary>
    /// The cache
    /// </summary>
    private TSoftObjectPtr<T>? _cache;

    /// <summary>
    /// The constructor
    /// </summary>
    private static readonly IntelligentCacheUtils.ConstructDelegateType<TSoftObjectPtr<T>> Constructor = address=> new TSoftObjectPtr<T>(address);

    /// <summary>
    /// Gets the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>System.Nullable&lt;TSoftObjectPtr&lt;T&gt;&gt;.</returns>
    public TSoftObjectPtr<T>? Get(IntPtr address,int offset)
    {
        return IntelligentCacheUtils.GetAddressBasedValue(
            ref _cachedAddress,
            ref _cache,
            address,
            offset,
            Constructor
        );
    }
}
#endregion

#region TSoftClassPtr
/// <summary>
/// Class TSoftClassPtr.
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
public class TSoftClassPtr<T> : ISoftClassPtr
    where T : UObject
{
    /// <summary>
    /// The soft object PTR
    /// </summary>
    private readonly TSoftObjectPtr<T> _softObjectPtr;

    /// <summary>
    /// Initializes a new instance of the <see cref="TSoftClassPtr{T}" /> class.
    /// </summary>
    /// <param name="nativePtr">The native PTR.</param>
    public TSoftClassPtr(IntPtr nativePtr) 
    {
        _softObjectPtr = new TSoftObjectPtr<T>(nativePtr);
    }

    /// <summary>
    /// Gets the native PTR.
    /// </summary>
    /// <returns>IntPtr.</returns>
    public IntPtr GetNativePtr()
    {
        return _softObjectPtr.GetNativePtr();
    }

    /// <summary>
    /// Reset the soft pointer back to the null state
    /// </summary>
    public void Reset()
    {
        _softObjectPtr.Reset();
    }

    /// <summary>
    /// Resets the weak ptr only, call this when ObjectId may change
    /// </summary>
    public void ResetWeakPtr()
    {
        _softObjectPtr.ResetWeakPtr();
    }

    /// <summary>
    /// Get soft class
    /// </summary>
    /// <returns>System.Nullable&lt;UClass&gt;.</returns>
    public UClass? Get()
    {
        if(!_softObjectPtr.IsBindingToUnreal)
        {
            return null;
        }

        var unrealObjectPtr = SoftObjectPtrInteropUtils.GetUnrealObjectPointerOfSoftObjectPtr(_softObjectPtr.GetNativePtr());

        return unrealObjectPtr == IntPtr.Zero ? null : new UClass(unrealObjectPtr);
    }

    /// <summary>
    /// Returns true if ... is valid.
    /// </summary>
    /// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid()
    {
        if (!_softObjectPtr.IsBindingToUnreal)
        {
            return false;
        }

        var unrealObjectPtr = SoftObjectPtrInteropUtils.GetUnrealObjectPointerOfSoftObjectPtr(_softObjectPtr.GetNativePtr());

        return unrealObjectPtr != IntPtr.Zero;
    }

    /// <summary>
    /// Determines whether this instance is pending.
    /// </summary>
    /// <returns><c>true</c> if this instance is pending; otherwise, <c>false</c>.</returns>
    public bool IsPending()
    {
        return _softObjectPtr.IsPending();
    }

    /// <summary>
    /// Determines whether this instance is null.
    /// </summary>
    /// <returns><c>true</c> if this instance is null; otherwise, <c>false</c>.</returns>
    public bool IsNull()
    {
        return _softObjectPtr.IsNull();
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return _softObjectPtr.ToString();
    }

    /// <summary>
    /// Copies from.
    /// </summary>
    /// <param name="classPtr">The class PTR.</param>
    public void CopyFrom(TSoftClassPtr<T>? classPtr)
    {
        _softObjectPtr.CopyFrom(classPtr?._softObjectPtr);
    }

    /// <summary>
    /// Resets from.
    /// </summary>
    /// <param name="other">The other.</param>
    public void ResetFrom(ISoftClassPtr other)
    {
        if(other is TSoftClassPtr<T> ptr)
        {
            CopyFrom(ptr);
        }
    }

    /// <summary>
    /// Writes to.
    /// </summary>
    /// <param name="addressOfNativeSoftClassPtr">The address of native soft class PTR.</param>
    public void WriteTo(IntPtr addressOfNativeSoftClassPtr)
    {
        _softObjectPtr.WriteTo(addressOfNativeSoftClassPtr);
    }
}
#endregion

#region TSoftClassPtrCache
/// <summary>
/// Struct TSoftClassPtrCache
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
public struct TSoftClassPtrCache<T> where T : UObject
{
    /// <summary>
    /// The cached address
    /// </summary>
    private IntPtr _cachedAddress;

    /// <summary>
    /// The cache
    /// </summary>
    private TSoftClassPtr<T>? _cache;

    /// <summary>
    /// The constructor
    /// </summary>
    private static readonly IntelligentCacheUtils.ConstructDelegateType<TSoftClassPtr<T>> Constructor = address => new TSoftClassPtr<T>(address);

    /// <summary>
    /// Gets the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>System.Nullable&lt;TSoftClassPtr&lt;T&gt;&gt;.</returns>
    public TSoftClassPtr<T>? Get(IntPtr address,int offset)
    {
        Logger.Assert(address != IntPtr.Zero);

        return IntelligentCacheUtils.GetAddressBasedValue(ref _cachedAddress, ref _cache, address, offset, Constructor);
    }
}
#endregion