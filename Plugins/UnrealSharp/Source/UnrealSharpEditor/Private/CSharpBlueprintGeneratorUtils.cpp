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
#include "CSharpBlueprintGeneratorUtils.h"
#include "BaseTypeDefinition.h"
#include "UserDefinedStructure/UserDefinedStructEditorData.h"
#include "Kismet2/StructureEditorUtils.h"
#include "Kismet2/KismetEditorUtilities.h"
#include "Kismet2/BlueprintEditorUtils.h"
#include "Classes/CSharpStruct.h"
#include "Classes/CSharpEnum.h"
#include "Classes/CSharpBlueprint.h"
#include "Classes/CSharpClass.h"
#include "Engine/UserDefinedEnum.h"
#include "Engine/SimpleConstructionScript.h"
#include "Engine/SCS_Node.h"
#include "PropertyDefinition.h"
#include "CSharpBlueprintGeneratorDatabase.h"
#include "Misc/UnrealSharpLog.h"
#include "EnumTypeDefinition.h"
#include "ScriptStructTypeDefinition.h"
#include "ClassTypeDefinition.h"
#include "K2Node_FunctionEntry.h"
#include "K2Node_FunctionResult.h"
#include "K2Node_CustomEvent.h"

namespace UnrealSharp
{
    const FString FCSharpBlueprintGeneratorUtils::CSharpBlueprintClassPrefixPath = TEXT("CSharpBlueprints");
    const int FCSharpBlueprintGeneratorUtils::GeneratorVersion = 1;
  
    FString FCSharpBlueprintGeneratorUtils::GetPackagePath(const TSharedPtr<FBaseTypeDefinition>& InType)
    {
        FString Category;

        if (InType->GetDefinitionType() == EDefinitionType::Enum)
        {
            Category = TEXT("Enums");
        }
        else if (InType->GetDefinitionType() == EDefinitionType::Struct)
        {
            Category = TEXT("Structs");
        }
        else if (InType->GetDefinitionType() == EDefinitionType::Class)
        {
            Category = TEXT("Classes");
        }
        else if (InType->GetDefinitionType() == EDefinitionType::Function)
        {
            Category = TEXT("Functions");
        }

        FString TopPackageName = InType->PackageName;

        int Index;
        if (TopPackageName.FindChar(TEXT('/'), Index))
        {
            TopPackageName = TopPackageName.Left(Index);
        }

        FString PackageName = FString::Printf(TEXT("/Game/%s/%s/%s/%s"), *CSharpBlueprintClassPrefixPath, *TopPackageName, *Category, *InType->Name);

        return PackageName;
    }

    FString FCSharpBlueprintGeneratorUtils::GetPackageFilePath(const FString& InPackagePath)
    {
        FString PackageFileName;
        FPackageName::TryConvertLongPackageNameToFilename(InPackagePath, PackageFileName);
        PackageFileName += FPackageName::GetAssetPackageExtension();

        return PackageFileName;
    }

    FString FCSharpBlueprintGeneratorUtils::GetPackageFilePath(const TSharedPtr<FBaseTypeDefinition>& InType)
    {
        return GetPackageFilePath(GetPackagePath(InType));
    }

    void FCSharpBlueprintGeneratorUtils::CleanCSharpEnum(UCSharpEnum* InEnum)
    {
        check(InEnum);
        InEnum->ClearEnums();
    }

    void FCSharpBlueprintGeneratorUtils::CleanCSharpStruct(UCSharpStruct* InStruct)
    {
        check(InStruct);
        // make a copy first
        TArray<FStructVariableDescription>& AllFields = FStructureEditorUtils::GetVarDesc(InStruct);
        AllFields.Empty();
    }

    void FCSharpBlueprintGeneratorUtils::RemoveSimpleConstructionScriptRecursively(USimpleConstructionScript* InSimpleConstructionScript, USCS_Node* InNode)
    {
        check(InNode);

        auto Children = InNode->GetChildNodes();
        for (const auto Node : Children)
        {
            RemoveSimpleConstructionScriptRecursively(InSimpleConstructionScript, Node);
        }

        InSimpleConstructionScript->RemoveNode(InNode);
    }

