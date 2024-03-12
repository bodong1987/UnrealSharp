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
    const FProperty* FInteropUtils::GetElementPropertyOfSet(const FSetProperty* InSetProperty)
    {
        check(InSetProperty);

        return InSetProperty->ElementProp;
    }

    int FInteropUtils::GetLengthOfSet(const void* InAddressOfSet, const FSetProperty* InSetProperty)
    {
        FScriptSetHelper Helper(InSetProperty, InAddressOfSet);

        return Helper.Num();
    }

    const void* FInteropUtils::GetElementAddressOfSet(const void* InAddressOfSet, const FSetProperty* InSetProperty, int InIndex)
    {
        FScriptSetHelper Helper(InSetProperty, InAddressOfSet);

        return Helper.GetElementPtr(InIndex);
    }

    bool FInteropUtils::IsSetContainsElement(const void* InAddressOfSet, const FSetProperty* InSetProperty, const void* InAddressOfElementTarget)
    {
        FScriptSetHelper Helper(InSetProperty, InAddressOfSet);

        int Index = Helper.FindElementIndex(InAddressOfElementTarget);
        return INDEX_NONE != Index;
    }

    bool FInteropUtils::AddSetElement(void* InAddressOfSet, const FSetProperty* InSetProperty, const void* InAddressOfElementTarget)
    {
        FScriptSetHelper Helper(InSetProperty, InAddressOfSet);

        int OldNum = Helper.Num();

        Helper.AddElement(InAddressOfElementTarget);

        return OldNum != Helper.Num();
    }

    bool FInteropUtils::RemoveSetElement(void* InAddressOfSet, const FSetProperty* InSetProperty, const void* InAddressOfElementTarget)
    {
        FScriptSetHelper Helper(InSetProperty, InAddressOfSet);

        return Helper.RemoveElement(InAddressOfElementTarget);
    }

    void FInteropUtils::ClearSet(void* InAddressOfSet, const FSetProperty* InSetProperty)
    {
        FScriptSetHelper Helper(InSetProperty, InAddressOfSet);

        Helper.EmptyElements();
    }
}

