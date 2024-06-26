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
#include "MonoRuntime/MonoRuntime.h"
#include "MonoRuntime/MonoInteropUtils.h"
#include "Classes/UnrealSharpSettings.h"
#include "Misc/UnrealSharpLog.h"
#include "Misc/ScopedExit.h"
#include "Misc/UnrealSharpPaths.h"

#if WITH_MONO
#include "MonoRuntime/MonoMethod.h"
#include "MonoRuntime/MonoType.h"
#include "MonoRuntime/MonoMethodInvocation.h"
#include "MonoRuntime/MonoPropertyMarshaller.h"
#include "MonoRuntime/MonoGCHandle.h"
#include "MonoRuntime/MonoApis.h"
#include "MonoRuntime/MonoLibraryAccessor.h"
#include <string>

#define LOCTEXT_NAMESPACE "MonoRuntime"

namespace UnrealSharp::Mono
{
    FString FMonoRuntime::NativeLibraryPath;
    FString FMonoRuntime::ManagedLibraryPath;
    TArray<FString> FMonoRuntime::LibrarySearchPaths;
    bool FMonoRuntime::bIsDebuggerAvailable = false;

    void FMonoRuntime::InitLibrarySearchPaths()
    {
        FString NativePath = FPaths::ConvertRelativePathToFull(
            FPaths::Combine(
                FPaths::ProjectDir(),
                TEXT("Plugins/UnrealSharp/") TEXT(UNREALSHARP_NATIVE_LIBDIRECTORY_RELATIVE_PATH)
            )
        );
        check(FPaths::DirectoryExists(*NativePath));

        US_LOG(TEXT("Native mono directory:%s"), *NativePath);

        NativeLibraryPath = NativePath;

        FString LibraryPath = FPaths::ConvertRelativePathToFull(
            FPaths::Combine(
                FPaths::ProjectDir(),
                TEXT("Plugins/UnrealSharp/") TEXT(UNREALSHARP_SYSTEM_MANAGED_LIBDIRECTORY_RELATIVE_PATH)
            )
        );

        US_LOG(TEXT("System managed libary directory:%s"), *LibraryPath);

        check(FPaths::DirectoryExists(*LibraryPath));
        
        ManagedLibraryPath = LibraryPath;

        FString UserPath = FPaths::ConvertRelativePathToFull(FUnrealSharpPaths::GetUnrealSharpManagedLibraryDir());
        checkf(FPaths::DirectoryExists(UserPath), TEXT("C# output directory is not exists: %s, Please build C# codes first."), *UserPath);

        LibrarySearchPaths.Add(UserPath);
        LibrarySearchPaths.Add(LibraryPath);
        LibrarySearchPaths.Add(NativePath);
    }

    FString FMonoRuntime::SearchLibrary(const FString& InName)
    {
        for (const auto& SearchPath : LibrarySearchPaths)
        {
            auto AsmPath = FPaths::Combine(*SearchPath, *InName);
            if (FPaths::FileExists(AsmPath))
            {
                return AsmPath;
            }
        }

        return FString();
    }

#if PLATFORM_MAC || PLATFORM_WINDOWS || PLATFORM_LINUX
    static const FString UnrealSharpTempFilePrefix = TEXT("__unrealsharp_temp.");
    static void DeleteIntermediateTempFiles(const FString& Directory)
    {
        IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();
                
        TArray<FString> Files;
        PlatformFile.FindFiles(Files, *Directory, TEXT(""));
                
        for (FString& File : Files)
        {            
            FString FileName = FPaths::GetCleanFilename(File);
            if (FileName.StartsWith(UnrealSharpTempFilePrefix, ESearchCase::IgnoreCase))
            {                
                // try delete it.
                PlatformFile.DeleteFile(*File);
            }
        }
    }
#endif

#if PLATFORM_WINDOWS
    // check here
    // https://www.cnblogs.com/bodong/p/17962564
    static void EnableVisualStudioToolsForUnitySupport()
    {
        IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();

        FString ProcessPath = FPlatformProcess::ExecutableName();                
        FString ProcessDirectory = FPaths::GetPath(ProcessPath);

        FString ProcessName = FPaths::GetBaseFilename(ProcessPath);                
        FString DataDirectoryPath = ProcessDirectory / (ProcessName + TEXT("_Data"));
        if (!PlatformFile.DirectoryExists(*DataDirectoryPath))
        {
            PlatformFile.CreateDirectory(*DataDirectoryPath);
        }
                
        FString DllFilePath = ProcessDirectory / TEXT("UnityPlayer.dll");
                
        if (!PlatformFile.FileExists(*DllFilePath))
        {
            FFileHelper::SaveStringToFile(TEXT(""), *DllFilePath);
        }
    }
#endif

