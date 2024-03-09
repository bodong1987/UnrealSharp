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

namespace UnrealSharp.UnrealEngine.Collections
{
    /// <summary>
    /// Class TMap.
    /// Please note that this container is just a View.
    /// Saving it outside the binding class is dangerous.
    /// If you want to do such an operation for any purpose, you need to understand the principle behind it.
    /// </summary>
    /// <typeparam name="TKey">The type of the t key.</typeparam>
    /// <typeparam name="TValue">The type of the t value.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(TMapDebugView<,>))]
    public class TMap<TKey, TValue> : IDictionary<TKey, TValue>, IUnrealCollectionDataView<KeyValuePair<TKey, TValue>>
        where TKey : notnull
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
        /// Gets the key property PTR.
        /// </summary>
        /// <value>The key property PTR.</value>
        public IntPtr KeyPropertyPtr { get; private set; }

        /// <summary>
        /// Gets the value property PTR.
        /// </summary>
        /// <value>The value property PTR.</value>
        public IntPtr ValuePropertyPtr { get; private set; }

        /// <summary>
        /// The key interop policy
        /// </summary>
        private static readonly IInteropPolicy<TKey> KeyInteropPolicy = InteropPolicyFactory.GetPolicy<TKey>();
        /// <summary>
        /// The value interop policy
        /// </summary>
        private static readonly IInteropPolicy<TValue> ValueInteropPolicy = InteropPolicyFactory.GetPolicy<TValue>();

        /// <summary>
        /// The key property size
        /// </summary>
#if !DEBUG
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        public readonly int KeyPropertySize;

