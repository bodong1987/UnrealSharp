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

class UCSharpStruct;
class UCSharpEnum;
class UCSharpClass;
class UCSharpBlueprint;
class ICSharpGeneratedType;
class UK2Node_EditablePinBase;
class UK2Node_FunctionResult;
class UK2Node_CustomEvent;
class UK2Node_FunctionEntry;
class UUserDefinedEnum;
class USimpleConstructionScript;

namespace UnrealSharp
{
	class FPropertyDefinition;
	class FBaseTypeDefinition;
	class FEnumTypeDefinition;
	class FFunctionTypeDefinition;
	class FScriptStructTypeDefinition;
	class FClassTypeDefinition;
	class FCSharpBlueprintGeneratorDatabase;
	struct FCSharpGeneratedTypeInfo;
		
	/*
	* Responsible for importing various C# types from TypeDefinitionDocument, 
	* and automatically generating corresponding blueprint classes, blueprint structures, 
	* and blueprint enumerations based on the type documents exported by C# assemblies.
	*/
	class UNREALSHARPEDITOR_API FCSharpBlueprintGeneratorUtils
	{
	public:
		// default folder of C# generated assets
		// default value: CSharpBlueprints
		static const FString		CSharpBlueprintClassPrefixPath;

		// generator version
		static const int			GeneratorVersion;

		// load exists object
		template <typename TObjectType>
		static TObjectType*			LoadObject(const FString& InPackageName)
		{
			TObjectType* ResultObject = ::LoadObject<TObjectType>(nullptr, *InPackageName, nullptr, LOAD_NoWarn | LOAD_NoRedirects);

			return ResultObject;
		}

		// get package path of type
		static FString				GetPackagePath(const TSharedPtr<FBaseTypeDefinition>& InType);

		// get package file path of type
		static FString				GetPackageFilePath(const TSharedPtr<FBaseTypeDefinition>& InType);

		// get package file path from package path
		static FString				GetPackageFilePath(const FString& InPackagePath);
		
		// get property pin category in blueprint
		static bool					GetPropertyPinCategory(const FPropertyDefinition& InPropertyDefinition, FName& OutPinCategory, FName& OutPinSubCategory);

		// get property terminal type
		static FEdGraphTerminalType GetPropertyTerminalType(const FPropertyDefinition& InPropertyDefinition, const FCSharpBlueprintGeneratorDatabase* InDatabase);

		// if need validate property 
		static bool					ShouldValidateFieldProperty(const FPropertyDefinition& InPropertyDefinition);

		// get EdGraphPinType of property
		static FEdGraphPinType		GetPropertyEdGraphPinType(const FPropertyDefinition& InPropertyDefinition, const FCSharpBlueprintGeneratorDatabase* InDatabase);

		// Clear an existing C# import enumeration. After clearing, you can re-import the data.
		static void					CleanCSharpEnum(UCSharpEnum* InEnum);

		// Clear an existing C# import struct. After clearing, you can re-import the data.
		static void					CleanCSharpStruct(UCSharpStruct* InStruct);

		// Clear an existing C# import class. After clearing, you can re-import the data.
		static void					CleanCSharpClass(UCSharpBlueprint* InBlueprint, UCSharpClass* InClass);

		// remove SimpleConstructionScript's node
		static void					RemoveSimpleConstructionScriptRecusivly(USimpleConstructionScript* InSimpleConstructionScript, USCS_Node* InNode);

		// add blueprint struct variable
		static bool					AddStructVariable(UCSharpStruct* InStruct, const FPropertyDefinition& InPropertyDefinition, const FCSharpBlueprintGeneratorDatabase* InDatabase);

		// get displayname from meta data
		static void					UpgradeDisplayNamesFromMetaData(UUserDefinedEnum* Enum);

		// create a new C# import enumeration
		static UCSharpEnum*			NewCSharpEnum(UPackage* InPackage, const FEnumTypeDefinition* InTypePtr);

		// create a new C# import struct
		static UCSharpStruct*		NewCSharpStruct(UPackage* InPackage, const FScriptStructTypeDefinition* InTypePtr);

		// create a new C# import class/Blueprint grpah
		static UCSharpBlueprint*	NewCSharpBlueprint(UPackage* InPackage, const FClassTypeDefinition* InTypePtr, UClass* InParentClass, const FCSharpBlueprintGeneratorDatabase* InDatabase);

		// force reset blueprint guid
		static void					ForceResetBlueprintGuid(UBlueprint* InBlueprint, const FGuid& InGuid);

		// add variable for C# import class/Blueprint graph
		static bool					AddClassVariable(UCSharpBlueprint* InBlueprint, UCSharpClass* InClass, const FPropertyDefinition& InPropertyDefinition, const FCSharpBlueprintGeneratorDatabase* InDatabase);

		// apply meta data
		static void					ApplyMetaData(FBPVariableDescription& InVariable, const FPropertyDefinition& InPropertyDefinition);

		// transfer base C# data
		// assembly name/full path/crccode...
		static void					PostGeneratedTypeConstructed(ICSharpGeneratedType* InType, const FBaseTypeDefinition* InTypeDefinition);

		// check if function implement as function 
		static bool					IsImplementationDesiredAsFunction(UBlueprint* InBlueprint, const UFunction* OverrideFunc);

		// add input pins for class function exit node
		static void					AddFunctionInputPropertyPins(UK2Node_EditablePinBase* InNode, FCSharpGeneratedTypeInfo* InInfo, const FFunctionTypeDefinition& InFunctionDefinition, const FCSharpBlueprintGeneratorDatabase* InDatabase);

		// add output pins for class function entry node
		static void					AddFunctionOutputPropertyPins(UK2Node_FunctionResult* InNode, FCSharpGeneratedTypeInfo* InInfo, const FFunctionTypeDefinition& InFunctionDefinition, const FCSharpBlueprintGeneratorDatabase* InDatabase);

		// apply function meta data
		static void					ApplyFunctionMetaData(UK2Node_FunctionEntry* InFunctionEntry, const FFunctionTypeDefinition& InFunctionDefinition);

		// apply custom event meta data
		static void					ApplyCustomEventMetaData(UK2Node_CustomEvent* InCustomEvent, const FFunctionTypeDefinition& InFunctionDefinition);
	};
}
