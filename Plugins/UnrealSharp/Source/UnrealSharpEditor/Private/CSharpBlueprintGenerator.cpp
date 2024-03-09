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
#include "CSharpBlueprintGenerator.h"
#include "TypeDefinitionDocument.h"
#include "ClassTypeDefinition.h"
#include "ScriptStructTypeDefinition.h"
#include "EnumTypeDefinition.h"
#include "Misc/UnrealSharpLog.h"
#include "Classes/CSharpBlueprint.h"
#include "Classes/CSharpEnum.h"
#include "Classes/CSharpStruct.h"
#include "Classes/CSharpClass.h"
#include "AssetRegistry/AssetRegistryModule.h"
#include "UObject/SavePackage.h"
#include "Kismet2/EnumEditorUtils.h"
#include "Kismet2/BlueprintEditorUtils.h"
#include "Kismet2/KismetEditorUtilities.h"
#include "UserDefinedStructure/UserDefinedStructEditorData.h"
#include "CSharpBlueprintGeneratorUtils.h"
#include "CSharpBlueprintGeneratorDatabase.h"
#include "EdGraphSchema_K2_Actions.h"
#include "K2Node_Event.h"
#include "K2Node_CustomEvent.h"
#include "K2Node_FunctionEntry.h"
#include "K2Node_FunctionResult.h"
#include "Engine/SimpleConstructionScript.h"
#include "Engine/SCS_Node.h"
#include "Misc/UnrealSharpUtils.h"

namespace UnrealSharp
{
    FCSharpBlueprintGenerator::FCSharpBlueprintGenerator(TSharedPtr<FTypeDefinitionDocument> InDocument) :
        Document(InDocument),
        Database(MakeUnique<FCSharpBlueprintGeneratorDatabase>(InDocument))
    {
    }

    FCSharpBlueprintGenerator::~FCSharpBlueprintGenerator()
    {
    }

    void FCSharpBlueprintGenerator::Process()
    {
        // process enum first
        Database->Accept([US_LAMBDA_CAPTURE_THIS](FCSharpGeneratedTypeInfo& InInfo)
        {
            if (InInfo.State != ECSharpGeneratedTypeState::Completed && Cast<UCSharpEnum>(InInfo.Field) != nullptr)
            {
                if (ProcessEnum(&InInfo))
                {
                    InInfo.State = ECSharpGeneratedTypeState::Completed;
                }
            }
        });

        // process struct
        Database->Accept([US_LAMBDA_CAPTURE_THIS](FCSharpGeneratedTypeInfo& InInfo)
        {
            if (InInfo.State != ECSharpGeneratedTypeState::Completed && Cast<UCSharpStruct>(InInfo.Field) != nullptr)
            {
                ProcessStruct(&InInfo);
            }
        });

        // process class
        Database->Accept([US_LAMBDA_CAPTURE_THIS](FCSharpGeneratedTypeInfo& InInfo)
        {
            if (InInfo.State != ECSharpGeneratedTypeState::Completed && Cast<UCSharpClass>(InInfo.Field) != nullptr)
            {
                ProcessClass(&InInfo);
            }
        });
    }

    bool FCSharpBlueprintGenerator::ProcessEnum(FCSharpGeneratedTypeInfo* InInfo)
    {   
        check(InInfo);
        check(InInfo->Field);

        US_LOG(TEXT("Process C# Enum : %s"), *InInfo->Definition->CSharpFullName);

        UPackage* Package = InInfo->Field->GetOutermost();
        UCSharpEnum* Enum = Cast<UCSharpEnum>(InInfo->Field);
        TSharedPtr<FEnumTypeDefinition> EnumType = StaticCastSharedPtr<FEnumTypeDefinition>(InInfo->Definition);
        check(Enum);

        Enum->ClearEnums();

        TArray<TPair<FName, int64>> Fields;

        for (auto& field : EnumType->Fields)
        {
            Fields.Emplace(FString::Printf(TEXT("%s::%s"), *EnumType->Name, *field.Name), (int64)field.Value);
        }

        Enum->SetEnums(Fields, UEnum::ECppForm::Namespaced);

        for (auto& meta : EnumType->Meta.Metas)
        {
            Enum->SetMetaData(*meta.Key, *meta.Value);

            if (meta.Key == TEXT("ToolTip"))
            {
                // force set enum description.
                FProperty* Property = Enum->GetClass()->FindPropertyByName(TEXT("EnumDescription"));
                if (Property != nullptr)
                {
                    Property->ImportText_Direct(*meta.Value, Property->ContainerPtrToValuePtr<void>(Enum, 0), nullptr, PPF_None);
                }
            }
        }

        FEnumEditorUtils::EnsureAllDisplayNamesExist(Enum);

        FAssetRegistryModule::AssetCreated(Enum);

        FSavePackageArgs Args;
        Args.TopLevelFlags = EObjectFlags::RF_Public | EObjectFlags::RF_Standalone;

        UPackage::SavePackage(Package, Enum, *InInfo->FilePath, Args);

        return true;
    }

