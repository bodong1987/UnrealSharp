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
#include "CSharpBlueprintGeneratorDatabase.h"
#include "TypeDefinitionDocument.h"
#include "CSharpBlueprintGeneratorUtils.h"
#include "Classes/CSharpEnum.h"
#include "Classes/CSharpStruct.h"
#include "Classes/CSharpClass.h"
#include "Classes/CSharpBlueprint.h"
#include "UserDefinedStructure/UserDefinedStructEditorData.h"
#include "PropertyDefinition.h"
#include "ObjectTools.h"
#include "ClassTypeDefinition.h"
#include "Misc/UnrealSharpLog.h"
#include "AssetRegistry/AssetRegistryModule.h"
#include "Misc/ScopedExit.h"
#include "Misc/UnrealSharpUtils.h"

namespace UnrealSharp
{
    bool FCSharpGeneratedTypeInfo::IsEnum() const
    {
        if (Field != nullptr)
        {
            return Field->IsA<UCSharpEnum>();
        }

        return Definition != nullptr && Definition->GetDefinitionType() == EDefinitionType::Enum;
    }

    bool FCSharpGeneratedTypeInfo::IsStruct() const
    {
        if (Field != nullptr)
        {
            return Field->IsA<UCSharpStruct>();
        }

        return Definition != nullptr && Definition->GetDefinitionType() == EDefinitionType::Struct;
    }

    bool FCSharpGeneratedTypeInfo::IsClass() const
    {
        if (Field != nullptr)
        {
            return Field->IsA<UCSharpClass>();
        }

        return Definition != nullptr && Definition->GetDefinitionType() == EDefinitionType::Class;
    }

    FCSharpBlueprintGeneratorDatabase::FCSharpBlueprintGeneratorDatabase(TSharedPtr<FTypeDefinitionDocument> InDocument) :
        Document(InDocument)
    {
        CacheNativeTypes();
        PrepareBuild();
    }

    FCSharpBlueprintGeneratorDatabase::~FCSharpBlueprintGeneratorDatabase()
    {
    }

    void FCSharpBlueprintGeneratorDatabase::CacheNativeTypes()
    {
        for (TObjectIterator<UField> Iter; Iter; ++Iter)
        {
            UField* Field = *Iter;

            if (Field->IsA<UScriptStruct>() ||
                Field->IsA<UEnum>() ||
                Field->IsA<UClass>())
            {
                PathToFieldNativeCaches.Add(Field->GetPathName(), Field);
                NameToFieldNativeCaches.Add(Field->GetName(), Field);

                FString CppName = FUnrealSharpUtils::GetCppTypeName(Field);

                CppNameToFieldNativeCaches.Add(CppName, Field);
            }
        }
    }

    void FCSharpBlueprintGeneratorDatabase::LoadExistsInfo()
    {
        // get directory path
        FString GeneratedPath = FPaths::Combine(FPaths::ProjectContentDir(), FCSharpBlueprintGeneratorUtils::CSharpBlueprintClassPrefixPath);

        TArray<FString> ExistsFiles;
        IFileManager::Get().IterateDirectoryRecursively(*GeneratedPath,
            [&, this](const TCHAR* InFileName, bool bIsDirectory) -> bool
            {
                if (!bIsDirectory && FString(InFileName).EndsWith(FPackageName::GetAssetPackageExtension()))
                {
                    FString RelativePath = InFileName;
                    FPaths::MakePathRelativeTo(RelativePath, *GeneratedPath);

                    FString PackagePath = FString::Printf(TEXT("/Game/%s"), *FPaths::GetBaseFilename(RelativePath, false));

                    FString Name = FPaths::GetBaseFilename(InFileName);
                    FString CppName = Name;

                    UField* TargetField;
                    UObject* AssetObject = FCSharpBlueprintGeneratorUtils::LoadObject<UObject>(PackagePath);

                    if (AssetObject == nullptr)
                    {
                        US_LOG_WARN(TEXT("Delete outdated C# generated asset:%s[%s]"), *PackagePath, InFileName);
                        IFileManager::Get().Delete(InFileName);
                        return true;
                    }

                    // checkf(AssetObject, TEXT("Failed load asset from %s"), *PackagePath);

                    UCSharpBlueprint* Blueprint = nullptr;

                    if (AssetObject->IsA<UCSharpStruct>())
                    {
                        UCSharpStruct* CSharpStruct = Cast<UCSharpStruct>(AssetObject);
                        TargetField = CSharpStruct;
                        CppName = CSharpStruct->GetCSharpTypeName();
                    }
                    else if (AssetObject->IsA<UCSharpBlueprint>())
                    {
                        Blueprint = Cast<UCSharpBlueprint>(AssetObject);
                        check(Blueprint);

                        AssetObject = Blueprint->GeneratedClass;

                        UCSharpClass* Class = Cast<UCSharpClass>(AssetObject);
                        check(Class);

                        CppName = Class->GetCSharpTypeName();

                        TargetField = Class;
                    }
                    else if (AssetObject->IsA<UCSharpEnum>())
                    {
                        UCSharpEnum* CSharpEnum = Cast<UCSharpEnum>(AssetObject);
                        CppName = CSharpEnum->GetCSharpTypeName();

                        TargetField = CSharpEnum;
                    }
                    else
                    {
                        // unsupported type ???
                        checkNoEntry();
                        TargetField = nullptr;
                    }

                    TSharedPtr<FCSharpGeneratedTypeInfo> InfoPtr = MakeShared<FCSharpGeneratedTypeInfo>();
                    FCSharpGeneratedTypeInfo& Info = *InfoPtr;

                    Info.Field = TargetField;
                    Info.Blueprint = Blueprint;
                    Info.State = ECSharpGeneratedTypeState::Undefined;
                    Info.Name = Name;
                    Info.CppName = CppName;
                    Info.PackagePath = PackagePath;
                    Info.FilePath = InFileName;

                    NameToTypeInfoCaches.Add(Name, InfoPtr);
                    CppNameToTypeInfoCaches.Add(CppName, InfoPtr);
                }

                return true;
            }
        );

        check(NameToTypeInfoCaches.Num() == CppNameToTypeInfoCaches.Num());
    }

