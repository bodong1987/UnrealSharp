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
    /// Class ArrayInteropUtils.
    /// </summary>
    public unsafe static class ArrayInteropUtils
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
            /// The get element property of array
            /// </summary>
            public readonly static IntPtr GetElementPropertyOfArray;
            /// <summary>
            /// The get length of array
            /// </summary>
            public readonly static IntPtr GetLengthOfArray;
            /// <summary>
            /// The get element address of array
            /// </summary>
            public readonly static IntPtr GetElementAddressOfArray;
            /// <summary>
            /// The clear array
            /// </summary>
            public readonly static IntPtr ClearArray;
            /// <summary>
            /// The remove at array index
            /// </summary>
            public readonly static IntPtr RemoveAtArrayIndex;
            /// <summary>
            /// The insert empty at array index
            /// </summary>
            public readonly static IntPtr InsertEmptyAtArrayIndex;
            /// <summary>
            /// The find index of array element
            /// </summary>
            public readonly static IntPtr FindIndexOfArrayElement;
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

        /// <summary>
        /// Gets the element property of array.
        /// </summary>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <returns>nint.</returns>
        public static IntPtr GetElementPropertyOfArray(IntPtr propertyPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetElementPropertyOfArray)(propertyPtr);
        }

        /// <summary>
        /// Gets the length of array.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <returns>int.</returns>
        public static int GetLengthOfArray(IntPtr addressPtr, IntPtr propertyPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int>)InteropFunctionPointers.GetLengthOfArray)(addressPtr, propertyPtr);
        }

        /// <summary>
        /// Gets the element address of array.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <param name="index">The index.</param>
        /// <returns>nint.</returns>
        public static IntPtr GetElementAddressOfArray(IntPtr addressPtr, IntPtr propertyPtr, int index)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, IntPtr>)InteropFunctionPointers.GetElementAddressOfArray)(addressPtr, propertyPtr, index);
        }

        /// <summary>
        /// Clears the array.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        public static void ClearArray(IntPtr addressPtr, IntPtr propertyPtr)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void>)InteropFunctionPointers.ClearArray)(addressPtr, propertyPtr);
        }

        /// <summary>
        /// Inserts an empty element before index.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <param name="index">The index.</param>
        /// <returns>nint.</returns>
        public static IntPtr InsertEmptyAtArrayIndex(IntPtr addressPtr, IntPtr propertyPtr, int index)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, IntPtr>)InteropFunctionPointers.InsertEmptyAtArrayIndex)(addressPtr, propertyPtr, index);
        }

        /// <summary>
        /// Removes the element at the index of this array.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <param name="index">The index.</param>
        public static void RemoveAtArrayIndex(IntPtr addressPtr, IntPtr propertyPtr, int index)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, void>)InteropFunctionPointers.RemoveAtArrayIndex)(addressPtr, propertyPtr, index);
        }

        /// <summary>
        /// Finds the index of an array element.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <param name="targetPtr">The target PTR.</param>
        /// <returns>int.</returns>
        public static int FindIndexOfArrayElement(IntPtr addressPtr, IntPtr propertyPtr, IntPtr targetPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, int>)InteropFunctionPointers.FindIndexOfArrayElement)(addressPtr, propertyPtr, targetPtr);
        }
    }
}
