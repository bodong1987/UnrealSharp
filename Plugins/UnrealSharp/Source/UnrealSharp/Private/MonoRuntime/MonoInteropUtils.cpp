#include "MonoRuntime/MonoInteropUtils.h"
#include "Misc/UnrealSharpUtils.h"
#include "ICSharpLibraryAccessor.h"
#include "ICSharpObjectTable.h"
#include "Misc/UnrealSharpLog.h"

#if WITH_MONO
#include "Misc/UnrealFunctionInvocation.h"
#include "Misc/CSharpStructures.h"
#include "MonoRuntime/MonoType.h"
#include "MonoRuntime/MonoMethod.h"
#include "MonoRuntime/MonoRuntime.h"
#include "Misc/ScopedExit.h"

namespace UnrealSharp::Mono
{
    FMonoRuntime* FMonoInteropUtils::Runtime = nullptr;
    TMap<uint32, TTuple<FString, void*>> FMonoInteropUtils::FallbackApis;

    static inline uint32 CalcHashFast(const char* p, uint32 s)
    {
        uint32 result = 0;
        const uint32 prime = 31;
        for (uint32 i = 0; i < s; ++i)
        {
            result = p[i] + (result * prime);
        }
        return result;
    }

    void* FMonoInteropUtils::MonoPInvokeLoadLib(const char* name, int flags, char** err, void* InUserData)
    {
        if (name != nullptr && FPlatformString::Stricmp(name, "UnrealSharp") == 0)
        {
            return &FallbackApis;
        }

        return nullptr;
    }

    void* FMonoInteropUtils::MonoPInvokeGetSymbol(void* handle, const char* name, char** err, void* InUserData)
    {
        if (name == nullptr)
        {
            return nullptr;
        }

        uint32 HashCode = CalcHashFast(name, strlen(name));

        const auto* Value = FallbackApis.Find(HashCode);

        return Value != nullptr ? Value->template Get<1>() : nullptr;
    }

    void* FMonoInteropUtils::MonoPInvokeFallbackClose(void* handle, void* InUserData)
    {
        return nullptr;
    }

    void FMonoInteropUtils::Initialize(FMonoRuntime* InRuntime)
    {
        check(InRuntime);
        Runtime = InRuntime;

        FallbackApis.Empty();

        Bind();

        mono_dl_fallback_register(MonoPInvokeLoadLib, MonoPInvokeGetSymbol, MonoPInvokeFallbackClose, nullptr);
    }

    void FMonoInteropUtils::Uninitialize()
    {
        FallbackApis.Empty();
        Runtime = nullptr;
    }

    FString FMonoInteropUtils::GetFString(MonoString* InMonoString)
    {
        if (InMonoString == nullptr)
        {
            return FString();
        }

        auto utf16Text = mono_string_to_utf16(InMonoString);

        UNREALSHARP_SCOPED_EXIT(mono_free(utf16Text));

        FString Text(utf16Text);

        return Text;
    }

    MonoString* FMonoInteropUtils::GetMonoString(const FString& InString)
    {
        MonoString* monoString = mono_string_new_utf16(Runtime->GetDomain(), (const mono_unichar2*)*InString, InString.Len());

        return monoString;
    }

    MonoString* FMonoInteropUtils::GetMonoString(const FStringView& InStringView)
    {
        MonoString* monoString = mono_string_new_utf16(Runtime->GetDomain(), (const mono_unichar2*)InStringView.GetData(), InStringView.Len());

        return monoString;
    }

    void FMonoInteropUtils::Bind()
    {
#define __PP_TEXT(name) #name
#define PP_TEXT(name) __PP_TEXT(name)
#define REGISTER_FALLBACK_API(name) \
		FallbackApis.Add(CalcHashFast("UnrealSharp_" PP_TEXT(name), strlen("UnrealSharp_" PP_TEXT(name))), MakeTuple<FString, void*>(TEXT("UnrealSharp_") TEXT(PP_TEXT(name)), (void*)&FMonoInteropUtils::name))

#define DECLARE_UNREAL_SHARP_INTEROP_API(returnType, name, parameters) \
			REGISTER_FALLBACK_API(name)

        // use interop function instead.
        // REGISTER_FALLBACK_API(LogMessage);

#undef DECLARE_UNREAL_SHARP_INTEROP_API
#undef REGISTER_FALLBACK_API
#undef PP_TEXT
#undef __PP_TEXT

    }
    void FMonoInteropUtils::DumpMonoObjectInformation(MonoObject* InMonoObject)
    {
        check(InMonoObject);

        MonoClass* kclass = mono_object_get_class(InMonoObject);

        const char* ClassNamespace = mono_class_get_namespace(kclass);
        const char* ClassName = mono_class_get_name(kclass);

        MonoType* type = mono_class_get_type(kclass);
        const char* FullTypeName = mono_type_get_name(type);

        US_LOG(TEXT("Class Information of MonoObject:%p => %s.%s, Full Type Name: %s"), InMonoObject, ANSI_TO_TCHAR(ClassNamespace), ANSI_TO_TCHAR(ClassName), ANSI_TO_TCHAR(FullTypeName));
    }

    void FMonoInteropUtils::DumpAssemblyClasses(MonoAssembly* InAssembly)
    {
        MonoImage* image = mono_assembly_get_image(InAssembly);
        check(image);

        MonoAssemblyName* AssemblyName = mono_assembly_get_name(InAssembly);
        const char* assemblyName = mono_assembly_name_get_name(AssemblyName);

        check(assemblyName);
        US_LOG(TEXT("Assembly:%s"), ANSI_TO_TCHAR(assemblyName));

        const MonoTableInfo* table = mono_image_get_table_info(image, MONO_TABLE_TYPEDEF);

        int rows = mono_table_info_get_rows(table);

        for (int i = 1; i < rows; i++)
        {
            uint32_t cols[MONO_TYPEDEF_SIZE];
            mono_metadata_decode_row(table, i, cols, MONO_TYPEDEF_SIZE);

            const char* Name = mono_metadata_string_heap(image, cols[MONO_TYPEDEF_NAME]);
            const char* Namespace = mono_metadata_string_heap(image, cols[MONO_TYPEDEF_NAMESPACE]);

            MonoClass* monoClass = mono_class_from_name(image, Namespace, Name);

            if (monoClass)
            {
                DumpClassInfomration(monoClass);
            }
        }
    }

    void FMonoInteropUtils::DumpClassInfomration(MonoClass* InClass)
    {
        const char* ClassNamespace = mono_class_get_namespace(InClass);
        const char* ClassName = mono_class_get_name(InClass);

        US_LOG(TEXT("  Class:%s.%s"), ANSI_TO_TCHAR(ClassNamespace), ANSI_TO_TCHAR(ClassName));

        void* iter = NULL;
        MonoMethod* method;
        while ((method = mono_class_get_methods(InClass, &iter)) != nullptr)
        {
            const char* MethodName = mono_method_get_name(method);
            const char* MethodFullName = mono_method_full_name(method, false);
            const char* MethodSignature = mono_method_full_name(method, true);

            UNREALSHARP_SCOPED_EXIT(mono_free((void*)MethodFullName); mono_free((void*)MethodSignature););

            MonoMethodDesc* desc = mono_method_desc_from_method(method);
            check(desc);

            UNREALSHARP_SCOPED_EXIT(mono_method_desc_free(desc));

            US_LOG(TEXT("    Method: %s [%s][%s]\n"), ANSI_TO_TCHAR(MethodName), ANSI_TO_TCHAR(MethodFullName), ANSI_TO_TCHAR(MethodSignature));
        }
    }
}
#endif