    FMonoRuntime::FMonoRuntime() :
        MarshallerCollectionPtr(MakeUnique<FPropertyMarshallerCollection>())
    {
#if WITH_EDITOR
        bUseTempCoreCLRLibrary = true;
#endif
        bIsDebuggerAvailable = false;

        if (LibrarySearchPaths.IsEmpty())
        {
            InitLibrarySearchPaths();
        }

        FString CoreClrRuntimePath = FPaths::Combine(NativeLibraryPath, TEXT(UNREALSHARP_CORECLR_LIBNAME));
        check(FPaths::FileExists(CoreClrRuntimePath));

        auto UnrealSharpTempDirectory = FUnrealSharpPaths::GetUnrealSharpIntermediateDir();

#if PLATFORM_MAC || PLATFORM_WINDOWS || PLATFORM_LINUX
        DeleteIntermediateTempFiles(UnrealSharpTempDirectory);
#endif

#if WITH_EDITOR
        if(bUseTempCoreCLRLibrary)
        {
            FString TempDllName = FPaths::Combine(UnrealSharpTempDirectory, UnrealSharpTempFilePrefix + TEXT("coreclr.") + FGuid::NewGuid().ToString() + TEXT(".") + FPlatformProcess::GetModuleExtension());

            IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();
            PlatformFile.CopyFile(*TempDllName, *CoreClrRuntimePath);

            check(FPaths::FileExists(TempDllName));

            CoreClrRuntimePath = TempDllName;
        }
#endif        

#if WITH_EDITOR
        LibraryHandle = FPlatformProcess::GetDllHandle(*CoreClrRuntimePath);

        checkf(LibraryHandle, TEXT("Failed load corelib from:%s"), *CoreClrRuntimePath);

        FMonoApis::Import(LibraryHandle);
#endif
        
#if PLATFORM_MAC
        // load managed require dylibs here
        // load extra files
        FString NativeExtraLibs[] = {
           // "libmsquic.dylib",
            "libSystem.Globalization.Native.dylib",
            "libSystem.IO.Compression.Native.dylib",
            "libSystem.IO.Ports.Native.dylib",
            "libSystem.Native.dylib",
            "libSystem.Net.Security.Native.dylib",
            "libSystem.Security.Cryptography.Native.Apple.dylib",
            "libSystem.Security.Cryptography.Native.OpenSsl.dylib"
        };

        for(auto& libName : NativeExtraLibs)
        {
            FString ExtraLibPath = FPaths::Combine(ManagedLibraryPath, libName);
            checkf(FPaths::FileExists(ExtraLibPath), TEXT("Failed find dependency library:%s"), *ExtraLibPath);

            // load it
            auto Handle = FPlatformProcess::GetDllHandle(*ExtraLibPath);
            checkf(Handle, TEXT("Failed load %s"), *ExtraLibPath);
            
            ExtraLibraryHandles.Add(Handle);
        }

#endif
    }

    FMonoRuntime::~FMonoRuntime()
    {
#if WITH_EDITOR
        FMonoApis::UnImport();
#endif

        if(LibraryHandle != nullptr)
        {
            if(!bUseTempCoreCLRLibrary)
            {
                FPlatformProcess::FreeDllHandle(LibraryHandle);
            }
            
            LibraryHandle = nullptr;
        }        
        
#if PLATFORM_MAC
        for(auto& handle : ExtraLibraryHandles)
        {
            FPlatformProcess::FreeDllHandle(handle);
        }
        
        ExtraLibraryHandles.Empty();
#endif

        bIsDebuggerAvailable = false;
    }

