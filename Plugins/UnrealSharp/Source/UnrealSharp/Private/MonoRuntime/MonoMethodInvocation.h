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

#if WITH_MONO
#include "ICSharpMethodInvocation.h"

namespace UnrealSharp::Mono
{
    class FMonoMethodInvokeFrame;
    class FMonoMethod;

    class FMonoMethodInvocation : public ICSharpMethodInvocation
    {
    public: 
        FMonoMethodInvocation(TSharedPtr<FMonoMethod> InMethod);
        ~FMonoMethodInvocation();

    public:
        virtual ICSharpMethod* GetMethod() const override;
        virtual void  BeginInvoke(const FStackMemory& InParameterBuffer) override;
        virtual void* Invoke(void* InInstance) override;
        virtual void* Invoke(void* InInstance, TUniquePtr<ICSharpMethodInvocationException>& OutException) override;
        virtual void  EndInvoke() override;
        virtual void* DecodeReturnPointer(void* InReturnValue) const override;

        virtual void AddArgument(void* InArgumentPtr) override;
        virtual int GetCSharpFunctionParameterCount() const override;

    private:                
        TSharedPtr<FMonoMethod>                                Method;
        const FStackMemory*                                    ParameterBuffer = nullptr;
        int                                                    ParamCount = 0;
    };
}

#endif

