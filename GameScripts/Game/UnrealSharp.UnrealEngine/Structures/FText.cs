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
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.UnrealEngine
{
    /// <summary>
    /// Struct FText
    /// </summary>
    [UnrealBuiltin]
    public struct FText
    {
        /// <summary>
        /// The text
        /// </summary>
        internal string? Text;

        /// <summary>
        /// Initializes a new instance of the <see cref="FText"/> struct.
        /// </summary>
        public FText()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FText"/> struct.
        /// </summary>
        /// <param name="text">The text.</param>
        public FText(string text)
        {
            Text = TextInteropUtils.GetTextCSharpStringFromCSharpString(text);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string? ToString()
        {
            return Text;
        }

        /// <summary>
        /// Converts to native.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToNative(IntPtr address, int offset, ref FText value)
        {
            TextInteropUtils.SetUnrealTextFromCSharpString(IntPtr.Add(address, offset), value.Text);
        }

        /// <summary>
        /// Froms the native.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>FText.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FText FromNative(IntPtr address, int offset)
        {
            FText value = new();
            value.Text = TextInteropUtils.GetTextCSharpStringFromUnrealText(IntPtr.Add(address, offset));

            return value;
        }

        /// <summary>
        /// Froms the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>FText.</returns>
        public static FText FromString(string text)
        {
            return new FText(text);
        }

        /// <summary>
        /// Performs an implicit conversion from FText to string
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string?(FText text)
        {
            return text.Text;
        }
    }
}