    const FName& FMonoRuntime::GetRuntimeType() const
    {
        static const FName Z_Type = "Mono";

        return Z_Type;
    }

    bool FMonoRuntime::InitializeInternal()
    {    
        mono_install_assembly_preload_hook(OnAssemblyLoaded, nullptr);

        InitLogger();

        if(!GetDefault<UUnrealSharpSettings>()->bPerformanceMode)
        {
            InitDebugger();
        }

        if (!InitDomain())
        {
            return false;
        }

        /*
        * Try Fix: mono_coop_mutex_lock Cannot transition thread 000000??0000???? from STATE_BLOCKING with DO_BLOCKING
        * https://www.cnblogs.com/bodong/p/18027808
        */
        if(bIsDebuggerAvailable && GetDefault<UUnrealSharpSettings>()->bWaitDebugger)
        {
            //
            const float SleepTime = GetDefault<UUnrealSharpSettings>()->DelayMonoStartTimeWhenWaitDebugger;
            US_LOG(TEXT("Sleep %f seconds, wait debugger refresh source infos."), SleepTime);
            FPlatformProcess::Sleep(SleepTime);
        }
                
        return true;
    }

    void FMonoRuntime::InitDebugger()
    {
        const UUnrealSharpSettings* Settings = GetDefault<UUnrealSharpSettings>();

#if !WITH_EDITOR
        if (Settings->bEnableDebugger)
#endif
        {
#if PLATFORM_WINDOWS
            // allow you debug mono C# code with Visual Studio Tools for unity
            EnableVisualStudioToolsForUnitySupport();
#endif

            // hack for Visual Studio Tools for unity
            int DebuggerPort = Settings->bEnableRiderDebuggerSupport ? Settings->RiderDebuggerDefaultPort : (56000 + FPlatformProcess::GetCurrentProcessId() % 1000);            
            FString LogFileArguments;
            FString LogLevelArguments;

            // if we use log file, it will not send to debugger
            if(Settings->bUseMonoLogFile)
            {
                const int logLevel = Settings->MonoLogLevel;
                LogLevelArguments = FString::Printf(TEXT(",loglevel=%d"), logLevel);

                auto UnrealSharpTempDirectory = FUnrealSharpPaths::GetUnrealSharpIntermediateDir();
                FString MonoLogFile = FPaths::Combine(UnrealSharpTempDirectory, TEXT("mono.log"));
                LogFileArguments = FString::Printf(TEXT(",logfile=%s"), *MonoLogFile);
            }            

            FString Arguments = FString::Printf(TEXT("--debugger-agent=transport=dt_socket,embedding=%s,server=y,suspend=%s%s%s,address=127.0.0.1:%d"), 
                Settings->bWaitDebugger ? TEXT("n") : TEXT("y"),
                Settings->bWaitDebugger ? TEXT("y") : TEXT("n"),
                *LogLevelArguments,
                *LogFileArguments,
                DebuggerPort                
            );            

            std::string argument = TCHAR_TO_ANSI(*Arguments);

            const char* options[] = 
            {
               argument.c_str()
            };

            // in debug support mode, we need force use interop mode
            // because jit mode will cause crash.
            // comment this code, you will cause the crash
            mono_jit_set_aot_mode(MONO_AOT_MODE_INTERP_ONLY);

            mono_jit_parse_options(sizeof(options)/sizeof(options[0]), (char**)options);
            mono_debug_init(MONO_DEBUG_FORMAT_MONO);

            bIsDebuggerAvailable = true;
        }
    }

