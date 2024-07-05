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
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnrealSharp.UnrealEngine.InteropService;
using UnrealSharp.Utils.Misc;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace UnrealSharp.UnrealEngine.Collections;

/// <summary>
/// Class TSet.
/// Please note that this container is just a View.
/// Saving it outside the binding class is dangerous.
/// If you want to do such an operation for any purpose, you need to understand the principle behind it.
/// Implements the <see cref="ISet{T}" />
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="ISet{T}" />
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(TSetDebugView<>))]
// ReSharper disable once InconsistentNaming
public class TSet<T> : ISet<T>, IUnrealCollectionDataView<T>
{
    #region Properties
    /// <summary>
    /// Gets the address PTR.
    /// </summary>
    /// <value>The address PTR.</value>
    public IntPtr AddressPtr { get; private set; }
    /// <summary>
    /// Gets the property PTR.
    /// </summary>
    /// <value>The property PTR.</value>
    public IntPtr PropertyPtr { get; private set; }
    /// <summary>
    /// Gets the element property PTR.
    /// </summary>
    /// <value>The element property PTR.</value>
    public IntPtr ElementPropertyPtr { get; private set; }

    /// <summary>
    /// The interop policy
    /// </summary>
    private static readonly IInteropPolicy<T> InteropPolicy = InteropPolicyFactory.GetPolicy<T>();

