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

#include "CoreMinimal.h"

namespace UnrealSharp
{
    // C# FText
    struct UNREALSHARP_API FCSharpText
    {
        void* Text;
    };

    // C# Top Level Asset path
    struct UNREALSHARP_API FCSharpTopLevelAssetPath
    {
        FName PackageName;
        FName AssetName;

        FCSharpTopLevelAssetPath();
        FCSharpTopLevelAssetPath(const FTopLevelAssetPath& InAssetPath);
    };

    // C# SubClassOf
    struct UNREALSHARP_API FCSharpSubclassOf
    {
        const UClass* ClassPtr;
    };

    // There seem to be some problems with marshaling an Object* directly, so wrap it in a structure.
    struct FCSharpObjectMarshalValue
    {
        void* ObjectPtr = nullptr;
    };

    /*
    * Through this structure, you can obtain the Key and Value pointers of a Map at one time, which can reduce one interactive function call.
    */
    struct FMapKeyValueAddressPair
    {
        const void* KeyAddressPointer;
        void* ValueAddressPointer;
    };

    // platform definition
    enum class EUnrealSharpPlatform : uint8
    {
        Windows,
        Mac,
        Linux,
        IOS,
        Android
    };

    // build config
    enum class EUnrealSharpBuildConfiguration : uint8
    {
        Debug,
        Release,
    };

    /*
    * Represents the compilation configuration of UnrealSharp's C++ and C#. Used to match both ends. 
    * This is important, for example, in the editor configuration, the FName size is 12, 
    * and in the game configuration, the size is 8. If the two do not match, it will cause memory problems.
    */
    struct FUnrealSharpBuildInfo
    {
    public:
        EUnrealSharpPlatform            Platform;
        EUnrealSharpBuildConfiguration  Configuration;
        bool                            bWithEditor;

    public:
        // get C++ build info
        static FUnrealSharpBuildInfo    GetNativeBuildInfo();
        static FString                  GetPlatformString(EUnrealSharpPlatform InPlatform);
        static FString                  GetBuildConfigurationString(EUnrealSharpBuildConfiguration InConfiguration);
    };

    // convert C# marshal string to FString or const TCHAR*
#define UNREALSHARP_STRING_TO_TCHAR(str) UTF8_TO_TCHAR(str)
}
