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

using System.Runtime.InteropServices;

namespace UnrealSharp.UnrealEngine.Main
{
    /// <summary>
    /// Struct FUnrealInteropFunctionsInfo
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FUnrealInteropFunctionsInfo
    {
        /// <summary>
        /// The size of this
        /// </summary>
        public int SizeOfThis;

        /// <summary>
        /// The native interop functions PTR
        /// </summary>
        public void* NativeInteropFunctionsPtr;

        /// <summary>
        /// The get unreal interop function pointer pointer
        /// </summary>
        public void* GetUnrealInteropFunctionPointerPointer;

        /// <summary>
        /// The log message function pointer
        /// </summary>
        public void* LogMessageFunctionPointer;

        /// <summary>
        /// The unreal major version
        /// </summary>
        public int UnrealMajorVersion;

        /// <summary>
        /// The unreal minor version
        /// </summary>
        public int UnrealMinorVersion;

        /// <summary>
        /// The unreal patch version
        /// </summary>
        public int UnrealPatchVersion;
    }
}
