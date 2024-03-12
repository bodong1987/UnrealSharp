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
#include "GameFramework/Actor.h"
#include "UnrealSharpTestsActorInterface.h"
#include "UnrealSharpTestsStructValueInCpp.h"
#include "UnrealSharpTestsActorInCpp.generated.h"

UCLASS(Blueprintable, placeable)
class UNREALSHARPTESTS_API AUnrealSharpTestsActorInCpp : public AActor, public IUnrealSharpTestsActorInterface
{
    GENERATED_BODY()
    
public:    
    // Sets default values for this actor's properties
    AUnrealSharpTestsActorInCpp();

protected:
    // Called when the game starts or when spawned
    virtual void BeginPlay() override;

public:    
    // Called every frame
    virtual void Tick(float DeltaTime) override;

public:
    UPROPERTY(BlueprintReadWrite, EditAnywhere)
    FUnrealSharpTestsStructValueInCpp StructValue;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category="Scalar")
    bool bBoolActorValue = true;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    uint8 u8ActorValue = 128;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    int32 i32ActorValue = 65535;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    float fActorValue = 3.1415926f;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    double dActorValue = 0.618;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    EUnrealSharpProjectsEnumTypeInCpp ProjectActorValue = EUnrealSharpProjectsEnumTypeInCpp::UnrealSharpProject;

    // unreal unsupport multiple enum flags as default value
    // it will export (INVALID) as default value text
    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Scalar")
    EUnrealSharpLanguageTypesInCpp LanguageFlags = EUnrealSharpLanguageTypesInCpp::CPlusPlus;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Text")
    FString StrActorValue = TEXT("Hello UnrealSharp!!!");

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Text")
    FName NameActorValue = TEXT("Hello UnrealSharp!!!");

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Text")
    FText TextActorValue = FText::FromString("Unreal");

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Internal Structures")
    FVector VecActorValue = FVector(1,2,3);
    
    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Internal Structures")
    FVector3f Vec3fActorValue = FVector3f(1,2,3);

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Internal Structures")
    FRotator RotActorValue = FRotator(10, 20, 30);

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Internal Structures")
    FGuid GuidActorValue;

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
    TSubclassOf<AActor> SoftClassPtrAllowAbstract;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Object")
    UObject* RawObjectPtr = nullptr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Object")
    TObjectPtr<UObject> ObjectPtr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Object")
    TSoftObjectPtr<UObject> SoftObjectPtr;
};
