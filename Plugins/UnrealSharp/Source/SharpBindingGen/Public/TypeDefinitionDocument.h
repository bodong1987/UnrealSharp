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
#include "BaseTypeDefinition.h"

namespace UnrealSharp
{
    class FTypeValidation;
    enum class ETypeValidationFlags;

    enum class ETypeDefinitionDocumentAttributes 
    {
        None,
        AllowFastInvokeGeneration = 1 << 0
    };
    
    /*
    * This is the class of the type export document. 
    * It contains all necessary export information. 
    * This information will fully express the static information of UClass, UStruct, UEnum, UFunction, and FProperty. 
    * They will be used to generate C# binding code, or Help generate blueprint classes, structures, enumerations, functions, etc.
    */
    class SHARPBINDINGGEN_API FTypeDefinitionDocument
    {
    public:
        typedef TSharedPtr<FBaseTypeDefinition>         TypeDefinitionPtr;

        FTypeDefinitionDocument();
        virtual ~FTypeDefinitionDocument();

    public:
        virtual bool                                    LoadFromFile(const TCHAR* InFilePath);
        virtual bool                                    LoadFromEngine(ETypeValidationFlags InFlags);
        virtual bool                                    LoadFromEngine(FTypeValidation* InTypeValidation, ETypeValidationFlags InFlags);

        virtual bool                                    SaveToFile(const TCHAR* InFilePath);

        virtual void                                    Reset();

        void                                            Merge(const FTypeDefinitionDocument& InDocument);

        const TSortedMap<FString, TypeDefinitionPtr>&   GetTypes() const{ return Types; }
        TypeDefinitionPtr                               GetType(const FString& InCppName);
        
    protected:
        virtual TypeDefinitionPtr                       CreateTypeDefinition(UField* InField, FTypeValidation* InTypeValidation) const;
        virtual TypeDefinitionPtr                       CreateTypeDefinition(EDefinitionType InType) const;  

    private:
        template <typename T>
        void                                            SaveStringCollection(TSharedPtr<FJsonObject>& InDocument, const T& InCollectionRef, const FString& InName);
        
        template <typename T>
        void                                            ReadStringCollection(TSharedPtr<FJsonObject>& InDocument, T& InCollectionRef, const FString& InName);

    protected:
        TSortedMap<FString, TypeDefinitionPtr>          Types;        
        TSet<FString>                                   FastAccessStructTypes;
        TSet<FString>                                   FastFunctionInvokeModuleNames;
        TSet<FString>                                   FastFunctionInvokeIgnoreNames;
        TSet<FString>                                   FastFunctionInvokeIgnoreClassNames;
        int                                             UnrealMajorVersion = ENGINE_MAJOR_VERSION;
        int                                             UnrealMinorVersion = ENGINE_MINOR_VERSION;
        int                                             UnrealPatchVersion = ENGINE_PATCH_VERSION;
        int                                             DocumentAttributes = 0;
    };
}
