# The Principle of UnrealSharp
UnrealSharp's main jobs are three:  
* Provides interactive methods to help you call C# functions from Unreal C++, which is achieved by calling the API of .NET Runtime.  
* Provides a binding framework to help you conveniently access the C++ and Blueprint interfaces provided by Unreal from C#, allowing you to access data, functions, etc. that can only be obtained in C++ and Blueprint from C#.This part of the work is automatically completed by the code generator, the UnrealSharp.Toolkit package.  
* The most important function of UnrealSharp is to enable the UCLASS, USTRUCT, UENUM, UFUNCTION, and UPROPERTY you implement in C# to be recognized and used by Unreal. Only by implementing this function can C# inherit C++ classes and blueprints inherit C# classes.  

## Type Definition Database
This is a database file describing the Unreal type. It is a text file in json format. The default format is .tdb.This database  include definitions of classes, structures, and enumerations, as well as attributes of classes and structures, enumeration values ​​of enumerations, functions of classes, and functions Parameters and other data.  

**The code for this part is all located in the SharpBindingGen module.**  

The database file you generate using the UnrealSharp extension menu in Unreal Editor is such a file. By default, they are generated to the Intermediate/UnrealSharp directory in the project root directory.  
* Intermediate/UnrealSharp/NativeTypeDefinition.tdb   
    All supported Unreal C++ Types
* Intermediate/UnrealSharp/BlueprintTypeDefinition.tdb  
    All supported Unreal Blueprint Types  

When you compile a C# project, this database file will automatically be dynamically generated for your C# dll.These files are placed in the Managed directory together with the C# dll:  
- Managed  
    - UnrealSharp.GameScripts.dll
    - UnrealSharp.GameScripts.tdb
    - UnrealSharp.GameContent.dll
    - UnrealSharp.GameContent.tdb  

UnrealSharp will analyze whether this data needs to be re-imported every time the level editor is activated. It is similar to the behavior of the Unity engine trying to recompile C# code when you activate UnityEditor.  

# Import 
**Essentially, what UnrealSharp does for you is to allow you to write blueprints in C#.**  
* C# UCLASS  => UCSharpBlueprint/UCSharpClass  
* C# USTRUCT => UCSharpStruct  
* C# UENUM   => UCSharpEnum  

Any UCLASS, USTRUCT, UENUM, UFUNCTION, and UPROPERTY you implement in C# will eventually be converted into a blueprint entity, and then UnrealSharp will help you associate them. For example, when you call a UFUNCTION from C#, it looks like you are actually calling a blueprint function in the blueprint, but underneath UnrealSharp will forward it to .NET Runtime for execution. Similarly, when you create a UObject or its subclass and try to access it in C#, UnrealSharp will automatically create a C# proxy class for you and associate the life cycle of the two. The Unreal object is the main one and the C# object is its attachments. This C# proxy object will provide a C# interface to help you access the properties and functions of this UObject, etc.  
Therefore, you can find the uasset resources corresponding to all C# types that UnrealSharp automatically imports for you in the Content/CSharpBlueprints directory.  
  
**Warning: These resources are automatically generated. You can open and view them, but please do not modify them directly, because if you change and save, the relevant data will be lost during the next automatic import.**  

So in essence, C# inheriting Unreal C++ is actually Blueprint inheriting C++, and Blueprint inheriting C# is actually Blueprint inheriting Blueprint.  

This is the design idea of ​​Unrealsharp and the working principle behind it.  

# Invoke C# UFunction
Although each UCLASS generated from C# is based on a blueprint, its type will be UCSharpClass instead of the blueprint's default UBlueprintGeneratedClass. In this UCSharpClass we record the basic information of the C# type, including paths, functions, etc. Every time we start the game, we will find all UCSharpClass in the system and actively redirect the functions from C#, that is, replace its NativeFunc.  
```C++
void UCSharpClass::RedirectAllCSharpFunctions()
{
	for (auto& func : CSharpFunctions)
	{
		UFunction* CSharpFunction = FindFunctionByName(func.Key, EIncludeSuperFlag::ExcludeSuper);
		checkf(CSharpFunction, TEXT("Failed find %s[%s] on CSharp Class %s"), *func.Key.ToString(), *func.Value.FunctionSignature, *GetCSharpFullName());

		if (CSharpFunction->HasAnyFunctionFlags(FUNC_Native))
		{
			// already redirected ???
			continue;
		}

		FCSharpFunctionRedirectionData RediretionData(CSharpFunction, &func.Value);

		CSharpFunction->FunctionFlags |= FUNC_Native;
		CSharpFunction->SetNativeFunc(&UCSharpClass::CallCSharpFunction);

		RedirectionCaches.Add(CSharpFunction, RediretionData);
	}
}
```
In this way, when we trigger a call to this C# UFunction in the code, it will be executed to UCSharp::CallCSharpFunction instead of the original default execution entry. In CallCSharpFunction we will send it to .NET Runtime for execution through a series of processes.  
```C++
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

		TSharedPtr<UnrealSharp::FUnrealFunctionInvokeRedirector> Invoker = 
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
```
When the game ends, the redirection information we temporarily modified will be changed back. This is to avoid unnecessary trouble, because after all, the game is not running and .NET RUNTIME has exited.  

