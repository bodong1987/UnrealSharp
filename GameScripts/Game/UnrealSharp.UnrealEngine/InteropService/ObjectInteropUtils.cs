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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnrealSharp.Utils.Misc;

namespace UnrealSharp.UnrealEngine.InteropService
{
    /// <summary>
    /// Struct FCSharpObjectMarshalValue
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct FCSharpObjectMarshalValue
    {
        /// <summary>
        /// The value
        /// </summary>
        [FieldOffset(0)]
        public object? Value;
    }

    /// <summary>
    /// Class ObjectInteropUtils.
    /// </summary>
    public unsafe static class ObjectInteropUtils
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
            /// The get default unreal object of class
            /// </summary>
            public readonly static IntPtr GetDefaultUnrealObjectOfClass;
            /// <summary>
            /// The get c sharp object of unreal object
            /// </summary>
            public readonly static IntPtr GetCSharpObjectOfUnrealObject;
            /// <summary>
            /// The get outer of unreal object
            /// </summary>
            public readonly static IntPtr GetOuterOfUnrealObject;
            /// <summary>
            /// The get name of unreal object
            /// </summary>
            public readonly static IntPtr GetNameOfUnrealObject;
            /// <summary>
            /// The get path name of unreal object
            /// </summary>
            public readonly static IntPtr GetPathNameOfUnrealObject;
            /// <summary>
            /// The create default subobject
            /// </summary>
            public readonly static IntPtr CreateDefaultSubobject;
            /// <summary>
            /// The get default subobject by name
            /// </summary>
            public readonly static IntPtr GetDefaultSubobjectByName;

            /// <summary>
            /// Creates new unrealobject.
            /// </summary>
            public readonly static IntPtr NewUnrealObject;

            /// <summary>
            /// The duplicate unreal object
            /// </summary>
            public readonly static IntPtr DuplicateUnrealObject;

            /// <summary>
            /// The get unreal transient package
            /// </summary>
            public readonly static IntPtr GetUnrealTransientPackage;

            /// <summary>
            /// The add unreal object to root
            /// </summary>
            public readonly static IntPtr AddUnrealObjectToRoot;

            /// <summary>
            /// The remove unreal object from root
            /// </summary>
            public readonly static IntPtr RemoveUnrealObjectFromRoot;

            /// <summary>
            /// The is unreal object rooted
            /// </summary>
            public readonly static IntPtr IsUnrealObjectRooted;

            /// <summary>
            /// The is unreal object valid
            /// </summary>
            public readonly static IntPtr IsUnrealObjectValid;

            /// <summary>
            /// The find unreal object fast
            /// </summary>
            public readonly static IntPtr FindUnrealObjectFast;

            /// <summary>
            /// The find unreal object
            /// </summary>
            public readonly static IntPtr FindUnrealObject;

            /// <summary>
            /// The find unreal object checked
            /// </summary>
            public readonly static IntPtr FindUnrealObjectChecked;

            /// <summary>
            /// The find unreal object safe
            /// </summary>
            public readonly static IntPtr FindUnrealObjectSafe;

            /// <summary>
            /// The load unreal object
            /// </summary>
            public readonly static IntPtr LoadUnrealObject;

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
        /// Gets the default object pointer of class.
        /// </summary>
        /// <param name="classPtr">The class PTR.</param>
        /// <returns>nint.</returns>
        public static IntPtr GetDefaultObjectPointerOfClass(IntPtr classPtr)
        {
            var unrealObjectPtr = ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetDefaultUnrealObjectOfClass)(classPtr);

