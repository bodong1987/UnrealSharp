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
#include "MonoRuntime/MonoMethodInvocation.h"
#include "Misc/UnrealSharpLog.h"

#if WITH_MONO
#include "MonoRuntime/MonoInteropUtils.h"
#include "MonoRuntime/MonoMethod.h"
#include "Misc/CSharpStructures.h"
#include "MonoRuntime/MonoRuntime.h"
#include "Misc/StackMemory.h"
#include "Misc/ScopedCSharpMethodInvocation.h"

namespace UnrealSharp::Mono
{
    class FMonoInvocationException : public ICSharpMethodInvocationException
    {
    public:
        FString Message;
        FString StackTrace;

        FMonoInvocationException(MonoObject* InExceptionObject)
        {
            MonoObject* ExceptionInStringConversion = nullptr;
            MonoString* MonoExceptionString = mono_object_to_string(InExceptionObject, &ExceptionInStringConversion);

            if (MonoExceptionString != nullptr)
            {
                Message = FMonoInteropUtils::GetFString(MonoExceptionString);
            }
            else
            {
                check(ExceptionInStringConversion);

                // Can't really get much out of the original exception with the public API, so just note that two exceptions were thrown
                FString ExceptionString;
                MonoExceptionString = mono_object_to_string(ExceptionInStringConversion, nullptr);
                check(MonoExceptionString);
                
                Message = FMonoInteropUtils::GetFString(MonoExceptionString);
            }

            // set to default if not valid...
            if (Message.IsEmpty())
            {
                Message = TEXT("MonoRuntimeException");
            }

            // try get stack trace
            MonoClass* exceptionClass = mono_object_get_class(InExceptionObject);

            // FMonoRuntime::DumpClassInfomration(exceptionClass);
            MonoProperty* stackTraceProperty = mono_class_get_property_from_name(exceptionClass, "StackTrace");

            if (stackTraceProperty != nullptr)
            {
                MonoString* stackTrace = (MonoString*)mono_property_get_value(stackTraceProperty, InExceptionObject, NULL, NULL);

                if (stackTrace != nullptr)
                {
                    StackTrace = FMonoInteropUtils::GetFString(stackTrace);
                }
            }            
        }

        virtual const FString& GetMessage() const override
        {
            return Message;
        }

        virtual const FString& GetStackTrace() const override
        {
            return StackTrace;
        }
    };
    
    FMonoMethodInvocation::FMonoMethodInvocation(TSharedPtr<FMonoMethod> InMethod) :
        Method(InMethod)
    {        
        
    }

    FMonoMethodInvocation::~FMonoMethodInvocation()
    {
    }

    ICSharpMethod* FMonoMethodInvocation::GetMethod() const
    {
        return Method.Get();
    }

    void* FMonoMethodInvocation::Invoke(void* InInstance)
    {
        TUniquePtr<ICSharpMethodInvocationException> OutException;
        return Invoke(InInstance, OutException);
    }

    void* FMonoMethodInvocation::Invoke(void* InInstance, TUniquePtr<ICSharpMethodInvocationException>& OutException)
    {
        MonoMethod* ActualMethod = Method->GetMethod();

        if (InInstance != nullptr && Method->IsVirtual())
        {
            ActualMethod = mono_object_get_virtual_method((MonoObject*)InInstance, ActualMethod);

#if UE_BUILD_DEBUG
        //    FString ActualMethodName = FMonoMethod::GetMethodFullName(ActualMethod, true);

        //    US_LOG(TEXT("invoke C# virtual function %s, map to %s"), *Method->GetFullyQualifiedName(), *ActualMethodName);
#endif
        }

        check(Method->IsStatic() || (!Method->IsStatic() && InInstance));

        // ParamBufferPtr can be null, but ParameterBuffer can't be null
        checkSlow(ParameterBuffer);
        void* ParamBufferPtr = ParameterBuffer->StackPointer;

        MonoObject* Exception = nullptr;
        MonoObject* ReturnValue = mono_runtime_invoke(ActualMethod, Method->IsStatic() ? nullptr : InInstance, (void**)ParamBufferPtr, &Exception);

        if (Exception != nullptr)
        {
            OutException.Reset(new FMonoInvocationException(Exception));

            US_LOG_ERROR(TEXT("C# Exception:%s"), *OutException->GetMessage());

            return nullptr;
        }

        return ReturnValue;
    }

    void FMonoMethodInvocation::BeginInvoke(const FStackMemory& InParameterBuffer)
    {
        check(ParameterBuffer == nullptr && ParamCount == 0);
        ParameterBuffer = &InParameterBuffer;
        ParamCount = 0;
    }

    void FMonoMethodInvocation::EndInvoke()
    {
        ParameterBuffer = nullptr;
        ParamCount = 0;
    }

    void FMonoMethodInvocation::AddArgument(void* InArgumentPtr)
    {
        check(ParameterBuffer != nullptr);
        check(ParameterBuffer->StackPointer != nullptr);
        check(ParamCount*sizeof(void*) <= ParameterBuffer->Size - sizeof(void*));

        void** ParamBufferPtr = (void**)ParameterBuffer->StackPointer;
        *(ParamBufferPtr + ParamCount) = InArgumentPtr;
        ++ParamCount;
    }

    int FMonoMethodInvocation::GetCSharpFunctionParameterCount() const
    {
        return Method ? Method->GetParameterCount() : 0;
    }
}
#endif

