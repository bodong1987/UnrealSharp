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
#include "Classes/CSharpClass.h"
#include "ICSharpRuntime.h"
#include "IUnrealFunctionInvokeRedirector.h"
#include "UnrealFunctionInvokeRedirector.h"
#include "ICSharpLibraryAccessor.h"
#include "Misc/InteropUtils.h"

bool FCSharpFunctionArgumentData::IsPassByReference() const
{
    return (Flags & CPF_ReferenceParm) != 0 ||
        (Flags & CPF_OutParm) != 0;
}

bool FCSharpFunctionArgumentData::IsReturnValue() const
{
    return (Flags & CPF_ReturnParm) != 0;
}

FCSharpFunctionRedirectionData::FCSharpFunctionRedirectionData() :
    Function(nullptr),
    FunctionData(nullptr),
    Flags(FUNC_None),
    FuncPtr(nullptr)
{
}

FCSharpFunctionRedirectionData::FCSharpFunctionRedirectionData(UFunction* InFunction, const FCSharpFunctionData* InFunctionData) :
    Function(InFunction),
    FunctionData(InFunctionData),
    Flags(InFunction->FunctionFlags),
    FuncPtr(InFunction->GetNativeFunc()),
    Script(MoveTemp(InFunction->Script))
{
}

void UCSharpClass::ClearCSharpDataCaches()
{
    CSharpFunctions.Empty();
}

void UCSharpClass::AddCSharpFunction(const FName& InName, const FCSharpFunctionData& InFunctionData)
{
    CSharpFunctions.Add(InName, InFunctionData);
}

void UCSharpClass::AddCSharpFunction(FName&& InName, FCSharpFunctionData&& InFunctionData)
{
    CSharpFunctions.Add(InName, InFunctionData);
}

FString UCSharpClass::GetCSharpTypeName() const
{
    if (int Index; CSharpFullName.FindLastChar(TEXT('.'), Index))
    {
        return CSharpFullName.Mid(Index + 1);
    }

    return CSharpFullName;
}

const FString& UCSharpClass::GetCSharpFunctionSignature(const FName& InName) const
{
    if (const auto Ptr = CSharpFunctions.Find(InName); Ptr != nullptr)
    {
        return Ptr->FunctionSignature;
    }

    static const FString Z_Empty;
    return Z_Empty;
}

const FCSharpFunctionData* UCSharpClass::FindCSharpFunction(const FName& InName) const
{
    return CSharpFunctions.Find(InName);
}

void UCSharpClass::Bind()
{
    Super::Bind();

    if (ClassConstructor != StaticConstructor)
    {
        DefaultClassConstructor = ClassConstructor;
        ClassConstructor = StaticConstructor;
    }
}

void UCSharpClass::StaticClassConstructor(UCSharpClass* InCSharpClass, const FObjectInitializer& ObjectInitializer) // NOLINT
{
    check(InCSharpClass);

    if (UCSharpClass* CSharpSuperClass = Cast<UCSharpClass>(InCSharpClass->GetSuperClass()))
    {
        StaticClassConstructor(CSharpSuperClass, ObjectInitializer);
    }
    else
    {
        UnrealSharp::FCSharpObjectMarshalValue CSharpObject;

        if (UnrealSharp::FCSharpRuntimeFactory::IsGlobalCSharpRuntimeValid())
        {
            CSharpObject = UnrealSharp::FInteropUtils::GetCSharpObjectOfUnrealObject(ObjectInitializer.GetObj());

            const auto Runtime = UnrealSharp::FCSharpRuntimeFactory::GetInstance();
            check(Runtime);

            Runtime->GetCSharpLibraryAccessor()->BeforeObjectConstructor(CSharpObject.ObjectPtr, ObjectInitializer);
        }

        InCSharpClass->DefaultClassConstructor(ObjectInitializer);

        if (CSharpObject.ObjectPtr != nullptr)
        {
            const auto Runtime = UnrealSharp::FCSharpRuntimeFactory::GetInstance();
            check(Runtime);

            Runtime->GetCSharpLibraryAccessor()->PostObjectConstructor(CSharpObject.ObjectPtr);
        }
    }
}

