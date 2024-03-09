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
#include "CSharpStructFactory.h"
#include "ICSharpRuntime.h"
#include "ICSharpMethodInvocation.h"
#include "Misc/StackMemory.h"
#include "Misc/ScopedCSharpMethodInvocation.h"

namespace UnrealSharp
{
    FCSharpStructFactory::FCSharpStructFactory(ICSharpRuntime* InRuntime, const FString& InAssemblyName, const FString& InFullName)
    {
		FString ToNativeFunctionSingature = FString::Printf(TEXT("%s:ToNative(intptr,int,%s&)"), *InFullName, *InFullName);		
		FString FromNativeFunctionSignature = FString::Printf(TEXT("%s:FromNative(intptr,int)"), *InFullName);

		ToNativeInvocation = InRuntime->CreateCSharpMethodInvocation(InAssemblyName, ToNativeFunctionSingature);
		checkf(ToNativeInvocation, TEXT("Failed bind C# method in assembly %s by signature:%s"), *InAssemblyName, *ToNativeFunctionSingature);

		FromNativeInvocation = InRuntime->CreateCSharpMethodInvocation(InAssemblyName, FromNativeFunctionSignature);
		checkf(FromNativeInvocation, TEXT("Failed bind C# method in assembly %s by signature:%s"), *InAssemblyName, *FromNativeFunctionSignature);
    }

	void* FCSharpStructFactory::FromNative(const void* InUnrealStructPtr)
	{
		UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(FromNativeInvocation);

		static const int offset = 0;

		void* Result = FromNativeInvocationInvoker.Invoke(nullptr, &InUnrealStructPtr, &offset);
		return Result;
	}

	void FCSharpStructFactory::ToNative(const void* InUnrealStructPtr, const void* InCSharpStructPtr)
	{
		UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(ToNativeInvocation);		

		static const int offset = 0;

		ToNativeInvocationInvoker.Invoke(nullptr, &InUnrealStructPtr, &offset, InCSharpStructPtr);		
	}
}
