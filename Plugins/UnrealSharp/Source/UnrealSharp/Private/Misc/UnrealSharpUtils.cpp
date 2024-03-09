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
#include "Misc/UnrealSharpUtils.h"
#include "Classes/CSharpStruct.h"
#include "Classes/CSharpClass.h"
#include "Classes/CSharpEnum.h"
#include "Classes/UnrealSharpSettings.h"
#include "ICSharpRuntime.h"

namespace UnrealSharp
{
    const FString FUnrealSharpUtils::UnrealSharpEngineProjectName = TEXT("UnrealSharp.UnrealEngine");
    const FString FUnrealSharpUtils::UnrealSharpEngineAssemblyName = TEXT("UnrealSharp.UnrealEngine.dll");
    const FString FUnrealSharpUtils::UnrealSharpEngineNamespace = TEXT("UnrealSharp.UnrealEngine");

    const FString FUnrealSharpUtils::UnrealSharpGameScriptsProjectName = TEXT("UnrealSharp.GameScripts");
    const FString FUnrealSharpUtils::UnrealSharpGameScriptsAssemblyName = TEXT("UnrealSharp.GameScripts.dll");
    const FString FUnrealSharpUtils::UnrealSharpGameScriptsNamespace = TEXT("UnrealSharp.GameScripts");

    const FString FUnrealSharpUtils::UnrealSharpGameContentProjectName = TEXT("UnrealSharp.GameContent");
    const FString FUnrealSharpUtils::UnrealSharpGameContentAssemblyName = TEXT("UnrealSharp.GameContent.dll");
    const FString FUnrealSharpUtils::UnrealSharpGameContentNamespace = TEXT("UnrealSharp.GameContent");

    bool FUnrealSharpUtils::IsNativeField(const UField* InField)
    {
        checkSlow(InField);

        const auto ClassOfField = InField->GetClass();

        return ClassOfField == UClass::StaticClass() ||
            ClassOfField == UScriptStruct::StaticClass() ||
            ClassOfField == UEnum::StaticClass();
    }

    static const FString WidgetBlueprintGeneratedClassName = TEXT("WidgetBlueprintGeneratedClass");

    bool FUnrealSharpUtils::IsBlueprintField(const UField* InField)
    {
        checkSlow(InField);

        const auto ClassOfField = InField->GetClass();

        return ClassOfField == UBlueprintGeneratedClass::StaticClass() ||
            ClassOfField == UUserDefinedStruct::StaticClass() ||
            ClassOfField == UUserDefinedEnum::StaticClass() ||
            ClassOfField->GetName() == WidgetBlueprintGeneratedClassName
            ;
    }

    bool FUnrealSharpUtils::IsCSharpField(const UField* InField)
    {        
        checkSlow(InField);

        const auto ClassOfField = InField->GetClass();

        return ClassOfField == UCSharpClass::StaticClass() ||
            ClassOfField == UCSharpStruct::StaticClass() ||
            ClassOfField == UCSharpEnum::StaticClass();
    }

    bool FUnrealSharpUtils::IsExportToGameScriptsField(const UField* InField)
    {
        if (!IsNativeField(InField))
        {
            return false;
        }

        return IsExportToGameScriptsNativeField(InField);
    }

    bool FUnrealSharpUtils::IsExportToGameScriptsNativeField(const UField* InNativeField)
    {
        checkSlow(IsNativeField(InNativeField));

        const UUnrealSharpSettings* Settings = GetDefault<UUnrealSharpSettings>();
        FName ModuleName = GetFieldModuleName(InNativeField);

        return Settings->IsExportToGameScripts(ModuleName);
    }

    bool FUnrealSharpUtils::IsNativeClass(const UClass* InClass)
    {
        return InClass->GetClass() == UClass::StaticClass();
    }

    bool FUnrealSharpUtils::IsBlueprintClass(const UClass* InClass)
    {
        return InClass->GetClass() == UBlueprintGeneratedClass::StaticClass() ||
            InClass->GetClass()->GetName() == WidgetBlueprintGeneratedClassName;
    }

    bool FUnrealSharpUtils::IsCSharpInheritBlueprintClass(const UClass* InClass)
    {
        const UClass* TestClass = InClass;
        while (TestClass != UObject::StaticClass() &&
            TestClass != UInterface::StaticClass()
            )
        {
            if (IsCSharpClass(TestClass))
            {
                return true;
            }
            else if (IsNativeClass(TestClass))
            {
                return false;
            }

            TestClass = TestClass->GetSuperClass();
        }

        return false;
    }

    bool FUnrealSharpUtils::IsCSharpClass(const UClass* InClass)
    {
        return InClass->GetClass() == UCSharpClass::StaticClass();
    }

    bool FUnrealSharpUtils::IsNativeStruct(const UScriptStruct* InStruct)
    {
        return InStruct->GetClass() == UScriptStruct::StaticClass();
    }

