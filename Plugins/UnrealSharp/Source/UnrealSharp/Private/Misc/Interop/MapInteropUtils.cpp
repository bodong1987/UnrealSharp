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
    const FProperty* FInteropUtils::GetKeyPropertyOfMap(const FMapProperty* InMapProperty)
    {
        check(InMapProperty);

        return InMapProperty->GetKeyProperty();
    }

    const FProperty* FInteropUtils::GetValuePropertyOfMap(const FMapProperty* InMapProperty)
    {
        check(InMapProperty);

        return InMapProperty->GetValueProperty();
    }

    int FInteropUtils::GetLengthOfMap(const void* InAddressOfMap, const FMapProperty* InMapProperty)
    {
        const FScriptMapHelper Helper(InMapProperty, InAddressOfMap);

        return Helper.Num();
    }

    void FInteropUtils::ClearMap(void* InAddressOfMap, const FMapProperty* InMapProperty)  // NOLINT
    {
        FScriptMapHelper Helper(InMapProperty, InAddressOfMap);

        Helper.EmptyValues();
    }

    const void* FInteropUtils::GetKeyAddressOfMapElement(void* InAddressOfMap, const FMapProperty* InMapProperty, int InIndex)  // NOLINT
    {
        FScriptMapHelper Helper(InMapProperty, InAddressOfMap);

        return Helper.GetKeyPtr(InIndex);
    }

    void* FInteropUtils::GetValueAddressOfMapElement(void* InAddressOfMap, const FMapProperty* InMapProperty, int InIndex)  // NOLINT
    {
        FScriptMapHelper Helper(InMapProperty, InAddressOfMap);

        return Helper.GetValuePtr(InIndex);
    }

    FMapKeyValueAddressPair FInteropUtils::GetAddressOfMapElement(void* InAddressOfMap, const FMapProperty* InMapProperty, int InIndex) // NOLINT
    {
        FScriptMapHelper Helper(InMapProperty, InAddressOfMap);

        return {Helper.GetKeyPtr(InIndex), Helper.GetValuePtr(InIndex)};
    }

    void* FInteropUtils::FindValueAddressOfElementKey(void* InAddressOfMap, const FMapProperty* InMapProperty, const void* InAddressOfKeyElementTarget) // NOLINT
    {
        FScriptMapHelper Helper(InMapProperty, InAddressOfMap);

        void* ValueAddress = Helper.FindValueFromHash(InAddressOfKeyElementTarget);

        return ValueAddress;
    }

    bool FInteropUtils::TryAddNewElementToMap(void* InAddressOfMap, const FMapProperty* InMapProperty, const void* InAddressOfKeyElementTarget, const void* InAddressOfValueElementTarget, bool InOverrideIfExists) // NOLINT
    {
        FScriptMapHelper Helper(InMapProperty, InAddressOfMap);

        if (InOverrideIfExists)
        {
            void* TargetAddress = Helper.FindOrAdd(InAddressOfKeyElementTarget);

            InMapProperty->GetValueProperty()->CopyCompleteValue(TargetAddress, InAddressOfValueElementTarget);

            return true;
        }

        const void* ValueAddress = Helper.FindValueFromHash(InAddressOfKeyElementTarget);

        if (ValueAddress != nullptr)
        {
            return false;
        }

        Helper.AddPair(InAddressOfKeyElementTarget, InAddressOfValueElementTarget);

        return true;
    }

    bool FInteropUtils::RemoveElementFromMap(void* InAddressOfMap, const FMapProperty* InMapProperty, const void* InAddressOfKeyElementTarget) // NOLINT
    {
        FScriptMapHelper Helper(InMapProperty, InAddressOfMap);

        return Helper.RemovePair(InAddressOfKeyElementTarget);
    }
}
