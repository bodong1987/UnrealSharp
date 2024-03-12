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

namespace UnrealSharp.UnrealEngine.InteropService
{
    /// <summary>
    /// Class StringInteropUtils.
    /// </summary>
    public unsafe static class StringInteropUtils
    {
        #region Interop Function Pointers     
        /// <summary>
        /// Class InteropFunctionPointers.
        /// Since mono does not support setting delegate* unamaged type fields directly through reflection,
        /// Therefore we cannot directly declare delegate* unmanged fields and set them through reflection
        /// So we use this method to set it indirectly, first save the external function pointer to these IntPtrs,
        /// and then solve it through forced type conversion when calling.Although this is a bit inconvenient,
        /// there is currently no other way unless Mono supports it in the future.
        /// @reference check here: https://github.com/dotnet/runtime/blob/main/src/mono/mono/metadata/icall.c#L2134  ves_icall_RuntimeFieldInfo_SetValueInternal
        /// </summary>
        private static class InteropFunctionPointers
        {
#pragma warning disable CS0649 // The compiler detected an uninitialized private or internal field declaration that is never assigned a value. [We use reflection to bind all fields of this class]
            /// <summary>
            /// The get c sharp marshal string
            /// </summary>
            public readonly static IntPtr GetCSharpMarshalString;
            /// <summary>
            /// The set unreal string
            /// </summary>
            public readonly static IntPtr SetUnrealString;
            /// <summary>
            /// The get unreal string length
            /// </summary>
            public readonly static IntPtr GetUnrealStringLength;
            /// <summary>
            /// The copy unreal string
            /// </summary>
            public readonly static IntPtr CopyUnrealString;
#pragma warning restore CS0649

            /// <summary>
            /// Cctors this instance.
            /// </summary>
            static InteropFunctionPointers()
            {
                InteropFunctions.BindInteropFunctionPointers(typeof(InteropFunctionPointers));
            }
        }
        #endregion

        #region Usage Methods
        /// <summary>
        /// Gets the string from native characters.
        /// </summary>
        /// <param name="addressOfCharacters">The address of characters.</param>
        /// <returns>string?.</returns>
        public static string? GetStringFromNativeCharacters(IntPtr addressOfCharacters)
        {
            if (addressOfCharacters == IntPtr.Zero)
            {
                return null;
            }

            return Marshal.PtrToStringUni(addressOfCharacters);
        }

        /// <summary>
        /// Gets the C# string from unreal string.
        /// </summary>
        /// <param name="addressOfFString">The address of f string.</param>
        /// <returns>string.</returns>
        public static string GetStringFromUnrealString(IntPtr addressOfFString)
        {
            return GetStringFromNativeCharacters(((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetCSharpMarshalString)(addressOfFString))!;
        }

        /// <summary>
        /// Sets the unreal string from a C# string
        /// </summary>
        /// <param name="addressOfFString">The address of f string.</param>
        /// <param name="str">The string.</param>
        public static void SetUnrealString(IntPtr addressOfFString, string? str)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, string?, void>)InteropFunctionPointers.SetUnrealString)(addressOfFString, str);
        }

        /// <summary>
        /// Gets the length of the unreal string.
        /// </summary>
        /// <param name="addressOfFString">The address of f string.</param>
        /// <returns>int.</returns>
        public static int GetUnrealStringLength(IntPtr addressOfFString)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, int>)InteropFunctionPointers.GetUnrealStringLength)(addressOfFString);
        }

        /// <summary>
        /// Copies the unreal string.
        /// </summary>
        /// <param name="addressOfTargetFString">The address of target f string.</param>
        /// <param name="addressOfSourceFString">The address of source f string.</param>
        public static void CopyUnrealString(IntPtr addressOfTargetFString, IntPtr addressOfSourceFString)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void>)InteropFunctionPointers.CopyUnrealString)(addressOfTargetFString, addressOfSourceFString);
        }
        #endregion
    }
}
