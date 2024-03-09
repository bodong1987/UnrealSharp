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

#include "IUnrealFunctionInvokeRedirector.h"

class UCSharpClass;
struct FCSharpFunctionData;

namespace UnrealSharp
{
    class ICSharpMethodInvocation;
    class ICSharpRuntime;
    class IPropertyMarshaller;
    class FUnrealFunctionInvokeRedirector;
    class FUnrealFunctionMarshallerLinker;
    struct FStackMemory;

    /*
    * Save the basic information of UFunction and corresponding C#Method, which will be used to pack and unpack parameters.
    */
    class UNREALSHARP_API FUnrealFunctionMarshallerLinker
    {
    public:
        struct FPropertyMarshallerInfo
        {
            FProperty* Property = nullptr;
            const IPropertyMarshaller* MarshallerPtr = nullptr;            
            int OffsetInTempParameterBuffer = 0;
            bool bPassByReference = false;

            bool IsValid() const
            {
                return Property != nullptr && MarshallerPtr != nullptr;
            }
        };

        struct FPropertyInfo
        {
            FProperty* Property = nullptr;
            TSharedPtr<FPropertyMarshallerInfo> MarshallerInfoPtr;
        };

        FUnrealFunctionMarshallerLinker(ICSharpRuntime* InRuntime, UFunction* InFunction,const FCSharpFunctionData* InFunctionData);

        int GetTempParameterSize() const { return TempParameterSize; }
        int GetCSharpParameterCount() const { return MarshallerQueue.Num(); }
        int GetUnrealFunctionParameterCount() const{ return PropertyQueue.Num(); }
        
        void BeginInvoke(
            ICSharpMethodInvocation* InInvocation,
            const FStackMemory& InParameterBuffer,
            const FStackMemory& InTempInteropParameterPointers,
            const FStackMemory& InUnrealParameterReferencePointers,
            UObject* Context,
            FFrame& Stack,
            RESULT_DECL
            ) const;

        void FinishInvoke(const FStackMemory& InParameterBuffer) const;

        void CopyReferenceParameters(
            const FStackMemory& InTempInteropParameterPointers,
            const FStackMemory& InUnrealParameterReferencePointers
            ) const;

        static void** GetUnrealParameterReferencePointerAddress(const FStackMemory& InUnrealParameterReferencePointers, int InIndexInProperties);
        static void** GetTempParameterPointerAddress(const FStackMemory& InTempInteropParameterPointers, int InOffsetInTempParameterBuffer);

    public:
        static const FString WorldContextName;

        friend class FUnrealFunctionInvokeRedirector;
    private:
        TArray<TSharedPtr<FPropertyMarshallerInfo>>                         MarshallerQueue;
        TSharedPtr<FPropertyMarshallerInfo>                                 ReturnValueMarshaller;
        TArray<FPropertyInfo>                                               PropertyQueue;
        int                                                                 TempParameterSize = 0;
    };

    /*
    * Forward the function call for UFunction* to C# for execution, and obtain the return value and the value of the parameters passed by reference.
    */
    class UNREALSHARP_API FUnrealFunctionInvokeRedirector : public IUnrealFunctionInvokeRedirector
    {
    public:
        FUnrealFunctionInvokeRedirector(
            ICSharpRuntime* InRuntime, 
            UCSharpClass* InClass, 
            UFunction* InFunction, 
            const FCSharpFunctionData* InFunctionData,
            TSharedPtr<ICSharpMethodInvocation> InInvocation
            );
        ~FUnrealFunctionInvokeRedirector();

        virtual const UFunction* GetFunction() const override;
        virtual void Invoke(UObject* Context, FFrame& Stack, RESULT_DECL) override;

    private:
        friend class FScopedUnrealFunctionParameters;

        ICSharpRuntime*                                                      Runtime;
        UFunction*                                                           Function;
        const FCSharpFunctionData*                                           FunctionData;
        TSharedPtr<ICSharpMethodInvocation>                                  Invocation;                
        FUnrealFunctionMarshallerLinker                                      Linker;
    };
}

