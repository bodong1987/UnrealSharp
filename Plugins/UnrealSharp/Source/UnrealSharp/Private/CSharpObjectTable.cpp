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
#include "CSharpObjectTable.h"
#include "ICSharpMethodInvocation.h"
#include "Misc/ScopedExit.h"
#include "ICSharpRuntime.h"
#include "ICSharpLibraryAccessor.h"
#include "Classes/CSharpClass.h"
#include "Misc/UnrealSharpUtils.h"
#include "ICSharpType.h"
#include "Misc/StackMemory.h"
#include "Misc/ScopedCSharpMethodInvocation.h"
#include "Classes/UnrealSharpSettings.h"
#include "Misc/UnrealSharpLog.h"

namespace UnrealSharp
{
    FCSharpObjectFactory::FCSharpObjectFactory(TSharedPtr<ICSharpType> InType, TSharedPtr<ICSharpMethodInvocation> InInvocation) :
        Type(InType),
        Invocation(InInvocation)
    {
    }

    void* FCSharpObjectFactory::Create(UObject* InObject)
    {
        check(Type && Invocation);

        // just malloc the target object memory
        void* ObjectInstance = Type->NewObject();
        check(ObjectInstance);

        UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(Invocation);

        // let's call the constructor
        // which has a parameter is (IntPtr nativePtr)
        void* Result = InvocationInvoker.Invoke(ObjectInstance, &InObject);

        return ObjectInstance;
    }

    FCSharpObjectTable::FCSharpObjectTable(ICSharpRuntime* InRuntime) :
        Runtime(InRuntime)
    {
        const UUnrealSharpSettings* Settings = GetDefault<UUnrealSharpSettings>();
        bSupportBlueprintBinding = Settings->bSupportBlueprintBinding;
        RegisterDelegates();
    }

    FCSharpObjectTable::~FCSharpObjectTable()
    {
        CSharpObjectMapping.Empty();

        UnRegisterDelegates();
    }

    void FCSharpObjectTable::RegisterDelegates()
    {
        OnWorldCleanupHandle = FWorldDelegates::OnWorldCleanup.AddRaw(this, &FCSharpObjectTable::OnWorldCleanup);
        PostReachabilityAnalysisHandle = FCoreUObjectDelegates::PostReachabilityAnalysis.AddRaw(this, &FCSharpObjectTable::OnPostReachabilityAnalysis);
        PostGarbageCollectHandle = FCoreUObjectDelegates::GetPostGarbageCollect().AddRaw(this, &FCSharpObjectTable::OnPostGarbageCollect);
    }

    void FCSharpObjectTable::UnRegisterDelegates()
    {
        FWorldDelegates::OnWorldCleanup.Remove(OnWorldCleanupHandle);
        FCoreUObjectDelegates::PostReachabilityAnalysis.Remove(PostReachabilityAnalysisHandle);
        FCoreUObjectDelegates::GetPostGarbageCollect().Remove(PostGarbageCollectHandle);
    }

    void FCSharpObjectTable::BreakCSharpObjectConnection(FCSharpObjectHandle& InHandle)
    {
        if (InHandle.IsValid())
        {
            void* CSharpObject = InHandle.GetObject();

            if (CSharpObject != nullptr)
            {
                auto FastAccessor = Runtime->GetCSharpLibraryAccessor();
                check(FastAccessor);

                FastAccessor->BreakCSharpObjectNativeConnection(CSharpObject);
            }
        }
    }

    void FCSharpObjectTable::OnPostReachabilityAnalysis()
    {
        double TraceExternalRootsTime = 0.0;
        {
            SCOPE_SECONDS_COUNTER(TraceExternalRootsTime);

            for (decltype(CSharpObjectMapping)::TIterator It(CSharpObjectMapping); It; ++It)
            {
                UObject* ReferencedObject = It.Key();
                FCSharpObjectHandle& Handle = It.Value();

                if (!IsValid(ReferencedObject) || ReferencedObject->IsUnreachable())
                {
                    BreakCSharpObjectConnection(Handle);

                    It.RemoveCurrent();
                }
            }

            double CSharpGCTime = 0.0;
            {
                SCOPE_SECONDS_COUNTER(CSharpGCTime)
                
                Runtime->ExecuteGarbageCollect(true);
            }            
        }

        if (TraceExternalRootsTime > 0.0)
        {
            US_LOG(TEXT("FCSharpObjectTable::OnPostReachabilityAnalysis %g ms"), TraceExternalRootsTime * 1000.0);
        }
    }

    void FCSharpObjectTable::OnPostGarbageCollect()
    {

    }

