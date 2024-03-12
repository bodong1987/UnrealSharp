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

#include "ICSharpMethod.h"

#if WITH_MONO
#include "MonoRuntime/Mono.h"

namespace UnrealSharp::Mono
{
    class FMonoMethod : public ICSharpMethod
    {
    public:
        FMonoMethod(MonoMethod* InMethod);

        inline MonoMethod*          GetMethod() const{ return Method; }

        virtual void*               GetHandle() const override { return Method; }
        virtual bool                IsVirtual() const override;
        virtual bool                IsStatic() const override;
        virtual bool                IsFinal() const override;
        virtual int                 GetParameterCount() const override;

    public:
        static FString              GetMethodAssemblyName(MonoMethod* InMonoMethod);
        static FString              GetMethodFullName(MonoMethod* InMonoMethod, bool bSignatrure = false);

#if UE_BUILD_DEBUG
        const FString&              GetAssemblyName() const { return AssemblyName; }
        const FString&              GetFullyQualifiedName() const{ return FullyQualifiedMethodName; }
#endif

    private:
        MonoMethod*                 Method = nullptr;
        uint32                      Flags = 0;
        int                         ParamCount = 0;

#if UE_BUILD_DEBUG
    private:
        FString                     AssemblyName;
        FString                     FullyQualifiedMethodName;
#endif
    };
}

#endif
