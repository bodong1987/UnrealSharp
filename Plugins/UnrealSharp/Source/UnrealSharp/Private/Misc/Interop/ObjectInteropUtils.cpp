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
#include "ICSharpRuntime.h"
#include "ICSharpObjectTable.h"
#include "ICSharpLibraryAccessor.h"

namespace UnrealSharp
{
	const UObject* FInteropUtils::GetDefaultUnrealObjectOfClass(const UClass* InClass)
	{
		return InClass != nullptr ? InClass->GetDefaultObject() : nullptr;
	}

	UObject* FInteropUtils::GetUnrealObjectOfCSharpObject(const void* InCSharpObject)
	{
		ICSharpRuntime* Runtime = FCSharpRuntimeFactory::GetInstance();

		check(Runtime);

		return Runtime->GetCSharpLibraryAccessor()->GetUnrealObject((UObject*)InCSharpObject);
	}


	FCSharpObjectMarshalValue FInteropUtils::GetCSharpObjectOfUnrealObject(const UObject* InObject)
	{
		if (InObject == nullptr)
		{
			return {};
		}

		ICSharpRuntime* Runtime = FCSharpRuntimeFactory::GetInstance();

		check(Runtime);

		return { Runtime->GetObjectTable()->GetCSharpObject((UObject*)InObject) };
	}

	FCSharpObjectMarshalValue FInteropUtils::GetOuterOfUnrealObject(const UObject* InObject)
	{
		if (InObject == nullptr)
		{
			return { nullptr };
		}

		UObject* OuterObject = InObject->GetOuter();

		if (OuterObject == nullptr)
		{
			return { nullptr };
		}

		return GetCSharpObjectOfUnrealObject(OuterObject);
	}

	const TCHAR* FInteropUtils::GetNameOfUnrealObject(const UObject* InObject)
	{
		if (InObject == nullptr)
		{
			return nullptr;
		}

		// ensure(IsInGameThread());
		static thread_local FString NameText;
		NameText = InObject->GetName();

		return *NameText;
	}

	const TCHAR* FInteropUtils::GetPathNameOfUnrealObject(const UObject* InObject)
	{
		if (InObject == nullptr)
		{
			return nullptr;
		}

		//ensure(IsInGameThread());
		static thread_local FString NameText;
		NameText = InObject->GetPathName();

		return *NameText;
	}

	FCSharpObjectMarshalValue FInteropUtils::CreateDefaultSubobject(UObject* InObject, const char* InCSharpSubobjectNameString, UClass* ReturnType, UClass* ClassToCreateByDefault, bool bIsRequired, bool bIsTransient)
	{
		check(InObject);
				
		UObject* ObjectResult = InObject->CreateDefaultSubobject(UNREALSHARP_STRING_TO_TCHAR(InCSharpSubobjectNameString), ReturnType, ClassToCreateByDefault, bIsRequired, bIsTransient);

		return GetCSharpObjectOfUnrealObject(ObjectResult);
	}

	FCSharpObjectMarshalValue FInteropUtils::GetDefaultSubobjectByName(UObject* InObject, const char* InCSharpSubobjectNameString)
	{
		check(InObject);

		return {InObject->GetDefaultSubobjectByName((InCSharpSubobjectNameString))};
	}

	FCSharpObjectMarshalValue FInteropUtils::NewUnrealObject(UObject* InOuter, UClass* InClass, const FName* InName, EObjectFlags InFlags, UObject* InTemplate, bool bInCopyTransientsFromClassDefaults)
	{
		UObject* Result = NewObject<UObject>(
			InOuter != nullptr ? InOuter : (UObject*)GetTransientPackage(), 
			InClass, 			
			InName != nullptr ? *InName : NAME_None, 
			InFlags,
			InTemplate,
			bInCopyTransientsFromClassDefaults
			);

		return GetCSharpObjectOfUnrealObject(Result);
	}

