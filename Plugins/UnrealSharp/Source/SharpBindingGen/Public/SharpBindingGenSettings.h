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

#include "CoreMinimal.h"
#include "Engine/DeveloperSettings.h"
#include "SharpBindingGenSettings.generated.h"

/**
 * Settings for SharpBinding Exporter
 * There are other runtime-related configurations in the UUnrealSharpSettings configuration. 
 */
UCLASS(config = UnrealSharp, defaultconfig, meta = (DisplayName = "UnrealSharpBindingGen"))
class SHARPBINDINGGEN_API USharpBindingGenSettings : public UDeveloperSettings
{
    GENERATED_UCLASS_BODY()
public:
    bool IsIgnoreModuleName(const FString& InModuleName) const;
    bool IsSupportedType(const FString& InName) const;
    bool IsNeedExportType(const FString& InName) const;
    bool IsFastAccessStructType(const FString& InName) const;
        
public:
    /*
    * These types are usually implemented manually on the C# side, 
    * so there is no need to export anything automatically anymore
    */
    UPROPERTY(EditAnywhere, config, Category="Binding Export")
    TSet<FString> BuiltinNames;

    /*
    * Force exports of these types to be ignored
    */
    UPROPERTY(EditAnywhere, config, Category = "Binding Export")
    TSet<FString> IgnoreExportTypeNames;

    /*
    * Structs with no public fields are usually automatically masked, 
    * but if you add it to this list it will force the struct to be exported. 
    * However, the export result of this structure is empty, 
    * and you usually need to manually fill in the remaining content: including fields, methods, etc.
    * for example: check FInputActionValue/ FInputActionValue.extends.cs
    */
    UPROPERTY(EditAnywhere, config, Category = "Binding Export")
    TSet<FString> ForceExportEmptyStructNames;

    /*
    * It does not make sense for most Modules to export access interfaces to C#, 
    * so if you want to add exported content, you can configure the corresponding module name here. 
    * It should be noted that if you add a new module, you usually need to add the modules it depends on, 
    * otherwise the generated C# code may not be compiled.
    */
    UPROPERTY(EditAnywhere, Config, Category = "Binding Export")
    TSet<FString> ExportModuleNames;

    // it will show ignore empty structures
    // If you find that some structure types do not exist on the C# side, 
    // consider turning on this switch and checking the output warnings when re-exporting the type database.
    UPROPERTY(EditAnywhere, Config, Category = "Binding Export")
    bool bShowIgnoreEmptyStructWarning = true;

public:
    // Enable this feature only for the modules specified here
    UPROPERTY(EditAnywhere, config, Category = "Binding Export|Fast Invoke")
    TSet<FString> FastFunctionInvokeModuleNames;

    /*
    * Added support for direct mapping of struct type names between C# and C++
    * After turning on relevant functions, the tool will help you generate faster interactive code
    * Please note that the structure that enables this feature must have exactly the same memory layout on both ends of C++ and C# (including the same structure size, size and offset of each field, etc.), 
    * otherwise it will cause serious consequences. So please think carefully before modifying this configuration.
    */
    UPROPERTY(EditAnywhere, config, Category="Binding Export|Fast Invoke")
    TSet<FString> FastAccessStructTypeNames;

    // Not all UFunctions in Unreal engine code support calling from the C++ code of external Modules, 
    // so additional means need to be used to prohibit the export of these functions.
    // Config it by Class C++ Name + :: + MethodName 
    // for example : UAnimMontage::IsValidAdditiveSlot
    UPROPERTY(EditAnywhere, config, Category = "Binding Export|Fast Invoke")
    TSet<FString> FastFunctionInvokeIgnoreNames;

    // Not all UFunctions in Unreal engine code support calling from the C++ code of external Modules, 
    // so additional means need to be used to prohibit the export of these functions.
    // ignore all functions in this class
    UPROPERTY(EditAnywhere, config, Category = "Binding Export|Fast Invoke")
    TSet<FString> FastFunctionInvokeIgnoreClassNames;

    /*
    * If you enable this feature, additional C++ interfaces will be generated for you. 
    * These interfaces will help you call Unreal C++ functions from C# faster.
    * Of course, this may also make the build pipeline more complex
    */
    UPROPERTY(EditAnywhere, config, Category="Binding Export|Fast Invoke")
    bool bEnableFastFunctionInvoke = true;

    
};

