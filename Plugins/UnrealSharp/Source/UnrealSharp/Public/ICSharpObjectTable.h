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
    * Used to save the mapping of UnrealObject (UObject*) to C# Object, and also create a proxy for UObject* on the C# side. 
    * It is also responsible for the coordination of the memory management of Unreal Object and the memory management of C# objects.     
    * As long as the Unreal Object still exists, the C# Object will definitely exist. 
    * This is achieved through GCHandle.
    * The lifetime of the C# object is determined by the lifetime of the Unreal Object. 
    * After the Unreal Object is garbage collected, the C# proxy object will be removed from GCHandle and its bound NativePtr will be empty.
    */
    class UNREALSHARP_API ICSharpObjectTable
    {
    public:
        virtual ~ICSharpObjectTable() = default;

    public:
        /*
        * Get the C# proxy object of UObject*. If it already exists, it will be returned directly; 
        * if not, you need to create one.
        */
        virtual void*                           GetCSharpObject(UObject* InObject) = 0;

        /*
        * Get the UObject* bound to the C# Object, 
        * which is obtained by invoke the GetNativePtr method of the C# UObject class.
        */
        virtual UObject*                        GetUnrealObject(void* InCSharpObject) = 0;        
    };
}
