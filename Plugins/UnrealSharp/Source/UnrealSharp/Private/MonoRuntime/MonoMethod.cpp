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

        MonoMethodSignature* Signature = mono_method_signature(InMethod);

        ParamCount = mono_signature_get_param_count(Signature);

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

        MonoClass* Klass = mono_method_get_class(InMonoMethod);
        check(Klass != nullptr);

        MonoImage* Image = mono_class_get_image(Klass);
        check(Image != nullptr);

        MonoAssembly* Assembly = mono_image_get_assembly(Image);
        check(Assembly != nullptr);

        MonoAssemblyName* AssemblyName = mono_assembly_get_name(Assembly);
        const char* assemblyName = mono_assembly_name_get_name(AssemblyName); // NOLINT

        FString Result = ANSI_TO_TCHAR(assemblyName);

        return Result;
    }

    FString FMonoMethod::GetMethodFullName(MonoMethod* InMonoMethod, bool bSignature)
    {
        char* Fullname = mono_method_full_name(InMonoMethod, bSignature);
        US_SCOPED_EXIT(mono_free(Fullname));

        FString Result = ANSI_TO_TCHAR(Fullname);

        return Result;
    }
}
#endif

