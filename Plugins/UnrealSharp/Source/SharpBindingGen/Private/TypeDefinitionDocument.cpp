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
#include "TypeDefinitionDocument.h"
#include "SharpBindingGenSettings.h"
#include "EnumTypeDefinition.h"
#include "ScriptStructTypeDefinition.h"
#include "ClassTypeDefinition.h"
#include "TypeValidation.h"
#include "Misc/UnrealSharpUtils.h"

namespace UnrealSharp
{   
    FTypeDefinitionDocument::FTypeDefinitionDocument()
    {
    }

    FTypeDefinitionDocument::~FTypeDefinitionDocument()
    {
    }

    FTypeDefinitionDocument::FTypeDefinitionPtr FTypeDefinitionDocument::GetType(const FString& InCppName)
    {
        auto* Ptr = Types.Find(InCppName);

        return Ptr != nullptr?*Ptr:FTypeDefinitionPtr();
    }

    bool FTypeDefinitionDocument::LoadFromEngine(const ETypeValidationFlags InFlags)
    {
        FTypeValidation TypeValidation;

        return LoadFromEngine(&TypeValidation, InFlags);
    }

    bool FTypeDefinitionDocument::LoadFromEngine(FTypeValidation* InTypeValidation, const ETypeValidationFlags InFlags)
    {
        Reset();

        const auto Settings = GetDefault<USharpBindingGenSettings>();

        for (UField* Field : InTypeValidation->GetSupportedFields())
        {
            if (!InTypeValidation->IsNeedExport(Field))
            {
                continue;
            }

            const bool bIsNativeField = FUnrealSharpUtils::IsNativeField(Field);

            if (const bool bIsBlueprintField = FUnrealSharpUtils::IsBlueprintField(Field); (bIsNativeField && (InFlags & ETypeValidationFlags::WithNativeType)) ||
                (bIsBlueprintField && (InFlags & ETypeValidationFlags::WithBlueprintType))
                )
            {
                if (auto TypeDefinition = CreateTypeDefinition(Field, InTypeValidation); TypeDefinition != nullptr)
                {
                    Types.Add(TypeDefinition->CppName, TypeDefinition);
                }
            }
        }

        FastAccessStructTypes = Settings->FastAccessStructTypeNames;
        FastFunctionInvokeModuleNames = Settings->FastFunctionInvokeModuleNames;
        FastFunctionInvokeIgnoreNames = Settings->FastFunctionInvokeIgnoreNames;
        FastFunctionInvokeIgnoreClassNames = Settings->FastFunctionInvokeIgnoreClassNames;

        if (Settings->bEnableFastFunctionInvoke)
        {
            DocumentAttributes |= static_cast<int>(ETypeDefinitionDocumentAttributes::AllowFastInvokeGeneration);
        }

        return true;
    }

    FTypeDefinitionDocument::FTypeDefinitionPtr FTypeDefinitionDocument::CreateTypeDefinition(UField* InField, FTypeValidation* InTypeValidation) const
    {
        if (UEnum* Enum = Cast<UEnum>(InField))
        {
            return MakeShared<FEnumTypeDefinition>(Enum, InTypeValidation);
        }

        if (UScriptStruct* Struct = Cast<UScriptStruct>(InField))
        {
            return MakeShared<FScriptStructTypeDefinition>(Struct, InTypeValidation);
        }

        if (UClass* Class = Cast<UClass>(InField))
        {
            return MakeShared<FClassTypeDefinition>(Class, InTypeValidation);
        }

        return FTypeDefinitionPtr();
    }

    template <typename T>
    void FTypeDefinitionDocument::SaveStringCollection(TSharedPtr<FJsonObject>& InDocument, const T& InCollectionRef, const FString& InName)
    {
        if (!InCollectionRef.IsEmpty())
        {
            TArray<TSharedPtr<FJsonValue>> TempTypes;

            for (auto& Name : InCollectionRef)
            {
                TSharedRef<FJsonValue> JsonValue = MakeShareable(new FJsonValueString(Name));
                TempTypes.Add(JsonValue);
            }

            InDocument->SetArrayField(InName, TempTypes);
        }
    }

    template <typename T>
    void FTypeDefinitionDocument::ReadStringCollection(TSharedPtr<FJsonObject>& InDocument, T& InCollectionRef, const FString& InName)
    {
        if (InDocument->HasField(InName))
        {
            for (const TArray< TSharedPtr<FJsonValue> >& TempArray = InDocument->GetArrayField(InName); auto& JsonValue : TempArray)
            {
                FString Name = JsonValue->AsString();

                InCollectionRef.Add(Name);
            }
        }
    }

