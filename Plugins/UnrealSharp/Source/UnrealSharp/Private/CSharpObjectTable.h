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

#include "CSharpObjectHandle.h"
#include "ICSharpObjectTable.h"

namespace UnrealSharp
{
    class ICSharpMethod;
    class ICSharpType;
    class ICSharpMethodInvocation;

    // used to create C# proxy object of UObject*
    struct UNREALSHARP_API FCSharpObjectFactory
    {
    public:
        FCSharpObjectFactory(const TSharedPtr<ICSharpType>& InType, const TSharedPtr<ICSharpMethodInvocation>& InInvocation);

        void*                                               Create(UObject* InObject) const;

    private:
        TSharedPtr<ICSharpType>                             Type;
        TSharedPtr<ICSharpMethodInvocation>                 Invocation;
    };

    /*
    * Used to save the mapping of UnrealObject (UObject*) to C# Object, and also create a proxy for UObject* on the C# side.
    * It is also responsible for the coordination of the memory management of Unreal Object and the memory management of C# objects.
    * As long as the Unreal Object still exists, the C# Object will definitely exist.
    * This is achieved through GCHandle.
    * The lifetime of the C# object is determined by the lifetime of the Unreal Object.
    * After the Unreal Object is garbage collected, the C# proxy object will be removed from GCHandle and its bound NativePtr will be empty.
    */
    class UNREALSHARP_API FCSharpObjectTable : public ICSharpObjectTable
    {
    public:
        FCSharpObjectTable(ICSharpRuntime* InRuntime);
        virtual ~FCSharpObjectTable() override;

        virtual void*                                       GetCSharpObject(UObject* InObject) override;
        virtual UObject*                                    GetUnrealObject(void* InCSharpObject) override;

    protected:
        // if UObject is garbage, break C# UObject connections
        void                                                OnPostReachabilityAnalysis();
        void                                                OnPostGarbageCollect();
        void                                                OnWorldCleanup(UWorld* InWorld, bool bSessionEnded, bool bCleanupResources);

    protected:
        void                                                RegisterDelegates();
        void                                                UnRegisterDelegates() const;

        // make C# UObject disconnect from Native UObject*
        void                                                BreakCSharpObjectConnection(const FCSharpObjectHandle& InHandle) const;

        FCSharpObjectHandle                                 CreateCSharpObjectHandle(UObject* InObject);
        void*                                               CreateCSharpObject(UClass* InClass, UObject* InObject);

    protected:
        ICSharpRuntime* Runtime;
        TMap<UObject*, FCSharpObjectHandle>                 CSharpObjectMapping;

        FDelegateHandle                                     OnWorldCleanupHandle;
        FDelegateHandle                                     PostReachabilityAnalysisHandle;
        FDelegateHandle                                     PostGarbageCollectHandle;

        TMap<UClass*, FCSharpObjectFactory>                 CSharpObjectFactoryMapping;
        bool                                                bSupportBlueprintBinding = true;
    };
}
