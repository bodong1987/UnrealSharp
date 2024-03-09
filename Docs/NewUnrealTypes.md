# New Unreal Types
## Enum
Just use `[UENUM]` to mark an enumeration.
```C#
// FUnrealSharpTestsStructValueInCSharp.def.cs
using UnrealSharp.UnrealEngine.Bindings.Placeholders;
using UnrealSharp.Utils.UnrealEngine;
using UnrealSharp.GameScripts.Bindings.Placeholders;

namespace UnrealSharp.GameScripts.Bindings.Defs.UnrealSharpTests
{
    [UENUM]
    public enum EUnrealSharpCSharpEnumTypeInCSharp : byte
    {
        None = 0,
        Property,
        Field,
        Method
    }
}
```
The parameters supported by UENUM are as follows:
```C#
    [Flags]
    public enum EEnumFlags : uint
    {
        None,
        Flags = 1 << 0,
        NewerVersionExists = 1 << 1,
    }
```

## Struct
Just use `[USTRUCT]` to mark an structure.

```C#
// FUnrealSharpTestsStructValueInCSharp.def.cs
using UnrealSharp.UnrealEngine.Bindings.Placeholders;
using UnrealSharp.Utils.UnrealEngine;
using UnrealSharp.GameScripts.Bindings.Placeholders;

namespace UnrealSharp.GameScripts.Bindings.Defs.UnrealSharpTests
{ 
    [USTRUCT]
    public struct FUnrealSharpTestsBaseStructValueInCSharp
    {
        [UPROPERTY(Category = "Scalar")]
        public bool bBoolValue = false;

		[UPROPERTY(Category = "Scalar")]
		public bool bBoolValueDefByProperty { get; set; } = true;

		[UPROPERTY(Category = "Scalar")]
        public byte u8Value = 255;

        [UPROPERTY(Category = "Scalar")]
        public int i32Value = 100;

		[UPROPERTY(Category = "Scalar")]
		public int i32ValueDefByProperty { get; set; } = 1024;

		[UPROPERTY(Category = "Scalar")]
        public float fValue = 3.1415926f;

        [UPROPERTY(Category = "Scalar")]
        public double dValue = 0.618;

        [UPROPERTY(Category = "Scalar")]
        [DefaultValueText("UnrealSharpTestsProject")]
        public EUnrealSharpProjectsEnumTypeInCpp ProjectValue;

        [UPROPERTY(Category = "Scalar")]
        [DefaultValueText("FSharp")]
        public EUnrealSharpLanguageTypesInCpp LanguageFlags;

        [UPROPERTY(Category = "Text")]
        public string? StrValue = "你好，UnrealSharp!!!";

        [UPROPERTY(Category = "Text")]
        [DefaultValueText("你好，UnrealSharp!!!")]
        public FName NameValue;

        [UPROPERTY(Category = "Text")]
        public FText TextValue;

        public FUnrealSharpTestsBaseStructValueInCSharp()
        {
        }
    }

    [USTRUCT]
    public struct FUnrealSharpTestsStructValueInCSharp
    {
        [UPROPERTY(Category= "Scalar")]
        public bool bBoolValue = true;

        [UPROPERTY(Category= "Scalar")]
        public byte u8Value = 128;

        [UPROPERTY(Category= "Scalar")]
        public int i32Value = 65535;

        [UPROPERTY(Category= "Scalar")]
        public float fValue = 3.1415926f;

        [UPROPERTY(Category= "Scalar")]
        public double dValue = 0.618;

        [UPROPERTY(Category= "Scalar")]
        [DefaultValueText("UnrealSharpTestsProject")]
        public EUnrealSharpProjectsEnumTypeInCpp ProjectValue;

        [UPROPERTY(Category= "Scalar")]
        [DefaultValueText("VisualBasic|JavaScript")]
        public EUnrealSharpLanguageTypesInCpp LanguageFlags;

        [UPROPERTY(Category = "Scalar")]
        [DefaultValueText("Property")]
        public EUnrealSharpCSharpEnumTypeInCSharp CSharpEnumValue;

        [UPROPERTY(Category= "Text")]
        public string StrValue = "Hello UnrealSharp!!!";

        [UPROPERTY(Category= "Text")]
        [DefaultValueText("Hello UnrealSharp!!!")]
        public FName NameValue;

        [UPROPERTY(Category= "Text")]
        [DefaultValueText("Unreal")]
        public FText TextValue;

        [UPROPERTY(Category= "Internal Structures")]
        [DefaultValueText("(X=1.000000,Y=2.000000,Z=3.000000)")]
        public FVector VecValue;

        [UPROPERTY(Category= "Internal Structures")]
        [DefaultValueText("(X=1.000000,Y=2.000000,Z=3.000000)")]
        public FVector3f Vec3fValue;

        [UPROPERTY(Category= "Internal Structures")]
        [DefaultValueText("(Pitch=10.000000,Yaw=20.000000,Roll=30.000000)")]
        public FRotator RotValue;

        [UPROPERTY(Category= "Internal Structures")]
        public FGuid GuidValue;

        [UPROPERTY(Category = "User Structures")]
        public FUnrealSharpTestsBaseStructValueInCpp TestBaseStruct;

        [UPROPERTY(Category = "User Structures")]
        public FUnrealSharpTestsBaseStructValueInCSharp CSharpTestBaseStruct;

        [UPROPERTY(Category= "Class")]
        public TSubclassOf<UObject> ClassRawPtrDefault;

        [UPROPERTY(Category = "Class")]
        [UMETA(MetaConstants.AllowAbstract, true)]
        public TSubclassOf<UObject> ClassRawPtr;

        [UPROPERTY(Category= "Class")]
        public TSubclassOf<AActor> SubclassOfActorDefault;

        [UPROPERTY(Category = "Class")]
        [UMETA(MetaConstants.AllowAbstract, true)]
        public TSubclassOf<AActor> SubclassOfActorAllowAbstract;

        [UPROPERTY(Category= "Class")]
        public TSoftClassPtr<AActor>? SoftClassPtrDefault;

        [UPROPERTY(Category = "Class")]
        [UMETA(MetaConstants.AllowAbstract, true)]
        public TSoftClassPtr<AActor>? SoftClassPtrAllowAbstract;

        [UPROPERTY(Category = "Object")]
        public UObject? RawObjectPtr = default;

        [UPROPERTY(Category = "Object")]
        public UObject? ObjectPtr = default;

        [UPROPERTY(Category = "Object")]
        public TSoftObjectPtr<UObject>? SoftObjectPtr;

        [UPROPERTY(Category = "Array")]
        public IList<int>? IntArray;

        [UPROPERTY(Category = "Array")]
        public IList<string?>? StringArray;

        [UPROPERTY(Category = "Array")]
        public IList<FName>? NameArray;

        [UPROPERTY(Category = "Array")]
        public IList<FVector>? VectorArray;

        [UPROPERTY(Category = "Array")]
        public IList<UObject?>? ObjectArray;

        [UPROPERTY(Category = "Set")]
        public ISet<float>? FloatSet;

        [UPROPERTY(Category = "Set")]
        public ISet<string?>? StringSet;

        [UPROPERTY(Category = "Set")]
        public ISet<UObject?>? ObjectSet;

        [UPROPERTY(Category = "Map")]
        public IDictionary<int, string?>? IntStringMap;

        [UPROPERTY(Category = "Map")]
        public IDictionary<FName, FVector>? NameVectorMap;

        [UPROPERTY(Category = "Map")]
        public IDictionary<float, UObject?>? FloatObjectMap;

        public FUnrealSharpTestsStructValueInCSharp()
        {
        }
    }
}
```  

