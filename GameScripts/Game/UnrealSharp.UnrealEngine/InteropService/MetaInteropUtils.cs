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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.UnrealEngine.InteropService
{
    /// <summary>
    /// Class MetaInteropUtils.
    /// </summary>
    public static class MetaInteropUtils
	{
		/// <summary>
		/// Binds the property meta cache.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="structPtr">The structure PTR.</param>
		public static void BindPropertyMetaCache([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)]Type type, IntPtr structPtr)
		{
			foreach (var field in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
			{
				string fieldName = field.Name;

				if (field.FieldType == typeof(short) && fieldName.EndsWith("_Offset"))
				{
					string propertyName = fieldName.Substring(0, fieldName.Length - "_Offset".Length);

					IntPtr propertyPointer = ClassInteropUtils.GetProperty(structPtr, propertyName);

					Logger.Ensure<AccessViolationException>(propertyPointer != IntPtr.Zero, "Failed find property {0}", propertyName);

					string propertyPointerFieldName = propertyName + "_Property";
					var propertyPointerField = type.GetField(propertyPointerFieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

					if (propertyPointerField != null)
					{
						propertyPointerField.SetValue(null, propertyPointer);
					}

					int propertyOffset = PropertyInteropUtils.GetPropertyOffset(propertyPointer);

					Logger.Ensure<ArgumentException>(propertyOffset < short.MaxValue);
					Logger.Ensure<ArgumentException>(propertyOffset != -1);

					field.SetValue(null, (short)propertyOffset);
				}
			}
		}

		/// <summary>
		/// Loads the structure property meta cache.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="structPath">The structure path.</param>
		public static void LoadStructPropertyMetaCache([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type, string structPath)
		{
			IntPtr structPtr = ClassInteropUtils.LoadUnrealField(structPath);

			Logger.Ensure<AccessViolationException>(structPtr != IntPtr.Zero, "Failed load UStruct:{0}", structPath);

			BindPropertyMetaCache(type, structPtr);
		}

		/// <summary>
		/// Binds the function property meta cache.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="classPtr">The class PTR.</param>
		/// <param name="functionName">Name of the function.</param>
		public static void BindFunctionPropertyMetaCache([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type, UClass classPtr, string functionName)
		{
			IntPtr functionPtr = classPtr.FindFunction(functionName);

			Logger.Ensure<Exception>(functionPtr != IntPtr.Zero, "Failed find function:{0}", functionName);

			BindPropertyMetaCache(type, functionPtr);
		}

		/// <summary>
		/// Loads the class if need.
		/// </summary>
		/// <param name="classObject">The class object.</param>
		/// <param name="classPath">The class path.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void LoadClassIfNeed([NotNull] ref UClass? classObject, string classPath)
		{
			if (classObject != null)
			{
				return;
			}

			IntPtr classNativePtr = ClassInteropUtils.LoadUnrealField(classPath);

			Logger.Ensure<Exception>(classNativePtr != IntPtr.Zero, "Failed load class from path:{0}", classPath);

			classObject = new UClass(classNativePtr);
		}

		/// <summary>
		/// Loads the function if need.
		/// </summary>
		/// <param name="invocation">The invocation.</param>
		/// <param name="class">The class.</param>
		/// <param name="methodName">Name of the method.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void LoadFunctionIfNeed([NotNull] ref UnrealInvocation? invocation, UClass @class, string methodName)
		{
			if (invocation != null)
			{
				return;
			}

			invocation = new UnrealInvocation(@class, methodName);
		}

		/// <summary>
		/// Loads the function meta cache.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="invocation">The invocation.</param>
		/// <param name="class">The class.</param>
		/// <param name="methodName">Name of the method.</param>
		public static void LoadFunctionMetaCache([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type, ref UnrealInvocation? invocation, UClass @class, string methodName)
		{
			LoadFunctionIfNeed(ref invocation, @class, methodName);

			BindPropertyMetaCache(type, invocation.GetFunction());
		}

		#region Fast Access Struct Helpers

		/// <summary>
		/// Class TStructFastAccessSafetyCheckState.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		private class TStructFastAccessSafetyCheckState<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T> where T : struct
		{
			readonly static bool Result;

			static TStructFastAccessSafetyCheckState()
			{
				Result = CheckStructFastAccessSafety(typeof(T));
			}

			public static bool Get()
			{
				return Result;
			}
		}

		/// <summary>
		/// Checks the structure fast access safety.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		public static bool CheckStructFastAccessSafety([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type)
		{
			Logger.Ensure<Exception>(type.IsDefined<FastAccessableAttribute>(), $"Invalid Fast Access Struct: {type}, Fast access struct need define an attribute [FastAccessable].");
			Logger.Ensure<Exception>(type.StructLayoutAttribute != null && type.StructLayoutAttribute.Value != LayoutKind.Auto, $"Invalid Fast Access Struct: {type}, The fast access struct needs to explicitly specify the memory layout using StructLayoutAttribute.");

			//Logger.LogD("CheckStructFastAccessSafety:{0}", type);

			unsafe
			{
				// check struct size
				FastAccessableAttribute attr = type.GetCustomAttribute<FastAccessableAttribute>()!;

#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
				int MarshalSize = Marshal.SizeOf(type);
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

				Logger.Ensure<Exception>(attr.NativeSize == MarshalSize, "Fast access struct size missmatch, native size={0}, C# size={1}", attr.NativeSize, MarshalSize);

				// check property size and offset
				var nativeBinding = type.GetCustomAttribute<NativeBindingAttribute>();
				
				if(nativeBinding == null)
				{
					// are builtin types: FName...
					return true;
				}

				IntPtr structNativePtr = ClassInteropUtils.LoadUnrealField(nativeBinding.Path);

				Logger.Ensure<Exception>(structNativePtr != IntPtr.Zero, "Failed load UStruct from {0}", nativeBinding.Path);

				foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
				{
					Logger.Ensure<Exception>(field.FieldType.IsValueType, $"property {field.Name} of fast access struct {type.Name}({nativeBinding.Path}) has invalid offset value. property of fast access struct can only be value type.");
					
					var propertyNativePtr = ClassInteropUtils.GetProperty(structNativePtr, field.Name);

					Logger.Ensure<Exception>(propertyNativePtr != IntPtr.Zero, "Failed find native property {0} on struct {1}({2})", field.Name, type.Name, nativeBinding.Path);

					int offset = PropertyInteropUtils.GetPropertyOffset(propertyNativePtr);
					int csharpOffset = Marshal.OffsetOf(type, field.Name).ToInt32();
					Logger.Ensure<Exception>(offset == csharpOffset, $"property {field.Name} of fast access struct {type.Name}({nativeBinding.Path}) has invalid offset value. native offset = {offset}, C# offset = {csharpOffset}.");

					if (!field.FieldType.IsGenericType && field.FieldType != typeof(bool))
					{
						int nativeSize = PropertyInteropUtils.GetPropertySize(propertyNativePtr);

#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
						int csharpSize = field.FieldType.IsEnum ? Marshal.SizeOf(Enum.GetUnderlyingType(field.FieldType)) : Marshal.SizeOf(field.FieldType);
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

						Logger.Ensure<Exception>(nativeSize == csharpSize, $"property {field.Name} of fast access struct {type.Name}({nativeBinding.Path}) has invalid property size value. native size = {nativeSize}, C# size = {csharpSize}.");
					}						
				}
			}

			return true;
		}

		/// <summary>
		/// Checks the fast structure access safety.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>		
		[Conditional("DEBUG")]
		public static void CheckStructFastAccessSafety<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>() where T : struct
		{
			Logger.Assert(TStructFastAccessSafetyCheckState<T>.Get());
		}

		/// <summary>
		/// Dumps all possible fast access in assembly.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		[Conditional("DEBUG")]
		public static void DumpAllPossibleFastAccessInAssembly(Assembly assembly)
		{
			HashSet<Type> checkedTypes = new HashSet<Type>();
			HashSet<Type> fastAccessTypes = new HashSet<Type>();
			HashSet<Type> potentialTypes = new HashSet<Type>();

			// this function is only valid in debug mode, so disable them is safe

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
			foreach (var type in assembly.GetTypes())
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
			{
				// this is struct
				if (type.IsValueType && !type.IsEnum && type.IsDefined<NativeBindingAttribute>())
				{
#pragma warning disable IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
					CheckStructFastAccessPosibility(type, checkedTypes, potentialTypes, fastAccessTypes);
#pragma warning restore IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
				}
			}

			Logger.LogD("Find {0} potential fast access types, Please check the possibility of modifying it so that it can be fast access:{1}{2}", 
				potentialTypes.Count,
				Environment.NewLine,
				string.Join($"    {Environment.NewLine}    ", potentialTypes.ToList().OrderBy(x => x.FullName).Select(x=>x.ToString()))
				);
		}

		private static void CheckStructFastAccessPosibility([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type, HashSet<Type> checkedTypes, HashSet<Type> potentialTypes, HashSet<Type> fastAccessTypes)
		{
			if (checkedTypes.Contains(type))
			{
				return;
			}

			checkedTypes.Add(type);

			if (type.IsDefined<FastAccessableAttribute>())
			{
				fastAccessTypes.Add(type);
				CheckStructFastAccessSafety(type);

				return;
			}

			// check property size and offset
			var nativeBinding = type.GetCustomAttribute<NativeBindingAttribute>();

			if(nativeBinding == null)
			{
				return;
			}

			//Logger.LogD("CheckStructFastAccessPosibility:{0}", type);

#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
			int MarshalSize = Marshal.SizeOf(type);
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

			IntPtr structNativePtr = ClassInteropUtils.LoadUnrealField(nativeBinding.Path);

			Logger.Ensure<Exception>(structNativePtr != IntPtr.Zero, "Failed load UStruct from {0}", nativeBinding.Path);

			int NativeSize = ClassInteropUtils.GetStructSize(structNativePtr);

			if (NativeSize != MarshalSize)
			{
				// size miss 
				return;
			}

			// check all field size and offset
			foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
			{
				if (!field.FieldType.IsValueType)
				{
					return;
				}

				//Logger.LogD("CheckStructFastAccessPosibility:{0} field:{1}", type, field.Name);

				if (!field.FieldType.IsNumericType() && !field.FieldType.IsGenericType)
				{
					if (!checkedTypes.Contains(field.FieldType))
					{
#pragma warning disable IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
						CheckStructFastAccessPosibility(field.FieldType, checkedTypes, potentialTypes, fastAccessTypes);
#pragma warning restore IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
					}

					if (!potentialTypes.Contains(field.FieldType) && fastAccessTypes.Contains(field.FieldType))
					{
						return;
					}
				}

				var propertyNativePtr = ClassInteropUtils.GetProperty(structNativePtr, field.Name);

				// no field in native
				if (propertyNativePtr == IntPtr.Zero)
				{
					return;
				}

				int offset = PropertyInteropUtils.GetPropertyOffset(propertyNativePtr);
				int csharpOffset = Marshal.OffsetOf(type, field.Name).ToInt32();

				if (offset != csharpOffset)
				{
					return;
				}

				/*
				if(!field.FieldType.IsGenericType && field.FieldType != typeof(bool))
				{
					int nativeSize = PropertyInteropUtils.GetPropertySize(propertyNativePtr);

#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
					int csharpSize = field.FieldType.IsEnum ? Marshal.SizeOf(Enum.GetUnderlyingType(field.FieldType)) : Marshal.SizeOf(field.FieldType);
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

					if (nativeSize != csharpSize)
					{
						return;
					}
				}
				*/
			}

			potentialTypes.Add(type);
		}
		#endregion
	}
}
