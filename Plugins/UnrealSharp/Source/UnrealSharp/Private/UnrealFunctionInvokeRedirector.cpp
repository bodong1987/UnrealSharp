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
#include "UnrealFunctionInvokeRedirector.h"
#include "Misc/ScopedExit.h"
#include "Classes/CSharpClass.h"
#include "ICSharpMethodInvocation.h"
#include "IPropertyMarshaller.h"
#include "ICSharpRuntime.h"
#include "ICSharpObjectTable.h"
#include "Misc/StackMemory.h"
#include "Misc/ScopedCSharpMethodInvocation.h"

namespace UnrealSharp
{
    // skip marshal parameter for __WorldContext
    // check FFunctionEntryHelper in K2Node-FunctionEntry.cpp
    // check FBlueprintCompilationManagerImpl::FastGenerateSkeletonClass in BlueprintCompilationManager.cpp
    const FString FUnrealFunctionMarshallerLinker::WorldContextName = TEXT("__WorldContext");

    FUnrealFunctionMarshallerLinker::FUnrealFunctionMarshallerLinker(ICSharpRuntime* InRuntime, UFunction* InFunction, const FCSharpFunctionData* InFunctionData)
    {
        // step 1 : cache all properties
        for (TFieldIterator<FProperty> PropertyIter(InFunction, EFieldIterationFlags::Default); PropertyIter; ++PropertyIter)
        {
            FProperty* Property = *PropertyIter;

            PropertyQueue.Add({Property});
        }

        // step 2 : construct Marshallers from function data
        check(InFunctionData != nullptr);

        // It can only be initialized from FunctionData, because the order of function parameters of C# may be inconsistent with UFunction. 
        // The main purpose of Marshaller is to pass parameters to C#.
        for (auto& ArgumentData : InFunctionData->Arguments)
        {
            auto* PropertyInfoPtr = PropertyQueue.FindByPredicate([&](const FPropertyInfo& info){
                return info.Property->GetFName() == ArgumentData.Name;
            });

            checkf(PropertyInfoPtr, TEXT("Failed find argument <%s> on UFunction:%s, Signature:%s"), *ArgumentData.Name.ToString(), *InFunction->GetName(), *InFunctionData->FunctionSignature);

            TSharedPtr<FPropertyMarshallerInfo> PropertyMarshallerInfoPtr = MakeShared<FPropertyMarshallerInfo>();
            PropertyMarshallerInfoPtr->MarshallerPtr = InRuntime->GetPropertyMarshaller(PropertyInfoPtr->Property);
            check(PropertyMarshallerInfoPtr->MarshallerPtr);
            PropertyMarshallerInfoPtr->Property = PropertyInfoPtr->Property;

            PropertyMarshallerInfoPtr->bPassByReference = ArgumentData.IsPassByReference();
            PropertyMarshallerInfoPtr->OffsetInTempParameterBuffer = TempParameterSize;
            TempParameterSize += PropertyMarshallerInfoPtr->MarshallerPtr->GetTempParameterBufferSize();

            // this is return type
            if (ArgumentData.IsReturnValue())
            {
                check(PropertyMarshallerInfoPtr->Property->HasAnyPropertyFlags(CPF_ReturnParm));
                check(!ReturnValueMarshaller.IsValid());

                ReturnValueMarshaller = PropertyMarshallerInfoPtr;
            }
            else
            {
                MarshallerQueue.Add(PropertyMarshallerInfoPtr);
            }

            PropertyInfoPtr->MarshallerInfoPtr = PropertyMarshallerInfoPtr;
        }
    }