Through the parameters of USTRUCT, you can also add the following additional tags:
```C#
[Flags]
public enum EStructFlags
{
    NoFlags = 0x00000000,
    Native = 0x00000001,

    /// <summary>
    /// If set, this struct will be compared using native code
    /// </summary>
    IdenticalNative = 0x00000002,

    HasInstancedReference = 0x00000004,

    NoExport = 0x00000008,

    /// <summary>
    /// Indicates that this struct should always be serialized as a single unit
    /// </summary>
    Atomic = 0x00000010,

    /// <summary>
    /// Indicates that this struct uses binary serialization; it is unsafe to add/remove members from this struct without incrementing the package version
    /// </summary>
    Immutable = 0x00000020,

    /// <summary>
    /// If set, native code needs to be run to find referenced objects
    /// </summary>
    AddStructReferencedObjects = 0x00000040,

    /// <summary>
    /// Indicates that this struct should be exportable/importable at the DLL layer.  Base structs must also be exportable for this to work.
    /// </summary>
    RequiredAPI = 0x00000200,

    /// <summary>
    /// If set, this struct will be serialized using the CPP net serializer
    /// </summary>
    NetSerializeNative = 0x00000400,

    /// <summary>
    /// If set, this struct will be serialized using the CPP serializer
    /// </summary>
    SerializeNative = 0x00000800,

    /// <summary>
    /// If set, this struct will be copied using the CPP operator=
    /// </summary>
    CopyNative = 0x00001000,

    /// <summary>
    /// If set, this struct will be copied using memcpy
    /// </summary>
    IsPlainOldData = 0x00002000,

    /// <summary>
    /// If set, this struct has no destructor and non will be called. IsPlainOldData implies NoDestructor
    /// </summary>
    NoDestructor = 0x00004000,

    /// <summary>
    /// If set, this struct will not be constructed because it is assumed that memory is zero before construction.
    /// </summary>
    ZeroConstructor = 0x00008000,

    /// <summary>
    /// If set, native code will be used to export text
    /// </summary>
    ExportTextItemNative = 0x00010000,

    /// <summary>
    /// If set, native code will be used to export text
    /// </summary>
    ImportTextItemNative = 0x00020000,

    /// <summary>
    /// If set, this struct will have PostSerialize called on it after CPP serializer or tagged property serialization is complete
    /// </summary>
    PostSerializeNative = 0x00040000,

    /// <summary>
    /// If set, this struct will have SerializeFromMismatchedTag called on it if a mismatched tag is encountered.
    /// </summary>
    SerializeFromMismatchedTag = 0x00080000,

    /// <summary>
    /// If set, this struct will be serialized using the CPP net delta serializer
    /// </summary>
    NetDeltaSerializeNative = 0x00100000,

    /// <summary>
    /// If set, this struct will be have PostScriptConstruct called on it after a temporary object is constructed in a running blueprint
    /// </summary>
    PostScriptConstruct = 0x00200000,

    /// <summary>
    /// If set, this struct can share net serialization state across connections
    /// </summary>
    NetSharedSerialization = 0x00400000,

    /// <summary>
    /// If set, this struct has been cleaned and sanitized (trashed) and should not be used
    /// </summary>
    Trashed = 0x00800000,

    /// <summary>
    /// If set, this structure has been replaced via reinstancing
    /// </summary>
    NewerVersionExists = 0x01000000,

    /// <summary>
    /// If set, this struct will have CanEditChange on it in the editor to determine if a child property can be edited
    /// </summary>
    CanEditChange = 0x02000000,

    /// <summary>
    /// Struct flags that are automatically inherited
    /// </summary>
    Inherit = HasInstancedReference | Atomic,

    /// <summary>
    /// Flags that are always computed, never loaded or done with code generation
    /// </summary>
    ComputedFlags = NetDeltaSerializeNative | NetSerializeNative | SerializeNative | PostSerializeNative | CopyNative | IsPlainOldData |
        NoDestructor | ZeroConstructor | IdenticalNative | AddStructReferencedObjects | ExportTextItemNative | ImportTextItemNative |
        SerializeFromMismatchedTag | PostScriptConstruct | NetSharedSerialization
}
```

