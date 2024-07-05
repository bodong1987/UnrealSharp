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

#include "EdGraph/EdGraphPin.h"

class UCSharpBlueprint;
class UCSharpStruct;

namespace UnrealSharp
{
    class FTypeDefinitionDocument;
    class FBaseTypeDefinition;
    class FEnumTypeDefinition;
    class FScriptStructTypeDefinition;
    class FClassTypeDefinition;
    class FPropertyDefinition;
    class FFunctionTypeDefinition;
    class FCSharpBlueprintGeneratorDatabase;
    struct FCSharpGeneratedTypeInfo;

    /*
    * The TypeDefinitionDocument created from the C# assembly automatically generates the main entrance of 
    * the blueprint class, blueprint structure, and blueprint enumeration.
    */
    class UNREALSHARPEDITOR_API FCSharpBlueprintGenerator
    {
    public:
        FCSharpBlueprintGenerator(TSharedPtr<FTypeDefinitionDocument> InDocument);
        ~FCSharpBlueprintGenerator();

        void            Process();

    private:
        bool            ProcessEnum(const FCSharpGeneratedTypeInfo* InInfo);
        bool            ProcessStruct(const FCSharpGeneratedTypeInfo* InInfo);
        bool            ProcessClass(FCSharpGeneratedTypeInfo* InInfo);
        bool            ProcessFunction(FCSharpGeneratedTypeInfo* InInfo, const FFunctionTypeDefinition& InFunctionDefinition);
        bool            ProcessDelegate(FCSharpGeneratedTypeInfo* InInfo, const FPropertyDefinition& InPropertyDefinition);
        bool            ProcessAutoAttachComponent(FCSharpGeneratedTypeInfo* InInfo, const FClassTypeDefinition* InClassTypeDefinition, const FPropertyDefinition& InPropertyDefinition, TSet<FName>& InProcessedNames);

    private:
        TSharedPtr<FTypeDefinitionDocument>                             Document;
        TSortedMap<FString, TSharedPtr<FEnumTypeDefinition>>            EnumTypes;
        TSortedMap<FString, TSharedPtr<FScriptStructTypeDefinition>>    StructTypes;
        TSortedMap<FString, TSharedPtr<FClassTypeDefinition>>           ClassTypes;

        TUniquePtr<FCSharpBlueprintGeneratorDatabase>                   Database;
    };
}