    static void AddNeedRemovePagesWithFilter(TArray<UEdGraph*>& InNeedRemovePages, const TArray<TObjectPtr<UEdGraph>>& InTargetGraphs)
    {
        for (int i = InTargetGraphs.Num() - 1; i >= 0; --i)
        {
            UEdGraph* Graph = InTargetGraphs[i];
            FName GraphName = *Graph->GetName();

            if (GraphName != UEdGraphSchema_K2::GN_EventGraph && GraphName != UEdGraphSchema_K2::FN_UserConstructionScript)
            {
                InNeedRemovePages.Add(Graph);
            }
            else if (GraphName == UEdGraphSchema_K2::GN_EventGraph)
            {
                TArray<UK2Node_Event*> NeedRemoveNodes;
                Graph->GetNodesOfClass<UK2Node_Event>(NeedRemoveNodes);

                for (const auto& Node : NeedRemoveNodes)
                {
                    Graph->RemoveNode(Node);
                }
            }
        }
    }

    void FCSharpBlueprintGeneratorUtils::CleanCSharpClass(UCSharpBlueprint* InBlueprint, UCSharpClass* InClass)
    {
        check(InBlueprint);
        check(InClass);
        check(InBlueprint->GeneratedClass == InClass);

        // clear all normal Graph
        TArray<UEdGraph*> NeedRemovePages;

        // ubergraph pages
        AddNeedRemovePagesWithFilter(NeedRemovePages, InBlueprint->UbergraphPages);

        // clear all normal function graph
        AddNeedRemovePagesWithFilter(NeedRemovePages, InBlueprint->FunctionGraphs);

        // clear all delegate signature graph
        AddNeedRemovePagesWithFilter(NeedRemovePages, InBlueprint->DelegateSignatureGraphs);

        // event graph
        AddNeedRemovePagesWithFilter(NeedRemovePages, InBlueprint->EventGraphs);
        
        // macro graph
        AddNeedRemovePagesWithFilter(NeedRemovePages, InBlueprint->MacroGraphs);
        
        // IntermediateGeneratedGraphs
        AddNeedRemovePagesWithFilter(NeedRemovePages, InBlueprint->IntermediateGeneratedGraphs);

        FBlueprintEditorUtils::RemoveGraphs(InBlueprint, NeedRemovePages);

        NeedRemovePages.Empty();

        // clear all auto attach components node
        if (InBlueprint->SimpleConstructionScript != nullptr)
        {
            auto NeedRemoveNodes = InBlueprint->SimpleConstructionScript->GetAllNodes();
            for (auto* Node : NeedRemoveNodes)
            {
                RemoveSimpleConstructionScriptRecursively(InBlueprint->SimpleConstructionScript, Node);
            }
        }

        // clear all variables
        TArray<FName> NeedRemoveVariables;
        for (auto& Variable : InBlueprint->NewVariables)
        {
            NeedRemoveVariables.Add(Variable.VarName);
        }

        FBlueprintEditorUtils::BulkRemoveMemberVariables(InBlueprint, NeedRemoveVariables);

        InClass->ClearCSharpDataCaches();

        FBlueprintEditorUtils::MarkBlueprintAsModified(InBlueprint);
        FBlueprintEditorUtils::RefreshAllNodes(InBlueprint);
        FKismetEditorUtilities::CompileBlueprint(InBlueprint);
    }

