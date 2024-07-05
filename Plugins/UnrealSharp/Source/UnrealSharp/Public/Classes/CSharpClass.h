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
#include "CSharpGeneratedType.h"
#include "CSharpClass.generated.h"

namespace UnrealSharp
{
    class IUnrealFunctionInvokeRedirector;
}

/*
* C# Method parameter information
*/
USTRUCT()
struct UNREALSHARP_API FCSharpFunctionArgumentData
{
    GENERATED_BODY()
public:
    UPROPERTY()
    FName                                   Name;

    UPROPERTY()
    int64                                   Flags = 0;

    UPROPERTY()
    int                                     Size = 0;

public:
    bool                                    IsPassByReference() const;
    bool                                    IsReturnValue() const;
};

/*
* Detailed information about C# Method, including name, signature, parameter description, etc.
* The reason why this data is needed is that after compilation, 
* the number and order of parameters of the blueprint we generate through C# information \
* may be different from the actual number and order of parameters of the C# method. 
* Therefore, when we extract the parameters of Unreal UFunction, 
* we need to follow the order of Unreal functions, 
* and when calling C#, we must follow the order of C#.
*/
USTRUCT()
struct UNREALSHARP_API FCSharpFunctionData
{
    GENERATED_BODY()
public:
    UPROPERTY()
    FString                                 FunctionName;

    UPROPERTY()
    FString                                 FunctionSignature;

    UPROPERTY()
    TArray<FCSharpFunctionArgumentData>     Arguments;
};


/*
* Cache redirect information that will be used to restore UFunction data after the C# runtime exits
*/
struct UNREALSHARP_API FCSharpFunctionRedirectionData
{
    typedef TSharedPtr<UnrealSharp::IUnrealFunctionInvokeRedirector> FInvokeRedirectorPtr;

    UFunction*                              Function;
    const FCSharpFunctionData*              FunctionData;

    EFunctionFlags                          Flags;
    FNativeFuncPtr                          FuncPtr;
    
    TArray<uint8>                           Script;

    FInvokeRedirectorPtr                    Invoker;

    FCSharpFunctionRedirectionData();
    FCSharpFunctionRedirectionData(UFunction* InFunction, const FCSharpFunctionData* InFunctionData);
};

/*
* UCSharpClass represents that this Class is generated through the Assembly of C#, 
* and it will internally save its basic information in C#, including the assembly, full path, UFunction of C# source, etc.
* Essentially, UnrealSharp just helps you analyze C# assemblies and find the UCLASS types in them, 
* then helps you generate blueprint classes, helps you add corresponding properties and methods, 
* and then provides corresponding C# bindings.
*/
UCLASS()
class UNREALSHARP_API UCSharpClass : public UBlueprintGeneratedClass, public ICSharpGeneratedType
{
    GENERATED_BODY()
public:
    typedef TMap<UFunction*, FCSharpFunctionRedirectionData>  FRedirectionDataMappingType;

    // get crc code
    // If the crc code changes, it means that the code on the C# side has changed significantly, 
    // and the tool should regenerate the blueprint proxy class to match the change.
    virtual int64                           GetCrcCode() const override { return CrcCode; }

    // Get the generator version, 
    // which should also be regenerated when the version of the build tool does not match
    virtual int                             GetGeneratorVersion() const override{ return GeneratorVersion; }

    // get C# FullPath
    virtual const FString&                  GetCSharpFullName() const override{ return CSharpFullName; }

    // get assembly name
    // include .dll extension
    virtual const FString&                  GetAssemblyName() const override{ return AssemblyName; }

    virtual void                            SetCrcCode(int64 InCrcCode) override { CrcCode = InCrcCode; }
    virtual void                            SetGeneratorVersion(int InVersion) override { GeneratorVersion = InVersion; }
    virtual void                            SetCSharpFullName(const FString& InCSharpFullName) override { CSharpFullName = InCSharpFullName; }
    virtual void                            SetAssemblyName(const FString& InAssemblyName) override { AssemblyName = InAssemblyName; }
    virtual FString                         GetCSharpTypeName() const override;

    // clean caches
    virtual void                            ClearCSharpDataCaches();

public:
    // Register a C# function. When UFunction is called, if C# data is found, the call will be sent to C# Runtime for execution.
    void                                    AddCSharpFunction(const FName& InName, const FCSharpFunctionData& InFunctionData);
    void                                    AddCSharpFunction(FName&& InName, FCSharpFunctionData&& InFunctionData);

    // get function signature
    const FString&                          GetCSharpFunctionSignature(const FName& InName) const;

    // find C# function
    const FCSharpFunctionData*              FindCSharpFunction(const FName& InName) const;

    // redirect all C# UFunction to C# runtime 
    void                                    RedirectAllCSharpFunctions();

    // restore all redirection
    void                                    RestoreAllCSharpFunctions();

    // get redirection data cache for UFunction*
    FCSharpFunctionRedirectionData*         GetCSharpFunctionRedirection(const UFunction* InFunction);

private:
    // call C# method
    static void                             CallCSharpFunction(UObject* Context, FFrame& TheStack, RESULT_DECL);
    static void                             StaticConstructor(const FObjectInitializer& ObjectInitializer);
    static void                             StaticClassConstructor(UCSharpClass* InCSharpClass, const FObjectInitializer& ObjectInitializer);

public:
    virtual void                            Bind() override;

private:
    ClassConstructorType                    DefaultClassConstructor;

private:
    UPROPERTY(EditDefaultsOnly)
    int64                                   CrcCode = 0;

    UPROPERTY(EditDefaultsOnly)
    int                                     GeneratorVersion;

    UPROPERTY()
    FString                                 CSharpFullName;

    UPROPERTY()
    FString                                 AssemblyName;

    UPROPERTY()
    TMap<FName, FCSharpFunctionData>        CSharpFunctions;

    // caches
    FRedirectionDataMappingType             RedirectionCaches;
};
