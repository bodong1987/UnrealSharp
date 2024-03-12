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
    class ICSharpMethod;
    struct FStackMemory;

    /*
    * Exception class interface executed by C# code. 
    * When calling a C# method, if this object is provided, 
    * then you can get the exception details from this object [only when an exception occurs]
    */
    class UNREALSHARP_API ICSharpMethodInvocationException
    {
    public:
        virtual ~ICSharpMethodInvocationException() = default;

    public:
        // Get Exception Message
        virtual const FString&                GetMessage() const = 0;

        // Get Stack trace
        virtual const FString&                GetStackTrace() const = 0;
    };

    /*
    * This is a package for C# method calls. 
    * It has functions such as parameter packaging and function calling.
    */
    class UNREALSHARP_API ICSharpMethodInvocation
    {
    public:
        virtual ~ICSharpMethodInvocation() = default;

    public:
        // get backend method
        virtual ICSharpMethod*                GetMethod() const = 0;

        // start invoke
        // reset invoke temp buffer 
        virtual void                          BeginInvoke(const FStackMemory& InParameterBuffer) = 0;

        // Call a C# function and ignore exception information
        // When the method is a static method, InInstance can be empty, otherwise it cannot be empty.
        virtual void*                         Invoke(void* InInstance) = 0;

        // Call a C# function and get exception information
        // When the method is a static method, InInstance can be empty, otherwise it cannot be empty.
        virtual void*                         Invoke(void* InInstance, TUniquePtr<ICSharpMethodInvocationException>& OutException) = 0;

        // finish invoke
        // release temp buffer if need
        virtual void                          EndInvoke() = 0;

        // add argument for this invoke
        virtual void                          AddArgument(void* InArgumentPtr) = 0;

        // get c# method parameter count
        virtual int                           GetCSharpFunctionParameterCount() const = 0;        
    };
}
