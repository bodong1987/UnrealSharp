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
    /// Class SoftObjectPtrInteropUtils.
    /// </summary>
    public unsafe static class SoftObjectPtrInteropUtils
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
            /// The reset soft object PTR
            /// </summary>
            public readonly static IntPtr ResetSoftObjectPtr;
            /// <summary>
            /// The reset soft object PTR weak PTR
            /// </summary>
            public readonly static IntPtr ResetSoftObjectPtrWeakPtr;
            /// <summary>
            /// The is soft object PTR pending
            /// </summary>
            public readonly static IntPtr IsSoftObjectPtrPending;
            /// <summary>
            /// The is soft object PTR valid
            /// </summary>
            public readonly static IntPtr IsSoftObjectPtrValid;
            /// <summary>
            /// The is soft object PTR stale
            /// </summary>
            public readonly static IntPtr IsSoftObjectPtrStale;
            /// <summary>
            /// The is soft object PTR null
            /// </summary>
            public readonly static IntPtr IsSoftObjectPtrNull;
            /// <summary>
            /// The get unreal object pointer of soft object PTR
            /// </summary>
            public readonly static IntPtr GetUnrealObjectPointerOfSoftObjectPtr;
            /// <summary>
            /// The get unreal object pointer of soft object PTR ex
            /// </summary>
            public readonly static IntPtr GetUnrealObjectPointerOfSoftObjectPtrEx;
            /// <summary>
            /// The get object identifier pointer of soft object PTR
            /// </summary>
            public readonly static IntPtr GetObjectIdPointerOfSoftObjectPtr;
            /// <summary>
            /// The load synchronous soft object PTR
            /// </summary>
            public readonly static IntPtr LoadSynchronousSoftObjectPtr;
            /// <summary>
            /// The copy soft object PTR
            /// </summary>
            public readonly static IntPtr CopySoftObjectPtr;
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
        /// Resets the soft object PTR.
        /// </summary>
        /// <param name="addressOfSoftObjectPtr">The address of soft object PTR.</param>
        public static void ResetSoftObjectPtr(IntPtr addressOfSoftObjectPtr)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, void>)InteropFunctionPointers.ResetSoftObjectPtr)(addressOfSoftObjectPtr);
        }

        /// <summary>
        /// Resets the soft object PTR weak PTR.
        /// </summary>
        /// <param name="addressOfSoftObjectPtr">The address of soft object PTR.</param>
        public static void ResetSoftObjectPtrWeakPtr(IntPtr addressOfSoftObjectPtr)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, void>)InteropFunctionPointers.ResetSoftObjectPtrWeakPtr)(addressOfSoftObjectPtr);
        }

        /// <summary>
        /// check is pending
        /// </summary>
        /// <param name="addressOfSoftObjectPtr">The address of soft object PTR.</param>
        /// <returns>bool.</returns>
        public static bool IsSoftObjectPtrPending(IntPtr addressOfSoftObjectPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, bool>)InteropFunctionPointers.IsSoftObjectPtrPending)(addressOfSoftObjectPtr);
        }

        /// <summary>
        /// check is valid
        /// </summary>
        /// <param name="addressOfSoftObjectPtr">The address of soft object PTR.</param>
        /// <returns>bool.</returns>
        public static bool IsSoftObjectPtrValid(IntPtr addressOfSoftObjectPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, bool>)InteropFunctionPointers.IsSoftObjectPtrValid)(addressOfSoftObjectPtr);
        }

        /// <summary>
        /// check is stale
        /// </summary>
        /// <param name="addressOfSoftObjectPtr">The address of soft object PTR.</param>
        /// <returns>bool.</returns>
        public static bool IsSoftObjectPtrStale(IntPtr addressOfSoftObjectPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, bool>)InteropFunctionPointers.IsSoftObjectPtrStale)(addressOfSoftObjectPtr);
        }

        /// <summary>
        /// check is null
        /// </summary>
        /// <param name="addressOfSoftObjectPtr">The address of soft object PTR.</param>
        /// <returns>bool.</returns>
        public static bool IsSoftObjectPtrNull(IntPtr addressOfSoftObjectPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, bool>)InteropFunctionPointers.IsSoftObjectPtrNull)(addressOfSoftObjectPtr);
        }

        /// <summary>
        /// Get backend UObject*
        /// </summary>
        /// <param name="addressOfSoftObjectPtr">The address of soft object PTR.</param>
        /// <returns>nint.</returns>
        public static IntPtr GetUnrealObjectPointerOfSoftObjectPtr(IntPtr addressOfSoftObjectPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetUnrealObjectPointerOfSoftObjectPtr)(addressOfSoftObjectPtr);
        }

        /// <summary>
        /// Get backend UObject* with eventIfPendingKill param
        /// </summary>
        /// <param name="addressOfSoftObjectPtr">The address of soft object PTR.</param>
        /// <param name="evenIfPendingKill">The even if pending kill.</param>
        /// <returns>nint.</returns>
        public static IntPtr GetUnrealObjectPointerOfSoftObjectPtrEx(IntPtr addressOfSoftObjectPtr, bool evenIfPendingKill)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, bool, IntPtr>)InteropFunctionPointers.GetUnrealObjectPointerOfSoftObjectPtrEx)(addressOfSoftObjectPtr, evenIfPendingKill);
        }

        /// <summary>
        /// Get FSoftObjectPath ptr
        /// </summary>
        /// <param name="addressOfSoftObjectPtr">The address of soft object PTR.</param>
        /// <returns>nint.</returns>
        public static IntPtr GetObjectIdPointerOfSoftObjectPtr(IntPtr addressOfSoftObjectPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetObjectIdPointerOfSoftObjectPtr)(addressOfSoftObjectPtr);
        }

        /// <summary>
        /// Loads the synchronous soft object 
        /// </summary>
        /// <param name="addressOfSoftObjectPtr">The address of soft object PTR.</param>
        /// <returns>nint.</returns>
        public static IntPtr LoadSynchronousSoftObjectPtr(IntPtr addressOfSoftObjectPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.LoadSynchronousSoftObjectPtr)(addressOfSoftObjectPtr);
        }

        /// <summary>
        /// Copies the soft object PTR.
        /// </summary>
        /// <param name="destAddressOfSoftObjectPtr">The dest address of soft object PTR.</param>
        /// <param name="sourceAddressOfSoftObjectPtr">The source address of soft object PTR.</param>
        public static void CopySoftObjectPtr(IntPtr destAddressOfSoftObjectPtr, IntPtr sourceAddressOfSoftObjectPtr)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void>)InteropFunctionPointers.CopySoftObjectPtr)(destAddressOfSoftObjectPtr, sourceAddressOfSoftObjectPtr);
        }
    }
}