    bool FUnrealSharpUtils::IsBlueprintStruct(const UScriptStruct* InStruct)
    {
        return InStruct->GetClass() == UUserDefinedStruct::StaticClass();
    }

    bool FUnrealSharpUtils::IsCSharpStruct(const UScriptStruct* InStruct)
    {
        return InStruct->GetClass() == UCSharpStruct::StaticClass();
    }

    bool FUnrealSharpUtils::IsNativeEnum(const UEnum* InEnum)
    {
        return InEnum->GetClass() == UEnum::StaticClass();
    }

    bool FUnrealSharpUtils::IsBlueprintEnum(const UEnum* InEnum)
    {
        return InEnum->GetClass() == UUserDefinedEnum::StaticClass();
    }

    bool FUnrealSharpUtils::IsCSharpEnum(const UEnum* InEnum)
    {
        return InEnum->GetClass() == UCSharpEnum::StaticClass();
    }

    bool FUnrealSharpUtils::IsSpecialClass(const UClass* InClass)
    {
        static const FString SpecialHeaders[] = {
            TEXT("HOTRELOADED_"),
            TEXT("PLACEHOLDER-"),
            TEXT("REINST_"),
            TEXT("SKEL_"),
            TEXT("TRASHCLASS_")
        };

        const FString& ClassName = InClass->GetName();

        for (auto& header : SpecialHeaders)
        {
            if (ClassName.StartsWith(header))
            {
                return true;
            }
        }

        return false;
    }

    FString FUnrealSharpUtils::GetCppTypeName(const UField* InField)
    {
        if (auto Class = Cast<UClass>(InField))
        {
            return Class->GetPrefixCPP() + Class->GetName();
        }
        else if (auto Struct = Cast<UScriptStruct>(InField))
        {
            return Struct->GetStructCPPName();
        }

        return InField->GetName();
    }

    FName FUnrealSharpUtils::GetFieldModuleName(const UField* InField)
    {
        checkSlow(InField);

        return *FPackageName::GetShortName(InField->GetOutermost()->GetFName());
    }

    const FString& FUnrealSharpUtils::GetDefaultExportProjectName(const UField* InField)
    {
        if (IsNativeField(InField))
        {            
            return IsExportToGameScriptsNativeField(InField) ? UnrealSharpGameScriptsProjectName : UnrealSharpEngineProjectName;
        }

        return UnrealSharpGameContentProjectName;
    }

    const FString& FUnrealSharpUtils::GetDefaultExportNamespace(const UField* InField)
    {
        if (IsNativeField(InField))
        {
            return IsExportToGameScriptsNativeField(InField) ? UnrealSharpGameScriptsNamespace : UnrealSharpEngineNamespace;
        }

        return UnrealSharpGameContentNamespace;
    }
   
    const FString& FUnrealSharpUtils::GetAssemblyName(const UField* InField)
    {
        if (IsCSharpField(InField))
        {
            const ICSharpGeneratedType* CSharpType = Cast<ICSharpGeneratedType>(InField);

            check(CSharpType != nullptr);

            return CSharpType->GetAssemblyName();
        }

        if (IsNativeField(InField))
        {
            return IsExportToGameScriptsNativeField(InField) ? UnrealSharpGameScriptsAssemblyName : UnrealSharpEngineAssemblyName;
        }

        return UnrealSharpGameContentAssemblyName;
    }

    FString FUnrealSharpUtils::GetCSharpFullPath(const UField* InField)
    {        
        if (IsCSharpField(InField))
        {
            const ICSharpGeneratedType* CSharpType = Cast<ICSharpGeneratedType>(InField);

            check(CSharpType != nullptr);

            return CSharpType->GetCSharpFullName();
        }

        FString CppName = GetCppTypeName(InField);
        FString DefaultExportNamespace = GetDefaultExportNamespace(InField);

        return DefaultExportNamespace + TEXT(".") + CppName;
    }

    const FString& FUnrealSharpUtils::GetAssemblyName(const FProperty* InProperty)
    {
        UField* InnerField = GetPropertyInnerField(InProperty);
        static const FString Z_Temp;

        return InnerField != nullptr ? GetAssemblyName(InnerField) : Z_Temp;
    }

    FString FUnrealSharpUtils::GetCSharpFullPath(const FProperty* InProperty)
    {
        UField* InnerField = GetPropertyInnerField(InProperty);

        return InnerField != nullptr ? GetCSharpFullPath(InnerField) : FString();
    }

