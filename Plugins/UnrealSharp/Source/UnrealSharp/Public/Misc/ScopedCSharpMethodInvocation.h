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

#include "ICSharpMethodInvocation.h"
#include "StackMemory.h" // NOLINT, used in Macros

namespace UnrealSharp
{
    /*
    * The secondary encapsulation of ICSharpMethodInvocation.
    * it is based on the RAII mechanism and template functions, 
    * combined with the use of the macro US_SCOPED_CSHARP_METHOD_INVOCATION, 
    * making the invocation of C# methods more streamlined.
    * @example:
    *    US_SCOPED_CSHARP_METHOD_INVOCATION(WriteSoftObjectPtrInvocation);
    *    WriteSoftObjectPtrInvocationInvoker.Invoke(nullptr, &InDestinationAddress, InSourceObjectInterface);
    * 
    *   Use a macro to define an Invoker, and then calling the C# method will be similar to calling the C# method directly, 
    *   without the need to manually call 
    *        BeginInvoke
    *        AddParameter
    *        Invoke
    *        EndInvoke, etc.
    */
    class UNREALSHARP_API FScopedCSharpMethodInvocation
    {
    public:
        FScopedCSharpMethodInvocation(ICSharpMethodInvocation& InInvocationRef, const FStackMemory& InParameterBuffer);
        FScopedCSharpMethodInvocation(const TSharedPtr<ICSharpMethodInvocation>& InInvocationPtr, const FStackMemory& InParameterBuffer);
        FScopedCSharpMethodInvocation(TSharedPtr<ICSharpMethodInvocation>&& InInvocationPtr, const FStackMemory& InParameterBuffer);
        ~FScopedCSharpMethodInvocation();

        // disable these operations
        FScopedCSharpMethodInvocation(const FScopedCSharpMethodInvocation&) = delete;
        FScopedCSharpMethodInvocation(FScopedCSharpMethodInvocation&&) = delete;
        FScopedCSharpMethodInvocation& operator = (const FScopedCSharpMethodInvocation&) = delete;
        FScopedCSharpMethodInvocation& operator = (FScopedCSharpMethodInvocation&&) = delete;

    public:
        // invoke C# method with instance(can be null for static method)
        void*                        Invoke(void* InInstance);

        // invoke C# method with instance(can be null for static method), capture exception information
        void*                        Invoke(void* InInstance, TUniquePtr<ICSharpMethodInvocationException>& OutException);

        // decode return value
        void*                        DecodeReturnValue(void* InReturnValue);

        // invoke C# method with instance(can be null for static method), auto decode return value
        void*                        DecodedInvoke(void* InInstance);

        // invoke C# method with instance(can be null for static method), capture exception information, auto decode return value
        void*                        DecodedInvoke(void* InInstance, TUniquePtr<ICSharpMethodInvocationException>& OutException);

        ICSharpMethodInvocation*     GetInvocation() const { return &Invocation; }

        void                         AddArgument(void* InArgumentPtr);

        // Parameters are automatically packaged during compilation, and there is no need to manually AddParameter, making the call easier and more convenient.
        template <typename... T>
        void*                        Invoke(void* InInstance, T*... InArgs)
        {
            AddParameter<T...>(InArgs...);

            return Invoke(InInstance);
        }

        // Parameters are automatically packaged during compilation, and there is no need to manually AddParameter, making the call easier and more convenient.
        // auto decode return value
        template <typename... T>
        void*                        DecodedInvoke(void* InInstance, T*... InArgs)
        {
            AddParameter<T...>(InArgs...);

            return DecodedInvoke(InInstance);
        }

        // If your C# function returns a simple value type, it is more convenient to use this interface directly.
        template <typename TReturnType, typename... T>
        TReturnType                  Invoke(void* InInstance, T*... InArgs)
        {
            static_assert(TIsPODType<TReturnType>::Value, "This interface is only available for value types (same on C# and C++ sides)");

            if constexpr (sizeof...(T) > 0)
            {
                AddParameter<T...>(InArgs...);
            }

            TReturnType* ReturnValuePointer = static_cast<TReturnType*>(DecodedInvoke(InInstance));
            
            return *ReturnValuePointer;
        }

    private:
        template <typename T, typename... TExtraTypes>
        void                         AddParameter(T* InArgs, TExtraTypes* ... InExtraArgs)
        {
            AddArgument((void*)(InArgs)); // NOLINT

            if constexpr (sizeof...(TExtraTypes) > 0)
            {
                AddParameter(InExtraArgs...);
            }
        }

    private:
        ICSharpMethodInvocation&     Invocation;
    };

// don't use FScopedCSharpMethodInvocation directly
// use this macro.
#define US_SCOPED_CSHARP_METHOD_INVOCATION(name) \
        checkSlow(name); \
        const int name##ParameterCount = name->GetCSharpFunctionParameterCount(); \
        const int name##ParameterBufferSize = sizeof(void*)*name##ParameterCount; \
        void* name##ParameterBuffer = name##ParameterBufferSize > 0 ? FMemory_Alloca(name##ParameterBufferSize) : nullptr; \
        ::UnrealSharp::FScopedCSharpMethodInvocation name##Invoker(name, {name##ParameterBuffer, name##ParameterBufferSize})
}
