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
#include "UnrealSharpSettings.generated.h"

/**
 * About the configuration of Unreal Sharp. 
 * For export configuration, please refer to USharpBindingGenSettings
 */
UCLASS(config = UnrealSharp, defaultconfig, meta = (DisplayName = "UnrealSharp"))
class UNREALSHARP_API UUnrealSharpSettings : public UDeveloperSettings
{
    GENERATED_BODY()

public:
    UUnrealSharpSettings();

public:
    bool IsExportToGameScripts(const UField* InField) const;
    bool IsExportToGameScripts(const FName& InModuleName) const;

public:
    /*
    * Adopt performance mode, which turns off some unnecessary functions to pursue higher running speeds
    */
    UPROPERTY(EditAnywhere, config, Category = "Debugger")
    bool bPerformanceMode;

    /*
    * Whether to start in a mode that supports the debugger. 
    * It is enabled by default in the editor mode, no matter what this option is.
    */
    UPROPERTY(EditAnywhere, config, Category = "Debugger")
    bool bEnableDebugger = false;

    /*
    * If this switch is turned on, when you start playing the game, it will enter the waiting state immediately after initializing the Runtime. 
    * At this time, you can use the debugger to attach it. 
    * This is useful if you want to debug code early in the startup process.
    */
    UPROPERTY(EditAnywhere, config, Category = "Debugger")
    bool bWaitDebugger = false;

    /*
    * There will be some incompatibilities between the Mono runtime and the Mono debugger when running in wait-to-start mode, 
    * which will cause mono to crash. 
    * Delaying startup for a certain period of time can avoid crashes caused by resource competition as much as possible. 
    * If you encounter the following error, you can try changing this value to a larger value:
    *      mono_coop_mutex_lock Cannot transition thread 000000??0000???? from STATE_BLOCKING with DO_BLOCKING
    */
    UPROPERTY(EditAnywhere, config, Category = "Debugger|Mono")
    float DelayMonoStartTimeWhenWaitDebugger = 1.0f;

    /*
    * If this option is turned on, when using the mono runtime, 
    * it will try to output some information inside the mono debugger to the specified file, 
    * located in $(UnrealProjectDirectory)Intermediate/UnrealSharp/mono.log.
    */
    UPROPERTY(EditAnywhere, config, Category = "Debugger|Mono")
    bool bUseMonoLogFile = false;

    /*
    * Mono Log Level
    * 0-10
    */
    UPROPERTY(EditAnywhere, config, Category = "Debugger|Mono")
    int MonoLogLevel = 10;        

    /*
    * Whether to support Blueprint binding. 
    * When this feature is turned on, bindings for blueprint types will be automatically generated. 
    * These bindings can only be used in GameContent projects.
    */
    UPROPERTY(EditAnywhere, config, Category = "Binding")
    bool bSupportBlueprintBinding = true;

    /*
    * Allows you to export some Module configurations to the GameScripts project. 
    * This usually configures the game project's own plug-ins and Modules. 
    * This can avoid all Native binding code being located in the UnrealSharp.UnrealEngine project, causing long compilation times.
    */
    UPROPERTY(EditAnywhere, config, Category = "Binding")
    TSet<FName> NativeExportToGameScriptsModules;

    /*
    * If you want to debug C# code with Rider, Please check this
    * If you enable this option, you will only be able to use Rider for debugging, and the Visual Studio debugger will no longer be able to be attached.
    * And it does not support debugging multiple UnrealEditor instances at the same time.
    */
    UPROPERTY(EditAnywhere, config, Category = "Debugger|Mono|Rider")
    bool bEnableRiderDebuggerSupport = false;

    /*
    * You need to connect to this port in Rider to debug correctly
    */
    UPROPERTY(EditAnywhere, config, Category = "Debugger|Mono|Rider")
    int RiderDebuggerDefaultPort = 57000;
};
