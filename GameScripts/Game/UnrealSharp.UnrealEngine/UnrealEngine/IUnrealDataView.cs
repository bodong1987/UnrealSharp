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

namespace UnrealSharp.UnrealEngine
{
    /// <summary>
    /// Interface IUnrealDataView
    /// </summary>
    public interface IUnrealDataView
    {
        /// <summary>
        /// Gets the native PTR.
        /// </summary>
        /// <returns>IntPtr.</returns>
        IntPtr GetNativePtr();

        /// <summary>
        /// Disconnects from native.
        /// </summary>
        void DisconnectFromNative();
    }

    /// <summary>
    /// Interface IUnrealCollectionDataView
    /// </summary>
    public interface IUnrealCollectionDataView : IUnrealDataView
    {
        /// <summary>
        /// Retains this instance.
        /// </summary>
        /// <returns>IEnumerable.</returns>
        IEnumerable Retain();

        /// <summary>
        /// Copies from.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool CopyFrom(IEnumerable enumerable);
    }


    /// <summary>
    /// Interface IUnrealCollectionDataView
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IUnrealCollectionDataView<out T> : IUnrealCollectionDataView
    {
        /// <summary>
        /// Retains this instance.
        /// </summary>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        new IEnumerable<T> Retain();
    }

    /// <summary>
    /// Class UnrealDataViewExtensions.
    /// </summary>
    public static class UnrealDataViewExtensions
    {
        /// <summary>
        /// Gets the native PTR safe.
        /// </summary>
        /// <param name="unrealDataView">The unreal data view.</param>
        /// <returns>IntPtr.</returns>
        public static IntPtr GetNativePtrSafe(this IUnrealDataView? unrealDataView)
        {
            return unrealDataView != null ? unrealDataView.GetNativePtr() : IntPtr.Zero;
        }
    }

}
