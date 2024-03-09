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
#include "UnrealSharpEditorModule.h"
#include "LevelEditor.h"
#include "TypeDefinitionDocument.h"
#include "Misc/UnrealSharpLog.h"
#include "HAL/PlatformFilemanager.h"
#include "CSharpBlueprintGenerator.h"
#include "Modules/ModuleManager.h"
#include "Misc/UnrealSharpPaths.h"
#include "Interfaces/IMainFrameModule.h"
#include "CSharpBlueprintImportDatabase.h"
#include "ProfilingDebugging/ScopedTimers.h"
#include "TypeValidation.h"
#include "Classes/UnrealSharpSettings.h"
#include "SharpBindingGenSettings.h"

IMPLEMENT_MODULE(FUnrealSharpEditorModule, UnrealSharpEditor)

static FAutoConsoleCommand ForceRefreshCSharpCodeDatabaseCommand(
	TEXT("UnrealSharp.RefreshDatabase"),
	TEXT("Force recreate C# import assets from C# generated database from $(ProjectDir)Managed/*.tdb"),
	FConsoleCommandDelegate::CreateLambda([]() {
		FUnrealSharpEditorModule* Module = (FUnrealSharpEditorModule*)FModuleManager::Get().GetModule(TEXT("UnrealSharpEditor"));
		check(Module != nullptr);

		Module->RefreshCSharpImportBlueprintAssets(true);
	}));

static FAutoConsoleCommand ForceExportNativeBindingCodesCommand(
	TEXT("UnrealSharp.ExportUnrealCppDatabase"),
	*FString::Printf(TEXT("Force export Unreal C++ types database file to %s"), *UnrealSharp::FUnrealSharpPaths::GetDefaultUnrealCppDatabaseFilePath()),
	FConsoleCommandDelegate::CreateLambda([]() {
		FUnrealSharpEditorModule* Module = (FUnrealSharpEditorModule*)FModuleManager::Get().GetModule(TEXT("UnrealSharpEditor"));
		check(Module != nullptr);

		Module->ExportDatabase(UnrealSharp::EUnrealTypeDatabaseExportFlags::WITH_CPP, false);
	}));

static FAutoConsoleCommand ForceExportBlueprintBindingCodesCommand(
	TEXT("UnrealSharp.ExportBlueprintDatabase"),
	*FString::Printf(TEXT("Force export Unreal Blueprint types database file to %s[Need Enable Blueprint Binding Support]"), *UnrealSharp::FUnrealSharpPaths::GetDefaultUnrealBlueprintDatabaseFilePath()),
	FConsoleCommandDelegate::CreateLambda([]() {
		FUnrealSharpEditorModule* Module = (FUnrealSharpEditorModule*)FModuleManager::Get().GetModule(TEXT("UnrealSharpEditor"));
		check(Module != nullptr);

		Module->ExportDatabase(UnrealSharp::EUnrealTypeDatabaseExportFlags::WITH_Blueprint, false);
		}));

static FAutoConsoleCommand ForceExportBindingCodesCommand(
	TEXT("UnrealSharp.ExportDatabase"),
	*FString::Printf(TEXT("Force Export Unreal C++/Blueprint type database file to %s and %s[Need Enable Blueprint Binding Support]"), *UnrealSharp::FUnrealSharpPaths::GetDefaultUnrealCppDatabaseFilePath(), *UnrealSharp::FUnrealSharpPaths::GetDefaultUnrealBlueprintDatabaseFilePath()),
	FConsoleCommandDelegate::CreateLambda([]() {
		FUnrealSharpEditorModule* Module = (FUnrealSharpEditorModule*)FModuleManager::Get().GetModule(TEXT("UnrealSharpEditor"));
		check(Module != nullptr);

		Module->ExportDatabase(UnrealSharp::EUnrealTypeDatabaseExportFlags::WITH_CPP| UnrealSharp::EUnrealTypeDatabaseExportFlags::WITH_Blueprint, false);
		}));

FUnrealSharpEditorModule::FUnrealSharpEditorModule()
{
	ImportDatabasePath = FPaths::Combine(UnrealSharp::FUnrealSharpPaths::GetUnrealSharpIntermediateDir(), TEXT("ImportDatabase.json"));
}

