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

#include "UnrealSharpTestsBaseObjectInCpp.h"
#include "UnrealSharpTestsObjectInCpp.generated.h"

UCLASS(Blueprintable)
class UNREALSHARPTESTS_API UUnrealSharpTestsObjectInCpp : public UUnrealSharpTestsBaseObjectInCpp
{
    GENERATED_UCLASS_BODY()
    
public:
    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Internal Structures")
    FVector VecValueInCpp = FVector(1,2,3);
    
    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Internal Structures")
    FVector3f Vec3fValueInCpp = FVector3f(1,2,3);

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Internal Structures")
    FRotator RotValueInCpp = FRotator(10, 20, 30);

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Internal Structures")
    FGuid GuidValueInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_User Structures")
    FUnrealSharpTestsBaseStructValueInCpp TestBaseStructInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Class")
    UClass* ClassRawPtrDefaultInCpp = nullptr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Class", meta=(AllowAbstract=true))
    UClass* ClassRawPtrInCpp = nullptr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Class")
    TSubclassOf<AActor> SubclassOfActorDefaultInCpp = nullptr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Class", meta=(AllowAbstract=true))
    TSubclassOf<AActor> SubclassOfActorAllowAbstractInCpp = nullptr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Class")
    TSoftClassPtr<AActor> SoftClassPtrDefaultInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Class", meta=(AllowAbstract=true))
    TSoftClassPtr<AActor> SoftClassPtrAllowAbstractInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Object")
    UObject* RawObjectPtrInCpp = nullptr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Object")
    TObjectPtr<UObject> ObjectPtrInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Object")
    TSoftObjectPtr<UObject> SoftObjectPtrInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Array")
    TArray<int> IntArrayInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Array")
    TArray<FString> StringArrayInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Array")
    TArray<FName> NameArrayInCpp;
    
    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Array")
    TArray<FVector> VectorArrayInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Array")
    TArray<UObject*> ObjectArrayInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Set")
    TSet<float> FloatSetInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Set")
    TSet<FString> StringSetInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Set")
    TSet<UObject*> ObjectSetInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Map")
    TMap<int, FString> IntStringMapInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Map")
    TMap<FName, FVector> NameVectorMapInCpp;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Map")
    TMap<float, UObject*> FloatObjectMapInCpp;
};

