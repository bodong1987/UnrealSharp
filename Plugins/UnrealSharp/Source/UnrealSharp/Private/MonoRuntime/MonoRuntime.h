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

#include "CSharpRuntimeBase.h"

#if WITH_MONO
#include "MonoRuntime/Mono.h"

namespace UnrealSharp::Mono
{
	class FMonoObjectTable;
	class FPropertyMarshallerCollection;

	class FMonoRuntime : public FCSharpRuntimeBase, public FRefCountBase
	{
	public:
		FMonoRuntime();
		~FMonoRuntime();

		virtual bool									InitializeInternal() override;
		virtual void									ShutdownInternal() override;
		virtual const FName&							GetRuntimeType() const;

		virtual TSharedPtr<ICSharpMethod>				LookupMethod(const FString& InAssemblyName, const FString& InFullyQualifiedMethodName) override;		
		virtual TSharedPtr<ICSharpMethod>				LookupMethod(ICSharpType* InType, const FString& InFullyQualifiedMethodName) override;

		virtual TSharedPtr<ICSharpType>					LookupType(const FString& InAssemblyName, const FString& InNamespace, const FString& InName) override;		
		virtual TSharedPtr<ICSharpMethodInvocation>		CreateCSharpMethodInvocation(TSharedPtr<ICSharpMethod> InMethod) override;
		virtual TSharedPtr<ICSharpMethodInvocation>		CreateCSharpMethodInvocation(const FString& InAssemblyName, const FString& InFullyQualifiedMethodName) override;
		virtual const IPropertyMarshaller*				GetPropertyMarshaller(FProperty* InProperty) const override;

		virtual TSharedPtr<ICSharpGCHandle>				CreateCSharpGCHandle(void* InCSharpObject, bool bInWeakReference) override;
		virtual void									ExecuteGarbageCollect(bool bFully) override;
		virtual TSharedPtr<ICSharpLibraryAccessor>		CreateCSharpLibraryAccessor() override; 

	public:
		virtual uint32									AddRef() const override{ return FRefCountBase::AddRef(); }
		virtual uint32									Release() const override{ return FRefCountBase::Release(); }
		virtual uint32									GetRefCount() const override{ return FRefCountBase::GetRefCount(); }
	public:
		inline MonoDomain*								GetDomain() const { return Domain; }

		MonoObject*										Invoke(MonoMethod* InMethod, MonoObject* Object, void** InArguments, MonoObject** OutException);
		MonoObject*										InvokeDelegate(MonoObject* InDelegate, void** InArguments, MonoObject** OutException);
		
		MonoMethod*										LoadMethod(MonoImage* InImage, const char* InFullyQualifiedMethodName);
		MonoMethod*										LoadMethod(MonoClass* InClass, const char* InFullyQualifiedMethodName);
		MonoMethod*										LoadMethod(const TCHAR* AssemblyName, const char* InFullyQualifiedMethodName);
				
		void											LogException(MonoObject* InException);

		static void										MonoStringToFString(FString& Result, MonoString* InString);
		static FName									MonoStringToFName(MonoString* InString);
		static void										SendErrorToMessageLog(FText InError);
	private:
		void											InitDebugger();
		bool											InitDomain();
		void											InitLogger();					

		static MonoAssembly*							OnAssemblyLoaded(MonoAssemblyName* aname, char** InAssemblies, void* InUserData);
		static FString									SearchLibrary(const FString& InName);

		static void										InitLibrarySearchPaths();
		static void										MonoLog(const char* InDomainName, const char* InLogLevel, const char* InMessage, mono_bool InFatal, void* InUserData);
		static void										MonoPrintf(const char* InString, mono_bool bIsStdout);

		struct FMonoAssemblyCache
		{
			MonoAssembly* Assembly = nullptr;
			MonoImage* Image = nullptr;

			bool IsValid() const { return Assembly != nullptr && Image != nullptr; }
		};

		FMonoAssemblyCache								LoadAssembly(const FString& InAssemblyName);

	private:
		void*											LibraryHandle = nullptr;
		MonoDomain*										Domain = nullptr;		
		TMap<FString, FMonoAssemblyCache>				AssemblyCaches;
		
		TUniquePtr<FPropertyMarshallerCollection>		MarshallerCollectionPtr;

		bool											bUseTempCoreCLRLibrary = false;
		bool											bIsDebuggerAvaialble = false;
        
#if PLATFORM_MAC
        TArray<void*>									ExtraLibraryHandles;
#endif
	public:
		static FString									NativeLibraryPath;
        static FString									ManagedLibraryPath;
		static TArray<FString>							LibrarySearchPaths;
	};
}
#endif
