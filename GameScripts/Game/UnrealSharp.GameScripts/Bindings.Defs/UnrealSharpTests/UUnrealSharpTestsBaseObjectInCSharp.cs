using UnrealSharp.UnrealEngine.Bindings.Placeholders;
using UnrealSharp.Utils.UnrealEngine;
using UnrealSharp.GameScripts.Bindings.Placeholders;

namespace UnrealSharp.GameScripts.Bindings.Defs.UnrealSharpTests
{
    [UCLASS(EClassFlags.Abstract)]
    public class UUnrealSharpTestsBaseObjectInCSharp : UObject
    {
#if false
        [UPROPERTY(Category = "Scalar")]
        public UClass? ClassType0; // unsupported error.
#endif

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
