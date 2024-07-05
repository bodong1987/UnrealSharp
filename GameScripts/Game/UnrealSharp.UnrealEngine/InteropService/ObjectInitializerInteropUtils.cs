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
namespace UnrealSharp.UnrealEngine.InteropService;

/// <summary>
/// Class ObjectInitializerInteropUtils.
/// </summary>
public static unsafe class ObjectInitializerInteropUtils
{
    #region Interop Function Pointers     
    /// <summary>
    /// Class InteropFunctionPointers
    /// Since mono does not support setting delegate* unmanaged type fields directly through reflection,
    /// Therefore we cannot directly declare delegate* unmanaged fields and set them through reflection
    /// So we use this method to set it indirectly, first save the external function pointer to these IntPtr,
    /// and then solve it through forced type conversion when calling.Although this is a bit inconvenient,
    /// there is currently no other way unless Mono supports it in the future.
    /// ReSharper disable once CommentTypo
    /// @reference check here: https://github.com/dotnet/runtime/blob/main/src/mono/mono/metadata/icall.c#L2134  ves_icall_RuntimeFieldInfo_SetValueInternal
    /// </summary>
    private static class InteropFunctionPointers
    {
#pragma warning disable CS0649 // The compiler detected an uninitialized private or internal field declaration that is never assigned a value. [We use reflection to bind all fields of this class]
        /// <summary>
        /// The get class of object initializer
        /// </summary>
        public static readonly IntPtr GetClassOfObjectInitializer;
        /// <summary>
        /// The get object of object initializer
        /// </summary>
        public static readonly IntPtr GetObjectOfObjectInitializer;
        /// <summary>
        /// create default subobject of object initializer
        /// </summary>
        public static readonly IntPtr CreateDefaultSubobjectOfObjectInitializer;
        /// <summary>
        /// create editor only default subobject of object initializer
        /// </summary>
        public static readonly IntPtr CreateEditorOnlyDefaultSubobjectOfObjectInitializer;
        /// <summary>
        /// The set default subobject class of object initializer
        /// </summary>
        public static readonly IntPtr SetDefaultSubobjectClassOfObjectInitializer;
        /// <summary>
        /// do not create default subobject of object initializer
        /// </summary>
        public static readonly IntPtr DoNotCreateDefaultSubobjectOfObjectInitializer;
        /// <summary>
        /// The set nested default subobject class of object initializer
        /// </summary>
        public static readonly IntPtr SetNestedDefaultSubobjectClassOfObjectInitializer;
        /// <summary>
        /// do not create nested default subobject of object initializer
        /// </summary>
        public static readonly IntPtr DoNotCreateNestedDefaultSubobjectOfObjectInitializer;

#pragma warning restore CS0649

        /// <summary>
        /// static constructor
        /// </summary>
        static InteropFunctionPointers()
        {
            InteropFunctions.BindInteropFunctionPointers(typeof(InteropFunctionPointers));
        }
    }
    #endregion

