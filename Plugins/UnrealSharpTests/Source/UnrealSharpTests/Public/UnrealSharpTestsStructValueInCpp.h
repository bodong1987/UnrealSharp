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
#pragma once

#include "CoreMinimal.h"
#include "UnrealSharpTestsStructValueInCpp.generated.h"

UENUM(BlueprintType)
enum class EUnrealSharpProjectsEnumTypeInCpp : uint8
{
    UnrealSharpProject,
    UnrealSharpTestsProject,
    SharpBindingGenProject,
    UnrealSharpEditorProject
};

UENUM(BlueprintType, meta=(Bitflags))
enum class EUnrealSharpLanguageTypesInCpp : uint8
{
    None = 0,
    CSharp = 1 << 0,
    CLanguage = 1 << 1,
    CPlusPlus = 1 << 2,
    Python = 1 << 3,
    VisualBasic = 1 << 4,
    FSharp = 1<<5,
    JavaScript = 1<<6
};

ENUM_CLASS_FLAGS(EUnrealSharpLanguageTypesInCpp);

USTRUCT(BlueprintType)
struct UNREALSHARPTESTS_API FUnrealSharpTestsBaseStructValueInCpp
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category="Scalar")
    bool bBoolValue = false;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category="Cpp_Scalar")
    uint8 bBoolBitMask0 : 1;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category="Cpp_Scalar")
    uint8 bBoolBitMask1 : 1;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category="Cpp_Scalar")
    uint8 bBoolBitMask2 : 1;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category="Cpp_Scalar")
    uint8 bBoolBitMask3 : 1;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    uint8 u8Value = 255;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    int32 i32Value = 100;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    float fValue = 3.1415926f;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    double dValue = 0.618;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    EUnrealSharpProjectsEnumTypeInCpp ProjectValue = EUnrealSharpProjectsEnumTypeInCpp::UnrealSharpTestsProject;

    // unreal unsupport multiple enum flags as default value
    // it will export (INVALID) as default value text
    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    EUnrealSharpLanguageTypesInCpp LanguageFlags = EUnrealSharpLanguageTypesInCpp::VisualBasic;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Text")
    FString StrValue = TEXT("Hello, UnrealSharp!!!");

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Text")
    FName NameValue = TEXT("Hello, UnrealSharp!!!");

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Text")
    FText TextValue = FText::FromString("Unreal");

    FUnrealSharpTestsBaseStructValueInCpp();
};

USTRUCT(BlueprintType)
struct UNREALSHARPTESTS_API FUnrealSharpTestsStructValueInCpp
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category="Scalar")
    bool bBoolValue = true;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    uint8 u8Value = 128;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    int32 i32Value = 65535;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    float fValue = 3.1415926f;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    double dValue = 0.618;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    EUnrealSharpProjectsEnumTypeInCpp ProjectValue = EUnrealSharpProjectsEnumTypeInCpp::UnrealSharpProject;

    // unreal unsupport multiple enum flags as default value
    // it will export (INVALID) as default value text
    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    EUnrealSharpLanguageTypesInCpp LanguageFlags = EUnrealSharpLanguageTypesInCpp::CSharp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Text")
    FString StrValue = TEXT("Hello UnrealSharp!!!");

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Text")
    FName NameValue = TEXT("Hello UnrealSharp!!!");

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Text")
    FText TextValue = FText::FromString("Unreal");

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Internal Structures")
    FVector VecValue = FVector(1,2,3);
    
    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Internal Structures")
    FVector3f Vec3fValue = FVector3f(1,2,3);

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Internal Structures")
    FRotator RotValue = FRotator(10, 20, 30);

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Internal Structures")
    FGuid GuidValue;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "User Structures")
    FUnrealSharpTestsBaseStructValueInCpp TestBaseStruct;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Class")
    UClass* ClassRawPtrDefault = nullptr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Class", meta=(AllowAbstract=true))
    UClass* ClassRawPtr = nullptr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Class")
    TSubclassOf<AActor> SubclassOfActorDefault = nullptr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Class", meta=(AllowAbstract=true))
    TSubclassOf<AActor> SubclassOfActorAllowAbstract = nullptr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Class")
    TSoftClassPtr<AActor> SoftClassPtrDefault;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Class", meta=(AllowAbstract=true))
    TSoftClassPtr<AActor> SoftClassPtrAllowAbstract;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Object")
    UObject* RawObjectPtr = nullptr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Object")
    TObjectPtr<UObject> ObjectPtr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Object")
    TSoftObjectPtr<UObject> SoftObjectPtr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Array")
    TArray<int> IntArray;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Array")
    TArray<FString> StringArray;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Array")
    TArray<FName> NameArray;
    
    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Array")
    TArray<FVector> VectorArray;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Array")
    TArray<UObject*> ObjectArray;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Set")
    TSet<float> FloatSet;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Set")
    TSet<FString> StringSet;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Set")
    TSet<UObject*> ObjectSet;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Map")
    TMap<int, FString> IntStringMap;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Map")
    TMap<FName, FVector> NameVectorMap;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Map")
    TMap<float, UObject*> FloatObjectMap;
};
