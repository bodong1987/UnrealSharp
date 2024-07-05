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
#include "MonoRuntime/MonoApis.h"
#include "MonoRuntime/Mono.h"

#if WITH_MONO

#ifndef MONO_RT_EXTERNAL_ONLY
#define MONO_RT_EXTERNAL_ONLY
#endif

#define MONO_API_FUNCTION(returnType, name, params) \
    namespace UnrealSharp::Mono{ \
        returnType (*name) params; \
    }

#include "MonoRuntime/MonoApisImport.h"  // NOLINT

#undef MONO_API_FUNCTION

namespace UnrealSharp::Mono
{
    void FMonoApis::Import(void* InHandle)
    {
#if UNREALSHARP_MONO_APIS_DYNAMIC_BINDING
#define MONO_API_FUNCTION(returnType, name, params) \
        name = (decltype(name))FPlatformProcess::GetDllExport(InHandle, TEXT(#name)); \
        checkf(name != nullptr, TEXT("Failed bind mono api:") TEXT(#name));
#else
#define MONO_API_FUNCTION(returnType, name, params) \
        name = &::name; \
        checkf(name != nullptr, TEXT("Failed bind mono api:") TEXT(#name));
#endif

#include "MonoRuntime/MonoApisImport.h"

#undef MONO_API_FUNCTION
    }

    void FMonoApis::UnImport()
    {
#define MONO_API_FUNCTION(returnType, name, params) \
        name = nullptr;

#include "MonoRuntime/MonoApisImport.h"

#undef MONO_API_FUNCTION
    }
}

#endif
