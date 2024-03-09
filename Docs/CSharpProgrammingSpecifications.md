# C# Programming Specifications
## C# Name Conventions
All Unreal Types must follow the naming rules of Unreal C++, that is: classes start with A or U, structures start with F, and enumerations start with E. Compilation will fail if these requirements are not met:
```C#
    [UCLASS()]
    public class UTestGameCharacter : ACharacter
    {
        [UPROPERTY(EPropertyFlags.Net, ReplicatedUsing = nameof(OnRep_DoubleJump), ReplicationCondition = LifetimeCondition.InitialOrOwner)]
        public bool bDoubleJump;

        [UFUNCTION]
        public void OnRep_DoubleJump() { }

        [UEVENT(Replicates = FunctionReplicateType.Server, IsReliable = true, Category = "Server")]
        public void ServerCastSkill(int skillId) { }

        [UEVENT(Replicates = FunctionReplicateType.Client, IsReliable = true, Category = "Client")]
        public void ClientReceiveMessage(string message) { }
	}
```  
```log
GameScripts\Game\UnrealSharp.GameScripts\Bindings.Defs\TestGame\ATestGameCharacter.def.cs(6): error InvalidTypeName: Invalid Actor Type Name : UTestGameCharacter : The name of the Unreal Actor class type defined in C# must start with 'A'
```

## Enum
* Only byte enumeration is supported. That is, when defining an enumeration, the type must be limited to byte. like:
```C#
    [UENUM]
    public enum EUnrealSharpCSharpEnumTypeInCSharp : byte
    {
        None = 0,
        Property,
        Field,
        Method
    }
``` 
If you remove the byte, you will get an error in the preprocessing stage when compiling:
```log
GameScripts\Game\UnrealSharp.GameScripts\Bindings.Defs\UnrealSharpTests\FUnrealSharpTestsStructValueInCSharp.def.cs(7): error InvalidEnumSize: Invalid enum Size : EUnrealSharpCSharpEnumTypeInCSharp : Currently, the size of enumerations exported to Unreal can only be constrained to byte. Consider defining it as: public enum EUnrealSharpCSharpEnumTypeInCSharp : byte
```

* Forcibly specifying the value of an enumeration field is not supported. Its size can only be automatically assigned in order. For example, if you change it to this, an error will be reported:  
```C#
    [UENUM]
    public enum EUnrealSharpCSharpEnumTypeInCSharp : byte
    {
        None = 0,
        Property = 2, // error, can't specify the value of UENUM's field
        Field,
        Method
    }
```
```log
GameScripts\Game\UnrealSharp.GameScripts\Bindings.Defs\UnrealSharpTests\FUnrealSharpTestsStructValueInCSharp.def.cs(7): error UEnumSpecifyFieldValueIsNotSupported: Invalid enum field UnrealSharp.GameScripts.Bindings.Defs.UnrealSharpTests.EUnrealSharpCSharpEnumTypeInCSharp UnrealSharp.GameScripts.Bindings.Defs.UnrealSharpTests.EUnrealSharpCSharpEnumTypeInCSharp::Property, UEnum does not support the value of the specified field. Please make sure to assign the value automatically in order.
```

## Class
* **UClass is not allowed to be declared static, abstract or sealed under any circumstances.This is also consistent with C++.**  
To add Abstract tags, you can use ClassFlags, such as:
```C#
    [UCLASS(EClassFlags.Abstract)]
    public class UUnrealSharpTestsBaseObjectInCSharp : UObject
``` 