    void FUnrealFunctionMarshallerLinker::BeginInvoke(
        ICSharpMethodInvocation* InInvocation,
        const FStackMemory& InParameterBuffer,
        const FStackMemory& InTempInteropParameterPointers,
        const FStackMemory& InUnrealParameterReferencePointers,
        UObject* Context, 
        FFrame& Stack, 
        RESULT_DECL
        ) const
    {
        // 1. read unreal data from Unreal stack
        // When reading parameters in the unreal stack, you need to read them in the unreal order.
        for (int Index = 0; Index < PropertyQueue.Num(); ++Index)
        {
            const FPropertyInfo& PropertyInfo = PropertyQueue[Index];
            const FProperty* Property = PropertyInfo.Property;

            checkSlow(InParameterBuffer.StackPointer);

            // when invoke C# method, the memory of return param is not used.
            if (!Property->HasAnyPropertyFlags(CPF_ReturnParm))
            {
                // reset to default value
                Property->InitializeValue_InContainer(InParameterBuffer.StackPointer);

                void* PropertyAddress = Property->ContainerPtrToValuePtr<void>(InParameterBuffer.StackPointer);
                Stack.StepCompiledIn(PropertyAddress, Property->GetClass());

                // Functions passed by non-reference in a blueprint are treated as output. 
                // Therefore, the buffer of the output parameter needs to be reset here, 
                // otherwise the ref parameter contains the residue of the old parameter.
                if (PropertyInfo.MarshallerInfoPtr && PropertyInfo.MarshallerInfoPtr->bPassByReference)
                {
                    // destroy old value
                    Property->DestroyValue(PropertyAddress);

                    // re initialize ...
                    Property->InitializeValue(PropertyAddress);
                }                

                // get reference 
                void* RefPropertyAddress = Stack.MostRecentPropertyAddress != NULL ? Stack.MostRecentPropertyAddress : PropertyAddress;

                checkSlow(InUnrealParameterReferencePointers.StackPointer);

                // save reference to unreal data
                // if pass by value, this pointer will point to PropertyAddress
                // if pass by reference, it will point to Unreal internal address
                void** UnrealDataReferencePointer = GetUnrealParameterReferencePointerAddress(InUnrealParameterReferencePointers, Index);
                *UnrealDataReferencePointer = RefPropertyAddress;
            }
        }

        P_FINISH;

        // 2. pass parameter to C#
        for (auto& MarshallerInfoPtr : MarshallerQueue)
        {
            checkSlow(InTempInteropParameterPointers.StackPointer);
            checkSlow(MarshallerInfoPtr && MarshallerInfoPtr->Property);

            void* PropertyAddress = MarshallerInfoPtr->Property->ContainerPtrToValuePtr<void>(InParameterBuffer.StackPointer);

            // if pass by reference, marshaller will save temp value's address in it
            void** TempAddress = GetTempParameterPointerAddress(InTempInteropParameterPointers, MarshallerInfoPtr->OffsetInTempParameterBuffer);

            FPropertyMarshallerParameters Parameters = {
                InInvocation,
                MarshallerInfoPtr->Property,
                PropertyAddress,
                TempAddress,
                MarshallerInfoPtr->bPassByReference
            };

            checkSlow(MarshallerInfoPtr->MarshallerPtr);

            MarshallerInfoPtr->MarshallerPtr->AddParameter(Parameters);
        }
    }

    void FUnrealFunctionMarshallerLinker::FinishInvoke(const FStackMemory& InParameterBuffer) const
    {
        for (int Index = 0; Index < PropertyQueue.Num(); ++Index)
        {
            const FPropertyInfo& PropertyInfo = PropertyQueue[Index];
            const FProperty* Property = PropertyInfo.Property;

            checkSlow(InParameterBuffer.StackPointer);

            // destroy values
            // when invoke C# method, the memory of return param is not used.
            if (!Property->HasAnyPropertyFlags(CPF_ReturnParm))
            {
                Property->DestroyValue_InContainer(InParameterBuffer.StackPointer);
            }
        }
    }

    void FUnrealFunctionMarshallerLinker::CopyReferenceParameters(
        const FStackMemory& InTempInteropParameterPointers,
        const FStackMemory& InUnrealParameterReferencePointers
    ) const
    {
        // copy ref parameters first
        for (int Index = 0; Index < PropertyQueue.Num(); ++Index)
        {
            const FPropertyInfo& PropertyInfo = PropertyQueue[Index];
            auto& MarshallerPtr = PropertyInfo.MarshallerInfoPtr;

            if (MarshallerPtr && MarshallerPtr->bPassByReference)
            {
                checkSlow(MarshallerPtr->Property == PropertyInfo.Property);

                void** UnrealInternalDataPointerAddress = GetUnrealParameterReferencePointerAddress(InUnrealParameterReferencePointers, Index);
                void** InteropTempDataPointerAddress = GetTempParameterPointerAddress(InTempInteropParameterPointers, MarshallerPtr->OffsetInTempParameterBuffer);

                MarshallerPtr->MarshallerPtr->Copy(
                    *UnrealInternalDataPointerAddress, 
                    *InteropTempDataPointerAddress, 
                    PropertyInfo.Property, 
                    EMarshalCopyDirection::CSharpToUnreal
                );
            }
        }
    }

