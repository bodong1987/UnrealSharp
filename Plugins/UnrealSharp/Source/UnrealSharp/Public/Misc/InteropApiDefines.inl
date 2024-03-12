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

/*
* Functions declared here will be automatically registered as interactive functions. 
* If you need to add new interactive functions, be sure to follow the agreed format, 
* otherwise you will need to complete everything manually.
*/

#ifndef DECLARE_UNREAL_SHARP_INTEROP_API
#error "internal use, don't include this file directly'"
#endif

// Actor Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, GetActorWorld, (const AActor* InActor));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, GetActorGameInstance, (const AActor* InActor));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, SpawnActorByTransform, (UWorld* InWorld, UClass* InClass, const void* InTransformPtr, int InTransformSize));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, SpawnActor, (UWorld* InWorld, UClass* InClass, const FVector* InLocation, const FRotator* InRotation));


// Array Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(const FProperty*, GetElementPropertyOfArray, (const FArrayProperty* InArrayProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(int, GetLengthOfArray, (const void* InAddressOfArray, const FArrayProperty* InArrayProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(const void*, GetElementAddressOfArray, (const void* InAddressOfArray, const FArrayProperty* InArrayProperty, int InIndex));
DECLARE_UNREAL_SHARP_INTEROP_API(void, ClearArray, (const void* InAddressOfArray, const FArrayProperty* InArrayProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(const void*, InsertEmptyAtArrayIndex, (const void* InAddressOfArray, const FArrayProperty* InArrayProperty, int InIndex));
DECLARE_UNREAL_SHARP_INTEROP_API(void, RemoveAtArrayIndex, (const void* InAddressOfArray, const FArrayProperty* InArrayProperty, int InIndex));
DECLARE_UNREAL_SHARP_INTEROP_API(int, FindIndexOfArrayElement, (const void* InAddressOfArray, const FArrayProperty* InArrayProperty, const void* InAddressOfElement));

// Class Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, GetDefaultObjectOfClass, (const UClass* InClass));
DECLARE_UNREAL_SHARP_INTEROP_API(const UClass*, GetClassPointerOfUnrealObject, (const UObject* InObject));
DECLARE_UNREAL_SHARP_INTEROP_API(const UField*, LoadUnrealField, (const char* InCSharpFieldPathName));
DECLARE_UNREAL_SHARP_INTEROP_API(bool, CheckUClassIsChildOf, (const UClass* InTestClass, const UClass* InTestBaseClass));
DECLARE_UNREAL_SHARP_INTEROP_API(const UClass*, GetSuperClass, (const UClass* InClass));
DECLARE_UNREAL_SHARP_INTEROP_API(const FProperty*, GetProperty, (const UStruct* InStruct, const char* InCSharpPropertyName));
DECLARE_UNREAL_SHARP_INTEROP_API(const UFunction*, GetFunction, (const UClass* InClass, const char* InCSharpFunctionName));
DECLARE_UNREAL_SHARP_INTEROP_API(int, GetStructSize, (const UStruct* InStruct));
DECLARE_UNREAL_SHARP_INTEROP_API(void, InitializeStructData, (const UStruct* InStruct, const void* InAddressOfStructData));
DECLARE_UNREAL_SHARP_INTEROP_API(void, UninitializeStructData, (const UStruct* InStruct, const void* InAddressOfStructData));
DECLARE_UNREAL_SHARP_INTEROP_API(const TCHAR*, GetFieldCSharpFullPath, (const UField* InField));
DECLARE_UNREAL_SHARP_INTEROP_API(EClassFlags, GetClassFlags,(const UClass* InClass));

// Delegate Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(void, BindDelegate, (const void* InDelegateAddress, const FProperty* InProperty, UObject* InObject, const char* InCSharpFunctionName));
DECLARE_UNREAL_SHARP_INTEROP_API(void, UnbindDelegate, (const void* InDelegateAddress, const FProperty* InProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(void, ClearDelegate, (const void* InDelegateAddress, const FProperty* InProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(void, AddDelegate, (const void* InDelegateAddress, const FProperty* InProperty, UObject* InObject, const char* InCSharpFunctionName));
DECLARE_UNREAL_SHARP_INTEROP_API(void, RemoveDelegate, (const void* InDelegateAddress, const FProperty* InProperty, UObject* InObject, const char* InCSharpFunctionName));
DECLARE_UNREAL_SHARP_INTEROP_API(void, RemoveAllDelegate, (const void* InDelegateAddress, const FProperty* InProperty, UObject* InObject));

// Invocation Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(FUnrealFunctionInvocation*, CreateUnrealInvocation, (const UClass* InClass, const char* InCSharpFunctionName));
DECLARE_UNREAL_SHARP_INTEROP_API(FUnrealFunctionInvocation*, CreateUnrealInvocationFromDelegateProperty, (const FProperty* InDelegateProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(void, DestroyUnrealInvocation, (FUnrealFunctionInvocation* InInvocation));
DECLARE_UNREAL_SHARP_INTEROP_API(void, InvokeUnrealInvocation, (FUnrealFunctionInvocation* InInvocation, UObject* InObject, void* InParameterBuffer, int InParameterBufferSize));
DECLARE_UNREAL_SHARP_INTEROP_API(UFunction*, GetUnrealInvocationFunction, (FUnrealFunctionInvocation* InInvocation));
DECLARE_UNREAL_SHARP_INTEROP_API(int, GetUnrealInvocationParameterSize, (FUnrealFunctionInvocation* InInvocation));
DECLARE_UNREAL_SHARP_INTEROP_API(void, InitializeUnrealInvocationParameters, (FUnrealFunctionInvocation* InInvocation, void* InParameterBuffer, int InParameterBufferSize));
DECLARE_UNREAL_SHARP_INTEROP_API(void, UnInitializeUnrealInvocationParameters, (FUnrealFunctionInvocation* InInvocation, void* InParameterBuffer, int InParameterBufferSize));

// Map Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(const FProperty*, GetKeyPropertyOfMap, (const FMapProperty* InMapProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(const FProperty*, GetValuePropertyOfMap, (const FMapProperty* InMapProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(int, GetLengthOfMap, (const void* InAddressOfMap, const FMapProperty* InMapProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(void, ClearMap, (void* InAddressOfMap, const FMapProperty* InMapProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(const void*, GetKeyAddressOfMapElement, (void* InAddressOfMap, const FMapProperty* InMapProperty, int InIndex));
DECLARE_UNREAL_SHARP_INTEROP_API(void*, GetValueAddressOfMapElement, (void* InAddressOfMap, const FMapProperty* InMapProperty, int InIndex));
DECLARE_UNREAL_SHARP_INTEROP_API(FMapKeyValueAddressPair, GetAddressOfMapElement, (void* InAddressOfMap, const FMapProperty* InMapProperty, int InIndex));
DECLARE_UNREAL_SHARP_INTEROP_API(void*, FindValueAddressOfElementKey, (void* InAddressOfMap, const FMapProperty* InMapProperty, const void* InAddressOfKeyElementTarget));
DECLARE_UNREAL_SHARP_INTEROP_API(bool, TryAddNewElementToMap, (void* InAddressOfMap, const FMapProperty* InMapProperty, const void* InAddressOfKeyElementTarget, const void* InAddressOfValueElementTarget, bool InOverrideIfExists));
DECLARE_UNREAL_SHARP_INTEROP_API(bool, RemoveElementFromMap, (void* InAddressOfMap, const FMapProperty* InMapProperty, const void* InAddressOfKeyElementTarget));

// Misc Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(FGuid, MakeGuidFromString, (const char* InCSharpGuidString));

// Name Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(const TCHAR*, GetStringOfName, (const FName* InNamePtr));
DECLARE_UNREAL_SHARP_INTEROP_API(FName, GetNameOfString, (const char* InCSharpNameString));

// ObjectInitializer Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(const UClass*, GetClassOfObjectInitializer, (const FObjectInitializer* InObjectInitializer));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, GetObjectOfObjectInitializer, (const FObjectInitializer* InObjectInitializer));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, CreateDefaultSubobjectOfObjectInitializer, (const FObjectInitializer* InObjectInitializer, UObject* InOuter, const char* InSubobjectNameString, UClass* InReturnType, UClass* InClassToCreateByDefault, bool bIsRequired, bool bIsTransient));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, CreateEditorOnlyDefaultSubobjectOfObjectInitializer, (const FObjectInitializer* InObjectInitializer, UObject* InOuter, const char* InSubobjectNameString, UClass* InReturnType, bool bIsTransient));
DECLARE_UNREAL_SHARP_INTEROP_API(void, SetDefaultSubobjectClassOfObjectInitializer, (const FObjectInitializer* InObjectInitializer, const char* InSubobjectNameString, UClass* InClass));
DECLARE_UNREAL_SHARP_INTEROP_API(void, DoNotCreateDefaultSubobjectOfObjectInitializer, (const FObjectInitializer* InObjectInitializer, const char* InSubobjectNameString));
DECLARE_UNREAL_SHARP_INTEROP_API(void, SetNestedDefaultSubobjectClassOfObjectInitializer, (const FObjectInitializer* InObjectInitializer, const char* InSubobjectNameString, UClass* InClass));
DECLARE_UNREAL_SHARP_INTEROP_API(void, DoNotCreateNestedDefaultSubobjectOfObjectInitializer, (const FObjectInitializer* InObjectInitializer, const char* InSubobjectNameString));

// Object Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(const UObject*, GetDefaultUnrealObjectOfClass, (const UClass* InClass));
DECLARE_UNREAL_SHARP_INTEROP_API(UObject*, GetUnrealObjectOfCSharpObject, (const void* InCSharpObject));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, GetCSharpObjectOfUnrealObject, (const UObject* InObject));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, GetOuterOfUnrealObject, (const UObject* InObject));
DECLARE_UNREAL_SHARP_INTEROP_API(const TCHAR*, GetNameOfUnrealObject, (const UObject* InObject));
DECLARE_UNREAL_SHARP_INTEROP_API(const TCHAR*, GetPathNameOfUnrealObject, (const UObject* InObject));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, CreateDefaultSubobject, (UObject* InObject, const char* InCSharpSubobjectNameString, UClass* ReturnType, UClass* ClassToCreateByDefault, bool bIsRequired, bool bIsTransient));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, GetDefaultSubobjectByName, (UObject* InObject, const char* InCSharpSubobjectNameString));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, NewUnrealObject, (UObject* InOuter, UClass* InClass, const FName* InName, EObjectFlags InFlags, UObject* InTemplate, bool bInCopyTransientsFromClassDefaults));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, DuplicateUnrealObject, (UClass* InClass, const UObject* InSourceObject, UObject* InOuter, const FName* InName));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, GetUnrealTransientPackage, ());
DECLARE_UNREAL_SHARP_INTEROP_API(void, AddUnrealObjectToRoot, (UObject* InObject));
DECLARE_UNREAL_SHARP_INTEROP_API(void, RemoveUnrealObjectFromRoot, (UObject* InObject));
DECLARE_UNREAL_SHARP_INTEROP_API(bool, IsUnrealObjectRooted, (UObject* InObject));
DECLARE_UNREAL_SHARP_INTEROP_API(bool, IsUnrealObjectValid, (UObject* InObject));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, FindUnrealObjectFast, (UClass* InClass, UObject* InOuter, const FName* InName, bool bInExactClass, EObjectFlags InExclusiveFlags));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, FindUnrealObject, (UClass* InClass, UObject* InOuter, const char* InName, bool bInExactClass));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, FindUnrealObjectChecked, (UClass* InClass, UObject* InOuter, const char* InName, bool bInExactClass));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, FindUnrealObjectSafe, (UClass* InClass, UObject* InOuter, const char* InName, bool bInExactClass));
DECLARE_UNREAL_SHARP_INTEROP_API(FCSharpObjectMarshalValue, LoadUnrealObject, (UClass* InClass, UObject* InOuter, const char* InName, const char* InFileName, uint32 InLoadFlags, UPackageMap* InSandbox));


