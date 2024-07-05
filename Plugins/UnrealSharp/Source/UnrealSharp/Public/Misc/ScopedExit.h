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

namespace UnrealSharp
{
    /// <summary>
    /// Class TScopedExit.
    /// based on RAII, used to safe delete resource, like __try __finally
    /// </summary>
    template <typename TCallable>
    class TScopedExit
    {
    public:
        TScopedExit(TCallable Func) :
            Func(Func)
        {
        }

        ~TScopedExit()
        {
            Func();
        }

        TScopedExit(const TScopedExit&) = delete;
        const TScopedExit& operator=(const TScopedExit&) = delete;

    private:
        TCallable Func;
    };
}

#define US_PP_CAT_IMPL_(a, b ) a ## b 
#define US_PP_CAT(a, b) US_PP_CAT_IMPL_( a, b )

// use this macro directly
#define US_SCOPED_EXIT(expression) \
    ::UnrealSharp::TScopedExit<TFunction<void()> > US_PP_CAT(ScopedExitInstalce_, __COUNTER__)([&]()\
{ \
    expression;\
})