    bool FCSharpBlueprintGeneratorUtils::GetPropertyPinCategory(const FPropertyDefinition& InPropertyDefinition, FName& OutPinCategory, FName& OutPinSubCategory)
    {
        if (InPropertyDefinition.IsBoolProperty())
        {
            OutPinCategory = UEdGraphSchema_K2::PC_Boolean;
        }
        else if (InPropertyDefinition.IsByteProperty())
        {
            OutPinCategory = UEdGraphSchema_K2::PC_Byte;
        }
        else if (InPropertyDefinition.IsIntProperty())
        {
            OutPinCategory = UEdGraphSchema_K2::PC_Int;
        }
        else if (InPropertyDefinition.IsInt64Property())
        {
            OutPinCategory = UEdGraphSchema_K2::PC_Int64;
        }
        else if (InPropertyDefinition.IsFloatProperty())
        {
            OutPinCategory = UEdGraphSchema_K2::PC_Real;
            OutPinSubCategory = UEdGraphSchema_K2::PC_Float;
        }
        else if (InPropertyDefinition.IsDoubleProperty())
        {
            OutPinCategory = UEdGraphSchema_K2::PC_Real;
            OutPinSubCategory = UEdGraphSchema_K2::PC_Double;
        }
        else if (InPropertyDefinition.IsStringProperty())
        {
            OutPinCategory = UEdGraphSchema_K2::PC_String;
        }
        else if (InPropertyDefinition.IsNameProperty())
        {
            OutPinCategory = UEdGraphSchema_K2::PC_Name;
        }
        else if (InPropertyDefinition.IsTextProperty())
        {
            OutPinCategory = UEdGraphSchema_K2::PC_Text;
        }
        else if (InPropertyDefinition.IsClassProperty())
        {
            OutPinCategory = UEdGraphSchema_K2::PC_Class;            
        }
        else if (InPropertyDefinition.IsStructProperty())
        {
            OutPinCategory = UEdGraphSchema_K2::PC_Struct;
        }
        else if (InPropertyDefinition.IsObjectProperty())
        {
            OutPinCategory = UEdGraphSchema_K2::PC_Object;
        }
        else if (InPropertyDefinition.IsEnumProperty())
        {
            // check EdGraphSchema_K2.cpp UEdGraphSchema_K2::DefaultValueSimpleValidation
            // UEdGraphSchema_K2::PC_Enum is not be supported.
            // OutPinCategory = UEdGraphSchema_K2::PC_Enum;
            OutPinCategory = UEdGraphSchema_K2::PC_Byte;
        }
        else if (InPropertyDefinition.IsArrayProperty() || 
                InPropertyDefinition.IsSetProperty() ||
                InPropertyDefinition.IsMapProperty()
                )
        {
            check(!InPropertyDefinition.InnerProperties.IsEmpty());
            GetPropertyPinCategory(*InPropertyDefinition.InnerProperties[0], OutPinCategory, OutPinSubCategory);
        }
        else if (InPropertyDefinition.IsSoftObjectProperty())
        {
            OutPinCategory = UEdGraphSchema_K2::PC_SoftObject;
        }
        else if (InPropertyDefinition.IsSoftClassProperty())
        {
            OutPinCategory = UEdGraphSchema_K2::PC_SoftClass;
        }

        return true;
    }

    bool FCSharpBlueprintGeneratorUtils::ShouldValidateFieldProperty(const FPropertyDefinition& InPropertyDefinition)
    {
        return InPropertyDefinition.IsClassProperty() ||
            InPropertyDefinition.IsObjectProperty() ||
            InPropertyDefinition.IsStructProperty() ||
            InPropertyDefinition.IsEnumProperty();
    }

    FEdGraphTerminalType FCSharpBlueprintGeneratorUtils::GetPropertyTerminalType(const FPropertyDefinition& InPropertyDefinition, const FCSharpBlueprintGeneratorDatabase* InDatabase)
    {
        FEdGraphTerminalType Terminal;

        GetPropertyPinCategory(InPropertyDefinition, Terminal.TerminalCategory, Terminal.TerminalSubCategory);

        Terminal.TerminalSubCategoryObject = InDatabase->GetField(InPropertyDefinition);

        if (ShouldValidateFieldProperty(InPropertyDefinition))
        {
            check(Terminal.TerminalSubCategoryObject.IsValid());
        }

        return Terminal;
    }

