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
#include "Misc/ScopedCSharpMethodInvocation.h"

namespace UnrealSharp
{
    FScopedCSharpMethodInvocation::FScopedCSharpMethodInvocation(ICSharpMethodInvocation& InInvocationRef, const FStackMemory& InParameterBuffer) :
        Invocation(InInvocationRef)
    {
        Invocation.BeginInvoke(InParameterBuffer);
    }

    FScopedCSharpMethodInvocation::FScopedCSharpMethodInvocation(TSharedPtr<ICSharpMethodInvocation> InInvocationPtr, const FStackMemory& InParameterBuffer) :
        Invocation(*InInvocationPtr)
    {
        Invocation.BeginInvoke(InParameterBuffer);
    }

    FScopedCSharpMethodInvocation::FScopedCSharpMethodInvocation(TSharedPtr<ICSharpMethodInvocation>&& InInvocationPtr, const FStackMemory& InParameterBuffer) :
        Invocation(*InInvocationPtr)
    {
        Invocation.BeginInvoke(InParameterBuffer);
    }

    FScopedCSharpMethodInvocation::~FScopedCSharpMethodInvocation()
    {
        Invocation.EndInvoke();
    }

    void* FScopedCSharpMethodInvocation::Invoke(void* InInstance)
    {
        return Invocation.Invoke(InInstance);
    }

    void* FScopedCSharpMethodInvocation::Invoke(void* InInstance, TUniquePtr<ICSharpMethodInvocationException>& OutException)
    {
        return Invocation.Invoke(InInstance, OutException);
    }

    void* FScopedCSharpMethodInvocation::DecodeReturnValue(void* InReturnValue)
    {
        return Invocation.DecodeReturnPointer(InReturnValue);
    }

    void* FScopedCSharpMethodInvocation::DecodedInvoke(void* InInstance)
    {
        return DecodeReturnValue(Invoke(InInstance));
    }

    void* FScopedCSharpMethodInvocation::DecodedInvoke(void* InInstance, TUniquePtr<ICSharpMethodInvocationException>& OutException)
    {
        return DecodeReturnValue(Invoke(InInstance, OutException));
    }

    void FScopedCSharpMethodInvocation::AddArgument(void* InArgumentPtr)
    {
        Invocation.AddArgument(InArgumentPtr);
    }
}