    void FCSharpBlueprintGeneratorDatabase::DeleteAsset(TSharedPtr<FCSharpGeneratedTypeInfo> InTypeInfo)
    {
        UObject* TargetObject = InTypeInfo->Blueprint != nullptr ? (UObject*)InTypeInfo->Blueprint : (UObject*)InTypeInfo->Field;

        check(TargetObject!=nullptr);

        ObjectTools::DeleteSingleObject(TargetObject, false);

        NameToTypeInfoCaches.Remove(InTypeInfo->Name);
        CppNameToTypeInfoCaches.Remove(InTypeInfo->CppName);

        check(NameToTypeInfoCaches.Num() == CppNameToTypeInfoCaches.Num());
    }

    void FCSharpBlueprintGeneratorDatabase::PrepareTypeStates()
    {
        // try delete outdated assets
        auto TempCppNameToTypeInfoCaches = CppNameToTypeInfoCaches;

        for (auto& pair : TempCppNameToTypeInfoCaches)
        {
            auto typeDefinition = Document->GetType(pair.Key);

            // this resource it not valid now
            if (!typeDefinition)
            {
                DeleteAsset(pair.Value);
            }
        }

        // try update or create assets
        for (auto& pair : Document->GetTypes())
        {
            auto& type = pair.Value;

            auto* infoPtr = NameToTypeInfoCaches.Find(type->Name);

            if (infoPtr != nullptr) // if this generated type already exists...
            {
                TSharedPtr<FCSharpGeneratedTypeInfo> info = *infoPtr;

                check(info->Field); // field must be valid.

                info->Definition = type;  // force reset Definition for it

                // check crccode and generator version
                if (Cast<ICSharpGeneratedType>(info->Field)->GetCrcCode() == type->CrcCode &&
                    Cast<ICSharpGeneratedType>(info->Field)->GetGeneratorVersion() == FCSharpBlueprintGeneratorUtils::GeneratorVersion)
                {
                    info->State = ECSharpGeneratedTypeState::Completed;
                    continue;
                }
                else
                {
                    info->State = ECSharpGeneratedTypeState::NeedUpdate;
                }
            }
        }

        check(NameToTypeInfoCaches.Num() == CppNameToTypeInfoCaches.Num());
    }

    void FCSharpBlueprintGeneratorDatabase::PrepareTypes()
    {
        // check states
        for (auto& pair : Document->GetTypes())
        {
            auto type = pair.Value;

            const auto* Ptr = NameToTypeInfoCaches.Find(type->Name);
            if (Ptr != nullptr)
            {
                if ((*Ptr)->State == ECSharpGeneratedTypeState::NeedUpdate)
                {
                    CleanCSharpBlueprintType(*Ptr, type);
                }                
            }
            else
            {
                NewCSharpBlueprintType(type);
            }
        }
    }