    bool FCSharpBlueprintGenerator::ProcessStruct(FCSharpGeneratedTypeInfo* InInfo)
    {
        check(InInfo);
        check(InInfo->Field);
        check(InInfo->Definition);

        US_LOG(TEXT("Process C# Struct : %s"), *InInfo->Definition->CSharpFullName);

        UPackage* Package = InInfo->Field->GetOutermost();
        UCSharpStruct* Struct = Cast<UCSharpStruct>(InInfo->Field);
        TSharedPtr<FScriptStructTypeDefinition> StructType = StaticCastSharedPtr<FScriptStructTypeDefinition>(InInfo->Definition);
        check(Struct);
                
        Struct->Guid = InInfo->Definition->Guid;

        FCSharpBlueprintGeneratorUtils::CleanCSharpStruct(Struct);

        for (auto& field : StructType->Properties)
        {
            if (!FCSharpBlueprintGeneratorUtils::AddStructVariable(Struct, field, Database.Get()))
            {
                US_LOG_ERROR(TEXT("Failed add struct variable %s:%s"), *InInfo->Definition->CSharpFullName, *field.Name);
                return false;
            }
        }

        // compile it
        FStructureEditorUtils::CompileStructure(Struct);

        FAssetRegistryModule::AssetCreated(Struct);

        FSavePackageArgs Args;
        Args.TopLevelFlags = EObjectFlags::RF_Public | EObjectFlags::RF_Standalone;

        UPackage::SavePackage(Package, Struct, *InInfo->FilePath, Args);

        return true;
    }

    bool FCSharpBlueprintGenerator::ProcessClass(FCSharpGeneratedTypeInfo* InInfo)
    {
        check(InInfo->Blueprint && InInfo->Field);

        US_LOG(TEXT("Process C# Class : %s"), *InInfo->Definition->CSharpFullName);
                
        UPackage* Package = InInfo->Field->GetOutermost();

        UCSharpClass* Class = Cast<UCSharpClass>(InInfo->Field);
        check(Class);

        FCSharpBlueprintGeneratorUtils::CleanCSharpClass(InInfo->Blueprint, Class);

        const bool bIsChildOfActor = Class->IsChildOf<AActor>();

        TSharedPtr<FClassTypeDefinition> ClassType = StaticCastSharedPtr<FClassTypeDefinition>(InInfo->Definition);

        TSet<FName> ProcessedAttachComponents;

        for (auto& property : ClassType->Properties)
        {
            if (property.IsDelegateRelatedProperty())
            {
                if (!ProcessDelegate(InInfo, property))
                {
                    US_LOG_ERROR(TEXT("Failed process delegate %s.%s"), *InInfo->CppName, *property.Name);

                    return false;
                }                
            }
            else if (bIsChildOfActor && property.IsAttachToActorProperty())
            {
                // this should be a component in Actor                
                if (!ProcessAutoAttachComponent(InInfo, ClassType.Get(), property, ProcessedAttachComponents))
                {
                    US_LOG_ERROR(TEXT("Failed process auto attach component %s.%s"), *InInfo->CppName, *property.Name);

                    return false;
                }
            }
            else
            {
                FCSharpBlueprintGeneratorUtils::AddClassVariable(InInfo->Blueprint, Class, property, Database.Get());
            }
        }

        for (auto& function : ClassType->Functions)
        {
            if (!ProcessFunction(InInfo, function))
            {
                US_LOG_ERROR(TEXT("Failed process function %s"), *function.CSharpFullName);
                return false;
            }
        }

        FAssetRegistryModule::AssetCreated(InInfo->Blueprint);

        FBlueprintEditorUtils::MarkBlueprintAsModified(InInfo->Blueprint);
        FKismetEditorUtilities::CompileBlueprint(InInfo->Blueprint);

        FSavePackageArgs Args;
        Args.TopLevelFlags = EObjectFlags::RF_Public | EObjectFlags::RF_Standalone;

        UPackage::SavePackage(Package, InInfo->Blueprint, *InInfo->FilePath, Args);
        
        return true;
    }

