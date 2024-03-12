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
#ifndef MONO_API_FUNCTION
#error "This is internal use header file, don't use this directly."
#endif

#include <mono/metadata/details/image-functions.h>
#include <mono/metadata/details/metadata-functions.h>
#include <mono/metadata/details/mono-gc-functions.h>
#include <mono/metadata/details/class-functions.h>
#include <mono/utils/details/mono-publib-functions.h>
#include <mono/metadata/details/debug-helpers-functions.h>
#include <mono/metadata/details/mono-debug-functions.h>
#include <mono/metadata/details/assembly-functions.h>
#include <mono/jit/details/jit-functions.h>
#include <mono/utils/details/mono-logger-functions.h>
#include <mono/utils/details/mono-dl-fallback-functions.h>
#include <mono/metadata/details/appdomain-functions.h>
#include <mono/metadata/details/object-functions.h>
#include <mono/metadata/details/loader-functions.h>