    void FCSharpBlueprintGeneratorDatabase::CleanCSharpBlueprintType(TSharedPtr<FCSharpGeneratedTypeInfo> InTypeInfo, TSharedPtr<FBaseTypeDefinition> InTypeDefinition)
    {
        if (InTypeDefinition->IsEnum())
        {
            UCSharpEnum* CSharpEnum = Cast<UCSharpEnum>(InTypeInfo->Field);
            check(CSharpEnum);

            FCSharpBlueprintGeneratorUtils::CleanCSharpEnum(CSharpEnum);
        }
        else if (InTypeInfo->IsStruct())
        {
            UCSharpStruct* CSharpStruct = Cast<UCSharpStruct>(InTypeInfo->Field);
            check(CSharpStruct);

            FCSharpBlueprintGeneratorUtils::CleanCSharpStruct(CSharpStruct);
        }
        else if (InTypeInfo->IsClass())
        {
            UCSharpBlueprint* Blueprint = Cast<UCSharpBlueprint>(InTypeInfo->Blueprint);
            check(Blueprint);

            UCSharpClass* Class = Cast<UCSharpClass>(Blueprint->GeneratedClass);
            check(Class != nullptr && Class == InTypeInfo->Field);

            FCSharpBlueprintGeneratorUtils::CleanCSharpClass(Blueprint, Class);
        }
    }

    TSharedPtr<FCSharpGeneratedTypeInfo> FCSharpBlueprintGeneratorDatabase::NewCSharpBlueprintType(TSharedPtr<FBaseTypeDefinition> InTypeDefinition)
    {
        US_LOG(TEXT("Prepare C# type:%s"), *InTypeDefinition->CSharpFullName);

        TSharedPtr<FCSharpGeneratedTypeInfo> typeInfoPtr = MakeShared<FCSharpGeneratedTypeInfo>();

        typeInfoPtr->State = ECSharpGeneratedTypeState::NeedUpdate;
        typeInfoPtr->Name = InTypeDefinition->Name;
        typeInfoPtr->CppName = InTypeDefinition->CppName;
        typeInfoPtr->PackagePath = FCSharpBlueprintGeneratorUtils::GetPackagePath(InTypeDefinition);
        typeInfoPtr->FilePath = FCSharpBlueprintGeneratorUtils::GetPackageFilePath(InTypeDefinition);
        typeInfoPtr->Definition = InTypeDefinition;

        UNREALSHARP_SCOPED_EXIT(
            if (typeInfoPtr)
            {
                CppNameToTypeInfoCaches.Add(typeInfoPtr->CppName, typeInfoPtr);
                NameToTypeInfoCaches.Add(typeInfoPtr->Name, typeInfoPtr);
                check(CppNameToTypeInfoCaches.Num() == NameToTypeInfoCaches.Num());
            }            
        );

        UPackage* Package = CreatePackage(*typeInfoPtr->PackagePath);

        // create place holder data
        if (InTypeDefinition->IsEnum())
        {
            typeInfoPtr->Field = FCSharpBlueprintGeneratorUtils::NewCSharpEnum(Package, (const FEnumTypeDefinition*)InTypeDefinition.Get());

            return typeInfoPtr;
        }
        else if(InTypeDefinition->IsStruct())
        {
            typeInfoPtr->Field = FCSharpBlueprintGeneratorUtils::NewCSharpStruct(Package, (const FScriptStructTypeDefinition*)InTypeDefinition.Get());

            return typeInfoPtr;
        }

        checkSlow(!InTypeDefinition->IsFunction());
        
        // class require special handling, because it has base class
        TSharedPtr<FClassTypeDefinition> ClassPtr = StaticCastSharedPtr<FClassTypeDefinition>(InTypeDefinition);
        checkSlow(ClassPtr);

        NewCSharpBlueprintClassIfNeed(Package, ClassPtr, typeInfoPtr);

        return typeInfoPtr;
    }

