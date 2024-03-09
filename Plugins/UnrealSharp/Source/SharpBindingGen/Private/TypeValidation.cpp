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
#include "TypeValidation.h"
#include "SharpBindingGenSettings.h"
#include "Engine/UserDefinedEnum.h"
#include "Engine/UserDefinedStruct.h"
#include "Misc/UnrealSharpUtils.h"
#include "Misc/UnrealSharpLog.h"

namespace UnrealSharp
{
    FTypeValidation::FTypeValidation(bool bAutoCheck) :
        GenSettings((const USharpBindingGenSettings*)USharpBindingGenSettings::StaticClass()->GetDefaultObject())
    {
        Reset(bAutoCheck);
    }

    bool FTypeValidation::IsSupported(UField* InField) const
    {
        return SupportedFields.Contains(InField);
    }

    bool FTypeValidation::IsNeedExport(UField* InField) const
    {
        return IsSupported(InField) && GenSettings->IsNeedExportType(GetFieldCheckedName(InField));
    }

    FTypeValidation::ECheckResult FTypeValidation::GetCheckResult(UField* InField) const
    {
        if (SupportedFields.Contains(InField))
        {
            return ECheckResult::Success;
        }

        if (UnSupportedFields.Contains(InField) || DeprecatedFields.Contains(InField) || CSharpFields.Contains(InField))
        {
            return ECheckResult::Failure;
        }

        return ECheckResult::Undefined;
    }

    void FTypeValidation::Reset(bool bAutoCheck /* = false */)
    {
        UnSupportedFields.Empty();
        SupportedFields.Empty();
        DeprecatedFields.Empty();
        CSharpFields.Empty();

        if (!bAutoCheck)
        {
            return;
        }

        for (TObjectIterator<UField> Iter; Iter; ++Iter)
        {
            UField* Field = *Iter;

            if (Field->IsA<UClass>() || Field->IsA<UScriptStruct>() || Field->IsA<UEnum>())
            {
                ValidateField(*Iter);
            }            
        }
    }

    bool FTypeValidation::ValidateField(UField* InField)
    {
        const ECheckResult CachedResult = GetCheckResult(InField);

        if (CachedResult == ECheckResult::Failure)
        {
            return false;
        }
        else if (CachedResult == ECheckResult::Success)
        {
            return true;
        }

        UPackage* Package = InField->GetOutermost();

        check(Package != nullptr);

        if (!AllowPackage(Package))
        {
            UnSupportedFields.Add(InField);
            return false;
        }

        const FString& DeprecatedFlag = InField->GetMetaData("Deprecated");

        // if this type is deprecated in unreal engine 4
        // skip this type
        if (!DeprecatedFlag.IsEmpty() && DeprecatedFlag.StartsWith("4"))
        {
            DeprecatedFields.Add(InField);
            return false;
        }

        if (FUnrealSharpUtils::IsCSharpField(InField))
        {
            CSharpFields.Add(InField);
            return false;
        }

        if (UClass* Class = Cast<UClass>(InField))
        {   
            if (FUnrealSharpUtils::IsSpecialClass(Class))
            {
                UnSupportedFields.Add(InField);
                return false;
            }

            FString ClassName = FUnrealSharpUtils::GetCppTypeName(Class);
            if (!GenSettings->IsSupportedType(ClassName))
            {
                UnSupportedFields.Add(InField);

                return false;
            }

            if (UClass* SuperClass = Class->GetSuperClass())
            {
                if (SuperClass != UObject::StaticClass())
                {
                    if (SuperClass == UInterface::StaticClass())
                    {
                        // export all interface by default
                        // UnSupportedFields.Add(Class);
                        // return false;
                    }
                    else if (!ValidateField(SuperClass))
                    {
                        UnSupportedFields.Add(Class);
                        return false;
                    }                    
                }
            }
        }
        else if (UScriptStruct* Struct = Cast<UScriptStruct>(InField))
        {
            // ignore this struct ???
            if (!GenSettings->IsSupportedType(Struct->GetStructCPPName()))
            {
                UnSupportedFields.Add(Struct);
                return false;
            }

            if (!GenSettings->ForceExportEmptyStructNames.Contains(Struct->GetStructCPPName()))
            {
                // ignore if this struct has no valid properties
                const int PropertyCount = FUnrealSharpUtils::GetPropertyCount(Struct, [&](const FProperty* InProperty)
                    {
                        auto InnerField = FUnrealSharpUtils::GetPropertyInnerField(InProperty);
                        if (InnerField == nullptr)
                        {
                            return true;
                        }

                        return ValidateField(InnerField);
                    });

                if (PropertyCount <= 0)
                {
                    if (GenSettings->bShowIgnoreEmptyStructWarning)
                    {
                        US_LOG(TEXT("Ignore struct %s[%s], It has no public fields, and it is meaningless to export such a structure. If you really need it, you can consider implementing such a structure as builtin structure manually or force export it by add its C++ name to BindingGen Settings."), *Struct->GetStructCPPName(), *Struct->GetPathName());
                    }
                    
                    UnSupportedFields.Add(Struct);
                    return false;
                }
            }
        }
        else if (UEnum* Enum = Cast<UEnum>(InField))
        {            
            if (!GenSettings->IsSupportedType(Enum->GetName()))
            {
                UnSupportedFields.Add(Enum);
                return false;
            }
        }

        SupportedFields.Add(InField);
        return true;
    }

    bool FTypeValidation::AllowPackage(UPackage* InPackage) const
    {
        const FString ModuleName = FPackageName::GetShortName(InPackage->GetFName());

        if (GenSettings->IsIgnoreModuleName(ModuleName))
        {
            return false;
        }

        // ignore editor modules
        return !ModuleName.Contains("Editor");
    }

    FString FTypeValidation::GetFieldCheckedName(UField* InField)
    {
        return FUnrealSharpUtils::GetCppTypeName(InField);
    }
    
}