void FUnrealSharpEditorModule::StartupModule()
{	
	UnrealSharp::FUnrealSharpPaths::EnsureUnrealSharpIntermediateDirExists();

    AddExportDatabaseMenu();

	RefreshCSharpImportBlueprintAssets(true);

	PreBeginPIEHandle = FEditorDelegates::PreBeginPIE.AddRaw(this, &FUnrealSharpEditorModule::OnPreBeginPIE);
	EndPIEHandle = FEditorDelegates::EndPIE.AddRaw(this, &FUnrealSharpEditorModule::OnEndPIE);

	IMainFrameModule& MainFrameModule = FModuleManager::LoadModuleChecked<IMainFrameModule>(TEXT("MainFrame"));
	MainFrameModule.OnMainFrameCreationFinished().AddRaw(this, &FUnrealSharpEditorModule::OnMainFrameCreationFinished);
}

void FUnrealSharpEditorModule::ShutdownModule()
{	
	FEditorDelegates::PreBeginPIE.Remove(PreBeginPIEHandle);
	FEditorDelegates::EndPIE.Remove(EndPIEHandle);

	PreBeginPIEHandle.Reset();
	EndPIEHandle.Reset();	
}

void FUnrealSharpEditorModule::AddExportDatabaseMenu()
{
    TSharedPtr<FExtender> MenuExtender = MakeShareable(new FExtender());
    MenuExtender->AddMenuExtension("Python",
        EExtensionHook::After,
        nullptr,
        FMenuExtensionDelegate::CreateLambda([US_LAMBDA_CAPTURE_THIS](FMenuBuilder& InBuilder){
            InBuilder.BeginSection("UnrealSharp", FText::FromString("UnrealSharp"));
            InBuilder.AddSubMenu(
				FText::FromString("UnrealSharp"),
				FText::FromName("Unreal Sharp Tools"),				
				FNewMenuDelegate::CreateLambda([US_LAMBDA_CAPTURE_THIS](FMenuBuilder& SubMemuBuilder)
					{
						SubMemuBuilder.AddMenuEntry(
							FText::FromString("Export C++ Database"),
							FText::FromString(
								FString::Printf(
									TEXT("Export Unreal C++ types database file to %s,\nIt is recommended that you execute this command once after adding the C++ BlueprintCallable function to expose your new interface to C#."), 
									*UnrealSharp::FUnrealSharpPaths::GetDefaultUnrealCppDatabaseFilePath()
									)
								),
							FSlateIcon(FAppStyle::GetAppStyleSetName(), "Icons.C++"),
							FUIAction(FExecuteAction::CreateRaw(this, &FUnrealSharpEditorModule::OnExportUnrealCppDatabase, true))
						);

						SubMemuBuilder.AddMenuEntry(
							FText::FromString("Export Blueprint Database"),
							FText::FromString(
								FString::Printf(
									TEXT("Force export Unreal Blueprint types database file to %s[Need Enable Blueprint Binding Support],\nIf Blueprint binding support is turned on, you should execute this command to ensure that the binding code on the C# side matches the real data after the blueprint class, blueprint structure, blueprint enumeration, etc. accessed in C# changes. "),
									*UnrealSharp::FUnrealSharpPaths::GetDefaultUnrealBlueprintDatabaseFilePath()
									)
								),
							FSlateIcon(FAppStyle::GetAppStyleSetName(), "Kismet.Tabs.BlueprintDefaults"),
							FUIAction(FExecuteAction::CreateRaw(this, &FUnrealSharpEditorModule::OnExportBlueprintDatabase, true))
						);

						SubMemuBuilder.AddSeparator();

						SubMemuBuilder.AddMenuEntry(
							FText::FromString("Export Database"),
							FText::FromString("Automatically export all C# binding databases for you"),
							FSlateIcon(),
							FUIAction(FExecuteAction::CreateRaw(this, &FUnrealSharpEditorModule::OnAutoExportAllDatabase))
						);
					}
				),
				false,
				FSlateIcon(FAppStyle::GetAppStyleSetName(), "ClassIcon.BlueprintCore")
            );

            InBuilder.EndSection();
    }));

    FLevelEditorModule& LevelEditorModule = FModuleManager::LoadModuleChecked<FLevelEditorModule>("LevelEditor");
    LevelEditorModule.GetMenuExtensibilityManager()->AddExtender(MenuExtender);
}

void FUnrealSharpEditorModule::OnExportUnrealCppDatabase(bool bInStrongReminder)
{
	DoExportDatabase(UnrealSharp::FUnrealSharpPaths::GetDefaultUnrealCppDatabaseFilePath(), UnrealSharp::ETypeValidationFlags::WithNativeType, bInStrongReminder);
}

