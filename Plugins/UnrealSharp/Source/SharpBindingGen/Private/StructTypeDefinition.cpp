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
#include "SharpBindingGenSettings.h"
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

	void FStructTypeDefinition::LoadProperties(UStruct* InStruct, void* InDefaultObjectPtr, EFieldIterationFlags InFlags, FTypeValidation* InTypeValidation, TFunction<bool(FProperty*)> InAccessFunc)
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

			InObject.SetArrayField("DependNamespaces", TempNamespaces);
		}
	}

	void FStructTypeDefinition::Read(FJsonObject& InObject)
	{
		Super::Read(InObject);

		const TArray< TSharedPtr<FJsonValue> >* PropertiesRefPtr = nullptr;

		if (InObject.TryGetArrayField("Properties", PropertiesRefPtr) && PropertiesRefPtr)
		{
			for (auto& PropertyObject : *PropertiesRefPtr)
			{
				TSharedPtr<FJsonObject>* ObjectPtr = nullptr;
				if (PropertyObject->TryGetObject(ObjectPtr) && ObjectPtr != nullptr)
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
		if ((InFunction->FunctionFlags & EFunctionFlags::FUNC_EditorOnly) != 0)
		{
			return false;
		}

		if (InFunction->HasMetaData("DeprecatedFunction"))
		{
			return false;
		}

		for (TFieldIterator<FProperty> PropertyIterator(InFunction); PropertyIterator; ++PropertyIterator)
		{
			FProperty* Property = *PropertyIterator;

			if (!IsSupportedElementProperty(Property, InTypeValidation))
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

		if (FStructProperty* StructProperty = CastField<FStructProperty>(InProperty))
		{
			return InTypeValidation->IsSupported(StructProperty->Struct);
		}
		else if (FClassProperty* ClassProperty = CastField<FClassProperty>(InProperty))
		{
			if (ClassProperty->MetaClass != nullptr)
			{
				return InTypeValidation->IsSupported(ClassProperty->MetaClass);
			}
		}
		else if (FObjectProperty* ObjectProperty = CastField<FObjectProperty>(InProperty))
		{
			return InTypeValidation->IsSupported(ObjectProperty->PropertyClass);
		}
		else if (FEnumProperty* EnumProperty = CastField<FEnumProperty>(InProperty))
		{
			return InTypeValidation->IsSupported(EnumProperty->GetEnum());
		}
		else if (FArrayProperty* ArrayProperty = CastField<FArrayProperty>(InProperty))
		{
			FProperty* Property = ArrayProperty->Inner;
			check(Property != nullptr);

			return IsSupportedElementProperty(Property, InTypeValidation);
		}
 		else if (FSetProperty* SetProperty = CastField<FSetProperty>(InProperty))
 		{
 			FProperty* Property = SetProperty->ElementProp;
 			check(Property != nullptr);
 			return IsSupportedElementProperty(Property, InTypeValidation);
 		}
 		else if (FMapProperty* MapProperty = CastField<FMapProperty>(InProperty))
 		{
 			FProperty* KeyProperty = MapProperty->KeyProp;
 			FProperty* ValueProperty = MapProperty->ValueProp;
 			check(KeyProperty != nullptr);
 			check(ValueProperty != nullptr);
 
 			return IsSupportedElementProperty(KeyProperty, InTypeValidation) && IsSupportedElementProperty(ValueProperty, InTypeValidation);
 		}
		else if (FDelegateProperty* DelegateProperty = CastField<FDelegateProperty>(InProperty))
		{
			check(DelegateProperty->SignatureFunction);

			return IsSupportedFunction(DelegateProperty->SignatureFunction, InTypeValidation);
		}
		else if (FMulticastDelegateProperty* MulticastDelegateProperty = CastField<FMulticastDelegateProperty>(InProperty))
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
			InProperty->IsA<FClassPtrProperty>() ||
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
		if (auto ArrayProperty = CastField<FArrayProperty>(InProperty))
		{
			AddDependNamespace(ArrayProperty->Inner);
		}
		else if (auto SetProperty = CastField<FSetProperty>(InProperty))
		{
			AddDependNamespace(SetProperty->ElementProp);
		}
		else if (auto MapProperty = CastField<FMapProperty>(InProperty))
		{
			AddDependNamespace(MapProperty->GetKeyProperty());
			AddDependNamespace(MapProperty->GetValueProperty());
		}
		else if (auto Field = FUnrealSharpUtils::GetPropertyInnerField(InProperty))
		{
			AddDependNamespace(Field);
		}
	}

	static inline FString ExtractNamespace(const FString& CSharpFullPath)
	{
		int32 LastDotIndex;
		if (CSharpFullPath.FindLastChar(TEXT('.'), LastDotIndex))
		{
			return CSharpFullPath.Left(LastDotIndex);
		}
		else
		{
			// No dot found, return the whole string
			return CSharpFullPath;
		}
	}

	void FStructTypeDefinition::AddDependNamespace(const UField* InField)
	{
		if (FUnrealSharpUtils::IsCSharpField(InField))
		{
			FString CSharpFullPath = FUnrealSharpUtils::GetCSharpFullPath(InField);

			auto DependNamespace = ExtractNamespace(CSharpFullPath);

			DependNamespaces.Add(DependNamespace, 0);
		}
	}
}