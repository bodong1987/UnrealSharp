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

UNREALSHARP_API DECLARE_LOG_CATEGORY_EXTERN(UnrealSharpLog, Log, All);

// log
#define US_LOG(Format, ...) UE_LOG(UnrealSharpLog, Log, Format, ##__VA_ARGS__)

// log warning
#define US_LOG_WARN(Format, ...) UE_LOG(UnrealSharpLog, Warning, Format, ##__VA_ARGS__)

// log error
#define US_LOG_ERROR(Format, ...) UE_LOG(UnrealSharpLog, Error, Format, ##__VA_ARGS__)

// log verbose
#define US_LOG_VERBOSE(Format, ...) UE_LOG(UnrealSharpLog, Verbose, Format, ##__VA_ARGS__)

// log debug only
#if UE_BUILD_DEBUG
#define US_LOG_DEBUG(Format, ...) UE_LOG(UnrealSharpLog, Log, Format, ##__VA_ARGS__)
#else
#define US_LOG_DEBUG(Format, ...)
#endif

// compatible between C++ 17 and C++ 20
#if ENGINE_MAJOR_VERSION == 5 && ENGINE_MINOR_VERSION > 2
#define US_LAMBDA_CAPTURE_THIS =, this
#else
#define US_LAMBDA_CAPTURE_THIS =
#endif

#define US_UNREFERENCED_PARAMETER(p) (void)p