            return unrealObjectPtr;
        }

        /// <summary>
        /// Gets the default object of class.
        /// </summary>
        /// <param name="classPtr">The class PTR.</param>
        /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
        public static UObject? GetDefaultObjectOfClass(IntPtr classPtr)
        {
            var unrealObjectPtr = GetDefaultObjectPointerOfClass(classPtr);

            return GetCSharpObjectOfUnrealObject(unrealObjectPtr);
        }

        /// <summary>
        /// Gets the c# object of UObject pointer.
        /// </summary>
        /// <param name="unrealObjectPtr">The unreal object PTR.</param>
        /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
        public static UObject? GetCSharpObjectOfUnrealObject(IntPtr unrealObjectPtr)
        {
            FCSharpObjectMarshalValue value = ((delegate* unmanaged[Cdecl]<IntPtr, FCSharpObjectMarshalValue>)InteropFunctionPointers.GetCSharpObjectOfUnrealObject)(unrealObjectPtr);

            return MarshalObject(value);
        }

        /// <summary>
        /// Gets the unreal object outer.
        /// </summary>
        /// <param name="unrealObjectPtr">The unreal object PTR.</param>
        /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
        public static UObject? GetUnrealObjectOuter(IntPtr unrealObjectPtr)
        {
            var outObjectPtr = ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetOuterOfUnrealObject)(unrealObjectPtr);

            return GetCSharpObjectOfUnrealObject(outObjectPtr);
        }

        /// <summary>
        /// Gets the name of the unreal object.
        /// </summary>
        /// <param name="unrealObjectPtr">The unreal object PTR.</param>
        /// <returns>string?.</returns>
        public static string? GetUnrealObjectName(IntPtr unrealObjectPtr)
        {
            var stringPtr = ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetNameOfUnrealObject)(unrealObjectPtr);

            return StringInteropUtils.GetStringFromNativeCharacters(stringPtr);
        }

        /// <summary>
        /// Gets the path name of the unreal object.
        /// </summary>
        /// <param name="unrealObjectPtr">The unreal object PTR.</param>
        /// <returns>string?.</returns>
        public static string? GetUnrealObjectPathName(IntPtr unrealObjectPtr)
        {
            var stringPtr = ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetPathNameOfUnrealObject)(unrealObjectPtr);

            return StringInteropUtils.GetStringFromNativeCharacters(stringPtr);
        }

        /// <summary>
        /// Creates the default subobject.
        /// </summary>
        /// <param name="objectPtr">The object PTR.</param>
        /// <param name="subobjectName">Name of the subobject.</param>
        /// <param name="returnTypeClassPtr">The return type class PTR.</param>
        /// <param name="classToCreateByDefaultClassPtr">The class to create by default class PTR.</param>
        /// <param name="isRequired">The is required.</param>
        /// <param name="isTransient">The is transient.</param>
        /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
        public static UObject? CreateDefaultSubobject(
            IntPtr objectPtr, 
            string subobjectName,
            IntPtr returnTypeClassPtr,
            IntPtr classToCreateByDefaultClassPtr,
            bool isRequired,
            bool isTransient
            )
        {
            FCSharpObjectMarshalValue value = 
                ((delegate* unmanaged[Cdecl]<IntPtr, string, IntPtr, IntPtr, bool, bool, FCSharpObjectMarshalValue>)InteropFunctionPointers.CreateDefaultSubobject)
                (
                    objectPtr,
                    subobjectName,
                    returnTypeClassPtr,
                    classToCreateByDefaultClassPtr,
                    isRequired,
                    isTransient
                );

            return MarshalObject(value);
        }

        /// <summary>
        /// Gets the default subobject by name
        /// </summary>
        /// <param name="objectPtr">The object PTR.</param>
        /// <param name="nameToFind">The name to find.</param>
        /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
        public static UObject? GetDefaultSubobjectByName(IntPtr objectPtr, string nameToFind)
        {
            FCSharpObjectMarshalValue value =
               ((delegate* unmanaged[Cdecl]<IntPtr, string, FCSharpObjectMarshalValue>)InteropFunctionPointers.GetDefaultSubobjectByName)
               (
                   objectPtr,
                   nameToFind
               );

            return MarshalObject(value);
        }

        /// <summary>
        /// Creates new unrealobject.
        /// </summary>
        /// <param name="outer">The outer.</param>
        /// <param name="class">The class.</param>
        /// <param name="name">The name.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="tempalte">The tempalte.</param>
        /// <param name="copyTransientsFromClassDefaults">The copy transients from class defaults.</param>
        /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
        public static UObject? NewUnrealObject(UObject? outer, UClass @class, FName name, EObjectFlags flags, UObject? tempalte, bool copyTransientsFromClassDefaults)
        {
            Logger.Ensure<ArgumentException>(!@class.IsAbstract(), $"You can't create native abstract class:{@class.GetPathName()}", "@class");

            FCSharpObjectMarshalValue value =
                ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, FName*, EObjectFlags, IntPtr, bool, FCSharpObjectMarshalValue>)InteropFunctionPointers.NewUnrealObject)
                (
                    outer != null ? outer.GetNativePtr() : IntPtr.Zero,
                    @class.GetNativePtr(),
                    &name,
                    flags,
                    tempalte.GetNativePtrSafe(),
                    copyTransientsFromClassDefaults
                );

            return MarshalObject(value);
        }

        /// <summary>
        /// Duplicates the unreal object.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="outer">The outer.</param>
        /// <param name="name">The name.</param>
        /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
        public static UObject? DuplicateUnrealObject(UClass @class, UObject sourceObject, UObject? outer, FName name)
        {
            FCSharpObjectMarshalValue value =
                ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, FName*, FCSharpObjectMarshalValue>)InteropFunctionPointers.DuplicateUnrealObject)
                (                    
                    @class.GetNativePtr(),
                    sourceObject.GetNativePtr(),
                    outer.GetNativePtrSafe(),
                    &name
                );

            return MarshalObject(value);
        }

        /// <summary>
        /// Gets the unreal transient package.
        /// </summary>
        /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
        public static UObject? GetUnrealTransientPackage()
        {
            FCSharpObjectMarshalValue value = ((delegate* unmanaged[Cdecl]<FCSharpObjectMarshalValue>)InteropFunctionPointers.GetUnrealTransientPackage)();

            return MarshalObject(value);
        }

        /// <summary>
        /// Adds the unreal object to root.
        /// </summary>
        /// <param name="object">The object.</param>
        public static void AddUnrealObjectToRoot(UObject @object)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, void>)InteropFunctionPointers.AddUnrealObjectToRoot)(@object.GetNativePtrChecked());
        }

        /// <summary>
        /// Removes the unreal object from root.
        /// </summary>
        /// <param name="object">The object.</param>
        public static void RemoveUnrealObjectFromRoot(UObject @object)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, void>)InteropFunctionPointers.RemoveUnrealObjectFromRoot)(@object.GetNativePtrChecked());
        }

        /// <summary>
        /// Determines whether [is unreal object rooted] [the specified object].
        /// </summary>
        /// <param name="object">The object.</param>
        /// <returns>bool.</returns>
        public static bool IsUnrealObjectRooted(UObject @object)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, bool>)InteropFunctionPointers.IsUnrealObjectRooted)(@object.GetNativePtrChecked());
        }

        /// <summary>
        /// Determines whether [is unreal object valid] [the specified object].
        /// </summary>
        /// <param name="object">The object.</param>
        /// <returns>bool.</returns>
        public static bool IsUnrealObjectValid(UObject @object)
        {
            return ((delegate* unmanaged[Cdecl]<IntPtr, bool>)InteropFunctionPointers.IsUnrealObjectValid)(@object.GetNativePtrChecked());
        }

        /// <summary>
        /// Finds the object fast.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="outer">The outer.</param>
        /// <param name="name">The name.</param>
        /// <param name="bExactClass">The b exact class.</param>
        /// <param name="exclusiveFlags">The exclusive flags.</param>
        /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
        public static UObject? FindObjectFast(UClass @class, UObject? outer, FName name, bool bExactClass = false, EObjectFlags exclusiveFlags = EObjectFlags.NoFlags)
        {
            FCSharpObjectMarshalValue value = ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, FName*, bool, EObjectFlags, FCSharpObjectMarshalValue>)InteropFunctionPointers.FindUnrealObjectFast)(
                    @class.GetNativePtr(),
                    outer.GetNativePtrSafe(),
                    &name,
                    bExactClass,
                    exclusiveFlags
                );

            return MarshalObject(value);
        }

        /// <summary>
        /// Finds the object.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="outer">The outer.</param>
        /// <param name="name">The name.</param>
        /// <param name="bExactClass">The b exact class.</param>
        /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
        public static UObject? FindObject(UClass @class, UObject? outer, string name, bool bExactClass = false)
        {
            FCSharpObjectMarshalValue value = ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, string?, bool, FCSharpObjectMarshalValue>)InteropFunctionPointers.FindUnrealObject)(
                    @class.GetNativePtr(),
                    outer.GetNativePtrSafe(),
                    name,
                    bExactClass
                );

            return MarshalObject(value);
        }

        /// <summary>
        /// Finds the object checked.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="outer">The outer.</param>
        /// <param name="name">The name.</param>
        /// <param name="bExactClass">The b exact class.</param>
        /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
        public static UObject? FindObjectChecked(UClass @class, UObject? outer, string name, bool bExactClass = false)
        {
            FCSharpObjectMarshalValue value = ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, string?, bool, FCSharpObjectMarshalValue>)InteropFunctionPointers.FindUnrealObjectChecked)(
                    @class.GetNativePtr(),
                    outer.GetNativePtrSafe(),
                    name,
                    bExactClass
                );

            return MarshalObject(value);
        }

        /// <summary>
        /// Finds the object safe.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="outer">The outer.</param>
        /// <param name="name">The name.</param>
        /// <param name="bExactClass">The b exact class.</param>
        /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
        public static UObject? FindObjectSafe(UClass @class, UObject? outer, string name, bool bExactClass = false)
        {
            FCSharpObjectMarshalValue value = ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, string?, bool, FCSharpObjectMarshalValue>)InteropFunctionPointers.FindUnrealObjectSafe)(
                    @class.GetNativePtr(),
                    outer.GetNativePtrSafe(),
                    name,
                    bExactClass
                );

            return MarshalObject(value);
        }

        /// <summary>
        /// Loads the object.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="outer">The outer.</param>
        /// <param name="name">The name.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="loadFlags">The load flags.</param>
        /// <param name="sandbox">The sandbox.</param>
        /// <returns>object?.</returns>
        public static Object? LoadObject(UClass @class, UObject? outer, string name, string? fileName = null, UInt32 loadFlags = 0, UPackageMap? sandbox = null)
        {
            FCSharpObjectMarshalValue value = ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, string, string?, UInt32, IntPtr, FCSharpObjectMarshalValue>)InteropFunctionPointers.LoadUnrealObject)(
                    @class.GetNativePtr(),
                    outer.GetNativePtrSafe(),
                    name,
                    fileName,
                    loadFlags,
                    sandbox.GetNativePtrSafe()
                );

            return MarshalObject(value);
        }

        #endregion

        /// <summary>
        /// Marshals the object.
        /// </summary>
        /// <param name="marshalValue">The marshal value.</param>
        /// <returns>System.Nullable&lt;UObject&gt;.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UObject? MarshalObject(FCSharpObjectMarshalValue marshalValue)
        {
            return marshalValue.Value as UObject;
        }

        /// <summary>
        /// Marshals the object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="marshalValue">The marshal value.</param>
        /// <returns>T?.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? MarshalObject<T>(FCSharpObjectMarshalValue marshalValue) where T : UObject
        {
            return marshalValue.Value as T;
        }
    }
}