        /// <summary>
        /// The value property size
        /// </summary>
#if !DEBUG
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        public readonly int ValuePropertySize;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="TMap{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        public TMap(IntPtr addressPtr, IntPtr propertyPtr)
        {
            AddressPtr = addressPtr;
            PropertyPtr = propertyPtr;

            KeyPropertyPtr = MapInteropUtils.GetKeyPropertyOfMap(PropertyPtr);
            ValuePropertyPtr = MapInteropUtils.GetValuePropertyOfMap(PropertyPtr);

            Logger.Ensure<Exception>(KeyPropertyPtr != IntPtr.Zero);
            Logger.Ensure<Exception>(ValuePropertyPtr != IntPtr.Zero);

            KeyPropertySize = PropertyInteropUtils.GetPropertySize(KeyPropertyPtr);
            Logger.Ensure<Exception>(KeyPropertySize > 0, "Get property element size of {0} is {1}", GetType().FullName!, KeyPropertySize);

            ValuePropertySize = PropertyInteropUtils.GetPropertySize(ValuePropertyPtr);
            Logger.Ensure<Exception>(ValuePropertySize > 0, "Get property element size of {0} is {1}", GetType().FullName!, ValuePropertySize);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TMap{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        public TMap(IntPtr addressPtr, int offset, IntPtr propertyPtr) :
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
            return HashCode.Combine(AddressPtr.GetHashCode(), PropertyPtr.GetHashCode());
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is TMap<TKey, TValue> map)
            {
                return AddressPtr == map.AddressPtr && PropertyPtr == map.PropertyPtr;
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

        #region Enumerable Interfaces
        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>The length.</value>
        public int Count
        {
            get
            {
                if (AddressPtr == IntPtr.Zero || PropertyPtr == IntPtr.Zero)
                {
                    return 0;
                }

                return MapInteropUtils.GetLengthOfMap(AddressPtr, PropertyPtr);
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>IEnumerator&lt;KeyValuePair&lt;TKey, TValue&gt;&gt;.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for(int i=0; i<Count; ++i)
            {
                var address = MapInteropUtils.GetAddressOfMapElement(AddressPtr, PropertyPtr, i);
                IntPtr keyAddress = address.KeyPointer;
                IntPtr valueAddress = address.ValuePointer;
				                                
                TKey key = KeyInteropPolicy.Read(keyAddress);
                TValue value = ValueInteropPolicy.Read(valueAddress);

                yield return new KeyValuePair<TKey, TValue>(key, value);
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
        #endregion

        #region IDictionary Interfaces
        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <value>The keys.</value>
        public ICollection<TKey> Keys
		{
			get
			{
				List<TKey> list = new List<TKey>();
				foreach(var item in this)
				{
					list.Add(item.Key);
				}

				return list;
			}
		}

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <value>The values.</value>
        public ICollection<TValue> Values
		{
			get
			{
				List<TValue> list = new List<TValue>();
				foreach(var item in this)
				{
					list.Add(item.Value);
				}

				return list;
			}
		}

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>TValue.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
        public TValue this[TKey key] 
		{ 
			get
			{
				if(TryGetValue(key, out var value))
				{
					return value;
				}

				throw new KeyNotFoundException();
			}
			set
			{
				AddOrSet(key, value);
			}
		}

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
		{
            unsafe
            {
                byte* bytes = stackalloc byte[KeyPropertySize];
                using ScopedPropertyTempVariable variable = new ScopedPropertyTempVariable(KeyPropertyPtr, bytes);

                KeyInteropPolicy.Write(variable.VariablePtr, key);

                var ResultAddress = MapInteropUtils.FindValueAddressOfElementKey(AddressPtr, PropertyPtr, variable.VariablePtr);

                if (ResultAddress == IntPtr.Zero)
                {
                    value = default(TValue);
                    return false;
                }

                value = ValueInteropPolicy.Read(ResultAddress);
            }            

			return true;
		}

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Remove(TKey key)
		{
            unsafe
            {
                byte* bytes = stackalloc byte[KeyPropertySize];
                using ScopedPropertyTempVariable variable = new ScopedPropertyTempVariable(KeyPropertyPtr, bytes);

                KeyInteropPolicy.Write(variable.VariablePtr, key);

                return MapInteropUtils.RemoveElementFromMap(AddressPtr, PropertyPtr, variable.VariablePtr);
            }            
		}

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
		{
			MapInteropUtils.ClearMap(AddressPtr, PropertyPtr);
		}

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool TryAdd(TKey key, TValue value)
		{
            unsafe
            {
                byte* keyBytes = stackalloc byte[KeyPropertySize];
                byte* valueBytes = stackalloc byte[ValuePropertySize];

                using ScopedPropertyTempVariable KeyVariable = new ScopedPropertyTempVariable(KeyPropertyPtr, keyBytes);
                using ScopedPropertyTempVariable ValueVariable = new ScopedPropertyTempVariable(ValuePropertyPtr, valueBytes);

                KeyInteropPolicy.Write(KeyVariable.VariablePtr, key);
                ValueInteropPolicy.Write(ValueVariable.VariablePtr, value);

                return MapInteropUtils.TryAddNewElementToMap(AddressPtr, PropertyPtr, KeyVariable.VariablePtr, ValueVariable.VariablePtr, false);
            }           
		}

        /// <summary>
        /// Adds the or set.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void AddOrSet(TKey key, TValue value)
		{
            unsafe
            {
                byte* keyBytes = stackalloc byte[KeyPropertySize];
                byte* valueBytes = stackalloc byte[ValuePropertySize];

                using ScopedPropertyTempVariable KeyVariable = new ScopedPropertyTempVariable(KeyPropertyPtr, keyBytes);
                using ScopedPropertyTempVariable ValueVariable = new ScopedPropertyTempVariable(ValuePropertyPtr, valueBytes);

                KeyInteropPolicy.Write(KeyVariable.VariablePtr, key);
                ValueInteropPolicy.Write(ValueVariable.VariablePtr, value);

                MapInteropUtils.TryAddNewElementToMap(AddressPtr, PropertyPtr, KeyVariable.VariablePtr, ValueVariable.VariablePtr, true);
            }            
		}

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="System.ArgumentException">An item with the same key has already been added.</exception>
        public void Add(TKey key, TValue value)
		{
			if (ContainsKey(key))
			{
				throw new ArgumentException("An item with the same key has already been added.");
			}

			TryAdd(key, value);
		}

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <returns><see langword="true" /> if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, <see langword="false" />.</returns>
        public bool ContainsKey(TKey key)
		{
            unsafe
            {
                byte* bytes = stackalloc byte[KeyPropertySize];
                using ScopedPropertyTempVariable variable = new ScopedPropertyTempVariable(KeyPropertyPtr, bytes);

                KeyInteropPolicy.Write(variable.VariablePtr, key);

                var ResultAddress = MapInteropUtils.FindValueAddressOfElementKey(AddressPtr, PropertyPtr, variable.VariablePtr);

                return ResultAddress != IntPtr.Zero;
            }            
		}

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns><see langword="true" /> if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
		{
			TryGetValue(item.Key, out var result);

			return result != null && result.Equals(item.Value);
		}

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns><see langword="true" /> if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			if (TryGetValue(item.Key, out var result) && result != null && result.Equals(item))
			{
				Remove(item.Key);
				return true;
			}

			return false;
		}

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="System.ArgumentNullException">array</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">arrayIndex</exception>
        /// <exception cref="System.ArgumentException">The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array.</exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			}

			if (array.Length - arrayIndex < Count)
			{
				throw new ArgumentException("The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array.");
			}

			int i = 0;
			foreach (var item in this)
			{
				array[arrayIndex + i] = item;
				++i;
			}
		}

        #endregion

        #region Interop
        /// <summary>
        /// Copies from.
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyFrom(IEnumerable<KeyValuePair<TKey, TValue>>? source)
        {
            if (source is TMap<TKey, TValue> sourceMap && sourceMap.Equals(this))
            {
                return;
            }

            Clear();

            if(source != null)
            {
                foreach (var i in source)
                {
                    TryAdd(i.Key, i.Value);
                }
            }            
        }

        /// <summary>
        /// Copies from.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CopyFrom(IEnumerable enumerable)
        {
            if (enumerable is IEnumerable<KeyValuePair<TKey, TValue>> e)
            {
                CopyFrom(e);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts to dictionary.
        /// </summary>
        /// <returns>Dictionary&lt;TKey, TValue&gt;.</returns>
        public Dictionary<TKey, TValue> ToDictionary()
        {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

            foreach(var p in this)
            {
                dict.Add(p.Key, p.Value);
            }

            return dict;
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
        public IEnumerable<KeyValuePair<TKey, TValue>> Retain()
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
        /// Froms the native.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <returns>Dictionary&lt;TKey, TValue&gt;.</returns>
        public static Dictionary<TKey, TValue> FromNative(IntPtr address, int offset, IntPtr propertyPtr)
		{
			TMap<TKey, TValue> TempMap = new TMap<TKey, TValue>(IntPtr.Add(address, offset), propertyPtr);

			return TempMap.ToDictionary();
		}

        /// <summary>
        /// Converts to native.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <param name="values">The values.</param>
        public static void ToNative(IntPtr address, int offset, IntPtr propertyPtr, IEnumerable<KeyValuePair<TKey, TValue>>? values)
		{
            TMap<TKey, TValue> TempMap = new TMap<TKey, TValue>(IntPtr.Add(address, offset), propertyPtr);

			TempMap.CopyFrom(values);
        }
        #endregion
    }

    #region Debugger Support
    /// <summary>
    /// Class TMapDebugView.
    /// </summary>
    /// <typeparam name="TKey">The type of the t key.</typeparam>
    /// <typeparam name="TValue">The type of the t value.</typeparam>
    class TMapDebugView<TKey, TValue> where TKey : notnull
    {
        /// <summary>
        /// The map
        /// </summary>
        readonly TMap<TKey, TValue>? Map;

        /// <summary>
        /// Initializes a new instance of the <see cref="TMapDebugView{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="map">The map.</param>
        public TMapDebugView(TMap<TKey, TValue>? map)
        {
            Map = map;
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>The items.</value>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<TKey,TValue>[] Items
        {
            get { return Map != null ? Map.ToArray() : new KeyValuePair<TKey, TValue>[] { }; }
        }
    }
    #endregion

    #region Extensions
    /// <summary>
    /// Class TMapExtensions.
    /// </summary>
    public static class TMapExtensions
	{
        /// <summary>
        /// Retains the specified dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the t key.</typeparam>
        /// <typeparam name="TValue">The type of the t value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>System.Nullable&lt;Dictionary&lt;TKey, TValue&gt;&gt;.</returns>
        public static Dictionary<TKey, TValue>? Retain<TKey,TValue>(this IDictionary<TKey, TValue>? dictionary) where TKey : notnull
		{
			if(dictionary == null)
			{
				return null;
			}

			if(dictionary is Dictionary<TKey, TValue> tdict)
			{
				return tdict;
			}
			else if(dictionary is TMap<TKey, TValue> tdictView)
			{
				return tdictView.ToDictionary();
			}

			return dictionary.ToDictionary(x=>x.Key, x=>x.Value);
		}
	}
    #endregion

    #region Property Cache
    /// <summary>
    /// Struct TMapPropertyCache
    /// </summary>
    /// <typeparam name="TKey">The type of the t key.</typeparam>
    /// <typeparam name="TValue">The type of the t value.</typeparam>
    public struct TMapPropertyCache<TKey, TValue>
		where TKey:notnull
    {
        /// <summary>
        /// The map cache
        /// </summary>
        private TMap<TKey, TValue>? MapCache;

        /// <summary>
        /// Gets the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <returns>System.Nullable&lt;TMap&lt;TKey, TValue&gt;&gt;.</returns>
        public TMap<TKey, TValue>? Get(IntPtr address, int offset, IntPtr propertyPtr)
        {
            if (address == IntPtr.Zero)
            {
                MapCache = null;
                return null;
            }

            if (MapCache != null)
            {
                return MapCache;
            }

            MapCache = new TMap<TKey, TValue>(address, offset, propertyPtr);

            return MapCache;
        }
    }
    #endregion

}
