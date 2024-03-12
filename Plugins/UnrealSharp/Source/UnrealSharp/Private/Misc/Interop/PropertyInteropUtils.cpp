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

namespace UnrealSharp
{
    int FInteropUtils::GetPropertyOffset(const FProperty* InProperty)
    {
        return InProperty != nullptr ? InProperty->GetOffset_ReplaceWith_ContainerPtrToValuePtr() : -1;
    }

    int FInteropUtils::GetPropertySize(const FProperty* InProperty)
    {
        return InProperty != nullptr ? InProperty->GetSize() : -1;
    }
    
    void FInteropUtils::InitializePropertyData(const FProperty* InProperty, void* InMemory)
    {
        check(InProperty);
        check(InMemory);

        InProperty->InitializeValue(InMemory);
    }

    void FInteropUtils::UnInitializePropertyData(const FProperty* InProperty, void* InMemory)
    {
        check(InProperty);
        check(InMemory);

        InProperty->DestroyValue(InMemory);
    }

    uint64 FInteropUtils::GetPropertyCastFlags(const FProperty* InProperty)
    {
        return InProperty != nullptr ? InProperty->GetClass()->GetCastFlags() : 0;
    }

    const UField* FInteropUtils::GetPropertyInnerField(const FProperty* InProperty)
    {
        return FUnrealSharpUtils::GetPropertyInnerField(InProperty);
    }

    void FInteropUtils::SetPropertyValueInContainer(const FProperty* InProperty, void* InOutContainer, const void* InValue)
    {
        check(InProperty && InOutContainer && InValue);

        InProperty->SetValue_InContainer(InOutContainer, InValue);
    }

    void FInteropUtils::GetPropertyValueInContainer(const FProperty* InProperty, const void* InContainer, void* OutValue)
    {
        check(InProperty && InContainer && OutValue);

        InProperty->GetValue_InContainer(InContainer, OutValue);
    }

    void FInteropUtils::SetBoolPropertyValue(const FBoolProperty* InBoolProperty, void* InTargetAddress, bool bInValue)
    {
        check(InBoolProperty && InTargetAddress);

        InBoolProperty->SetPropertyValue(InTargetAddress, bInValue);
    }

    bool FInteropUtils::GetBoolPropertyValue(const FBoolProperty* InBoolProperty, void* InTargetAddress)
    {
        check(InBoolProperty && InTargetAddress);

        return InBoolProperty->GetPropertyValue(InTargetAddress);
    }
}