## Class
Just use `[UCLASS]` to mark an Class, and must make this class inheirt from UObject or it's children class.
```C#
// UUnrealSharpCSharpTestsObjectInCSharp.def.cs
using UnrealSharp.UnrealEngine.Bindings.Placeholders;
using UnrealSharp.Utils.UnrealEngine;
using UnrealSharp.GameScripts.Bindings.Placeholders;

namespace UnrealSharp.GameScripts.Bindings.Defs.UnrealSharpTests
{
    [UCLASS(EClassFlags.Abstract)]
    public class UUnrealSharpTestsBaseObjectInCSharp : UObject
    {
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
    }
}
```  
Through UCLASS parameters, you can also add the following tags:  
```C#
[Flags]
public enum EClassFlags : uint
{
    /// <summary>
    /// No Flags
    /// </summary>
    None = 0x00000000u,

    /// <summary>
    /// Class is abstract and can't be instantiated directly.
    /// </summary>
    Abstract = 0x00000001u,

    /// <summary>
    /// Save object configuration only to Default INIs, never to local INIs. Must be combined with "Config"
    /// </summary>
    DefaultConfig = 0x00000002u,

    /// <summary>
    /// Load object configuration at construction time.
    /// </summary>
    Config = 0x00000004u,

    /// <summary>
    /// This object type can't be saved; null it out at save time.
    /// </summary>
    Transient = 0x00000008u,

    /// <summary>
    /// This object type may not be available in certain context. (i.e. game runtime or in certain configuration). Optional class data is saved separately to other object types. (i.e. might use sidecar files)
    /// </summary>
    Optional = 0x00000010u,

    /// <summary>
    /// 
    /// </summary>
    MatchedSerializers = 0x00000020u,

    /// <summary>
    /// Indicates that the config settings for this class will be saved to Project/User*.ini (similar to "GlobalUserConfig")
    /// </summary>
    ProjectUserConfig = 0x00000040u,

    /// <summary>
    /// Class is a native class - native interfaces will have "Native" set, but not RF_MarkAsNative
    /// </summary>
    Native = 0x00000080u,

    /// <summary>
    /// Don't export to C++ header.
    /// </summary>
    [Obsolete("No longer used in the engine.")]
    NoExport = 0x00000100u,

    /// <summary>
    /// Do not allow users to create in the editor.
    /// </summary>
    NotPlaceable = 0x00000200u,

    /// <summary>
    /// Handle object configuration on a per-object basis, rather than per-class.
    /// </summary>
    PerObjectConfig = 0x00000400u,

    /// <summary>
    /// Whether SetUpRuntimeReplicationData still needs to be called for this class
    /// </summary>
    ReplicationDataIsSetUp = 0x00000800u,

    /// <summary>
    /// Class can be constructed from editinline New button.
    /// </summary>
    EditInlineNew = 0x00001000u,

    /// <summary>
    /// Display properties in the editor without using categories.
    /// </summary>
    CollapseCategories = 0x00002000u,

    /// <summary>
    /// Class is an interface
    /// </summary>
    Interface = 0x00004000u,

    /// <summary>
    /// Do not export a constructor for this class, assuming it is in the cpptext
    /// </summary>
    [Obsolete("No longer used in the engine.")]
    CustomConstructor = 0x00008000u,

    /// <summary>
    /// all properties and functions in this class are const and should be exported as const
    /// </summary>
    Const = 0x00010000u,

    /// <summary>
    /// Class flag indicating objects of this class need deferred dependency loading
    /// </summary>
    NeedsDeferredDependencyLoading = 0x00020000u,

    /// <summary>
    /// Indicates that the class was created from blueprint source material
    /// </summary>
    CompiledFromBlueprint = 0x00040000u,

    /// <summary>
    /// Indicates that only the bare minimum bits of this class should be DLL exported/imported
    /// </summary>
    MinimalAPI = 0x00080000u,

    /// <summary>
    /// Indicates this class must be DLL exported/imported (along with all of it's members)
    /// </summary>
    RequiredAPI = 0x00100000u,

    /// <summary>
    /// Indicates that references to this class default to instanced. Used to be subclasses of UComponent, but now can be any UObject
    /// </summary>
    DefaultToInstanced = 0x00200000u,

    /// <summary>
    /// Indicates that the parent token stream has been merged with ours.
    /// </summary>
    TokenStreamAssembled = 0x00400000u,

    /// <summary>
    /// Class has component properties.
    /// </summary>
    HasInstancedReference = 0x00800000u,

    /// <summary>
    /// Don't show this class in the editor class browser or edit inline new menus.
    /// </summary>
    Hidden = 0x01000000u,

    /// <summary>
    /// Don't save objects of this class when serializing
    /// </summary>
    Deprecated = 0x02000000u,

    /// <summary>
    /// Class not shown in editor drop down for class selection
    /// </summary>
    HideDropDown = 0x04000000u,

    /// <summary>
    /// Class settings are saved to [AppData]/..../Blah.ini (as opposed to "DefaultConfig")
    /// </summary>
    GlobalUserConfig = 0x08000000u,

    /// <summary>
    /// Class was declared directly in C++ and has no boilerplate generated by UnrealHeaderTool
    /// </summary>
    Intrinsic = 0x10000000u,

    /// <summary>
    /// Class has already been constructed (maybe in a previous DLL version before hot-reload).
    /// </summary>
    Constructed = 0x20000000u,

    /// <summary>
    /// Indicates that object configuration will not check against ini base/defaults when serialized
    /// </summary>
    ConfigDoNotCheckDefaults = 0x40000000u,

    /// <summary>
    /// Class has been consigned to oblivion as part of a blueprint recompile, and a newer version currently exists.
    /// </summary>
    NewerVersionExists = 0x80000000u,

    /// <summary>
    /// Flags to inherit from base class
    /// </summary>
    Inherit = Transient | Optional | DefaultConfig | Config | PerObjectConfig | ConfigDoNotCheckDefaults | NotPlaceable | Const | HasInstancedReference |
        Deprecated | DefaultToInstanced | GlobalUserConfig | ProjectUserConfig | NeedsDeferredDependencyLoading,

    /// <summary>
    /// These flags will be cleared by the compiler when the class is parsed during script compilation
    /// </summary>
    RecompilerClear = Inherit | Abstract | Native | Intrinsic | TokenStreamAssembled,

    /// <summary>
    /// These flags will be cleared by the compiler when the class is parsed during script compilation
    /// </summary>
    ShouldNeverBeLoaded = Native | Optional | Intrinsic | TokenStreamAssembled,

    /// <summary>
    /// These flags will be inherited from the base class only for non-intrinsic classes
    /// </summary>
    ScriptInherit = Inherit | EditInlineNew | CollapseCategories,

    /// <summary>
    /// This is used as a mask for the flags put into generated code for "compiled in" classes.
    /// </summary>
    SaveInCompiledInClasses = Abstract | DefaultConfig | GlobalUserConfig | ProjectUserConfig | Config | Transient | Optional | Native | NotPlaceable | PerObjectConfig |
        ConfigDoNotCheckDefaults | EditInlineNew | CollapseCategories | Interface | DefaultToInstanced | HasInstancedReference | Hidden | Deprecated |
        HideDropDown | Intrinsic | Const | MinimalAPI | RequiredAPI | MatchedSerializers | NeedsDeferredDependencyLoading,
};
```


