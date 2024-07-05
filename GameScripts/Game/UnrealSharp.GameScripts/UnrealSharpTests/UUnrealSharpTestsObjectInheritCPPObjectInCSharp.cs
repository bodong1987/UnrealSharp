using UnrealSharp.UnrealEngine;
using UnrealSharp.Utils.UnrealEngine;
// ReSharper disable InconsistentNaming

namespace UnrealSharp.GameScripts.UnrealSharpTests;

public partial class UUnrealSharpTestsObjectInheritCPPObjectInCSharp
{
    public int OnCppDelegateCallbackCount;

    public struct NativeDelegateValue
    {
        public int IntParam;
        public string? StringParam;
        public FName NameParam;
    }

    public NativeDelegateValue LastCppDelegateParam;

    public struct NativeMulticastDelegateValue
    {
        public bool BoolParam;
        public FVector VecParam;
        public string? StringParam;
        public UObject? ObjectParam;
    }

    public NativeMulticastDelegateValue LastCppMulticastDelegateParam;

    [UFUNCTION]
    public void OnCppDelegateCallback(int intParam, string? strParam, FName nameParam)
    {
        ++OnCppDelegateCallbackCount;

        LastCppDelegateParam.IntParam = intParam;
        LastCppDelegateParam.StringParam = strParam;
        LastCppDelegateParam.NameParam = nameParam;
    }

    [UFUNCTION]
    public void OnCppMulticastDelegateCallback(bool boolParam, FVector vecParam, string? strParam, UObject? objectParam)
    {
        ++OnCppDelegateCallbackCount;

        LastCppMulticastDelegateParam.BoolParam = boolParam;
        LastCppMulticastDelegateParam.VecParam = vecParam;
        LastCppMulticastDelegateParam.StringParam= strParam;
        LastCppMulticastDelegateParam.ObjectParam = objectParam;
    }

    public int OnCSharpDelegateCallbackCount;

    public struct CSharpDelegateParam
    {
        public int IntParam;
        public string? StringParam;
        public UObject? ObjectParam;
    }

    public CSharpDelegateParam LastCSharpMulticastDelegateParam;

    [UFUNCTION]
    public void OnCSharpMulticastDelegateCallback(int intParam, string? strParam, UObject? objectParam)
    {
        ++OnCSharpDelegateCallbackCount;

        LastCSharpMulticastDelegateParam.IntParam = intParam;
        LastCSharpMulticastDelegateParam.StringParam = strParam;
        LastCSharpMulticastDelegateParam.ObjectParam = objectParam;
    }



    // invalid usage
#if false
        [UFUNCTION]
        public static void DoSthStatic(int value)
        {

        }

        [UFUNCTION]
        public void DoSthNormal(DateTime time)
        {

        }

        

        [UFUNCTION]
        public void DoSthNormal2(uint time)
        {

        }
#endif
}