    void FMonoRuntime::MonoLog(const char* InDomainName, const char* InLogLevel, const char* InMessage, mono_bool InFatal, void* InUserData)
    {        
        if (InFatal || 0 == FCStringAnsi::Strncmp("error", InLogLevel, 5))
        {
            // fatal error
            UE_LOG(UnrealSharpLog, Fatal, TEXT("[Mono]%s%s%s"), InDomainName != nullptr ? ANSI_TO_TCHAR(InDomainName) : TEXT(""), InDomainName != nullptr ? TEXT(": ") : TEXT(""), ANSI_TO_TCHAR(InMessage)); // NOLINT
        }
#if NO_LOGGING
#else
        else if (0 == FCStringAnsi::Strncmp("warning", InLogLevel, 7))
        {
            US_LOG_WARN(TEXT("[Mono]%s%s%s"), InDomainName != nullptr ? ANSI_TO_TCHAR(InDomainName) : TEXT(""), InDomainName != nullptr ? TEXT(": ") : TEXT(""), ANSI_TO_TCHAR(InMessage));
        }
        else if (0 == FCStringAnsi::Strncmp("critical", InLogLevel, 8))
        {
            US_LOG_ERROR(TEXT("[Mono]%s%s%s"), InDomainName != nullptr ? ANSI_TO_TCHAR(InDomainName) : TEXT(""), InDomainName != nullptr ? TEXT(": ") : TEXT(""), ANSI_TO_TCHAR(InMessage));
        }
        else
        {
            US_LOG(TEXT("[Mono]%s%s%s"), InDomainName != nullptr ? ANSI_TO_TCHAR(InDomainName) : TEXT(""), InDomainName != nullptr ? TEXT(": ") : TEXT(""), ANSI_TO_TCHAR(InMessage));
        }
#endif
    }

    void FMonoRuntime::MonoPrintf(const char* InString, mono_bool bIsStdout)
    {
#if !NO_LOGGING
        US_LOG(TEXT("[Mono]%s"), ANSI_TO_TCHAR(InString));
#endif
    }

    void FMonoRuntime::InitLogger()
    {
        mono_trace_set_log_handler(MonoLog, nullptr);
        mono_trace_set_print_handler(MonoPrintf);
        mono_trace_set_printerr_handler(MonoPrintf);
    }

    bool FMonoRuntime::InitDomain()
    {
        Domain = mono_jit_init_version(TCHAR_TO_ANSI(FApp::GetProjectName()), DOTNET_VERSION);
        
        check(Domain != nullptr);

        if (Domain == nullptr)
        {
            US_LOG_ERROR(TEXT("Failed init Mono runtime"));
            return false;
        }

        FMonoInteropUtils::Initialize(this);

        char* mono_version = mono_get_runtime_build_info();
        auto MonoVersion = FString(ANSI_TO_TCHAR(mono_version));
        mono_free(mono_version);

        US_LOG(TEXT("Loaded Mono runtime %s"), *MonoVersion);

        return true;
    }

    void FMonoRuntime::ShutdownInternal()
    {
        FMonoInteropUtils::Uninitialize();
        
        if (!bUseTempCoreCLRLibrary)
        {
            // invoke this will cause mono internal error:
            //     Cannot transition thread 000000??0000???? from STATE_BLOCKING with DO_BLOCKING
            // mono_domain_finalize(Domain, -1);            
        }        

        mono_jit_cleanup(Domain);
    }

    void FMonoRuntime::MonoStringToFString(FString& Result, MonoString* InString)
    {
        auto* utf16Text = mono_string_to_utf16(InString);
        Result = utf16Text;

        mono_free(utf16Text);
    }

    FName FMonoRuntime::MonoStringToFName(MonoString* InString)
    {
        FString TempString;
        MonoStringToFString(TempString, InString);
        return FName(*TempString);
    }

