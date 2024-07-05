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

class UCSharpBlueprint;

namespace UnrealSharp
{
    class FTypeDefinitionDocument;
    class FBaseTypeDefinition;
    class FPropertyDefinition;
    class FClassTypeDefinition;
    class FScriptStructTypeDefinition;
    class FEnumTypeDefinition;

    // current asset state
    enum class ECSharpGeneratedTypeState
    {
        Undefined,
        NeedUpdate,
        Completed
    };

    // base info
    struct UNREALSHARPEDITOR_API FCSharpGeneratedTypeInfo
    {
    public:
        UField*                                 Field = nullptr;
        UCSharpBlueprint*                       Blueprint = nullptr;
        FString                                 Name;
        FString                                 CppName;
        FString                                 PackagePath;
        FString                                 FilePath;
        ECSharpGeneratedTypeState               State = ECSharpGeneratedTypeState::Undefined;

        TSharedPtr<FBaseTypeDefinition>         Definition;

        bool                                    IsEnum() const;
        bool                                    IsStruct() const;
        bool                                    IsClass() const;
    };

    /*
    * The status of existing C# imported assets is used to determine which assets need to be re-imported, 
    & which need to be deleted, which need to be retained, etc.
    */
    class UNREALSHARPEDITOR_API FCSharpBlueprintGeneratorDatabase
    {
    public:
        FCSharpBlueprintGeneratorDatabase(const TSharedPtr<FTypeDefinitionDocument>& InDocument);
        ~FCSharpBlueprintGeneratorDatabase();

        void                                    Accept(const TFunction<void(FCSharpGeneratedTypeInfo&)>& InVisitor);

        FCSharpGeneratedTypeInfo*               FindTypeByName(const FString& InName);
        const FCSharpGeneratedTypeInfo*         FindTypeByName(const FString& InName) const;

        FCSharpGeneratedTypeInfo*               FindTypeByCppName(const FString& InCppName);
        const FCSharpGeneratedTypeInfo*         FindTypeByCppName(const FString& InCppName) const;

        UField*                                 FindNativeTypeByPath(const FString& InPath) const;
        UField*                                 FindNativeTypeByName(const FString& InName) const;
        UField*                                 FindNativeTypeByCppName(const FString& InCppName) const;

        UField*                                 GetField(const FPropertyDefinition& InPropertyDefinition) const;
        UField*                                 GetField(const FString& InCppName) const;

    private:
        void                                    PrepareBuild();
        void                                    CacheNativeTypes();
        void                                    LoadExistsInfo();
        void                                    PrepareTypeStates();
        void                                    PrepareTypes();        
        void                                    CleanCSharpBlueprintType(const TSharedPtr<FCSharpGeneratedTypeInfo>& InTypeInfo, const TSharedPtr<FBaseTypeDefinition>& InTypeDefinition);

        void                                    NewCSharpBlueprintClassIfNeed(UPackage* InPackage, const TSharedPtr<FClassTypeDefinition>& InClassDefinition, const TSharedPtr<FCSharpGeneratedTypeInfo>& InTypeInfo);
        void                                    DeleteAsset(const TSharedPtr<FCSharpGeneratedTypeInfo>& InTypeInfo);

        TSharedPtr<FCSharpGeneratedTypeInfo>    NewCSharpBlueprintType(const TSharedPtr<FBaseTypeDefinition>& InTypeDefinition);
    private:
        TSharedPtr<FTypeDefinitionDocument>                   Document;
        TMap<FString, TSharedPtr<FCSharpGeneratedTypeInfo>>   NameToTypeInfoCaches;
        TMap<FString, TSharedPtr<FCSharpGeneratedTypeInfo>>   CppNameToTypeInfoCaches;

        TMap<FString, UField*>                                PathToFieldNativeCaches;
        TMap<FString, UField*>                                NameToFieldNativeCaches;
        TMap<FString, UField*>                                CppNameToFieldNativeCaches;
    };
}
