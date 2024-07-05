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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnrealSharp.UnrealEngine.InteropService;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.UnrealEngine;

#region Base Interfaces
/// <summary>
/// Struct FDelegate
/// </summary>
public struct FDelegate
{
    /// <summary>
    /// Gets the target.
    /// </summary>
    /// <value>The target.</value>
    public UObject? Target { get; private set; }
    /// <summary>
    /// Gets the name of the method.
    /// </summary>
    /// <value>The name of the method.</value>
    public string? MethodName { get; private set; }

    /// <summary>
    /// Returns true if ... is valid.
    /// </summary>
    /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
    public bool IsValid => Target != null && MethodName != null;

    /// <summary>
    /// Initializes a new instance of the <see cref="FDelegate" /> struct.
    /// </summary>
    public FDelegate()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FDelegate" /> struct.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="methodName">Name of the method.</param>
    public FDelegate(UObject target, string methodName)
    {
        Target = target;
        MethodName = methodName;
    }

    /// <summary>
    /// Converts to native.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="delegate">The delegate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ToNative(IntPtr address, ref FDelegate @delegate)
    {
        InteropUtils.SetObject(address, 0, @delegate.Target);
        InteropUtils.SetString(address, Marshal.SizeOf<IntPtr>(), @delegate.MethodName);
    }

    /// <summary>
    /// From the native.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>FDelegate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FDelegate FromNative(IntPtr address)
    {
        var @delegate = new FDelegate();
            
        var target = InteropUtils.GetObject<UObject>(address, 0);
        var methodName = InteropUtils.GetString(address, Marshal.SizeOf<IntPtr>());

        @delegate.Target = target;
        @delegate.MethodName = methodName;

        return @delegate;
    }
}
#endregion

#region Base Template
/// <summary>
/// Class TBaseDelegate.
/// Implements the <see cref="System.IDisposable" />
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="System.IDisposable" />
// ReSharper disable once InconsistentNaming
public class TBaseDelegate<T> : IDisposable
    where T : Delegate
{
    /// <summary>
    /// The owner
    /// </summary>
    public readonly UObject? Owner;
    /// <summary>
    /// The property PTR
    /// </summary>
    public readonly IntPtr PropertyPtr;
    /// <summary>
    /// The address PTR
    /// </summary>
    public readonly IntPtr AddressPtr;

    /// <summary>
    /// The delegate invoke method
    /// </summary>
    public static readonly MethodInfo DelegateInvokeMethod = typeof(T).GetMethod("Invoke")!;

    /// <summary>
    /// The invocation private
    /// </summary>
    private UnrealInvocation? _invocationPrivate;

    /// <summary>
    /// Gets the invocation.
    /// </summary>
    /// <value>The invocation.</value>
    public UnrealInvocation Invocation => _invocationPrivate ??= new UnrealInvocation(PropertyPtr);

    /// <summary>
    /// Initializes a new instance of the <see cref="TBaseDelegate{T}" /> class.
    /// </summary>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="addressPtr">The address PTR.</param>
    public TBaseDelegate(IntPtr propertyPtr, IntPtr addressPtr)
    {
        PropertyPtr = propertyPtr;
        AddressPtr = addressPtr;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TBaseDelegate{T}" /> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="addressPtr">The address PTR.</param>
    public TBaseDelegate(UObject owner, IntPtr propertyPtr, IntPtr addressPtr)
    {
        Owner = owner;
        PropertyPtr = propertyPtr;
        AddressPtr = addressPtr;
    }

    /// <summary>
    /// Checks the signature.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="methodName">Name of the method.</param>
    [Conditional("DEBUG")]
    public static void CheckSignature(UObject target, string methodName)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static);

        Logger.Ensure<Exception>(method != null, $"Failed find UFUNCTION/UEVENT {methodName}");
        // ReSharper disable once StringLiteralTypo
        Logger.Ensure<ArgumentException>(method!.IsDefined<UFUNCTIONAttribute>(), "Delegate target method must be UFUNCTION or UEVENT");
    }

    /// <summary>
    /// Clears the delegates.
    /// </summary>
    public void ClearDelegates()
    {
        DelegateInteropUtils.ClearDelegate(AddressPtr, PropertyPtr);
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is TBaseDelegate<T> tbd && tbd.AddressPtr == AddressPtr && tbd.PropertyPtr == PropertyPtr;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(AddressPtr.GetHashCode(), PropertyPtr.GetHashCode());
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
#pragma warning disable CA1816
    public void Dispose()
#pragma warning restore CA1816
    {
        if (_invocationPrivate == null)
        {
            return;
        }
        
        _invocationPrivate.Dispose();
        _invocationPrivate = null;
    }
}