    void FMonoRuntime::LogException(MonoObject* InException)
    {
        FText ExceptionError;
        MonoObject* ExceptionInStringConversion = nullptr;
        MonoString* MonoExceptionString = mono_object_to_string(InException, &ExceptionInStringConversion);
        if (nullptr != MonoExceptionString)
        {
            FString ExceptionString;
            MonoStringToFString(ExceptionString, MonoExceptionString);
            FFormatNamedArguments Args;
            Args.Add(TEXT("ExceptionMessage"), FText::FromString(ExceptionString));
            ExceptionError = FText::Format(LOCTEXT("ExceptionError", "Managed exception: {ExceptionMessage}"), Args);
        }
        else
        {
            check(ExceptionInStringConversion);
            // Can't really get much out of the original exception with the public API, so just note that two exceptions were thrown
            FString ExceptionString;
            MonoExceptionString = mono_object_to_string(ExceptionInStringConversion, nullptr);
            check(MonoExceptionString);
            MonoStringToFString(ExceptionString, MonoExceptionString);
            FFormatNamedArguments Args;
            MonoClass* ExceptionClass = mono_object_get_class(InException);
            Args.Add(TEXT("OriginalExceptionType"), FText::FromString(mono_class_get_name(ExceptionClass)));
            Args.Add(TEXT("NestedExceptionMessage"), FText::FromString(ExceptionString));
            ExceptionError = FText::Format(LOCTEXT("NestedExceptionError", "Nested exception! Original exception was of type '{OriginalExceptionType}'. Nested Exception: {NestedExceptionMessage}"), Args);
        }

        if (IsInGameThread())
        {
            SendErrorToMessageLog(ExceptionError);
        }
        else
        {
            // dispatch to game thread
            FSimpleDelegateGraphTask::CreateAndDispatchWhenReady(
                FSimpleDelegateGraphTask::FDelegate::CreateStatic(SendErrorToMessageLog, ExceptionError)
                , NULL
                , NULL
                , ENamedThreads::GameThread
            );
        }
    }

    void FMonoRuntime::SendErrorToMessageLog(FText InError)
    {
        static const FName NAME_MonoErrors("MonoErrors");

        FMessageLog(NAME_MonoErrors).Error(InError);
    }

    MonoAssembly* FMonoRuntime::OnAssemblyLoaded(MonoAssemblyName* aname, char** InAssemblies, void* InUserData)
    {
        const char* name = mono_assembly_name_get_name(aname);
        const char* culture = mono_assembly_name_get_culture(aname);
        FString AsmName = FString(ANSI_TO_TCHAR(name));
        FString AsmCulture = FString(ANSI_TO_TCHAR(culture));

        if (!AsmName.EndsWith(TEXT(".dll"), ESearchCase::IgnoreCase))
        {
            AsmName = AsmName + TEXT(".dll");
        }

        FString AsmPath;
        for (const auto& SearchPath : LibrarySearchPaths)
        {
            AsmPath = FPaths::Combine(*SearchPath, *AsmName);

            if (!FPaths::FileExists(AsmPath))
            {
                AsmPath = FPaths::Combine(*SearchPath, *AsmCulture, *AsmName);
                if (!FPaths::FileExists(AsmPath))
                {
                    continue;
                }
            }

            US_LOG(TEXT("Found assembly %s at path '%s'."), *AsmName, *AsmPath);

            auto cache = StaticLoadAssembly(AsmPath);

            return cache.Assembly;
        }

        US_LOG_ERROR(TEXT("Could not find assembly %s."), *AsmName);

        return nullptr;
    }