    /// <summary>
    /// The element property size
    /// </summary>
    public readonly int ElementPropertySize;

    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="TSet{T}" /> class.
    /// </summary>
    /// <param name="addressPtr">The address PTR.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    public TSet(IntPtr addressPtr, IntPtr propertyPtr)
    {
        AddressPtr = addressPtr;
        PropertyPtr = propertyPtr;

        ElementPropertyPtr = SetInteropUtils.GetElementPropertyOfSet(PropertyPtr);

        Logger.Ensure<Exception>(ElementPropertyPtr != IntPtr.Zero);

        ElementPropertySize = PropertyInteropUtils.GetPropertySize(ElementPropertyPtr);
        Logger.Ensure<Exception>(ElementPropertySize > 0, "Get property element size of {0} is {1}", GetType().FullName!, ElementPropertySize);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TSet{T}" /> class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    public TSet(IntPtr address, int offset, IntPtr propertyPtr) :
        this(IntPtr.Add(address, offset), propertyPtr)
    {
    }
    #endregion

    #region Base Interfaces
    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            AddressPtr.GetHashCode(), 
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            PropertyPtr.GetHashCode());
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is TSet<T> set)
        {
            return AddressPtr == set.AddressPtr && PropertyPtr == set.PropertyPtr;
        }

        return false;
    }

    /// <summary>
    /// Determines whether this instance is empty.
    /// </summary>
    /// <returns><c>true</c> if this instance is empty; otherwise, <c>false</c>.</returns>
    public bool IsEmpty()
    {
        return Count <= 0;
    }
    #endregion

    #region Collection Interfaces
    /// <summary>
    /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
    /// </summary>
    /// <value>The count.</value>
    public int Count
    {
        get
        {
            if (AddressPtr == IntPtr.Zero || PropertyPtr == IntPtr.Zero)
            {
                return 0;
            }

            return SetInteropUtils.GetLengthOfSet(AddressPtr, PropertyPtr);
        }
    }

    /// <summary>
    /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
    /// </summary>
    /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
    /// <exception cref="System.NotImplementedException"></exception>
    public bool IsReadOnly => throw new NotImplementedException();

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns>IEnumerator&lt;T&gt;.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < Count; ++i)
        {
            var address = SetInteropUtils.GetElementAddressOfSet(AddressPtr, PropertyPtr, i);

            var value = InteropPolicy.Read(address);

            yield return value;
        }
    }

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns>IEnumerator.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Determines whether this instance contains the object.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns><c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.</returns>
    public bool Contains(T item)
    {
        unsafe
        {
            var bytes = stackalloc byte[ElementPropertySize];

            using var tempVariable = new ScopedPropertyTempVariable(ElementPropertyPtr, bytes);

            InteropPolicy.Write(tempVariable.VariablePtr, item);

            return SetInteropUtils.IsSetContainsElement(AddressPtr, PropertyPtr, tempVariable.VariablePtr);
        }
    }

    /// <summary>
    /// Adds the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns><c>true</c> if add success, <c>false</c> otherwise.</returns>
    public bool Add(T item)
    {
        unsafe
        {
            var bytes = stackalloc byte[ElementPropertySize];

            using var tempVariable = new ScopedPropertyTempVariable(ElementPropertyPtr, bytes);

            InteropPolicy.Write(tempVariable.VariablePtr, item);

            return SetInteropUtils.AddSetElement(AddressPtr, PropertyPtr, tempVariable.VariablePtr);
        }            
    }

    /// <summary>
    /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    void ICollection<T>.Add(T item)
    {
        if(!Contains(item))
        {
            Add(item);
        }
    }

    /// <summary>
    /// Removes the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns><see langword="true" /> if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
    public bool Remove(T item)
    {
        var oldLength = Count;

        unsafe
        {
            var bytes = stackalloc byte[ElementPropertySize];
            using var tempVariable = new ScopedPropertyTempVariable(ElementPropertyPtr, bytes);

            InteropPolicy.Write(tempVariable.VariablePtr, item);

            SetInteropUtils.RemoveSetElement(AddressPtr, PropertyPtr, tempVariable.VariablePtr);

            return oldLength != Count;
        }            
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    public void Clear()
    {
        SetInteropUtils.ClearSet(AddressPtr, PropertyPtr);
    }
    #endregion

    #region Interop
    /// <summary>
    /// Copies from.
    /// </summary>
    /// <param name="source">The source.</param>
    public void CopyFrom(IEnumerable<T>? source)
    {
        if (source is TSet<T> sourceSet && sourceSet.Equals(this))
        {
            return;
        }

        Clear();

        if (source == null)
        {
            return;
        }
            
        foreach(var i in source)
        {
            Add(i);
        }
    }

    /// <summary>
    /// Copies from.
    /// </summary>
    /// <param name="enumerable">The enumerable.</param>
    /// <returns><c>true</c> if copy success, <c>false</c> otherwise.</returns>
    public bool CopyFrom(IEnumerable enumerable)
    {
        if (enumerable is not IEnumerable<T> e)
        {
            return false;
        }
            
        CopyFrom(e);
        return true;

    }

    /// <summary>
    /// Converts to hashset.
    /// </summary>
    /// <returns>HashSet&lt;T&gt;.</returns>
    public HashSet<T> ToHashSet()
    {
        var values = new HashSet<T>();

        foreach(var i in this)
        {
            values.Add(i);
        }

        return values;
    }
    #endregion

    #region ISet Interfaces
    /// <summary>
    /// Removes all elements in the specified collection from the current set.
    /// </summary>
    /// <param name="other">The collection of items to remove from the set.</param>
    public void ExceptWith(IEnumerable<T> other)
    {
        Logger.EnsureNotNull(other);

        // This is already the empty set; return.
        if (Count == 0)
        {
            return;
        }

        // Special case if other is this; a set minus itself is the empty set.
        if (Equals(other, this))
        {
            Clear();
            return;
        }

        // Remove every element in other from this.
        foreach (var element in other)
        {
            Remove(element);
        }
    }

    /// <summary>
    /// Modifies the current set so that it contains only elements that are also in a specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    public void IntersectWith(IEnumerable<T> other)
    {
        Logger.EnsureNotNull(other);

        // Intersection of anything with empty set is empty set, so return if count is 0.
        // Same if the set intersecting with itself is the same set.
        if (Count == 0 || Equals(other, this))
        {
            return;
        }

        var enumerable = other as T[] ?? other.ToArray();
        if(enumerable.Length == 0)
        {
            Clear();
            return;
        }

        foreach(var item in this.ToArray())
        {
            if(!enumerable.Contains(item))
            {
                Remove(item);
            }
        }
    }

    /// <summary>
    /// Determines whether the current set is a proper (strict) subset of a specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns><see langword="true" /> if the current set is a proper subset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        Logger.EnsureNotNull(other);
            
        var enumerable = other as T[] ?? other.ToArray();
            
        return Count != enumerable.Length && IsSubsetOf(enumerable);
    }

    /// <summary>
    /// Determines whether the current set is a proper (strict) superset of a specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns><see langword="true" /> if the current set is a proper superset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        Logger.EnsureNotNull(other);

        var enumerable = other as T[] ?? other.ToArray();
            
        return Count != enumerable.Length && IsSupersetOf(enumerable);
    }

    /// <summary>
    /// Determines whether a set is a subset of a specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns><see langword="true" /> if the current set is a subset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        Logger.EnsureNotNull(other);

        var enumerable = other.ToList();

        return this.All(item => enumerable.Contains(item));
    }

    /// <summary>
    /// Determines whether the current set is a superset of a specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns><see langword="true" /> if the current set is a superset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        Logger.EnsureNotNull(other);

        return other.All(Contains);
    }

    /// <summary>
    /// Determines whether the current set overlaps with the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns><see langword="true" /> if the current set and <paramref name="other" /> share at least one common element; otherwise, <see langword="false" />.</returns>
    public bool Overlaps(IEnumerable<T> other)
    {
        Logger.EnsureNotNull(other);

        return Equals(other, this) || other.Any(Contains);
    }

    /// <summary>
    /// Determines whether the current set and the specified collection contain the same elements.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns><see langword="true" /> if the current set is equal to <paramref name="other" />; otherwise, <see langword="false" />.</returns>
    public bool SetEquals(IEnumerable<T> other)
    {
        Logger.EnsureNotNull(other);

        if (Equals(other, this))
        {
            return true;
        }

        var enumerable = other as T[] ?? other.ToArray();
            
        return Count == enumerable.Length && enumerable.All(Contains);
    }

    /// <summary>
    /// Modifies the current set so that it contains only elements that are present either in the current set or in the specified collection, but not both.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        Logger.EnsureNotNull(other);

        var list = new List<T>();
        foreach(var item in other)
        {
            if(!Contains(item))
            {
                list.Add(item);
            }
            else
            {
                Remove(item);
            }
        }

        foreach(var item in list)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Modifies the current set so that it contains all elements that are present in the current set, in the specified collection, or in both.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    public void UnionWith(IEnumerable<T> other)
    {
        Logger.EnsureNotNull(other);
            
        foreach(var item in other)
        {
            if(!Contains(item))
            {
                Add(item);
            }
        }
    }

    /// <summary>
    /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
    /// </summary>
    /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    /// <exception cref="System.ArgumentNullException">array</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">arrayIndex</exception>
    /// <exception cref="System.ArgumentException">The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array.</exception>
    public void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);

        if (arrayIndex < 0 || arrayIndex > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        if (array.Length - arrayIndex < Count)
        {
            throw new ArgumentException("The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array.");
        }

        var i = 0;
        foreach(var item in this)
        {
            array[arrayIndex + i] = item;
            ++i;
        }
    }

    /// <summary>
    /// Gets the native PTR.
    /// </summary>
    /// <returns>IntPtr.</returns>
    public nint GetNativePtr()
    {
        return AddressPtr;
    }

    /// <summary>
    /// Disconnects from native.
    /// </summary>
    public void DisconnectFromNative()
    {
        AddressPtr = IntPtr.Zero;
    }

    /// <summary>
    /// Retains this instance.
    /// </summary>
    /// <returns>IEnumerable&lt;T&gt;.</returns>
    public IEnumerable<T> Retain()
    {
        return this.ToHashSet<T>();
    }

    /// <summary>
    /// Retains this instance.
    /// </summary>
    /// <returns>IEnumerable.</returns>
    IEnumerable IUnrealCollectionDataView.Retain()
    {
        return this.ToHashSet<T>();
    }

    #endregion

    #region Interop Service
    /// <summary>
    /// From the native.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <returns>HashSet&lt;T&gt;.</returns>
    public static HashSet<T>? FromNative(IntPtr address, int offset, IntPtr propertyPtr)
    {
        if (address == IntPtr.Zero)
        {
            return null;
        }
            
        var tempSet = new TSet<T>(IntPtr.Add(address, offset), propertyPtr);

        return tempSet.ToHashSet();
    }

    /// <summary>
    /// Converts to native.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <param name="values">The values.</param>
    public static void ToNative(IntPtr address, int offset, IntPtr propertyPtr, IEnumerable<T>? values)
    {
        var tempSet = new TSet<T>(IntPtr.Add(address, offset), propertyPtr);

        tempSet.CopyFrom(values);
    }
    #endregion
}

