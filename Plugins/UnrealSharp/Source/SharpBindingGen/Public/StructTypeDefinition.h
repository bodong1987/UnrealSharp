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

#include "BaseTypeDefinition.h"
#include "PropertyDefinition.h"

namespace UnrealSharp
{
    // definition of UStruct(not UScriptStruct)
    class SHARPBINDINGGEN_API FStructTypeDefinition : public FBaseTypeDefinition
    {
    public:
        typedef FBaseTypeDefinition Super;

        FStructTypeDefinition();
        FStructTypeDefinition(UStruct* InStruct, FTypeValidation* InTypeValidation);

        virtual void                            Read(FJsonObject& InObject) override;
        virtual void                            Write(FJsonObject& InObject) override;

        virtual bool                            IsSupportedProperty(FProperty* InProperty, FTypeValidation* InTypeValidation);
        virtual bool                            IsSupportedElementProperty(FProperty* InProperty, FTypeValidation* InTypeValidation);
        virtual bool                            IsSupportedFunction(UFunction* InFunction, FTypeValidation* InTypeValidation);

        FPropertyDefinition*                    GetPropertyDefinition(const FString& InPropertyName);
        const FPropertyDefinition*              GetPropertyDefinition(const FString& InPropertyName) const;

        void                                    AddDependNamespace(const FProperty* InProperty);
        void                                    AddDependNamespace(const UField* InField);

    protected:
        void                                    LoadProperties(UStruct* InStruct, void* InDefaultObjectPtr, EFieldIterationFlags InFlags, FTypeValidation* InTypeValidation, TFunction<bool(FProperty*)> InAccessFunc);

    public:
        TArray<FPropertyDefinition>             Properties;
        TSortedMap<FString, int>                DependNamespaces;        
    };
}