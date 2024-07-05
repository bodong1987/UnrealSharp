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
#include "CSharpRuntimeBase.h"
#include "CSharpObjectTable.h"
#include "CSharpLibraryAccessor.h"
#include "Misc/UnrealSharpUtils.h"
#include "ICSharpMethodInvocation.h"
#include "Misc/ScopedCSharpMethodInvocation.h"
#include "Misc/UnrealSharpPaths.h"
#include "Misc/UnrealInteropFunctions.h"

namespace UnrealSharp
{
    bool FCSharpRuntimeBase::Initialize()
    {
        if (!InitializeInternal())
        {
            return false;
        }

        PostInitialized();

        InvokeMain();

        return true;
    }

    void FCSharpRuntimeBase::Shutdown()
    {
        BeforeShutdown();

        ShutdownInternal();
    }

    void FCSharpRuntimeBase::PostInitialized()
    {
        CSharpLibraryAccessorPtr = CreateCSharpLibraryAccessor();
        ObjectTablePtr = CreateCSharpObjectTable();
    }

    void FCSharpRuntimeBase::BeforeShutdown()
    {
        CSharpLibraryAccessorPtr.Reset();
        ObjectTablePtr.Reset();
    }

    TSharedPtr<ICSharpLibraryAccessor> FCSharpRuntimeBase::CreateCSharpLibraryAccessor()
    {
        return MakeShared<FCSharpLibraryAccessor>(this);
    }

    TSharedPtr< ICSharpObjectTable> FCSharpRuntimeBase::CreateCSharpObjectTable()
    {
        return MakeShared<FCSharpObjectTable>(this);
    }

    ICSharpLibraryAccessor* FCSharpRuntimeBase::GetCSharpLibraryAccessor()
    {
        return CSharpLibraryAccessorPtr.Get();
    }

    ICSharpObjectTable* FCSharpRuntimeBase::GetObjectTable()
    {
        return ObjectTablePtr.Get();
    }

    TSharedPtr<ICSharpType> FCSharpRuntimeBase::LookupType(const FString& InAssemblyName, const FString& InFullName)
    {
        int Index = 0;
        if (InFullName.FindLastChar('.', Index))
        {
            return LookupType(InAssemblyName, InFullName.Left(Index), InFullName.Right(InFullName.Len()-Index-1));
        }

        static const FString Z_Empty = TEXT("");
        return LookupType(InAssemblyName, Z_Empty, InFullName);
    }

    void FCSharpRuntimeBase::InvokeMain()
    {
        const auto Invocation = CreateCSharpMethodInvocation(*FUnrealSharpUtils::UnrealSharpEngineAssemblyName, TEXT("UnrealSharp.UnrealEngine.Main.UnrealSharpEntry:Main (intptr,intptr)"));

        checkf(Invocation, TEXT("Failed find method UnrealSharpEntry:Main"));

        US_SCOPED_CSHARP_METHOD_INVOCATION(Invocation);

        const TCHAR* CmdLine = FCommandLine::Get();
        FString CommandArguments = FString::Printf(TEXT("--app=\"%s\""), CmdLine);
        CommandArguments.Append(TEXT(" "));
        CommandArguments.Append(FString::Printf(TEXT("--projectDir=\"%s\""), *FPaths::ProjectDir()));
        CommandArguments.Append(TEXT(" "));
        CommandArguments.Append(FString::Printf(TEXT("--unrealsharpIntermediateDir=\"%s\""), *FUnrealSharpPaths::GetUnrealSharpIntermediateDir()));
        
        FUnrealInteropFunctionsInfo* InteropInfoPtr = FUnrealInteropFunctions::GetInteropFunctionsInfoPtr();
        const void* CommandArgumentStringPtr = *CommandArguments;

        InvocationInvoker.Invoke(nullptr, &InteropInfoPtr, &CommandArgumentStringPtr);
    }
}