    void** FUnrealFunctionMarshallerLinker::GetUnrealParameterReferencePointerAddress(const FStackMemory& InUnrealParameterReferencePointers, int InIndexInProperties)
    {
        checkSlow(InUnrealParameterReferencePointers.StackPointer);
        check(InIndexInProperties*sizeof(void*) < InUnrealParameterReferencePointers.Size);

        return (void**)((uint8*)InUnrealParameterReferencePointers.StackPointer + sizeof(void*) * InIndexInProperties);
    }

    void** FUnrealFunctionMarshallerLinker::GetTempParameterPointerAddress(const FStackMemory& InTempInteropParameterPointers, int InOffsetInTempParameterBuffer)
    {
        checkSlow(InTempInteropParameterPointers.StackPointer);
        checkSlow(InOffsetInTempParameterBuffer >= 0);
        check(InOffsetInTempParameterBuffer < InTempInteropParameterPointers.Size);

        return (void**)((uint8*)InTempInteropParameterPointers.StackPointer + InOffsetInTempParameterBuffer);
    }

    FUnrealFunctionInvokeRedirector::FUnrealFunctionInvokeRedirector(
        ICSharpRuntime* InRuntime, 
        UCSharpClass* InClass, 
        UFunction* InFunction, 
        const FCSharpFunctionData* InFunctionData, 
        TSharedPtr<ICSharpMethodInvocation> InInvocation) :
        Runtime(InRuntime),
        Function(InFunction),
        FunctionData(InFunctionData),
        Invocation(InInvocation),
        Linker(InRuntime, InFunction, InFunctionData)
    {
    }

    FUnrealFunctionInvokeRedirector::~FUnrealFunctionInvokeRedirector()
    {
    }

    const UFunction* FUnrealFunctionInvokeRedirector::GetFunction() const
    {
        return Function;
    }

    void FUnrealFunctionInvokeRedirector::Invoke(UObject* Context, FFrame& Stack, RESULT_DECL)
    {
        check(Invocation);

        UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(Invocation);

        // We need to copy the unreal data separately, because the order of function parameters in C# and the order in the UFunction stack may be different.
        // used to save all parameters in it
        void* const ParameterBuffer = Function->PropertiesSize > 0 ? FMemory_Alloca_Aligned(Function->PropertiesSize, Function->GetMinAlignment()) : nullptr;

        // This memory is used to store some memory that is temporarily needed during the Marshaller process.
        // used to save temp interop value in it
        const int TempParameterSize = Linker.GetTempParameterSize();
        void* const TempParameterPointers = TempParameterSize > 0 ? FMemory_Alloca(TempParameterSize) : nullptr;

        // used to save unreal write back pointers
        // if pass by value, this pointer will point to ParameterBuffer
        // if pass by reference, this pointer will point to unreal internal data
        const int ParameterCount = Linker.GetUnrealFunctionParameterCount();
        const int ParameterSize = ParameterCount * sizeof(void*);
        void* const UnrealParameterReferencePointers = ParameterCount > 0 ? FMemory_Alloca(ParameterSize) : nullptr;

        FStackMemory ParameterMemory = { ParameterBuffer, Function->PropertiesSize };
        FStackMemory TempParameterMemory = { TempParameterPointers, TempParameterSize };
        FStackMemory UnrealParameterReferenceMemory = { UnrealParameterReferencePointers, ParameterSize };

        Linker.BeginInvoke(
            Invocation.Get(), 
            ParameterMemory,
            TempParameterMemory,
            UnrealParameterReferenceMemory,
            Context, 
            Stack, 
            RESULT_PARAM
        );

        UNREALSHARP_SCOPED_EXIT(Linker.FinishInvoke(ParameterMemory));
        
        P_NATIVE_BEGIN;

        // static function should be called with nullptr
        void* CSharpObject = (Function->FunctionFlags & FUNC_Static) != 0 || Context == nullptr ? nullptr : Runtime->GetObjectTable()->GetCSharpObject(Context);

        TUniquePtr<ICSharpMethodInvocationException> ExceptionContext;
        void* Result = Invocation->Invoke(CSharpObject, ExceptionContext);

        // Copy Reference parameter back, if need
        Linker.CopyReferenceParameters(TempParameterMemory, UnrealParameterReferenceMemory);
        
        // convert result value to unreal data type
        if (Result != nullptr && Linker.ReturnValueMarshaller.IsValid())
        {
            Linker.ReturnValueMarshaller->MarshallerPtr->Copy(
                RESULT_PARAM, 
                Result, 
                Linker.ReturnValueMarshaller->Property, 
                EMarshalCopyDirection::CSharpReturnValueToUnreal
                );
        }

        P_NATIVE_END;
    }
}