## Association between Unreal Object and C# Object
When calling `UFunction` on the C# side, we cannot directly hand over native C++ data such as `UObject*` to C#. This makes no sense. For C#, this is just a 64-bit integer. So we need to actively convert this native data into a `C# proxy object[UObject? in C#]`, which is achieved through `ICSharpObjectTable`.  
In this table, the mapping from Unreal Object to C# object will be saved. When a new Unreal Object applies for its C# proxy, a `GCHandle` is created and its proxy object is placed in this `GCHandle` to ensure that it will not be recycled by the C# garbage collector.  
`ICSharpObjectTable` will register the message that the garbage collection analysis is completed. After the Unreal Object is marked as about to be recycled, the system will forcefully end the state of the C# Object being held by `GCHandle` and clean up the Unreal Object pointer stored inside the `C# UObject`. this ensure that C# will not access an expired native pointer.  

## Invoke Unreal C++ in C#
UnrealSharp will help you export a UFunction that can be called by C# and help you generate the code to call it in C#. UnrealSharp has two methods to call UFunction. 
* The first is based on the Unreal virtual machine and called through UObject::ProcessEvent. This method involves a large number of packing and unpacking operations, and the performance is relatively low.  
This method will always be used when you access non-C++ code or call Event Function.  
```C#
/// <summary>
/// Set the actor's RootComponent to the specified relative rotation
/// @param NewRelativeRotation   New relative rotation of the actor's root component
/// @param bSweep                                Whether we sweep to the destination location, triggering overlaps along the way and stopping short of the target if blocked by something.
/// Only the root component is swept and checked for blocking collision, child components move without sweeping. If collision is off, this has no effect.
/// @param bTeleport                             Whether we teleport the physics state (if physics collision is enabled for this object).
/// If true, physics velocity for this object is unchanged (so ragdoll parts are not affected by change in location).
/// If false, physics velocity is updated based on the change in position (affecting ragdoll parts).
/// If CCD is on and not teleporting, this will affect objects along the entire swept volume.
/// </summary>
/// <meta name="AdvancedDisplay">bSweep,SweepHitResult,bTeleport</meta>
/// <meta name="Category">Transformation</meta>
/// <meta name="DisplayName">Set Actor Relative Rotation</meta>
/// <meta name="ScriptName">SetActorRelativeRotation</meta>
[UFUNCTION(EFunctionFlags.Final|EFunctionFlags.RequiredAPI|EFunctionFlags.Native|EFunctionFlags.Public|EFunctionFlags.HasOutParms|EFunctionFlags.HasDefaults|EFunctionFlags.BlueprintCallable, Category = "Transformation")]
[UMETA("AdvancedDisplay", "bSweep,SweepHitResult,bTeleport")]
[UMETA("DisplayName", "Set Actor Relative Rotation")]
[UMETA("ScriptName", "SetActorRelativeRotation")]
public void K2_SetActorRelativeRotation(FRotator NewRelativeRotation, bool bSweep, ref FHitResult SweepHitResult, bool bTeleport)
{
    var __invocation = K2_SetActorRelativeRotationMetaData.K2_SetActorRelativeRotationInvocation!;
    unsafe
    {
        byte* __paramBufferPointer = stackalloc byte[__invocation.ParamSize];

        using var __scopedInvoker = new ScopedUnrealInvocation(__invocation, (IntPtr)__paramBufferPointer);
        FRotator.ToNative((IntPtr)__paramBufferPointer, K2_SetActorRelativeRotationMetaData.NewRelativeRotation_Offset, ref NewRelativeRotation);
        InteropUtils.SetBoolean((IntPtr)__paramBufferPointer, K2_SetActorRelativeRotationMetaData.bSweep_Offset, bSweep);
        FHitResult.ToNative((IntPtr)__paramBufferPointer, K2_SetActorRelativeRotationMetaData.SweepHitResult_Offset, ref SweepHitResult);
        InteropUtils.SetBoolean((IntPtr)__paramBufferPointer, K2_SetActorRelativeRotationMetaData.bTeleport_Offset, bTeleport);

        __invocation.Invoke(this, (IntPtr)__paramBufferPointer);

        // Copy out parameters back 
        SweepHitResult = FHitResult.FromNative((IntPtr)__paramBufferPointer, K2_SetActorRelativeRotationMetaData.SweepHitResult_Offset);
    }
}
```
* The second method is provided by UnrealSharp, which actively helps you generate C++ binding code and directly calls the generated C++ interface through the new [Function Pointer in C# 9.0](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/function-pointers).  
```C#
    /// <summary>
    /// Set whether this actor replicates to network clients. When this actor is spawned on the server it will be sent to clients as well.
    /// Properties flagged for replication will update on clients if they change on the server.
    /// Internally changes the RemoteRole property and handles the cases where the actor needs to be added to the network actor list.
    /// @param bInReplicates Whether this Actor replicates to network clients.
    /// @see https://docs.unrealengine.com/InteractiveExperiences/Networking/Actors
    /// </summary>
    /// <meta name="Category">Networking</meta>
    [UFUNCTION(EFunctionFlags.Final|EFunctionFlags.RequiredAPI|EFunctionFlags.BlueprintAuthorityOnly|EFunctionFlags.Native|EFunctionFlags.Public|EFunctionFlags.BlueprintCallable, Category = "Networking")]
    [FastAccessable]
    public void SetReplicates(bool bInReplicates)
    {
        unsafe
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, bool, void>)SetReplicatesInteropFunctionPointers.AActor_SetReplicates)(GetNativePtrChecked(), bInReplicates);
        }
    }
```

**It should be noted that not all UFunction supports fast invocation using the second method, because there are still many restrictions on parameter types when calling in this way.**  
The default test data is as follows:  
```log
Find Fast accessable 2530 methods, unsupported 1417 methods
```
Usually the reason for unsupported functions is because a certain parameter does not support fast access, that is, the memory size and layout of the C++ and C# sides are exactly the same.  






