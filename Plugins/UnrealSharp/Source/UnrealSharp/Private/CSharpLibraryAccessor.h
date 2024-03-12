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

#include "ICSharpLibraryAccessor.h"

namespace UnrealSharp
{
    class ICSharpRuntime;
    class ICSharpMethodInvocation;
    class FCSharpStructFactory;

    /*
    * Used to invoke C# code from C++
    * Provides entry points for calling some commonly used C# methods on the C++ side for easy use.
    * Here is the generic implementation
    */
    class UNREALSHARP_API FCSharpLibraryAccessor : public ICSharpLibraryAccessor
    {
    public:
        FCSharpLibraryAccessor(ICSharpRuntime* InRuntime);

        virtual void                                                BreakCSharpObjectNativeConnection(void* InCSharpObject) override;
        virtual UObject*                                            GetUnrealObject(void* InCSharpObject) override;
        virtual void                                                BeforeObjectConstructor(void* InCSharpObject, const FObjectInitializer& InObjectInitializer) override;
        virtual void                                                PostObjectConstructor(void* InCSharpObject) override;

        virtual void*                                               CreateCSharpStruct(const void* InUnrealStructPtr, const UScriptStruct* InStruct) override;
        virtual void                                                StructToNative(const UScriptStruct* InStruct, void* InNativePtr, const void* InCSharpStructPtr) override;
        virtual void*                                               CreateCSharpCollection(void* InAddressOfCollection, FProperty* InCollectionProperty) override;
        virtual void                                                CopyFromCSharpCollection(void* InAddressOfCollection, FProperty* InCollectionProperty, void* InCSharpCollection) override;        
        virtual void*                                               CreateCSharpSoftObjectPtr(void* InAddressOfSoftObjectPtr, FSoftObjectProperty* InSoftObjectProperty) override;
        virtual void                                                CopySoftObjectPtr(void* InDestinationAddress, const void* InSourceObjectInterface) override;
        virtual void*                                               CreateCSharpSoftClassPtr(void* InAddressOfSoftClassPtr, FSoftClassProperty* InSoftClassProperty) override;
        virtual void                                                CopySoftClassPtr(void* InDestinationAddress, const void* InSourceObjectInterface) override;

    protected:
        TSharedPtr<FCSharpStructFactory>                            QueryStructFactory(const UScriptStruct* InStruct);

    protected:
        ICSharpRuntime* Runtime;

        // object series
        TSharedPtr<ICSharpMethodInvocation>                         GetNativePtrInvocation;
        TSharedPtr<ICSharpMethodInvocation>                         DisconnectToNativeInvocation;
        TSharedPtr<ICSharpMethodInvocation>                         BeforeObjectConstructorInvocation;
        TSharedPtr<ICSharpMethodInvocation>                         PostObjectConstructorInvocation;

        // Collection series
        TSharedPtr<ICSharpMethodInvocation>                         CreateArrayInvocation;
        TSharedPtr<ICSharpMethodInvocation>                         WriteArrayInvocation;
        TSharedPtr<ICSharpMethodInvocation>                         CreateSetInvocation;
        TSharedPtr<ICSharpMethodInvocation>                         WriteSetInvocation;
        TSharedPtr<ICSharpMethodInvocation>                         CreateMapInvocation;
        TSharedPtr<ICSharpMethodInvocation>                         WriteMapInvocation;
        TSharedPtr<ICSharpMethodInvocation>                         CreateSoftObjectInvocation;
        TSharedPtr<ICSharpMethodInvocation>                         WriteSoftObjectPtrInvocation;
        TSharedPtr<ICSharpMethodInvocation>                         CreateSoftClassInvocation;
        TSharedPtr<ICSharpMethodInvocation>                         WriteSoftClassPtrInvocation;

        
    private:
        // Struct Factory caches
        TMap<const UStruct*, TSharedPtr<FCSharpStructFactory>>      StructFactories;
    };
}