    FMonoRuntime::FMonoAssemblyCache FMonoRuntime::StaticLoadAssembly(const FString& InAssemblyPath)
    {
        FString AbsoluteAssemblyPath = IFileManager::Get().ConvertToAbsolutePathForExternalAppForRead(*InAssemblyPath);
        FString AsmName = FPaths::GetBaseFilename(InAssemblyPath);

        MonoImageOpenStatus status;
        MonoAssembly* LoadedAssembly = nullptr; // NOLINT

        // direct load library by file
        if (!AsmName.StartsWith(TEXT("UnrealSharp.")))
        {
            LoadedAssembly = mono_assembly_open(TCHAR_TO_ANSI(*AbsoluteAssemblyPath), &status);
            if (LoadedAssembly)
            {
                US_LOG(TEXT("Loaded assembly from path '%s'."), *AbsoluteAssemblyPath);

                return { LoadedAssembly, mono_assembly_get_image(LoadedAssembly)};
            }
        }
#if PLATFORM_MAC || PLATFORM_WINDOWS || PLATFORM_LINUX
        else if(bIsDebuggerAvailable)
        {
            // create temp files for them
            FString SourceAssemblyPath = AbsoluteAssemblyPath;
            FString SourcePdbPath = FPaths::ChangeExtension(AbsoluteAssemblyPath, TEXT("pdb"));

            FString TempFileName = FString::Printf(TEXT("%s%s.%s"), *UnrealSharpTempFilePrefix, *AsmName, *FGuid::NewGuid().ToString());
            FString IntermediateDirectory = FUnrealSharpPaths::GetUnrealSharpIntermediateDir();

            FString IntermediateDllPath = FPaths::Combine(IntermediateDirectory, TempFileName + TEXT(".dll"));
            FString IntermediatePdbPath = FPaths::Combine(IntermediateDirectory, TempFileName + TEXT(".pdb"));

            IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();

            PlatformFile.CopyFile(*IntermediateDllPath, *SourceAssemblyPath);
            PlatformFile.CopyFile(*IntermediatePdbPath, *SourcePdbPath);

            LoadedAssembly = mono_assembly_open(TCHAR_TO_ANSI(*IntermediateDllPath), &status);
            if (LoadedAssembly)
            {
                US_LOG(TEXT("Loaded assembly from path temp path '%s'."), *IntermediateDllPath);

                return { LoadedAssembly, mono_assembly_get_image(LoadedAssembly) };
            }
        }
#endif

        TUniquePtr<FArchive> Reader(IFileManager::Get().CreateFileReader(*AbsoluteAssemblyPath));
        if (!Reader)
        {
            US_LOG_ERROR(TEXT("Failed to read assembly from path '%s'."), *AbsoluteAssemblyPath);
            
            return {};
        }

        uint32 Size = Reader->TotalSize();
        void* Data = FMemory::Malloc(Size);

        UNREALSHARP_SCOPED_EXIT(FMemory::Free(Data););

        Reader->Serialize(Data, Size);

        MonoImage* LoadedImage = mono_image_open_from_data_with_name((char*)Data, Size, true, &status, false, TCHAR_TO_UTF8(*AsmName));

        if (!LoadedImage)
        {
            US_LOG_ERROR(TEXT("Failed to load image from path '%s'."), *AbsoluteAssemblyPath);
            
            return {};
        }

        LoadedAssembly = mono_assembly_load_from_full(LoadedImage, TCHAR_TO_UTF8(*AsmName), &status, 0);
        if (!LoadedAssembly)
        {
            US_LOG_ERROR(TEXT("Failed to load image from path '%s'."), *AbsoluteAssemblyPath);
            
            return {};
        }

        US_LOG(TEXT("Loaded assembly from path '%s'."), *AbsoluteAssemblyPath);

        return { LoadedAssembly, mono_assembly_get_image(LoadedAssembly) };
    }

    FMonoRuntime::FMonoAssemblyCache FMonoRuntime::LoadAssembly(const FString& InAssemblyName)
    {
        const FString* AssemblyNamePtr = &InAssemblyName;
        FString TempAssemblyPath;

        if (!InAssemblyName.EndsWith(TEXT(".dll"), ESearchCase::IgnoreCase))
        {
            TempAssemblyPath = InAssemblyName + TEXT(".dll");
            AssemblyNamePtr = &TempAssemblyPath;
        }

        auto* Ptr = AssemblyCaches.Find(*AssemblyNamePtr);

        if (Ptr != nullptr)
        {
            return *Ptr;
        }

        FString TargetPath = SearchLibrary(*AssemblyNamePtr);

        if (!FPaths::FileExists(*TargetPath))
        {
            US_LOG_ERROR(TEXT("Failed find assembly:%s"), **AssemblyNamePtr);
            return FMonoAssemblyCache();
        }

        auto Cache = StaticLoadAssembly(TargetPath);

        AssemblyCaches.Add(*AssemblyNamePtr, Cache);

        return Cache;
    }
        
