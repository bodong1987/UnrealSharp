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
#include "BaseTypeDefinition.h"
#include "Misc/UnrealSharpUtils.h"
#include <inttypes.h>

namespace UnrealSharp
{
    FBaseTypeDefinition::FBaseTypeDefinition() :
        ProjectName(FUnrealSharpUtils::UnrealSharpEngineProjectName),        
        Namespace(FUnrealSharpUtils::UnrealSharpEngineNamespace),
        AssemblyName(FUnrealSharpUtils::UnrealSharpEngineAssemblyName)
    {
    }

    FBaseTypeDefinition::FBaseTypeDefinition(UField* InField, FTypeValidation* /*InTypeValidation*/) :
        ProjectName(FUnrealSharpUtils::GetDefaultExportProjectName(InField)),        
        Namespace(FUnrealSharpUtils::GetDefaultExportNamespace(InField)),
        AssemblyName(FUnrealSharpUtils::GetAssemblyName(InField))
    {
        Name = InField->GetName();
        CppName = CppName = FUnrealSharpUtils::GetCppTypeName(InField);
        PathName = InField->GetPathName();
        
        if (const auto Struct = Cast<UStruct>(InField))
        {
            Size = Struct->GetStructureSize();
        }
        else if (const auto Enum = Cast<UEnum>(InField))
        {
            Size = FUnrealSharpUtils::CalcEnumUnderlyingTypeSize(Enum);
        }

        if (FUnrealSharpUtils::IsNativeField(InField))
        {
            // native type use real package
            PackageName = FPackageName::GetShortName(InField->GetOutermost()->GetFName());
        }
        else
        {
            // blueprint use 
            PackageName = GetBlueprintFieldPackageName(PathName);
        }

        CSharpFullName = FString::Printf(TEXT("%s.%s"), *Namespace, *CppName);
        
        Meta.Load(InField);
    }

    FBaseTypeDefinition::~FBaseTypeDefinition()
    {
    }

    FString FBaseTypeDefinition::GetBlueprintFieldPackageName(const FString& InPath)
    {
        TArray<FString> Parts;
        InPath.ParseIntoArray(Parts, TEXT("/"), true);

        FString Result;

        // Only add the first two parts if they exist
        for (int32 i = 0; i < Parts.Num() && i < 2; i++)
        {
            if (!Result.IsEmpty())
            {
                Result += TEXT("/");
            }
            Result += Parts[i];
        }

        return Result;
    }

    void FBaseTypeDefinition::Write(FJsonObject& InObject)
    {        
        InObject.SetNumberField(TEXT("Type"), Type);
        InObject.SetStringField(TEXT("Name"), Name);
        InObject.SetStringField(TEXT("CppName"), CppName);
        InObject.SetStringField(TEXT("PathName"), PathName);
        InObject.SetStringField(TEXT("PackageName"), PackageName);
        InObject.SetStringField(TEXT("ProjectName"), ProjectName);
        InObject.SetStringField(TEXT("Namespace"), Namespace);
        InObject.SetStringField(TEXT("AssemblyName"), AssemblyName);
        InObject.SetStringField(TEXT("CSharpFullName"), CSharpFullName);
        InObject.SetStringField(TEXT("FlagsT"), FString::Printf(TEXT("%") TEXT(PRIu64), Flags));
        InObject.SetStringField(TEXT("CrcCodeT"), FString::Printf(TEXT("%") TEXT(PRIi64), CrcCode));
        InObject.SetNumberField(TEXT("Size"), Size);

        if (Guid.IsValid())
        {
            InObject.SetStringField(TEXT("Guid"), Guid.ToString(EGuidFormats::DigitsWithHyphensLower));
        }        
     
        Meta.Write(InObject);
    }

    void FBaseTypeDefinition::Read(FJsonObject& InObject)
    {
        Type = static_cast<int>(InObject.GetNumberField(TEXT("Type")));
        Name = InObject.GetStringField(TEXT("Name"));
        CppName = InObject.GetStringField(TEXT("CppName"));
        PathName = InObject.GetStringField(TEXT("PathName"));
        PackageName = InObject.GetStringField(TEXT("PackageName"));
        ProjectName = InObject.GetStringField(TEXT("ProjectName"));
        Namespace = InObject.GetStringField(TEXT("Namespace"));
        InObject.TryGetStringField(TEXT("AssemblyName"), AssemblyName);
        CSharpFullName = InObject.GetStringField(TEXT("CSharpFullName"));
        LexFromString(Flags, *InObject.GetStringField(TEXT("FlagsT")));
        LexFromString(CrcCode, *InObject.GetStringField(TEXT("CrcCodeT")));
        Size = static_cast<uint8>(InObject.GetNumberField(TEXT("Size")));
        
        if (InObject.HasField(TEXT("Guid")))
        {
            FGuid::Parse(InObject.GetStringField(TEXT("Guid")), Guid);
        }        

        Meta.Read(InObject);
    }

    FString FBaseTypeDefinition::GetCppTypeName(const UField* InField)
    {
        return FUnrealSharpUtils::GetCppTypeName(InField);
    }
}