	#if ENGINE_MAJOR_VERSION >= 5 && ENGINE_MINOR_VERSION <= 2
	static UObject* LocalStaticDuplicateObject(UClass* Class, const UObject* SourceObject, UObject* Outer, FName Name)
	{
		if (SourceObject != nullptr)
		{
			if (Outer == nullptr || Outer == INVALID_OBJECT)
			{
				Outer = (UObject*)GetTransientOuterForRename(Class);
			}
			return StaticDuplicateObject(SourceObject, Outer, Name);
		}
		return nullptr;
	}
	#endif


	FCSharpObjectMarshalValue FInteropUtils::DuplicateUnrealObject(UClass* InClass, const UObject* InSourceObject, UObject* InOuter, const FName* InName)
	{
#if ENGINE_MAJOR_VERSION >= 5 && ENGINE_MINOR_VERSION <= 2
		UObject* Result = LocalStaticDuplicateObject(InClass, InSourceObject, InOuter, InName != nullptr ? *InName : NAME_None);
#else
		UObject* Result = DuplicateObject_Internal(InClass, InSourceObject, InOuter, InName != nullptr ? *InName : NAME_None);
#endif

		return GetCSharpObjectOfUnrealObject(Result);
	}

	FCSharpObjectMarshalValue FInteropUtils::GetUnrealTransientPackage()
	{
		return GetCSharpObjectOfUnrealObject(GetTransientPackage());
	}

	void FInteropUtils::AddUnrealObjectToRoot(UObject* InObject)
	{
		if (InObject)
		{
			InObject->AddToRoot();
		}
	}

	void FInteropUtils::RemoveUnrealObjectFromRoot(UObject* InObject)
	{
		if (InObject)
		{
			InObject->RemoveFromRoot();
		}
	}

	bool FInteropUtils::IsUnrealObjectRooted(UObject* InObject)
	{
		return InObject != nullptr && InObject->IsRooted();
	}

	bool FInteropUtils::IsUnrealObjectValid(UObject* InObject)
	{
		return ::IsValid(InObject);
	}

	FCSharpObjectMarshalValue FInteropUtils::FindUnrealObjectFast(UClass* InClass, UObject* InOuter, const FName* InName, bool bInExactClass, EObjectFlags InExclusiveFlags)
	{
		UObject* Result = StaticFindObjectFast(InClass, InOuter, InName != nullptr ? *InName : NAME_None, bInExactClass, InExclusiveFlags);
		
		return GetCSharpObjectOfUnrealObject(Result);
	}

	FCSharpObjectMarshalValue FInteropUtils::FindUnrealObject(UClass* InClass, UObject* InOuter, const char* InName, bool bInExactClass)
	{
		UObject* Result = StaticFindObject(InClass, InOuter, UNREALSHARP_STRING_TO_TCHAR(InName), bInExactClass);

		return GetCSharpObjectOfUnrealObject(Result);
	}

	FCSharpObjectMarshalValue FInteropUtils::FindUnrealObjectChecked(UClass* InClass, UObject* InOuter, const char* InName, bool bInExactClass)
	{
		UObject* Result = StaticFindObjectChecked(InClass, InOuter, UNREALSHARP_STRING_TO_TCHAR(InName), bInExactClass);

		return GetCSharpObjectOfUnrealObject(Result);
	}

	FCSharpObjectMarshalValue FInteropUtils::FindUnrealObjectSafe(UClass* InClass, UObject* InOuter, const char* InName, bool bInExactClass)
	{
		UObject* Result = StaticFindObjectSafe(InClass, InOuter, UNREALSHARP_STRING_TO_TCHAR(InName), bInExactClass);

		return GetCSharpObjectOfUnrealObject(Result);
	}

	FCSharpObjectMarshalValue FInteropUtils::LoadUnrealObject(UClass* InClass, UObject* InOuter, const char* InName, const char* InFileName, uint32 InLoadFlags, UPackageMap* InSandbox)
	{
		UObject* Result = StaticLoadObject(InClass, InOuter, UNREALSHARP_STRING_TO_TCHAR(InName), UNREALSHARP_STRING_TO_TCHAR(InFileName), InLoadFlags, InSandbox);

		return GetCSharpObjectOfUnrealObject(Result);
	}
}
