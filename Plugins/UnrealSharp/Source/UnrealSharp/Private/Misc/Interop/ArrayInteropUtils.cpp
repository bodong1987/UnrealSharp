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
    const FProperty* FInteropUtils::GetElementPropertyOfArray(const FArrayProperty* InArrayProperty)
    {
        check(InArrayProperty);

        return InArrayProperty->Inner;
    }

    int FInteropUtils::GetLengthOfArray(const void* InAddressOfArray, const FArrayProperty* InArrayProperty)
    {
        FScriptArrayHelper Helper(InArrayProperty, InAddressOfArray);

        return Helper.Num();
    }

    const void* FInteropUtils::GetElementAddressOfArray(const void* InAddressOfArray, const FArrayProperty* InArrayProperty, int InIndex)
    {
        return InArrayProperty->GetValueAddressAtIndex_Direct(InArrayProperty->Inner, (void*)InAddressOfArray, InIndex);
    }

    void FInteropUtils::ClearArray(const void* InAddressOfArray, const FArrayProperty* InArrayProperty)
    {
        FScriptArrayHelper Helper(InArrayProperty, InAddressOfArray);
        Helper.EmptyValues();
    }

    void FInteropUtils::RemoveAtArrayIndex(const void* InAddressOfArray, const FArrayProperty* InArrayProperty, int InIndex)
    {
        FScriptArrayHelper Helper(InArrayProperty, InAddressOfArray);
        Helper.RemoveValues(InIndex);
    }

    const void* FInteropUtils::InsertEmptyAtArrayIndex(const void* InAddressOfArray, const FArrayProperty* InArrayProperty, int InIndex)
    {
        FScriptArrayHelper Helper(InArrayProperty, InAddressOfArray);
        Helper.InsertValues(InIndex);

        return GetElementAddressOfArray(InAddressOfArray, InArrayProperty, InIndex);
    }

    int FInteropUtils::FindIndexOfArrayElement(const void* InAddressOfArray, const FArrayProperty* InArrayProperty, const void* InAddressOfTargetElement)
    {
        FScriptArrayHelper Helper(InArrayProperty, InAddressOfArray);

        const int Length = Helper.Num();

        for (int i = 0; i < Length; ++i)
        {
            if (InArrayProperty->Inner->Identical(GetElementAddressOfArray(InAddressOfArray, InArrayProperty, i), InAddressOfTargetElement))
            {
                return i;
            }
        }

        return INDEX_NONE;
    }
}

