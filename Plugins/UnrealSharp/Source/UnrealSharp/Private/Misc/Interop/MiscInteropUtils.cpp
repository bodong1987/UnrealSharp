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
#include "ICSharpLibraryAccessor.h"

namespace UnrealSharp
{
    FGuid FInteropUtils::MakeGuidFromString(const char* InCSharpGuidString)
    {
        FGuid GUID;
        FGuid::Parse(US_STRING_TO_TCHAR(InCSharpGuidString), GUID);

        return GUID;
    }

    void* FInteropUtils::CreateCSharpStruct(const void* InUnrealStructPtr, UScriptStruct* InStruct) // NOLINT
    {
        ICSharpRuntime* Runtime = FCSharpRuntimeFactory::GetInstance();

        check(Runtime);

        return Runtime->GetCSharpLibraryAccessor()->CreateCSharpStruct(InUnrealStructPtr, InStruct);
    }

    void FInteropUtils::StructToNative(UScriptStruct* InStructType, void* InNativePtr, const void* InCSharpStructPtr) // NOLINT
    {
        ICSharpRuntime* Runtime = FCSharpRuntimeFactory::GetInstance();

        check(Runtime);

        return Runtime->GetCSharpLibraryAccessor()->StructToNative(InStructType, InNativePtr, InCSharpStructPtr);
    }

    void* FInteropUtils::CreateCSharpCollection(void* InAddressOfCollection, FProperty* InCollectionProperty)
    {
        ICSharpRuntime* Runtime = FCSharpRuntimeFactory::GetInstance();

        check(Runtime);

        return Runtime->GetCSharpLibraryAccessor()->CreateCSharpCollection(InAddressOfCollection, InCollectionProperty);
    }

    void FInteropUtils::CopyFromCSharpCollection(void* InAddressOfCollection, FProperty* InCollectionProperty, void* InCSharpCollection)
    {
        ICSharpRuntime* Runtime = FCSharpRuntimeFactory::GetInstance();

        check(Runtime);

        return Runtime->GetCSharpLibraryAccessor()->CopyFromCSharpCollection(InAddressOfCollection, InCollectionProperty, InCSharpCollection);
    }
}
