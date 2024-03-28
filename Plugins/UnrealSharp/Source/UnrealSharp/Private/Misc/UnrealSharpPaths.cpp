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
#include "Misc/UnrealSharpPaths.h"

namespace UnrealSharp
{
    const FString FUnrealSharpPaths::UnrealCppDatabaseFileName = TEXT("NativeTypeDefinition.tdb");
    const FString FUnrealSharpPaths::UnrealBlueprintDatabaseFileName = TEXT("BlueprintTypeDefinition.tdb");

    static inline FString GetUnrealSharpIntermediateDirInner()
    {
        FString Path = FPaths::ConvertRelativePathToFull(FPaths::Combine(FPaths::ProjectIntermediateDir(), TEXT("UnrealSharp")));

        return Path;
    }

    FString FUnrealSharpPaths::GetUnrealSharpIntermediateDir()
    {
        FString Path = GetUnrealSharpIntermediateDirInner();

        if (!FPaths::DirectoryExists(Path))
        {
            FPlatformFileManager::Get().GetPlatformFile().CreateDirectoryTree(*Path);
        }

        return Path;
    }

    void FUnrealSharpPaths::EnsureUnrealSharpIntermediateDirExists()
    {
        FString Path = GetUnrealSharpIntermediateDirInner();

        if (!FPaths::DirectoryExists(Path))
        {
            FPlatformFileManager::Get().GetPlatformFile().CreateDirectoryTree(*Path);
        }
    }

    FString FUnrealSharpPaths::GetUnrealSharpManagedLibraryDir()
    {
        // MANAGED_DIRECTORYNAME is defined in Build.cs
        FString ManagedPath = FPaths::ConvertRelativePathToFull(FPaths::Combine(FPaths::ProjectDir(), TEXT("Managed/") TEXT(MANAGED_DIRECTORYNAME)));
        return ManagedPath;
    }

    FString FUnrealSharpPaths::GetDefaultUnrealCppDatabaseFilePath()
    {
        return FPaths::Combine(GetUnrealSharpIntermediateDirInner(), UnrealCppDatabaseFileName);
    }

    FString FUnrealSharpPaths::GetDefaultUnrealBlueprintDatabaseFilePath()
    {
        return FPaths::Combine(GetUnrealSharpIntermediateDirInner(), UnrealBlueprintDatabaseFileName);
    }
}