## Properties
* All objects use Nullable type. for example: 
```C#
    [UPROPERTY(Category = "Class")]
    public TSoftClassPtr<AActor>? SoftClassPtrDefaultInObject;

    [UPROPERTY(Category = "Class")]
    [UMETA(MetaConstants.AllowAbstract, true)]
    public TSoftClassPtr<AActor>? SoftClassPtrAllowAbstractInObject;

    [UPROPERTY(Category = "Object")]
    public UObject? RawObjectPtrInObject = default;

    [UPROPERTY(Category = "Object")]
    public UObject? ObjectPtrInObject = default;

    [UPROPERTY(Category = "Object")]
    public TSoftObjectPtr<UObject>? SoftObjectPtrInObject;

    [UPROPERTY(Category = "Array")]
    public IList<int>? IntArrayInObject;

    [UPROPERTY(Category = "Array")]
    public IList<string?>? StringArrayInObject;

    [UPROPERTY(Category = "Array")]
    public IList<FName>? NameArrayInObject;

    [UPROPERTY(Category = "Array")]
    public IList<FVector>? VectorArrayInObject;

    [UPROPERTY(Category = "Array")]
    public IList<UObject?>? ObjectArrayInObject;

    [UPROPERTY(Category = "Set")]
    public ISet<float>? FloatSetInObject;

    [UPROPERTY(Category = "Set")]
    public ISet<string?>? StringSetInObject;

    [UPROPERTY(Category = "Set")]
    public ISet<UObject?>? ObjectSetInObject;

    [UPROPERTY(Category = "Map")]
    public IDictionary<int, string?>? IntStringMapInObject;

    [UPROPERTY(Category = "Map")]
    public IDictionary<FName, FVector>? NameVectorMapInObject;

    [UPROPERTY(Category = "Map")]
    public IDictionary<float, UObject?>? FloatObjectMapInObject;
```
* Nullable is prohibited for all value types and structure types

```C#
    [UPROPERTY(Category = "Scalar")]
    public bool bBoolValueInObject = true;

    [UPROPERTY(Category = "Scalar")]
    public byte u8ValueInObject = 128;

    [UPROPERTY(Category = "Scalar")]
    public int i32ValueInObject = 65535;

    [UPROPERTY(Category = "Scalar")]
    public float fValueInObject = 3.1415926f;

    [UPROPERTY(Category = "Scalar")]
    public double dValueInObject = 0.618;

    [UPROPERTY(Category = "Scalar")]
    [DefaultValueText("UnrealSharpTestsProject")]
    public EUnrealSharpProjectsEnumTypeInCpp ProjectValueInObject;

    [UPROPERTY(Category = "Scalar")]
    [DefaultValueText("VisualBasic|JavaScript")]
    public EUnrealSharpLanguageTypesInCpp LanguageFlagsInObject;

    [UPROPERTY(Category = "Scalar")]
    [DefaultValueText("Property")]
    public EUnrealSharpCSharpEnumTypeInCSharp CSharpEnumValueInObject;

    [UPROPERTY(Category = "Text")]
    public string StrValueInObject = "Hello UnrealSharp!!!";

    [UPROPERTY(Category = "Text")]
    [DefaultValueText("Hello UnrealSharp!!!")]
    public FName NameValueInObject;

    [UPROPERTY(Category = "Text")]
    [DefaultValueText("Unreal")]
    public FText TextValueInObject;

    [UPROPERTY(Category = "Internal Structures")]
    [DefaultValueText("(X=1.000000,Y=2.000000,Z=3.000000)")]
    public FVector VecValueInObject;

    [UPROPERTY(Category = "Internal Structures")]
    [DefaultValueText("(X=1.000000,Y=2.000000,Z=3.000000)")]
    public FVector3f Vec3fValueInObject;

    [UPROPERTY(Category = "Internal Structures")]
    [DefaultValueText("(Pitch=10.000000,Yaw=20.000000,Roll=30.000000)")]
    public FRotator RotValueInObject;

    [UPROPERTY(Category = "Internal Structures")]
    public FGuid GuidValueInObject;

    [UPROPERTY(Category = "User Structures")]
    public FUnrealSharpTestsBaseStructValueInCpp CppTestBaseStructInObject;

    [UPROPERTY(Category = "User Structures")]
    public FUnrealSharpTestsBaseStructValueInCSharp CSharpTestBaseStructInObject;

    [UPROPERTY(Category = "Class")]
    public TSubclassOf<UObject> ClassRawPtrDefaultInObject;

    [UPROPERTY(Category = "Class")]
    [UMETA(MetaConstants.AllowAbstract, true)]
    public TSubclassOf<UObject> ClassRawPtrInObject;

    [UPROPERTY(Category = "Class")]
    public TSubclassOf<AActor> SubclassOfActorDefaultInObject;

    [UPROPERTY(Category = "Class")]
    [UMETA(MetaConstants.AllowAbstract, true)]
    public TSubclassOf<AActor> SubclassOfActorAllowAbstractInObject;
```

