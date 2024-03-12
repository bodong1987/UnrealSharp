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
    const UClass* FInteropUtils::GetClassOfObjectInitializer(const FObjectInitializer* InObjectInitializer)
    {
        check(InObjectInitializer);
        return InObjectInitializer->GetClass();
    }

    FCSharpObjectMarshalValue FInteropUtils::GetObjectOfObjectInitializer(const FObjectInitializer* InObjectInitializer)
    {
        check(InObjectInitializer);

        UObject* Obj = InObjectInitializer->GetObj();

        return GetCSharpObjectOfUnrealObject(Obj);
    }

    FCSharpObjectMarshalValue FInteropUtils::CreateDefaultSubobjectOfObjectInitializer(const FObjectInitializer* InObjectInitializer, UObject* InOuter, const char* InSubobjectNameString, UClass* InReturnType, UClass* InClassToCreateByDefault, bool bIsRequired, bool bIsTransient)
    {
        check(InObjectInitializer);

        UObject* Target = InObjectInitializer->CreateDefaultSubobject(InOuter, UNREALSHARP_STRING_TO_TCHAR(InSubobjectNameString), InReturnType, InClassToCreateByDefault, bIsRequired, bIsTransient);

        return GetCSharpObjectOfUnrealObject(Target);
    }

    FCSharpObjectMarshalValue FInteropUtils::CreateEditorOnlyDefaultSubobjectOfObjectInitializer(const FObjectInitializer* InObjectInitializer, UObject* InOuter, const char* InSubobjectNameString, UClass* InReturnType, bool bIsTransient)
    {
        check(InObjectInitializer);

        UObject* Target = InObjectInitializer->CreateEditorOnlyDefaultSubobject(InOuter, UNREALSHARP_STRING_TO_TCHAR(InSubobjectNameString), InReturnType, bIsTransient);

        return GetCSharpObjectOfUnrealObject(Target);
    }

    void FInteropUtils::SetDefaultSubobjectClassOfObjectInitializer(const FObjectInitializer* InObjectInitializer, const char* InSubobjectNameString, UClass* InClass)
    {
        check(InObjectInitializer);

        InObjectInitializer->SetDefaultSubobjectClass(UNREALSHARP_STRING_TO_TCHAR(InSubobjectNameString), InClass);
    }

    void FInteropUtils::DoNotCreateDefaultSubobjectOfObjectInitializer(const FObjectInitializer* InObjectInitializer, const char* InSubobjectNameString)
    {
        check(InObjectInitializer);

        InObjectInitializer->DoNotCreateDefaultSubobject(UNREALSHARP_STRING_TO_TCHAR(InSubobjectNameString));
    }

    void FInteropUtils::SetNestedDefaultSubobjectClassOfObjectInitializer(const FObjectInitializer* InObjectInitializer, const char* InSubobjectNameString, UClass* InClass)
    {
        check(InObjectInitializer);

        InObjectInitializer->SetNestedDefaultSubobjectClass(UNREALSHARP_STRING_TO_TCHAR(InSubobjectNameString), InClass);
    }

    void FInteropUtils::DoNotCreateNestedDefaultSubobjectOfObjectInitializer(const FObjectInitializer* InObjectInitializer, const char* InSubobjectNameString)
    {
        check(InObjectInitializer);

        InObjectInitializer->DoNotCreateNestedDefaultSubobject(UNREALSHARP_STRING_TO_TCHAR(InSubobjectNameString));
    }
}

