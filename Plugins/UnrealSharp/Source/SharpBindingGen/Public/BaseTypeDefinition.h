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

#include "MetaDefinition.h"

namespace UnrealSharp
{
    // The kind of exported type definition
    enum class EDefinitionType
    {
        None,
        Enum,
        Struct,
        Class,
        Function,
        Interface
    };

    class FTypeValidation;

    // Base class for exported type data. Most exported types are derived from this class.
    class SHARPBINDINGGEN_API FBaseTypeDefinition
    {
    public:
        FBaseTypeDefinition();
        FBaseTypeDefinition(UField* InField, FTypeValidation* InTypeValidation);

        virtual ~FBaseTypeDefinition();

        // Read this type from JsonDoc
        virtual void                    Read(FJsonObject& InObject);

        // Write this type to JsonDoc
        virtual void                    Write(FJsonObject& InObject);

        static FString                  GetCppTypeName(UField* InField);
        static FString                  GetBlueprintFieldPackageName(const FString& InPath);

        inline EDefinitionType          GetDefinitionType() const { return (EDefinitionType)Type; }
        inline bool                     IsEnum() const { return (EDefinitionType)Type == EDefinitionType::Enum; }
        inline bool                     IsStruct() const { return (EDefinitionType)Type == EDefinitionType::Struct; }
        inline bool                     IsClass() const { return (EDefinitionType)Type == EDefinitionType::Class; }
        inline bool                     IsFunction() const { return (EDefinitionType)Type == EDefinitionType::Function; }
        inline bool                     IsInterface() const { return (EDefinitionType)Type == EDefinitionType::Interface; }
    protected:
        int                             Type = (int)EDefinitionType::None;

    public:        
        FString                         Name;
        FString                         CppName;
        FString                         PathName;
        FString                         PackageName;
        FString                         ProjectName;
        FString                         Namespace;
        FString                         AssemblyName;
        FString                         CSharpFullName;
        uint64                          Flags = 0;
        int64                           CrcCode = 0;
        FGuid                           Guid;
        int                             Size = 0;

        FMetaDefinition                 Meta;
    };
}