    /// <summary>
    /// Gets the class.
    /// </summary>
    /// <param name="objectInitializerPtr">The object initializer PTR.</param>
    /// <returns>nint.</returns>
    public static IntPtr GetClass(IntPtr objectInitializerPtr)
    {
        return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetClassOfObjectInitializer)(objectInitializerPtr);
    }

    /// <summary>
    /// Gets the object.
    /// </summary>
    /// <param name="objectInitializerPtr">The object initializer PTR.</param>
    /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
    public static UObject? GetObject(IntPtr objectInitializerPtr)
    {
        var value = ((delegate* unmanaged[Cdecl]<IntPtr, FCSharpObjectMarshalValue>)InteropFunctionPointers.GetObjectOfObjectInitializer)(objectInitializerPtr);

        return ObjectInteropUtils.MarshalObject(value);
    }

    /// <summary>
    /// Creates the default subobject.
    /// </summary>
    /// <param name="objectInitializerPtr">The object initializer PTR.</param>
    /// <param name="outerPtr">The outer PTR.</param>
    /// <param name="subobjectName">Name of the subobject.</param>
    /// <param name="returnTypeClassPtr">The return type class PTR.</param>
    /// <param name="classToCreateByDefaultPtr">The class to create by default PTR.</param>
    /// <param name="isRequired">The is required.</param>
    /// <param name="isTransient">The is transient.</param>
    /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
    public static UObject? CreateDefaultSubobject(
        IntPtr objectInitializerPtr,
        IntPtr outerPtr,
        string subobjectName,
        IntPtr returnTypeClassPtr,
        IntPtr classToCreateByDefaultPtr,
        bool isRequired,
        bool isTransient
    )
    {
        var value =
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, string, IntPtr, IntPtr, bool, bool, FCSharpObjectMarshalValue>)InteropFunctionPointers.CreateDefaultSubobjectOfObjectInitializer)
            (
                objectInitializerPtr,
                outerPtr,
                subobjectName,
                returnTypeClassPtr,
                classToCreateByDefaultPtr,
                isRequired,
                isTransient
            );

        return ObjectInteropUtils.MarshalObject(value);
    }

    /// <summary>
    /// Creates the editor only default subobject.
    /// </summary>
    /// <param name="objectInitializerPtr">The object initializer PTR.</param>
    /// <param name="outerPtr">The outer PTR.</param>
    /// <param name="subobjectName">Name of the subobject.</param>
    /// <param name="returnTypeClassPtr">The return type class PTR.</param>
    /// <param name="isTransient">The is transient.</param>
    /// <returns>UnrealSharp.UnrealEngine.UObject?.</returns>
    public static UObject? CreateEditorOnlyDefaultSubobject(
        IntPtr objectInitializerPtr,
        IntPtr outerPtr,
        string subobjectName,
        IntPtr returnTypeClassPtr,
        bool isTransient
    )
    {
        var value =
            ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, string, IntPtr, bool, FCSharpObjectMarshalValue>)InteropFunctionPointers.CreateEditorOnlyDefaultSubobjectOfObjectInitializer)
            (
                objectInitializerPtr,
                outerPtr,
                subobjectName,
                returnTypeClassPtr,
                isTransient
            );

        return ObjectInteropUtils.MarshalObject(value);
    }

    /// <summary>
    /// Sets the default subobject class.
    /// </summary>
    /// <param name="objectInitializerPtr">The object initializer PTR.</param>
    /// <param name="subobjectName">Name of the subobject.</param>
    /// <param name="classPtr">The class PTR.</param>
    public static void SetDefaultSubobjectClass(IntPtr objectInitializerPtr, string subobjectName, IntPtr classPtr)
    {
        ((delegate* unmanaged[Cdecl]<IntPtr, string, IntPtr, void>)InteropFunctionPointers.SetDefaultSubobjectClassOfObjectInitializer)
        (
            objectInitializerPtr,
            subobjectName,
            classPtr
        );
    }

    /// <summary>
    /// Does the not create default subobject.
    /// </summary>
    /// <param name="objectInitializerPtr">The object initializer PTR.</param>
    /// <param name="subobjectName">Name of the subobject.</param>
    public static void DoNotCreateDefaultSubobject(IntPtr objectInitializerPtr, string subobjectName)
    {
        ((delegate* unmanaged[Cdecl]<IntPtr, string, void>)InteropFunctionPointers.DoNotCreateDefaultSubobjectOfObjectInitializer)
        (
            objectInitializerPtr,
            subobjectName
        );
    }

    /// <summary>
    /// Sets the nested default subobject class.
    /// </summary>
    /// <param name="objectInitializerPtr">The object initializer PTR.</param>
    /// <param name="subobjectName">Name of the subobject.</param>
    /// <param name="classPtr">The class PTR.</param>
    public static void SetNestedDefaultSubobjectClass(IntPtr objectInitializerPtr, string subobjectName, IntPtr classPtr)
    {
        ((delegate* unmanaged[Cdecl]<IntPtr, string, IntPtr, void>)InteropFunctionPointers.SetNestedDefaultSubobjectClassOfObjectInitializer)
        (
            objectInitializerPtr,
            subobjectName,
            classPtr
        );
    }

    /// <summary>
    /// Does the not create nested default subobject.
    /// </summary>
    /// <param name="objectInitializerPtr">The object initializer PTR.</param>
    /// <param name="subobjectName">Name of the subobject.</param>
    public static void DoNotCreateNestedDefaultSubobject(IntPtr objectInitializerPtr, string subobjectName)
    {
        ((delegate* unmanaged[Cdecl]<IntPtr, string, void>)InteropFunctionPointers.DoNotCreateNestedDefaultSubobjectOfObjectInitializer)
        (
            objectInitializerPtr,
            subobjectName
        );
    }
}