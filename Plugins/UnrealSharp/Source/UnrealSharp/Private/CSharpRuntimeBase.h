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

#include "ICSharpRuntime.h"

namespace UnrealSharp
{
	class ICSharpLibraryAccessor;
	class ICSharpObjectTable;

	class FCSharpStructFactory;

	/*
	* The public implementation of C# Runtime implements some common capabilities.
	*/
	class UNREALSHARP_API FCSharpRuntimeBase : public ICSharpRuntime
	{
	public:
		// Initialize runtime
		virtual bool											Initialize() override final;

		// stop runtime running
		virtual void											Shutdown() override final;

		virtual TSharedPtr<ICSharpMethodInvocation>				CreateCSharpMethodInvocation(TSharedPtr<ICSharpMethod> InMethod) = 0;
		virtual TSharedPtr<ICSharpMethodInvocation>				CreateCSharpMethodInvocation(const FString& InAssemblyName, const FString& InFullyQualifiedMethodName) = 0;

		virtual TSharedPtr<ICSharpType>							LookupType(const FString& InAssemblyName, const FString& InNamespace, const FString& InName) = 0;
		virtual TSharedPtr<ICSharpType>							LookupType(const FString& InAssemblyName, const FString& InFullName) override;

		virtual ICSharpLibraryAccessor*							GetCSharpLibraryAccessor() override;
		virtual ICSharpObjectTable*								GetObjectTable() override;		
	protected:
		virtual bool                                            InitializeInternal() = 0;
		virtual void                                            ShutdownInternal() = 0;

		virtual void											PostInitialized();
		virtual void											BeforeShutdown();		

		virtual TSharedPtr<ICSharpLibraryAccessor>				CreateCSharpLibraryAccessor();
		virtual TSharedPtr< ICSharpObjectTable>					CreateCSharpObjectTable();
		void													InvokeMain();

	protected:
		TSharedPtr<ICSharpLibraryAccessor>						CSharpLibraryAccessorPtr;
		TSharedPtr<ICSharpObjectTable>							ObjectTablePtr;
		TMap<const UStruct*, TSharedPtr<FCSharpStructFactory>>	StructFactories;

		TMap<const UField*, FString>                            CSharpFullPathDict;
	};
}
