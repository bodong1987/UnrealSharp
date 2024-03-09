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
    /// Class NameInteropUtils.
    /// </summary>
    public unsafe static class NameInteropUtils
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
            /// The get string of name
            /// </summary>
            public readonly static IntPtr GetStringOfName;
            /// <summary>
            /// The get name of string
            /// </summary>
            public readonly static IntPtr GetNameOfString;

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
        /// convert FName to string
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>string?.</returns>
        public static string? GetStringOfName(FName name)
        {
            var ptr = ((delegate* unmanaged[Cdecl]<FName*, IntPtr>)InteropFunctionPointers.GetStringOfName)(&name);

            return StringInteropUtils.GetStringFromNativeCharacters(ptr);
        }

        /// <summary>
        /// Gets the name from string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>UnrealSharp.UnrealEngine.FName.</returns>
        public static FName GetNameOfString(string? str)
        {
            FName name = ((delegate* unmanaged[Cdecl]<string?, FName>)InteropFunctionPointers.GetNameOfString)(str);

            return name;
        }        
		#endregion
	}
}
