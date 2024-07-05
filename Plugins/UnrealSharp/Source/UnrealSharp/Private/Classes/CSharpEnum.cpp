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
#include "Classes/CSharpEnum.h"
#include "Classes/UnrealSharpFunctionLibrary.h"

UCSharpEnum::UCSharpEnum(const FObjectInitializer& ObjectInitializer) :
    Super(ObjectInitializer)
{
#if WITH_EDITORONLY_DATA
    SetMetaData(TEXT("BlueprintType"), TEXT("true"));
    WarningTip = UUnrealSharpFunctionLibrary::GetGeneratedTypeWarningTips();
#endif
}

void UCSharpEnum::ClearEnums()
{
    TArray<TPair<FName, int64>> EmptyNames;
    SetEnums(EmptyNames, ECppForm::Namespaced);
}

FString UCSharpEnum::GetCSharpTypeName() const
{
    int Index;
    if (CSharpFullName.FindLastChar(TEXT('.'), Index))
    {
        return CSharpFullName.Mid(Index + 1);
    }

    return CSharpFullName;
}

