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
