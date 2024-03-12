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
    class ICSharpMethodInvocation;
    class ICSharpRuntime;

    // Data copy direction
    enum class EMarshalCopyDirection
    {
        UnrealToCSharp,
        CSharpToUnreal,

        // copy return value is not be same with CSharpToUnreal
        // for example: In Mono Runtime, value return an MonoObject*, you should unbox it, and then get the data pointer
        // but in common usage, the pointer is the data pointer directly, So we have to distinguish between these two situations
        CSharpReturnValueToUnreal 
    };

    // Marshaller represents data exchange between C# and C++.
    struct UNREALSHARP_API FPropertyMarshallerParameters
    {
        // Invocation target
        ICSharpMethodInvocation* Invocation;

        // property for this argument
        FProperty* Property;

        // input address
        void* InputAddress;

        // if you pass by reference, save your pointer in it
        // This is a pointer to a temporary buffer
        void** InputReferenceAddress;
                
        // if this argument pass by reference??        
        bool bPassAsReference;
    };

    /*
    * This interface will be used to handle the data exchange and mapping between unreal function parameters and C# function parameters. 
    * It will be responsible for converting unreal parameters into C# runtime-specific formats and submitting them to the C# runtime. 
    * In addition, it is necessary to handle the return value and the write-back operation of parameters to unreal when passing by reference.
    * 
    *   @Warning: These interfaces are used internally, and it is best not to use them directly externally unless you know what you are doing.
    */
    class UNREALSHARP_API IPropertyMarshaller
    {
    public:
        virtual ~IPropertyMarshaller() = default;

        /*
        * Most parameters do not change when converting from Unreal to C#, such as integers and floating point numbers. 
        * These types do not require additional storage space to complete data conversion. 
        * But there are some special cases such as structures. 
        * We need to first create the C# data of this structure on the C# side, then copy the Unreal data into it, 
        * and then use it for function calls, so some additional memory is needed to handle these special cases. 
        * So each IPropertyMarshaller can decide the size of the temporary memory area it needs.
        */
        virtual int                 GetTempParameterBufferSize() const = 0;

        /*
        * This interface is used to push parameters into the parameter list buffer of C#. 
        * After the IPropertyMarshaller of all parameters is executed, the Invoke operation can be performed.
        */
        virtual void                AddParameter(const FPropertyMarshallerParameters& InParameters) const = 0;

        /*
        * This interface is used for data exchange. 
        * Data can be copied from unreal to C# and back from C#. 
        * There are two points to note: 
        *   1. the copy return value is different from the parameter, which has been explained in the previous comments; 
        *   2. not every Property supports all copy operations, for example, containers and structures do not support copying from Unreal is copied directly to C#, 
        *      and these operations require special processes to handle. 
        *      So we can't assume that if you pass any parameters, the bottom layer will do it for you.
        */
        virtual void                Copy(const void* InUnrealDataPointer, const void* InCSharpDataPointer, FProperty* InProperty, EMarshalCopyDirection InCopyDirection) const = 0;
    };

}
