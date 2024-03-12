using UnrealSharp.UnrealEngine;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.GameScripts.SharpGame
{
    public partial class ASharpGameCharacter
    {
        public void ReceiveBeginPlay_Implementation()
        {
            // invoke super
            base.ReceiveBeginPlay<ACharacter>();

            if (Controller is APlayerController PlayerController)
            {
                UEnhancedInputLocalPlayerSubsystem? Subsystem = PlayerController.GetLocalPlayer()?.GetSubsystem<UEnhancedInputLocalPlayerSubsystem>();

                if(Subsystem != null)
                {
                    Logger.EnsureNotNull(DefaultMappingContext, "Please config DefaultMappingContext in blueprint.");

                    Subsystem.AddMappingContext(DefaultMappingContext, 0);
                }
                else
                {
                    Logger.LogError("Failed find UEnhancedInputLocalPlayerSubsystem");
                }
            }

            OnJump?.Add(this, nameof(OnPlayerJump));
        }

        [UFUNCTION(Category = "Input")]        
        public void Move(FInputActionValue value)
        {
            // input is a Vector2D
            FVector2D MovementVector = value.GetAxis2D();

            if (Controller != null)
            {
                // find out which way is forward
                FRotator Rotation = Controller.GetControlRotation();
                FRotator YawRotation = new FRotator(0, Rotation.Yaw, 0);

                // get forward vector
                FVector ForwardDirection = UKismetMathLibrary.GetForwardVector(YawRotation);

                // get right vector 
                FVector RightDirection = UKismetMathLibrary.GetRightVector(YawRotation);

                // add movement 
                AddMovementInput(ForwardDirection, (float)MovementVector.Y);
                AddMovementInput(RightDirection, (float)MovementVector.X);

                //Logger.LogD("Move in C# {0}, {1}", MovementVector.X, MovementVector.Y);
            }
        }

        [UFUNCTION(Category = "Input")]
        public void Look(FInputActionValue value)
        {
            // input is a Vector2D
            FVector2D LookAxisVector = value.GetAxis2D();

            if (Controller != null)
            {
                // add yaw and pitch input to controller
                AddControllerYawInput((float)LookAxisVector.X);
                AddControllerPitchInput((float)LookAxisVector.Y);

                //Logger.LogD("Look in C# {0}, {1}", LookAxisVector.X, LookAxisVector.Y);
            }
        }

        [UFUNCTION(Category = "Input")]
        public void JumpWithEvent()
        {
            Jump();
            
            OnJump?.Broadcast(JumpCount);

            Logger.Log("Jump Event with:{0}", GetLocalRole());

            if(GetLocalRole() != ENetRole.ROLE_Authority)
            {
                ServerIncJumpCount();
            }
        }

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
}