    void FCSharpBlueprintGeneratorDatabase::NewCSharpBlueprintClassIfNeed(UPackage* InPackage, TSharedPtr<FClassTypeDefinition> InClassDefinition, TSharedPtr<FCSharpGeneratedTypeInfo> InTypeInfo)
    {
        const FString& SuperName = InClassDefinition->SuperName;

        UClass* SuperClass = Cast<UClass>(FindNativeTypeByCppName(SuperName));
        const bool bIsUnrealSuperClass = SuperClass != nullptr;

        if (!bIsUnrealSuperClass)
        {
            // try find in caches
            auto SuperInfo = FindTypeByCppName(SuperName);

            if (SuperInfo == nullptr)
            {
                auto SuperDefinition = Document->GetType(SuperName);
                checkf(SuperDefinition, TEXT("Failed find super type name:%s, If you refactor your C++ code, you may need to delete $(Project)/Content/CSharpBlueprints and $(Project)/Managed and then re-execute the import process"), *SuperName);

                NewCSharpBlueprintType(SuperDefinition);

                SuperInfo = FindTypeByCppName(SuperName);

                checkf(SuperInfo, TEXT("Failed find super type name:%s, If you refactor your C++ code, you may need to delete $(Project)/Content/CSharpBlueprints and $(Project)/Managed and then re-execute the import process"), *SuperName);                
            }

            SuperClass = Cast<UClass>(SuperInfo->Field);            
        }

        check(SuperClass);

        InTypeInfo->Blueprint = FCSharpBlueprintGeneratorUtils::NewCSharpBlueprint(InPackage, InClassDefinition.Get(), SuperClass, this);
        check(InTypeInfo->Blueprint);

        InTypeInfo->Field = InTypeInfo->Blueprint->GeneratedClass;
        check(InTypeInfo->Field);
    }

    void FCSharpBlueprintGeneratorDatabase::PrepareBuild()
    {
        LoadExistsInfo();

        PrepareTypeStates();

        PrepareTypes();
    }

    void FCSharpBlueprintGeneratorDatabase::Accept(TFunction<void(FCSharpGeneratedTypeInfo&)> InVisitor)
    {
        for (auto& pair : NameToTypeInfoCaches)
        {
            check(pair.Value->Definition);
            InVisitor(*pair.Value);
        }
    }

    FCSharpGeneratedTypeInfo* FCSharpBlueprintGeneratorDatabase::FindTypeByName(const FString& InName)
    {
        auto ptr = NameToTypeInfoCaches.Find(InName);

        return ptr != nullptr ? ptr->Get() : nullptr;
    }

    const FCSharpGeneratedTypeInfo* FCSharpBlueprintGeneratorDatabase::FindTypeByName(const FString& InName) const
    {
        auto ptr = NameToTypeInfoCaches.Find(InName);

        return ptr != nullptr ? ptr->Get() : nullptr;
    }

    FCSharpGeneratedTypeInfo* FCSharpBlueprintGeneratorDatabase::FindTypeByCppName(const FString& InCppName)
    {
        auto ptr = CppNameToTypeInfoCaches.Find(InCppName);

        return ptr != nullptr ? ptr->Get() : nullptr;
    }

    const FCSharpGeneratedTypeInfo* FCSharpBlueprintGeneratorDatabase::FindTypeByCppName(const FString& InCppName) const
    {
        auto ptr = CppNameToTypeInfoCaches.Find(InCppName);

        return ptr != nullptr ? ptr->Get() : nullptr;
    }

    UField* FCSharpBlueprintGeneratorDatabase::FindNativeTypeByPath(const FString& InPath) const
    {
        auto* ptr = PathToFieldNativeCaches.Find(InPath);

        if (ptr != nullptr)
        {
            return *ptr;
        }

        return nullptr;
    }

    UField* FCSharpBlueprintGeneratorDatabase::FindNativeTypeByName(const FString& InName) const
    {
        auto* ptr = NameToFieldNativeCaches.Find(InName);

        return ptr != nullptr ? *ptr : nullptr;
    }

    UField* FCSharpBlueprintGeneratorDatabase::FindNativeTypeByCppName(const FString& InCppName) const
    {
        auto* ptr = CppNameToFieldNativeCaches.Find(InCppName);

        return ptr != nullptr ? *ptr : nullptr;
    }

    UField* FCSharpBlueprintGeneratorDatabase::GetField(const FString& InCppName) const
    {
        auto Field = FindNativeTypeByCppName(InCppName);
        if (Field != nullptr)
        {
            return Field;
        }

        auto info = FindTypeByCppName(InCppName);

        if (info != nullptr)
        {
            return info->Field;
        }

        return nullptr;
    }

    UField* FCSharpBlueprintGeneratorDatabase::GetField(const FPropertyDefinition& InPropertyDefinition) const
    {        
        if (InPropertyDefinition.IsClassProperty() && !InPropertyDefinition.MetaClass.IsEmpty())
        {
            return GetField(InPropertyDefinition.MetaClass);
        }

        if (InPropertyDefinition.ReferenceType == EReferenceType::UnrealType)
        {
            return FindNativeTypeByPath(InPropertyDefinition.ClassPath);
        }

        auto info = FindTypeByName(InPropertyDefinition.TypeName);

        if (info != nullptr)
        {
            return info->Field;
        }

        return nullptr;
    }
}
