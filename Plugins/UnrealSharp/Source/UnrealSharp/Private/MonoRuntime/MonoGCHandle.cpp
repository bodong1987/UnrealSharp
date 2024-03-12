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
#include "MonoGCHandle.h"

#if WITH_MONO
#include "MonoRuntime/Mono.h"

namespace UnrealSharp::Mono
{
    FMonoGCHandle::FMonoGCHandle(void* InCSharpObject, bool bInWeakReference) :
        Handle(0),
        bIsWeak(bInWeakReference)
    {
        if (bInWeakReference)
        {
            Handle = mono_gchandle_new_weakref((MonoObject*)InCSharpObject, false);
        }
        else
        {
            Handle = mono_gchandle_new((MonoObject*)InCSharpObject, false);
        }
    }

    FMonoGCHandle::~FMonoGCHandle()
    {
        if (Handle != 0)
        {
            mono_gchandle_free(Handle);
            Handle = 0;
        }
    }

    bool FMonoGCHandle::IsWeakReference() const
    {
        return bIsWeak;
    }

    bool FMonoGCHandle::IsValid() const 
    {
        return Handle != 0;
    }

    void* FMonoGCHandle::GetObject() const 
    {
        check(Handle != 0);

        return (void*)mono_gchandle_get_target(Handle);
    }
}
#endif
