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
    /*
    * This represents calling UFunction through Unreal's UFunction calling mechanism. 
    * Of course, when FastInvoke is turned on, 
    * some function calls for UFunction will be implemented through C++ interactive functions and will not pass here. 
    * In addition, this class also implements calls to delegates.
    */
    class UNREALSHARP_API FUnrealFunctionInvocation
    {
    public:
        FUnrealFunctionInvocation();

        // construct by function path
        explicit FUnrealFunctionInvocation(const TCHAR* InFunctionPath);

        // construct by class and method name
        explicit FUnrealFunctionInvocation(const UClass* InClass, const TCHAR* InFunctionName);

        // construct by UFunction pointer
        explicit FUnrealFunctionInvocation(UFunction* InFunction);

        // construct by Delegate property
        explicit FUnrealFunctionInvocation(const FDelegateProperty* InDelegateProperty);

        // construct by Multicast Delegate Property
        explicit FUnrealFunctionInvocation(const FMulticastDelegateProperty* InMulticastDelegateProperty);

        ~FUnrealFunctionInvocation();

        void                                Load(const TCHAR* InFunctionPath);
        void                                Load(const UClass* InClass, const TCHAR* InFunctionName);
                                                
        // Initialize the parameter stack of this UFunction* call
        void                                InitializeParameterBuffer(void* InParameterBuffer, int InParameterBufferSize) const;

        // Clear the parameter stack of this UFunction* call
        void                                UnInitializeParameterBuffer(void* InParameterBuffer, int InParameterBufferSize) const;

        // Invoke UFunction*
        void                                Invoke(UObject* InObject, void* InParameterBuffer, int InParameterBufferSize) const;
        
        // get backend UFunction*
        UFunction*                          GetFunction() const{ return Function; }
        
    private:
        UFunction*                          Function = nullptr;
        const FMulticastDelegateProperty*   MulticastDelegateProperty = nullptr;
        const FDelegateProperty*            DelegateProperty = nullptr;
    };
}
