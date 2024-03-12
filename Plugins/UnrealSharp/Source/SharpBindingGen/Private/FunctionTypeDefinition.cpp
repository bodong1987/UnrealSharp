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
#include "FunctionTypeDefinition.h"
#include "Misc/UnrealSharpUtils.h"
#include "Misc/UnrealSharpLog.h"

namespace UnrealSharp
{
    FFunctionTypeDefinition::FFunctionTypeDefinition()
    {
        Type = (int)EDefinitionType::Function;
    }

    FFunctionTypeDefinition::FFunctionTypeDefinition(UFunction* InFunction, FTypeValidation* InTypeValidation) :
        Super(InFunction, InTypeValidation)
    {
        Type = (int)EDefinitionType::Function;
        Flags = InFunction->FunctionFlags;

        auto* OwnerClass = InFunction->GetOuterUClass();
        Namespace = FUnrealSharpUtils::GetDefaultExportNamespace(OwnerClass) + TEXT(".") + FUnrealSharpUtils::GetCppTypeName(OwnerClass);
        ProjectName = FUnrealSharpUtils::GetDefaultExportProjectName(OwnerClass);
        AssemblyName = FUnrealSharpUtils::GetAssemblyName(OwnerClass);

        CSharpFullName = FString::Printf(TEXT("%s.%s"), *Namespace, *CppName);

        LoadProperties(InFunction, nullptr, EFieldIterationFlags::IncludeAll, InTypeValidation,
            [US_LAMBDA_CAPTURE_THIS](FProperty* InProperty){
            return IsSupportedProperty(InProperty, InTypeValidation);
        });
    }

    void FFunctionTypeDefinition::Write(FJsonObject& InObject)
    {
        Super::Write(InObject);
    }

    void FFunctionTypeDefinition::Read(FJsonObject& InObject)
    {
        Super::Read(InObject);
                
        // only valid for C# method...
        InObject.TryGetBoolField("IsOverrideFunction", bIsOverrideFunction);
        InObject.TryGetStringField("Signature", Signature);
    }

    bool FFunctionTypeDefinition::IsExportAsEvent() const
    {
        if ((Flags & FUNC_Event) != 0 ||
            (Flags & FUNC_Net)
            )
        {
            return true;
        }

        return false;
    }

    const FPropertyDefinition* FFunctionTypeDefinition::GetReturnPropertyDefinition() const
    {
        for (int i = Properties.Num() - 1; i >= 0; --i)
        {
            auto& Propertyref = Properties[i];

            if (Propertyref.IsReturnProperty())
            {
                return &Propertyref;
            }
        }

        return nullptr;
    }

    bool FFunctionTypeDefinition::HasReturnType() const
    {
        return GetReturnPropertyDefinition() != nullptr;
    }

    bool FFunctionTypeDefinition::HasAnyOutParameter() const
    {
        for (int i = Properties.Num() - 1; i >= 0; --i)
        {
            auto& PropertyRef = Properties[i];

            if (PropertyRef.IsReturnProperty() || PropertyRef.IsOut())
            {
                return true;
            }
        }

        return false;
    }
}