    MonoMethod* FMonoRuntime::LoadMethod(MonoImage* InImage, const char* InFullyQualifiedMethodName)
    {
        check(InImage);
        check(InFullyQualifiedMethodName);

        MonoMethodDesc* MethodDesc = mono_method_desc_new(InFullyQualifiedMethodName, true);
        check(MethodDesc);

        UNREALSHARP_SCOPED_EXIT(mono_method_desc_free(MethodDesc));

        MonoMethod* Method = mono_method_desc_search_in_image(MethodDesc, InImage);

#if !UE_BUILD_SHIPPING
        if (Method == nullptr)
        {
            US_LOG_WARN(TEXT("Failed Find method by signature:%s"), ANSI_TO_TCHAR(InFullyQualifiedMethodName));
            FMonoInteropUtils::DumpAssemblyClasses(mono_image_get_assembly(InImage));
        }
#endif

        return Method;
    }

    MonoMethod* FMonoRuntime::LoadMethod(MonoClass* InClass, const char* InFullyQualifiedMethodName)
    {
        MonoMethodDesc* MethodDesc = mono_method_desc_new(InFullyQualifiedMethodName, true);
        check(MethodDesc);

        UNREALSHARP_SCOPED_EXIT(mono_method_desc_free(MethodDesc));

        MonoMethod* Method = mono_method_desc_search_in_class(MethodDesc, InClass);

#if !UE_BUILD_SHIPPING
        if (Method == nullptr)
        {
            US_LOG_WARN(TEXT("Failed Find method by signature:%s"), ANSI_TO_TCHAR(InFullyQualifiedMethodName));
            FMonoInteropUtils::DumpClassInfomration(InClass);
        }
#endif

        return Method;
    }

    MonoMethod* FMonoRuntime::LoadMethod(const TCHAR* AssemblyName, const char* InFullyQualifiedMethodName)
    {
        auto AssemblyCache = LoadAssembly(AssemblyName);

        if (!AssemblyCache.IsValid())
        {
            return nullptr;
        }

        MonoMethodDesc* MethodDesc = mono_method_desc_new(InFullyQualifiedMethodName, true);
        check(MethodDesc);

        UNREALSHARP_SCOPED_EXIT(mono_method_desc_free(MethodDesc));

        MonoMethod* Method = mono_method_desc_search_in_image(MethodDesc, AssemblyCache.Image);

#if !UE_BUILD_SHIPPING
        if (Method == nullptr)
        {
            US_LOG_WARN(TEXT("Failed Find method by signature:%s in assembly:%s"), ANSI_TO_TCHAR(InFullyQualifiedMethodName), AssemblyName);

            FMonoInteropUtils::DumpAssemblyClasses(AssemblyCache.Assembly);
        }
#endif

        return Method;
    }

    MonoObject* FMonoRuntime::Invoke(MonoMethod* InMethod, MonoObject* Object, void** InArguments, MonoObject** OutException)
    {
        check(InMethod);

        MonoObject* Exception = nullptr;
        MonoObject* ReturnValue = mono_runtime_invoke(InMethod, Object, InArguments, &Exception);

        if (OutException != nullptr)
        {
            *OutException = Exception;
        }

        if (Exception == nullptr)
        {
            return ReturnValue;
        }
        else
        {
            LogException(Exception);

            return nullptr;
        }
    }

    MonoObject* FMonoRuntime::InvokeDelegate(MonoObject* InDelegate, void** InArguments, MonoObject** OutException)
    {
        check(InDelegate);

        MonoObject* Exception = nullptr;
        MonoObject* ReturnValue = mono_runtime_delegate_invoke(InDelegate, InArguments, &Exception);

        if (OutException != nullptr)
        {
            *OutException = Exception;
        }

        if (Exception == nullptr)
        {
            return ReturnValue;
        }
        else
        {
            LogException(Exception);

            return nullptr;
        }
    }

