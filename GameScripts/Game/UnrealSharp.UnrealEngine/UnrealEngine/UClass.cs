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
using System.Diagnostics.CodeAnalysis;
using UnrealSharp.UnrealEngine.InteropService;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.UnrealEngine
{
    /// <summary>
    /// Class UClass.
    /// Implements the <see cref="UnrealSharp.UnrealEngine.UObject" />
    /// </summary>
    /// <seealso cref="UnrealSharp.UnrealEngine.UObject" />
    [NativeBinding("Class", "UClass", "/Script/CoreUObject.Class")]
	public class UClass : UStruct
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="UClass"/> class.
        /// </summary>
        /// <param name="nativePtr">The native PTR.</param>
        public UClass(IntPtr nativePtr) :
			base(nativePtr)
		{
		}

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string? ToString()
        {
            return GetName();
        }

        /// <summary>
        /// Gets the class flags.
        /// </summary>
        /// <returns>EClassFlags.</returns>
        public EClassFlags GetClassFlags()
        {
            return ClassInteropUtils.GetClassFlags(GetNativePtr());
        }

        /// <summary>
        /// Determines whether this instance is abstract.
        /// </summary>
        /// <returns><c>true</c> if this instance is abstract; otherwise, <c>false</c>.</returns>
        public bool IsAbstract()
        {
            EClassFlags Flags = GetClassFlags();

            return Flags.HasFlag(EClassFlags.Abstract);
        }

        /// <summary>
        /// Gets the default object.
        /// </summary>
        /// <returns>System.Nullable&lt;UObject&gt;.</returns>
        public UObject? GetDefaultObject()
		{
			return GetNativePtr() != IntPtr.Zero ? ClassInteropUtils.GetDefaultObjectOfClass(GetNativePtr()) : null;
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
		/// Finds the function.
		/// </summary>
		/// <param name="functionName">Name of the function.</param>
		/// <returns>IntPtr.</returns>
		public IntPtr FindFunction(string functionName)
        {
            if(!IsBindingToUnreal)
            {
                return IntPtr.Zero;
            }

            return ClassInteropUtils.GetFunction(GetNativePtr(), functionName);
        }

        /// <summary>
        /// Determines whether [is child of] [the specified parent class].
        /// </summary>
        /// <param name="parentClass">The parent class.</param>
        /// <returns><c>true</c> if [is child of] [the specified parent class]; otherwise, <c>false</c>.</returns>
        public bool IsChildOf(UClass? parentClass)
		{
			if(parentClass == null)
			{
				return false;
			}

			return ClassInteropUtils.CheckUClassIsChildOf(GetNativePtr(), parentClass.GetNativePtr());
		}

		/// <summary>
		/// Implements the == operator.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(UClass? left, UClass? right)
		{
			return left.GetNativePtrSafe() == right.GetNativePtrSafe();
		}

		/// <summary>
		/// Implements the != operator.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(UClass? left, UClass? right)
		{
			return left.GetNativePtrSafe() != right.GetNativePtrSafe();
		}


		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
		public override bool Equals(object? obj)
		{
			return obj is UClass u && u.GetNativePtr() == GetNativePtr();
		}

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
		{
			return GetNativePtr().GetHashCode();
		}

        /// <summary>
        /// Gets the class of.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>System.Nullable&lt;UClass&gt;.</returns>
        public static UClass GetClassOf<T>() where T : UObject
		{
            return TUClassOf<T>.Class;
        }

        /// <summary>
        /// Gets the t sub class of.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>TSubclassOf&lt;T&gt;.</returns>
        public static TSubclassOf<T> GetTSubClassOf<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>() where T : UObject
		{
			var @class = GetClassOf<T>();

			return new TSubclassOf<T>(@class);
		}

        /// <summary>
        /// Loads the class.
        /// </summary>
        /// <param name="classPath">The class path.</param>
        /// <returns>System.Nullable&lt;UClass&gt;.</returns>
        public static UClass? LoadClass(string classPath)
		{
			IntPtr classPtr = ClassInteropUtils.LoadUnrealField(classPath);

			return classPtr != IntPtr.Zero ? new UClass(classPtr) : null;
        }
    }

    #region TUClassOf
    /// <summary>
    /// Class TUClassOf.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class TUClassOf<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T> where T : UObject
    {
        /// <summary>
        /// The class
        /// </summary>
        public readonly static UClass Class;

        /// <summary>
        /// Initializes static members of the <see cref="TUClassOf{T}"/> class.
        /// </summary>
        static TUClassOf()
        {
            var StaticClassMethod = typeof(T).GetMethod("StaticClass", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            Logger.EnsureNotNull(StaticClassMethod, "Failed get StaticClass on type:{0}", typeof(T).FullName!);

            var Result = StaticClassMethod.Invoke(null, null);

            Logger.EnsureNotNull(Result, "Failed get StaticClass on type:{0}", typeof(T).FullName!);
            Class = (Result! as UClass)!;
        }
    }
    #endregion
}
