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
using System.Runtime.CompilerServices;
using UnrealSharp.UnrealEngine.InteropService;
using UnrealSharp.Utils.Misc;

namespace UnrealSharp.UnrealEngine
{
    /// <summary>
    /// Class IntelligentCacheUtils.
    /// </summary>
    public static class IntelligentCacheUtils
    {
        /// <summary>
        /// Delegate ConstructDelegateType
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        public delegate T? ConstructDelegateType<T>(IntPtr address);

        /// <summary>
        /// Gets the address based value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cachedAddress">The cached address.</param>
        /// <param name="cachedValue">The cached value.</param>
        /// <param name="address">The address.</param>
        /// <param name="constructor">The constructor.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        public static T? GetAddressBasedValue<T>(ref IntPtr cachedAddress, ref T? cachedValue, IntPtr address, ConstructDelegateType<T> constructor)
        {
            if (address == IntPtr.Zero)
            {
                cachedAddress = default;
                cachedValue = default;
                return default;
            }

            if (address != cachedAddress)
            {
                cachedAddress = address;

                cachedValue = constructor(cachedAddress);
            }

            return cachedValue;
        }

        /// <summary>
        /// Gets the address based value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cachedAddress">The cached address.</param>
        /// <param name="cachedValue">The cached value.</param>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="constructor">The constructor.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        public static T? GetAddressBasedValue<T>(ref IntPtr cachedAddress, ref T? cachedValue, IntPtr address, int offset, ConstructDelegateType<T> constructor)
        {
            if (address == IntPtr.Zero)
            {
                cachedAddress = default;
                cachedValue = default;
                return default;
            }

            IntPtr TargetAddress = IntPtr.Add(address, offset);

            if (TargetAddress == IntPtr.Zero)
            {
                cachedAddress = default;
                cachedValue = default;
                return default;
            }

            if (TargetAddress != cachedAddress)
            {
                cachedAddress = TargetAddress;

                cachedValue = constructor(cachedAddress);
            }

            return cachedValue;
        }

        /// <summary>
        /// Gets the pointer based value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cachedPointer">The cached pointer.</param>
        /// <param name="cachedValue">The cached value.</param>
        /// <param name="address">The address.</param>
        /// <param name="constructor">The constructor.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        public static T? GetPointerBasedValue<T>(ref IntPtr cachedPointer, ref T? cachedValue, IntPtr address, ConstructDelegateType<T> constructor)
        {
            if (address == IntPtr.Zero)
            {
                cachedPointer = default;
                cachedValue = default;
                return default;
            }

            IntPtr CurrentPointer = IntPtr.Zero;
            unsafe
            {
                CurrentPointer = *(IntPtr*)address;
            }

            if (CurrentPointer != cachedPointer)
            {
                cachedPointer = CurrentPointer;

                cachedValue = constructor(address);
            }

            return cachedValue;
        }

        /// <summary>
        /// Gets the pointer based value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cachedPointer">The cached pointer.</param>
        /// <param name="cachedValue">The cached value.</param>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="constructor">The constructor.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        public static T? GetPointerBasedValue<T>(ref IntPtr cachedPointer, ref T? cachedValue, IntPtr address, int offset, ConstructDelegateType<T> constructor)
        {
            if (address == IntPtr.Zero)
            {
                cachedValue = default;
                cachedPointer = default;
                return default;
            }

            IntPtr TargetAddress = IntPtr.Add(address, offset);

            if (TargetAddress == IntPtr.Zero)
            {
                cachedValue = default;
                cachedPointer = default;
                return default;
            }

            IntPtr CurrentPointer = IntPtr.Zero;
            unsafe
            {
                CurrentPointer = *(IntPtr*)TargetAddress;
            }

            if (CurrentPointer != cachedPointer)
            {
                cachedPointer = CurrentPointer;

                cachedValue = constructor(TargetAddress);
            }

            return cachedValue;
        }
    }

    /// <summary>
    /// Struct TAddressBasedIntelligentCache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct TAddressBasedIntelligentCache<T>
    {
        /// <summary>
        /// The cached address
        /// </summary>
        private IntPtr CachedAddress;
        /// <summary>
        /// The cached value
        /// </summary>
        private T? CachedValue;

        /// <summary>
        /// The constructor
        /// </summary>
        private IntelligentCacheUtils.ConstructDelegateType<T> Constructor;

        /// <summary>
        /// Initializes a new instance of the <see cref="TAddressBasedIntelligentCache{T}" /> struct.
        /// </summary>
        /// <param name="constructDelegate">The construct delegate.</param>
        public TAddressBasedIntelligentCache(IntelligentCacheUtils.ConstructDelegateType<T> constructDelegate)
        {
            Constructor = constructDelegate;
        }

        /// <summary>
        /// Gets the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Get(IntPtr address)
        {
            return IntelligentCacheUtils.GetAddressBasedValue<T>(ref CachedAddress, ref CachedValue, address, Constructor);
        }

        /// <summary>
        /// Gets the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Get(IntPtr address, int offset)
        {
            return IntelligentCacheUtils.GetAddressBasedValue<T>(ref CachedAddress, ref CachedValue, address, offset, Constructor);
        }
    }