    FEdGraphPinType FCSharpBlueprintGeneratorUtils::GetPropertyEdGraphPinType(const FPropertyDefinition& InPropertyDefinition, const FCSharpBlueprintGeneratorDatabase* InDatabase)
    {
        FEdGraphPinType Pin;
        Pin.bIsReference = InPropertyDefinition.IsReference();
        Pin.bIsConst = InPropertyDefinition.IsConst();

        GetPropertyPinCategory(InPropertyDefinition, Pin.PinCategory, Pin.PinSubCategory);
               
        if (ShouldValidateFieldProperty(InPropertyDefinition))
        {   
            Pin.PinSubCategoryObject = InDatabase->GetField(InPropertyDefinition);

            check(Pin.PinSubCategoryObject.IsValid());
        }        
        else if (InPropertyDefinition.IsArrayProperty())
        {
            Pin.ContainerType = EPinContainerType::Array;
            
            check(InPropertyDefinition.InnerProperties.Num() == 1);
                        
            Pin.PinSubCategoryObject = InDatabase->GetField(*InPropertyDefinition.InnerProperties[0]);

            if (ShouldValidateFieldProperty(*InPropertyDefinition.InnerProperties[0]))
            {
                check(Pin.PinSubCategoryObject.IsValid());
            }
        }
        else if (InPropertyDefinition.IsSetProperty())
        {
            Pin.ContainerType = EPinContainerType::Set;

            check(InPropertyDefinition.InnerProperties.Num() == 1);

            Pin.PinSubCategoryObject = InDatabase->GetField(*InPropertyDefinition.InnerProperties[0]);

            if (ShouldValidateFieldProperty(*InPropertyDefinition.InnerProperties[0]))
            {
                check(Pin.PinSubCategoryObject.IsValid());
            }
        }
        else if (InPropertyDefinition.IsMapProperty())
        {
            Pin.ContainerType = EPinContainerType::Map;

            check(InPropertyDefinition.InnerProperties.Num() == 2);

            Pin.PinSubCategoryObject = InDatabase->GetField(*InPropertyDefinition.InnerProperties[0]);

            if (ShouldValidateFieldProperty(*InPropertyDefinition.InnerProperties[0]))
            {
                check(Pin.PinSubCategoryObject.IsValid());
            }

            Pin.PinValueType = GetPropertyTerminalType(*InPropertyDefinition.InnerProperties[1], InDatabase);
        }
        else if (InPropertyDefinition.IsSoftObjectProperty() || InPropertyDefinition.IsSoftClassProperty())
        {
            Pin.PinSubCategoryObject = InDatabase->GetField(*InPropertyDefinition.InnerProperties[0]);

            if (ShouldValidateFieldProperty(*InPropertyDefinition.InnerProperties[0]))
            {
                check(Pin.PinSubCategoryObject.IsValid());
            }
        }

        return Pin;
    }

    void FCSharpBlueprintGeneratorUtils::UpgradeDisplayNamesFromMetaData(UUserDefinedEnum* Enum)
    {
        if (Enum)
        {
            const int32 EnumeratorsToEnsure = FMath::Max(Enum->NumEnums() - 1, 0);
            Enum->DisplayNameMap.Empty(EnumeratorsToEnsure);

            for (int32 Index = 0; Index < EnumeratorsToEnsure; ++Index)
            {
                const FString& MetaDataEntryDisplayName = Enum->GetMetaData(TEXT("DisplayName"), Index);
                if (!MetaDataEntryDisplayName.IsEmpty())
                {
                    const FName EnumEntryName = *Enum->GetNameStringByIndex(Index);

                    FText DisplayNameToSet = FText::FromName(EnumEntryName);

                    Enum->DisplayNameMap.Add(EnumEntryName, DisplayNameToSet);
                }
            }
        }
    }

    // This code is copy from StructureEditorUtils.cpp 
    struct FMemberVariableNameHelper
    {
        static FName Generate(const UUserDefinedStruct* Struct, const FString& NameBase, const FGuid Guid, FString* OutFriendlyName = nullptr)
        {
            check(Struct);

            FString Result;
            if (!NameBase.IsEmpty())
            {
                if (!FName::IsValidXName(NameBase, INVALID_OBJECTNAME_CHARACTERS))
                {
                    Result = MakeObjectNameFromDisplayLabel(NameBase, NAME_None).GetPlainNameString();
                }
                else
                {
                    Result = NameBase;
                }
            }

            if (Result.IsEmpty())
            {
                Result = TEXT("MemberVar");
            }

            const uint32 UniqueNameId = CastChecked<UUserDefinedStructEditorData>(Struct->EditorData)->GenerateUniqueNameIdForMemberVariable();
            const FString FriendlyName = FString::Printf(TEXT("%s_%u"), *Result, UniqueNameId);
            if (OutFriendlyName)
            {
                *OutFriendlyName = FriendlyName;
            }
            const FName NameResult = *FString::Printf(TEXT("%s_%s"), *FriendlyName, *Guid.ToString(EGuidFormats::Digits));
            check(NameResult.IsValidXName(INVALID_OBJECTNAME_CHARACTERS));
            return NameResult;
        }

