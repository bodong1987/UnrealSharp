﻿/*
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
#include "PropertyDefinition.h"
#include <inttypes.h>
#include "FunctionTypeDefinition.h"
#include "Misc/UnrealSharpUtils.h"

namespace UnrealSharp
{
    FPropertyDefinition::FPropertyDefinition()
    {
    }

    FPropertyDefinition::FPropertyDefinition(UStruct* InStruct, void* InDefaultObjectPtr, FProperty* InProperty, FTypeValidation* InTypeValidation)
    {
        TypeName = CppTypeName = InProperty->GetCPPType();         
        TypeClass = InProperty->GetClass()->GetName();
        Name = InProperty->GetName();        
        Offset = InProperty->GetOffset_ReplaceWith_ContainerPtrToValuePtr();
        PropertyFlags = (uint64)InProperty->GetPropertyFlags();
        Size = InProperty->GetSize();

        if (InDefaultObjectPtr != nullptr)
        {
            InProperty->ExportText_InContainer(0, DefaultValue, InDefaultObjectPtr, InDefaultObjectPtr, nullptr, PPF_None);
        }
                        
        if (InProperty->IsA<FStructProperty>() ||
            InProperty->IsA<FObjectProperty>() ||
            InProperty->IsA<FClassProperty>() ||
            InProperty->IsA<FEnumProperty>())
        {
            ReferenceType = EReferenceType::UnrealType;
        }
        else
        {
            ReferenceType = EReferenceType::BuiltInType;
        }        

        if (FBoolProperty* BoolProperty = CastField<FBoolProperty>(InProperty))
        {
#if ENGINE_MAJOR_VERSION == 5 && ENGINE_MINOR_VERSION >= 3
            FieldMask = BoolProperty->GetFieldMask();
#else
            // low version does not support get field mask.... so...
            if (BoolProperty->IsNativeBool())
            {
                FieldMask = 0xFF;
            }
#endif
        }
        else if (FEnumProperty* EnumProperty = CastField<FEnumProperty>(InProperty))
        {
            ClassPath = EnumProperty->GetEnum()->GetPathName();
        }
        else if (FClassProperty* ClassProperty = CastField<FClassProperty>(InProperty))
        {
            if (ClassProperty->MetaClass)
            {
                MetaClass = FUnrealSharpUtils::GetCppTypeName(ClassProperty->MetaClass);
            }            
        }
        else if (FObjectProperty* ObjectProperty = CastField<FObjectProperty>(InProperty))
        {
            ClassPath = ObjectProperty->PropertyClass->GetPathName();
            TypeName = ObjectProperty->PropertyClass->GetName();
        }
        else if (FStructProperty* StructProperty = CastField<FStructProperty>(InProperty))
        {
            ClassPath = StructProperty->Struct->GetPathName();
            TypeName = StructProperty->Struct->GetName();
        }
        else if (FArrayProperty* ArrayProperty = CastField<FArrayProperty>(InProperty))
        {
            FProperty* Property = ArrayProperty->Inner;
            check(Property != nullptr);

            InnerProperties.Add(MakeShared<FPropertyDefinition>(InStruct, nullptr, Property, InTypeValidation));
        }
        else if (FSetProperty* SetProperty = CastField<FSetProperty>(InProperty))
        {
            FProperty* Property = SetProperty->ElementProp;
            check(Property != nullptr);

            InnerProperties.Add(MakeShared<FPropertyDefinition>(InStruct, nullptr, Property, InTypeValidation));
        }
        else if (FMapProperty* MapProperty = CastField<FMapProperty>(InProperty))
        {
            FProperty* KeyProperty = MapProperty->KeyProp;
            FProperty* ValueProperty = MapProperty->ValueProp;
            check(KeyProperty != nullptr);
            check(ValueProperty != nullptr);

            InnerProperties.Add(MakeShared<FPropertyDefinition>(InStruct, nullptr, KeyProperty, InTypeValidation));
            InnerProperties.Add(MakeShared<FPropertyDefinition>(InStruct, nullptr, ValueProperty, InTypeValidation));
        }
        else if (FDelegateProperty* DelegateProperty = CastField<FDelegateProperty>(InProperty))
        {
            SignatureFunction = MakeShared<FFunctionTypeDefinition>(DelegateProperty->SignatureFunction, InTypeValidation);
        }
        else if (FMulticastDelegateProperty* MulticastDelegateProperty = CastField<FMulticastDelegateProperty>(InProperty))
        {
            SignatureFunction = MakeShared<FFunctionTypeDefinition>(MulticastDelegateProperty->SignatureFunction, InTypeValidation);
        }

        Metas.Load(InProperty);
    }

    void FPropertyDefinition::Write(FJsonObject& InObject)
    {
        InObject.SetStringField(TEXT("CppTypeName"), CppTypeName);
        InObject.SetStringField(TEXT("TypeName"), TypeName);
        InObject.SetStringField(TEXT("TypeClass"), TypeClass);
        InObject.SetStringField(TEXT("Name"), Name);
        InObject.SetStringField(TEXT("ClassPath"), ClassPath);

        if (!DefaultValue.IsEmpty())
        {
            InObject.SetStringField(TEXT("DefaultValue"), DefaultValue);
        }
        
        InObject.SetNumberField(TEXT("Offset"), Offset);
        InObject.SetStringField(TEXT("FlagsT"), FString::Printf(TEXT("%") TEXT(PRIu64), PropertyFlags));
        InObject.SetNumberField(TEXT("Size"), Size);

        if (FieldMask != 0xFF)
        {
            InObject.SetNumberField(TEXT("FieldMask"), FieldMask);
        }
        
        InObject.SetNumberField(TEXT("ReferenceType"), (int)ReferenceType);

        if (Guid.IsValid())
        {
            InObject.SetStringField(TEXT("Guid"), Guid.ToString(EGuidFormats::DigitsWithHyphensLower));
        }        

        if (!MetaClass.IsEmpty())
        {
            InObject.SetStringField(TEXT("MetaClass"), MetaClass);
        }

        if (!InnerProperties.IsEmpty())
        {
            TArray<TSharedPtr<FJsonValue>> TempInnerProperties;
            for (auto& Property : InnerProperties)
            {
                TSharedPtr<FJsonObject> PropertyObject = MakeShared<FJsonObject>();
                Property->Write(*PropertyObject);
                TempInnerProperties.Add(MakeShared<FJsonValueObject>(PropertyObject));
            }

            InObject.SetArrayField(TEXT("InnerProperties"), TempInnerProperties);
        }        

        if (SignatureFunction)
        {
            TSharedPtr<FJsonObject> SignatureFunctionObject = MakeShared<FJsonObject>();
            SignatureFunction->Write(*SignatureFunctionObject);

            InObject.SetObjectField(TEXT("SignatureFunction"), SignatureFunctionObject);
        }

        Metas.Write(InObject);
    }

    void FPropertyDefinition::Read(FJsonObject& InObject)
    {
        CppTypeName = InObject.GetStringField(TEXT("CppTypeName"));
        TypeName = InObject.GetStringField(TEXT("TypeName"));
        TypeClass = InObject.GetStringField(TEXT("TypeClass"));
        Name = InObject.GetStringField(TEXT("Name"));
        ClassPath = InObject.GetStringField(TEXT("ClassPath"));
        Offset = (int)InObject.GetNumberField(TEXT("Offset"));        
        LexFromString(PropertyFlags, *InObject.GetStringField(TEXT("FlagsT")));
        Size = (int)InObject.GetNumberField(TEXT("Size"));

        if (InObject.HasField(TEXT("FieldMask")))
        {
            FieldMask = (uint8)InObject.GetNumberField(TEXT("FieldMask"));
        }
        
        ReferenceType = (EReferenceType)InObject.GetNumberField(TEXT("ReferenceType"));

        InObject.TryGetStringField(TEXT("DefaultValue"), DefaultValue);
        InObject.TryGetStringField(TEXT("MetaClass"), MetaClass);

        if (InObject.HasField(TEXT("Guid")))
        {
            FGuid::Parse(InObject.GetStringField(TEXT("Guid")), Guid);
        }        

        const TArray< TSharedPtr<FJsonValue> >* InnerPropertiesRefPtr;
        if (InObject.TryGetArrayField(TEXT("InnerProperties"), InnerPropertiesRefPtr) && InnerPropertiesRefPtr)
        {
            for (auto& PropertyObject : *InnerPropertiesRefPtr)
            {
                TSharedPtr<FJsonObject>* ObjectPtr = nullptr;
                if (PropertyObject->TryGetObject(ObjectPtr) && ObjectPtr != nullptr)
                {
                    FPropertyDefinitionPtr Def = MakeShared<FPropertyDefinition>();
                    Def->Read(**ObjectPtr);
                    InnerProperties.Add(Def);
                }
            }
        }

        const TSharedPtr<FJsonObject>* SignatureFunctionObject;
        if (InObject.TryGetObjectField(TEXT("SignatureFunction"), SignatureFunctionObject) && SignatureFunctionObject)
        {
            SignatureFunction = MakeShared<FFunctionTypeDefinition>();

            SignatureFunction->Read(**SignatureFunctionObject);
        }
        
        Metas.Read(InObject);

        Metas.TryGetMeta(TEXT("IsActorComponent"), bIsActorComponent);
        Metas.TryGetMeta(TEXT("AttachToComponentName"), AttachToComponentName);
        Metas.TryGetMeta(TEXT("AttachToSocketName"), AttachToSocketName);
        Metas.TryGetMeta(TEXT("ReplicatedUsing"), ReplicatedUsing);
        Metas.TryGetMeta(TEXT("ReplicationCondition"), *(int*)&ReplicationCondition);
    }

    bool FPropertyDefinition::IsReference() const
    {
        return (PropertyFlags & CPF_ReferenceParm) != 0;
    }

    bool FPropertyDefinition::IsOut() const
    {
        return (PropertyFlags & CPF_OutParm) != 0;
    }

    bool FPropertyDefinition::IsConst() const
    {
        return (PropertyFlags & CPF_ConstParm) != 0;
    }

    bool FPropertyDefinition::IsEditConst() const
    {
        return (PropertyFlags & CPF_EditConst) != 0;
    }

    bool FPropertyDefinition::IsReturnProperty() const
    {
        return (PropertyFlags & CPF_ReturnParm) != 0;
    }
}