#region Debugger Support
/// <summary>
/// Class TSetDebugView.
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
internal class TSetDebugView<T>
{
    /// <summary>
    /// The set
    /// </summary>
    private readonly TSet<T>? _set;

    /// <summary>
    /// Initializes a new instance of the <see cref="TSetDebugView{T}"/> class.
    /// </summary>
    /// <param name="set">The set.</param>
    public TSetDebugView(TSet<T>? set)
    {
        _set = set;
    }

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <value>The items.</value>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items => _set != null ? _set.ToArray() : Array.Empty<T>();
}
#endregion

#region Extensions
/// <summary>
/// Class TSetExtensions.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class TSetExtensions
{
    /// <summary>
    /// Retains the specified set.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="set">The set.</param>
    /// <returns>System.Nullable&lt;HashSet&lt;T&gt;&gt;.</returns>
    public static HashSet<T>? Retain<T>(this ISet<T>? set)
    {
        return set switch
        {
            null => null,
            HashSet<T> csharpHashSet => csharpHashSet,
            TSet<T> unrealSetView => unrealSetView.ToHashSet(),
            _ => set.ToHashSet()
        };
    }
}
#endregion

#region Property Cache
/// <summary>
/// Struct TSetPropertyCache
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
public struct TSetPropertyCache<T>
{
    /// <summary>
    /// The set cache
    /// </summary>
    private TSet<T>? _setCache;

    /// <summary>
    /// Gets the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <returns>System.Nullable&lt;TSet&lt;T&gt;&gt;.</returns>
    public TSet<T>? Get(IntPtr address, int offset, IntPtr propertyPtr)
    {
        if (address == IntPtr.Zero)
        {
            _setCache = null;
            return null;
        }

        if (_setCache != null)
        {
            return _setCache;
        }

        _setCache = new TSet<T>(address, offset, propertyPtr);

        return _setCache;
    }
}
#endregion