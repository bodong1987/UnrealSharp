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
#include "MonoRuntime/MonoMethod.h"
#include "Misc/ScopedExit.h"

#if WITH_MONO
namespace UnrealSharp::Mono
{
	FMonoMethod::FMonoMethod(MonoMethod* InMethod) :
		Method(InMethod)
	{
		checkSlow(InMethod);

		Flags = mono_method_get_flags(InMethod, nullptr);

		MonoMethodSignature* signature = mono_method_signature(InMethod);

		ParamCount = mono_signature_get_param_count(signature);

#if UE_BUILD_DEBUG
		AssemblyName = GetMethodAssemblyName(InMethod);
		FullyQualifiedMethodName = GetMethodFullName(InMethod, true);
#endif
	}

#define METHOD_ATTRIBUTE_STATIC                    0x0010
#define METHOD_ATTRIBUTE_FINAL                     0x0020
#define METHOD_ATTRIBUTE_VIRTUAL                   0x0040

	bool FMonoMethod::IsVirtual() const
	{
		return (Flags & METHOD_ATTRIBUTE_VIRTUAL) != 0;
	}

	bool FMonoMethod::IsStatic() const
	{
		return (Flags & METHOD_ATTRIBUTE_STATIC) != 0;
	}

	bool FMonoMethod::IsFinal() const
	{
		return (Flags & METHOD_ATTRIBUTE_FINAL) != 0;
	}

	int FMonoMethod::GetParameterCount() const
	{
		return ParamCount;
	}

	FString FMonoMethod::GetMethodAssemblyName(MonoMethod* InMonoMethod)
	{
		check(InMonoMethod != nullptr);

		MonoClass* klass = mono_method_get_class(InMonoMethod);
		check(klass != nullptr);

		MonoImage* image = mono_class_get_image(klass);
		check(image != nullptr);

		MonoAssembly* assembly = mono_image_get_assembly(image);
		check(assembly != nullptr);

		MonoAssemblyName* AssemblyName = mono_assembly_get_name(assembly);
		const char* assemblyName = mono_assembly_name_get_name(AssemblyName);

		FString Result = ANSI_TO_TCHAR(assemblyName);

		return Result;
	}

	FString FMonoMethod::GetMethodFullName(MonoMethod* InMonoMethod, bool bSignatrure)
	{
		char* fullname = mono_method_full_name(InMonoMethod, bSignatrure);
		UNREALSHARP_SCOPED_EXIT(mono_free(fullname));

		FString Result = ANSI_TO_TCHAR(fullname);

		return Result;
	}
}
#endif

