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
#include "CSharpLibraryAccessor.h"
#include "ICSharpRuntime.h"
#include "ICSharpMethodInvocation.h"
#include "Misc/ScopedExit.h"
#include "Misc/UnrealSharpUtils.h"
#include "Classes/CSharpStruct.h"
#include "CSharpStructFactory.h"
#include "Misc/StackMemory.h"
#include "Misc/ScopedCSharpMethodInvocation.h"

namespace UnrealSharp
{
    FCSharpLibraryAccessor::FCSharpLibraryAccessor(ICSharpRuntime* InRuntime) :
        Runtime(InRuntime)
    {
        check(InRuntime);

        DisconnectToNativeInvocation = FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(InRuntime, TEXT("UObject"), TEXT("DisconnectFromNative ()"));
        GetNativePtrInvocation = FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(InRuntime, TEXT("UObject"), TEXT("GetNativePtr ()"));
        BeforeObjectConstructorInvocation = FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(InRuntime, TEXT("UObject"), TEXT("BeforeObjectConstructorInternal (intptr)"));
        PostObjectConstructorInvocation = FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(InRuntime, TEXT("UObject"), TEXT("PostObjectConstructor ()"));
        CreateArrayInvocation = FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(InRuntime, TEXT("GenericObjectFactory"), TEXT("CreateArray (intptr,intptr)"));
        WriteArrayInvocation = FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(InRuntime, TEXT("GenericObjectFactory"), TEXT("WriteArray (intptr,intptr,System.Collections.IEnumerable)"));
        CreateSetInvocation = FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(InRuntime, TEXT("GenericObjectFactory"), TEXT("CreateSet (intptr,intptr)"));
        WriteSetInvocation = FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(InRuntime, TEXT("GenericObjectFactory"), TEXT("WriteSet (intptr,intptr,System.Collections.IEnumerable)"));
        CreateMapInvocation = FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(InRuntime, TEXT("GenericObjectFactory"), TEXT("CreateMap (intptr,intptr)"));
        WriteMapInvocation = FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(InRuntime, TEXT("GenericObjectFactory"), TEXT("WriteMap (intptr,intptr,System.Collections.IEnumerable)"));
        CreateSoftObjectInvocation = FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(InRuntime, TEXT("GenericObjectFactory"), TEXT("CreateSoftObjectPtr (intptr,intptr)"));
        WriteSoftObjectPtrInvocation = FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(InRuntime, TEXT("GenericObjectFactory"), TEXT("WriteSoftObjectPtr (intptr,UnrealSharp.UnrealEngine.ISoftObjectPtr)"));
        CreateSoftClassInvocation = FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(InRuntime, TEXT("GenericObjectFactory"), TEXT("CreateSoftClassPtr (intptr,intptr)"));
        WriteSoftClassPtrInvocation = FUnrealSharpUtils::BindUnrealEngineCSharpMethodChecked(InRuntime, TEXT("GenericObjectFactory"), TEXT("WriteSoftClassPtr (intptr,UnrealSharp.UnrealEngine.ISoftClassPtr)"));
    }

    void FCSharpLibraryAccessor::BreakCSharpObjectNativeConnection(void* InCSharpObject)
    {
        UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(DisconnectToNativeInvocation);

        DisconnectToNativeInvocationInvoker.Invoke(InCSharpObject);
    }

    UObject* FCSharpLibraryAccessor::GetUnrealObject(void* InCSharpObject)
    {
        UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(GetNativePtrInvocation);

        UObject* Result = GetNativePtrInvocationInvoker.Invoke<UObject*>(InCSharpObject);
        
        return Result;
    }

    void FCSharpLibraryAccessor::BeforeObjectConstructor(void* InCSharpObject, const FObjectInitializer& InObjectInitializer)
    {
        UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(BeforeObjectConstructorInvocation);
        
        const FObjectInitializer* ObjectInitializerPtr = &InObjectInitializer;

        BeforeObjectConstructorInvocationInvoker.Invoke(InCSharpObject, &ObjectInitializerPtr);
    }

    void FCSharpLibraryAccessor::PostObjectConstructor(void* InCSharpObject)
    {
        UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(PostObjectConstructorInvocation);
        PostObjectConstructorInvocationInvoker.Invoke(InCSharpObject);
    }

