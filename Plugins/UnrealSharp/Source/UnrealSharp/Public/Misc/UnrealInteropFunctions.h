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
    class FUnrealInteropFunctions;
    struct FUnrealSharpBuildInfo;

    /*
    * This data carries all the key information used for interactive communication between C++ and C#. 
    * Subsequent operations can be combined or used by these basic information.
    */
    struct UNREALSHARP_API FUnrealInteropFunctionsInfo
    {
        // sizeof this structure
        int                                 SizeOfThis;

        // interop function pointers instance
        const FUnrealInteropFunctions*      Instance;

        // get interop function pointer
        void*                               GetUnrealInteropFunctionPointerFunc;

        // log message
        void*                               LogMessageFunctionPointerFunc;


        // engine versions
        // C# will check this for Compatibility testing
        int                                 UnrealMajorVersion = ENGINE_MAJOR_VERSION;
        int                                 UnrealMinorVersion = ENGINE_MINOR_VERSION;
        int                                 UnrealPatchVersion = ENGINE_PATCH_VERSION;
    };

    /*
    * To exchange function containers, we map interactive function pointers through strings. 
    * On the C# side, we access these interactive functions through delegate* unmanaged, that is, C# function pointers, 
    * and bind them through function names at runtime.
    * @see also: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/function-pointers
    * @warning : 
    *    All interactive function names must be globally unique, otherwise conflicts will occur, 
    *    which may cause the application to crash due to mismatched signatures.
    */
    class UNREALSHARP_API FUnrealInteropFunctions
    {
    public:
        FUnrealInteropFunctions();
        ~FUnrealInteropFunctions();        
                        
        // register a new interop function
        bool                                AddInteropFunction(const TCHAR* InFunctionName, void* InFunc, bool bInAllowOverride = false);

        // register a new interop function
        bool                                AddInteropFunction(const FString& InFunctionName, void* InFunc, bool bInAllowOverride = false);

        // get interop function by name
        void*                               GetInteropFunction(const FString& InFunctionName) const;

        // remove interop function
        void                                RemoveInteropFunction(const FString& InFunctionName);

    private:                           
        // setup base interop functions
        void                                SetupBaseInteropFunctions();

        // setup internal interop functions;
        void                                SetupInternalInteropFunctions();
    public:
        static FUnrealInteropFunctions*     GetUnrealInteropFunctionsPtr();
        static FUnrealInteropFunctionsInfo* GetInteropFunctionsInfoPtr();

        // Verify the compilation configuration information of C++ and C#. 
        // Mismatched or incompatible C# code and compilation configuration will cause errors.
        static void                         ValidateUnrealSharpBuildInfo(const FUnrealSharpBuildInfo* InBuildInfo);

        // get interop function pointer
        static void*                        GetUnrealInteropFunctionPointer(const FUnrealInteropFunctions* InInstance, const char* InCSharpText);

        // log message
        static void                         LogMessage(int InLevel, const char* InMessage);
        
    private:
        TMap<FString, void*>                InteropFunctions;
    };

#define US_ADD_GLOBAL_INTEROP_FUNCTION(InteropFunctionsName, FunctionName) \
    InteropFunctionsName->AddInteropFunction(TEXT(#FunctionName), (void*)FunctionName)

#define US_ADD_GENERATED_GLOBAL_INTEROP_FUNCTION_HELPER(FunctionName) \
    US_ADD_GLOBAL_INTEROP_FUNCTION(InInteropFunctions, FunctionName)
}
