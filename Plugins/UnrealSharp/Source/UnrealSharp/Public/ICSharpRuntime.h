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

class UCSharpClass;

namespace UnrealSharp
{
    class ICSharpMethod;
    class ICSharpType;
    class ICSharpGCHandle;
    class IUnrealFunctionInvokeRedirector;
    class ICSharpMethodInvocation;
    class IPropertyMarshaller;
    class ICSharpObjectTable;
    class ICSharpLibraryAccessor;

    /*
    * This represents a C# runtime, which may be CoreCLR, Mono, or of course a virtual machine implemented by yourself.
    * The current ICSharpRuntime exposed by UnrealSharp is transparent to the outside world. 
    * Although it is currently only implemented based on Mono, of course there may be others in the future.
    */
    class UNREALSHARP_API ICSharpRuntime : public IRefCountedObject
    {    
    public:
        virtual ~ICSharpRuntime() = default;
    public:
        // Initialize runtime
        virtual bool                                    Initialize() = 0;

        // stop runtime running
        virtual void                                    Shutdown() = 0;

    public:
        // get runtime type name
        virtual const FName&                            GetRuntimeType() const = 0;

        // find a type in assembly
        virtual TSharedPtr<ICSharpType>                 LookupType(const FString& InAssemblyName, const FString& InNamespace, const FString& InName) = 0;

        // find a type in assembly
        virtual TSharedPtr<ICSharpType>                 LookupType(const FString& InAssemblyName, const FString& InFullName) = 0;

        // find a C# method in assembly
        virtual TSharedPtr<ICSharpMethod>               LookupMethod(const FString& InAssemblyName, const FString& InFullyQualifiedMethodName) = 0;        

        // find a C# method in a type
        virtual TSharedPtr<ICSharpMethod>               LookupMethod(ICSharpType* InType, const FString& InFullyQualifiedMethodName) = 0;
                
        // Create C# invocation from method
        virtual TSharedPtr<ICSharpMethodInvocation>     CreateCSharpMethodInvocation(TSharedPtr<ICSharpMethod> InMethod) = 0;        

        // create C# method invocation from assembly name and method signature
        virtual TSharedPtr<ICSharpMethodInvocation>     CreateCSharpMethodInvocation(const FString& InAssemblyName, const FString& InFullyQualifiedMethodName) = 0;

        // create a gc handle
        virtual TSharedPtr<ICSharpGCHandle>             CreateCSharpGCHandle(void* InCSharpObject, bool bInWeakReference = false) = 0;        

        // get property marshaller interface from Unreal property pointer
        virtual const IPropertyMarshaller*              GetPropertyMarshaller(const FProperty* InProperty) const = 0;

        // get property marshaller interface from Unreal property's FieldClass
        virtual const IPropertyMarshaller*              GetPropertyMarshaller(const FFieldClass* InFieldClass) const = 0;

        // force execute GC on runtime
        virtual void                                    ExecuteGarbageCollect(bool bFully) = 0;

        // get C# library accessor tools
        virtual ICSharpLibraryAccessor*                 GetCSharpLibraryAccessor() = 0;

        // get C# object table
        virtual ICSharpObjectTable*                     GetObjectTable() = 0;        
    };

    /*
    * This is the factory class of C# Runtime, which can be used to create C# Runtime and can also be queried. 
    * Only one C# Runtime can exist at the same time.
    */
    class UNREALSHARP_API FCSharpRuntimeFactory
    {
    public:
        // get C# runtime and increase reference counter
        static TRefCountPtr<ICSharpRuntime>             RetainCSharpRuntime();

        // release C# runtime and decrease reference counter
        static void                                     ReleaseCSharpRuntime(TRefCountPtr<ICSharpRuntime>&& InRuntime);

        // check is global C# runtime exists!
        static bool                                     IsGlobalCSharpRuntimeValid();

        // get global instance
        // it need IsGlobalCSharpRuntimeValid() == true
        // It does not increment the reference count, so you need to ensure its safety yourself
        static ICSharpRuntime*                          GetInstance();
    };
}
