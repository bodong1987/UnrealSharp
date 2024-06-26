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

#include "UnrealSharpTestsFunctionLibraryInCpp.h"
#include "UnrealSharpTestsObjectInCpp.h"

bool UUnrealSharpTestsFunctionLibraryInCpp::CppAddAndReturnByRef_Bool(bool a, bool b, bool& outA, bool& outB)
{
    outA = a;
    outB = b;

    return a && b;
}

uint8 UUnrealSharpTestsFunctionLibraryInCpp::CppAddAndReturnByRef_UInt8(uint8 a, uint8 b, uint8& outA, uint8& outB)
{
    outA = a;
    outB = b;

    return a + b;
}

int UUnrealSharpTestsFunctionLibraryInCpp::CppAddAndReturnByRef_Int32(int a, int b, int& outA, int& outB)
{
    outA = a;
    outB = b;

    return a + b;
}

float UUnrealSharpTestsFunctionLibraryInCpp::CppAddAndReturnByRef_Float(float a, float b, float& outA, float& outB)
{
    outA = a;
    outB = b;

    return a + b;
}

double UUnrealSharpTestsFunctionLibraryInCpp::CppAddAndReturnByRef_Double(double a, double b, double& outA, double& outB)
{
    outA = a;
    outB = b;

    return a + b;
}

FString UUnrealSharpTestsFunctionLibraryInCpp::CppAddAndReturnByRef_String(const FString& a, const FString& b, FString& outA, FString& outB)
{
    outA = a;
    outB = b;

    return a + b;
}

FName UUnrealSharpTestsFunctionLibraryInCpp::CppAddAndReturnByRef_Name(const FName& a, const FName& b, FName& outA, FName& outB)
{
    outA = a;
    outB = b;

    return *(a.ToString() + b.ToString());
}

FVector UUnrealSharpTestsFunctionLibraryInCpp::CppAddAndReturnByRef_Vector(const FVector& a, const FVector& b, FVector& outA, FVector& outB)
{
    outA = a;
    outB = b;

    return a + b;
}

FVector UUnrealSharpTestsFunctionLibraryInCpp::CppAddAndReturnByRef_VectorValue(FVector a, FVector b, FVector& outA, FVector& outB)
{
    outA = a;
    outB = b;

    return a + b;
}

EUnrealSharpLanguageTypesInCpp UUnrealSharpTestsFunctionLibraryInCpp::CppAddAndReturnByRef_Enum(EUnrealSharpLanguageTypesInCpp a, EUnrealSharpLanguageTypesInCpp b, EUnrealSharpLanguageTypesInCpp& outA, EUnrealSharpLanguageTypesInCpp& outB)
{
    outA = a;
    outB = b;

    return a | b;
}

EUnrealSharpProjectsEnumTypeInCpp UUnrealSharpTestsFunctionLibraryInCpp::CppAddAndReturnByRef_ByteEnum(EUnrealSharpProjectsEnumTypeInCpp a, EUnrealSharpProjectsEnumTypeInCpp b, EUnrealSharpProjectsEnumTypeInCpp& outA, EUnrealSharpProjectsEnumTypeInCpp& outB)
{
    outA = a;
    outB = b;

    return a;
}

void UUnrealSharpTestsFunctionLibraryInCpp::CppPrintText(const FString& Text, FColor TextColor /* = FColor::White */, float TimeOfDisplay /* = 5.0f */)
{
    GEngine->AddOnScreenDebugMessage(-1, TimeOfDisplay, TextColor, Text);
}

FUnrealSharpTestsBaseStructValueInCpp UUnrealSharpTestsFunctionLibraryInCpp::CppGetUserStructAndReturnByRef(FUnrealSharpTestsBaseStructValueInCpp a, FUnrealSharpTestsBaseStructValueInCpp b, FUnrealSharpTestsBaseStructValueInCpp& outA, FUnrealSharpTestsBaseStructValueInCpp& outB)
{
    outA = a;
    outB = b;

    FUnrealSharpTestsBaseStructValueInCpp Return;
    Return.fValue = 1024;
    Return.dValue = 2048;
    Return.StrValue = TEXT("UnrealSharp: Hello, My Friends!");

    outA.StrValue = outB.StrValue = Return.StrValue;
    outA.NameValue = outB.NameValue = *Return.StrValue;

    return Return;
}

UObject* UUnrealSharpTestsFunctionLibraryInCpp::CppGetObjectAndReturnByRef(UUnrealSharpTestsBaseObjectInCpp* a, UUnrealSharpTestsBaseObjectInCpp* b, UUnrealSharpTestsBaseObjectInCpp*& outA, UUnrealSharpTestsBaseObjectInCpp*& outB)
{
    outA = a;
    outB = NewObject<UUnrealSharpTestsObjectInCpp>();

    return outB;
}

TSubclassOf<UUnrealSharpTestsBaseObjectInCpp> UUnrealSharpTestsFunctionLibraryInCpp::CppGetSubclassOfAndReturnByRef(TSubclassOf<UUnrealSharpTestsBaseObjectInCpp> a, TSubclassOf<UUnrealSharpTestsBaseObjectInCpp> b, TSubclassOf<UUnrealSharpTestsBaseObjectInCpp>& outA, TSubclassOf<UUnrealSharpTestsBaseObjectInCpp>& outB)
{
    outA = a;
    outB = b;

    return UUnrealSharpTestsObjectInCpp::StaticClass();
}

UClass* UUnrealSharpTestsFunctionLibraryInCpp::CppGetClassAndReturnByRef(UClass* a, UClass* b, UClass*& outA, UClass*& outB)
{
    outA = a;
    outB = b;

    return UUnrealSharpTestsBaseObjectInCpp::StaticClass();
}

TArray<FString> UUnrealSharpTestsFunctionLibraryInCpp::CppGetStringArrayAndReturnByRef(const TArray<FString>& a, const TArray<FString>& b, TArray<FString>& outA, TArray<FString>& outB)
{
    outA = a;
    outB = b;

    TArray<FString> Result;
    Result.Append(a);
    Result.Append(b);

    return Result;
}

TSet<FName> UUnrealSharpTestsFunctionLibraryInCpp::CppGetNameSetAndReturnByRef(const TSet<FName>& a, const TSet<FName>& b, TSet<FName>& outA, TSet<FName>& outB)
{
    outA = a;
    outB = b;

    TSet<FName> Result;

    Result.Append(a);
    Result.Append(b);

    return Result;
}

TMap<int64, double> UUnrealSharpTestsFunctionLibraryInCpp::CppGetInt64DoubleMapAndReturnByRef(const TMap<int64, double>& a, const TMap<int64, double>& b, TMap<int64, double>& outA, TMap<int64, double>& outB)
{
    outA = a;
    outB = b;

    TMap<int64, double> Result;

    Result.Append(a);
    Result.Append(b);

    return Result;
}