        static FGuid GetGuidFromName(const FName Name)
        {
            const FString NameStr = Name.ToString();
            constexpr int32 GuidStrLen = 32;
            if (NameStr.Len() > GuidStrLen + 1)
            {
                const int32 UnderscoreIndex = NameStr.Len() - GuidStrLen - 1;
                if (TEXT('_') == NameStr[UnderscoreIndex])
                {
                    const FString GuidStr = NameStr.Right(GuidStrLen);
                    FGuid Guid;
                    if (FGuid::ParseExact(GuidStr, EGuidFormats::Digits, Guid))
                    {
                        return Guid;
                    }
                }
            }
            return FGuid();
        }
    };


    bool FCSharpBlueprintGeneratorUtils::AddStructVariable(UCSharpStruct* InStruct, const FPropertyDefinition& InPropertyDefinition, const FCSharpBlueprintGeneratorDatabase* InDatabase)
    {
        const FEdGraphPinType VarType = GetPropertyEdGraphPinType(InPropertyDefinition, InDatabase);

        FString ErrorMessage;
        if (!FStructureEditorUtils::CanHaveAMemberVariableOfType(InStruct, VarType, &ErrorMessage))
        {
            US_LOG_ERROR(TEXT("%s"), *ErrorMessage);
            return false;
        }

        FString DisplayName;
        InPropertyDefinition.Metas.TryGetMeta(TEXT("DisplayName"), DisplayName);

        const FName VarName = FMemberVariableNameHelper::Generate(InStruct, InPropertyDefinition.Name, InPropertyDefinition.Guid);

        FStructVariableDescription NewVar;
        NewVar.VarName = VarName;
        NewVar.FriendlyName = DisplayName.IsEmpty() ? InPropertyDefinition.Name : DisplayName;
        NewVar.SetPinType(VarType);
        NewVar.VarGuid = InPropertyDefinition.Guid;

        if (!InPropertyDefinition.DefaultValue.IsEmpty())
        {
            NewVar.DefaultValue = InPropertyDefinition.DefaultValue;
        }        

        FString Tooltip;

        if (InPropertyDefinition.Metas.TryGetMeta(TEXT("ToolTip"), Tooltip))
        {
            NewVar.ToolTip = Tooltip;
        }

        FStructureEditorUtils::GetVarDesc(InStruct).Add(NewVar);

        return true;
    }

    UCSharpEnum* FCSharpBlueprintGeneratorUtils::NewCSharpEnum(UPackage* InPackage, const FEnumTypeDefinition* InTypePtr)
    {
        UCSharpEnum* Enum = NewObject<UCSharpEnum>(InPackage, *InTypePtr->Name, RF_Public | RF_Standalone | RF_Transactional);

        check(Enum);

        Enum->SetMetaData(TEXT("Blueprint"), TEXT("true"));
        
        PostGeneratedTypeConstructed(Enum, InTypePtr);

        return Enum;
    }

    UCSharpStruct* FCSharpBlueprintGeneratorUtils::NewCSharpStruct(UPackage* InPackage, const FScriptStructTypeDefinition* InTypePtr)
    {
        UCSharpStruct* Struct = NewObject<UCSharpStruct>(InPackage, *InTypePtr->Name, RF_Public | RF_Standalone | RF_Transactional);
        check(Struct);

        Struct->EditorData = NewObject<UUserDefinedStructEditorData>(Struct, NAME_None, RF_Transactional);
        check(Struct->EditorData);

        Struct->Guid = InTypePtr->Guid;
        Struct->SetMetaData(TEXT("BlueprintType"), TEXT("true"));
        Struct->Bind();
        Struct->StaticLink(true);
        Struct->Status = UDSS_Error;
        
        PostGeneratedTypeConstructed(Struct, InTypePtr);

        return Struct;
    }

