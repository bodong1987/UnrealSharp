#include "MonoRuntime/MonoInteropUtils.h"
#include "Misc/UnrealSharpLog.h"

#if WITH_MONO
#include "MonoRuntime/MonoType.h"
#include "MonoRuntime/MonoRuntime.h"
#include "Misc/ScopedExit.h"

namespace UnrealSharp::Mono
{
    FMonoRuntime* FMonoInteropUtils::Runtime = nullptr;
    TMap<uint32, TTuple<FString, void*>> FMonoInteropUtils::FallbackApis;

    static inline uint32 CalcHashFast(const char* p, uint32 s) // NOLINT
    {
        uint32 Result = 0;
        for (uint32 i = 0; i < s; ++i)
        {
            constexpr uint32 Prime = 31;
            Result = p[i] + Result * Prime;
        }
        return Result;
    }

    void* FMonoInteropUtils::MonoPInvokeLoadLib(const char* name, int flags, char** err, void* InUserData) // NOLINT
    {
        if (name != nullptr && FPlatformString::Stricmp(name, "UnrealSharp") == 0)
        {
            return &FallbackApis;
        }

        return nullptr;
    }

    void* FMonoInteropUtils::MonoPInvokeGetSymbol(void* handle, const char* name, char** err, void* InUserData) // NOLINT
    {
        if (name == nullptr)
        {
            return nullptr;
        }

        const uint32 HashCode = CalcHashFast(name, strlen(name));

        const auto* Value = FallbackApis.Find(HashCode);

        return Value != nullptr ? Value->Get<1>() : nullptr;
    }

    void* FMonoInteropUtils::MonoPInvokeFallbackClose(void* handle, void* InUserData) // NOLINT
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

        const auto UTF16Text = mono_string_to_utf16(InMonoString);

        US_SCOPED_EXIT(mono_free(UTF16Text));

        FString Text(UTF16Text);

        return Text;
    }

    MonoString* FMonoInteropUtils::GetMonoString(const FString& InString)
    {
        MonoString* String = mono_string_new_utf16(Runtime->GetDomain(), (const mono_unichar2*)*InString, InString.Len()); // NOLINT

        return String;
    }

    MonoString* FMonoInteropUtils::GetMonoString(const FStringView& InStringView)
    {
        MonoString* String = mono_string_new_utf16(Runtime->GetDomain(), (const mono_unichar2*)InStringView.GetData(), InStringView.Len());// NOLINT

        return String;
    }

    void FMonoInteropUtils::Bind()
    {
#define __PP_TEXT(name) #name /* NOLINT */
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

        MonoClass* Klass = mono_object_get_class(InMonoObject); 

        const char* ClassNamespace = mono_class_get_namespace(Klass);
        const char* ClassName = mono_class_get_name(Klass);

        MonoType* Type = mono_class_get_type(Klass);
        const char* FullTypeName = mono_type_get_name(Type);

        US_LOG(TEXT("Class Information of MonoObject:%p => %s.%s, Full Type Name: %s"), InMonoObject, ANSI_TO_TCHAR(ClassNamespace), ANSI_TO_TCHAR(ClassName), ANSI_TO_TCHAR(FullTypeName));
    }

    void FMonoInteropUtils::DumpAssemblyClasses(MonoAssembly* InAssembly)
    {
        MonoImage* Image = mono_assembly_get_image(InAssembly);
        check(Image);

        MonoAssemblyName* AssemblyName = mono_assembly_get_name(InAssembly);
        const char* assemblyName = mono_assembly_name_get_name(AssemblyName); // NOLINT

        check(assemblyName);
        US_LOG(TEXT("Assembly:%s"), ANSI_TO_TCHAR(assemblyName));

        const MonoTableInfo* Table = mono_image_get_table_info(Image, MONO_TABLE_TYPEDEF);

        const int Rows = mono_table_info_get_rows(Table);

        for (int i = 1; i < Rows; i++)
        {
            uint32_t Cols[MONO_TYPEDEF_SIZE];
            mono_metadata_decode_row(Table, i, Cols, MONO_TYPEDEF_SIZE);

            const char* Name = mono_metadata_string_heap(Image, Cols[MONO_TYPEDEF_NAME]);
            const char* Namespace = mono_metadata_string_heap(Image, Cols[MONO_TYPEDEF_NAMESPACE]);

            if (MonoClass* monoClass = mono_class_from_name(Image, Namespace, Name)) // NOLINT
            {
                DumpClassInformation(monoClass);
            }
        }
    }

    void FMonoInteropUtils::DumpClassInformation(MonoClass* InClass)
    {
        const char* ClassNamespace = mono_class_get_namespace(InClass);
        const char* ClassName = mono_class_get_name(InClass);

        US_LOG(TEXT("  Class:%s.%s"), ANSI_TO_TCHAR(ClassNamespace), ANSI_TO_TCHAR(ClassName));

        void* Iter = nullptr;
        MonoMethod* Method;
        while ((Method = mono_class_get_methods(InClass, &Iter)) != nullptr)
        {
            const char* MethodName = mono_method_get_name(Method);
            const char* MethodFullName = mono_method_full_name(Method, false);
            const char* MethodSignature = mono_method_full_name(Method, true);

            US_SCOPED_EXIT(mono_free((void*)MethodFullName); mono_free((void*)MethodSignature);); // NOLINT

            MonoMethodDesc* Desc = mono_method_desc_from_method(Method);
            check(Desc);

            US_SCOPED_EXIT(mono_method_desc_free(Desc));

            US_LOG(TEXT("    Method: %s [%s][%s]\n"), ANSI_TO_TCHAR(MethodName), ANSI_TO_TCHAR(MethodFullName), ANSI_TO_TCHAR(MethodSignature));
        }
    }
}
#endif
