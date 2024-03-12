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

#include "StructTypeDefinition.h"
#include "FunctionTypeDefinition.h"

namespace UnrealSharp
{
    // definition of UClass
    class SHARPBINDINGGEN_API FClassTypeDefinition : public FStructTypeDefinition
    {
    public:
        typedef FStructTypeDefinition Super;

        FClassTypeDefinition();
        FClassTypeDefinition(UClass* InClass, FTypeValidation* InTypeValidation);

        virtual void                        Read(FJsonObject& InObject) override;
        virtual void                        Write(FJsonObject& InObject) override;

    private:
        void                                LoadInterfaces(UClass* InClass);
        void                                LoadFunctions(UClass* InClass, FTypeValidation* InTypeValidation);    
        void                                AddDependNamespace(const UFunction* InFunction);
    public:
        FString                             SuperName;
        FString                             ConfigName;
        TArray<FFunctionTypeDefinition>     Functions;
        TArray<FString>                     Interfaces;
    };
}
