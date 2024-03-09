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

#if WITH_MONO

#include "MonoRuntime/Mono.h"
#include "Misc/InteropUtils.h"

namespace UnrealSharp::Mono
{
	class FMonoRuntime;
	class FMonoInteropUtils : public FInteropUtils
	{
	public:
		typedef TMap<uint32, TTuple<FString, void*>> FallbackApiMappingType;

		static void						Initialize(FMonoRuntime* InRuntime);
		static void						Uninitialize();

	public:
		static FString					GetFString(MonoString* InMonoString);		
		static MonoString*				GetMonoString(const FString& InString);
		static MonoString*				GetMonoString(const FStringView& InStringView);

		static void						DumpMonoObjectInformation(MonoObject* InMonoObject);		
		static void						DumpAssemblyClasses(MonoAssembly* InAssembly);
		static void						DumpClassInfomration(MonoClass* InClass);


	private:
        static void						Bind();
        static void*					MonoPInvokeLoadLib(const char* name, int flags, char** err, void* InUserData);
        static void*					MonoPInvokeGetSymbol(void* handle, const char* name, char** err, void* InUserData);
        static void*					MonoPInvokeFallbackClose(void* handle, void* InUserData);

	public:
		static FMonoRuntime*			Runtime;
		static FallbackApiMappingType   FallbackApis;
	};
}
#endif
