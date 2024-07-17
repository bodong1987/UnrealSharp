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
#include "MonoRuntime/MonoPropertyMarshaller.h"
#include "ICSharpMethodInvocation.h"
#include "Misc/CSharpStructures.h"

#if WITH_MONO
#include "MonoRuntime/MonoInteropUtils.h"
#include "ICSharpRuntime.h"
#include "ICSharpLibraryAccessor.h"

namespace UnrealSharp::Mono
{
    FPropertyMarshallerCollection::FPropertyMarshallerCollection()
    {
        AddMarshaller<FBoolPropertyMarshaller>();
        AddMarshaller<TPropertyMarshaller<FByteProperty>>();
        AddMarshaller<TPropertyMarshaller<FIntProperty>>();
        AddMarshaller<TPropertyMarshaller<FInt64Property>>();
        AddMarshaller<TPropertyMarshaller<FFloatProperty>>();
        AddMarshaller<TPropertyMarshaller<FDoubleProperty>>();        
        AddMarshaller<FEnumPropertyMarshaller>();
        AddMarshaller<FStrPropertyMarshaller>();
        AddMarshaller<FNamePropertyMarshaller>();
        AddMarshaller<FTextPropertyMarshaller>();
        AddMarshaller<FObjectPropertyMarshaller>();
        AddMarshaller<FClassPropertyMarshaller>();
        AddMarshaller<FSoftObjectPropertyMarshaller>();
        AddMarshaller<FSoftClassPropertyMarshaller>();
        AddMarshaller<FStructPropertyMarshaller>();
        AddMarshaller<FCollectionPropertyMarshaller>();
    }

    const IPropertyMarshaller* FPropertyMarshallerCollection::GetMarshaller(const FProperty* InProperty) const
    {
        checkSlow(InProperty!=nullptr);

        const auto Class = InProperty->GetClass();

        return GetMarshaller(Class);
    }

    const IPropertyMarshaller* FPropertyMarshallerCollection::GetMarshaller(const FFieldClass* InFieldClass) const
    {
        checkSlow(InFieldClass!=nullptr);

        auto* Ptr = Marshallers.Find(InFieldClass);

        checkf(Ptr != nullptr, TEXT("Failed find Marshaller for type:%s"), *InFieldClass->GetName());

        return Ptr->Get();
    }

    void FPropertyMarshaller::CopyValue(const void* InDestination, const void* InSource, FProperty* InProperty) const
    {
        checkNoEntry();
    }

    int FPropertyMarshaller::GetTempParameterBufferSize() const
    {
        return sizeof(void*);
    }

    void* FPropertyMarshaller::GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const
    {        
        if (InParameters.bPassAsReference)
        {
            *InParameters.InputReferenceAddress = InParameters.InputAddress;
        }
        
        return InParameters.InputAddress;
    }

    void FPropertyMarshaller::ResetProperty(const FPropertyMarshallerParameters& InParameters) const
    {
        // I will use this method for the time being. If there is a better method, I will change it later.
        // or you can override ResetProperty make it faster
        InParameters.Property->ImportText_Direct(TEXT(""), InParameters.InputAddress, nullptr, PPF_None);
    }

    void FPropertyMarshaller::AddParameter(const FPropertyMarshallerParameters& InParameters) const
    {
        if (InParameters.bPassAsReference)
        {
            // pass by reference
            // so we need reset the property
            ResetProperty(InParameters);
        }

        void* PassToCSharpPointer = GetPassToCSharpPointer(InParameters);

        InParameters.Invocation->AddArgument(PassToCSharpPointer);
    }

