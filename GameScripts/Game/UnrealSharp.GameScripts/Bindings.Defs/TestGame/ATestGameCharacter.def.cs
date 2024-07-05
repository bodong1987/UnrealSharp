using UnrealSharp.Utils.UnrealEngine;
using UnrealSharp.UnrealEngine.Bindings.Placeholders;
// ReSharper disable InconsistentNaming

namespace UnrealSharp.GameScripts.Bindings.Defs.TestGame;

[USTRUCT(ExportFlags = EBindingExportFlags.WithStructView)]
public struct FTestGameValue
{
    [UPROPERTY]
    public int XValue;

    [UPROPERTY]
    public string? SValue;

    [UPROPERTY]
    public FVector VecValue;
}

[UCLASS]
public class ATestGameCharacter : ACharacter
{
    [UPROPERTY(EPropertyFlags.Net, ReplicatedUsing = nameof(OnRep_DoubleJump), ReplicationCondition = LifetimeCondition.InitialOrOwner)]
    public bool bDoubleJump;

    [UFUNCTION]
    public void OnRep_DoubleJump() { }

    [UEVENT(Replicates = FunctionReplicateType.Server, IsReliable = true, Category = "Server")]
    public void ServerCastSkill(int skillId) { }

    [UEVENT(Replicates = FunctionReplicateType.Client, IsReliable = true, Category = "Client")]
    public void ClientReceiveMessage(string message) { }

    [UPROPERTY]
    public AActor? ActorReference;

    [UPROPERTY]
    public TSubclassOf<AActor> ActorClass;

    [UPROPERTY]
    public TSoftObjectPtr<AActor>? SoftActor;

    [UPROPERTY]
    public TSoftClassPtr<AActor>? SoftActorClass;

    [UPROPERTY]
    public int IntValueTest;

    // Compile Error Tests...
#if false
        [UPROPERTY]
        public DateTime DateValueTest;

        [UFUNCTION]
        public void TestMethod(DateTime dt) { }

        [UFUNCTION]
        public void TestGenericFunction<T>() { }

#endif
}

#if false // error tests
    [UCLASS]
    public class ATestCharacter<T> : ACharacter
    {

    }
#endif