void UCSharpClass::StaticConstructor(const FObjectInitializer& ObjectInitializer)
{
    // if you create an object which is a CSharpClass type
    // we need find the parent CSharpClass here
    UClass* TargetClass = ObjectInitializer.GetClass();
    check(TargetClass != nullptr);

    UCSharpClass* SharpClass = nullptr; // NOLINT

    while ((SharpClass = Cast<UCSharpClass>(TargetClass)) == nullptr)
    {
        TargetClass = TargetClass->GetSuperClass();

        if (TargetClass == nullptr)
        {
            break;
        }
    }

    check(SharpClass);
    
    StaticClassConstructor(SharpClass, ObjectInitializer);
}

void UCSharpClass::RedirectAllCSharpFunctions()
{
    for (auto& Func : CSharpFunctions)
    {
        UFunction* CSharpFunction = FindFunctionByName(Func.Key, EIncludeSuperFlag::ExcludeSuper);
        checkf(CSharpFunction, TEXT("Failed find %s[%s] on CSharp Class %s"), *Func.Key.ToString(), *Func.Value.FunctionSignature, *GetCSharpFullName());

        if (CSharpFunction->HasAnyFunctionFlags(FUNC_Native))
        {
            // already redirected ???
            continue;
        }

        FCSharpFunctionRedirectionData RedirectionData(CSharpFunction, &Func.Value);

        CSharpFunction->FunctionFlags |= FUNC_Native;
        CSharpFunction->SetNativeFunc(&UCSharpClass::CallCSharpFunction);

        RedirectionCaches.Add(CSharpFunction, RedirectionData);
    }
}

void UCSharpClass::RestoreAllCSharpFunctions()
{
    for (auto& Cache : RedirectionCaches)
    {
        Cache.Value.Function->FunctionFlags = Cache.Value.Flags;
        Cache.Value.Function->Script = MoveTemp(Cache.Value.Script);
        Cache.Value.Function->SetNativeFunc(Cache.Value.FuncPtr);        
    }

    RedirectionCaches.Empty();
}

FCSharpFunctionRedirectionData* UCSharpClass::GetCSharpFunctionRedirection(const UFunction* InFunction)
{
    return RedirectionCaches.Find(InFunction);
}

void UCSharpClass::CallCSharpFunction(UObject* Context, FFrame& TheStack, RESULT_DECL)
{
    UFunction* Func = TheStack.CurrentNativeFunction ? TheStack.CurrentNativeFunction : TheStack.Node;
    check(Func);

    UCSharpClass* Class = Cast<UCSharpClass>(Func->GetOuter());
    checkf(Class, TEXT("Only C# Binding function can use this method."));

    FCSharpFunctionRedirectionData* Data = Class->RedirectionCaches.Find(Func);

    checkf(Data != nullptr, TEXT("Failed find C# binding data for %s:%s"), *Class->CSharpFullName, *Func->GetName());

    if (!Data->Invoker)
    {
        UnrealSharp::ICSharpRuntime* Runtime = UnrealSharp::FCSharpRuntimeFactory::GetInstance();

        checkSlow(Runtime != nullptr);

        const FString& Signature = Class->GetCSharpFunctionSignature(*Func->GetName());

        checkf(!Signature.IsEmpty(), TEXT("missing C# method signature for: %s.%s"), *Class->CSharpFullName, *Func->GetName());

        TSharedPtr<UnrealSharp::ICSharpMethodInvocation> InvocationPtr = Runtime->CreateCSharpMethodInvocation(Class->AssemblyName, Signature);

        checkf(InvocationPtr, TEXT("Failed create invocation from signature (%s) in %s"), *Signature, *Class->CSharpFullName);

        const TSharedPtr<UnrealSharp::FUnrealFunctionInvokeRedirector> Invoker = 
            MakeShared<UnrealSharp::FUnrealFunctionInvokeRedirector>(
                Runtime, 
                Class, 
                Func, 
                Data->FunctionData, 
                InvocationPtr
            );

        Data->Invoker = Invoker;

        checkf(Data->Invoker, TEXT("Failed bind C# method %s:%s"), *Class->CSharpFullName, *Func->GetName());
    }

    Data->Invoker->Invoke(Context, TheStack, RESULT_PARAM);
}

