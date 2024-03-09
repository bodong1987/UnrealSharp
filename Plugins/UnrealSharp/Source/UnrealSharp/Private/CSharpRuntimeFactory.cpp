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
#include "ICSharpRuntime.h"
#include "MonoRuntime/MonoRuntime.h"
#include "Misc/CSharpFunctionRedirectionUtils.h"
#include "Misc/UnrealSharpLog.h"

namespace UnrealSharp
{
	TRefCountPtr<ICSharpRuntime> Z_GlobalCSharpRuntime;
	TRefCountPtr<ICSharpRuntime> FCSharpRuntimeFactory::RetainCSharpRuntime()
	{
		if (Z_GlobalCSharpRuntime)
		{
			return Z_GlobalCSharpRuntime;
		}
		
		US_LOG(TEXT("New CSharpRuntime instance."));

#if WITH_MONO
		Z_GlobalCSharpRuntime = TRefCountPtr<ICSharpRuntime>(new Mono::FMonoRuntime(), true);
#else
		return TSharedPtr<ICSharpRuntime>();
#endif

		checkf(Z_GlobalCSharpRuntime, TEXT("Failed create C# runtime."));

		US_LOG(TEXT("New C# runtime with type:%s"), *Z_GlobalCSharpRuntime->GetRuntimeType().ToString());

		ensureMsgf(Z_GlobalCSharpRuntime->Initialize(), TEXT("Failed Initialize C# runtime."));

		US_LOG(TEXT("Initialize C# runtime Success."));

		FCSharpFunctionRedirectionUtils::RedirectAllCSharpFunctions();

		US_LOG(TEXT("Redirect C# functions Success."));

		return Z_GlobalCSharpRuntime;
	}

	void FCSharpRuntimeFactory::ReleaseCSharpRuntime(TRefCountPtr<ICSharpRuntime>&& InRuntime)
	{
		InRuntime.SafeRelease();

		if (Z_GlobalCSharpRuntime->GetRefCount() == 1)
		{
			US_LOG(TEXT("This is Last C# Runtime, release it."));

			// this is the last one
			Z_GlobalCSharpRuntime->Shutdown();

			US_LOG(TEXT("Shutdown C# runtime success."));

			Z_GlobalCSharpRuntime.SafeRelease();

			check(!Z_GlobalCSharpRuntime.IsValid());

			FCSharpFunctionRedirectionUtils::RestoreAllCSharpFunctions();

			US_LOG(TEXT("restore all C# functions success."));
		}
	}

	bool FCSharpRuntimeFactory::IsGlobalCSharpRuntimeValid()
	{
		return Z_GlobalCSharpRuntime.IsValid();
	}

	ICSharpRuntime* FCSharpRuntimeFactory::GetInstance()
	{
		check(Z_GlobalCSharpRuntime);

		return Z_GlobalCSharpRuntime.GetReference();
	}
}

