using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.GameScripts.TestGame
{
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

#if false // test overload error

        [UFUNCTION]
        public void TestThisIsAUFunction()
        {

        }

        [UFUNCTION]
        public void TestThisIsAUFunction(int a)
        {

        }
#endif
    }
}
