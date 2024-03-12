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
    /// Class DelegateInteropUtils.
    /// </summary>
    public unsafe static class DelegateInteropUtils
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
            /// The bind delegate
            /// </summary>
            public readonly static IntPtr BindDelegate;
            /// <summary>
            /// The unbind delegate
            /// </summary>
            public readonly static IntPtr UnbindDelegate;
            /// <summary>
            /// The add delegate
            /// </summary>
            public readonly static IntPtr AddDelegate;
            /// <summary>
            /// The remove delegate
            /// </summary>
            public readonly static IntPtr RemoveDelegate;
            /// <summary>
            /// The remove all delegate
            /// </summary>
            public readonly static IntPtr RemoveAllDelegate;
            /// <summary>
            /// The clear delegate
            /// </summary>
            public readonly static IntPtr ClearDelegate;
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
        /// Binds the delegate.
        /// </summary>
        /// <param name="addressOfDelegate">The address of delegate.</param>
        /// <param name="addressOfProperty">The address of property.</param>
        /// <param name="objectPtr">The object PTR.</param>
        /// <param name="methodName">Name of the method.</param>
        public static void BindDelegate(IntPtr addressOfDelegate, IntPtr addressOfProperty, IntPtr objectPtr, string methodName)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, string, void>)InteropFunctionPointers.BindDelegate)(addressOfDelegate, addressOfProperty, objectPtr, methodName);
        }

        /// <summary>
        /// Unbinds the delegate.
        /// </summary>
        /// <param name="addressOfDelegate">The address of delegate.</param>
        /// <param name="addressOfProperty">The address of property.</param>
        public static void UnbindDelegate(IntPtr addressOfDelegate, IntPtr addressOfProperty)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void>)InteropFunctionPointers.UnbindDelegate)(addressOfDelegate, addressOfProperty);
        }

        /// <summary>
        /// Adds the delegate.
        /// </summary>
        /// <param name="addressOfDelegate">The address of delegate.</param>
        /// <param name="addressOfProperty">The address of property.</param>
        /// <param name="objectPtr">The object PTR.</param>
        /// <param name="methodName">Name of the method.</param>
        public static void AddDelegate(IntPtr addressOfDelegate, IntPtr addressOfProperty, IntPtr objectPtr, string methodName)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, string, void>)InteropFunctionPointers.AddDelegate)(addressOfDelegate, addressOfProperty, objectPtr, methodName);
        }

        /// <summary>
        /// Removes the delegate.
        /// </summary>
        /// <param name="addressOfDelegate">The address of delegate.</param>
        /// <param name="addressOfProperty">The address of property.</param>
        /// <param name="objectPtr">The object PTR.</param>
        /// <param name="methodName">Name of the method.</param>
        public static void RemoveDelegate(IntPtr addressOfDelegate, IntPtr addressOfProperty, IntPtr objectPtr, string methodName)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, string, void>)InteropFunctionPointers.RemoveDelegate)(addressOfDelegate, addressOfProperty, objectPtr, methodName);
        }

        /// <summary>
        /// Removes all delegate.
        /// </summary>
        /// <param name="addressOfDelegate">The address of delegate.</param>
        /// <param name="addressOfProperty">The address of property.</param>
        /// <param name="objectPtr">The object PTR.</param>
        public static void RemoveAllDelegate(IntPtr addressOfDelegate, IntPtr addressOfProperty, IntPtr objectPtr)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void>)InteropFunctionPointers.RemoveAllDelegate)(addressOfDelegate, addressOfProperty, objectPtr);
        }

        /// <summary>
        /// Clears the delegate.
        /// </summary>
        /// <param name="addressOfDelegate">The address of delegate.</param>
        /// <param name="addressOfProperty">The address of property.</param>
        public static void ClearDelegate(IntPtr addressOfDelegate, IntPtr addressOfProperty)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void>)InteropFunctionPointers.ClearDelegate)(addressOfDelegate, addressOfProperty);
        }
    }
}