    void FCSharpBlueprintGeneratorUtils::ForceResetBlueprintGuid(UBlueprint* InBlueprint, const FGuid& InGuid)
    {
        const FProperty* BlueprintGuidProperty = InBlueprint->GetClass()->FindPropertyByName(TEXT("BlueprintGuid"));
        check(BlueprintGuidProperty);

        BlueprintGuidProperty->ImportText_Direct(*InGuid.ToString(), BlueprintGuidProperty->ContainerPtrToValuePtr<void>(InBlueprint), InBlueprint, PPF_None);
    }

    UCSharpBlueprint* FCSharpBlueprintGeneratorUtils::NewCSharpBlueprint(UPackage* InPackage, const FClassTypeDefinition* InTypePtr, UClass* InParentClass, const FCSharpBlueprintGeneratorDatabase* InDatabase)
    {
        US_UNREFERENCED_PARAMETER(InDatabase);
        
        const EBlueprintType BaseType = InTypePtr->SuperName == TEXT("UBlueprintFunctionLibrary") ? BPTYPE_FunctionLibrary : BPTYPE_Normal;

        const FString GeneratedClassNameString = FString::Printf(TEXT("%s_C"), *InTypePtr->Name);
        const FName GeneratedClassName = FName(*GeneratedClassNameString);
        // generate a class here
        UCSharpClass* NewClass = NewObject<UCSharpClass>(InPackage, GeneratedClassName, RF_Public | RF_Transactional); // NOLINT
        
        UCSharpBlueprint* Blueprint = Cast<UCSharpBlueprint>(FKismetEditorUtilities::CreateBlueprint(
            InParentClass, 
            InPackage, 
            *InTypePtr->Name, 
            BaseType, 
            UCSharpBlueprint::StaticClass(),
            UCSharpClass::StaticClass(),
            FName("CSharpBlueprintGenerator")
            )
        );

        check(Blueprint);

        ForceResetBlueprintGuid(Blueprint, InTypePtr->Guid);

        check(Blueprint->GeneratedClass == NewClass);

        UCSharpClass* GeneratedClass = Cast<UCSharpClass>(Blueprint->GeneratedClass);
        check(GeneratedClass);

        PostGeneratedTypeConstructed(GeneratedClass, InTypePtr);

        return Blueprint;
    }

    void FCSharpBlueprintGeneratorUtils::PostGeneratedTypeConstructed(ICSharpGeneratedType* InType, const FBaseTypeDefinition* InTypeDefinition)
    {
        checkSlow(InType);
        checkSlow(InTypeDefinition);

        InType->SetCrcCode(InTypeDefinition->CrcCode);
        InType->SetGeneratorVersion(GeneratorVersion);
        InType->SetCSharpFullName(InTypeDefinition->CSharpFullName);
        InType->SetAssemblyName(InTypeDefinition->AssemblyName);
    }

    bool FCSharpBlueprintGeneratorUtils::AddClassVariable(UCSharpBlueprint* InBlueprint, const UCSharpClass* InClass, const FPropertyDefinition& InPropertyDefinition, const FCSharpBlueprintGeneratorDatabase* InDatabase)
    {
        US_UNREFERENCED_PARAMETER(InClass);
        
        const FEdGraphPinType PinType = GetPropertyEdGraphPinType(InPropertyDefinition, InDatabase);

        const bool bResult = FBlueprintEditorUtils::AddMemberVariable(InBlueprint, *InPropertyDefinition.Name, PinType, InPropertyDefinition.DefaultValue);

        if (bResult)
        {
            FBPVariableDescription* VariablePtr = InBlueprint->NewVariables.FindByPredicate([&](const FBPVariableDescription& InVariable)
                {
                    return InVariable.VarName == *InPropertyDefinition.Name;
                }
            );

            check(VariablePtr != nullptr);

            VariablePtr->VarGuid = InPropertyDefinition.Guid;

            ApplyMetaData(*VariablePtr, InPropertyDefinition);
        }

        return bResult;
    }

