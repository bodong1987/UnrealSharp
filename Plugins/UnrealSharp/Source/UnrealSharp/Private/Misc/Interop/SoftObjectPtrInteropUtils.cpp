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
    void FInteropUtils::ResetSoftObjectPtr(FSoftObjectPtr* InSoftObjectPtr)
    {
        if (InSoftObjectPtr)
        {
            InSoftObjectPtr->Reset();
        }
    }

    void FInteropUtils::ResetSoftObjectPtrWeakPtr(FSoftObjectPtr* InSoftObjectPtr)
    {
        if (InSoftObjectPtr)
        {
            InSoftObjectPtr->ResetWeakPtr();
        }
    }

    bool FInteropUtils::IsSoftObjectPtrPending(const FSoftObjectPtr* InSoftObjectPtr)
    {
        return InSoftObjectPtr != nullptr && InSoftObjectPtr->IsPending();
    }

    bool FInteropUtils::IsSoftObjectPtrValid(const FSoftObjectPtr* InSoftObjectPtr)
    {
        return InSoftObjectPtr != nullptr && InSoftObjectPtr->IsValid();
    }

    bool FInteropUtils::IsSoftObjectPtrStale(const FSoftObjectPtr* InSoftObjectPtr)
    {
        return InSoftObjectPtr != nullptr && InSoftObjectPtr->IsStale();
    }

    bool FInteropUtils::IsSoftObjectPtrNull(const FSoftObjectPtr* InSoftObjectPtr)
    {
        return InSoftObjectPtr != nullptr && InSoftObjectPtr->IsNull();
    }

    UObject* FInteropUtils::GetUnrealObjectPointerOfSoftObjectPtr(FSoftObjectPtr* InSoftObjectPtr)
    {
        if (InSoftObjectPtr == nullptr)
        {
            return nullptr;
        }

        return InSoftObjectPtr->Get();
    }

    UObject* FInteropUtils::GetUnrealObjectPointerOfSoftObjectPtrEx(FSoftObjectPtr* InSoftObjectPtr, bool evenIfPendingKill)
    {
        if (InSoftObjectPtr == nullptr)
        {
            return nullptr;
        }

        return ((TPersistentObjectPtr<FSoftObjectPath>*)InSoftObjectPtr)->Get(evenIfPendingKill);
    }

    FSoftObjectPath* FInteropUtils::GetObjectIdPointerOfSoftObjectPtr(FSoftObjectPtr* InSoftObjectPtr)
    {
        if (InSoftObjectPtr == nullptr)
        {
            return nullptr;
        }

        return &InSoftObjectPtr->GetUniqueID();
    }

    UObject* FInteropUtils::LoadSynchronousSoftObjectPtr(FSoftObjectPtr* InSoftObjectPtr)
    {
        return InSoftObjectPtr != nullptr ? InSoftObjectPtr->LoadSynchronous() : nullptr;
    }

    void FInteropUtils::CopySoftObjectPtr(FSoftObjectPtr* InDestination, const FSoftObjectPtr* InSource)
    {
        if (InDestination)
        {
            if (InSource)
            {
                *InDestination = *InSource;
            }
            else
            {
                InDestination->Reset();
            }
        }
    }
}

