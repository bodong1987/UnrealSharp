using UnrealSharp.UnrealEngine.Bindings.Placeholders;
using UnrealSharp.GameScripts.Bindings.Placeholders;
using UnrealSharp.Utils.UnrealEngine;
// ReSharper disable InconsistentNaming

namespace UnrealSharp.GameScripts.Bindings.Defs.UnrealSharpTests;

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