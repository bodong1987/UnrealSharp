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

#include "Modules/ModuleManager.h"

namespace UnrealSharp
{
    class FCSharpBlueprintImportDatabase;
    enum class ETypeValidationFlags;

    enum class EUnrealTypeDatabaseExportFlags
    {
        WITH_CPP = 1 << 0,
        WITH_Blueprint = 1 << 1
    };

    inline EUnrealTypeDatabaseExportFlags operator | (EUnrealTypeDatabaseExportFlags InLeft, EUnrealTypeDatabaseExportFlags InRight)
    {
        return (EUnrealTypeDatabaseExportFlags)(((int)InLeft) | ((int)InRight));
    }
}

/*
* The main entrance to the editor module, the extension ui, 
* is responsible for detecting whether C# data needs to be re-imported when the level editor gains focus. 
* Export C# binding code, etc.
*/
class UNREALSHARPEDITOR_API FUnrealSharpEditorModule : public IModuleInterface
{
public:
    FUnrealSharpEditorModule();

    virtual void                StartupModule() override;
    virtual void                ShutdownModule() override;

    int                         LaunchExternalProcess(const FString& InExecutablePath, const FString& InCommandArgument);
    
    void                        RefreshCSharpImportBlueprintAssets(bool bForceRecreate = false);

    bool                        ForceReloadCSharpTypes();
    void                        ExportDatabase(UnrealSharp::EUnrealTypeDatabaseExportFlags InFlags, bool bInStrongReminder);

private:
    void                        AddExportDatabaseMenu();
    void                        OnExportUnrealCppDatabase(bool bInStrongReminder);
    void                        OnExportBlueprintDatabase(bool bInStrongReminder);
    void                        OnAutoExportAllDatabase();
    void                        DoExportDatabase(const FString& InDatabaseFilePath, UnrealSharp::ETypeValidationFlags InFlags, bool bInStrongReminder);
    void                        OnPreBeginPIE(bool bIsSimulating);
    void                        OnEndPIE(bool bIsSimulating);
    void                        OnMainFrameCreationFinished(TSharedPtr<SWindow> InRootWindow, bool bIsRunningStartupDialog);
    void                        OnMainFrameWindowActivated();

private:
    FDelegateHandle             PreBeginPIEHandle, EndPIEHandle;
    FString                     ImportDatabasePath;    
};