    TSharedPtr<FCSharpStructFactory> FCSharpLibraryAccessor::QueryStructFactory(const UScriptStruct* InStruct)
    {
        const UScriptStruct* const Struct = InStruct;

        TSharedPtr<FCSharpStructFactory>* Factory = StructFactories.Find(Struct);

        if (Factory == nullptr)
        {
            FString AssemblyName = FUnrealSharpUtils::GetAssemblyName(Struct);
            FString ClassPath = FUnrealSharpUtils::GetCSharpFullPath(Struct);

            TSharedPtr<FCSharpStructFactory> FactoryPtr = MakeShared<FCSharpStructFactory>(Runtime, AssemblyName, ClassPath);
            Factory = &StructFactories.Add(Struct, FactoryPtr);
        }

        return *Factory;
    }

    void* FCSharpLibraryAccessor::CreateCSharpStruct(const void* InUnrealStructPtr, const UScriptStruct* InStruct)
    {
        check(InUnrealStructPtr);

        auto Factory = QueryStructFactory(InStruct);

        check(Factory != nullptr);

        return Factory->FromNative(InUnrealStructPtr);
    }

    void FCSharpLibraryAccessor::StructToNative(const UScriptStruct* InStruct, void* InNativePtr, const void* InCSharpStructPtr)
    {
        check(InNativePtr);

        auto Factory = QueryStructFactory(InStruct);

        check(Factory != nullptr);

        Factory->ToNative(InNativePtr, InCSharpStructPtr);
    }

    void* FCSharpLibraryAccessor::CreateCSharpCollection(void* InAddressOfCollection, FProperty* InCollectionProperty)
    {
        check(InAddressOfCollection);
        check(InCollectionProperty);

        TSharedPtr<ICSharpMethodInvocation> Invocation;

        if (InCollectionProperty->IsA<FArrayProperty>())
        {
            Invocation = CreateArrayInvocation;
        }
        else if (InCollectionProperty->IsA<FSetProperty>())
        {
            Invocation = CreateSetInvocation;
        }
        else if (InCollectionProperty->IsA<FMapProperty>())
        {
            Invocation = CreateMapInvocation;
        }

        checkf(Invocation, TEXT("Unsupported property, it is not an valid collection property!"));

        UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(Invocation);

        return InvocationInvoker.Invoke(nullptr, &InAddressOfCollection, &InCollectionProperty);
    }

    void FCSharpLibraryAccessor::CopyFromCSharpCollection(void* InAddressOfCollection, FProperty* InCollectionProperty, void* InCSharpCollection)
    {
        check(InAddressOfCollection);
        check(InCollectionProperty);
        check(InCSharpCollection);

        TSharedPtr<ICSharpMethodInvocation> Invocation;

        if (InCollectionProperty->IsA<FArrayProperty>())
        {
            Invocation = WriteArrayInvocation;
        }
        else if (InCollectionProperty->IsA<FSetProperty>())
        {
            Invocation = WriteSetInvocation;
        }
        else if (InCollectionProperty->IsA<FMapProperty>())
        {
            Invocation = WriteMapInvocation;
        }

        UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(Invocation);

        InvocationInvoker.Invoke(nullptr, &InAddressOfCollection, &InCollectionProperty, InCSharpCollection);
    }

    void* FCSharpLibraryAccessor::CreateCSharpSoftObjectPtr(void* InAddressOfSoftObjectPtr, FSoftObjectProperty* InSoftObjectProperty)
    {
        check(InAddressOfSoftObjectPtr);
        check(InSoftObjectProperty);

        UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(CreateSoftObjectInvocation);

        return CreateSoftObjectInvocationInvoker.Invoke(nullptr, &InAddressOfSoftObjectPtr, &InSoftObjectProperty);
    }

    void FCSharpLibraryAccessor::CopySoftObjectPtr(void* InDestinationAddress, const void* InSourceObjectInterface)
    {
        check(InDestinationAddress);
        check(InSourceObjectInterface);

        UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(WriteSoftObjectPtrInvocation);

        WriteSoftObjectPtrInvocationInvoker.Invoke(nullptr, &InDestinationAddress, InSourceObjectInterface);
    }

    void* FCSharpLibraryAccessor::CreateCSharpSoftClassPtr(void* InAddressOfSoftClassPtr, FSoftClassProperty* InSoftClassProperty)
    {
        check(InAddressOfSoftClassPtr);
        check(InSoftClassProperty);

        UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(CreateSoftClassInvocation);

        return CreateSoftClassInvocationInvoker.Invoke(nullptr, &InAddressOfSoftClassPtr, &InSoftClassProperty);
    }

    void FCSharpLibraryAccessor::CopySoftClassPtr(void* InDestinationAddress, const void* InSourceObjectInterface)
    {
        check(InDestinationAddress);
        check(InSourceObjectInterface);

        UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(WriteSoftClassPtrInvocation);

        WriteSoftClassPtrInvocationInvoker.Invoke(nullptr, &InDestinationAddress, InSourceObjectInterface);
    }

}
