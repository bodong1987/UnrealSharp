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
#include "Misc/UnrealFunctionInvocation.h"

namespace UnrealSharp
{
    FUnrealFunctionInvocation::FUnrealFunctionInvocation()
    {
    }

    FUnrealFunctionInvocation::FUnrealFunctionInvocation(const TCHAR* InFunctionPath)
    {
        Load(InFunctionPath);
    }

    FUnrealFunctionInvocation::FUnrealFunctionInvocation(const UClass* InClass, const TCHAR* InFunctionName)
    {
        Load(InClass, InFunctionName);
    }

    FUnrealFunctionInvocation::FUnrealFunctionInvocation(UFunction* InFunction) :
        Function(InFunction)
    {
    }

    FUnrealFunctionInvocation::FUnrealFunctionInvocation(const FDelegateProperty* InDelegateProeprty) :
        Function(InDelegateProeprty->SignatureFunction),
        DelegateProperty(InDelegateProeprty)
    {        
    }

    FUnrealFunctionInvocation::FUnrealFunctionInvocation(const FMulticastDelegateProperty* InMulticastDelegateProperty) :
        Function(InMulticastDelegateProperty->SignatureFunction),
        MulticastDelegateProperty(InMulticastDelegateProperty)
    {
    }
    
    FUnrealFunctionInvocation::~FUnrealFunctionInvocation()
    {
    }

    void FUnrealFunctionInvocation::Load(const TCHAR* InFunctionPath)
    {
        Function = LoadObject<UFunction>(nullptr, InFunctionPath);

        checkf(Function != nullptr, TEXT("Failed bind function %s"), InFunctionPath);
    }

    void FUnrealFunctionInvocation::Load(const UClass* InClass, const TCHAR* InFunctionName)
    {
        check(InClass != nullptr);
        check(InFunctionName != nullptr);

        Function = InClass->FindFunctionByName(InFunctionName);

        checkf(Function != nullptr, TEXT("Failed bind function %s in class %s"), InFunctionName, *InClass->GetPathName());
    }

    void FUnrealFunctionInvocation::InitializeParameterBuffer(void* InParameterBuffer, int InParameterBufferSize) const
    {
        check(Function);
        check(InParameterBuffer);

        for (TFieldIterator<FProperty> PropertyIter(Function, EFieldIterationFlags::IncludeAll); PropertyIter; ++PropertyIter)
        {
            FProperty* Property = *PropertyIter;

            checkSlow(Property->GetOffset_ForUFunction() < InParameterBufferSize);

            Property->InitializeValue_InContainer(InParameterBuffer);
        }
    }

    void FUnrealFunctionInvocation::UnInitializeParameterBuffer(void* InParameterBuffer, int InParameterBufferSize) const
    {
        check(Function);
        check(InParameterBuffer);

        for (TFieldIterator<FProperty> PropertyIter(Function, EFieldIterationFlags::IncludeAll); PropertyIter; ++PropertyIter)
        {
            FProperty* Property = *PropertyIter;

            checkSlow(Property->GetOffset_ForUFunction()<InParameterBufferSize);

            Property->DestroyValue_InContainer(InParameterBuffer);
        }
    }

    void FUnrealFunctionInvocation::Invoke(UObject* InObject, void* InParameterBuffer, int InParameterBufferSize) const
    {        
        void* Address = InParameterBuffer;

        if (DelegateProperty == nullptr && MulticastDelegateProperty == nullptr)
        {
            check(Function != nullptr);
            check(Function->ParmsSize <= InParameterBufferSize);

            // is static
            if ((Function->FunctionFlags & EFunctionFlags::FUNC_Static) != 0)
            {
                if (InObject != nullptr)
                {
                    InObject->ProcessEvent(Function, Address);
                }
                else
                {
                    Function->GetOwnerClass()->GetDefaultObject()->ProcessEvent(Function, Address);
                }
            }
            else
            {
                check(InObject != nullptr);

                InObject->ProcessEvent(Function, Address);
            }
        }
        else
        {
            check(InObject != nullptr);

            if (MulticastDelegateProperty != nullptr)
            {
                const void* Addr = MulticastDelegateProperty->ContainerPtrToValuePtr<void>(InObject, 0);
                const FMulticastScriptDelegate* Value = MulticastDelegateProperty->GetMulticastDelegate(Addr);

                checkSlow(Value);

                Value->ProcessMulticastDelegate<UObject>(Address);
            }
            else if (DelegateProperty != nullptr)
            {
                const void* Addr = DelegateProperty->ContainerPtrToValuePtr<void>(InObject, 0);
                const FScriptDelegate* Value = DelegateProperty->GetPropertyValuePtr(Addr);

                checkSlow(Value);

                Value->ProcessDelegate<UObject>(Address);
            }
        }        
    }
}

