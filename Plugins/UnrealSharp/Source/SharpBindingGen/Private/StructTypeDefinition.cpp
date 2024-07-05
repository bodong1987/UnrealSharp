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
#include "StructTypeDefinition.h"
#include "TypeValidation.h"
#include "Misc/UnrealSharpUtils.h"

namespace UnrealSharp
{
    FStructTypeDefinition::FStructTypeDefinition()
    {        
    }

    FStructTypeDefinition::FStructTypeDefinition(UStruct* InStruct, FTypeValidation* InTypeValidation) :
        Super(InStruct, InTypeValidation)
    {
    }

    void FStructTypeDefinition::LoadProperties(UStruct* InStruct, const void* InDefaultObjectPtr, const EFieldIterationFlags InFlags, FTypeValidation* InTypeValidation, const TFunction<bool(FProperty*)>& InAccessFunc)
    {
        for (TFieldIterator<FProperty> PropertyIter(InStruct, InFlags); PropertyIter; ++PropertyIter)
        {
            if (InAccessFunc(*PropertyIter))
            {
                Properties.Add(FPropertyDefinition(InStruct, InDefaultObjectPtr, *PropertyIter, InTypeValidation));

                AddDependNamespace(*PropertyIter);
            }
        }
    }

    void FStructTypeDefinition::Write(FJsonObject& InObject)
    {
        Super::Write(InObject);

        if (!Properties.IsEmpty())
        {
            TArray<TSharedPtr<FJsonValue>> TempProperties;
            for (auto& Property : Properties)
            {
                TSharedPtr<FJsonObject> PropertyObject = MakeShared<FJsonObject>();
                Property.Write(*PropertyObject);
                TempProperties.Add(MakeShared<FJsonValueObject>(PropertyObject));
            }

            InObject.SetArrayField("Properties", TempProperties);
        }

        if (!DependNamespaces.IsEmpty())
        {
            TArray<TSharedPtr<FJsonValue>> TempNamespaces;
            for (auto& Property : DependNamespaces)
            {
                TSharedRef<FJsonValue> JsonValue = MakeShareable(new FJsonValueString(Property.Key));
                TempNamespaces.Add(JsonValue);
            }

            InObject.SetArrayField(TEXT("DependNamespaces"), TempNamespaces);
        }
    }

    void FStructTypeDefinition::Read(FJsonObject& InObject)
    {
        Super::Read(InObject);

        if (const TArray< TSharedPtr<FJsonValue> >* PropertiesRefPtr = nullptr; InObject.TryGetArrayField(TEXT("Properties"), PropertiesRefPtr) && PropertiesRefPtr)
        {
            for (auto& PropertyObject : *PropertiesRefPtr)
            {
                if (TSharedPtr<FJsonObject>* ObjectPtr = nullptr; PropertyObject->TryGetObject(ObjectPtr) && ObjectPtr != nullptr)
                {
                    FPropertyDefinition Def;
                    Def.Read(**ObjectPtr);
                    Properties.Add(Def);
                }
            }
        }        
    }

    bool FStructTypeDefinition::IsSupportedFunction(UFunction* InFunction, FTypeValidation* InTypeValidation)
    {
        // skip editor only functions...
        if ((InFunction->FunctionFlags & FUNC_EditorOnly) != 0)
        {
            return false;
        }

        if (InFunction->HasMetaData("DeprecatedFunction"))
        {
            return false;
        }

        for (TFieldIterator<FProperty> PropertyIterator(InFunction); PropertyIterator; ++PropertyIterator)
        {
            if (FProperty* Property = *PropertyIterator; !IsSupportedElementProperty(Property, InTypeValidation))
            {
                return false;
            }
        }

        return true;
    }

    bool FStructTypeDefinition::IsSupportedElementProperty(FProperty* InProperty, FTypeValidation* InTypeValidation)
    {
        if (!IsSupportedProperty(InProperty, InTypeValidation))
        {
            return false;
        }

        if (InProperty->IsA<FDelegateProperty>() || InProperty->IsA<FMulticastDelegateProperty>())
        {
            return false;
        }

        if (InProperty->IsA<FSoftObjectProperty>() || InProperty->IsA<FSoftClassProperty>())
        {
            return false;
        }

        return true;
    }

