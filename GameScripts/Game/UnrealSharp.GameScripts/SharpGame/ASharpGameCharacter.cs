using UnrealSharp.UnrealEngine;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace UnrealSharp.GameScripts.SharpGame;

public partial class ASharpGameCharacter
{
    public void ReceiveBeginPlay_Implementation()
    {
        // invoke super
        base.ReceiveBeginPlay<ACharacter>();

        if (Controller is APlayerController playerController)
        {
            var subsystem = playerController.GetLocalPlayer()?.GetSubsystem<UEnhancedInputLocalPlayerSubsystem>();

            if(subsystem != null)
            {
                Logger.EnsureNotNull(DefaultMappingContext, "Please config DefaultMappingContext in blueprint.");

                subsystem.AddMappingContext(DefaultMappingContext);
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
        var movementVector = value.GetAxis2D();

        // ReSharper disable once InvertIf
        if (Controller != null)
        {
            // find out which way is forward
            var rotation = Controller.GetControlRotation();
            var yawRotation = new FRotator(0, rotation.Yaw, 0);

            // get forward vector
            var forwardDirection = UKismetMathLibrary.GetForwardVector(yawRotation);

            // get right vector 
            var rightDirection = UKismetMathLibrary.GetRightVector(yawRotation);

            // add movement 
            AddMovementInput(forwardDirection, (float)movementVector.Y);
            AddMovementInput(rightDirection, (float)movementVector.X);

            //Logger.LogD("Move in C# {0}, {1}", MovementVector.X, MovementVector.Y);
        }
    }

    [UFUNCTION(Category = "Input")]
    public void Look(FInputActionValue value)
    {
        // input is a Vector2D
        var lookAxisVector = value.GetAxis2D();

        // ReSharper disable once InvertIf
        if (Controller != null)
        {
            // add yaw and pitch input to controller
            AddControllerYawInput((float)lookAxisVector.X);
            AddControllerPitchInput((float)lookAxisVector.Y);

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

    [UFUNCTION]
#pragma warning disable CA1822
    private void OnPlayerJump(int jumpCount)
#pragma warning restore CA1822
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

    [UFUNCTION]
    public void OnRep_JumpCount()
    {
        Logger.Log("[{1}]OnRep_JumpCount, JumpCount = {0}", JumpCount, GetLocalRole());
    }
}