    /// <summary>
    /// Struct TGenericAddressBasedIntelligentCache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct TGenericAddressBasedIntelligentCache<T>
    {
        /// <summary>
        /// The cached address
        /// </summary>
        private IntPtr CachedAddress;
        /// <summary>
        /// The cached value
        /// </summary>
        private T? CachedValue;

        /// <summary>
        /// The interop policy
        /// </summary>
        public static readonly IInteropPolicy<T> InteropPolicy = InteropPolicyFactory.GetPolicy<T>();

        private static IntelligentCacheUtils.ConstructDelegateType<T> Constructor;

        /// <summary>
        /// Initializes static members of the <see cref="TGenericAddressBasedIntelligentCache{T}" /> struct.
        /// </summary>
        static TGenericAddressBasedIntelligentCache()
        {
            Logger.EnsureNotNull(InteropPolicy);

            Constructor = (a) => InteropPolicy.Read(a);
        }

        /// <summary>
        /// Gets the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Get(IntPtr address)
        {
            return IntelligentCacheUtils.GetAddressBasedValue<T>(ref CachedAddress, ref CachedValue, address, Constructor);
        }

        /// <summary>
        /// Gets the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Get(IntPtr address, int offset)
        {
            return IntelligentCacheUtils.GetAddressBasedValue<T>(ref CachedAddress, ref CachedValue, address, offset, Constructor);
        }
    }

    /// <summary>
    /// Struct TPointerBasedIntelligentCache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct TPointerBasedIntelligentCache<T>
    {
        /// <summary>
        /// The cached pointer
        /// </summary>
        private IntPtr CachedPointer;
        /// <summary>
        /// The cached value
        /// </summary>
        private T? CachedValue;

        /// <summary>
        /// The constructor
        /// </summary>
        private IntelligentCacheUtils.ConstructDelegateType<T> Constructor;

        /// <summary>
        /// Initializes a new instance of the <see cref="TPointerBasedIntelligentCache{T}" /> struct.
        /// </summary>
        /// <param name="constructDelegate">The construct delegate.</param>
        public TPointerBasedIntelligentCache(IntelligentCacheUtils.ConstructDelegateType<T> constructDelegate)
        {
            Constructor = constructDelegate;
        }

        /// <summary>
        /// Gets the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Get(IntPtr address)
        {
            return IntelligentCacheUtils.GetPointerBasedValue<T>(ref CachedPointer, ref CachedValue, address, Constructor);
        }

        /// <summary>
        /// Gets the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Get(IntPtr address, int offset)
        {
            return IntelligentCacheUtils.GetPointerBasedValue<T>(ref CachedPointer, ref CachedValue, address, offset, Constructor);
        }
    }

    /// <summary>
    /// Struct TGenericPointerBasedIntelligentCache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct TGenericPointerBasedIntelligentCache<T>
    {
        /// <summary>
        /// The cached pointer
        /// </summary>
        private IntPtr CachedPointer;
        /// <summary>
        /// The cached value
        /// </summary>
        private T? CachedValue;

        /// <summary>
        /// The interop policy
        /// </summary>
        public static readonly IInteropPolicy<T> InteropPolicy = InteropPolicyFactory.GetPolicy<T>();

        private static IntelligentCacheUtils.ConstructDelegateType<T> Constructor;

        /// <summary>
        /// Initializes static members of the <see cref="TGenericPointerBasedIntelligentCache{T}" /> struct.
        /// </summary>
        static TGenericPointerBasedIntelligentCache()
        {
            Logger.EnsureNotNull(InteropPolicy);

            Constructor = (a) => InteropPolicy.Read(a);
        }
                
        /// <summary>
        /// Gets the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Get(IntPtr address)
        {
            return IntelligentCacheUtils.GetPointerBasedValue<T>(ref CachedPointer, ref CachedValue, address, Constructor);
        }

        /// <summary>
        /// Gets the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Get(IntPtr address, int offset)
        {
            return IntelligentCacheUtils.GetPointerBasedValue<T>(ref CachedPointer, ref CachedValue, address, offset, Constructor);
        }
    }
}