* Ordinary types can use = to assign default values ​​directly; complex types set default values ​​through additional DefaultValueTextAttribute, and their parameter string format is Unreal default value string format.  

* Properties or function parameters of type `UClass?` are disabled and can be replaced by `TSubClassof<UObject>`.  
```C#
    [UCLASS(EClassFlags.Abstract)]
    public class UUnrealSharpTestsBaseObjectInCSharp : UObject
    {
        [UPROPERTY(Category = "Scalar")]
        public UClass? ClassType0;
    }
```

```log
GameScripts\Game\UnrealSharp.GameScripts\Bindings.Defs\UnrealSharpTests\UUnrealSharpTestsBaseObjectInCSharp.cs(10): error RawUClassPointerIsNotSupported: Invalid property: UnrealSharp.UnrealEngine.Bindings.Placeholders.UClass UnrealSharp.GameScripts.Bindings.Defs.UnrealSharpTests.UUnrealSharpTestsBaseObjectInCSharp::ClassType0. For performance reasons, UClass? As exported properties have been disabled, please consider using TSubclassOf<UObject> instead it.
```

* `TDelegate<Signature>` in C# is not supported. you can use `TMulticastDelegate<Signature>` instead.


## Functions
* C#'s UFunctions do not support overloading, that is, UFunction does not allow the same name:
```C#
    public partial class ATestGameCharacter
    {
        [UFUNCTION]
        public void OnRep_DoubleJump()
        {
        }

        public void ServerCastSkill_Implementation(int skillId)
        {

        }

        public void ClientReceiveMessage_Implementation(string? message)
        {

        }

        [UFUNCTION]
        public void TestThisIsAUFunction()
        {

        }

        [UFUNCTION]
        public void TestThisIsAUFunction(int a)
        {

        }
    }
```
```log
GameScripts\Game\UnrealSharp.GameScripts\TestGame\ATestGameCharacter.cs(23): error UFunctionOverloadIsNotSupported: UFunction overload found:
1>    System.Void UnrealSharp.GameScripts.TestGame.ATestGameCharacter::TestThisIsAUFunction()
1>    System.Void UnrealSharp.GameScripts.TestGame.ATestGameCharacter::TestThisIsAUFunction(System.Int32)
```

* Like C++, when implementing event UFunction, the specific implementation function is not the UFunction itself, but its implementation is original name +Implementation. When generating code, the tool will automatically generate the corresponding interface method interface for you, so you will encounter compilation errors if you lack the corresponding implementation.  
```C#
    [UCLASS(Config = "Game")]
    public class ASharpGameCharacter : ACharacter
    {       
        public delegate void OnJumpDelegate(int jumpCount);
        
        [UPROPERTY(EPropertyFlags.BlueprintAssignable)]
        public TMulticastDelegate<OnJumpDelegate>? OnJump;

        [UEVENT()]
        public override void ReceiveBeginPlay()
        {
        }

        [UPROPERTY(EPropertyFlags.Net | EPropertyFlags.RepNotify, ReplicatedUsing = nameof(OnRep_JumpCount))]
        public int JumpCount;

        [UEVENT(EFunctionFlags.Net | EFunctionFlags.NetServer)]
        public void ServerIncJumpCount() { }

        [UFUNCTION]
        public void OnRep_JumpCount() { }
    }
```

```C#
	public partial class ASharpGameCharacter
	{
		[UFUNCTION()]
		private void OnPlayerJump(int jumpCount)
		{
			Logger.LogD("Player Jump, Count={0}", jumpCount);
		}

		public virtual void ServerIncJumpCount_Implementation()
		{
			// should call on ROLE_Authority
			var localRole = GetLocalRole();

			if (localRole != ENetRole.ROLE_Authority)
			{
				Logger.LogError("Invalid usage of server method.");
			}
			else
			{
				++JumpCount;
				ForceNetUpdate();
			}
		}

		[UFUNCTION()]
		public void OnRep_JumpCount()
		{
			Logger.Log("[{1}]OnRep_JumpCount, JumpCount = {0}", JumpCount, GetLocalRole());
		}
	}
```
