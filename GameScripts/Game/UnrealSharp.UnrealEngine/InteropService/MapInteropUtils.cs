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
    /// Class MapInteropUtils.
    /// </summary>
    public unsafe static class MapInteropUtils
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
            /// The get key property of map
            /// </summary>
            public readonly static IntPtr GetKeyPropertyOfMap;
            /// <summary>
            /// The get value property of map
            /// </summary>
            public readonly static IntPtr GetValuePropertyOfMap;
            /// <summary>
            /// The get length of map
            /// </summary>
            public readonly static IntPtr GetLengthOfMap;
            /// <summary>
            /// The clear map
            /// </summary>
            public readonly static IntPtr ClearMap;
            /// <summary>
            /// The get key address of map element
            /// </summary>
            public readonly static IntPtr GetKeyAddressOfMapElement;
            /// <summary>
            /// The get value address of map element
            /// </summary>
            public readonly static IntPtr GetValueAddressOfMapElement;
            /// <summary>
            /// The get address of map element
            /// </summary>
            public readonly static IntPtr GetAddressOfMapElement;
            /// <summary>
            /// The find value address of element key
            /// </summary>
            public readonly static IntPtr FindValueAddressOfElementKey;
            /// <summary>
            /// The try add new element to map
            /// </summary>
            public readonly static IntPtr TryAddNewElementToMap;
            /// <summary>
            /// The remove element from map
            /// </summary>
            public readonly static IntPtr RemoveElementFromMap;
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
        /// Gets the key property of map.
        /// </summary>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <returns>nint.</returns>
        public static IntPtr GetKeyPropertyOfMap(IntPtr propertyPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetKeyPropertyOfMap)(propertyPtr);
        }

        /// <summary>
        /// Gets the value property of map.
        /// </summary>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <returns>nint.</returns>
        public static IntPtr GetValuePropertyOfMap(IntPtr propertyPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetValuePropertyOfMap)(propertyPtr);
        }

        /// <summary>
        /// Gets the length of map.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <returns>int.</returns>
        public static int GetLengthOfMap(IntPtr addressPtr, IntPtr propertyPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int>)InteropFunctionPointers.GetLengthOfMap)(addressPtr, propertyPtr);
        }

        /// <summary>
        /// Clears the map.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        public static void ClearMap(IntPtr addressPtr, IntPtr propertyPtr)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void>)InteropFunctionPointers.ClearMap)(addressPtr, propertyPtr);
        }

        /// <summary>
        /// Gets the key address of map element.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <param name="index">The index.</param>
        /// <returns>nint.</returns>
        public static IntPtr GetKeyAddressOfMapElement(IntPtr addressPtr, IntPtr propertyPtr, int index)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, IntPtr>)InteropFunctionPointers.GetKeyAddressOfMapElement)(addressPtr, propertyPtr, index);
        }

        /// <summary>
        /// Gets the value address of map element.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <param name="index">The index.</param>
        /// <returns>nint.</returns>
        public static IntPtr GetValueAddressOfMapElement(IntPtr addressPtr, IntPtr propertyPtr, int index)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, IntPtr>)InteropFunctionPointers.GetValueAddressOfMapElement)(addressPtr, propertyPtr, index);
        }

        /// <summary>
        /// Struct FMapKeyValueAddressPair
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct FMapKeyValueAddressPair
        {
            /// <summary>
            /// The key pointer
            /// </summary>
            public IntPtr KeyPointer;

            /// <summary>
            /// The value pointer
            /// </summary>
            public IntPtr ValuePointer;
        }

        /// <summary>
        /// Gets the address of map element.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <param name="index">The index.</param>
        /// <returns>UnrealSharp.UnrealEngine.InteropService.MapInteropUtils.FMapKeyValueAddressPair.</returns>
        public static FMapKeyValueAddressPair GetAddressOfMapElement(IntPtr addressPtr, IntPtr propertyPtr, int index)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, FMapKeyValueAddressPair>)InteropFunctionPointers.GetAddressOfMapElement)(addressPtr, propertyPtr, index);
        }

        /// <summary>
        /// Finds the value address of element key.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <param name="keyPtr">The key PTR.</param>
        /// <returns>nint.</returns>
        public static IntPtr FindValueAddressOfElementKey(IntPtr addressPtr, IntPtr propertyPtr, IntPtr keyPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, IntPtr>)InteropFunctionPointers.FindValueAddressOfElementKey)(addressPtr, propertyPtr, keyPtr);
        }

        /// <summary>
        /// Tries the add new element to map.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <param name="keyPtr">The key PTR.</param>
        /// <param name="valuePtr">The value PTR.</param>
        /// <param name="overrideIfExists">The override if exists.</param>
        /// <returns>bool.</returns>
        public static bool TryAddNewElementToMap(IntPtr addressPtr, IntPtr propertyPtr, IntPtr keyPtr, IntPtr valuePtr, bool overrideIfExists)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, IntPtr, bool, bool>)InteropFunctionPointers.TryAddNewElementToMap)(addressPtr, propertyPtr, keyPtr, valuePtr, overrideIfExists);
        }

        /// <summary>
        /// Removes the element from map.
        /// </summary>
        /// <param name="addressPtr">The address PTR.</param>
        /// <param name="propertyPtr">The property PTR.</param>
        /// <param name="keyPtr">The key PTR.</param>
        /// <returns>bool.</returns>
        public static bool RemoveElementFromMap(IntPtr addressPtr, IntPtr propertyPtr, IntPtr keyPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, bool>)InteropFunctionPointers.RemoveElementFromMap)(addressPtr, propertyPtr, keyPtr);
        }
    }
}