    void FCSharpObjectTable::OnWorldCleanup(UWorld* InWorld, bool bSessionEnded, bool bCleanupResources)
    {
        check(InWorld);
        UPackage* Outermost = InWorld->GetOutermost();
        
        for (TMap<UObject*, FCSharpObjectHandle>::TIterator It(CSharpObjectMapping); It; ++It)
        {
            UObject* Object = It.Key();
            if (Object->IsIn(Outermost))
            {
                auto& Handle = It.Value();
                BreakCSharpObjectConnection(Handle);

                It.RemoveCurrent();
            }
        }
    }

    void* FCSharpObjectTable::GetCSharpObject(UObject* InObject)
    {
        if (InObject == nullptr)
        {
            return nullptr;
        }

        auto Ptr = CSharpObjectMapping.Find(InObject);

        if (Ptr != nullptr)
        {
            return (*Ptr).GetObject();
        }

        FCSharpObjectHandle handle = CreateCSharpObjectHandle(InObject);
        checkSlow(handle.IsValid());

        void* ObjectPtr = handle.GetObject();

        CSharpObjectMapping.Add(InObject, MoveTemp(handle));

        return ObjectPtr;
    }

    UObject* FCSharpObjectTable::GetUnrealObject(void* InCSharpObject)
    {
        return Runtime->GetCSharpLibraryAccessor()->GetUnrealObject(InCSharpObject);
    }

    FCSharpObjectHandle FCSharpObjectTable::CreateCSharpObjectHandle(UObject* InObject)
    {
        checkSlow(InObject);

        // find the first CSharpClass or native class
        UClass* ObjectClass = InObject->GetClass();
        while (ObjectClass != nullptr)
        {
            // 1. is native C++ UClass, so it always have a proxy C# class
            // 2. is a UCSharpClass, it is generated from C#
            // 3. if enable blueprint binding, also support blueprint C# proxy class
            if (FUnrealSharpUtils::IsNativeClass(ObjectClass) ||
                FUnrealSharpUtils::IsCSharpClass(ObjectClass) ||
                (bSupportBlueprintBinding && FUnrealSharpUtils::IsBlueprintClass(ObjectClass) && !FUnrealSharpUtils::IsCSharpInheritBlueprintClass(ObjectClass))
                )
            {
                break;
            }

            ObjectClass = ObjectClass->GetSuperClass();
        }

        check(ObjectClass != nullptr);

        void* ObjectPtr = CreateCSharpObject(ObjectClass, InObject);
        checkf(ObjectPtr != nullptr, TEXT("Failed create C# proxy object for unreal class:%s"), *ObjectClass->GetPathName());

        FCSharpObjectHandle handle(Runtime, ObjectPtr, false);
        return handle;
    }

    void* FCSharpObjectTable::CreateCSharpObject(UClass* InClass, UObject* InObject)
    {
        auto Ptr = CSharpObjectFactoryMapping.Find(InClass);

        if (Ptr != nullptr)
        {
            return Ptr->Create(InObject);
        }

        FString AssemblyName = FUnrealSharpUtils::GetAssemblyName(InClass);
        FString ClassFullPath = FUnrealSharpUtils::GetCSharpFullPath(InClass);

#if UE_BUILD_DEBUG
        // US_LOG(TEXT("Get CSharpObject:%s, ClassPathName:%s"), *InObject->GetName(), *InClass->GetPathName());
#endif

        TSharedPtr<ICSharpType> ClassType = Runtime->LookupType(AssemblyName, ClassFullPath);
        checkf(ClassType, TEXT("Failed find C# class %s in %s"), *ClassFullPath, *AssemblyName);
        
        const bool IsBlueprintLibrary = InClass->IsChildOf<UBlueprintFunctionLibrary>();
        FString ConstructorSignature = FString::Printf(TEXT("%s:.ctor (%s)"), *ClassFullPath, IsBlueprintLibrary?TEXT(""):TEXT("intptr"));
        TSharedPtr<ICSharpMethod> Method = Runtime->LookupMethod(ClassType.Get(), ConstructorSignature);

        // TSharedPtr<ICSharpMethod> Method = Runtime->LookupMethod(ClassType.Get(), TEXT(".ctor"), 1);

        checkf(Method, TEXT("Failed find a constructor with IntPtr on C# type:%s"), *ClassFullPath);

        TSharedPtr<ICSharpMethodInvocation> Invocation = Runtime->CreateCSharpMethodInvocation(Method);

        FCSharpObjectFactory FactoryRef = CSharpObjectFactoryMapping.Add(InClass, { ClassType, Invocation });
        return FactoryRef.Create(InObject);
    }
}
