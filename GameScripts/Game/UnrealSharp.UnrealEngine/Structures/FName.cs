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
using System.Runtime.InteropServices;
using UnrealSharp.UnrealEngine.InteropService;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.UnrealEngine
{
    /// <summary>
    /// Struct FName
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [FastAccessable(FName.SturctureSize)]
    [UnrealBuiltin]
    public struct FName
    {
        /// <summary>
        /// The comparison index
        /// </summary>
        internal uint ComparisonIndex;
        
        /// <summary>
        /// The number
        /// </summary>
        internal int Number;

#if WITH_EDITOR
        /// <summary>
        /// The display index
        /// </summary>
        internal uint DisplayIndex;
#endif

        /// <summary>
        /// The name none
        /// </summary>
        public static readonly FName NAME_None = new FName();

        /// <summary>
        /// The sturcture size
        /// </summary>
#if WITH_EDITOR
        public const int SturctureSize = 12;
#else
        public const int SturctureSize = 8;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="FName"/> struct.
        /// </summary>
        public FName()
        {
            unsafe
            {
                Logger.Assert(sizeof(FName) == SturctureSize);
            }            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FName"/> struct.
        /// </summary>
        /// <param name="text">The text.</param>
        public FName(string text)
        {
            unsafe
            {
                Logger.Assert(sizeof(FName) == SturctureSize);
            }

            this = NameInteropUtils.GetNameOfString(text);
        }

        /// <summary>
        /// Converts to unstableint.
        /// </summary>
        /// <returns>UInt64.</returns>
        public UInt64 ToUnstableInt()
        {
            return ((UInt64)ComparisonIndex << 32) | (UInt64)(uint)Number;
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(FName left, FName right)
        {
            return left.ToUnstableInt() == right.ToUnstableInt();
        }

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(FName left, FName right)
        {
            return left.ToUnstableInt() != right.ToUnstableInt();
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
        public bool IsValid()
        {
            return !IsNone();
        }

        /// <summary>
        /// Determines whether this instance is none.
        /// </summary>
        /// <returns><c>true</c> if this instance is none; otherwise, <c>false</c>.</returns>
        public bool IsNone()
        {
            return ComparisonIndex == 0 && Number == 0;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return ToUnstableInt().GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if(obj is FName fn)
            {
                return this == fn;
            }

            return false;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string? ToString()
        {
            return NameInteropUtils.GetStringOfName(this);
        }

        /// <summary>
        /// Froms the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>FName.</returns>
        public static FName FromString(string? text)
        {
            return text != null ? new FName(text) : FName.NAME_None;
        }

        /// <summary>
        /// Froms the native.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>FName.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FName FromNative(IntPtr address, int offset)
        {
            return InteropUtils.GetStructUnsafe<FName>(address, offset);
        }

        /// <summary>
        /// Converts to native.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToNative(IntPtr address, int offset, ref FName value)
        {
            InteropUtils.SetStructUnsafe(address, offset, ref value);
        }
    }
}
