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
#include "Kismet/BlueprintFunctionLibrary.h"
#include "UnrealSharpTestsStructValueInCpp.h"
#include "UnrealSharpTestsFunctionLibraryInCpp.generated.h"

class UUnrealSharpTestsBaseObjectInCpp;
/**
 * 
 */
UCLASS(BlueprintType)
class UNREALSHARPTESTS_API UUnrealSharpTestsFunctionLibraryInCpp : public UBlueprintFunctionLibrary
{
	GENERATED_BODY()
public:	
	UFUNCTION(BlueprintCallable, Category="UnrealSharp_CPP")
	static bool CppAddAndReturnByRef_Bool(bool a, bool b, bool& outA, bool& outB);
	
	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static uint8 CppAddAndReturnByRef_UInt8(uint8 a, uint8 b, uint8& outA, uint8& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static int CppAddAndReturnByRef_Int32(int a, int b, int& outA, int& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static float CppAddAndReturnByRef_Float(float a, float b, float& outA, float& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static double CppAddAndReturnByRef_Double(double a, double b, double& outA, double& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static FString CppAddAndReturnByRef_String(const FString& a, const FString& b, FString& outA, FString& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static FName CppAddAndReturnByRef_Name(const FName& a, const FName& b, FName& outA, FName& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static FVector CppAddAndReturnByRef_Vector(const FVector& a, const FVector& b, FVector& outA, FVector& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static FVector CppAddAndReturnByRef_VectorValue(FVector a, FVector b, FVector& outA, FVector& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static EUnrealSharpLanguageTypesInCpp CppAddAndReturnByRef_Enum(EUnrealSharpLanguageTypesInCpp a, EUnrealSharpLanguageTypesInCpp b, EUnrealSharpLanguageTypesInCpp& outA, EUnrealSharpLanguageTypesInCpp& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static EUnrealSharpProjectsEnumTypeInCpp CppAddAndReturnByRef_ByteEnum(EUnrealSharpProjectsEnumTypeInCpp a, EUnrealSharpProjectsEnumTypeInCpp b, EUnrealSharpProjectsEnumTypeInCpp& outA, EUnrealSharpProjectsEnumTypeInCpp& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static FUnrealSharpTestsBaseStructValueInCpp CppGetUserStructAndReturnByRef(FUnrealSharpTestsBaseStructValueInCpp a, FUnrealSharpTestsBaseStructValueInCpp b, FUnrealSharpTestsBaseStructValueInCpp& outA, FUnrealSharpTestsBaseStructValueInCpp& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static UObject* CppGetObjectAndReturnByRef(UUnrealSharpTestsBaseObjectInCpp* a, UUnrealSharpTestsBaseObjectInCpp* b, UUnrealSharpTestsBaseObjectInCpp*& outA, UUnrealSharpTestsBaseObjectInCpp*& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static TSubclassOf<UUnrealSharpTestsBaseObjectInCpp> CppGetSubclassOfAndReturnByRef(TSubclassOf<UUnrealSharpTestsBaseObjectInCpp> a, TSubclassOf<UUnrealSharpTestsBaseObjectInCpp> b, TSubclassOf<UUnrealSharpTestsBaseObjectInCpp>& outA, TSubclassOf<UUnrealSharpTestsBaseObjectInCpp>& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static TArray<FString> CppGetStringArrayAndReturnByRef(const TArray<FString>& a, const TArray<FString>& b, TArray<FString>& outA, TArray<FString>& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static TSet<FName> CppGetNameSetAndReturnByRef(const TSet<FName>& a, const TSet<FName>& b, TSet<FName>& outA, TSet<FName>& outB);

	UFUNCTION(BlueprintCallable, Category = "UnrealSharp_CPP")
	static TMap<int64, double> CppGetInt64DoubleMapAndReturnByRef(const TMap<int64, double>& a, const TMap<int64, double>& b, TMap<int64, double>& outA, TMap<int64, double>& outB);

	UFUNCTION(BlueprintCallable, Category = "Debug")
	static void CppPrintText(const FString& Text, FColor TextColor = FColor::White, float TimeOfDisplay = 5.0f);
};
