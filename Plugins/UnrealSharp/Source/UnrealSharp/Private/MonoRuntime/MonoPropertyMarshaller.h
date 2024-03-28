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

#include "IPropertyMarshaller.h"
#include "ICSharpMethodInvocation.h"

#if WITH_MONO
namespace UnrealSharp::Mono
{
    /*
    * check mono document
    * http://docs.go-mono.com/?link=xhtml%3adeploy%2fmono-api-methods.html
    */
    class FPropertyMarshallerCollection
    {
    public:
        FPropertyMarshallerCollection();

        template <typename TMarshallerType>
        void AddMarshaller()
        {
            TSharedPtr<TMarshallerType> Marshaller = MakeShared<TMarshallerType>();

            for (auto& TypeClass : Marshaller->GetMatchedPropertyTypes())
            {
                check(!Marshallers.Contains(TypeClass));

                Marshallers.Add(TypeClass, Marshaller);
            }
        }

    public:
        const IPropertyMarshaller* GetMarshaller(const FProperty* InProperty) const;
        const IPropertyMarshaller* GetMarshaller(const FFieldClass* InFieldClass) const;

    private:
        TMap<const FFieldClass*, TSharedPtr<IPropertyMarshaller>>  Marshallers;
    };

    class FPropertyMarshaller : public IPropertyMarshaller
    {
    public:
        virtual const TArray<FFieldClass*>& GetMatchedPropertyTypes() const { return MatchedTypes; }

        virtual int   GetTempParameterBufferSize() const override;
        virtual void* GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const;

        virtual void AddParameter(const FPropertyMarshallerParameters& InParameters) const override;
        virtual void Copy(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const override;

    protected:
        virtual void ResetProperty(const FPropertyMarshallerParameters& InParameters) const;
        virtual void CopyReturnValue(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty) const;
        virtual void CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const;

        virtual void CopyValue(const void* InDestination, const void* InSource, FProperty* InProperty) const;

    protected:
        TArray<FFieldClass*> MatchedTypes;
    };

    template <typename TPropertyType, typename TCppType>
    class TBasePropertyMarshaller : public FPropertyMarshaller
    {
    public:
        TBasePropertyMarshaller()
        {
            MatchedTypes = { TPropertyType::StaticClass() };
        }

        virtual void CopyValue(const void* InDestination, const void* InSource, FProperty* InProperty) const override
        {
            checkSlow(InDestination && InSource);

            *(TCppType*)InDestination = *(const TCppType*)InSource;
        }
    };

    template <typename TPropertyType>
    class TPropertyMarshaller : public TBasePropertyMarshaller<TPropertyType, typename TPropertyType::TCppType>
    {
    public:
    };

    class FBoolPropertyMarshaller : public TPropertyMarshaller<FBoolProperty>
    {
    };

    class FEnumPropertyMarshaller : public FPropertyMarshaller
    {
    public:
        FEnumPropertyMarshaller();
        virtual void CopyValue(const void* InDestination, const void* InSource, FProperty* InProperty) const override;
    };

    class FStrPropertyMarshaller : public TBasePropertyMarshaller<FStrProperty, FString>
    {
    public:
        virtual void* GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const override;
        virtual void CopyReturnValue(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty) const override;
        virtual void CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const override;
    };

    class FNamePropertyMarshaller : public TBasePropertyMarshaller<FNameProperty, FName>
    {
    public:
        virtual int GetTempParameterBufferSize() const override;
        virtual void* GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const override;
        virtual void CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const override;
    };

    class FTextPropertyMarshaller : public TBasePropertyMarshaller<FTextProperty, FText>
    {
    public:
        virtual int GetTempParameterBufferSize() const override;
        virtual void* GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const override;
        virtual void CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const override;
    };

    class FObjectPropertyMarshaller : public TBasePropertyMarshaller<FObjectProperty, UObject*>
    {
    public:
        virtual void* GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const override;
        virtual void CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const override;
    };

    class FClassPropertyMarshaller : public FPropertyMarshaller
    {
    public:
        FClassPropertyMarshaller();

        virtual int GetTempParameterBufferSize() const override;
        virtual void* GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const override;
        virtual void CopyReturnValue(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty) const override;
        virtual void CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const override;
    };

    class FSoftObjectPropertyMarshaller : public FPropertyMarshaller
    {
    public:
        FSoftObjectPropertyMarshaller();

        virtual void* GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const override;
        virtual void CopyReturnValue(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty) const override;
        virtual void CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const override;
    };

    class FSoftClassPropertyMarshaller : public FPropertyMarshaller
    {
    public:
        FSoftClassPropertyMarshaller();

        virtual void* GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const override;
        virtual void CopyReturnValue(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty) const override;
        virtual void CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const override;
    };

    class FStructPropertyMarshaller : public TBasePropertyMarshaller<FStructProperty, void*>
    {
    public:
        virtual void* GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const override;
        virtual void CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const override;
    };

    class FCollectionPropertyMarshaller : public FPropertyMarshaller
    {
    public:
        FCollectionPropertyMarshaller();

        virtual void* GetPassToCSharpPointer(const FPropertyMarshallerParameters& InParameters) const override;
        virtual void CopyReturnValue(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty) const override;
        virtual void CopyProperty(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const override;
    };
}
#endif

