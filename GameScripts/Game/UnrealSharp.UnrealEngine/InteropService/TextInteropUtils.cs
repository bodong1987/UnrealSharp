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
namespace UnrealSharp.UnrealEngine.InteropService
{
    /// <summary>
    /// Class TextInteropUtils.
    /// </summary>
    public unsafe static class TextInteropUtils
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
            /// The get text c sharp marshal string from unreal text
            /// </summary>
            public readonly static IntPtr GetTextCSharpMarshalStringFromUnrealText;
            /// <summary>
            /// The get text c sharp marshal string from c sharp string
            /// </summary>
            public readonly static IntPtr GetTextCSharpMarshalStringFromCSharpString;
            /// <summary>
            /// The set unreal text from c sharp string
            /// </summary>
            public readonly static IntPtr SetUnrealTextFromCSharpString;
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

        #region Imports
        /// <summary>
        /// Gets the text c sharp string from unreal text.
        /// </summary>
        /// <param name="addressOfUnrealText">The address of unreal text.</param>
        /// <returns>string?.</returns>
        public static string? GetTextCSharpStringFromUnrealText(IntPtr addressOfUnrealText)
        {
            IntPtr ptr = ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetTextCSharpMarshalStringFromUnrealText)(addressOfUnrealText);

            return StringInteropUtils.GetStringFromNativeCharacters(ptr);
        }

        /// <summary>
        /// Gets the text c sharp string from c sharp string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>string?.</returns>
        public static string? GetTextCSharpStringFromCSharpString(string? str)
        {
            IntPtr ptr = ((delegate* unmanaged[Cdecl]<string?, IntPtr>)InteropFunctionPointers.GetTextCSharpMarshalStringFromCSharpString)(str);

            return StringInteropUtils.GetStringFromNativeCharacters(ptr);
        }

        /// <summary>
        /// Sets the unreal text from c sharp string.
        /// </summary>
        /// <param name="addressOfUnrealText">The address of unreal text.</param>
        /// <param name="str">The string.</param>
        public static void SetUnrealTextFromCSharpString(IntPtr addressOfUnrealText, string? str)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, string?, void>)InteropFunctionPointers.SetUnrealTextFromCSharpString)(addressOfUnrealText, str);
        }
        #endregion
    }
}
