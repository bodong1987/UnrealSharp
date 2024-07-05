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
/// Class TArray.
/// Please note that this container is just a View.
/// Saving it outside the binding class is dangerous.
/// If you want to do such an operation for any purpose, you need to understand the principle behind it.
/// Implements the <see cref="IList{T}" />
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="IList{T}" />
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(TArrayDebugView<>))]
// ReSharper disable once InconsistentNaming
public class TArray<T> : IList<T>, IUnrealCollectionDataView<T>
{
    #region Properties
    /// <summary>
    /// Gets the address PTR.
    /// </summary>
    /// <value>The address PTR.</value>
#if !DEBUG
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
    public IntPtr AddressPtr { get; private set; }
    /// <summary>
    /// Gets the property PTR.
    /// </summary>
    /// <value>The property PTR.</value>
#if !DEBUG
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
    public IntPtr PropertyPtr { get; private set; }

    /// <summary>
    /// Gets the element property PTR.
    /// </summary>
    /// <value>The element property PTR.</value>
#if !DEBUG
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
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
    /// Initializes a new instance of the <see cref="TArray{T}" /> class.
    /// </summary>
    /// <param name="addressPtr">The address PTR.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    public TArray(IntPtr addressPtr, IntPtr propertyPtr)
    {
        AddressPtr = addressPtr;
        PropertyPtr = propertyPtr;

        ElementPropertyPtr = ArrayInteropUtils.GetElementPropertyOfArray(propertyPtr);

        Logger.Ensure<Exception>(ElementPropertyPtr != IntPtr.Zero);

        ElementPropertySize = PropertyInteropUtils.GetPropertySize(ElementPropertyPtr);
        Logger.Ensure<Exception>(ElementPropertySize > 0, "Get property element size of {0} is {1}", GetType().FullName!, ElementPropertySize);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TArray{T}" /> class.
    /// </summary>
    /// <param name="addressPtr">The address PTR.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    public TArray(IntPtr addressPtr, int offset, IntPtr propertyPtr) :
        this(IntPtr.Add(addressPtr, offset), propertyPtr)
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
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return HashCode.Combine(
            AddressPtr.GetHashCode(), 
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            PropertyPtr.GetHashCode()
        );
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is TArray<T> array)
        {
            return AddressPtr == array.AddressPtr && PropertyPtr == array.PropertyPtr;
        }

        return false;
    }

    // unreal engine's FArrayProperty no this interface...
#if false
        /// <summary>
        /// Gets or sets the capacity.
        /// </summary>
        /// <value>The capacity.</value>
        public int Capacity
        {
            get => ArrayInteropUtils.GetCapacityOfArray(AddressPtr, PropertyPtr);
            set => ArrayInteropUtils.SetCapacityOfArray(AddressPtr, PropertyPtr, value);
        }