#endregion

#region Single Delegate
/// <summary>
/// Class TDelegate.
/// Implements the <see cref="TBaseDelegate{T}" />
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="TBaseDelegate{T}" />
// ReSharper disable once InconsistentNaming
public class TDelegate<T> : TBaseDelegate<T> where T : Delegate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TDelegate{T}" /> class.
    /// </summary>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="addressPtr">The address PTR.</param>
    public TDelegate(IntPtr propertyPtr, IntPtr addressPtr) :
        base(propertyPtr, addressPtr)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TDelegate{T}" /> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="addressPtr">The address PTR.</param>
    public TDelegate(UObject owner, IntPtr propertyPtr, IntPtr addressPtr) :
        base(owner, propertyPtr, addressPtr)
    {
    }

    /// <summary>
    /// Binds the specified target.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="methodName">Name of the method.</param>
    public void Bind(UObject target, string methodName)
    {
        CheckSignature(target, methodName);

        DelegateInteropUtils.BindDelegate(AddressPtr, PropertyPtr, target.GetNativePtr(), methodName);
    }

    /// <summary>
    /// Unbinds this instance.
    /// </summary>
    public void Unbind()
    {
        DelegateInteropUtils.UnbindDelegate(AddressPtr, PropertyPtr);
    }

    /// <summary>
    /// Binds the specified delegate.
    /// </summary>
    /// <param name="delegate">The delegate.</param>
    public void Bind(FDelegate @delegate)
    {
        if (@delegate.IsValid)
        {
            Bind(@delegate.Target!, @delegate.MethodName!);
        }
    }
}
#endregion

#region Multicast Delegate
/// <summary>
/// Class TMulticastDelegate.
/// Implements the <see cref="TBaseDelegate{T}" />
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="TBaseDelegate{T}" />
// ReSharper disable once InconsistentNaming
public class TMulticastDelegate<T> : TBaseDelegate<T> where T : Delegate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TMulticastDelegate{T}" /> class.
    /// </summary>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="addressPtr">The address PTR.</param>
    public TMulticastDelegate(IntPtr propertyPtr, IntPtr addressPtr) :
        base(propertyPtr, addressPtr)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TMulticastDelegate{T}" /> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="addressPtr">The address PTR.</param>
    public TMulticastDelegate(UObject owner, IntPtr propertyPtr, IntPtr addressPtr) :
        base(owner, propertyPtr, addressPtr)
    {
    }

    /// <summary>
    /// Adds the specified target.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="methodName">Name of the method.</param>
    public void Add(UObject target, string methodName)
    {
        CheckSignature(target, methodName);

        DelegateInteropUtils.AddDelegate(AddressPtr, PropertyPtr, target.GetNativePtr(), methodName);
    }

    /// <summary>
    /// Removes the specified target.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="methodName">Name of the method.</param>
    public void Remove(UObject target, string methodName)
    {
        CheckSignature(target, methodName);

        DelegateInteropUtils.RemoveDelegate(AddressPtr, PropertyPtr, target.GetNativePtr(), methodName);
    }

    /// <summary>
    /// Adds the specified delegate.
    /// </summary>
    /// <param name="delegate">The delegate.</param>
    public void Add(FDelegate @delegate)
    {
        if (@delegate.IsValid)
        {
            Add(@delegate.Target!, @delegate.MethodName!);
        }
    }

    /// <summary>
    /// Removes the specified delegate.
    /// </summary>
    /// <param name="delegate">The delegate.</param>
    public void Remove(FDelegate @delegate)
    {
        if (@delegate.IsValid)
        {
            Remove(@delegate.Target!, @delegate.MethodName!);
        }
    }

    /// <summary>
    /// Removes all.
    /// </summary>
    /// <param name="target">The target.</param>
    public void RemoveAll(UObject target)
    {
        DelegateInteropUtils.RemoveAllDelegate(AddressPtr, PropertyPtr, target.GetNativePtr());
    }
}
#endregion

#region Property Cache
/// <summary>
/// Struct TDelegatePropertyCache
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
public struct TDelegatePropertyCache<T> where T : Delegate
{
    /// <summary>
    /// The delegate cache
    /// </summary>
    private TDelegate<T>? _delegateCache;