    bool FTypeDefinitionDocument::LoadFromFile(const TCHAR* InFilePath)
    {
        Reset();

        FString JsonString;
        if (!FFileHelper::LoadFileToString(JsonString, InFilePath))
        {
            return false;
        }

        TSharedPtr<FJsonObject> JsonObject;

        if (const TSharedPtr<TJsonReader<>> JsonReader = TJsonReaderFactory<>::Create(JsonString);
            !JsonReader ||
            !FJsonSerializer::Deserialize(JsonReader.ToSharedRef(), JsonObject) ||
            !JsonObject.IsValid())
        {
            return false;
        }

        JsonObject->TryGetNumberField(TEXT("UnrealMajorVersion"), UnrealMajorVersion);
        JsonObject->TryGetNumberField(TEXT("UnrealMinorVersion"), UnrealMinorVersion);
        JsonObject->TryGetNumberField(TEXT("UnrealPatchVersion"), UnrealPatchVersion);
        JsonObject->TryGetNumberField(TEXT("DocumentAttributes"), DocumentAttributes);

        for (const TArray< TSharedPtr<FJsonValue> >& TempTypes = JsonObject->GetArrayField(TEXT("Types"));
            auto& JSONValuePtr : TempTypes)
        {
            if (TSharedPtr<FJsonObject>* TypedJsonObject; JSONValuePtr->TryGetObject(TypedJsonObject) && TypedJsonObject)
            {
                const EDefinitionType Type = static_cast<EDefinitionType>((*TypedJsonObject)->GetNumberField(TEXT("Type")));

                if (auto TypeDefinition = CreateTypeDefinition(Type))
                {
                    TypeDefinition->Read(**TypedJsonObject);

                    Types.Add(TypeDefinition->CppName, TypeDefinition);
                }
            }
        }

        ReadStringCollection(JsonObject, FastAccessStructTypes, TEXT("FastAccessStructTypes"));
        ReadStringCollection(JsonObject, FastFunctionInvokeModuleNames, TEXT("FastFunctionInvokeModuleNames"));
        ReadStringCollection(JsonObject, FastFunctionInvokeIgnoreClassNames, TEXT("FastFunctionInvokeIgnoreClassNames"));
        ReadStringCollection(JsonObject, FastFunctionInvokeIgnoreNames, TEXT("FastFunctionInvokeIgnoreNames"));

        return true;
    }

    FTypeDefinitionDocument::FTypeDefinitionPtr FTypeDefinitionDocument::CreateTypeDefinition(const EDefinitionType InType) const
    {
        switch (InType)
        {
        case EDefinitionType::None:
            break;
        case EDefinitionType::Enum:
            return MakeShared<FEnumTypeDefinition>();
        case EDefinitionType::Struct:
            return MakeShared<FScriptStructTypeDefinition>();
        case EDefinitionType::Class:
            return MakeShared<FClassTypeDefinition>();
        case EDefinitionType::Function:
            return MakeShared<FFunctionTypeDefinition>();
        default:
            break;
        }

        return FTypeDefinitionPtr();
    }

    void FTypeDefinitionDocument::Reset()
    {
        Types.Empty();
        FastAccessStructTypes.Empty();
        FastFunctionInvokeModuleNames.Empty();
        FastFunctionInvokeIgnoreClassNames.Empty();
        FastFunctionInvokeIgnoreNames.Empty();
    }

    bool FTypeDefinitionDocument::SaveToFile(const TCHAR* InFilePath)
    {
        TSharedPtr<FJsonObject> Doc = MakeShared<FJsonObject>();

        TArray<TSharedPtr<FJsonValue>> TempTypes;
        
        for (const auto& Pair : Types)
        {
            TSharedPtr<FJsonObject> TypeObject = MakeShared<FJsonObject>();
            Pair.Value->Write(*TypeObject);

            TempTypes.Add(MakeShared<FJsonValueObject>(TypeObject));
        }

        Doc->SetNumberField("UnrealMajorVersion", UnrealMajorVersion);
        Doc->SetNumberField("UnrealMinorVersion", UnrealMinorVersion);
        Doc->SetNumberField("UnrealPatchVersion", UnrealPatchVersion);
        Doc->SetNumberField("DocumentAttributes", DocumentAttributes);
        Doc->SetArrayField("Types", TempTypes);

        SaveStringCollection(Doc, FastAccessStructTypes, TEXT("FastAccessStructTypes"));
        SaveStringCollection(Doc, FastFunctionInvokeModuleNames, TEXT("FastFunctionInvokeModuleNames"));
        SaveStringCollection(Doc, FastFunctionInvokeIgnoreClassNames, TEXT("FastFunctionInvokeIgnoreClassNames"));
        SaveStringCollection(Doc, FastFunctionInvokeIgnoreNames, TEXT("FastFunctionInvokeIgnoreNames"));

        FString JsonString;

        if (const TSharedPtr<TJsonWriter<>> JsonWriter = TJsonWriterFactory<>::Create(&JsonString);
            FJsonSerializer::Serialize(Doc.ToSharedRef(), JsonWriter.ToSharedRef()))
        {
            return FFileHelper::SaveStringToFile(JsonString, InFilePath);
        }

        return false;
    }

    void FTypeDefinitionDocument::Merge(const FTypeDefinitionDocument& InDocument)
    {
        for (auto& Pair : InDocument.Types)
        {
            if (Types.Contains(Pair.Key))
            {
                UE_LOG(LogTemp, Warning, TEXT("[UnrealSharp]Skip type:%s, already exists!"), *Pair.Key);
            }
            else
            {
                Types.Add(Pair.Key, Pair.Value);
            }
        }

        for (auto& Type : InDocument.FastAccessStructTypes)
        {
            FastAccessStructTypes.Add(Type);
        }

        for (auto& Type : InDocument.FastFunctionInvokeModuleNames)
        {
            FastFunctionInvokeModuleNames.Add(Type);
        }

        for (auto& Type : InDocument.FastFunctionInvokeIgnoreNames)
        {
            FastFunctionInvokeIgnoreNames.Add(Type);
        }

        for (auto& Type : InDocument.FastFunctionInvokeIgnoreClassNames)
        {
            FastFunctionInvokeIgnoreClassNames.Add(Type);
        }
    }
}