    inline FCSharpFunctionData BuildCSharpFunctionData(const FFunctionTypeDefinition& InFunctionDefinition)
    {
        FCSharpFunctionData Data;
        Data.FunctionName = InFunctionDefinition.Name;
        Data.FunctionSignature = InFunctionDefinition.Signature;

        for (auto& property : InFunctionDefinition.Properties)
        {
            FCSharpFunctionArgumentData ArgumentData = {*property.Name, property.PropertyFlags, property.Size};

            Data.Arguments.Emplace(ArgumentData);
        }

        return Data;
    }

    bool FCSharpBlueprintGenerator::ProcessFunction(FCSharpGeneratedTypeInfo* InInfo, const FFunctionTypeDefinition& InFunctionDefinition)
    {
        check(InInfo && InInfo->Blueprint);

        UCSharpClass* Class = Cast<UCSharpClass>(InInfo->Field);
        check(Class);

        Class->AddCSharpFunction(*InFunctionDefinition.Name, BuildCSharpFunctionData(InFunctionDefinition));

        UFunction* OverrideFunc = nullptr;
        UClass* const OverrideFuncClass = FBlueprintEditorUtils::GetOverrideFunctionClass(InInfo->Blueprint, *InFunctionDefinition.Name, &OverrideFunc);

        UEdGraph* EventGraph = FBlueprintEditorUtils::FindEventGraph(InInfo->Blueprint);
        
        bool bCanImplementAsEvent = false;

        if (OverrideFunc != nullptr)
        {
            bCanImplementAsEvent = UEdGraphSchema_K2::FunctionCanBePlacedAsEvent(OverrideFunc) &&
                !FCSharpBlueprintGeneratorUtils::IsImplementationDesiredAsFunction(InInfo->Blueprint, OverrideFunc) &&
                EventGraph;
        }
        else
        {
            bCanImplementAsEvent = InFunctionDefinition.IsExportAsEvent();
        }

        // should be event
        if (bCanImplementAsEvent && 
            InInfo->Blueprint->BlueprintType != BPTYPE_FunctionLibrary
            )
        {            
            checkf(EventGraph, TEXT("Implement as event need EventGraph exsits."));

            checkf(!InFunctionDefinition.HasAnyOutParameter(), TEXT("event can't have any out parameter or return type : %s"), *InFunctionDefinition.CSharpFullName);

            FName EventName = *InFunctionDefinition.Name;
            UK2Node_Event* ExistingNode = FBlueprintEditorUtils::FindOverrideForFunction(InInfo->Blueprint, OverrideFuncClass, EventName);

            check(!ExistingNode);

            if (OverrideFuncClass && OverrideFunc)
            {
                UK2Node_Event* NewEventNode = FEdGraphSchemaAction_K2NewNode::SpawnNode<UK2Node_Event>(
                    EventGraph,
                    EventGraph->GetGoodPlaceForNewNode(),
                    EK2NewNodeFlags::SelectNewNode,
                    [EventName, OverrideFuncClass, OverrideFunc](UK2Node_Event* NewInstance)
                    {
                        NewInstance->EventReference.SetExternalMember(EventName, OverrideFuncClass);
                        NewInstance->bOverrideFunction = true;
                    }
                );
            }
            else
            {
                UK2Node_CustomEvent* EventNode = FEdGraphSchemaAction_K2NewNode::SpawnNode<UK2Node_CustomEvent>(EventGraph,
                    EventGraph->GetGoodPlaceForNewNode(), EK2NewNodeFlags::SelectNewNode,
                    [&InFunctionDefinition](UK2Node_Event* NewInstance)
                    {
                        NewInstance->CustomFunctionName = *InFunctionDefinition.Name;
                        NewInstance->bIsEditable = true;
                    });

                FCSharpBlueprintGeneratorUtils::ApplyCustomEventMetaData(EventNode, InFunctionDefinition);
                FCSharpBlueprintGeneratorUtils::AddFunctionInputPropertyPins(EventNode, InInfo, InFunctionDefinition, Database.Get());
            }
        }
        else
        {
            // used as function
            UEdGraph* FunctionGraph = FBlueprintEditorUtils::CreateNewGraph(InInfo->Blueprint, *InFunctionDefinition.Name, UEdGraph::StaticClass(), UEdGraphSchema_K2::StaticClass());
            check(FunctionGraph);

            FBlueprintEditorUtils::AddFunctionGraph<UClass>(InInfo->Blueprint, FunctionGraph, OverrideFuncClass == nullptr, OverrideFuncClass);

            if (OverrideFuncClass == nullptr)
            {
                UK2Node_FunctionEntry* FunctionNode = Cast<UK2Node_FunctionEntry>(FBlueprintEditorUtils::GetEntryNode(FunctionGraph));
                check(FunctionNode);

                FCSharpBlueprintGeneratorUtils::ApplyFunctionMetaData(FunctionNode, InFunctionDefinition);

                FCSharpBlueprintGeneratorUtils::AddFunctionInputPropertyPins(FunctionNode, InInfo, InFunctionDefinition, Database.Get());

                if (InFunctionDefinition.HasAnyOutParameter())
                {
                    UK2Node_FunctionResult* FunctionResult = FBlueprintEditorUtils::FindOrCreateFunctionResultNode(FunctionNode);
                    check(FunctionResult);

                    FCSharpBlueprintGeneratorUtils::AddFunctionOutputPropertyPins(FunctionResult, InInfo, InFunctionDefinition, Database.Get());
                }
            }            
        }

        return true;
    }

