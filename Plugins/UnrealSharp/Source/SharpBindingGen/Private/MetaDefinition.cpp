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
#include "MetaDefinition.h"

namespace UnrealSharp
{
    void FMetaDefinition::Write(FJsonObject& InObject)
    {
        if (!Metas.IsEmpty())
        {
            TArray<TSharedPtr<FJsonValue>> MetaValues;

            for (const auto& Pair : Metas)
            {
                TSharedPtr<FJsonObject> MetaPtr = MakeShared<FJsonObject>();
                MetaPtr->SetStringField(TEXT("Name"), Pair.Key);
                MetaPtr->SetStringField(TEXT("Value"), Pair.Value);

                MetaValues.Add(MakeShared<FJsonValueObject>(MetaPtr));
            }

            InObject.SetArrayField("Metas", MetaValues);
        }        
    }

    void FMetaDefinition::Read(const FJsonObject& InObject)
    {
        if (const TArray< TSharedPtr<FJsonValue> >* MetasRefPtr = nullptr; InObject.TryGetArrayField(TEXT("Metas"), MetasRefPtr) && MetasRefPtr)
        {
            for (auto& MetaObject : *MetasRefPtr)
            {
                if (TSharedPtr<FJsonObject>* ObjectPtr = nullptr; MetaObject->TryGetObject(ObjectPtr) && ObjectPtr != nullptr)
                {
                    FString Key = (*ObjectPtr)->GetStringField(TEXT("Name"));
                    FString Value = (*ObjectPtr)->GetStringField(TEXT("Value"));

                    Metas.Add(Key, Value);
                }
            }
        }        
    }

    void FMetaDefinition::Reset()
    {
        Metas.Empty();
    }

    void FMetaDefinition::Load(UField* InField)
    {
        Reset();
        
        if (UPackage* SourcePackage = InField->GetOutermost())
        {
            if (const UMetaData* SourceMetaData = SourcePackage->GetMetaData())
            {
                if (const TMap<FName, FString>* SourceObjectMetaData = SourceMetaData->ObjectMetaDataMap.Find(InField))
                {
                    for (auto& Pair : *SourceObjectMetaData)
                    {
                        Metas.Add(Pair.Key.ToString(), Pair.Value);
                    }
                }
            }
        }
    }

    void FMetaDefinition::Load(const FProperty* InProperty)
    {
        Reset();

        if (const TMap<FName, FString>* PropertyMetaDataMap = InProperty->GetMetaDataMap(); PropertyMetaDataMap != nullptr)
        {
            for (auto& Pair : *PropertyMetaDataMap)
            {
                Metas.Add(Pair.Key.ToString(), Pair.Value);
            }
        }
    }

    bool FMetaDefinition::TryGetMeta(const FString& InKey, FString& OutMeta) const
    {
        if (const auto* Result = Metas.Find(InKey); Result != nullptr)
        {
            OutMeta = *Result;

            return true;
        }

        return false;
    }

    bool FMetaDefinition::TryGetMeta(const FString& InKey, bool& OutMeta) const
    {
        if (const auto* Result = Metas.Find(InKey); Result != nullptr)
        {
            OutMeta = FCString::Stricmp(**Result, TEXT("True")) == 0;

            return true;
        }

        return false;
    }

    bool FMetaDefinition::TryGetMeta(const FString& InKey, int& OutMeta) const
    {
        if (const auto* Result = Metas.Find(InKey); Result != nullptr)
        {
            OutMeta = FCString::Atoi(**Result);

            return true;
        }

        return false;
    }

    bool FMetaDefinition::HasMeta(const FString& InKey) const
    {
        return Metas.Find(InKey) != nullptr;
    }
}