## Specific steps
Most of the time, generating a new Unreal Type is done in two steps:   
1. First add a new C# file in the Bindings.Defs directory. These files usually end with .def.cs for easy identification. This file is only allowed to reference the namespace related to **Placeholders** (Except UnrealSharp.Utils.UnrealEngine). Referencing the wrong namespace will trigger a compilation error at compile time.    



```C#
// UUnrealSharpCSharpTestsObjectInCSharp.def.cs
using UnrealSharp.UnrealEngine.Bindings.Placeholders;
using UnrealSharp.GameScripts.Bindings.Placeholders;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.GameScripts.Bindings.Defs.UnrealSharpTests
{
    [UCLASS]
    public class UUnrealSharpCSharpTestsObjectInCSharp : UUnrealSharpTestsBaseObjectInCSharp
    {
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

    }
}

```

```C#
// ASharpGameCharacter.def.cs
using UnrealSharp.Utils.UnrealEngine;
using UnrealSharp.UnrealEngine.Bindings.Placeholders;

namespace UnrealSharp.GameScripts.Bindings.Defs.SharpGame
{
    [UCLASS(Config = "Game")]
    public class ASharpGameCharacter : ACharacter
    {
        [UPROPERTY(EPropertyFlags.BlueprintReadOnly|EPropertyFlags.BlueprintVisible,Category = "Camera",AllowPrivateAccess = true, IsActorComponent = true)]        
        public USpringArmComponent? CameraBoom;

        [UPROPERTY(EPropertyFlags.BlueprintReadOnly | EPropertyFlags.BlueprintVisible, Category = "Camera", AllowPrivateAccess = true, IsActorComponent = true, AttachToComponentName = nameof(CameraBoom), AttachToSocketName= "SpringEndpoint")]
		public UCameraComponent? FollowCamera;

        [UPROPERTY(EPropertyFlags.BlueprintReadOnly, Category = "Input", AllowPrivateAccess = true)]
		public UInputMappingContext? DefaultMappingContext;

        [UPROPERTY(EPropertyFlags.BlueprintReadOnly, Category = "Input", AllowPrivateAccess = true)]
		public UInputAction? JumpAction;

        [UPROPERTY(EPropertyFlags.BlueprintReadOnly, Category = "Input", AllowPrivateAccess = true)]
		public UInputAction? MoveAction;

        [UPROPERTY(EPropertyFlags.BlueprintReadOnly, Category = "Input", AllowPrivateAccess = true)]
		public UInputAction? LookAction;

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
}
```

