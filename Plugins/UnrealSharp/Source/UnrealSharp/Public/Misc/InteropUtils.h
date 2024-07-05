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

#include "Misc/CSharpStructures.h"

namespace UnrealSharp
{
    class FUnrealFunctionInvocation;

    /*
    * Here are the basic interactive functions required by UnrealSharp to implement basic functions.
    * If you want to add a new API to access Unreal C++ for your C#, there are two main methods:
    * 1. the first is to implement a new C++ BlueprintFunctionLibrary. 
    *     The UFUNCTION in this class will be automatically exported to C#; 
    * 2. the second is Get FUnrealInteropFunctions through IUnrealSharpModule and manually register your interactive functions into the framework; 
    *    however, it should be noted that the names of interactive functions must be globally unique, otherwise conflicts will occur.
    */
    class UNREALSHARP_API FInteropUtils
    {
    public:    
        #define DECLARE_UNREAL_SHARP_INTEROP_API(returnType, name, parameters) \
            static returnType name parameters

        #include "Misc/InteropApiDefines.inl"

        #undef DECLARE_UNREAL_SHARP_INTEROP_API

        // create C# struct
        static void*               CreateCSharpStruct(const void* InUnrealStructPtr, UScriptStruct* InStruct);

        // copy C# struct 's data to unreal
        static void                StructToNative(UScriptStruct* InStructType, void* InNativePtr, const void* InCSharpStructPtr);

        // create C# collection
        static void*               CreateCSharpCollection(void* InAddressOfCollection, FProperty* InCollectionProperty);

        // copy C# collection data to unreal
        static void                CopyFromCSharpCollection(void* InAddressOfCollection, FProperty* InCollectionProperty, void* InCSharpCollection);
    };
}
