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
#include "Misc/UnrealInteropFunctions.h"
#include "IUnrealSharpModule.h"
#include "Misc/InteropUtils.h"
#include "Misc/CSharpStructures.h"
#include "Misc/UnrealSharpLog.h"

namespace UnrealSharp
{
    FUnrealInteropFunctions::FUnrealInteropFunctions()
    {
        SetupBaseInteropFunctions();
        SetupInternalInteropFunctions();
    }

    FUnrealInteropFunctions::~FUnrealInteropFunctions()
    {
    }

    bool FUnrealInteropFunctions::AddInteropFunction(const TCHAR* InFunctionName, void* InFunc, bool bInAllowOverride /* = false */)
    {
        FString FunctionName(InFunctionName);
        return AddInteropFunction(FunctionName, InFunc, bInAllowOverride);
    }

    bool FUnrealInteropFunctions::AddInteropFunction(const FString& InFunctionName, void* InFunc, bool bInAllowOverride /* = false */)
    {
        auto* Ptr = InteropFunctions.Find(InFunctionName);

        if (Ptr != nullptr)
        {
            if (!bInAllowOverride)
            {
                // compatible with multiple clients and players in UnrealEditor...
                if (InFunc != *Ptr)
                {
                    checkfSlow(false, TEXT("Can't Add interop function %s, this name is already exists, but bInAllowOverride=False"), *InFunctionName);
                    return false;
                }

                return true;
            }
            else
            {
                *Ptr = InFunc;
            }
        }
        else
        {
            InteropFunctions.Add(InFunctionName, InFunc);
        }

        return true;
    }

    void* FUnrealInteropFunctions::GetInteropFunction(const FString& InFunctionName) const
    {
        auto* Ptr = InteropFunctions.Find(InFunctionName);
        return Ptr != nullptr ? *Ptr : nullptr;
    }

    void FUnrealInteropFunctions::SetupBaseInteropFunctions()
    {
        int InteropFunctionCount = 0;

#define UNREALSHARP_REGISTER_BASE_INTEROP_FUNCTION(name) \
        AddInteropFunction(TEXT(#name), (void*)&FUnrealInteropFunctions::name)

        UNREALSHARP_REGISTER_BASE_INTEROP_FUNCTION(GetUnrealInteropFunctionsPtr);
        UNREALSHARP_REGISTER_BASE_INTEROP_FUNCTION(GetUnrealInteropFunctionPointer);
        UNREALSHARP_REGISTER_BASE_INTEROP_FUNCTION(ValidateUnrealSharpBuildInfo);
        

#undef UNREALSHARP_REGISTER_BASE_INTEROP_FUNCTION
    }

    void FUnrealInteropFunctions::SetupInternalInteropFunctions()
    {
        int LocalInternalIndex = 0;

#define DECLARE_UNREAL_SHARP_INTEROP_API(returnType, name, parameters) \
			AddInteropFunction(TEXT(#name), (void*)&FInteropUtils::name)

#include "Misc/InteropApiDefines.inl"

#undef DECLARE_UNREAL_SHARP_INTEROP_API	
    }

    FUnrealInteropFunctions* FUnrealInteropFunctions::GetUnrealInteropFunctionsPtr()
    {
        IUnrealSharpModule* UnrealSharpModule = (IUnrealSharpModule*)FModuleManager::Get().LoadModule(TEXT("UnrealSharp"));
        check(UnrealSharpModule);

        return UnrealSharpModule->GetInteropFunctions();
    }

    FUnrealInteropFunctionsInfo* FUnrealInteropFunctions::GetInteropFunctionsInfoPtr()
    {
        FUnrealInteropFunctions* Instance = GetUnrealInteropFunctionsPtr();
        check(Instance);

        static FUnrealInteropFunctionsInfo Info;

        Info = {
            sizeof(Info),
            Instance, 
            (void*)&FUnrealInteropFunctions::GetUnrealInteropFunctionPointer,
            (void*)&FUnrealInteropFunctions::LogMessage
        };

        return &Info;
    }

    void* FUnrealInteropFunctions::GetUnrealInteropFunctionPointer(const FUnrealInteropFunctions* InInstance, const char* InCSharpText)
    {
        checkSlow(InInstance);
        checkSlow(InCSharpText);

        return InInstance->GetInteropFunction(UNREALSHARP_STRING_TO_TCHAR(InCSharpText));
    }

    void FUnrealInteropFunctions::LogMessage(int InLevel, const char* InMessage)
    {
        // must match with C#
        enum class LoggerLevel
        {
            /// <summary>
            /// The verbose
            /// </summary>
            Verbose = -10,

            /// <summary>
            /// The information
            /// </summary>
            Information = -9,

            /// <summary>
            /// The warning
            /// </summary>
            Warning = -8,

            /// <summary>
            /// The error
            /// </summary>
            Error = -7,

            /// <summary>
            /// The total
            /// </summary>
            Total = -5
        };

        if (InMessage != nullptr)
        {
            if (InLevel == (int)LoggerLevel::Error)
            {
                US_LOG_ERROR(TEXT("%s"), UNREALSHARP_STRING_TO_TCHAR(InMessage));
            }
            else if (InLevel == (int)LoggerLevel::Warning)
            {
                US_LOG_WARN(TEXT("%s"), UNREALSHARP_STRING_TO_TCHAR(InMessage));
            }
            else if (InLevel == (int)LoggerLevel::Verbose)
            {
                US_LOG_VERBOSE(TEXT("%s"), UNREALSHARP_STRING_TO_TCHAR(InMessage));
            }
            else
            {
                US_LOG(TEXT("%s"), UNREALSHARP_STRING_TO_TCHAR(InMessage));
            }
        }
    }

    void FUnrealInteropFunctions::ValidateUnrealSharpBuildInfo(FUnrealSharpBuildInfo* InBuildInfo)
    {
        check(InBuildInfo != nullptr);

        US_LOG(TEXT("UnrealSharp C# Build Info: Platform = %s, Configuration = %s, Editor = %s"),
            *FUnrealSharpBuildInfo::GetPlatformString(InBuildInfo->Platform),
            *FUnrealSharpBuildInfo::GetBuildConfigurationString(InBuildInfo->Configuration),
            InBuildInfo->bWithEditor ? TEXT("true") : TEXT("false")
        );

        FUnrealSharpBuildInfo NativeBuildInfo = FUnrealSharpBuildInfo::GetNativeBuildInfo();

        checkf(NativeBuildInfo.bWithEditor == InBuildInfo->bWithEditor, 
            TEXT("UnrealSharp is build with invalid configuration. C++ WITH_EDITOR=%s but C# WITH_EDITOR=%s"),
            NativeBuildInfo.bWithEditor?TEXT("true"):TEXT("false"),
            InBuildInfo->bWithEditor ? TEXT("true") : TEXT("false")
            );
        
        checkf(NativeBuildInfo.Platform == InBuildInfo->Platform,
            TEXT("UnrealSharp is build with invalid configuration. C++ Platform=%s but C# Platform=%s"),
            *FUnrealSharpBuildInfo::GetPlatformString(NativeBuildInfo.Platform),
            *FUnrealSharpBuildInfo::GetPlatformString(InBuildInfo->Platform)
            );
    }
}