2. Compile this project.  
In the Prebuild phase, the tool will automatically generate binding code for you; in the PostBuild phase, the tool will automatically export the type database for Unreal to use.  
Generally speaking, the name of a class or structure will appear in three places:  
* Bindings
* Bindings.Defs
* YourCustomNamespace  
  
**Take ASharpGameCharacter as an example:**  
- UnrealSharp.GameScripts
    - Bindings
        - CSharpBinding  
            - GameScripts
                - SharpGame  
                    - Classes
                        - ASharpGameCharacter.gen.cs
    - Bindings.Defs
        - SharpGame
            - ASharpGameCharacter.def.cs

    - SharpGame
        - ASharpGameCharacter.cs

**annotation:**  
* *.gen.cs   
The tool automatically generates code for you. This code will complete the data exchange between C# and C++.  
* *.def.cs  
This is the class definition code you wrote yourself, which contains only the simplest declarations of types, properties, and methods.  
* *.cs or *.extends.cs  
Here are the parts that tools cannot help you generate, such as class methods added through the partial mechanism, or specific implementations of UFunction and UEVENT, etc.

3. For the generated database to take effect, you need to switch back to the Unreal editor and activate the level editor, just like Unity recompiles the C# script.  

## User-defined metadata
Through Attribute `UMETAAttribute` you can add custom metadata, which has the same meaning as the metadata you add in C++.  
```C++
        UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Class")
        TSubclassOf<AActor> SubclassOfActorDefault = nullptr;

        UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Class", meta=(AllowAbstract=true))
        TSubclassOf<AActor> SubclassOfActorAllowAbstract = nullptr;
```
```C#
        [UPROPERTY(Category= "Class")]
        public TSubclassOf<AActor> SubclassOfActorDefault;

        [UPROPERTY(Category = "Class")]
        [UMETA(MetaConstants.AllowAbstract, true)]
        public TSubclassOf<AActor> SubclassOfActorAllowAbstract;
```
**`UMETAAttribute` can add multiple** 




