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
using UnrealSharp.UnrealEngine.InteropService;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.UnrealEngine
{
    /// <summary>
    /// Class FObjectInitializer.
    /// </summary>
    [UnrealBuiltin]
    public class FObjectInitializer
    {
        /// <summary>
        /// The native PTR
        /// </summary>
        public readonly IntPtr NativePtr;

        /// <summary>
        /// Initializes a new instance of the <see cref="FObjectInitializer"/> class.
        /// </summary>
        /// <param name="nativePtr">The native PTR.</param>
        internal FObjectInitializer(IntPtr nativePtr)
        {
            NativePtr = nativePtr;
        }

        /// <summary>
        /// Gets the object.
        /// </summary>
        /// <returns>System.Nullable&lt;UObject&gt;.</returns>
        public UObject? GetObj()
        {
            Logger.Assert(NativePtr != IntPtr.Zero);

            var Obj = ObjectInitializerInteropUtils.GetObject(NativePtr);

            return Obj;
        }

        /// <summary>
        /// Gets the class.
        /// </summary>
        /// <returns>System.Nullable&lt;UClass&gt;.</returns>
        public UClass? GetClass()
        {
            Logger.Assert(NativePtr != IntPtr.Zero);

            var ClassPtr = ObjectInitializerInteropUtils.GetClass(NativePtr);

            return ClassPtr != IntPtr.Zero ? new UClass(ClassPtr) : null;
        }

        /// <summary>
        /// Creates the default subobject.
        /// </summary>
        /// <param name="outer">The outer.</param>
        /// <param name="subobjectName">Name of the subobject.</param>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="classToCreateByDefault">The class to create by default.</param>
        /// <param name="isRequired">if set to <c>true</c> [is required].</param>
        /// <param name="isTransient">if set to <c>true</c> [is transient].</param>
        /// <returns>System.Nullable&lt;UObject&gt;.</returns>
        public UObject? CreateDefaultSubobject(UObject? outer, string subobjectName, UClass? returnType, UClass? classToCreateByDefault, bool isRequired = true, bool isTransient = false)
        {
            Logger.Assert(NativePtr != IntPtr.Zero);

            return ObjectInitializerInteropUtils.CreateDefaultSubobject(
                NativePtr,
                outer != null ? outer.GetNativePtr() : IntPtr.Zero,
                subobjectName,
                returnType != null ? returnType.GetNativePtr() : IntPtr.Zero,
                classToCreateByDefault != null ? classToCreateByDefault.GetNativePtr() : IntPtr.Zero,
                isRequired,
                isTransient                
            );
        }

        /// <summary>
        /// Creates the editor only default subobject.
        /// </summary>
        /// <param name="outer">The outer.</param>
        /// <param name="subbjectName">Name of the subbject.</param>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="isTransient">if set to <c>true</c> [is transient].</param>
        /// <returns>System.Nullable&lt;UObject&gt;.</returns>
        public UObject? CreateEditorOnlyDefaultSubobject(UObject? outer, string subbjectName, UClass? returnType, bool isTransient = false)
        {
            Logger.Assert(NativePtr != IntPtr.Zero);

            return ObjectInitializerInteropUtils.CreateEditorOnlyDefaultSubobject(
                NativePtr,
                outer != null ? outer.GetNativePtr() : IntPtr.Zero,
                subbjectName,
                returnType != null ? returnType.GetNativePtr() : IntPtr.Zero,
                isTransient                
            );
        }

        /// <summary>
        /// Sets the default subobject class.
        /// </summary>
        /// <param name="subobjectName">Name of the subobject.</param>
        /// <param name="class">The class.</param>
        /// <returns>FObjectInitializer.</returns>
        public FObjectInitializer SetDefaultSubobjectClass(string subobjectName, UClass? @class)
        {
            Logger.Assert(NativePtr != IntPtr.Zero);

            ObjectInitializerInteropUtils.SetDefaultSubobjectClass(NativePtr, subobjectName, @class != null ? @class.GetNativePtr() : IntPtr.Zero);

            return this;
        }

        /// <summary>
        /// Does the not create default subobject.
        /// </summary>
        /// <param name="subobjectName">Name of the subobject.</param>
        /// <returns>FObjectInitializer.</returns>
        public FObjectInitializer DoNotCreateDefaultSubobject(string subobjectName)
        {
            Logger.Assert(NativePtr != IntPtr.Zero);

            ObjectInitializerInteropUtils.DoNotCreateDefaultSubobject(NativePtr, subobjectName);

            return this;
        }

        /// <summary>
        /// Sets the nested default subobject class.
        /// </summary>
        /// <param name="subobjectName">Name of the subobject.</param>
        /// <param name="class">The class.</param>
        /// <returns>FObjectInitializer.</returns>
        public FObjectInitializer SetNestedDefaultSubobjectClass(string subobjectName, UClass? @class)
        {
            Logger.Assert(NativePtr != IntPtr.Zero);

            ObjectInitializerInteropUtils.SetNestedDefaultSubobjectClass(NativePtr, subobjectName, @class != null ? @class.GetNativePtr() : IntPtr.Zero);

            return this;
        }

        /// <summary>
        /// Does the not create nested default subobject.
        /// </summary>
        /// <param name="subobjectName">Name of the subobject.</param>
        /// <returns>FObjectInitializer.</returns>
        public FObjectInitializer DoNotCreateNestedDefaultSubobject(string subobjectName)
        {
            Logger.Assert(NativePtr != IntPtr.Zero);

            ObjectInitializerInteropUtils.DoNotCreateNestedDefaultSubobject(NativePtr, subobjectName);

            return this;
        }
    }
}
