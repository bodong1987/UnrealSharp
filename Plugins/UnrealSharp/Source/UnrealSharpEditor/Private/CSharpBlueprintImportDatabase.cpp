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
#include "CSharpBlueprintImportDatabase.h"

namespace UnrealSharp
{
    FCSharpBlueprintImportDatabase::FCSharpBlueprintImportDatabase()
    {
    }

    void FCSharpBlueprintImportDatabase::Reset()
    {
        Records.Empty();
    }

    bool FCSharpBlueprintImportDatabase::LoadFromFile(const TCHAR* InFilePath)
    {
        Reset();

        FString JsonString;
        if (!FFileHelper::LoadFileToString(JsonString, InFilePath))
        {
            return false;
        }

        TSharedPtr<FJsonObject> JsonObject;

        const TSharedPtr<TJsonReader<>> JsonReader = TJsonReaderFactory<>::Create(JsonString);

        if (!JsonReader ||
            !FJsonSerializer::Deserialize(JsonReader.ToSharedRef(), JsonObject) ||
            !JsonObject.IsValid())
        {
            return false;
        }

        const TArray< TSharedPtr<FJsonValue> >& Files = JsonObject->GetArrayField(TEXT("Files"));

        for (auto& JSONValuePtr : Files)
        {
            TSharedPtr<FJsonObject>* TypedJsonObject;
            if (JSONValuePtr->TryGetObject(TypedJsonObject) && TypedJsonObject)
            {
                FString File = TypedJsonObject->Get()->GetStringField(TEXT("File"));
                uint32 Value = static_cast<uint32>(TypedJsonObject->Get()->GetNumberField(TEXT("Crc")));
                Records.Add(TKeyValuePair(File, Value));
            }
        }

        return true;
    }

    bool FCSharpBlueprintImportDatabase::SaveToFile(const TCHAR* InFilePath)
    {
        const TSharedPtr<FJsonObject> Doc = MakeShared<FJsonObject>();

        TArray<TSharedPtr<FJsonValue>> Files;

        for (auto& Pair : Records)
        {
            TSharedPtr<FJsonObject> FileObject = MakeShared<FJsonObject>();
            
            FileObject->SetStringField("File", Pair.Key);
            FileObject->SetNumberField("Crc", Pair.Value);

            Files.Add(MakeShared<FJsonValueObject>(FileObject));
        }

        Doc->SetArrayField("Files", Files);

        FString JsonString;
        const TSharedPtr<TJsonWriter<>> JsonWriter = TJsonWriterFactory<>::Create(&JsonString);

        if (FJsonSerializer::Serialize(Doc.ToSharedRef(), JsonWriter.ToSharedRef()))
        {
            return FFileHelper::SaveStringToFile(JsonString, InFilePath);
        }

        return false;
    }

    bool FCSharpBlueprintImportDatabase::IsEqualTo(const FCSharpBlueprintImportDatabase& InDatabase) const
    {
        if (Records.Num() != InDatabase.Records.Num())
        {
            return false;
        }

        for (auto Pair : Records)
        {
            auto* Result = InDatabase.Records.FindByPredicate([&](const TKeyValuePair<FString, uint32>& Target) {
                return Target.Key == Pair.Key;
            });

            if (Result == nullptr || Result->Value != Pair.Value)
            {
                return false;
            }
        }

        return true;
    }

    bool FCSharpBlueprintImportDatabase::operator== (const FCSharpBlueprintImportDatabase& InOther) const
    {
        return IsEqualTo(InOther);
    }

    bool FCSharpBlueprintImportDatabase::operator!= (const FCSharpBlueprintImportDatabase& InOther) const
    {
        return !IsEqualTo(InOther);
    }

    bool FCSharpBlueprintImportDatabase::LoadFromDirectory(const TCHAR* InDirectoryPath)
    {
        check(InDirectoryPath != nullptr);
        
        Reset();

        TArray<FString> Result;
        IFileManager& FileManager = IFileManager::Get();

        FileManager.FindFiles(Result, InDirectoryPath, TEXT(".tdb"));

        for (auto& FileName : Result)
        {
            FString FullPath = FPaths::Combine(InDirectoryPath, FileName);

            TKeyValuePair Data(FileName, CalcFileCrc32(*FullPath));
            Records.Add(Data);
        }

        return true;
    }

    uint32 FCSharpBlueprintImportDatabase::CalcFileCrc32(const TCHAR* InFilePath)
    {
        TArray<uint8> FileData;
        if (FFileHelper::LoadFileToArray(FileData, InFilePath))
        {
            return FCrc::MemCrc32(FileData.GetData(), FileData.Num());
        }

        return 0;
    }
}