void FUnrealSharpEditorModule::OnExportBlueprintDatabase(bool bInStrongReminder)
{
	const UUnrealSharpSettings* Settings = GetDefault<UUnrealSharpSettings>();
	if (!Settings->bSupportBlueprintBinding)
	{
		if (bInStrongReminder)
		{
			FMessageDialog::Open(EAppMsgType::Ok, FText::FromString("Blueprint binding code can only be exported if Blueprint export support is turned on in settings."));
		}
		else
		{
			US_LOG_WARN(TEXT("Blueprint binding code can only be exported if Blueprint export support is turned on in settings."));
		}
		
		return;
	}

	DoExportDatabase(UnrealSharp::FUnrealSharpPaths::GetDefaultUnrealBlueprintDatabaseFilePath(), UnrealSharp::ETypeValidationFlags::WithBlueprintType, bInStrongReminder);
}

void FUnrealSharpEditorModule::DoExportDatabase(const FString& InDatabaseFilePath, UnrealSharp::ETypeValidationFlags InFlags, bool bInStrongReminder)
{	
	UnrealSharp::FTypeValidation TypeValidation;

	TUniquePtr<UnrealSharp::FTypeDefinitionDocument> Document = MakeUnique<UnrealSharp::FTypeDefinitionDocument>();
	Document->LoadFromEngine(&TypeValidation, InFlags);

	if (!Document->SaveToFile(*InDatabaseFilePath))
	{
		US_LOG_ERROR(TEXT("Failed Save Type Database File : %s"), *InDatabaseFilePath);
				
		if (bInStrongReminder)
		{
			FMessageDialog::Open(
				EAppMsgType::Ok, 
				FText::FromString(FText::FromString("Failed save tdb file to : ").ToString() + InDatabaseFilePath)
				);
		}		
	}
	else
	{
		US_LOG(TEXT("Type Database File saved successfully : %s"), *InDatabaseFilePath);
	}
}

void FUnrealSharpEditorModule::OnAutoExportAllDatabase()
{
	OnExportUnrealCppDatabase(true);

	const UUnrealSharpSettings* Settings = GetDefault<UUnrealSharpSettings>();
	if (Settings->bSupportBlueprintBinding)
	{
		OnExportBlueprintDatabase(true);
	}
}

void FUnrealSharpEditorModule::ExportDatabase(UnrealSharp::EUnrealTypeDatabaseExportFlags InFlags, bool bInStrongReminder)
{
	if (((int)InFlags & (int)UnrealSharp::EUnrealTypeDatabaseExportFlags::WITH_CPP) != 0)
	{
		OnExportUnrealCppDatabase(bInStrongReminder);
	}

	if (((int)InFlags & (int)UnrealSharp::EUnrealTypeDatabaseExportFlags::WITH_Blueprint) != 0)
	{
		const UUnrealSharpSettings* Settings = GetDefault<UUnrealSharpSettings>();
		if (Settings->bSupportBlueprintBinding)
		{
			OnExportBlueprintDatabase(bInStrongReminder);
		}		
	}
}

int FUnrealSharpEditorModule::LaunchExternalProcess(const FString& InExecutablePath, const FString& InCommandArgument)
{
	const bool bLaunchDetached = false;
	const bool bLaunchHidden = true;
	const bool bLaunchReallyHidden = bLaunchHidden;

	int ReturnCode = 0;
	void* PipeRead = nullptr;
	void* PipeWrite = nullptr;

	verify(FPlatformProcess::CreatePipe(PipeRead, PipeWrite));

	FProcHandle ProcessHandle = FPlatformProcess::CreateProc(
		*InExecutablePath, 
		*InCommandArgument, 
		bLaunchDetached, 
		bLaunchHidden, 
		bLaunchReallyHidden, 
		nullptr, 
		0, 
		nullptr, 
		PipeWrite, 
		nullptr, 
		nullptr
	);

	if (ProcessHandle.IsValid())
	{
		TArray<uint8> BinaryFileContent;
		while (FPlatformProcess::IsProcRunning(ProcessHandle))
		{
			FPlatformProcess::Sleep(0.01);

			TArray<uint8> BinaryData;
			FPlatformProcess::ReadPipeToArray(PipeRead, BinaryData);
			if (BinaryData.Num() > 0)
			{
				BinaryFileContent.Append(MoveTemp(BinaryData));
			}
		}

		TArray<uint8> BinaryData;
		FPlatformProcess::ReadPipeToArray(PipeRead, BinaryData);
		if (BinaryData.Num() > 0)
		{
			BinaryFileContent.Append(MoveTemp(BinaryData));
		}

		FPlatformProcess::GetProcReturnCode(ProcessHandle, &ReturnCode);
		
		FPlatformProcess::CloseProc(ProcessHandle);

		BinaryFileContent.Add(0);

		if (ReturnCode == 0)
		{
			US_LOG(TEXT("%s"), ANSI_TO_TCHAR((char*)BinaryFileContent.GetData()));
		}
		else
		{
			US_LOG_ERROR(TEXT("exit code: %d\n%s"), ReturnCode, ANSI_TO_TCHAR((char*)BinaryFileContent.GetData()));
		}		

		FPlatformProcess::ClosePipe(PipeRead, PipeWrite);

		return ReturnCode;
	}
	else
	{
		US_LOG_ERROR(TEXT("Failed to launch %s"), *InExecutablePath);

		FPlatformProcess::ClosePipe(PipeRead, PipeWrite);
	}

	return -1;
}

