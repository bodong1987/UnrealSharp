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
#include "Engine/UserDefinedStruct.h"
#include "CSharpGeneratedType.h"
#include "CSharpStruct.generated.h"

/*
* Indicates that this structure is generated from a C# assembly. 
* Essentially, UnrealSharp just helps you analyze the C# assembly and find the USTRUCT definition, 
* automatically generates user-defined interfaces in the blueprint, 
* and then provides you with the corresponding C# bindings.
*/
UCLASS()
class UNREALSHARP_API UCSharpStruct : public UUserDefinedStruct, public ICSharpGeneratedType
{
    GENERATED_BODY()

    UCSharpStruct(const FObjectInitializer& ObjectInitializer);
public:
    virtual int64                               GetCrcCode() const override { return CrcCode; }
    virtual int                                 GetGeneratorVersion() const override{ return GeneratorVersion; }
    virtual const FString&                      GetCSharpFullName() const override { return CSharpFullName; }
    virtual const FString&                      GetAssemblyName() const override { return AssemblyName; }
    virtual void                                SetCrcCode(int64 InCrcCode) override{ CrcCode = InCrcCode; }
    virtual void                                SetGeneratorVersion(int InVersion) override { GeneratorVersion = InVersion; }
    virtual void                                SetCSharpFullName(const FString& InCSharpFullName) override{ CSharpFullName = InCSharpFullName; }
    virtual void                                SetAssemblyName(const FString& InAssemblyName) override{ AssemblyName = InAssemblyName; }
    virtual FString                             GetCSharpTypeName() const override;

protected:
    UPROPERTY(EditDefaultsOnly)
    int64                                       CrcCode = 0;

    UPROPERTY()
    int                                         GeneratorVersion = 0;

    UPROPERTY()
    FString                                     CSharpFullName;

    UPROPERTY()
    FString                                     AssemblyName;

#if WITH_EDITORONLY_DATA
    UPROPERTY(BlueprintReadOnly, VisibleAnywhere, Category = Description, meta = (MultiLine = "true"))
    FText                                       WarningTip;
#endif
};
