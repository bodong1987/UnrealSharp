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
#include "Misc/InteropUtils.h"

namespace UnrealSharp
{
    const TCHAR* FInteropUtils::GetTextCSharpMarshalStringFromUnrealText(const FText* InTextPtr)
    {
        if (InTextPtr == nullptr)
        {
            return nullptr;
        }

        static thread_local FString S_Temp; // NOLINT
        S_Temp = InTextPtr->ToString();

        return *S_Temp;
    }

    const TCHAR* FInteropUtils::GetTextCSharpMarshalStringFromCSharpString(const char* InCSharpMarshalString)
    {
        if (InCSharpMarshalString == nullptr)
        {
            return nullptr;
        }

        const FText Text = FText::FromStringView(US_STRING_TO_TCHAR(InCSharpMarshalString));

        return GetTextCSharpMarshalStringFromUnrealText(&Text);
    }

    void FInteropUtils::SetUnrealTextFromCSharpString(FText* InTextPtr, const char* InCSharpMarshalString)
    {
        *InTextPtr = FText::FromStringView(US_STRING_TO_TCHAR(InCSharpMarshalString));
    }
}