    /// <summary>
    /// Delegate ConstructorDelegateType
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="addressPtr">The address PTR.</param>
    /// <returns>TDelegate&lt;T&gt;.</returns>
    public delegate TDelegate<T> ConstructorDelegateType(UObject owner, IntPtr propertyPtr, IntPtr addressPtr);

    /// <summary>
    /// The constructor
    /// </summary>
    private readonly ConstructorDelegateType? _constructor;

    /// <summary>
    /// Initializes a new instance of the <see cref="TDelegatePropertyCache{T}"/> struct.
    /// </summary>
    public TDelegatePropertyCache()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TDelegatePropertyCache{T}"/> struct.
    /// </summary>
    /// <param name="customConstructor">The custom constructor.</param>
    public TDelegatePropertyCache(ConstructorDelegateType customConstructor)
    {
        _constructor = customConstructor;
    }

    /// <summary>
    /// Gets the specified owner.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="addressPtr">The address PTR.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <returns>System.Nullable&lt;TDelegate&lt;T&gt;&gt;.</returns>
    public TDelegate<T>? Get(UObject owner, IntPtr addressPtr, int offset, IntPtr propertyPtr)
    {
        if (addressPtr != IntPtr.Zero)
        {
            return _delegateCache ??= _constructor?.Invoke(owner, propertyPtr, IntPtr.Add(addressPtr, offset)) ??
                                      new TDelegate<T>(owner, propertyPtr, IntPtr.Add(addressPtr, offset));
        }
            
        _delegateCache = null;
        return null;

    }

    /// <summary>
    /// Gets the specified address PTR.
    /// </summary>
    /// <param name="addressPtr">The address PTR.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <returns>System.Nullable&lt;TDelegate&lt;T&gt;&gt;.</returns>
    public TDelegate<T>? Get(IntPtr addressPtr, int offset, IntPtr propertyPtr)
    {
        if (addressPtr != IntPtr.Zero)
        {
            return _delegateCache ??= new TDelegate<T>(propertyPtr, IntPtr.Add(addressPtr, offset));
        }
            
        _delegateCache = null;
            
        return null;

    }
}

/// <summary>
/// Struct TMulticastDelegatePropertyCache
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
public struct TMulticastDelegatePropertyCache<T> where T : Delegate
{
    /// <summary>
    /// The delegate cache
    /// </summary>
    private TMulticastDelegate<T>? _delegateCache;

    /// <summary>
    /// Delegate ConstructorDelegateType
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="addressPtr">The address PTR.</param>
    /// <returns>TMulticastDelegate&lt;T&gt;.</returns>
    public delegate TMulticastDelegate<T> ConstructorDelegateType(UObject owner, IntPtr propertyPtr, IntPtr addressPtr);

    /// <summary>
    /// The constructor
    /// </summary>
    private readonly ConstructorDelegateType? _constructor;

    /// <summary>
    /// Initializes a new instance of the <see cref="TMulticastDelegatePropertyCache{T}"/> struct.
    /// </summary>
    public TMulticastDelegatePropertyCache()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TMulticastDelegatePropertyCache{T}"/> struct.
    /// </summary>
    /// <param name="customConstructor">The custom constructor.</param>
    public TMulticastDelegatePropertyCache(ConstructorDelegateType customConstructor)
    {
        _constructor = customConstructor;
    }

    /// <summary>
    /// Gets the specified owner.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="addressPtr">The address PTR.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <returns>System.Nullable&lt;TMulticastDelegate&lt;T&gt;&gt;.</returns>
    public TMulticastDelegate<T>? Get(UObject owner, IntPtr addressPtr, int offset, IntPtr propertyPtr)
    {
        if (addressPtr != IntPtr.Zero)
        {
            return _delegateCache ??= _constructor?.Invoke(owner, propertyPtr, IntPtr.Add(addressPtr, offset)) ??
                                      new TMulticastDelegate<T>(owner, propertyPtr, IntPtr.Add(addressPtr, offset));
        }
                
        _delegateCache = null;
            
        return null;

    }

    /// <summary>
    /// Gets the specified address PTR.
    /// </summary>
    /// <param name="addressPtr">The address PTR.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <returns>System.Nullable&lt;TMulticastDelegate&lt;T&gt;&gt;.</returns>
    public TMulticastDelegate<T>? Get(IntPtr addressPtr, int offset, IntPtr propertyPtr)
    {
        if (addressPtr != IntPtr.Zero)
        {
            return _delegateCache ??= new TMulticastDelegate<T>(propertyPtr, IntPtr.Add(addressPtr, offset));
        }
                
        _delegateCache = null;
            
        return null;
    }
}
#endregion