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

namespace UnrealSharp
{
	void FInteropUtils::BindDelegate(const void* InDelegateAddress, const FProperty* InProperty, UObject* InObject, const char* InCSharpFunctionName)
	{
		check(InProperty && InProperty->IsA<FDelegateProperty>());

		FScriptDelegate* Delegate = (FScriptDelegate*)InDelegateAddress;
		check(Delegate != nullptr);

		Delegate->BindUFunction(InObject, UNREALSHARP_STRING_TO_TCHAR(InCSharpFunctionName));
	}

	void FInteropUtils::UnbindDelegate(const void* InDelegateAddress, const FProperty* InProperty)
	{
		check(InProperty && InProperty->IsA<FDelegateProperty>());

		FScriptDelegate* Delegate = (FScriptDelegate*)InDelegateAddress;
		check(Delegate != nullptr);

		Delegate->Unbind();
	}

	void FInteropUtils::ClearDelegate(const void* InDelegateAddress, const FProperty* InProperty)
	{
		if (InProperty->IsA<FMulticastDelegateProperty>())
		{
			FMulticastScriptDelegate* Delegate = (FMulticastScriptDelegate*)InDelegateAddress;
			Delegate->Clear();
		}
		else if (InProperty->IsA<FDelegateProperty>())
		{
			FScriptDelegate* Delegate = (FScriptDelegate*)InDelegateAddress;
			Delegate->Unbind();
		}
	}

	void FInteropUtils::AddDelegate(const void* InDelegateAddress, const FProperty* InProperty, UObject* InObject, const char* InCSharpFunctionName)
	{
		check(InProperty && InProperty->IsA<FMulticastDelegateProperty>());

		FMulticastScriptDelegate* Delegate = (FMulticastScriptDelegate*)InDelegateAddress;
		check(Delegate != nullptr);
				
		FScriptDelegate ScriptDelegate;
		ScriptDelegate.BindUFunction(InObject, UNREALSHARP_STRING_TO_TCHAR(InCSharpFunctionName));
		Delegate->AddUnique(ScriptDelegate);
	}

	void FInteropUtils::RemoveDelegate(const void* InDelegateAddress, const FProperty* InProperty, UObject* InObject, const char* InCSharpFunctionName)
	{
		check(InProperty && InProperty->IsA<FMulticastDelegateProperty>());

		FMulticastScriptDelegate* Delegate = (FMulticastScriptDelegate*)InDelegateAddress;
		check(Delegate != nullptr);

		FScriptDelegate ScriptDelegate;
		ScriptDelegate.BindUFunction(InObject, UNREALSHARP_STRING_TO_TCHAR(InCSharpFunctionName));
		Delegate->Remove(ScriptDelegate);
	}

	void FInteropUtils::RemoveAllDelegate(const void* InDelegateAddress, const FProperty* InProperty, UObject* InObject)
	{
		check(InProperty && InProperty->IsA<FMulticastDelegateProperty>());

		FMulticastScriptDelegate* Delegate = (FMulticastScriptDelegate*)InDelegateAddress;
		check(Delegate != nullptr);

		Delegate->RemoveAll(InObject);
	}
}