void FUnrealSharpEditorModule::RefreshCSharpImportBlueprintAssets(bool bForceRecreate)
{	
	FString ManagedDirectory = UnrealSharp::FUnrealSharpPaths::GetUnrealSharpManagedLibraryDir();

	TSharedPtr<UnrealSharp::FCSharpBlueprintImportDatabase> NewDatabase = MakeShared<UnrealSharp::FCSharpBlueprintImportDatabase>();
	TSharedPtr<UnrealSharp::FCSharpBlueprintImportDatabase> ImportDatabase = MakeShared<UnrealSharp::FCSharpBlueprintImportDatabase>();

	{
		FScopedDurationTimeLogger RecordCheckDirectory(TEXT("refresh import database"));
		
		ImportDatabase->LoadFromFile(*ImportDatabasePath);
		NewDatabase->LoadFromDirectory(*ManagedDirectory);
	}	

	if (bForceRecreate || *NewDatabase != *ImportDatabase)
	{
		US_LOG(TEXT("C# database is changed, reimport them now."));
		FScopedDurationTimeLogger RecordCheckDirectory(TEXT("reimport C# types"));

		if (ForceReloadCSharpTypes())
		{
			ImportDatabase = MoveTemp(NewDatabase);

			UnrealSharp::FUnrealSharpPaths::EnsureUnrealSharpIntermediateDirExists();

			ImportDatabase->SaveToFile(*ImportDatabasePath);
		}
	}
}

bool FUnrealSharpEditorModule::ForceReloadCSharpTypes()
{
	TArray<FString> Result;
	IFileManager& FileManager = IFileManager::Get();

	FString ManagedDirectory = UnrealSharp::FUnrealSharpPaths::GetUnrealSharpManagedLibraryDir();
	FileManager.FindFiles(Result, *ManagedDirectory, TEXT(".tdb"));

	TSharedPtr<UnrealSharp::FTypeDefinitionDocument> Document = MakeShared<UnrealSharp::FTypeDefinitionDocument>();

	for (auto& FileName : Result)
	{
		FString FullPath = FPaths::Combine(ManagedDirectory, FileName);

		TUniquePtr<UnrealSharp::FTypeDefinitionDocument> ThisDocument = MakeUnique<UnrealSharp::FTypeDefinitionDocument>();

		if (!ThisDocument->LoadFromFile(*FullPath))
		{
			US_LOG_ERROR(TEXT("Failed load file:%s"), *FileName);
			return false;
		}

		Document->Merge(MoveTemp(*ThisDocument));
	}

	TUniquePtr<UnrealSharp::FCSharpBlueprintGenerator> Generator = MakeUnique<UnrealSharp::FCSharpBlueprintGenerator>(Document);

	Generator->Process();

	return true;
}

void FUnrealSharpEditorModule::OnPreBeginPIE(bool bIsSimulating)
{

}

void FUnrealSharpEditorModule::OnEndPIE(bool bIsSimulating)
{

}

void FUnrealSharpEditorModule::OnMainFrameCreationFinished(TSharedPtr<SWindow> InRootWindow, bool bIsRunningStartupDialog)
{
	InRootWindow->GetOnWindowActivatedEvent().AddRaw(this, &FUnrealSharpEditorModule::OnMainFrameWindowActivated);	
}

void FUnrealSharpEditorModule::OnMainFrameWindowActivated()
{
	RefreshCSharpImportBlueprintAssets();	
}

