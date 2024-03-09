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

#include "CSharpObjectHandle.h"
#include "ICSharpRuntime.h"

namespace UnrealSharp
{
	FCSharpObjectHandle::FCSharpObjectHandle() :
		State(ECSharpObjectHandleState::Reset)
	{
	}

	FCSharpObjectHandle::FCSharpObjectHandle(ICSharpRuntime* InRuntime, void* InCSharpObject, bool bInWeakReference) :
		Runtime(InRuntime)
	{
		checkSlow(InRuntime);

		Handle = InRuntime->CreateCSharpGCHandle(InCSharpObject, bInWeakReference);

		check(Handle);

		State = bInWeakReference ? ECSharpObjectHandleState::WeakReferenced : ECSharpObjectHandleState::Referenced;
	}

	FCSharpObjectHandle::~FCSharpObjectHandle()
	{
	}

	bool FCSharpObjectHandle::IsValid() const
	{
		return Handle && Handle->IsValid();
	}

	void* FCSharpObjectHandle::GetObject() const
	{
		return Handle ? Handle->GetObject() : nullptr;
	}

	void FCSharpObjectHandle::Reset()
	{
		State = ECSharpObjectHandleState::Reset;
		Handle.Reset();
	}

	void FCSharpObjectHandle::SetState(ECSharpObjectHandleState InState)
	{
		if (InState == ECSharpObjectHandleState::Reset)
		{
			Reset();
		}
		else if (IsValid() && State != InState)
		{
			State = InState;

			void* TargetObject = Handle->GetObject();
			check(TargetObject);

			Handle = Runtime->CreateCSharpGCHandle(TargetObject, State == ECSharpObjectHandleState::WeakReferenced);
		}		
	}
}