    bool FCSharpBlueprintGenerator::ProcessDelegate(FCSharpGeneratedTypeInfo* InInfo, const FPropertyDefinition& InPropertyDefinition)
    {
        check(InPropertyDefinition.SignatureFunction);

        FEdGraphPinType DelegateType;
        DelegateType.PinCategory = InPropertyDefinition.IsDelegateProperty() ? UEdGraphSchema_K2::PC_Delegate : UEdGraphSchema_K2::PC_MCDelegate;
        const bool bVarCreatedSuccess = FBlueprintEditorUtils::AddMemberVariable(InInfo->Blueprint, *InPropertyDefinition.Name, DelegateType);
        checkf(bVarCreatedSuccess, TEXT("Failed create delegate variable:%s"), *InPropertyDefinition.Name);

        UEdGraph* NewGraph = FBlueprintEditorUtils::CreateNewGraph(InInfo->Blueprint, *InPropertyDefinition.Name, UEdGraph::StaticClass(), UEdGraphSchema_K2::StaticClass());
        checkf(NewGraph, TEXT("Failed create delegate graph:%s"), *InPropertyDefinition.Name);

        NewGraph->bEditable = false;

        const UEdGraphSchema_K2* K2Schema = GetDefault<UEdGraphSchema_K2>();
        check(nullptr != K2Schema);

        K2Schema->CreateDefaultNodesForGraph(*NewGraph);
        K2Schema->CreateFunctionGraphTerminators(*NewGraph, (UClass*)nullptr);
        K2Schema->AddExtraFunctionFlags(NewGraph, (FUNC_BlueprintCallable | FUNC_BlueprintEvent | FUNC_Public));
        K2Schema->MarkFunctionEntryAsEditable(NewGraph, true);

        InInfo->Blueprint->DelegateSignatureGraphs.Add(NewGraph);

        auto EntryNode = FBlueprintEditorUtils::GetEntryNode(NewGraph);
        check(EntryNode);

        FCSharpBlueprintGeneratorUtils::AddFunctionInputPropertyPins(EntryNode, InInfo, *InPropertyDefinition.SignatureFunction, Database.Get());

        return true;
    }