    void FPropertyMarshaller::Copy(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const
    {
        if (InCopyDirection == EMarshalCopyDirection::CSharpReturnValueToUnreal)
        {            
            CopyReturnValue(InUnrealDataPointer, InCSharpDataPointer, InProperty);
        }
        else
        {
            CopyProperty(InUnrealDataPointer, InCSharpDataPointer, InProperty, InCopyDirection);
        }        
    }

    void FPropertyMarshaller::CopyReturnValue(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty) const
    {
        check(InCSharpDataPointer);

        MonoObject* ObjectPtr = (MonoObject*)InCSharpDataPointer;// NOLINT
        MonoClass* Klass = mono_object_get_class(ObjectPtr);

        if (mono_class_is_valuetype(Klass))
        {
            const void* RawAddress = mono_object_unbox(ObjectPtr);

            CopyProperty(InUnrealDataPointer, RawAddress, InProperty, EMarshalCopyDirection::CSharpToUnreal);
        }
        else
        {            
            CopyProperty(InUnrealDataPointer, InCSharpDataPointer, InProperty, EMarshalCopyDirection::CSharpToUnreal);
        }
    }

    void FPropertyMarshaller::CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const
    {
        if (InCopyDirection == EMarshalCopyDirection::CSharpToUnreal)
        {
            CopyValue(InUnrealDataPointer, InCSharpDataPointer, InProperty);
        }
        else if (InCopyDirection == EMarshalCopyDirection::UnrealToCSharp)
        {
            CopyValue(InCSharpDataPointer, InUnrealDataPointer, InProperty);
        }
    }
            
    enum class ELocalTempEnumSize8 : uint8
    {
    };

    enum class ELocalTempEnumSize16 : uint16 
    {
    };

    enum class ELocalTempEnumSize32 : uint32
    {
    };

    enum class ELocalTempEnumSize64 : uint64
    {
    };

    FEnumPropertyMarshaller::FEnumPropertyMarshaller()
    {
        MatchedTypes = {
            FEnumProperty::StaticClass()
        };
    }

    void FEnumPropertyMarshaller::CopyValue(const void* InDestination, const void* InSource, FProperty* InProperty) const
    {        
        checkSlow(InDestination && InSource);
        
        const int EnumSize = InProperty->GetSize();// NOLINT

        switch (EnumSize)
        {
            case sizeof(ELocalTempEnumSize8) :
            {
                *(ELocalTempEnumSize8*)InDestination = *(const ELocalTempEnumSize8*)InSource;// NOLINT
            }
            break;

            case sizeof(ELocalTempEnumSize32) :
            {
                *(ELocalTempEnumSize32*)InDestination = *(const ELocalTempEnumSize32*)InSource; // NOLINT
            }
            break;

            case sizeof(ELocalTempEnumSize64) :
            {
                *(ELocalTempEnumSize64*)InDestination = *(const ELocalTempEnumSize64*)InSource; // NOLINT
            }
            break;

            case sizeof(ELocalTempEnumSize16) :
            {
                *(ELocalTempEnumSize16*)InDestination = *(const ELocalTempEnumSize16*)InSource; // NOLINT
            }
            break;
            default:
            {
                checkf(false, TEXT("Unsupported enum size:%d"), EnumSize);
            }
        }
    }

    void* FStrPropertyMarshaller::GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const
    {
        // MonoString* is MonoObject*
        MonoString* CSharpString = FMonoInteropUtils::GetMonoString(*(FString*)InParameters.InputAddress); // NOLINT

        if (InParameters.bPassAsReference)
        {
            *InParameters.InputReferenceAddress = CSharpString;

            return InParameters.InputReferenceAddress;
        }

        return CSharpString;
    }    
    
    void FStrPropertyMarshaller::CopyReturnValue(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty) const
    {
        CopyProperty(InUnrealDataPointer, InCSharpDataPointer, InProperty, EMarshalCopyDirection::CSharpToUnreal);
    }

    void FStrPropertyMarshaller::CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const
    {
        if (InCopyDirection == EMarshalCopyDirection::CSharpToUnreal)
        {
            MonoString* CSharpString = (MonoString*)InCSharpDataPointer; // NOLINT

            *(FString*)InUnrealDataPointer = FMonoInteropUtils::GetFString(CSharpString); // NOLINT
        }
        else
        {
            check(InUnrealDataPointer && InCSharpDataPointer);

            MonoString* CSharpString = FMonoInteropUtils::GetMonoString(*(FString*)InUnrealDataPointer); // NOLINT

            *(MonoString**)InCSharpDataPointer = CSharpString; // NOLINT
        }
    }

    int FNamePropertyMarshaller::GetTempParameterBufferSize() const
    {
#if WITH_EDITOR
        static_assert(sizeof(FName) == 12, "C# version FName need native size of FName equal to 12(In Editor Mode)");
#else
        static_assert(sizeof(FName) == 8, "C# version FName need native size of FName equal to 8(In Game Mode)");
#endif

        return sizeof(void*) + sizeof(FName);
    }

    void* FNamePropertyMarshaller::GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const
    {
        const FName* NamePtr = (const FName*)InParameters.InputAddress; // NOLINT
        FName* TempBuffer = (FName*)(InParameters.InputReferenceAddress + 1); // NOLINT

        checkSlow(reinterpret_cast<SIZE_T>(static_cast<void*>(TempBuffer)) - reinterpret_cast<SIZE_T>(static_cast<void*>(InParameters.InputReferenceAddress)) == sizeof(void*));

        // For special processing of this type, we obtain more temporary space through GetTempParameterBufferSize, 
        // and then store the data in the extra space. 
        // This part of the content is aligned with the structure on the C# side, 
        // so Marshall operations can be quickly performed.
        *TempBuffer = *NamePtr;

        if (InParameters.bPassAsReference)
        {
            *InParameters.InputReferenceAddress = TempBuffer;
        }        

        return TempBuffer;
    }

    void FNamePropertyMarshaller::CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const
    {
        if (InCopyDirection == EMarshalCopyDirection::CSharpToUnreal)
        {
            const FName* NamePtr = (const FName*)InCSharpDataPointer; // NOLINT

            *(FName*)InUnrealDataPointer = *NamePtr; // NOLINT
        }
        else if (InCopyDirection == EMarshalCopyDirection::UnrealToCSharp)
        {
            const FName* NamePtr = (const FName*)InUnrealDataPointer; // NOLINT

            *(FName*)InCSharpDataPointer = *NamePtr; // NOLINT
        }
    }    

    int FTextPropertyMarshaller::GetTempParameterBufferSize() const
    {
        return sizeof(void*) + sizeof(FCSharpText);
    }

    void* FTextPropertyMarshaller::GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const
    {
        const FText* TextPtr = (const FText*)InParameters.InputAddress; // NOLINT
        const FString Text = TextPtr->ToString();

        FCSharpText* TempBuffer = (FCSharpText*)(InParameters.InputReferenceAddress + 1); // NOLINT
        checkSlow(reinterpret_cast<SIZE_T>(TempBuffer) - reinterpret_cast<SIZE_T>(InParameters.InputReferenceAddress) == sizeof(void*));

        // For special processing of this type, we obtain more temporary space through GetTempParameterBufferSize, 
        // and then store the data in the extra space. 
        // This part of the content is aligned with the structure on the C# side, 
        // so Marshall operations can be quickly performed.
        TempBuffer->Text = FMonoInteropUtils::GetMonoString(Text);
        
        if (InParameters.bPassAsReference)
        {
            *InParameters.InputReferenceAddress = TempBuffer;
        }

        return TempBuffer;
    }

    void FTextPropertyMarshaller::CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const
    {
        if (InCopyDirection == EMarshalCopyDirection::CSharpToUnreal)
        {
            FCSharpText* TextPtr = (FCSharpText*)InCSharpDataPointer; // NOLINT

            FString Text = FMonoInteropUtils::GetFString((MonoString*)TextPtr->Text); // NOLINT

            *(FText*)InUnrealDataPointer = FText::FromString(Text); // NOLINT
        }
        else if (InCopyDirection == EMarshalCopyDirection::UnrealToCSharp)
        {
            const FText* TextPtr = (const FText*)InUnrealDataPointer; // NOLINT

            FCSharpText* CSharpText = (FCSharpText*)InCSharpDataPointer; // NOLINT

            const FString Text = TextPtr->ToString();

            CSharpText->Text = FMonoInteropUtils::GetMonoString(Text);
        }
    }

    void* FObjectPropertyMarshaller::GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const
    {
        FCSharpObjectMarshalValue Value = FMonoInteropUtils::GetCSharpObjectOfUnrealObject(*(UObject**)InParameters.InputAddress); // NOLINT
        MonoObject* ObjectPtr = (MonoObject*)Value.ObjectPtr; // NOLINT

        if (InParameters.bPassAsReference)
        {
            *InParameters.InputReferenceAddress = ObjectPtr;

            return InParameters.InputReferenceAddress;
        }

        return ObjectPtr;
    }

    void FObjectPropertyMarshaller::CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const
    {
        if (InCopyDirection == EMarshalCopyDirection::CSharpToUnreal)
        {
            MonoObject* ObjectPtr = (MonoObject*)InCSharpDataPointer; // NOLINT

            FMonoInteropUtils::DumpMonoObjectInformation(ObjectPtr);

            UObject* UnrealObjectPtr = FMonoInteropUtils::GetUnrealObjectOfCSharpObject(ObjectPtr);

            *(UObject**)InUnrealDataPointer = UnrealObjectPtr; // NOLINT
        }
        else if (InCopyDirection == EMarshalCopyDirection::UnrealToCSharp)
        {
            UObject* Param = *(UObject**)InUnrealDataPointer; // NOLINT

            const auto Value = FMonoInteropUtils::GetCSharpObjectOfUnrealObject(Param); // NOLINT

            MonoObject* ObjectPtr = (MonoObject*)Value.ObjectPtr; // NOLINT

            *(MonoObject**)InCSharpDataPointer = ObjectPtr; // NOLINT
        }
    }

    FClassPropertyMarshaller::FClassPropertyMarshaller()
    {
        MatchedTypes = 
        { 
            FClassProperty::StaticClass()
#if ENGINE_MAJOR_VERSION < 5 || (ENGINE_MAJOR_VERSION == 5 && ENGINE_MINOR_VERSION < 4)
            ,FClassPtrProperty::StaticClass() 
#endif
        };
    }

    int FClassPropertyMarshaller::GetTempParameterBufferSize() const
    {
        static_assert(sizeof(FCSharpSubclassOf) == sizeof(void*), "Invalid size.");

        return sizeof(void*) + sizeof(FCSharpSubclassOf);
    }

    void* FClassPropertyMarshaller::GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const
    {
        const FClassProperty* ClassProperty = CastField<FClassProperty>(InParameters.Property);
        check(ClassProperty);
        check(ClassProperty->MetaClass);

        const UClass** AddressOfClassPointer = (const UClass**)InParameters.InputAddress; // NOLINT
        FCSharpSubclassOf* TempBuffer = (FCSharpSubclassOf*)(InParameters.InputReferenceAddress + 1); // NOLINT

        checkSlow(reinterpret_cast<SIZE_T>(TempBuffer) - reinterpret_cast<SIZE_T>(InParameters.InputReferenceAddress) == sizeof(void*));

        // For special processing of this type, we obtain more temporary space through GetTempParameterBufferSize, 
        // and then store the data in the extra space. 
        // This part of the content is aligned with the structure on the C# side, 
        // so Marshall operations can be quickly performed.
        TempBuffer->ClassPtr = *AddressOfClassPointer;

        if (InParameters.bPassAsReference)
        {
            *InParameters.InputReferenceAddress = TempBuffer;
        }

        return TempBuffer;
    }

    void FClassPropertyMarshaller::CopyReturnValue(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty) const
    {
        // ClassProperty always use TSubclassOf<T>, so it is always value type.
        check(InCSharpDataPointer);
        
        MonoObject* ObjectPtr = (MonoObject*)InCSharpDataPointer; // NOLINT

#if !UE_BUILD_SHIPPING
        MonoClass* Klass = mono_object_get_class(ObjectPtr);

        check(mono_class_is_valuetype(Klass));
#endif

        const void* RawAddress = mono_object_unbox(ObjectPtr);

        CopyProperty(InUnrealDataPointer, RawAddress, InProperty, EMarshalCopyDirection::CSharpToUnreal);
    }

    void FClassPropertyMarshaller::CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const
    {
        if (InCopyDirection == EMarshalCopyDirection::CSharpToUnreal)
        {
            const FCSharpSubclassOf* SubclassOfPtr = (const FCSharpSubclassOf*)InCSharpDataPointer; // NOLINT

            *(const UClass**)InUnrealDataPointer = SubclassOfPtr->ClassPtr; // NOLINT
        }
        else if (InCopyDirection == EMarshalCopyDirection::UnrealToCSharp)
        {
            const FCSharpSubclassOf* SubclassOfPtr = (const FCSharpSubclassOf*)InUnrealDataPointer; // NOLINT

            ((FCSharpSubclassOf*)InCSharpDataPointer)->ClassPtr = SubclassOfPtr->ClassPtr; // NOLINT
        }
    }

    FSoftObjectPropertyMarshaller::FSoftObjectPropertyMarshaller()
    {
        MatchedTypes = {FSoftObjectProperty::StaticClass()};
    }

    void* FSoftObjectPropertyMarshaller::GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const
    {
        ICSharpRuntime* Runtime = FCSharpRuntimeFactory::GetInstance();
        checkSlow(Runtime);

        FSoftObjectProperty* SoftObjectProperty = CastField<FSoftObjectProperty>(InParameters.Property);
        checkSlow(SoftObjectProperty);

        void* CSharpSoftObject = Runtime->GetCSharpLibraryAccessor()->CreateCSharpSoftObjectPtr(InParameters.InputAddress, SoftObjectProperty);

        if (InParameters.bPassAsReference)
        {
            *InParameters.InputReferenceAddress = CSharpSoftObject;

            return InParameters.InputReferenceAddress;
        }

        return CSharpSoftObject;
    }

    void FSoftObjectPropertyMarshaller::CopyReturnValue(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty) const
    {
        // soft object is copy data from MonoObject*
        CopyProperty(InUnrealDataPointer, InCSharpDataPointer, InProperty, EMarshalCopyDirection::CSharpToUnreal);
    }

    void FSoftObjectPropertyMarshaller::CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const
    {
        if (InCopyDirection == EMarshalCopyDirection::CSharpToUnreal)
        {
            // InCSharpDataPointer is MonoObject*
            MonoObject* CSharpObject = (MonoObject*)InCSharpDataPointer; // NOLINT
            //FMonoInteropUtils::DumpMonoObjectInformation(CSharpObject);

            ICSharpRuntime* Runtime = FCSharpRuntimeFactory::GetInstance();
            checkSlow(Runtime);
            Runtime->GetCSharpLibraryAccessor()->CopySoftObjectPtr((void*)InUnrealDataPointer, CSharpObject); // NOLINT
        }
        else
        {
            // this operate is not supported.
            checkNoEntry();
        }
    }

    FSoftClassPropertyMarshaller::FSoftClassPropertyMarshaller()
    {
        MatchedTypes = {FSoftClassProperty::StaticClass()};
    }

    void* FSoftClassPropertyMarshaller::GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const
    {
        ICSharpRuntime* Runtime = FCSharpRuntimeFactory::GetInstance();
        checkSlow(Runtime);

        FSoftClassProperty* SoftClassProperty = CastField<FSoftClassProperty>(InParameters.Property);
        checkSlow(SoftClassProperty);

        void* CSharpSoftClass = Runtime->GetCSharpLibraryAccessor()->CreateCSharpSoftClassPtr(InParameters.InputAddress, SoftClassProperty);

        if (InParameters.bPassAsReference)
        {
            *InParameters.InputReferenceAddress = CSharpSoftClass;

            return InParameters.InputReferenceAddress;
        }

        return CSharpSoftClass;
    }

    void FSoftClassPropertyMarshaller::CopyReturnValue(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty) const
    {
        // TSoftClassPtr<T> in C# is always object(MonoObject*)
        CopyProperty(InUnrealDataPointer, InCSharpDataPointer, InProperty, EMarshalCopyDirection::CSharpToUnreal);
    }

    void FSoftClassPropertyMarshaller::CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const
    {
        if (InCopyDirection == EMarshalCopyDirection::CSharpToUnreal)
        {
            // InCSharpDataPointer is MonoObject*
            MonoObject* CSharpObject = (MonoObject*)InCSharpDataPointer; // NOLINT
            //FMonoInteropUtils::DumpMonoObjectInformation(CSharpObject);

            ICSharpRuntime* Runtime = FCSharpRuntimeFactory::GetInstance();
            checkSlow(Runtime);
            Runtime->GetCSharpLibraryAccessor()->CopySoftClassPtr((void*)InUnrealDataPointer, CSharpObject); // NOLINT
        }
        else
        {
            // this operate is not supported.
            checkNoEntry();
        }
    }

    void* FStructPropertyMarshaller::GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const
    {
        const FStructProperty* StructProperty = CastField<FStructProperty>(InParameters.Property);
        checkSlow(StructProperty != nullptr);

        UScriptStruct* Struct = Cast<UScriptStruct>(StructProperty->Struct);
        check(Struct);

        void* CSharpStructPtr = FMonoInteropUtils::CreateCSharpStruct(InParameters.InputAddress, Struct);        
        checkf(CSharpStructPtr != nullptr, TEXT("Failed create C# struct of type:%s, please check log for more information."), *Struct->GetStructCPPName());

#if !UE_BUILD_SHIPPING
        // validate type
        check(mono_class_is_valuetype(mono_object_get_class((MonoObject*)CSharpStructPtr))); // NOLINT
#endif

        void* TargetPtr = mono_object_unbox((MonoObject*)CSharpStructPtr); // NOLINT
        
        if (InParameters.bPassAsReference)
        {
            *InParameters.InputReferenceAddress = TargetPtr;
        }

        return TargetPtr;
    }

    void FStructPropertyMarshaller::CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const
    {
        if (InCopyDirection == EMarshalCopyDirection::CSharpToUnreal)
        {
            const FStructProperty* StructProperty = CastField<FStructProperty>(InProperty);
            checkSlow(StructProperty != nullptr);

            UScriptStruct* Struct = Cast<UScriptStruct>(StructProperty->Struct);
            check(Struct);

            FMonoInteropUtils::StructToNative(Struct, (void*)InUnrealDataPointer, InCSharpDataPointer); // NOLINT
        }
        else if(InCopyDirection == EMarshalCopyDirection::UnrealToCSharp)
        {
            // this operate is not supported.
            checkNoEntry();
        }
    }

    FCollectionPropertyMarshaller::FCollectionPropertyMarshaller()
    {
        MatchedTypes = {
            FArrayProperty::StaticClass(),
            FSetProperty::StaticClass(),
            FMapProperty::StaticClass()
        };
    }
    
    void* FCollectionPropertyMarshaller::GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const
    {
        checkSlow(InParameters.Property->IsA<FArrayProperty>() || InParameters.Property->IsA<FSetProperty>() || InParameters.Property->IsA<FMapProperty>());

        void* CSharpCollection = FInteropUtils::CreateCSharpCollection(InParameters.InputAddress, InParameters.Property);

        if (InParameters.bPassAsReference)
        {
            *InParameters.InputReferenceAddress = CSharpCollection;

            return InParameters.InputReferenceAddress;
        }

        return CSharpCollection;
    }

    void FCollectionPropertyMarshaller::CopyReturnValue(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty) const
    {
        CopyProperty(InUnrealDataPointer, InCSharpDataPointer, InProperty, EMarshalCopyDirection::CSharpToUnreal);
    }

    void FCollectionPropertyMarshaller::CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const
    {
        if (InCopyDirection == EMarshalCopyDirection::CSharpToUnreal)
        {
            FMonoInteropUtils::CopyFromCSharpCollection((void*)InUnrealDataPointer, InProperty, (void*)InCSharpDataPointer); // NOLINT
        }
        else
        {
            // this operate is not supported.
            checkNoEntry();
        }
    }
}
#endif
