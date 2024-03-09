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
#include "SharpBindingGenSettings.h"

USharpBindingGenSettings::USharpBindingGenSettings(const FObjectInitializer& ObjectInitializer) :
	Super(ObjectInitializer)
{	
	// These classes are all handwritten on the C# side.
	BuiltinNames.Emplace(TEXT("UObject"));
	BuiltinNames.Emplace(TEXT("UInterface"));
	BuiltinNames.Emplace(TEXT("UClass"));

	// 
	ForceExportEmptyStructNames.Add("FInputActionValue");

	//
	FastAccessStructTypeNames.Add("FVector");
	FastAccessStructTypeNames.Add("FColor");
	FastAccessStructTypeNames.Add("FLinearColor");
	FastAccessStructTypeNames.Add("FGuid");
	FastAccessStructTypeNames.Add("FRotator");
	FastAccessStructTypeNames.Add("FGuid");
	FastAccessStructTypeNames.Add("FTimecode");
	FastAccessStructTypeNames.Add("FFrameNumber");
	FastAccessStructTypeNames.Add("FFrameRate");
	FastAccessStructTypeNames.Add("FInt32Interval");
	FastAccessStructTypeNames.Add("FFloatInterval");
	FastAccessStructTypeNames.Add("FPlatformUserId");
	FastAccessStructTypeNames.Add("FInputActionValue");

	FastFunctionInvokeModuleNames.Add(TEXT("Core"));
	FastFunctionInvokeModuleNames.Add(TEXT("CoreUObject"));
	FastFunctionInvokeModuleNames.Add(TEXT("Engine"));	
	FastFunctionInvokeModuleNames.Add(TEXT("InputCore"));
	FastFunctionInvokeModuleNames.Add(TEXT("InputDevice"));
	FastFunctionInvokeModuleNames.Add(TEXT("UnrealSharp"));
	FastFunctionInvokeModuleNames.Add(TEXT("UnrealSharpTests"));
	FastFunctionInvokeModuleNames.Add(FApp::GetProjectName());

	// ignore methods
	FastFunctionInvokeIgnoreNames.Add("UAnimMontage::IsValidAdditiveSlot");
	FastFunctionInvokeIgnoreNames.Add("APlanarReflection::OnInterpToggle");
	FastFunctionInvokeIgnoreNames.Add("ASceneCapture2D::OnInterpToggle");
	FastFunctionInvokeIgnoreNames.Add("UChaosBlueprintLibrary::GetEventRelayFromContext");	
	FastFunctionInvokeIgnoreNames.Add("UKismetSystemLibrary::StackTrace");
	FastFunctionInvokeIgnoreNames.Add("UMeshComponent::GetOverlayMaterialMaxDrawDistance");
	FastFunctionInvokeIgnoreNames.Add("UTexture::Blueprint_GetMemorySize");
	FastFunctionInvokeIgnoreNames.Add("UTexture::Blueprint_GetTextureSourceDiskAndMemorySize");
	FastFunctionInvokeIgnoreNames.Add("UTexture::ComputeTextureSourceChannelMinMax");
	FastFunctionInvokeIgnoreNames.Add("UTexture2D::Blueprint_GetSizeX");
	FastFunctionInvokeIgnoreNames.Add("UTexture2D::Blueprint_GetSizeY");
	FastFunctionInvokeIgnoreNames.Add("UMaterialInstanceConstant::K2_GetScalarParameterValue");
	FastFunctionInvokeIgnoreNames.Add("UMaterialInstanceConstant::K2_GetTextureParameterValue");
	FastFunctionInvokeIgnoreNames.Add("UMaterialInstanceConstant::K2_GetVectorParameterValue");
	FastFunctionInvokeIgnoreNames.Add("UMaterialParameterCollection::GetScalarParameterDefaultValue");
	FastFunctionInvokeIgnoreNames.Add("UMaterialParameterCollection::GetVectorParameterDefaultValue");
	FastFunctionInvokeIgnoreNames.Add("UMeshVertexPainterKismetLibrary::PaintVerticesSingleColor");
	FastFunctionInvokeIgnoreNames.Add("UMeshVertexPainterKismetLibrary::PaintVerticesLerpAlongAxis");
	FastFunctionInvokeIgnoreNames.Add("UMeshVertexPainterKismetLibrary::RemovePaintedVertices");
	FastFunctionInvokeIgnoreNames.Add("UParticleSystem::ContainsEmitterType");
	FastFunctionInvokeIgnoreNames.Add("UPhysicsObjectBlueprintLibrary::ApplyRadialImpulse");	
	FastFunctionInvokeIgnoreNames.Add("UWorldPartitionBlueprintLibrary::GetDataLayerManager");

	FastFunctionInvokeIgnoreClassNames.Add("UExporter");
	FastFunctionInvokeIgnoreClassNames.Add("UVisualLoggerKismetLibrary");
	FastFunctionInvokeIgnoreClassNames.Add("UPluginBlueprintLibrary");
	
	// 
	ExportModuleNames.Emplace(TEXT("Core"));
	ExportModuleNames.Emplace(TEXT("CoreUObject"));
	ExportModuleNames.Emplace(TEXT("Engine"));
	ExportModuleNames.Emplace(TEXT("Slate"));
	ExportModuleNames.Emplace(TEXT("SlateCore"));
	ExportModuleNames.Emplace(TEXT("RenderCore"));
	ExportModuleNames.Emplace(TEXT("RHI"));
	ExportModuleNames.Emplace(TEXT("ApplicationCore"));
	ExportModuleNames.Emplace(TEXT("UMG"));	
	ExportModuleNames.Emplace(TEXT("AIModule"));
	ExportModuleNames.Emplace(TEXT("GameplayTasks"));
	ExportModuleNames.Emplace(TEXT("NetCore"));
	ExportModuleNames.Emplace(TEXT("DeveloperSettings"));
	ExportModuleNames.Emplace(TEXT("Projects"));
	ExportModuleNames.Emplace(TEXT("InputCore"));
	ExportModuleNames.Emplace(TEXT("InputDevice"));
	ExportModuleNames.Emplace(TEXT("EnhancedInput"));
	ExportModuleNames.Emplace(TEXT("PhysicsCore"));
	ExportModuleNames.Emplace(TEXT("AdvancedWidgets"));
	ExportModuleNames.Emplace(TEXT("FieldNotification"));
	ExportModuleNames.Emplace(TEXT("TypedElementFramework"));
	ExportModuleNames.Emplace(TEXT("TypedElementRuntime"));

	ExportModuleNames.Emplace(TEXT("ChaosCore"));
	ExportModuleNames.Emplace(TEXT("Voronoi"));
	ExportModuleNames.Emplace(TEXT("GeometryCore"));
	ExportModuleNames.Emplace(TEXT("Chaos"));
	ExportModuleNames.Emplace(TEXT("ChaosSolverEngine"));

	ExportModuleNames.Emplace(TEXT("Navmesh"));
	ExportModuleNames.Emplace(TEXT("GeometryCollectionEngine"));
	ExportModuleNames.Emplace(TEXT("NavigationSystem"));

	ExportModuleNames.Emplace(TEXT("SignalProcessing"));
	ExportModuleNames.Emplace(TEXT("AudioMixerCore"));
	ExportModuleNames.Emplace(TEXT("AudioMixer"));	
	ExportModuleNames.Emplace(TEXT("AudioExtensions"));

	ExportModuleNames.Emplace(FApp::GetProjectName());

	// for tests...
	ExportModuleNames.Emplace(TEXT("UnrealSharpTests"));
}

bool USharpBindingGenSettings::IsNeedExportType(const FString& InName) const
{
	return !IgnoreExportTypeNames.Contains(InName) && !BuiltinNames.Contains(InName);
}

bool USharpBindingGenSettings::IsSupportedType(const FString& InName) const
{
	return BuiltinNames.Contains(InName) || !IgnoreExportTypeNames.Contains(InName);
}

bool USharpBindingGenSettings::IsIgnoreModuleName(const FString& InModuleName) const
{
	return !ExportModuleNames.Contains(InModuleName);
}

bool USharpBindingGenSettings::IsFastAccessStructType(const FString& InName) const
{
	return FastAccessStructTypeNames.Contains(InName);
}