    TSharedPtr<ICSharpMethod> FMonoRuntime::LookupMethod(const FString& InAssemblyName, const FString& InFullyQualifiedMethodName)
    {
        MonoMethod* Method = LoadMethod(*InAssemblyName, TCHAR_TO_ANSI(*InFullyQualifiedMethodName));

        if (Method == nullptr)
        {
            return TSharedPtr<ICSharpMethod>();
        }

        TSharedPtr<FMonoMethod> MethodPtr = MakeShared<FMonoMethod>(Method);

        return MethodPtr;
    }

    TSharedPtr<ICSharpMethod> FMonoRuntime::LookupMethod(ICSharpType* InType, const FString& InFullyQualifiedMethodName)
    {
        check(InType);

        MonoMethod* Method = LoadMethod(((FMonoType*)InType)->GetClass(), TCHAR_TO_ANSI(*InFullyQualifiedMethodName));

        if (Method == nullptr)
        {
            US_LOG_WARN(TEXT("Failed find method %s"), *InFullyQualifiedMethodName);
            return TSharedPtr<ICSharpMethod>();
        }

        TSharedPtr<FMonoMethod> MethodPtr = MakeShared<FMonoMethod>(Method);

        return MethodPtr;
    }

    TSharedPtr<ICSharpType> FMonoRuntime::LookupType(const FString& InAssemblyName, const FString& InNamespace, const FString& InName)
    {
        auto AssemblyCache = LoadAssembly(InAssemblyName);

        if (!AssemblyCache.IsValid())
        {
            return TSharedPtr<ICSharpType>();
        }

        MonoClass* Class = mono_class_from_name(AssemblyCache.Image, TCHAR_TO_ANSI(*InNamespace), TCHAR_TO_ANSI(*InName));

        if (Class == nullptr)
        {
            return TSharedPtr<ICSharpType>();
        }

        TSharedPtr<FMonoType> TypePtr = MakeShared<FMonoType>(Class);

        return TypePtr;
    }

    TSharedPtr<ICSharpMethodInvocation> FMonoRuntime::CreateCSharpMethodInvocation(TSharedPtr<ICSharpMethod> InMethod)
    {
        TSharedPtr<FMonoMethodInvocation> Invocation = MakeShared<FMonoMethodInvocation>(StaticCastSharedPtr<FMonoMethod>(InMethod));

        return Invocation;
    }

    TSharedPtr<ICSharpMethodInvocation>    FMonoRuntime::CreateCSharpMethodInvocation(const FString& InAssemblyName, const FString& InFullyQualifiedMethodName)
    {
        auto Method = LookupMethod(InAssemblyName, InFullyQualifiedMethodName);

        if (Method)
        {
            return CreateCSharpMethodInvocation(Method);
        }

        return TSharedPtr<ICSharpMethodInvocation>();
    }

    const IPropertyMarshaller* FMonoRuntime::GetPropertyMarshaller(const FProperty* InProperty) const
    {        
        return MarshallerCollectionPtr->GetMarshaller(InProperty);
    }

    const IPropertyMarshaller* FMonoRuntime::GetPropertyMarshaller(const FFieldClass* InFieldClass) const
    {
        return MarshallerCollectionPtr->GetMarshaller(InFieldClass);
    }       

    TSharedPtr<ICSharpGCHandle> FMonoRuntime::CreateCSharpGCHandle(void* InCSharpObject, bool bInWeakReference)
    {
        TSharedPtr<ICSharpGCHandle> Result = MakeShared<FMonoGCHandle>(InCSharpObject, bInWeakReference);

        return Result;
    }

    void FMonoRuntime::ExecuteGarbageCollect(bool bFully)
    {
        if (bFully)
        {
            mono_gc_collect(mono_gc_max_generation());
        }
        else
        {
            mono_gc_collect(0);
        }
    }

    TSharedPtr<ICSharpLibraryAccessor> FMonoRuntime::CreateCSharpLibraryAccessor()
    {
        return MakeShared<FMonoLibraryAccessor>(this);
    }
}

#endif