// Property Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(int, GetPropertyOffset, (const FProperty* InProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(int, GetPropertySize, (const FProperty* InProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(void, InitializePropertyData, (const FProperty* InProperty, void* InAddressOfPropertyValue));
DECLARE_UNREAL_SHARP_INTEROP_API(void, UnInitializePropertyData, (const FProperty* InProperty, void* InAddressOfPropertyValue));
DECLARE_UNREAL_SHARP_INTEROP_API(uint64, GetPropertyCastFlags, (const FProperty* InProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(const UField*, GetPropertyInnerField, (const FProperty* InProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(void, SetPropertyValueInContainer, (const FProperty* InProperty, void* InOutContainer, const void* InValue));
DECLARE_UNREAL_SHARP_INTEROP_API(void, GetPropertyValueInContainer, (const FProperty* InProperty, const void* InContainer, void* OutValue));
DECLARE_UNREAL_SHARP_INTEROP_API(void, SetBoolPropertyValue, (const FBoolProperty* InBoolProperty, void* InTargetAddress, bool bInValue));
DECLARE_UNREAL_SHARP_INTEROP_API(bool, GetBoolPropertyValue, (const FBoolProperty* InBoolProperty, void* InTargetAddress));

// Set Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(const FProperty*, GetElementPropertyOfSet, (const FSetProperty* InSetProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(int, GetLengthOfSet, (const void* InAddressOfSet, const FSetProperty* InSetProperty));
DECLARE_UNREAL_SHARP_INTEROP_API(const void*, GetElementAddressOfSet, (const void* InAddressOfSet, const FSetProperty* InSetProperty, int InIndex));
DECLARE_UNREAL_SHARP_INTEROP_API(bool, IsSetContainsElement, (const void* InAddressOfSet, const FSetProperty* InSetProperty, const void* InAddressOfElementTarget));
DECLARE_UNREAL_SHARP_INTEROP_API(bool, AddSetElement, (void* InAddressOfSet, const FSetProperty* InSetProperty, const void* InAddressOfElementTarget));
DECLARE_UNREAL_SHARP_INTEROP_API(bool, RemoveSetElement, (void* InAddressOfSet, const FSetProperty* InSetProperty, const void* InAddressOfElementTarget));
DECLARE_UNREAL_SHARP_INTEROP_API(void, ClearSet, (void* InAddressOfSet, const FSetProperty* InSetProperty));

// Soft Object Ptr Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(void, ResetSoftObjectPtr, (FSoftObjectPtr* InSoftObjectPtr));
DECLARE_UNREAL_SHARP_INTEROP_API(void, ResetSoftObjectPtrWeakPtr, (FSoftObjectPtr* InSoftObjectPtr));
DECLARE_UNREAL_SHARP_INTEROP_API(bool, IsSoftObjectPtrPending, (const FSoftObjectPtr* InSoftObjectPtr));
DECLARE_UNREAL_SHARP_INTEROP_API(bool, IsSoftObjectPtrValid, (const FSoftObjectPtr* InSoftObjectPtr));
DECLARE_UNREAL_SHARP_INTEROP_API(bool, IsSoftObjectPtrStale, (const FSoftObjectPtr* InSoftObjectPtr));
DECLARE_UNREAL_SHARP_INTEROP_API(bool, IsSoftObjectPtrNull, (const FSoftObjectPtr* InSoftObjectPtr));
DECLARE_UNREAL_SHARP_INTEROP_API(UObject*, GetUnrealObjectPointerOfSoftObjectPtr, (FSoftObjectPtr* InSoftObjectPtr));
DECLARE_UNREAL_SHARP_INTEROP_API(UObject*, GetUnrealObjectPointerOfSoftObjectPtrEx, (FSoftObjectPtr* InSoftObjectPtr, bool evenIfPendingKill));
DECLARE_UNREAL_SHARP_INTEROP_API(FSoftObjectPath*, GetObjectIdPointerOfSoftObjectPtr, (FSoftObjectPtr* InSoftObjectPtr));
DECLARE_UNREAL_SHARP_INTEROP_API(UObject*, LoadSynchronousSoftObjectPtr, (FSoftObjectPtr* InSoftObjectPtr));
DECLARE_UNREAL_SHARP_INTEROP_API(void, CopySoftObjectPtr, (FSoftObjectPtr* InDestination, const FSoftObjectPtr* InSource));

// String Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(const TCHAR*, GetCSharpMarshalString, (const FString* InStringPtr));
DECLARE_UNREAL_SHARP_INTEROP_API(void, SetUnrealString, (FString* InTargetStringPtr, const char* InCSharpString));
DECLARE_UNREAL_SHARP_INTEROP_API(int, GetUnrealStringLength, (const FString* InTargetStringPtr));
DECLARE_UNREAL_SHARP_INTEROP_API(void, CopyUnrealString, (FString* InTargetStringPtr, const FString* InSourceStringPtr));

// Text Interop Utils
DECLARE_UNREAL_SHARP_INTEROP_API(const TCHAR*, GetTextCSharpMarshalStringFromUnrealText, (const FText* InTextPtr));
DECLARE_UNREAL_SHARP_INTEROP_API(const TCHAR*, GetTextCSharpMarshalStringFromCSharpString, (const char* InCSharpMarshalString));
DECLARE_UNREAL_SHARP_INTEROP_API(void, SetUnrealTextFromCSharpString, (FText* InTextPtr, const char* InCSharpMarshalString));
