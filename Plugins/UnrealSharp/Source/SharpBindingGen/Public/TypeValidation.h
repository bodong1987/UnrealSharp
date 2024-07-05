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

class USharpBindingGenSettings;
namespace UnrealSharp
{
    enum class ETypeValidationFlags
    {
        WithNativeType = 1 << 0,
        WithBlueprintType = 1 << 1
    };

    inline bool operator & (ETypeValidationFlags InFlags, ETypeValidationFlags InFlags2)
    {
        return (static_cast<int>(InFlags) & static_cast<int>(InFlags2)) != 0;
    }

    inline ETypeValidationFlags operator | (ETypeValidationFlags InFlags, ETypeValidationFlags InFlags2)
    {
        return static_cast<ETypeValidationFlags>(static_cast<int>(InFlags) | static_cast<int>(InFlags2));
    }

    /*
    * Not all unreal classes, structures, functions, properties, etc. can support C#'s automatic binding, 
    * so we need an object to determine whether these types can be exported to C#. 
    * This class does this job.
    */
    class SHARPBINDINGGEN_API FTypeValidation
    {
    public:
        explicit FTypeValidation(const bool bAutoCheck = true);

        bool                                IsSupported(const UField* InField) const;
        bool                                IsNeedExport(const UField* InField) const;
        static FString                      GetFieldCheckedName(const UField* InField);

        void                                Reset(const bool bAutoCheck = false);

        const TSet<UField*>&                GetSupportedFields() const { return SupportedFields; }
        const TSet<UField*>&                GetUnSupportedFields() const { return UnSupportedFields; }

    private:
        bool                                ValidateField(UField* InField);
                
        enum class ECheckResult
        {
            Undefined,
            Success,
            Failure
        };

        ECheckResult                        GetCheckResult(const UField* InField) const;
        bool                                AllowPackage(const UPackage* InPackage) const;

    private:
        const USharpBindingGenSettings*     GenSettings;
        TSet<UField*>                       UnSupportedFields;
        TSet<UField*>                       SupportedFields;
        TSet<UField*>                       DeprecatedFields;
        TSet<UField*>                       CSharpFields;
    };
}