    UField* FUnrealSharpUtils::GetPropertyInnerField(const FProperty* InProperty)
    {
        if (const FObjectProperty* ObjectProperty = CastField<FObjectProperty>(InProperty))
        {
            return ObjectProperty->PropertyClass;
        }
        else if (const FStructProperty* StructProperty = CastField<FStructProperty>(InProperty))
        {
            return StructProperty->Struct;
        }
        else if (const FEnumProperty* EnumProperty = CastField<FEnumProperty>(InProperty))
        {
            return EnumProperty->GetEnum();
        }
        else if (const FClassProperty* ClassProperty = CastField<FClassProperty>(InProperty))
        {
            return ClassProperty->MetaClass;
        }
        else if (const FByteProperty* ByteProperty = CastField<FByteProperty>(InProperty)) // for TEnumAsByte
        {
            return ByteProperty->Enum;
        }
        else if (const FSoftObjectProperty* SoftObjectProperty = CastField<FSoftObjectProperty>(InProperty))
        {
            return SoftObjectProperty->PropertyClass;
        }
        else if (const FSoftClassProperty* SoftClassProperty = CastField<FSoftClassProperty>(InProperty))
        {
            return SoftClassProperty->PropertyClass;
        }

        return nullptr;
    }

    TSharedPtr<ICSharpMethodInvocation> FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(ICSharpRuntime* InRuntime, const FString& InClassName, const FString& InBaseSignature)
    {
        return BindCSharpMethodChecked(InRuntime, UnrealSharpEngineAssemblyName, UnrealSharpEngineNamespace, InClassName, InBaseSignature);
    }

    TSharedPtr<ICSharpMethodInvocation> FUnrealSharpUtils::BindCSharpMethodChecked(ICSharpRuntime* InRuntime, const FString& InAssemblyName, const FString& InNamespace, const FString& InClassName, const FString& InBaseSignature)
    {
        FString FullSignature = FString::Printf(TEXT("%s.%s:%s"), *InNamespace, *InClassName, *InBaseSignature);

        TSharedPtr<ICSharpMethodInvocation> Invocation = InRuntime->CreateCSharpMethodInvocation(
            InAssemblyName,
            FullSignature
        );

        checkf(Invocation, TEXT("Failed bind C# method by signature:%s"), *FullSignature);

        return Invocation;
    }

    int FUnrealSharpUtils::CalcEnumUnderlyingTypeSize(const UEnum* InEnum)
    {
        int64 MaxValue = (std::numeric_limits<int64>::min)();

        const int NumEnums = InEnum->NumEnums();

        for (int i = 0; i < NumEnums - 1; ++i)
        {
            const int64 Value = InEnum->GetValueByIndex(i);

            if (Value > MaxValue)
            {
                MaxValue = Value;
            }
        }

        if (MaxValue > (std::numeric_limits<uint8>::min)() && MaxValue <= (std::numeric_limits<uint8>::max)())
        {
            return sizeof(uint8);
        }
        else if (MaxValue > (std::numeric_limits<int32>::min)() && MaxValue <= (std::numeric_limits<int32>::max)())
        {
            return sizeof(int32);
        }
        else if ((uint64)MaxValue > (std::numeric_limits<uint64>::min)() && (uint64)MaxValue <= (std::numeric_limits<uint64>::max)())
        {
            return sizeof(int64);
        }

        return sizeof(int32);
    }

    int FUnrealSharpUtils::GetPropertyCount(const UStruct* InStruct)
    {
        int PropertyCount = 0;

        for (FProperty* Property = InStruct->PropertyLink; Property != nullptr; Property = Property->PropertyLinkNext)
        {
            ++PropertyCount;
        }

        return PropertyCount;
    }

    int FUnrealSharpUtils::GetPropertyCount(const UStruct* InStruct, TFunction<bool(const FProperty*)> InFilter)
    {
        int PropertyCount = 0;

        for (FProperty* Property = InStruct->PropertyLink; Property != nullptr; Property = Property->PropertyLinkNext)
        {
            if (InFilter(Property))
            {
                ++PropertyCount;
            }            
        }

        return PropertyCount;
    }

    FName FUnrealSharpUtils::ExtraUserDefinedStructPropertyName(const FProperty* InProperty)
    {
        checkSlow(InProperty != nullptr);
                
        FString PropertyName = InProperty->GetName();
        constexpr int32 GuidStrLen = 32;

        // Find the position of the last two underscores
        int32 LastUnderscoreIndex = PropertyName.Find(TEXT("_"), ESearchCase::CaseSensitive, ESearchDir::FromEnd);
        if (LastUnderscoreIndex != INDEX_NONE)
        {
            int32 SecondLastUnderscoreIndex = PropertyName.Find(TEXT("_"), ESearchCase::CaseSensitive, ESearchDir::FromEnd, LastUnderscoreIndex - 1);

            // Check if the underscores were found and if the length of GUID is correct
            if (SecondLastUnderscoreIndex != INDEX_NONE && PropertyName.Len() - LastUnderscoreIndex - 1 == GuidStrLen)
            {
                // Extract the real property name
                PropertyName = PropertyName.Left(SecondLastUnderscoreIndex);
            }
        }

        return *PropertyName;
    }
}

