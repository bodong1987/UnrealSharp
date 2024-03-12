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
#include "ClassTypeDefinition.h"
#include "Misc/UnrealSharpLog.h"

namespace UnrealSharp
{
    FClassTypeDefinition::FClassTypeDefinition()
    {
        SuperName = TEXT("UObject");
        Type = (int)EDefinitionType::Class;
    }

    FClassTypeDefinition::FClassTypeDefinition(UClass* InClass, FTypeValidation* InTypeValidation) :
        Super(InClass, InTypeValidation)
    {
        Type = InClass->IsChildOf<UInterface>() ? (int)EDefinitionType::Interface : (int)EDefinitionType::Class;

        if (InClass->HasAnyClassFlags(CLASS_Config))
        {
            ConfigName = InClass->GetConfigName();
        }        

        SuperName = InClass->GetSuperClass() ? GetCppTypeName(InClass->GetSuperClass()) : TEXT("UObject");
        Flags = InClass->ClassFlags;

        LoadProperties(InClass, InClass->GetDefaultObject(), (EFieldIterationFlags)EFieldIteratorFlags::ExcludeSuper, InTypeValidation, [US_LAMBDA_CAPTURE_THIS](FProperty* InProperty) {
            return IsSupportedProperty(InProperty, InTypeValidation);
            });

        LoadFunctions(InClass, InTypeValidation);

        LoadInterfaces(InClass);
    }

    void FClassTypeDefinition::LoadFunctions(UClass* InClass, FTypeValidation* InTypeValidation)
    {
        for (TFieldIterator<UFunction> FunctionIterator(InClass, EFieldIteratorFlags::ExcludeSuper); FunctionIterator; ++FunctionIterator)
        {
            if (IsSupportedFunction(*FunctionIterator, InTypeValidation))
            {
                FFunctionTypeDefinition FunctionTypeDefinition(*FunctionIterator, InTypeValidation);

                Functions.Add(FunctionTypeDefinition);

                AddDependNamespace(*FunctionIterator);
            }
        }

        // get methods from interfaces
        if (!InClass->HasAnyClassFlags(CLASS_Interface))
        {
            for (auto& implementation : InClass->Interfaces)
            {
                for (TFieldIterator<UFunction> FunctionIterator(implementation.Class, EFieldIterationFlags::IncludeSuper); FunctionIterator; ++FunctionIterator)
                {
                    UClass* DeclareClass = (*FunctionIterator)->GetOwnerClass();

                    if (DeclareClass == UInterface::StaticClass() || DeclareClass == UObject::StaticClass())
                    {
                        continue;
                    }

                    if (IsSupportedFunction(*FunctionIterator, InTypeValidation))
                    {
                        if (Functions.FindByPredicate([=](const FFunctionTypeDefinition& InDefinition) {
                            return InDefinition.Name == (*FunctionIterator)->GetName();
                            }) == nullptr)
                        {
                            FFunctionTypeDefinition FunctionTypeDefinition(*FunctionIterator, InTypeValidation);

                            Functions.Add(FunctionTypeDefinition);

                            AddDependNamespace(*FunctionIterator);
                        }
                    }
                }
            }
        }
    }

    void FClassTypeDefinition::LoadInterfaces(UClass* InClass)
    {
        for (auto& Interface : InClass->Interfaces)
        {
            Interfaces.Add(GetCppTypeName(Interface.Class));
        }
    }

    void FClassTypeDefinition::Write(FJsonObject& InObject)
    {
        // write type for C#
        if (Type == (int)EDefinitionType::Interface)
        {
            InObject.SetStringField("$type", "UnrealSharpTool.Core.TypeInfo.InterfaceClassTypeDefinition, UnrealSharpTool.Core");
        }
        else
        {
            InObject.SetStringField("$type", "UnrealSharpTool.Core.TypeInfo.ClassTypeDefinition, UnrealSharpTool.Core");
        }        

        Super::Write(InObject);

        InObject.SetStringField("SuperName", SuperName);
        InObject.SetStringField("ConfigName", ConfigName);

        if (!Functions.IsEmpty())
        {
            TArray<TSharedPtr<FJsonValue>> TempFunctions;
            for (auto& Func : Functions)
            {
                TSharedPtr<FJsonObject> FunctionObject = MakeShared<FJsonObject>();
                Func.Write(*FunctionObject);

                TempFunctions.Add(MakeShared<FJsonValueObject>(FunctionObject));
            }

            InObject.SetArrayField("Functions", TempFunctions);
        }

        if (!Interfaces.IsEmpty())
        {
            TArray<TSharedPtr<FJsonValue>> TempInterfaces;
            for (auto& InterfaceName : Interfaces)
            {
                TSharedPtr<FJsonValueString> StringValue = MakeShared<FJsonValueString>(InterfaceName);
                
                TempInterfaces.Add(StringValue);
            }

            InObject.SetArrayField("Interfaces", TempInterfaces);
        }
    }

    void FClassTypeDefinition::Read(FJsonObject& InObject)
    {
        Super::Read(InObject);

        SuperName = InObject.GetStringField("SuperName");
        ConfigName = InObject.GetStringField("ConfigName");

        const TArray< TSharedPtr<FJsonValue> >* FunctionsRefPtr = nullptr;

        if (InObject.TryGetArrayField("Functions", FunctionsRefPtr) && FunctionsRefPtr)
        {
            for (auto& FunctionObject : *FunctionsRefPtr)
            {
                TSharedPtr<FJsonObject>* ObjectPtr = nullptr;
                if (FunctionObject->TryGetObject(ObjectPtr) && ObjectPtr != nullptr)
                {
                    FFunctionTypeDefinition Def;
                    Def.Read(**ObjectPtr);

                    Functions.Add(Def);
                }
            }
        }
        
        const TArray<TSharedPtr<FJsonValue>>* InterfacesRefPtr = nullptr;
        if (InObject.TryGetArrayField("Interfaces", InterfacesRefPtr) && InterfacesRefPtr)
        {
            for (auto& InterfaceObject : *InterfacesRefPtr)
            {
                FString InterfaceName;
                InterfaceObject->TryGetString(InterfaceName);

                Interfaces.Add(InterfaceName);
            }
        }
    }

    void FClassTypeDefinition::AddDependNamespace(const UFunction* InFunction)
    {
        for (TFieldIterator<FProperty> PropertyIter(InFunction); PropertyIter; ++PropertyIter)
        {
            Super::AddDependNamespace(*PropertyIter);
        }
    }
}
