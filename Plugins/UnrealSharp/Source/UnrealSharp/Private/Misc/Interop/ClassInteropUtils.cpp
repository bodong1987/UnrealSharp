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
#include "Misc/InteropUtils.h"
#include "Misc/UnrealSharpUtils.h"
#include "Classes/CSharpStruct.h"

namespace UnrealSharp
{
	FCSharpObjectMarshalValue FInteropUtils::GetDefaultObjectOfClass(const UClass* InClass)
	{
		if (InClass == nullptr)
		{
			return { nullptr };
		}

		return GetCSharpObjectOfUnrealObject(InClass->GetDefaultObject());
	}

	const UClass* FInteropUtils::GetClassPointerOfUnrealObject(const UObject* InObject)
	{
		return InObject != nullptr ? InObject->GetClass() : nullptr;
	}

	const UField* FInteropUtils::LoadUnrealField(const char* InCSharpFieldPathName)
	{
		if (InCSharpFieldPathName == nullptr)
		{
			return nullptr;
		}

		UField* Result = LoadObject<UField>(nullptr, UNREALSHARP_STRING_TO_TCHAR(InCSharpFieldPathName));

		return Result;
	}

	bool FInteropUtils::CheckUClassIsChildOf(const UClass* InTestClass, const UClass* InTestBaseClass)
	{
		check(InTestClass);
		check(InTestBaseClass);

		return InTestClass->IsChildOf(InTestBaseClass);
	}

	const UClass* FInteropUtils::GetSuperClass(const UClass* InClass)
	{
		return InClass != nullptr ? InClass->GetSuperClass() : nullptr;
	}

	int FInteropUtils::GetStructSize(const UStruct* InStruct)
	{
		return InStruct != nullptr ? InStruct->GetStructureSize() : 0;
	}

	const FProperty* FInteropUtils::GetProperty(const UStruct* InStruct, const char* InCSharpPropertyName)
	{
		if (InCSharpPropertyName == nullptr || InStruct == nullptr)
		{
			return nullptr;
		}

		const FName TargetName = UNREALSHARP_STRING_TO_TCHAR(InCSharpPropertyName);

		if (const UUserDefinedStruct* UserDefinedStruct = Cast<UUserDefinedStruct>(InStruct))
		{
			for (FProperty* Property = UserDefinedStruct->PropertyLink; Property != nullptr; Property = Property->PropertyLinkNext)
			{				
				const FName PropertyName = FUnrealSharpUtils::ExtraUserDefinedStructPropertyName(Property);

				if (PropertyName == TargetName)
				{
					return Property;
				}
			}
		}

		return InStruct->FindPropertyByName(TargetName);
	}

	const UFunction* FInteropUtils::GetFunction(const UClass* InClass, const char* InCSharpFunctionName)
	{
		if (InCSharpFunctionName == nullptr)
		{
			return nullptr;
		}

		return InClass != nullptr ? InClass->FindFunctionByName(UNREALSHARP_STRING_TO_TCHAR(InCSharpFunctionName)) : nullptr;
	}

	void FInteropUtils::InitializeStructData(const UStruct* InStruct, const void* InAddressOfStructData)
	{
		if (InStruct && InAddressOfStructData)
		{
			InStruct->InitializeStruct((void*)InAddressOfStructData);
		}
	}

	void FInteropUtils::UninitializeStructData(const UStruct* InStruct, const void* InAddressOfStructData)
	{
		if (InStruct && InAddressOfStructData)
		{
			InStruct->DestroyStruct((void*)InAddressOfStructData);
		}
	}

	const TCHAR* FInteropUtils::GetFieldCSharpFullPath(const UField* InField)
	{
		check(InField);
		// ensure(IsInGameThread());

		static thread_local FString s_Temp;

		s_Temp = FUnrealSharpUtils::GetCSharpFullPath(InField);

		return *s_Temp;
	}

	EClassFlags FInteropUtils::GetClassFlags(const UClass* InClass)
	{
		return InClass != nullptr ? InClass->GetClassFlags() : CLASS_None;
	}
}
