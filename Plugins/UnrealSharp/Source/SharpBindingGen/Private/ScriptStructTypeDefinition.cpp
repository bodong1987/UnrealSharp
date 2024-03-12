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
#include "ScriptStructTypeDefinition.h"
#include "Misc/ScopedExit.h"
#include "Misc/UnrealSharpLog.h"

namespace UnrealSharp
{
    FScriptStructTypeDefinition::FScriptStructTypeDefinition()
    {
        Type = (int)EDefinitionType::Struct;
    }

    FScriptStructTypeDefinition::FScriptStructTypeDefinition(UScriptStruct* InStruct, FTypeValidation* InTypeValidation) :
        Super(InStruct, InTypeValidation)
    {
        Type = (int)EDefinitionType::Struct;
        Flags = InStruct->StructFlags;

        void* StructInstance = FMemory::Malloc(InStruct->GetStructureSize());
        InStruct->InitializeDefaultValue((uint8*)StructInstance);

        UNREALSHARP_SCOPED_EXIT(
            InStruct->DestroyStruct(StructInstance);
            FMemory::Free(StructInstance);
        );

        LoadProperties(InStruct, StructInstance, EFieldIterationFlags::IncludeSuper, InTypeValidation, [US_LAMBDA_CAPTURE_THIS](FProperty* InProperty) {
            return IsSupportedProperty(InProperty, InTypeValidation);
            });
    }

    void FScriptStructTypeDefinition::Write(FJsonObject& InObject)
    {
        InObject.SetStringField("$type", "UnrealSharpTool.Core.TypeInfo.ScriptStructTypeDefinition, UnrealSharpTool.Core");

        Super::Write(InObject);
    }
}
