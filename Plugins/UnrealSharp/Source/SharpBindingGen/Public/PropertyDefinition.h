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
#pragma once

#include "MetaDefinition.h"

namespace UnrealSharp
{
    // 
    enum class EReferenceType
    {
        Unknown,
        BuiltInType,
        UnrealType,
        UserType
    };

    class FTypeValidation;
    class FFunctionTypeDefinition;

    // Property Definition
    // Function parameters are PropertyDefinition too...
    class SHARPBINDINGGEN_API FPropertyDefinition
    {
    public:
        typedef TSharedPtr<FPropertyDefinition> FPropertyDefinitionPtr;
        
        FPropertyDefinition();
        FPropertyDefinition(UStruct* InStruct, void* InDefaultObjectPtr, FProperty* InProperty, FTypeValidation* InTypeValidation);

        void                                    Write(FJsonObject& InObject);
        void                                    Read(FJsonObject& InObject);

        bool                                    IsReference() const;
        bool                                    IsOut() const;
        bool                                    IsConst() const;
        bool                                    IsEditConst() const;
        bool                                    IsReturnProperty() const;
        bool                                    IsOutputProperty() const { return IsOut() || IsReturnProperty(); }
        bool                                    IsInputProperty() const { return !IsOutputProperty(); }

        bool                                    IsBoolProperty() const { return TypeClass == TEXT("BoolProperty"); }
        bool                                    IsByteProperty() const { return TypeClass == TEXT("ByteProperty"); }
        bool                                    IsIntProperty() const { return TypeClass == TEXT("IntProperty"); }
        bool                                    IsInt64Property() const { return TypeClass == TEXT("Int64Property"); }
        bool                                    IsFloatProperty() const { return TypeClass == TEXT("FloatProperty"); }
        bool                                    IsDoubleProperty() const { return TypeClass == TEXT("DoubleProperty"); }
        bool                                    IsStringProperty() const { return TypeClass == TEXT("StrProperty"); }
        bool                                    IsNameProperty() const { return TypeClass == TEXT("NameProperty"); }
        bool                                    IsTextProperty() const { return TypeClass == TEXT("TextProperty"); }
        bool                                    IsClassProperty() const { return TypeClass == TEXT("ClassProperty"); }
        bool                                    IsClassPtrProperty() const { return TypeClass == TEXT("ClassPtrProperty"); }
        bool                                    IsObjectProperty() const { return TypeClass == TEXT("ObjectProperty"); }
        bool                                    IsStructProperty() const { return TypeClass == TEXT("StructProperty"); }
        bool                                    IsEnumProperty() const { return TypeClass == TEXT("EnumProperty"); }
        bool                                    IsArrayProperty() const { return TypeClass == TEXT("ArrayProperty"); }
        bool                                    IsSetProperty() const { return TypeClass == TEXT("SetProperty"); }
        bool                                    IsMapProperty() const { return TypeClass == TEXT("MapProperty"); }
        bool                                    IsSoftObjectProperty() const { return TypeClass == TEXT("SoftObjectProperty"); }
        bool                                    IsSoftClassProperty() const { return TypeClass == TEXT("SoftClassProperty"); }

        bool                                    IsDelegateRelatedProperty() const
        { 
            return IsDelegateProperty() || IsMulticastDelegateProperty();
        }
        
        bool                                    IsDelegateProperty() const{ return TypeClass == TEXT("DelegateProperty"); }

        bool                                    IsMulticastDelegateProperty() const{ 
            return TypeClass == TEXT("MulticastDelegateProperty") ||
                TypeClass == TEXT("MulticastInlineDelegateProperty") ||
                TypeClass == TEXT("MulticastSparseDelegateProperty");
        }

        bool                                    IsAttachToActorProperty() const{ return bIsActorComponent; }

    public:
        FString                                 CppTypeName;
        FString                                 TypeName;
        FString                                 TypeClass;
        FString                                 Name;
        FString                                 ClassPath;
        FString                                 DefaultValue;
        FString                                 MetaClass;
        FString                                 AttachToComponentName;
        FString                                 AttachToSocketName;
        FString                                 ReplicatedUsing;
        ELifetimeCondition                      ReplicationCondition = COND_None;
        uint64                                  PropertyFlags = 0;
        int                                     Offset = 0;
        int                                     Size = 0;
        uint8                                   FieldMask = 0;
        FGuid                                   Guid;
        EReferenceType                          ReferenceType = EReferenceType::Unknown;

        TArray<FPropertyDefinitionPtr>          InnerProperties;

        FMetaDefinition                         Metas;

        TSharedPtr<FFunctionTypeDefinition>     SignatureFunction;
        bool                                    bIsActorComponent = false;
    };
}
