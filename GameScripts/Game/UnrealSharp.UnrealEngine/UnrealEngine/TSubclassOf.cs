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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using UnrealSharp.UnrealEngine.InteropService;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.UnrealEngine
{
    /// <summary>
    /// class TSubclassOf
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnrealBuiltin]
    [FastAccessable(8)] // only support 64bit 
    public struct TSubclassOf<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T> where T : class, IUObjectInterface
    {
        private IntPtr NativeClassPtr;

        /// <summary>
        /// The base class
        /// </summary>
        public readonly static UClass? ModelClass;

        /// <summary>
        /// Initializes static members of the <see cref="TSubclassOf{T}"/> struct.
        /// </summary>
        static TSubclassOf()
        {
            if(typeof(T).IsInterface)
            {
                string? bindingPath = string.Empty;
                var bindingAttribute = typeof(T).GetCustomAttribute<NativeBindingAttribute>();

                bindingPath = bindingAttribute?.Path ?? string.Empty;

                if(bindingPath.IsNullOrEmpty())
                {
                    var blueprintBindingAttr = typeof(T).GetCustomAttribute<BlueprintBindingAttribute>();

                    if(blueprintBindingAttr != null)
                    {
                        bindingPath = blueprintBindingAttr.Path;
                    }
                }

                Logger.Ensure<Exception>(bindingPath.IsNotNullOrEmpty(), $"Failed get binding path for type:{typeof(T)}");

                ModelClass = UClass.LoadClass(bindingPath);

                Logger.EnsureNotNull(ModelClass, "Failed load interface class from path:{0}", bindingPath);
            }
            else
            {
                var method = typeof(T).GetMethod("StaticClass", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

                Logger.EnsureNotNull(method, "Failed find StaticClass on type {0}", typeof(T).FullName!);

                ModelClass = (method!.Invoke(null, null) as UClass)!;

				Logger.EnsureNotNull(ModelClass, "Invalid class return by {0}.StaticClass", typeof(T).FullName!);
			}
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TSubclassOf{T}"/> struct.
        /// </summary>
        public TSubclassOf()
        {
			CheckStructSize();
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="TSubclassOf{T}"/> struct.
        /// </summary>
        /// <param name="classPtr">The class PTR.</param>
        public TSubclassOf(IntPtr classPtr)
        {
			CheckStructSize();

			NativeClassPtr = classPtr;

            CheckSubclassValidation(NativeClassPtr);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TSubclassOf{T}"/> struct.
        /// </summary>
        /// <param name="class">The class.</param>
        public TSubclassOf(UClass? @class)
        {
            CheckStructSize();

			NativeClassPtr = @class != null ? @class.GetNativePtr() : IntPtr.Zero;

            CheckSubclassValidation(NativeClassPtr);
        }

        [Conditional("DEBUG")]
        private void CheckStructSize()
        {
			unsafe
			{
				Logger.Assert(sizeof(TSubclassOf<T>) == 8, "sizeof(TSubclassOf<T>) must be 8");
			}
		}

        /// <summary>
        /// Checks the subclass validation.
        /// </summary>
        /// <param name="nativeClassPtr">The native class PTR.</param>
        private void CheckSubclassValidation(IntPtr nativeClassPtr)
        {
            if(NativeClassPtr == IntPtr.Zero)
            {
                return;
            }

            bool bIsChildOf = ClassInteropUtils.CheckUClassIsChildOf(nativeClassPtr, ModelClass!.GetNativePtr());

            Logger.Ensure<Exception>(bIsChildOf, "Invalid construct of {0}, target type is not valid.", GetType().FullName!);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="UClass"/> to <see cref="TSubclassOf{T}"/>.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator TSubclassOf<T>(UClass? @class)
        {
            return new TSubclassOf<T> { NativeClassPtr = @class != null ? @class.GetNativePtr() : IntPtr.Zero };
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="TSubclassOf{T}"/> to <see cref="System.Nullable{UClass}"/>.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator UClass?(TSubclassOf<T> @class)
        {
            return @class.NativeClassPtr != IntPtr.Zero ? new UClass(@class.NativeClassPtr) : null;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="TSubclassOf{T}"/> to <see cref="System.Boolean"/>.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator bool(TSubclassOf<T> @class)
        {
            return @class.IsValid();
        }

		/// <summary>
		/// Implements the == operator.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(TSubclassOf<T> left, TSubclassOf<T> right)
		{
            return left.GetNativePtr() == right.GetNativePtr();
		}

		/// <summary>
		/// Implements the != operator.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(TSubclassOf<T> left, TSubclassOf<T> right)
		{
			return left.GetNativePtr() != right.GetNativePtr();
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
		public override int GetHashCode()
		{
            return NativeClassPtr.GetHashCode();
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
		public override bool Equals([NotNullWhen(true)] object? obj)
		{
            return obj is IUnrealDataView dv && dv.GetNativePtr() == NativeClassPtr;
		}

		/// <summary>
		/// Returns true if ... is valid.
		/// </summary>
		/// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
		public bool IsValid()
        {
            return NativeClassPtr != IntPtr.Zero;
        }

        /// <summary>
        /// Gets the native PTR.
        /// </summary>
        /// <returns>IntPtr.</returns>
        public IntPtr GetNativePtr()
        {
            return NativeClassPtr;
        }

        /// <summary>
        /// Gets the class.
        /// </summary>
        /// <returns>System.Nullable&lt;UClass&gt;.</returns>
        public UClass? GetClass()
        {
            return NativeClassPtr != IntPtr.Zero ? new UClass(NativeClassPtr) : null;
        }

        /// <summary>
        /// Gets the default object.
        /// </summary>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        public T? GetDefaultObject()
        {
            return IsValid() ? ClassInteropUtils.GetDefaultObjectOfClass(GetNativePtr()) as T : null;
        }

        /// <summary>
        /// Gets the super class.
        /// </summary>
        /// <returns>System.Nullable&lt;UClass&gt;.</returns>
        public UClass? GetSuperClass()
        {
            IntPtr SuperPtr = ClassInteropUtils.GetClassPointerOfUnrealObject(GetNativePtr());

            return SuperPtr != IntPtr.Zero ? new UClass(SuperPtr) : null;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public string? GetName()
        {
            if (NativeClassPtr == IntPtr.Zero)
            {
                return null;
            }

            return ObjectInteropUtils.GetUnrealObjectName(NativeClassPtr);
        }

        /// <summary>
        /// Gets the name of the path.
        /// </summary>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public string? GetPathName()
        {
            if (NativeClassPtr == IntPtr.Zero)
            {
                return null;
            }

            return ObjectInteropUtils.GetUnrealObjectPathName(NativeClassPtr);
        }
    }
}
