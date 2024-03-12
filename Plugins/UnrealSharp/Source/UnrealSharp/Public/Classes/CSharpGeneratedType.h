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
#include "UObject/Interface.h"
#include "CSharpGeneratedType.generated.h"

// This class does not need to be modified.
UINTERFACE(MinimalAPI)
class UCSharpGeneratedType : public UInterface
{
    GENERATED_BODY()
};

/**
 * Constraints on C# generated types need to satisfy these interfaces. 
 * Only with these interfaces can you find them in the C# assembly and associate them.
 */
class UNREALSHARP_API ICSharpGeneratedType
{
    GENERATED_BODY()

public:
    virtual int64           GetCrcCode() const = 0;    
    virtual int             GetGeneratorVersion() const = 0;
    virtual const FString&  GetCSharpFullName() const = 0;
    virtual const FString&  GetAssemblyName() const = 0;
    virtual FString         GetCSharpTypeName() const = 0;

    // used in editor...
    virtual void            SetCrcCode(int64 InCrcCode) = 0;
    virtual void            SetGeneratorVersion(int InVersion) = 0;
    virtual void            SetCSharpFullName(const FString& InCSharpFullName) = 0;
    virtual void            SetAssemblyName(const FString& InAssemblyName) = 0;
};
