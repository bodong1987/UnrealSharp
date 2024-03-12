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
#include "UObject/NoExportTypes.h"
#include "UnrealSharpTestsStructValueInCpp.h"
#include "UnrealSharpTestsBaseObjectInCpp.generated.h"

DECLARE_DYNAMIC_DELEGATE_ThreeParams(FUnrealSharpTestsDelegateTypeInCpp, int, intParam, FString, strParam, FName, nameParam);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_FourParams(FUnrealSharpTestsMulticastDelegateTypeInCpp, bool, boolParam, FVector, vecParam, FString, strParam, UObject*, objParam);

/**
 * 
 */
UCLASS(Blueprintable)
class UNREALSHARPTESTS_API UUnrealSharpTestsBaseObjectInCpp : public UObject
{
    GENERATED_UCLASS_BODY()
    
public:
    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category="Cpp_Scalar")
    bool bBoolValueInCpp = true;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Scalar")
    uint8 bBoolBitMaskInCpp0 : 1;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Scalar")
    uint8 bBoolBitMaskInCpp1 : 1;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Scalar")
    uint8 bBoolBitMaskInCpp2 : 1;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Scalar")
    uint8 bBoolBitMaskInCpp3 : 1;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Scalar")
    uint8 u8ValueInCpp = 128;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Scalar")
    int32 i32ValueInCpp = 65535;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Scalar")
    float fValueInCpp = 3.1415926f;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Scalar")
    double dValueInCpp = 0.618;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Scalar")
    EUnrealSharpProjectsEnumTypeInCpp ProjectValueInCpp = EUnrealSharpProjectsEnumTypeInCpp::UnrealSharpProject;

    // unreal unsupport multiple enum flags as default value
    // it will export (INVALID) as default value text
    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Scalar")
    EUnrealSharpLanguageTypesInCpp LanguageFlags = EUnrealSharpLanguageTypesInCpp::Python;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Text")
    FString StrValueInCpp = TEXT("Hello UnrealSharp!!!");

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Text")
    FName NameValueInCpp = TEXT("Hello UnrealSharp!!!");

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Cpp_Text")
    FText TextValueInCpp = FText::FromString("Unreal");

    UPROPERTY(BlueprintReadWrite, Category = "Cpp_Delegate")
    FUnrealSharpTestsDelegateTypeInCpp DelegateInCpp;

    UPROPERTY(BlueprintAssignable, BlueprintReadWrite, Category = "Cpp_Delegate")
    FUnrealSharpTestsMulticastDelegateTypeInCpp MulticastDelegateInCpp;

    UFUNCTION(BlueprintCallable)
    void InvokeDelegateInCpp(int intParam, const FString& strParam, FName nameParam);

    UFUNCTION(BlueprintCallable)
    void BroadcastDelegateInCpp(bool bValue, FVector vecParam, const FString& strParam, UObject* objectParam);
};
