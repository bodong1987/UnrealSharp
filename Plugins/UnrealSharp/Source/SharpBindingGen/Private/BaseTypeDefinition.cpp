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

    FBaseTypeDefinition::FBaseTypeDefinition(UField* InField, FTypeValidation* InTypeValidation) :
        ProjectName(FUnrealSharpUtils::GetDefaultExportProjectName(InField)),        
        Namespace(FUnrealSharpUtils::GetDefaultExportNamespace(InField)),
        AssemblyName(FUnrealSharpUtils::GetAssemblyName(InField))
    {
        Name = InField->GetName();
        CppName = CppName = FUnrealSharpUtils::GetCppTypeName(InField);;
        PathName = InField->GetPathName();
        
        if (auto Struct = Cast<UStruct>(InField))
        {
            Size = Struct->GetStructureSize();
        }
        else if (auto Enum = Cast<UEnum>(InField))
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
        InObject.SetNumberField("Type", Type);
        InObject.SetStringField("Name", Name);
        InObject.SetStringField("CppName", CppName);
        InObject.SetStringField("PathName", PathName);
        InObject.SetStringField("PackageName", PackageName);
        InObject.SetStringField("ProjectName", ProjectName);
        InObject.SetStringField("Namespace", Namespace);
        InObject.SetStringField("AssemblyName", AssemblyName);
        InObject.SetStringField("CSharpFullName", CSharpFullName);
        InObject.SetStringField("FlagsT", FString::Printf(TEXT("%") TEXT(PRIu64), Flags));
        InObject.SetStringField("CrcCodeT", FString::Printf(TEXT("%") TEXT(PRIi64), CrcCode));
        InObject.SetNumberField("Size", Size);

        if (Guid.IsValid())
        {
            InObject.SetStringField("Guid", Guid.ToString(EGuidFormats::DigitsWithHyphensLower));
        }        
     
        Meta.Write(InObject);
    }

    void FBaseTypeDefinition::Read(FJsonObject& InObject)
    {
        Type = (int)InObject.GetNumberField("Type");
        Name = InObject.GetStringField("Name");
        CppName = InObject.GetStringField("CppName");
        PathName = InObject.GetStringField("PathName");
        PackageName = InObject.GetStringField("PackageName");
        ProjectName = InObject.GetStringField("ProjectName");
        Namespace = InObject.GetStringField("Namespace");
        InObject.TryGetStringField("AssemblyName", AssemblyName);
        CSharpFullName = InObject.GetStringField("CSharpFullName");
        LexFromString(Flags, *InObject.GetStringField("FlagsT"));
        LexFromString(CrcCode, *InObject.GetStringField("CrcCodeT"));
        Size = (uint8)InObject.GetNumberField("Size");
        
        if (InObject.HasField("Guid"))
        {
            FGuid::Parse(InObject.GetStringField("Guid"), Guid);
        }        

        Meta.Read(InObject);
    }

    FString FBaseTypeDefinition::GetCppTypeName(UField* InField)
    {
        return FUnrealSharpUtils::GetCppTypeName(InField);
    }
}