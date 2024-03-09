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

#include "ICSharpMethodInvocation.h"
#include "Misc/StackMemory.h"

namespace UnrealSharp
{
	/*
	* The secondary encapsulation of ICSharpMethodInvocation.
	* it is based on the RAII mechanism and template functions, 
	* combined with the use of the macro UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION, 
	* making the invocation of C# methods more streamlined.
	* @example:
	*	UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(WriteSoftObjectPtrInvocation);
	*	WriteSoftObjectPtrInvocationInvoker.Invoke(nullptr, &InDestinationAddress, InSourceObjectInterface);
	* 
	*   Use a macro to define an Invoker, and then calling the C# method will be similar to calling the C# method directly, 
	*   without the need to manually call 
	*		BeinInvoke
	*		AddParameter
	*		Invoke
	*		EndInvoke, etc.
	*/
	class UNREALSHARP_API FScopedCSharpMethodInvocation
	{
	public:
		FScopedCSharpMethodInvocation(ICSharpMethodInvocation& InInvocationRef, const FStackMemory& InParameterBuffer);
		FScopedCSharpMethodInvocation(TSharedPtr<ICSharpMethodInvocation> InInvocationPtr, const FStackMemory& InParameterBuffer);
		FScopedCSharpMethodInvocation(TSharedPtr<ICSharpMethodInvocation>&& InInvocationPtr, const FStackMemory& InParameterBuffer);
		~FScopedCSharpMethodInvocation();

		// disable these operations
		FScopedCSharpMethodInvocation(const FScopedCSharpMethodInvocation&) = delete;
		FScopedCSharpMethodInvocation(FScopedCSharpMethodInvocation&&) = delete;
		FScopedCSharpMethodInvocation& operator = (const FScopedCSharpMethodInvocation&) = delete;
		FScopedCSharpMethodInvocation& operator = (FScopedCSharpMethodInvocation&&) = delete;

	public:
		void*						Invoke(void* InInstance);
		void*						Invoke(void* InInstance, TUniquePtr<ICSharpMethodInvocationException>& OutException);

		ICSharpMethodInvocation*	GetInvocation() { return &Invocation; }

		void						AddArgument(void* InArgumentPtr);

		template <typename... T>
		inline void*				Invoke(void* InInstance, T*... args)
		{
			AddParameter<T...>(args...);

			return Invoke(InInstance);
		}

	private:
		template <typename T, typename... TExtraTypes>
		inline void					AddParameter(T* args, TExtraTypes* ... extraArgs)
		{
			AddArgument((void*)args);

			if constexpr (sizeof...(TExtraTypes) > 0)
			{
				AddParameter(extraArgs...);
			}
		}

	private:
		ICSharpMethodInvocation&			Invocation;
	};

// don't use FScopedCSharpMethodInvocation directly
// use this macro.
#define UNREALSHARP_SCOPED_CSHARP_METHOD_INVOCATION(name) \
		checkSlow(name); \
		const int name##ParameterCount = name->GetCSharpFunctionParameterCount(); \
		const int name##ParameterBufferSize = sizeof(void*)*name##ParameterCount; \
		void* name##ParameterBuffer = name##ParameterBufferSize > 0 ? FMemory_Alloca(name##ParameterBufferSize) : nullptr; \
		::UnrealSharp::FScopedCSharpMethodInvocation name##Invoker(name, {name##ParameterBuffer, name##ParameterBufferSize})
}