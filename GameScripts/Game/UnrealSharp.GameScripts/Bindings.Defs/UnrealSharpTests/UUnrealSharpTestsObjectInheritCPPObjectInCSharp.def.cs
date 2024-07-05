using UnrealSharp.GameScripts.Bindings.Placeholders;
using UnrealSharp.UnrealEngine.Bindings.Placeholders;
using UnrealSharp.Utils.UnrealEngine;
// ReSharper disable InconsistentNaming

namespace UnrealSharp.GameScripts.Bindings.Defs.UnrealSharpTests;

[UCLASS]
public class UUnrealSharpTestsObjectInheritCPPObjectInCSharp : Placeholders.UUnrealSharpTestsObjectInCpp
{
    [UPROPERTY(Category = "CSharp_Scalar")]
    public bool bBoolValueInCSharp = true;

    [UPROPERTY(Category = "CSharp_Scalar")]
    public byte u8ValueInCSharp = 128;

    [UPROPERTY(Category = "CSharp_Scalar")]
    public int i32ValueInCSharp = 65535;

    [UPROPERTY(Category = "CSharp_Scalar")]
    public float fValueInCSharp = 3.1415926f;

    [UPROPERTY(Category = "CSharp_Scalar")]
    public double dValueInCSharp = 0.618;

    [UPROPERTY(Category = "CSharp_Scalar")]
    [DefaultValueText("UnrealSharpTestsProject")]
    public EUnrealSharpProjectsEnumTypeInCpp ProjectValueInCSharp;

    [UPROPERTY(Category = "CSharp_Scalar")]
    [DefaultValueText("FSharp")]
    public EUnrealSharpLanguageTypesInCpp LanguageFlagsInCSharp;

    [UPROPERTY(Category = "CSharp_Scalar")]
    [DefaultValueText("Property")]
    public EUnrealSharpCSharpEnumTypeInCSharp CSharpEnumValueInCSharp;

    [UPROPERTY(Category = "CSharp_Text")]
    public string StrValueInCSharp = "Hello UnrealSharp!!!";

    [UPROPERTY(Category = "CSharp_Text")]
    [DefaultValueText("Hello UnrealSharp!!!")]
    public FName NameValueInCSharp;

    [UPROPERTY(Category = "CSharp_Text")]
    [DefaultValueText("Unreal")]
    public FText TextValueInCSharp;

    [UPROPERTY(Category = "CSharp_Internal Structures")]
    [DefaultValueText("(X=1.000000,Y=2.000000,Z=3.000000)")]
    public FVector VecValueInCSharp;

    [UPROPERTY(Category = "CSharp_Internal Structures")]
    [DefaultValueText("(X=1.000000,Y=2.000000,Z=3.000000)")]
    public FVector3f Vec3fValueInCSharp;

    [UPROPERTY(Category = "CSharp_Internal Structures")]
    [DefaultValueText("(Pitch=10.000000,Yaw=20.000000,Roll=30.000000)")]
    public FRotator RotValueInCSharp;

    [UPROPERTY(Category = "CSharp_Internal Structures")]
    public FGuid GuidValueInCSharp;

    [UPROPERTY(Category = "CSharp_User Structures")]
    public FUnrealSharpTestsBaseStructValueInCpp CppTestBaseStructInCSharp;

    [UPROPERTY(Category = "CSharp_User Structures")]
    public FUnrealSharpTestsBaseStructValueInCSharp CSharpTestBaseStructInCSharp;

    [UPROPERTY(Category = "CSharp_Class")]
    public TSubclassOf<UObject> ClassRawPtrDefaultInCSharp;

    [UPROPERTY(Category = "CSharp_Class")]
    [UMETA(MetaConstants.AllowAbstract, true)]
    public TSubclassOf<UObject> ClassRawPtrInCSharp;

    [UPROPERTY(Category = "CSharp_Class")]
    public TSubclassOf<AActor> SubclassOfActorDefaultInCSharp;

    [UPROPERTY(Category = "CSharp_Class")]
    [UMETA(MetaConstants.AllowAbstract, true)]
    public TSubclassOf<AActor> SubclassOfActorAllowAbstractInCSharp;

    [UPROPERTY(Category = "CSharp_Class")]
    public TSoftClassPtr<AActor>? SoftClassPtrDefaultInCSharp;

    [UPROPERTY(Category = "CSharp_Class")]
    [UMETA(MetaConstants.AllowAbstract, true)]
    public TSoftClassPtr<AActor>? SoftClassPtrAllowAbstractInCSharp;

    [UPROPERTY(Category = "CSharp_Object")]
    public UObject? RawObjectPtrInCSharp = default;

    [UPROPERTY(Category = "CSharp_Object")]
    public UObject? ObjectPtrInCSharp = default;

    [UPROPERTY(Category = "CSharp_Object")]
    public TSoftObjectPtr<UObject>? SoftObjectPtrInCSharp;

    [UPROPERTY(Category = "CSharp_Array")]
    public IList<int>? IntArrayInCSharp;

    [UPROPERTY(Category = "CSharp_Array")]
    public IList<string?>? StringArrayInCSharp;

    [UPROPERTY(Category = "CSharp_Array")]
    public IList<FName>? NameArrayInCSharp;

    [UPROPERTY(Category = "CSharp_Array")]
    public IList<FVector>? VectorArrayInCSharp;

    [UPROPERTY(Category = "CSharp_Array")]
    public IList<UObject?>? ObjectArrayInCSharp;

    [UPROPERTY(Category = "CSharp_Set")]
    public ISet<float>? FloatSetInCSharp;

    [UPROPERTY(Category = "CSharp_Set")]
    public ISet<string?>? StringSetInCSharp;

    [UPROPERTY(Category = "CSharp_Set")]
    public ISet<UObject?>? ObjectSetInCSharp;

    [UPROPERTY(Category = "CSharp_Map")]
    public IDictionary<int, string?>? IntStringMapInCSharp;

    [UPROPERTY(Category = "CSharp_Map")]
    public IDictionary<FName, FVector>? NameVectorMapInCSharp;

    [UPROPERTY(Category = "CSharp_Map")]
    public IDictionary<float, UObject?>? FloatObjectMapInCSharp;

    public delegate void OnCSharpDelegateType(int intValue, string? strValue, UObject? objectValue);

    [UPROPERTY(Category = "CSharp_Delegate")]
    public TMulticastDelegate<OnCSharpDelegateType>? MulticastDelegateInCSharp;
}