using UnrealSharp.UnrealEngine.Bindings.Placeholders;
using UnrealSharp.Utils.UnrealEngine;
using UnrealSharp.GameScripts.Bindings.Placeholders;
// ReSharper disable InconsistentNaming
// ReSharper disable CommentTypo

namespace UnrealSharp.GameScripts.Bindings.Defs.UnrealSharpTests;

[UENUM]
public enum EUnrealSharpCSharpEnumTypeInCSharp : byte
{
    None = 0,
    Property /*= 2*/, // error, can't specify the value of UENUM's field
    Field,
    Method
}

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

    [UPROPERTY(Category = "Object")]
    public UObject? RawObjectPtr = default;

    [UPROPERTY(Category = "Object")]
    public UObject? ObjectPtr = default;

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


    // invalid property usage
#if false
        [UPROPERTY(Category = "Map")]
        public static IDictionary<float, UObject?>? FloatObjectMap2 { get; set; }

        [UPROPERTY(Category = "Map", IsActorComponent = true)]
        public IDictionary<float, UObject?>? FloatObjectMap2;

        
        [UPROPERTY(Category = "Comp", IsActorComponent = true)]
        public ULightComponent? LightComp;
#endif

#if false
        // unsupported property tests.
        [UPROPERTY]
        public TSoftObjectPtr<AActor>? SoftActorPtr;

        [UPROPERTY]
        public TSoftClassPtr<AActor>? SoftActorClassPtr;

        [UPROPERTY(Category = "Map")]
        public char ChValue;

        [UPROPERTY]
        public short shortValue;

        [UPROPERTY]
        public short uShortValue;
        
        [UPROPERTY]
        public UInt32 UInt32Value;

        [UPROPERTY]
        public UInt64 UInt64Value;
    
        public delegate void OnTestDelegateType(int value);
        [UPROPERTY]
        public TMulticastDelegate<OnTestDelegateType>? OnDelegate; // not support

         [UPROPERTY]
        public DateTime dtValue;
#endif

    public FUnrealSharpTestsStructValueInCSharp()
    {
    }
}