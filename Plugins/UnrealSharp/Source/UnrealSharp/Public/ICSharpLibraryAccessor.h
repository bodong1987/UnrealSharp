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
#pragma once

namespace UnrealSharp
{
	/*
	* Used to invoke C# code from C++
	* Provides entry points for calling some commonly used C# methods on the C++ side for easy use.
	*/
	class UNREALSHARP_API ICSharpLibraryAccessor
	{
	public:
		virtual ~ICSharpLibraryAccessor() = default;

	public:
		// release C# object link to UObject*
		virtual void						BreakCSharpObjectNativeConnection(void* InCSharpObject) = 0;

		// get UObject* of C# UObject
		virtual UObject*					GetUnrealObject(void* InCSharpObject) = 0;

		// Invoke before a C# UCLASS object constructed
		virtual void						BeforeObjectConstructor(void* InCSharpObject, const FObjectInitializer& InObjectInitializer) = 0;

		// invoke post a C# UCLASS object constructed
		virtual void						PostObjectConstructor(void* InCSharpObject) = 0;		

		// create a C# struct by UScriptStruct*
		virtual void*						CreateCSharpStruct(const void* InUnrealStructPtr, const UScriptStruct* InStruct) = 0;

		// copy C# struct values to Unreal struct
		virtual void						StructToNative(const UScriptStruct* InStruct, void* InNativePtr, const void* InCSharpStructPtr) = 0;

		// Create C# collection 
		// TArray/TSet/TMap
		virtual void*						CreateCSharpCollection(void* InAddressOfCollection, FProperty* InCollectionProperty) = 0;

		// copy C# collection to Unreal collection
		virtual void						CopyFromCSharpCollection(void* InAddressOfCollection, FProperty* InCollectionProperty, void* InCSharpCollection) = 0;		

		// create C# TSoftObjectPtr
		virtual void*						CreateCSharpSoftObjectPtr(void* InAddressOfSoftObjectPtr, FSoftObjectProperty* InSoftObjectProperty) = 0;

		// Copy C# TSoftObjectPtr to Unreal
		virtual void						CopySoftObjectPtr(void* InDestinationAddress, const void* InSourceObjectInterface) = 0;

		// create C# TSoftClassPtr
		virtual void*						CreateCSharpSoftClassPtr(void* InAddressOfSoftClassPtr, FSoftClassProperty* InSoftClassProperty) = 0;

		// create C# TSoftClassPtr to Unreal
		virtual void						CopySoftClassPtr(void* InDestinationAddress, const void* InSourceObjectInterface) = 0;
	};
}