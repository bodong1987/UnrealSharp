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
#include "Misc/UnrealFunctionInvocation.h"

namespace UnrealSharp
{
    FUnrealFunctionInvocation* FInteropUtils::CreateUnrealInvocation(const UClass* InClass, const char* InCSharpFunctionName)
    {
        checkSlow(InClass);
        checkSlow(InCSharpFunctionName);

        FUnrealFunctionInvocation* Invocation = new FUnrealFunctionInvocation(InClass, US_STRING_TO_TCHAR(InCSharpFunctionName));

        return Invocation;
    }

    FUnrealFunctionInvocation* FInteropUtils::CreateUnrealInvocationFromDelegateProperty(const FProperty* InDelegateProperty)
    {
        if (const FMulticastDelegateProperty* MulticastDelegateProperty = CastField<FMulticastDelegateProperty>(InDelegateProperty))
        {
            return new FUnrealFunctionInvocation(MulticastDelegateProperty);
        }
        else if (const FDelegateProperty* DelegateProperty = CastField<FDelegateProperty>(InDelegateProperty))
        {
            return new FUnrealFunctionInvocation(DelegateProperty);
        }

        checkNoEntry();

        return nullptr;
    }

    void FInteropUtils::DestroyUnrealInvocation(FUnrealFunctionInvocation* InInvocation) // NOLINT
    {
        if (InInvocation != nullptr)
        {
            delete InInvocation;
        }
    }

    void FInteropUtils::InvokeUnrealInvocation(FUnrealFunctionInvocation* InInvocation, UObject* InObject, void* InParameterBuffer, int InParameterBufferSize) // NOLINT
    {
        checkSlow(InInvocation);

        InInvocation->Invoke(InObject, InParameterBuffer, InParameterBufferSize);
    }

    UFunction* FInteropUtils::GetUnrealInvocationFunction(FUnrealFunctionInvocation* InInvocation) // NOLINT
    {
        return InInvocation->GetFunction();
    }

    int FInteropUtils::GetUnrealInvocationParameterSize(FUnrealFunctionInvocation* InInvocation) // NOLINT
    {
        check(InInvocation);
        check(InInvocation->GetFunction());

        return InInvocation->GetFunction()->ParmsSize;
    }

    void FInteropUtils::InitializeUnrealInvocationParameters(FUnrealFunctionInvocation* InInvocation, void* InParameterBuffer, int InParameterBufferSize) // NOLINT
    {
        check(InInvocation);

        InInvocation->InitializeParameterBuffer(InParameterBuffer, InParameterBufferSize);
    }

    void FInteropUtils::UnInitializeUnrealInvocationParameters(FUnrealFunctionInvocation* InInvocation, void* InParameterBuffer, int InParameterBufferSize) // NOLINT
    {
        check(InInvocation);

        InInvocation->UnInitializeParameterBuffer(InParameterBuffer, InParameterBufferSize);
    }
}
