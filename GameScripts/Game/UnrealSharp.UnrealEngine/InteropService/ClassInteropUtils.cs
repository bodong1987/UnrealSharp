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
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.UnrealEngine.InteropService
{
    /// <summary>
    /// Class ClassInteropUtils.
    /// </summary>
    public unsafe static class ClassInteropUtils
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
            /// The get c sharp default object of class
            /// </summary>
            public readonly static IntPtr GetDefaultObjectOfClass;
            /// <summary>
            /// The get class pointer of unreal object
            /// </summary>
            public readonly static IntPtr GetClassPointerOfUnrealObject;
            /// <summary>
            /// The load unreal field
            /// </summary>
            public readonly static IntPtr LoadUnrealField;
            /// <summary>
            /// The check u class is child of
            /// </summary>
            public readonly static IntPtr CheckUClassIsChildOf;
            /// <summary>
            /// The get super class
            /// </summary>
            public readonly static IntPtr GetSuperClass;
            /// <summary>
            /// The get property
            /// </summary>
            public readonly static IntPtr GetProperty;
            /// <summary>
            /// The get function
            /// </summary>
            public readonly static IntPtr GetFunction;
            /// <summary>
            /// The get structure size
            /// </summary>
            public readonly static IntPtr GetStructSize;
            /// <summary>
            /// The initialize structure
            /// </summary>
            public readonly static IntPtr InitializeStructData;
            /// <summary>
            /// The uninitialize structure
            /// </summary>
            public readonly static IntPtr UninitializeStructData;
            /// <summary>
            /// The get field c sharp full path
            /// </summary>
            public readonly static IntPtr GetFieldCSharpFullPath;

            /// <summary>
            /// The get class flags
            /// </summary>
            public readonly static IntPtr GetClassFlags;
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

        #region Usage Apis
        /// <summary>
        /// Gets the default object of class.
        /// </summary>
        /// <param name="classPtr">The class PTR.</param>
        /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
        public static UObject? GetDefaultObjectOfClass(IntPtr classPtr)
        {
            FCSharpObjectMarshalValue value = ((delegate* unmanaged[Cdecl]<IntPtr, FCSharpObjectMarshalValue>)InteropFunctionPointers.GetDefaultObjectOfClass)(classPtr);

            return ObjectInteropUtils.MarshalObject(value);
        }

        /// <summary>
        /// Gets the class pointer of unreal object.
        /// </summary>
        /// <param name="unrealObjectPtr">The unreal object PTR.</param>
        /// <returns>nint.</returns>
        public static IntPtr GetClassPointerOfUnrealObject(IntPtr unrealObjectPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetClassPointerOfUnrealObject)(unrealObjectPtr);
        }

        /// <summary>
        /// Loads the unreal field.
        /// </summary>
        /// <param name="fieldPath">The field path.</param>
        /// <returns>nint. it is UField pointer</returns>
        public static IntPtr LoadUnrealField(string fieldPath)
        {
            return ((delegate* unmanaged[Cdecl]<string, IntPtr>)InteropFunctionPointers.LoadUnrealField)(fieldPath);
        }

        /// <summary>
        /// Check UClass is child of the base UClass
        /// </summary>
        /// <param name="testClassPtr">The test class PTR.</param>
        /// <param name="testBaseClassPtr">The test base class PTR.</param>
        /// <returns>bool.</returns>
        public static bool CheckUClassIsChildOf(IntPtr testClassPtr, IntPtr testBaseClassPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, bool>)InteropFunctionPointers.CheckUClassIsChildOf)(testClassPtr, testBaseClassPtr);
        }

        /// <summary>
        /// Gets the super class.
        /// </summary>
        /// <param name="classPtr">The class PTR.</param>
        /// <returns>nint.</returns>
        public static IntPtr GetSuperClass(IntPtr classPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetSuperClass)(classPtr);
        }

        /// <summary>
        /// Gets the property of a UStruct.
        /// </summary>
        /// <param name="structPtr">The structure PTR.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>nint.</returns>
        public static IntPtr GetProperty(IntPtr structPtr, string propertyName)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, string, IntPtr>)InteropFunctionPointers.GetProperty)(structPtr, propertyName);
        }

        /// <summary>
        /// Gets UFunction Pointer of a UClass.
        /// </summary>
        /// <param name="classPtr">The class PTR.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>nint.</returns>
        public static IntPtr GetFunction(IntPtr classPtr, string functionName)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, string, IntPtr>)InteropFunctionPointers.GetFunction)(classPtr, functionName);
        }

        /// <summary>
        /// Gets the size of the UStruct.
        /// </summary>
        /// <param name="structPtr">The structure PTR.</param>
        /// <returns>int.</returns>
        public static int GetStructSize(IntPtr structPtr)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, int>)InteropFunctionPointers.GetStructSize)(structPtr);
        }

        /// <summary>
        /// Initializes the structure data based on StructData pointer.
        /// call the constructor of Struct
        /// </summary>
        /// <param name="structPtr">The structure PTR.</param>
        /// <param name="addressOfStruct">The address of structure.</param>
        public static void InitializeStructData(IntPtr structPtr, IntPtr addressOfStruct)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void>)InteropFunctionPointers.InitializeStructData)(structPtr, addressOfStruct);
        }

        /// <summary>
        /// Uninitializes the structure based on StructData pointer
        /// call the desctructor of Struct
        /// </summary>
        /// <param name="structPtr">The structure PTR.</param>
        /// <param name="addressOfStruct">The address of structure.</param>
        public static void UninitializeStructData(IntPtr structPtr, IntPtr addressOfStruct)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void>)InteropFunctionPointers.UninitializeStructData)(structPtr, addressOfStruct);
        }

        /// <summary>
        /// Gets the class flags.
        /// </summary>
        /// <param name="classPtr">The class PTR.</param>
        /// <returns>UnrealSharp.Utils.UnrealEngine.EClassFlags.</returns>
        public static EClassFlags GetClassFlags(IntPtr classPtr)
        {
            int Result = ((delegate* unmanaged[Cdecl]<IntPtr, int>)InteropFunctionPointers.GetClassFlags)(classPtr);

            return (EClassFlags)Result;
        }

        /// <summary>
        /// Gets the c# full path of native field.
        /// this path can be used to find System.Type of the proxy type of Unreal Types in C# side
        /// </summary>
        /// <param name="fieldPtr">The field PTR.</param>
        /// <returns>string?.</returns>
        public static string? GetCSharpFullPathOfNativeField(IntPtr fieldPtr)
        {
            IntPtr pathPtr = ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetFieldCSharpFullPath)(fieldPtr);

            return StringInteropUtils.GetStringFromNativeCharacters(pathPtr);
        }
        #endregion
    }
}
