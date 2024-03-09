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
using UnrealSharp.UnrealEngine.InteropService;
using UnrealSharp.Utils.Misc;

namespace UnrealSharp.UnrealEngine
{
    /// <summary>
    /// Struct FStringView
    /// Implements the <see cref="UnrealSharp.UnrealEngine.IUnrealDataView" />
    /// </summary>
    /// <seealso cref="UnrealSharp.UnrealEngine.IUnrealDataView" />
    public struct FStringView : IUnrealDataView
    {
        /// <summary>
        /// The native PTR
        /// </summary>
        IntPtr NativePtr;

        /// <summary>
        /// Initializes a new instance of the <see cref="FStringView"/> struct.
        /// </summary>
        /// <param name="nativePtr">The native PTR.</param>
        public FStringView(IntPtr nativePtr)
        {
            NativePtr = nativePtr;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string? ToString()
        {
            return StringInteropUtils.GetStringFromUnrealString(NativePtr);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return NativePtr.ToInt32();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is FStringView fs && fs.NativePtr == NativePtr;
        }

        /// <summary>
        /// Froms the string.
        /// </summary>
        /// <param name="value">The value.</param>
        public void FromString(string? value)
        {
            Logger.Assert(NativePtr != IntPtr.Zero);
            StringInteropUtils.SetUnrealString(NativePtr, value);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty => NativePtr != IntPtr.Zero ? StringInteropUtils.GetUnrealStringLength(NativePtr) <= 0 : false;

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>The length.</value>
        public int Length => NativePtr != IntPtr.Zero ? StringInteropUtils.GetUnrealStringLength(NativePtr) : 0;

        /// <summary>
        /// Converts to name.
        /// </summary>
        /// <returns>FName.</returns>
        public FName ToName()
        {
            var text = ToString();
            return text != null ? new FName(text) : new FName();
        }

        /// <summary>
        /// Converts to native.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToNative(IntPtr address, int offset, ref FStringView value)
        {
            if(address != IntPtr.Zero && value.NativePtr != address && value.NativePtr != IntPtr.Zero)
            {
                StringInteropUtils.CopyUnrealString(IntPtr.Add(address, offset), value.NativePtr);
            }
        }

        /// <summary>
        /// Froms the native.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>FStringView.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FStringView FromNative(IntPtr address, int offset)
        {
            return new FStringView(IntPtr.Add(address, offset));
        }

        /// <summary>
        /// Gets the native PTR.
        /// </summary>
        /// <returns>IntPtr.</returns>
        public nint GetNativePtr()
		{
            return NativePtr;
		}

        /// <summary>
        /// Disconnects from native.
        /// </summary>
        public void DisconnectFromNative()
		{
            NativePtr = IntPtr.Zero;
		}
	}
}
