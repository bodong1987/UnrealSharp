using UnrealSharp.Utils.UnrealEngine;
using UnrealSharp.UnrealEngine.Bindings.Placeholders;
using UnrealSharp.GameScripts.Bindings.Placeholders;

namespace UnrealSharp.GameContent.Bindings.Defs.Tests;

[UCLASS]
internal class UUITestController : UObject
{
    [UPROPERTY]
    public TSubclassOf<UObject> ClassType = default;

    // error tests...
#if false
        [UPROPERTY]
        public TSubclassOf<UFrameTestAbstractObject> ClassType2 = default;

        [UPROPERTY]
        [UMETA(MetaConstants.AllowAbstract, true)]
        public TSubclassOf<UFrameTestAbstractObject> ClassTypeAllowAbstract = default;
#endif
}