    bool FCSharpBlueprintGenerator::ProcessAutoAttachComponent(FCSharpGeneratedTypeInfo* InInfo, const FClassTypeDefinition* InClassTypeDefinition, const FPropertyDefinition& InPropertyDefinition, TSet<FName>& InProcessedNames)
    {
        if (!InPropertyDefinition.IsAttachToActorProperty())
        {
            US_LOG_ERROR(TEXT("Missing auto attach target name."));
            return false;
        }

        if (InProcessedNames.Contains(*InPropertyDefinition.Name))
        {
            return true;
        }

        bool bIsParentComponentNative = false;
        FString ParentClassName = *InClassTypeDefinition->SuperName;

        if (!InPropertyDefinition.AttachToComponentName.IsEmpty() && !InProcessedNames.Contains(*InPropertyDefinition.AttachToComponentName))
        {
            const FPropertyDefinition* TargetDefinition = InClassTypeDefinition->GetPropertyDefinition(InPropertyDefinition.AttachToComponentName);

            if (TargetDefinition != nullptr)
            {
                // process attach first
                if (!ProcessAutoAttachComponent(InInfo, InClassTypeDefinition, *TargetDefinition, InProcessedNames))
                {
                    return false;
                }
            }
            else
            {
                UField* ParentClassField = Database->GetField(ParentClassName);

                bIsParentComponentNative = ParentClassField != nullptr && FUnrealSharpUtils::IsNativeField(ParentClassField);
                ParentClassName = ParentClassField != nullptr ? ParentClassField->GetName() : *FString::Printf(TEXT("%s_C"), *ParentClassName.Right(ParentClassName.Len() - 1));
            }
        }

        // process current component now
        UBlueprint* Blueprint = InInfo->Blueprint;
        check(Blueprint);
        check(Blueprint->SimpleConstructionScript);

        UField* PropertyFieldType = Database->GetField(InPropertyDefinition.CppTypeName);
        check(PropertyFieldType != nullptr);

        UClass* PropertyClassType = Cast<UClass>(PropertyFieldType);
        checkf(PropertyClassType, TEXT("%s is not an valid class type."), *InPropertyDefinition.CppTypeName);

        auto ExistsNode = Blueprint->SimpleConstructionScript->FindSCSNode(*InPropertyDefinition.Name);
        if (ExistsNode != nullptr)
        {
            Blueprint->SimpleConstructionScript->RemoveNode(ExistsNode);
            ExistsNode = nullptr;
        }

        USCS_Node* NewNode = Blueprint->SimpleConstructionScript->CreateNode(PropertyClassType, *InPropertyDefinition.Name);
        NewNode->VariableGuid = InPropertyDefinition.Guid;
        NewNode->ParentComponentOrVariableName = *InPropertyDefinition.AttachToComponentName;
        NewNode->AttachToName = *InPropertyDefinition.AttachToSocketName;
        NewNode->bIsParentComponentNative = bIsParentComponentNative;
        NewNode->ParentComponentOwnerClassName = *ParentClassName;

        FString CategoryName;
        
        if (InPropertyDefinition.Metas.TryGetMeta(TEXT("Category"), CategoryName))
        {
            NewNode->CategoryName = FText::FromString(CategoryName);
        }

        Blueprint->SimpleConstructionScript->AddNode(NewNode);

        auto* ParentNode = InPropertyDefinition.AttachToComponentName.IsEmpty() ? 
            Blueprint->SimpleConstructionScript->GetDefaultSceneRootNode() : 
            Blueprint->SimpleConstructionScript->FindSCSNode(*InPropertyDefinition.AttachToComponentName);

        if (ParentNode != nullptr)
        {
            ParentNode->AddChildNode(NewNode, false);            
        }
        else
        {
            US_LOG_WARN(TEXT("Failed find attach target node:%s"), *InPropertyDefinition.AttachToComponentName);
        }        

        InProcessedNames.Add(*InPropertyDefinition.Name);

        return true;
    }
}