#endif

    /// <summary>
    /// Determines whether this instance is empty.
    /// </summary>
    /// <returns><c>true</c> if this instance is empty; otherwise, <c>false</c>.</returns>
    public bool IsEmpty()
    {
        return Count <= 0;
    }
    #endregion

    #region C# Interfaces
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

            return ArrayInteropUtils.GetLengthOfArray(AddressPtr, PropertyPtr);
        }
    }


    /// <summary>
    /// Determines whether this instance contains the object.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> if [contains] [the specified value]; otherwise, <c>false</c>.</returns>
    public bool Contains(T value)
    {
        return IndexOf(value) != -1;
    }

    /// <summary>
    /// Removes the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> if remove success, <c>false</c> otherwise.</returns>
    public bool Remove(T value)
    {
        var index = IndexOf(value);

        if (index == -1)
        {
            return false;
        }
            
        RemoveAt(index);
            
        return true;
    }

    /// <summary>
    /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
    /// </summary>
    /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns>IEnumerator&lt;T&gt;.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        if (!IsValid)
        {
            yield break;
        }

        for (var i = 0; i < Count; ++i)
        {
            var address = ArrayInteropUtils.GetElementAddressOfArray(AddressPtr, PropertyPtr, i);

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
    /// Clears this instance.
    /// </summary>
    public void Clear()
    {
        Logger.Ensure<AccessViolationException>(IsValid);

        ArrayInteropUtils.ClearArray(AddressPtr, PropertyPtr);
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    public void Add(T value)
    {
        Insert(Count, value);
    }

    /// <summary>
    /// Adds the range.
    /// </summary>
    /// <param name="values">The values.</param>
    public void AddRange(IEnumerable<T> values)
    {
        foreach(var v in values)
        {
            Add(v);
        }
    }

    /// <summary>
    /// Indexes the of.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>System.Int32.</returns>
    public int IndexOf(T value)
    {
        Logger.Ensure<AccessViolationException>(IsValid);

        unsafe
        {
            var bytes = stackalloc byte[ElementPropertySize];

            using var tempVariable = new ScopedPropertyTempVariable(ElementPropertyPtr, bytes);

            InteropPolicy.Write(tempVariable.VariablePtr, value);

            return ArrayInteropUtils.FindIndexOfArrayElement(AddressPtr, PropertyPtr, tempVariable.VariablePtr);
        }
    }

    /// <summary>
    /// Removes at.
    /// </summary>
    /// <param name="index">The index.</param>
    public void RemoveAt(int index)
    {
        Logger.Ensure<AccessViolationException>(IsValid);
        Logger.Ensure<ArgumentOutOfRangeException>(IsValidIndex(index));

        ArrayInteropUtils.RemoveAtArrayIndex(AddressPtr, PropertyPtr, index);
    }

    /// <summary>
    /// Inserts the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="value">The value.</param>
    public void Insert(int index, T value)
    {
        Logger.Ensure<AccessViolationException>(IsValid);

        var address = ArrayInteropUtils.InsertEmptyAtArrayIndex(AddressPtr, PropertyPtr, index);

        Logger.Ensure<AccessViolationException>(address != IntPtr.Zero, "address can not be null!");

        InteropPolicy.Write(address, value);
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
        Logger.Ensure<AccessViolationException>(IsValid);

        ArgumentNullException.ThrowIfNull(array);

        if (arrayIndex < 0 || arrayIndex > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        if (array.Length - arrayIndex < Count)
        {
            throw new ArgumentException("The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array.");
        }

        for (var i = 0; i < Count; i++)
        {
            array[arrayIndex + i] = this[i];
        }
    }
    #endregion

    #region Random Access
    /// <summary>
    /// get and set the element
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>T.</returns>
    public T this[int index]
    {
        get
        {
            Logger.Ensure<AccessViolationException>(IsValid);
            Logger.Ensure<ArgumentOutOfRangeException>(IsValidIndex(index));

            var address = ArrayInteropUtils.GetElementAddressOfArray(AddressPtr, PropertyPtr, index);

            Logger.Ensure<AccessViolationException>(address != IntPtr.Zero, "address can not be null!");

            var value = InteropPolicy.Read(address);

            return value;
        }
        set
        {
            Logger.Ensure<AccessViolationException>(IsValid);
            Logger.Ensure<ArgumentOutOfRangeException>(IsValidIndex(index));

            var address = ArrayInteropUtils.GetElementAddressOfArray(AddressPtr, PropertyPtr, index);

            Logger.Ensure<AccessViolationException>(address != IntPtr.Zero, "address can not be null!");

            InteropPolicy.Write(address, value);
        }
    }
    #endregion

    #region Unreal Interfaces
    /// <summary>
    /// Determines whether [is valid index] [the specified index].
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns><c>true</c> if [is valid index] [the specified index]; otherwise, <c>false</c>.</returns>
    public bool IsValidIndex(int index)
    {
        return IsValid && index >= 0 && index < Count;
    }

    /// <summary>
    /// Returns true if ... is valid.
    /// </summary>
    /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
    public bool IsValid => AddressPtr != IntPtr.Zero && PropertyPtr != IntPtr.Zero;

    /// <summary>
    /// Copies from.
    /// </summary>
    /// <param name="source">The source.</param>
    public void CopyFrom(IEnumerable<T>? source)
    {
        if (source is TArray<T> sourceArray && sourceArray.Equals(this))
        {
            return;
        }

        Clear();

        if (source == null)
        {
            return;
        }
        
        // Capacity = source.Count();

        foreach (var i in source)
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
    /// Gets the native PTR.
    /// </summary>
    /// <returns>IntPtr.</returns>
    public IntPtr GetNativePtr()
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
        return this.ToList();
    }

    /// <summary>
    /// Retains this instance.
    /// </summary>
    /// <returns>IEnumerable.</returns>
    IEnumerable IUnrealCollectionDataView.Retain()
    {
        return this.ToList();
    }
    #endregion

    #region Interop Service
    /// <summary>
    /// From the native.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <returns>List&lt;T&gt;.</returns>
    public static List<T>? FromNative(IntPtr address, int offset, IntPtr propertyPtr)
    {
        if (address == IntPtr.Zero)
        {
            return null;
        }
            
        // ReSharper disable once CollectionNeverUpdated.Local
        var tempArray = new TArray<T>(IntPtr.Add(address, offset), propertyPtr);

        var result = new List<T>(tempArray.Count);
        result.AddRange(tempArray);

        return result;
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
        var tempArray = new TArray<T>(IntPtr.Add(address, offset), propertyPtr);
        tempArray.CopyFrom(values);
    }
    #endregion
}

#region Debugger Support
/// <summary>
/// Class TArrayDebugView.
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
internal class TArrayDebugView<T>
{
    /// <summary>
    /// The array
    /// </summary>
    private readonly TArray<T>? _array;

    /// <summary>
    /// Initializes a new instance of the <see cref="TArrayDebugView{T}"/> class.
    /// </summary>
    /// <param name="array">The array.</param>
    public TArrayDebugView(TArray<T>? array)
    {
        _array = array;
    }

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <value>The items.</value>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items => _array != null ? _array.ToArray() : Array.Empty<T>();
}
#endregion

#region Extensions
/// <summary>
/// Class TArrayExtensions.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class TArrayExtensions
{
    /// <summary>
    /// Retains the specified list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list.</param>
    /// <returns>System.Nullable&lt;List&lt;T&gt;&gt;.</returns>
    public static List<T>? Retain<T>(this IList<T>? list)
    {
        return list switch
        {
            null => null,
            TArray<T> unrealArrayView => unrealArrayView.ToList(),
            List<T> csharpList => csharpList,
            _ => list.ToList()
        };
    }

    /// <summary>
    /// Adds the range.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list.</param>
    /// <param name="targets">The targets.</param>
    public static void AddRange<T>(this IList<T>? list, IEnumerable<T> targets)
    {
        if(list == null)
        {
            return;
        }

        foreach(var t in targets)
        {
            list.Add(t);
        }
    }
}
#endregion

#region Property Cache
/// <summary>
/// Struct TArrayPropertyCache
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
public struct TArrayPropertyCache<T>
{
    /// <summary>
    /// The array cache
    /// </summary>
    private TArray<T>? _arrayCache;

    /// <summary>
    /// Gets the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="propertyPtr">The property PTR.</param>
    /// <returns>System.Nullable&lt;TArray&lt;T&gt;&gt;.</returns>
    public TArray<T>? Get(IntPtr address, int offset, IntPtr propertyPtr)
    {
        if(address == IntPtr.Zero)
        {
            _arrayCache = null;
            return null;
        }

        if(_arrayCache != null)
        {
            return _arrayCache;
        }

        _arrayCache = new TArray<T>(address, offset, propertyPtr);

        return _arrayCache;
    }
}
#endregion