    bool FStructTypeDefinition::IsSupportedProperty(FProperty* InProperty, FTypeValidation* InTypeValidation)
    {        
        // ignore deprecated property
        if ((InProperty->PropertyFlags & CPF_Deprecated) != 0 || InProperty->HasMetaData("DeprecatedProperty"))
        {
            return false;
        }

        // ignore editor only property
        if ((InProperty->PropertyFlags & CPF_EditorOnly) != 0)
        {
            return false;
        }

        if (const FStructProperty* StructProperty = CastField<FStructProperty>(InProperty))
        {
            return InTypeValidation->IsSupported(StructProperty->Struct);
        }

        if (const FClassProperty* ClassProperty = CastField<FClassProperty>(InProperty))
        {
            if (ClassProperty->MetaClass != nullptr)
            {
                return InTypeValidation->IsSupported(ClassProperty->MetaClass);
            }
        }
        else if (const FObjectProperty* ObjectProperty = CastField<FObjectProperty>(InProperty))
        {
            return InTypeValidation->IsSupported(ObjectProperty->PropertyClass);
        }
        else if (const FEnumProperty* EnumProperty = CastField<FEnumProperty>(InProperty))
        {
            return InTypeValidation->IsSupported(EnumProperty->GetEnum());
        }
        else if (const FArrayProperty* ArrayProperty = CastField<FArrayProperty>(InProperty))
        {
            FProperty* Property = ArrayProperty->Inner;
            check(Property != nullptr);

            return IsSupportedElementProperty(Property, InTypeValidation);
        }
        else if (const FSetProperty* SetProperty = CastField<FSetProperty>(InProperty))
        {
            FProperty* Property = SetProperty->ElementProp;
            check(Property != nullptr);
            return IsSupportedElementProperty(Property, InTypeValidation);
        }
        else if (const FMapProperty* MapProperty = CastField<FMapProperty>(InProperty))
        {
            FProperty* KeyProperty = MapProperty->KeyProp;
            FProperty* ValueProperty = MapProperty->ValueProp;
            check(KeyProperty != nullptr);
            check(ValueProperty != nullptr);
 
            return IsSupportedElementProperty(KeyProperty, InTypeValidation) && IsSupportedElementProperty(ValueProperty, InTypeValidation);
        }
        else if (const FDelegateProperty* DelegateProperty = CastField<FDelegateProperty>(InProperty))
        {
            check(DelegateProperty->SignatureFunction);

            return IsSupportedFunction(DelegateProperty->SignatureFunction, InTypeValidation);
        }
        else if (const FMulticastDelegateProperty* MulticastDelegateProperty = CastField<FMulticastDelegateProperty>(InProperty))
        {
            check(MulticastDelegateProperty);

            return IsSupportedFunction(MulticastDelegateProperty->SignatureFunction, InTypeValidation);
        }

        return 
            InProperty->IsA<FBoolProperty>() ||
            InProperty->IsA<FInt8Property>() ||
            InProperty->IsA<FByteProperty>() ||
            InProperty->IsA<FInt16Property>() ||
            InProperty->IsA<FUInt16Property>() ||
            InProperty->IsA<FIntProperty>() ||
            InProperty->IsA<FUInt32Property>() ||
            InProperty->IsA<FInt64Property>() ||
            InProperty->IsA<FUInt64Property>() ||
            InProperty->IsA<FFloatProperty>() ||
            InProperty->IsA<FDoubleProperty>() ||
            InProperty->IsA<FStrProperty>() ||
            InProperty->IsA<FNameProperty>() ||
            InProperty->IsA<FTextProperty>() ||
            InProperty->IsA<FClassProperty>() ||
#if ENGINE_MAJOR_VERSION < 5 || (ENGINE_MAJOR_VERSION == 5 && ENGINE_MINOR_VERSION < 4)
            InProperty->IsA<FClassPtrProperty>() ||
#endif
             InProperty->IsA<FSoftObjectProperty>() ||
             InProperty->IsA<FSoftClassProperty>();
    }

    FPropertyDefinition* FStructTypeDefinition::GetPropertyDefinition(const FString& InPropertyName)
    {
        return Properties.FindByPredicate([&](const FPropertyDefinition& InDef) {
            return InDef.Name == InPropertyName;
            });
    }

    const FPropertyDefinition* FStructTypeDefinition::GetPropertyDefinition(const FString& InPropertyName) const
    {
        return Properties.FindByPredicate([&](const FPropertyDefinition& InDef) {
            return InDef.Name == InPropertyName;
            });
    }

    void FStructTypeDefinition::AddDependNamespace(const FProperty* InProperty)
    {
        if (const auto ArrayProperty = CastField<FArrayProperty>(InProperty))
        {
            AddDependNamespace(ArrayProperty->Inner);
        }
        else if (const auto SetProperty = CastField<FSetProperty>(InProperty))
        {
            AddDependNamespace(SetProperty->ElementProp);
        }
        else if (const auto MapProperty = CastField<FMapProperty>(InProperty))
        {
            AddDependNamespace(MapProperty->GetKeyProperty());
            AddDependNamespace(MapProperty->GetValueProperty());
        }
        else if (const auto Field = FUnrealSharpUtils::GetPropertyInnerField(InProperty))
        {
            AddDependNamespace(Field);
        }
    }

    static FString ExtractNamespace(const FString& CSharpFullPath)
    {
        if (int32 LastDotIndex; CSharpFullPath.FindLastChar(TEXT('.'), LastDotIndex))
        {
            return CSharpFullPath.Left(LastDotIndex);
        }
        
        // No dot found, return the whole string
        return CSharpFullPath;
    }

    void FStructTypeDefinition::AddDependNamespace(const UField* InField)
    {
        if (FUnrealSharpUtils::IsCSharpField(InField))
        {
            const FString CSharpFullPath = FUnrealSharpUtils::GetCSharpFullPath(InField);

            const auto DependNamespace = ExtractNamespace(CSharpFullPath);

            DependNamespaces.Add(DependNamespace, 0);
        }
    }
}