    void FCSharpBlueprintGeneratorUtils::ApplyMetaData(FBPVariableDescription& InVariable, const FPropertyDefinition& InPropertyDefinition)
    {
        InVariable.PropertyFlags |= InPropertyDefinition.PropertyFlags;

        FString Result;
        if (InPropertyDefinition.Metas.TryGetMeta(TEXT("Category"), Result))
        {
            InVariable.Category = FText::FromString(Result);
        }

        InVariable.DefaultValue = InPropertyDefinition.DefaultValue;

        if (!InPropertyDefinition.ReplicatedUsing.IsEmpty())
        {
            InVariable.RepNotifyFunc = *InPropertyDefinition.ReplicatedUsing;
            InVariable.ReplicationCondition = InPropertyDefinition.ReplicationCondition;
        }        

        for (auto& Pair : InPropertyDefinition.Metas.Metas)
        {
            InVariable.SetMetaData(*Pair.Key, Pair.Value);
        }
    }

    bool FCSharpBlueprintGeneratorUtils::IsImplementationDesiredAsFunction(const UBlueprint* InBlueprint, const UFunction* OverrideFunc)
    {
        // If the original function was created in a parent blueprint, then prefer a BP function
        if (OverrideFunc)
        {
            const FName OverrideName = *OverrideFunc->GetName();
            TSet<FName> GraphNames;
            FBlueprintEditorUtils::GetAllGraphNames(InBlueprint, GraphNames);
            for (const FName& Name : GraphNames)
            {
                if (Name == OverrideName)
                {
                    return true;
                }
            }
        }

        // Otherwise, we would prefer an event
        return false;
    }

    void FCSharpBlueprintGeneratorUtils::AddFunctionInputPropertyPins(UK2Node_EditablePinBase* InNode, const FCSharpGeneratedTypeInfo* InInfo, const FFunctionTypeDefinition& InFunctionDefinition, const FCSharpBlueprintGeneratorDatabase* InDatabase)
    {
        US_UNREFERENCED_PARAMETER(InInfo);
        
        for (auto& Property : InFunctionDefinition.Properties)
        {
            if (!Property.IsInputProperty())
            {
                continue;
            }

            FEdGraphPinType PinType = GetPropertyEdGraphPinType(Property, InDatabase);

            FText OutMessage;
            
            verifyf(InNode->CanCreateUserDefinedPin(PinType, EGPD_Output, OutMessage), TEXT("Can't supported type:%s"), *OutMessage.ToString());

            InNode->CreateUserDefinedPin(*Property.Name, PinType, EGPD_Output, false);
        }
    }

    void FCSharpBlueprintGeneratorUtils::AddFunctionOutputPropertyPins(UK2Node_FunctionResult* InNode, const FCSharpGeneratedTypeInfo* InInfo, const FFunctionTypeDefinition& InFunctionDefinition, const FCSharpBlueprintGeneratorDatabase* InDatabase)
    {
        US_UNREFERENCED_PARAMETER(InInfo);
        
        for (auto& Property : InFunctionDefinition.Properties)
        {
            if (Property.IsInputProperty())
            {
                continue;
            }

            FEdGraphPinType PinType = GetPropertyEdGraphPinType(Property, InDatabase);

            FText OutMessage;

            verifyf(InNode->CanCreateUserDefinedPin(PinType, EGPD_Input, OutMessage), TEXT("Can't supported type:%s"), *OutMessage.ToString());

            InNode->CreateUserDefinedPin(*Property.Name, PinType, EGPD_Input, false);
        }
    }

    void FCSharpBlueprintGeneratorUtils::ApplyFunctionMetaData(UK2Node_FunctionEntry* InFunctionEntry, const FFunctionTypeDefinition& InFunctionDefinition)
    {
        const auto OldFlag = InFunctionEntry->GetExtraFlags();

        InFunctionEntry->SetExtraFlags(OldFlag|static_cast<uint32>(InFunctionDefinition.Flags));
        
        for (auto& Pair : InFunctionDefinition.Meta.Metas)
        {
            InFunctionEntry->MetaData.SetMetaData(*Pair.Key, Pair.Value);
        }        
    }

    void FCSharpBlueprintGeneratorUtils::ApplyCustomEventMetaData(UK2Node_CustomEvent* InCustomEvent, const FFunctionTypeDefinition& InFunctionDefinition)
    {
        InCustomEvent->FunctionFlags |= static_cast<uint32>(InFunctionDefinition.Flags);

        for (auto& Pair : InFunctionDefinition.Meta.Metas)
        {
            InCustomEvent->GetUserDefinedMetaData().SetMetaData(*Pair.Key, Pair.Value);
        }
    }
}

