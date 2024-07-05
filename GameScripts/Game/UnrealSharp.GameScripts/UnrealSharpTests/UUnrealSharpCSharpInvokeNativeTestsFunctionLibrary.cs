using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnrealSharp.UnrealEngine;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;
// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable PossibleUnintendedReferenceComparison
// ReSharper disable StringLiteralTypo
// ReSharper disable InlineTemporaryVariable
// ReSharper disable InvertIf
// ReSharper disable ConvertToConstant.Local
// ReSharper disable RedundantEmptyObjectCreationArgumentList
// ReSharper disable RedundantToStringCallForValueType

namespace UnrealSharp.GameScripts.UnrealSharpTests;

[UCLASS]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1861:不要将常量数组作为参数")]
public class UUnrealSharpCSharpInvokeNativeTestsFunctionLibrary : UBlueprintFunctionLibrary
{
    private static Random _random = new();

    public static bool CheckResult(bool result, [CallerMemberName] string? callerName = "", [CallerLineNumber] int number = 0)
    {
        var checkResult = $"{callerName} [{number}]:{(result ? "Success" : "Failure")}";

        if (result)
        {
            Logger.Log(checkResult);
        }
        else
        {
            Logger.LogError(checkResult);
            UUnrealSharpTestsFunctionLibraryInCpp.CppPrintText(checkResult, result ? FColor.Green : FColor.Red);
        }

        return result;
    }

    /// <summary>
    /// test invoke all tests function implement in UnrealTestsModule
    /// check C++ source in UnrealSharpTestsFunctionLibrary.cpp
    /// </summary>
    [UFUNCTION]
    public static void TestInvokeCppFunctionLibrary()
    {
        CheckObjectPropertyAccess_Bool();
        CheckObjectPropertyAccess_UInt8();
        CheckObjectPropertyAccess_Int32();
        CheckObjectPropertyAccess_Float();
        CheckObjectPropertyAccess_Double();
        CheckFString();
        CheckObjectPropertyAccess_FName();
        CheckObjectPropertyAccess_FVector();
        CheckFVectorValue();
        CheckFlagsEnum();
        CheckByteEnum();
        CheckFUnrealSharpTestsBaseStructValue();
        CheckGetObjectAndRef();
        CheckTSubclassOf();
        CheckUClass();
        CheckStringArray();
        CheckNameHashSet();
        CheckInt64DoubleDictionary();

        CheckObjectPropertiesAccess();


        UUnrealSharpTestsFunctionLibraryInCpp.CppPrintText("The C# call C++ test has been completed, and all errors have been output to the screen in red strings. If not, all tests have passed.", FColor.Green);
    }

    #region Invoke C++ Tests
    private static void CheckObjectPropertyAccess_Bool()
    {
        var a = true;
        var b = false;
        var oa = false;
        var ob = false;

        var result = UUnrealSharpTestsFunctionLibraryInCpp.CppAddAndReturnByRef_Bool(a, b, ref oa, ref ob);
        CheckResult(!result);
        CheckResult(a == oa);
        CheckResult(b == ob);

        result = UUnrealSharpTestsFunctionLibraryInCpp.CppAddAndReturnByRef_Bool(true, true, ref oa, ref ob);
        CheckResult(result);
        CheckResult(oa);
        CheckResult(ob);
    }

    private static void CheckObjectPropertyAccess_UInt8()
    {
        var a = (byte)_random.Next(byte.MaxValue);
        var b = (byte)_random.Next(byte.MaxValue);
        byte oa = 0;
        byte ob = 0;

        var result = UUnrealSharpTestsFunctionLibraryInCpp.CppAddAndReturnByRef_UInt8(a, b, ref oa, ref ob);
        CheckResult(result == (byte)(a + b));
        CheckResult(a == oa);
        CheckResult(b == ob);
    }

    private static void CheckObjectPropertyAccess_Int32()
    {
        var a = _random.Next();
        var b = _random.Next();
        var oa = 0;
        var ob = 0;

        var result = UUnrealSharpTestsFunctionLibraryInCpp.CppAddAndReturnByRef_Int32(a, b, ref oa, ref ob);
        CheckResult(result == a + b);
        CheckResult(a == oa);
        CheckResult(b == ob);
    }

    private static void CheckObjectPropertyAccess_Float()
    {
        var a = (float)_random.NextDouble();
        var b = (float)_random.NextDouble();
        var oa = 0.0f;
        var ob = 0.0f;

        var result = UUnrealSharpTestsFunctionLibraryInCpp.CppAddAndReturnByRef_Float(a, b, ref oa, ref ob);
        CheckResult(Math.Abs(result - (a + b)) < 0.0001f);
        CheckResult(Math.Abs(a - oa) < 0.0001f);
        CheckResult(Math.Abs(b - ob) < 0.0001f);
    }

    private static void CheckObjectPropertyAccess_Double()
    {
        var a = _random.NextDouble();
        var b = _random.NextDouble();
        var oa = 0.0;
        var ob = 0.0;

        var result = UUnrealSharpTestsFunctionLibraryInCpp.CppAddAndReturnByRef_Double(a, b, ref oa, ref ob);
        CheckResult(Math.Abs(result - (a + b)) < 0.0001);
        CheckResult(Math.Abs(a - oa) < 0.0001);
        CheckResult(Math.Abs(b - ob) < 0.0001);
    }

    private static void CheckFString()
    {
        var testStrings = new List<string> { "Hello", "World", "你好", "世界", "こんにちは", "世界" };

        foreach (var testString in testStrings)
        {
            var a = testString;
            var b = testString;
            var oa = string.Empty;
            var ob = string.Empty;

            var result = UUnrealSharpTestsFunctionLibraryInCpp.CppAddAndReturnByRef_String(a, b, ref oa, ref ob);
            CheckResult(result == a + b);
            CheckResult(a == oa);
            CheckResult(b == ob);
        }
    }

    private static void CheckObjectPropertyAccess_FName()
    {
        var testStrings = new List<string> { "Hello", "World", "你好", "世界", "こんにちは", "世界" };

        foreach (var testString in testStrings)
        {
            FName a = new FName(testString);
            FName b = new FName(testString);
            FName oa = FName.NAME_None;
            FName ob = FName.NAME_None;

            FName result = UUnrealSharpTestsFunctionLibraryInCpp.CppAddAndReturnByRef_Name(ref a, ref b, ref oa, ref ob);
            CheckResult(result == new FName(a.ToString() + b.ToString()));
            CheckResult(a == oa);
            CheckResult(b == ob);
        }
    }

    private static void CheckObjectPropertyAccess_FVector()
    {
        FVector a = new FVector(1.0f, 2.0f, 3.0f);
        FVector b = new FVector(4.0f, 5.0f, 6.0f);
        FVector oa = FVector.Zero;
        FVector ob = FVector.Zero;

        FVector result = UUnrealSharpTestsFunctionLibraryInCpp.CppAddAndReturnByRef_Vector(ref a, ref b, ref oa, ref ob);
        CheckResult((result - (a + b)).Length() < 0.0001f);
        CheckResult((a - oa).Length() < 0.0001f);
        CheckResult((b - ob).Length() < 0.0001f);
    }

    private static void CheckFVectorValue()
    {
        FVector a = new FVector(1.0f, 2.0f, 3.0f);
        FVector b = new FVector(4.0f, 5.0f, 6.0f);
        FVector oa = FVector.Zero;
        FVector ob = FVector.Zero;

        FVector result = UUnrealSharpTestsFunctionLibraryInCpp.CppAddAndReturnByRef_VectorValue(a, b, ref oa, ref ob);
        CheckResult((result - (a + b)).Length() < 0.0001f);
        CheckResult((a - oa).Length() < 0.0001f);
        CheckResult((b - ob).Length() < 0.0001f);
    }

    private static void CheckFlagsEnum()
    {
        EUnrealSharpLanguageTypesInCpp a = EUnrealSharpLanguageTypesInCpp.CSharp;
        EUnrealSharpLanguageTypesInCpp b = EUnrealSharpLanguageTypesInCpp.Python;
        EUnrealSharpLanguageTypesInCpp oa = EUnrealSharpLanguageTypesInCpp.None;
        EUnrealSharpLanguageTypesInCpp ob = EUnrealSharpLanguageTypesInCpp.None;

        EUnrealSharpLanguageTypesInCpp result = UUnrealSharpTestsFunctionLibraryInCpp.CppAddAndReturnByRef_Enum(a, b, ref oa, ref ob);
        CheckResult(result == (a | b));
        CheckResult(a == oa);
        CheckResult(b == ob);
    }

    private static void CheckByteEnum()
    {
        EUnrealSharpProjectsEnumTypeInCpp a = EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpProject;
        EUnrealSharpProjectsEnumTypeInCpp b = EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpTestsProject;
        EUnrealSharpProjectsEnumTypeInCpp oa = EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpProject;
        EUnrealSharpProjectsEnumTypeInCpp ob = EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpTestsProject;

        EUnrealSharpProjectsEnumTypeInCpp result = UUnrealSharpTestsFunctionLibraryInCpp.CppAddAndReturnByRef_ByteEnum(a, b, ref oa, ref ob);
        CheckResult(result == a);
        CheckResult(a == oa);
        CheckResult(b == ob);
    }

    private static void CheckFUnrealSharpTestsBaseStructValue()
    {
        FUnrealSharpTestsBaseStructValueInCpp a = new FUnrealSharpTestsBaseStructValueInCpp
        {
            bBoolValue = false,
            u8Value = 255,
            i32Value = 100,
            fValue = 3.141593f,
            dValue = 0.618000d,
            ProjectValue = EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpTestsProject,
            LanguageFlags = EUnrealSharpLanguageTypesInCpp.VisualBasic | EUnrealSharpLanguageTypesInCpp.JavaScript,
            StrValue = "Hello, UnrealSharp!!!",
            NameValue = new FName("Hello, UnrealSharp!!!"),
            TextValue = FText.FromString("Unreal")
        };

        FUnrealSharpTestsBaseStructValueInCpp b = a;
        FUnrealSharpTestsBaseStructValueInCpp oa = new FUnrealSharpTestsBaseStructValueInCpp();
        FUnrealSharpTestsBaseStructValueInCpp ob = new FUnrealSharpTestsBaseStructValueInCpp();

        FUnrealSharpTestsBaseStructValueInCpp result = UUnrealSharpTestsFunctionLibraryInCpp.CppGetUserStructAndReturnByRef(a, b, ref oa, ref ob);
        CheckResult(result.fValue == 1024);
        CheckResult(result.dValue == 2048);
        CheckResult(result.StrValue == "UnrealSharp: Hello, My Friends!");
        CheckResult(oa.StrValue == result.StrValue);
        CheckResult(ob.StrValue == result.StrValue);
    }

    private static void CheckGetObjectAndRef()
    {
        var a = NewObject<UUnrealSharpTestsObjectInCpp>(null, FName.FromString("Test"))!;
        a.i32ValueInCpp = 1024;
        a.StrValueInCpp = "Hello";

        var b = NewObject<UUnrealSharpTestsObjectInCpp>(null, FName.FromString("Test2"))!;

        b.i32ValueInCpp = 2048;
        b.StrValueInCpp = "World";

        UUnrealSharpTestsBaseObjectInCpp? oa = null;
        UUnrealSharpTestsBaseObjectInCpp? ob = null;

        UObject? result = UUnrealSharpTestsFunctionLibraryInCpp.CppGetObjectAndReturnByRef(a, b, ref oa, ref ob);

        CheckResult(result != null);
        CheckResult(oa == a);

        CheckResult(result!.GetClass()!.IsChildOf(UUnrealSharpTestsObjectInCpp.StaticClass()));
        CheckResult(ob == result);
    }

    private static void CheckTSubclassOf()
    {
        var a = UClass.GetTSubClassOf<UUnrealSharpTestsBaseObjectInCpp>();
        var b = new TSubclassOf<UUnrealSharpTestsBaseObjectInCpp>(UUnrealSharpTestsObjectInCpp.StaticClass());

        TSubclassOf<UUnrealSharpTestsBaseObjectInCpp> oa = default;
        TSubclassOf<UUnrealSharpTestsBaseObjectInCpp> ob = default;

        var result = UUnrealSharpTestsFunctionLibraryInCpp.CppGetSubclassOfAndReturnByRef(a, b, ref oa, ref ob);

        // Logger.Log("a = {0:X}, b = {1:X}, oa = {2:X}, ob = {3:X}", a.GetNativePtr(), b.GetNativePtr(), oa.GetNativePtr(), ob.GetNativePtr());

        CheckResult(result);
        CheckResult(oa == a);
        CheckResult(ob == b);
        CheckResult(result.GetClass() != null);
        CheckResult(result.GetClass() == UUnrealSharpTestsObjectInCpp.StaticClass());
    }

    private static void CheckUClass()
    {
        var a = (TSubclassOf<UObject>)UUnrealSharpTestsBaseObjectInCpp.StaticClass();
        var b = (TSubclassOf<UObject>)UUnrealSharpTestsObjectInCpp.StaticClass();

        TSubclassOf<UObject> oa = default;
        TSubclassOf<UObject> ob = default;

        var result = UUnrealSharpTestsFunctionLibraryInCpp.CppGetClassAndReturnByRef(a, b, ref oa, ref ob);

        // Logger.Log("a = {0:X}, b = {1:X}, oa = {2:X}, ob = {3:X}", a.GetNativePtr(), b.GetNativePtr(), oa.GetNativePtr(), ob.GetNativePtr());

        CheckResult(result);
        CheckResult(oa == a);
        CheckResult(ob == b);
        CheckResult(result.GetClass() != null);
        CheckResult(result.GetClass() == UUnrealSharpTestsBaseObjectInCpp.StaticClass());
    }

    private static void CheckStringArray()
    {
        IList<string?>? a = new List<string?> { "Hello", "World", "你好" };
        IList<string?>? b = new List<string?> { "Unreal", "Engine", "虚幻" };

        IList<string?>? oa = null;
        IList<string?>? ob = null;

        var result = UUnrealSharpTestsFunctionLibraryInCpp.CppGetStringArrayAndReturnByRef(ref a, ref b, ref oa, ref ob);

        if (CheckResult(result != null))
        {
            CheckResult(result!.Count == a!.Count + b!.Count);

            CheckResult(result.SequenceEqual([
                .. a,
                .. b
            ]));
        }

        if (CheckResult(oa != null))
        {
            CheckResult(oa!.SequenceEqual(a!));
        }

        if (CheckResult(ob != null))
        {
            CheckResult(ob!.SequenceEqual(b!));
        }
    }

    private static void CheckNameHashSet()
    {
        ISet<FName>? a = new HashSet<FName> { FName.FromString("Hello"), FName.FromString("World"), FName.FromString("Unreal") };
        ISet<FName>? b = new HashSet<FName> { FName.FromString("Unreal"), FName.FromString("Engine") };

        ISet<FName>? oa = null;
        ISet<FName>? ob = null;

        var result = UUnrealSharpTestsFunctionLibraryInCpp.CppGetNameSetAndReturnByRef(ref a, ref b, ref oa, ref ob);

        if (CheckResult(result != null))
        {
            CheckResult(result!.SetEquals(a!.Union(b!)));
        }

        if (CheckResult(oa != null))
        {
            CheckResult(oa!.SetEquals(a!));
        }

        if (CheckResult(ob != null))
        {
            CheckResult(ob!.SetEquals(b!));
        }
    }

    private static void CheckInt64DoubleDictionary()
    {
        IDictionary<long, double>? a = new Dictionary<long, double> { { 1, 1.0 }, { 2, 2.0 } };
        IDictionary<long, double>? b = new Dictionary<long, double> { { 3, 3.0 }, { 4, 4.0 } };

        IDictionary<long, double>? oa = null;
        IDictionary<long, double>? ob = null;

        var result = UUnrealSharpTestsFunctionLibraryInCpp.CppGetInt64DoubleMapAndReturnByRef(ref a, ref b, ref oa, ref ob);

        if (CheckResult(result != null))
        {
            CheckResult(result!.Count == a!.Count + b!.Count);

            foreach (var pair in a)
            {
                CheckResult(result.ContainsKey(pair.Key));
                CheckResult(result[pair.Key] == pair.Value);
            }

            foreach (var pair in b)
            {
                CheckResult(result.ContainsKey(pair.Key));
                CheckResult(result[pair.Key] == pair.Value);
            }
        }

        if (CheckResult(oa != null))
        {
            CheckResult(oa!.SequenceEqual(a!));
        }

        if (CheckResult(ob != null))
        {
            CheckResult(ob!.SequenceEqual(b!));
        }
    }
    #endregion

    #region C# Object Tests
    private static void CheckObjectPropertiesAccess()
    {
        UUnrealSharpTestsObjectInheritCPPObjectInCSharp? csObject = NewObject<UUnrealSharpTestsObjectInheritCPPObjectInCSharp>();

        CheckResult(csObject != null);
        Logger.EnsureNotNull(csObject);

        using ScopedPreventUnrealObjectGc scopedGc = new ScopedPreventUnrealObjectGc(csObject);

        CheckObjectPropertyAccess_Bool(csObject);
        CheckObjectPropertyAccess_UInt8(csObject);
        CheckObjectPropertyAccess_Int32(csObject);
        CheckObjectPropertyAccess_Float(csObject);
        CheckObjectPropertyAccess_Double(csObject);
        CheckObjectPropertyAccess_String(csObject);
        CheckObjectPropertyAccess_FName(csObject);
        CheckObjectPropertyAccess_FVector(csObject);
        CheckObjectPropertyAccess_ProjectValue(csObject);
        CheckObjectPropertyAccess_LanguageFlags(csObject);
        CheckObjectPropertyAccess_FVector3f(csObject);
        CheckObjectPropertyAccess_FRotator(csObject);
        CheckObjectPropertyAccess_FGuid(csObject);
        CheckObjectPropertyAccess_BaseTestBaseStructInCSharp(csObject);
        CheckObjectPropertyAccess_CppTestBaseStructInCSharp(csObject);
        CheckObjectPropertyAccess_CSharpTestBaseStructInCSharp(csObject);

        CheckObjectPropertyAccess_IntArray(csObject);
        CheckObjectPropertyAccess_StringArray(csObject);
        CheckObjectPropertyAccess_NameArray(csObject);
        CheckObjectPropertyAccess_VectorArray(csObject);
        CheckObjectPropertyAccess_ObjectArray(csObject);
        CheckObjectPropertyAccess_FloatSet(csObject);
        CheckObjectPropertyAccess_StringSet(csObject);
        CheckObjectPropertyAccess_ObjectSet(csObject);

        CheckObjectPropertyAccess_IntStringMap(csObject);
        CheckObjectPropertyAccess_NameVectorMap(csObject);
        CheckObjectPropertyAccess_FloatObjectMap(csObject);

        CheckObjectDelegate(csObject);
    }

    #region Common Tests
    private static void CheckObjectPropertyAccess_Bool(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        CheckResult(csObject.bBoolValueInCpp);
        csObject.bBoolValueInCpp = false;
        CheckResult(csObject.bBoolValueInCpp == false);

        CheckResult(csObject.bBoolBitMaskInCpp0);
        CheckResult(csObject.bBoolBitMaskInCpp1 == false);
        CheckResult(csObject.bBoolBitMaskInCpp2);
        CheckResult(csObject.bBoolBitMaskInCpp3 == false);

        csObject.bBoolBitMaskInCpp0 = false;
        csObject.bBoolBitMaskInCpp1 = true;
        csObject.bBoolBitMaskInCpp2 = false;
        csObject.bBoolBitMaskInCpp3 = true;

        CheckResult(csObject.bBoolBitMaskInCpp0 == false);
        CheckResult(csObject.bBoolBitMaskInCpp1);
        CheckResult(csObject.bBoolBitMaskInCpp2 == false);
        CheckResult(csObject.bBoolBitMaskInCpp3);

        CheckResult(csObject.bBoolValueInCSharp);
        csObject.bBoolValueInCSharp = false;
        CheckResult(csObject.bBoolValueInCSharp == false);
    }

    private static void CheckObjectPropertyAccess_UInt8(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        CheckResult(csObject.u8ValueInCpp == 128);
        csObject.u8ValueInCpp = 100;
        CheckResult(csObject.u8ValueInCpp == 100);

        CheckResult(csObject.u8ValueInCSharp == 128);
        csObject.u8ValueInCSharp = 100;
        CheckResult(csObject.u8ValueInCSharp == 100);
    }

    private static void CheckObjectPropertyAccess_Int32(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        CheckResult(csObject.i32ValueInCpp == 65535);
        csObject.i32ValueInCpp = 50000;
        CheckResult(csObject.i32ValueInCpp == 50000);

        CheckResult(csObject.i32ValueInCSharp == 65535);
        csObject.i32ValueInCSharp = 50000;
        CheckResult(csObject.i32ValueInCSharp == 50000);
    }

    private static void CheckObjectPropertyAccess_Float(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        CheckResult(Math.Abs(csObject.fValueInCpp - 3.1415926f) < 0.000001);
        csObject.fValueInCpp = 2.718281f;
        CheckResult(Math.Abs(csObject.fValueInCpp - 2.718281f) < 0.000001);

        CheckResult(Math.Abs(csObject.fValueInCSharp - 3.1415926f) < 0.000001);
        csObject.fValueInCSharp = 2.718281f;
        CheckResult(Math.Abs(csObject.fValueInCSharp - 2.718281f) < 0.000001);
    }

    private static void CheckObjectPropertyAccess_Double(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        CheckResult(Math.Abs(csObject.dValueInCpp - 0.618) < 0.000001);
        csObject.dValueInCpp = 1.414213;
        CheckResult(Math.Abs(csObject.dValueInCpp - 1.414213) < 0.000001);

        CheckResult(Math.Abs(csObject.dValueInCSharp - 0.618) < 0.000001);
        csObject.dValueInCSharp = 1.414213;
        CheckResult(Math.Abs(csObject.dValueInCSharp - 1.414213) < 0.000001);
    }

    private static void CheckObjectPropertyAccess_String(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        CheckResult(csObject.StrValueInCpp == "Hello UnrealSharp!!!");
        csObject.StrValueInCpp = "你好 虚幻!!!";
        CheckResult(csObject.StrValueInCpp == "你好 虚幻!!!");

        CheckResult(csObject.StrValueInCSharp == "Hello UnrealSharp!!!");
        csObject.StrValueInCSharp = "你好 虚幻!!!";
        CheckResult(csObject.StrValueInCSharp == "你好 虚幻!!!");
    }

    private static void CheckObjectPropertyAccess_FName(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        CheckResult(csObject.NameValueInCpp == new FName("Hello UnrealSharp!!!"));
        csObject.NameValueInCpp = new FName("你好 虚幻!!!");
        CheckResult(csObject.NameValueInCpp == new FName("你好 虚幻!!!"));

        CheckResult(csObject.NameValueInCSharp == new FName("Hello UnrealSharp!!!"));
        csObject.NameValueInCSharp = new FName("你好 虚幻!!!");
        CheckResult(csObject.NameValueInCSharp == new FName("你好 虚幻!!!"));
    }

    private static void CheckObjectPropertyAccess_FVector(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        CheckResult(csObject.VecValueInCpp.IsNearlyEqual(new FVector(1, 2, 3)));
        csObject.VecValueInCpp = new FVector(4, 5, 6);
        CheckResult(csObject.VecValueInCpp.IsNearlyEqual(new FVector(4, 5, 6)));

        CheckResult(csObject.VecValueInCSharp.IsNearlyEqual(new FVector(1, 2, 3)));
        csObject.VecValueInCSharp = new FVector(4, 5, 6);
        CheckResult(csObject.VecValueInCSharp.IsNearlyEqual(new FVector(4, 5, 6)));
    }

    private static void CheckObjectPropertyAccess_ProjectValue(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        CheckResult(csObject.ProjectValueInCpp == EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpProject);
        csObject.ProjectValueInCpp = EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpEditorProject;
        CheckResult(csObject.ProjectValueInCpp == EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpEditorProject);

        CheckResult(csObject.ProjectValueInCSharp == EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpTestsProject);
        csObject.ProjectValueInCSharp = EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpEditorProject;
        CheckResult(csObject.ProjectValueInCSharp == EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpEditorProject);
    }

    private static void CheckObjectPropertyAccess_LanguageFlags(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        // unreal does not support multiple flag fields default value
        csObject.LanguageFlags = EUnrealSharpLanguageTypesInCpp.VisualBasic | EUnrealSharpLanguageTypesInCpp.CPlusPlus;
        CheckResult(csObject.LanguageFlags == (EUnrealSharpLanguageTypesInCpp.VisualBasic | EUnrealSharpLanguageTypesInCpp.CPlusPlus));

        csObject.LanguageFlagsInCSharp = EUnrealSharpLanguageTypesInCpp.VisualBasic | EUnrealSharpLanguageTypesInCpp.CPlusPlus;
        CheckResult(csObject.LanguageFlagsInCSharp == (EUnrealSharpLanguageTypesInCpp.VisualBasic | EUnrealSharpLanguageTypesInCpp.CPlusPlus));
    }

    private static void CheckObjectPropertyAccess_FVector3f(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        CheckResult(Math.Abs(csObject.Vec3fValueInCpp.X - 1) < UnrealConstants.KindaSmallNumber);
        CheckResult(Math.Abs(csObject.Vec3fValueInCpp.Y - 2) < UnrealConstants.KindaSmallNumber);
        CheckResult(Math.Abs(csObject.Vec3fValueInCpp.Z - 3) < UnrealConstants.KindaSmallNumber);

        csObject.Vec3fValueInCpp = new FVector3f() { X = 4, Y = 5, Z = 6 };

        CheckResult(Math.Abs(csObject.Vec3fValueInCpp.X - 4) < UnrealConstants.KindaSmallNumber);
        CheckResult(Math.Abs(csObject.Vec3fValueInCpp.Y - 5) < UnrealConstants.KindaSmallNumber);
        CheckResult(Math.Abs(csObject.Vec3fValueInCpp.Z - 6) < UnrealConstants.KindaSmallNumber);

        CheckResult(Math.Abs(csObject.Vec3fValueInCSharp.X - 1) < UnrealConstants.KindaSmallNumber);
        CheckResult(Math.Abs(csObject.Vec3fValueInCSharp.Y - 2) < UnrealConstants.KindaSmallNumber);
        CheckResult(Math.Abs(csObject.Vec3fValueInCSharp.Z - 3) < UnrealConstants.KindaSmallNumber);

        csObject.Vec3fValueInCSharp = new FVector3f() { X = 4, Y = 5, Z = 6 };

        CheckResult(Math.Abs(csObject.Vec3fValueInCSharp.X - 4) < UnrealConstants.KindaSmallNumber);
        CheckResult(Math.Abs(csObject.Vec3fValueInCSharp.Y - 5) < UnrealConstants.KindaSmallNumber);
        CheckResult(Math.Abs(csObject.Vec3fValueInCSharp.Z - 6) < UnrealConstants.KindaSmallNumber);
    }

    private static void CheckObjectPropertyAccess_FRotator(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        CheckResult(csObject.RotValueInCpp.IsNearlyEqual(new FRotator(10, 20, 30)));
        csObject.RotValueInCpp = new FRotator(40, 50, 60);
        CheckResult(csObject.RotValueInCpp.IsNearlyEqual(new FRotator(40, 50, 60)));

        CheckResult(csObject.RotValueInCSharp.IsNearlyEqual(new FRotator(10, 20, 30)));
        csObject.RotValueInCSharp = new FRotator(40, 50, 60);
        CheckResult(csObject.RotValueInCSharp.IsNearlyEqual(new FRotator(40, 50, 60)));
    }

    private static void CheckObjectPropertyAccess_FGuid(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        FGuid newGuid = FGuid.NewGuid();
        csObject.GuidValueInCpp = newGuid;
        CheckResult(csObject.GuidValueInCpp == newGuid);

        newGuid = FGuid.NewGuid();
        csObject.GuidValueInCSharp = newGuid;
        CheckResult(csObject.GuidValueInCSharp == newGuid);
    }
    #endregion

    #region Structures Tests
    private static void CheckObjectPropertyAccess_BaseTestBaseStructInCSharp(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        var structValue = csObject.TestBaseStructInCpp;
        CheckResult(structValue.bBoolValue == false);
        CheckResult(structValue.u8Value == 255);
        CheckResult(structValue.i32Value == 100);
        CheckResult(Math.Abs(structValue.fValue - 3.1415926f) < UnrealConstants.KindaSmallNumber);
        CheckResult(Math.Abs(structValue.dValue - 0.618) < UnrealConstants.KindaSmallNumber);
        CheckResult(structValue.ProjectValue == EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpEditorProject);
        CheckResult(structValue.StrValue == "Hello, UnrealSharp!!!");
        CheckResult(structValue.NameValue == new FName("Hello, UnrealSharp!!!"));
        CheckResult(structValue.TextValue.ToString() == "Unreal");

        // Modify and check again
        structValue.bBoolValue = true;
        structValue.u8Value = 128;
        structValue.i32Value = 65535;
        structValue.fValue = 2.71828f;
        structValue.dValue = 0.577;
        structValue.ProjectValue = EUnrealSharpProjectsEnumTypeInCpp.SharpBindingGenProject;
        structValue.LanguageFlags = EUnrealSharpLanguageTypesInCpp.CSharp;
        structValue.StrValue = "你好，虚幻！！！";
        structValue.NameValue = new FName("你好，虚幻！！！");
        structValue.TextValue = FText.FromString("UnrealEngine");
        csObject.TestBaseStructInCpp = structValue;

        structValue = csObject.TestBaseStructInCpp;
        CheckResult(structValue.bBoolValue);
        CheckResult(structValue.u8Value == 128);
        CheckResult(structValue.i32Value == 65535);
        CheckResult(Math.Abs(structValue.fValue - 2.71828f) < UnrealConstants.KindaSmallNumber);
        CheckResult(Math.Abs(structValue.dValue - 0.577) < UnrealConstants.KindaSmallNumber);
        CheckResult(structValue.ProjectValue == EUnrealSharpProjectsEnumTypeInCpp.SharpBindingGenProject);
        CheckResult(structValue.LanguageFlags == EUnrealSharpLanguageTypesInCpp.CSharp);
        CheckResult(structValue.StrValue == "你好，虚幻！！！");
        CheckResult(structValue.NameValue == new FName("你好，虚幻！！！"));
        CheckResult(structValue.TextValue.ToString() == "UnrealEngine");
    }

    private static void CheckObjectPropertyAccess_CppTestBaseStructInCSharp(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        var structValue = csObject.CppTestBaseStructInCSharp;
        CheckResult(structValue.bBoolValue == false);
        CheckResult(structValue.u8Value == 255);
        CheckResult(structValue.i32Value == 100);
        CheckResult(Math.Abs(structValue.fValue - 3.1415926f) < UnrealConstants.KindaSmallNumber);
        CheckResult(Math.Abs(structValue.dValue - 0.618) < UnrealConstants.KindaSmallNumber);
        CheckResult(structValue.StrValue == "Hello, UnrealSharp!!!");
        CheckResult(structValue.NameValue == new FName("Hello, UnrealSharp!!!"));
        CheckResult(structValue.TextValue.ToString() == "Unreal");

        // Modify and check again
        structValue.bBoolValue = true;
        structValue.u8Value = 128;
        structValue.i32Value = 65535;
        structValue.fValue = 2.71828f;
        structValue.dValue = 0.577;
        structValue.ProjectValue = EUnrealSharpProjectsEnumTypeInCpp.SharpBindingGenProject;
        structValue.LanguageFlags = EUnrealSharpLanguageTypesInCpp.CSharp;
        structValue.StrValue = "你好，虚幻！！！";
        structValue.NameValue = new FName("你好，虚幻！！！");
        structValue.TextValue = FText.FromString("UnrealEngine");
        csObject.CppTestBaseStructInCSharp = structValue;

        structValue = csObject.CppTestBaseStructInCSharp;
        CheckResult(structValue.bBoolValue);
        CheckResult(structValue.u8Value == 128);
        CheckResult(structValue.i32Value == 65535);
        CheckResult(Math.Abs(structValue.fValue - 2.71828f) < UnrealConstants.KindaSmallNumber);
        CheckResult(Math.Abs(structValue.dValue - 0.577) < UnrealConstants.KindaSmallNumber);
        CheckResult(structValue.ProjectValue == EUnrealSharpProjectsEnumTypeInCpp.SharpBindingGenProject);
        CheckResult(structValue.LanguageFlags == EUnrealSharpLanguageTypesInCpp.CSharp);
        CheckResult(structValue.StrValue == "你好，虚幻！！！");
        CheckResult(structValue.NameValue == new FName("你好，虚幻！！！"));
        CheckResult(structValue.TextValue.ToString() == "UnrealEngine");
    }

    private static void CheckObjectPropertyAccess_CSharpTestBaseStructInCSharp(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        var structValue = csObject.CSharpTestBaseStructInCSharp;
        CheckResult(structValue.bBoolValueDefByProperty);
        CheckResult(structValue.i32ValueDefByProperty == 1024);
        CheckResult(structValue.bBoolValue == false);
        CheckResult(structValue.u8Value == 255);
        CheckResult(structValue.i32Value == 100);
        CheckResult(Math.Abs(structValue.fValue - 3.1415926f) < UnrealConstants.KindaSmallNumber);
        CheckResult(Math.Abs(structValue.dValue - 0.618) < UnrealConstants.KindaSmallNumber);
        CheckResult(structValue.ProjectValue == EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpTestsProject);
        CheckResult(structValue.LanguageFlags == EUnrealSharpLanguageTypesInCpp.FSharp);
        CheckResult(structValue.StrValue == "你好，UnrealSharp!!!");
        CheckResult(!structValue.NameValue.IsValid());
        CheckResult(structValue.TextValue.ToString().IsNullOrEmpty());

        // Modify and check again
        structValue.bBoolValueDefByProperty = false;
        structValue.i32ValueDefByProperty = 2048;
        structValue.bBoolValue = true;
        structValue.u8Value = 128;
        structValue.i32Value = 65535;
        structValue.fValue = 2.71828f;
        structValue.dValue = 0.577;
        structValue.ProjectValue = EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpProject;
        structValue.LanguageFlags = EUnrealSharpLanguageTypesInCpp.CSharp;
        structValue.StrValue = "你好，虚幻！！！";
        structValue.NameValue = new FName("你好，虚幻！！！");
        structValue.TextValue = FText.FromString("UnrealEngine");
        csObject.CSharpTestBaseStructInCSharp = structValue;

        structValue = csObject.CSharpTestBaseStructInCSharp;
        CheckResult(structValue.bBoolValueDefByProperty == false);
        CheckResult(structValue.i32ValueDefByProperty == 2048);
        CheckResult(structValue.bBoolValue);
        CheckResult(structValue.u8Value == 128);
        CheckResult(structValue.i32Value == 65535);
        CheckResult(Math.Abs(structValue.fValue - 2.71828f) < UnrealConstants.KindaSmallNumber);
        CheckResult(Math.Abs(structValue.dValue - 0.577) < UnrealConstants.KindaSmallNumber);
        CheckResult(structValue.ProjectValue == EUnrealSharpProjectsEnumTypeInCpp.UnrealSharpProject);
        CheckResult(structValue.LanguageFlags == EUnrealSharpLanguageTypesInCpp.CSharp);
        CheckResult(structValue.StrValue == "你好，虚幻！！！");
        CheckResult(structValue.NameValue == new FName("你好，虚幻！！！"));
        CheckResult(structValue.TextValue.ToString() == "UnrealEngine");
    }
    #endregion

    #region Test Int Array
    private static void CheckObjectPropertyAccess_IntArray(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        var cppArray = csObject.IntArrayInCpp;
        if (CheckResult(cppArray != null))
        {
            // Test Add
            cppArray!.Add(1000);
            cppArray.Add(2000);
            cppArray.Add(3000);
            cppArray.Add(4000);
            cppArray.Add(5000);
            cppArray.Add(6000);

            // Test Count
            CheckResult(cppArray.Count == 6);

            // Test Indexer
            CheckResult(cppArray[0] == 1000);
            CheckResult(cppArray[5] == 6000);

            // Test Contains
            CheckResult(cppArray.Contains(2000));
            CheckResult(!cppArray.Contains(7000));

            // Test IndexOf
            CheckResult(cppArray.IndexOf(3000) == 2);

            // Test Insert
            cppArray.Insert(3, 7000);
            CheckResult(cppArray.Count == 7);
            CheckResult(cppArray[3] == 7000);

            // Test Remove
            cppArray.Remove(7000);
            CheckResult(cppArray.Count == 6);
            CheckResult(!cppArray.Contains(7000));

            // Test RemoveAt
            cppArray.RemoveAt(5);
            CheckResult(cppArray.Count == 5);
            CheckResult(!cppArray.Contains(6000));

            // Test CopyFrom
            cppArray.CopyFrom(new[] { 1024, 2048, 3072, 4096, 5120 });
            CheckResult(cppArray.Count == 5);
            CheckResult(cppArray[0] == 1024);

            // Test Clear
            cppArray.Clear();
            CheckResult(cppArray.IsEmpty());
        }

        var csharpArray = csObject.IntArrayInCSharp;
        if (CheckResult(csharpArray != null))
        {
            // Test Add
            csharpArray!.Add(1000);
            csharpArray.Add(2000);
            csharpArray.Add(3000);
            csharpArray.Add(4000);
            csharpArray.Add(5000);
            csharpArray.Add(6000);

            // Test Count
            CheckResult(csharpArray.Count == 6);

            // Test Indexer
            CheckResult(csharpArray[0] == 1000);
            CheckResult(csharpArray[5] == 6000);

            // Test Contains
            CheckResult(csharpArray.Contains(2000));
            CheckResult(!csharpArray.Contains(7000));

            // Test IndexOf
            CheckResult(csharpArray.IndexOf(3000) == 2);

            // Test Insert
            csharpArray.Insert(3, 7000);
            CheckResult(csharpArray.Count == 7);
            CheckResult(csharpArray[3] == 7000);

            // Test Remove
            csharpArray.Remove(7000);
            CheckResult(csharpArray.Count == 6);
            CheckResult(!csharpArray.Contains(7000));

            // Test RemoveAt
            csharpArray.RemoveAt(5);
            CheckResult(csharpArray.Count == 5);
            CheckResult(!csharpArray.Contains(6000));

            // Test AddRange
            csharpArray.AddRange(new[] { 7000, 8000 });
            CheckResult(csharpArray.Count == 7);
            CheckResult(csharpArray.Contains(7000));
            CheckResult(csharpArray.Contains(8000));

            // Test Clear
            csharpArray.Clear();
            CheckResult(csharpArray.IsEmpty());
        }
    }
    #endregion

    #region Test String Array
    private static void CheckObjectPropertyAccess_StringArray(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        var cppArray = csObject.StringArrayInCpp;
        if (CheckResult(cppArray != null))
        {
            // Test Add
            cppArray!.Add("Hello");
            cppArray.Add("你好");
            cppArray.Add("こんにちは");
            cppArray.Add("안녕하세요");
            cppArray.Add("Bonjour");

            // Test Count
            CheckResult(cppArray.Count == 5);

            // Test Indexer
            CheckResult(cppArray[0] == "Hello");
            CheckResult(cppArray[4] == "Bonjour");

            // Test Contains
            CheckResult(cppArray.Contains("你好"));
            CheckResult(!cppArray.Contains("Hola"));

            // Test IndexOf
            CheckResult(cppArray.IndexOf("こんにちは") == 2);

            // Test Insert
            cppArray.Insert(3, "Hola");
            CheckResult(cppArray.Count == 6);
            CheckResult(cppArray[3] == "Hola");

            // Test Remove
            cppArray.Remove("Hola");
            CheckResult(cppArray.Count == 5);
            CheckResult(!cppArray.Contains("Hola"));

            // Test RemoveAt
            cppArray.RemoveAt(4);
            CheckResult(cppArray.Count == 4);
            CheckResult(!cppArray.Contains("Bonjour"));

            // Test CopyFrom
            cppArray.CopyFrom(new[] { "Guten Tag", "Buongiorno", "Добрый день" });
            CheckResult(cppArray.Count == 3);
            CheckResult(cppArray[0] == "Guten Tag");

            // Test Clear
            cppArray.Clear();
            CheckResult(cppArray.IsEmpty());
        }

        var csharpArray = csObject.StringArrayInCSharp;
        if (CheckResult(csharpArray != null))
        {
            // Test Add
            csharpArray!.Add("Hello");
            csharpArray.Add("你好");
            csharpArray.Add("こんにちは");
            csharpArray.Add("안녕하세요");
            csharpArray.Add("Bonjour");

            // Test Count
            CheckResult(csharpArray.Count == 5);

            // Test Indexer
            CheckResult(csharpArray[0] == "Hello");
            CheckResult(csharpArray[4] == "Bonjour");

            // Test Contains
            CheckResult(csharpArray.Contains("你好"));
            CheckResult(!csharpArray.Contains("Hola"));

            // Test IndexOf
            CheckResult(csharpArray.IndexOf("こんにちは") == 2);

            // Test Insert
            csharpArray.Insert(3, "Hola");
            CheckResult(csharpArray.Count == 6);
            CheckResult(csharpArray[3] == "Hola");

            // Test Remove
            csharpArray.Remove("Hola");
            CheckResult(csharpArray.Count == 5);
            CheckResult(!csharpArray.Contains("Hola"));

            // Test RemoveAt
            csharpArray.RemoveAt(4);
            CheckResult(csharpArray.Count == 4);
            CheckResult(!csharpArray.Contains("Bonjour"));

            // Test AddRange
            csharpArray.AddRange(new[] { "Guten Tag", "Buongiorno", "Добрый день" });
            CheckResult(csharpArray.Count == 7);
            CheckResult(csharpArray.Contains("Guten Tag"));
            CheckResult(csharpArray.Contains("Buongiorno"));
            CheckResult(csharpArray.Contains("Добрый день"));

            // Test Clear
            csharpArray.Clear();
            CheckResult(csharpArray.IsEmpty());
        }
    }
    #endregion

    #region Test FName Array
    private static void CheckObjectPropertyAccess_NameArray(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        var cppArray = csObject.NameArrayInCpp;
        if (CheckResult(cppArray != null))
        {
            // Test Add
            cppArray!.Add(FName.FromString("Hello"));
            cppArray.Add(FName.FromString("你好"));
            cppArray.Add(FName.FromString("こんにちは"));
            cppArray.Add(FName.FromString("안녕하세요"));
            cppArray.Add(FName.FromString("Bonjour"));

            // Test Count
            CheckResult(cppArray.Count == 5);

            // Test Indexer
            CheckResult(cppArray[0] == FName.FromString("Hello"));
            CheckResult(cppArray[4] == FName.FromString("Bonjour"));

            // Test Contains
            CheckResult(cppArray.Contains(FName.FromString("你好")));
            CheckResult(!cppArray.Contains(FName.FromString("Hola")));

            // Test IndexOf
            CheckResult(cppArray.IndexOf(FName.FromString("こんにちは")) == 2);

            // Test Insert
            cppArray.Insert(3, FName.FromString("Hola"));
            CheckResult(cppArray.Count == 6);
            CheckResult(cppArray[3] == FName.FromString("Hola"));

            // Test Remove
            cppArray.Remove(FName.FromString("Hola"));
            CheckResult(cppArray.Count == 5);
            CheckResult(!cppArray.Contains(FName.FromString("Hola")));

            // Test RemoveAt
            cppArray.RemoveAt(4);
            CheckResult(cppArray.Count == 4);
            CheckResult(!cppArray.Contains(FName.FromString("Bonjour")));

            // Test CopyFrom
            cppArray.CopyFrom(new[] { FName.FromString("Guten Tag"), FName.FromString("Buongiorno"), FName.FromString("Добрый день") });
            CheckResult(cppArray.Count == 3);
            CheckResult(cppArray[0] == FName.FromString("Guten Tag"));

            // Test Clear
            cppArray.Clear();
            CheckResult(cppArray.IsEmpty());
        }

        var csharpArray = csObject.NameArrayInCSharp;
        if (CheckResult(csharpArray != null))
        {
            // Test Add
            csharpArray!.Add(FName.FromString("Hello"));
            csharpArray.Add(FName.FromString("你好"));
            csharpArray.Add(FName.FromString("こんにちは"));
            csharpArray.Add(FName.FromString("안녕하세요"));
            csharpArray.Add(FName.FromString("Bonjour"));

            // Test Count
            CheckResult(csharpArray.Count == 5);

            // Test Indexer
            CheckResult(csharpArray[0] == FName.FromString("Hello"));
            CheckResult(csharpArray[4] == FName.FromString("Bonjour"));

            // Test Contains
            CheckResult(csharpArray.Contains(FName.FromString("你好")));
            CheckResult(!csharpArray.Contains(FName.FromString("Hola")));

            // Test IndexOf
            CheckResult(csharpArray.IndexOf(FName.FromString("こんにちは")) == 2);

            // Test Insert
            csharpArray.Insert(3, FName.FromString("Hola"));
            CheckResult(csharpArray.Count == 6);
            CheckResult(csharpArray[3] == FName.FromString("Hola"));

            // Test Remove
            csharpArray.Remove(FName.FromString("Hola"));
            CheckResult(csharpArray.Count == 5);
            CheckResult(!csharpArray.Contains(FName.FromString("Hola")));

            // Test RemoveAt
            csharpArray.RemoveAt(4);
            CheckResult(csharpArray.Count == 4);
            CheckResult(!csharpArray.Contains(FName.FromString("Bonjour")));

            // Test AddRange
            csharpArray.AddRange(new[] { FName.FromString("Guten Tag"), FName.FromString("Buongiorno"), FName.FromString("Добрый день") });
            CheckResult(csharpArray.Count == 7);
            CheckResult(csharpArray.Contains(FName.FromString("Guten Tag")));
            CheckResult(csharpArray.Contains(FName.FromString("Buongiorno")));
            CheckResult(csharpArray.Contains(FName.FromString("Добрый день")));

            // Test Clear
            csharpArray.Clear();
            CheckResult(csharpArray.IsEmpty());
        }
    }
    #endregion

    #region Test FVector Array
    private static void CheckObjectPropertyAccess_VectorArray(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        var cppArray = csObject.VectorArrayInCpp;
        if (CheckResult(cppArray != null))
        {
            // Test Add
            cppArray!.Add(new FVector(1, 2, 3));
            cppArray.Add(new FVector(4, 5, 6));
            cppArray.Add(new FVector(7, 8, 9));
            cppArray.Add(new FVector(10, 11, 12));
            cppArray.Add(new FVector(13, 14, 15));

            // Test Count
            CheckResult(cppArray.Count == 5);

            // Test Indexer
            CheckResult(cppArray[0].IsNearlyEqual(new FVector(1, 2, 3)));
            CheckResult(cppArray[4].IsNearlyEqual(new FVector(13, 14, 15)));

            // Test Contains
            CheckResult(cppArray.Any(item => item.IsNearlyEqual(new FVector(4, 5, 6))));
            CheckResult(!cppArray.Any(item => item.IsNearlyEqual(new FVector(16, 17, 18))));

            // Test IndexOf
            CheckResult(cppArray.IndexOf(new FVector(7, 8, 9)) == 2);

            // Test Insert
            cppArray.Insert(3, new FVector(16, 17, 18));
            CheckResult(cppArray.Count == 6);
            CheckResult(cppArray[3].IsNearlyEqual(new FVector(16, 17, 18)));

            // Test Remove
            cppArray.Remove(new FVector(16, 17, 18));
            CheckResult(cppArray.Count == 5);
            CheckResult(!cppArray.Any(item => item.IsNearlyEqual(new FVector(16, 17, 18))));

            // Test RemoveAt
            cppArray.RemoveAt(4);
            CheckResult(cppArray.Count == 4);
            CheckResult(!cppArray.Any(item => item.IsNearlyEqual(new FVector(13, 14, 15))));

            // Test CopyFrom
            cppArray.CopyFrom(new[] { new FVector(19, 20, 21), new FVector(22, 23, 24), new FVector(25, 26, 27) });
            CheckResult(cppArray.Count == 3);
            CheckResult(cppArray[0].IsNearlyEqual(new FVector(19, 20, 21)));

            // Test Clear
            cppArray.Clear();
            CheckResult(cppArray.IsEmpty());
        }

        var csharpArray = csObject.VectorArrayInCSharp;
        if (CheckResult(csharpArray != null))
        {
            // Test Add
            csharpArray!.Add(new FVector(1, 2, 3));
            csharpArray.Add(new FVector(4, 5, 6));
            csharpArray.Add(new FVector(7, 8, 9));
            csharpArray.Add(new FVector(10, 11, 12));
            csharpArray.Add(new FVector(13, 14, 15));

            // Test Count
            CheckResult(csharpArray.Count == 5);

            // Test Indexer
            CheckResult(csharpArray[0].IsNearlyEqual(new FVector(1, 2, 3)));
            CheckResult(csharpArray[4].IsNearlyEqual(new FVector(13, 14, 15)));

            // Test Contains
            CheckResult(csharpArray.Any(item => item.IsNearlyEqual(new FVector(4, 5, 6))));
            CheckResult(!csharpArray.Any(item => item.IsNearlyEqual(new FVector(16, 17, 18))));

            // Test IndexOf
            CheckResult(csharpArray.IndexOf(new FVector(7, 8, 9)) == 2);

            // Test Insert
            csharpArray.Insert(3, new FVector(16, 17, 18));
            CheckResult(csharpArray.Count == 6);
            CheckResult(csharpArray[3].IsNearlyEqual(new FVector(16, 17, 18)));

            // Test Remove
            csharpArray.Remove(new FVector(16, 17, 18));
            CheckResult(csharpArray.Count == 5);
            CheckResult(!csharpArray.Any(item => item.IsNearlyEqual(new FVector(16, 17, 18))));

            // Test RemoveAt
            csharpArray.RemoveAt(4);
            CheckResult(csharpArray.Count == 4);
            CheckResult(!csharpArray.Any(item => item.IsNearlyEqual(new FVector(13, 14, 15))));

            // Test AddRange
            csharpArray.AddRange(new[] { new FVector(19, 20, 21), new FVector(22, 23, 24), new FVector(25, 26, 27) });
            CheckResult(csharpArray.Count == 7);
            CheckResult(csharpArray.Any(item => item.IsNearlyEqual(new FVector(19, 20, 21))));
            CheckResult(csharpArray.Any(item => item.IsNearlyEqual(new FVector(22, 23, 24))));
            CheckResult(csharpArray.Any(item => item.IsNearlyEqual(new FVector(25, 26, 27))));

            // Test Clear
            csharpArray.Clear();
            CheckResult(csharpArray.IsEmpty());
        }
    }
    #endregion

    #region Test Object Array
    private static void CheckObjectPropertyAccess_ObjectArray(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        var cppArray = csObject.ObjectArrayInCpp;
        if (CheckResult(cppArray != null))
        {
            // Test Add
            var baseObject1 = NewObject<UUnrealSharpTestsBaseObjectInCpp>();
            var object1 = NewObject<UUnrealSharpTestsObjectInCpp>();
            cppArray!.Add(baseObject1);
            cppArray.Add(object1);

            // Test Count
            CheckResult(cppArray.Count == 2);

            // Test Indexer
            CheckResult(Equals(cppArray[0], baseObject1));
            CheckResult(Equals(cppArray[1], object1));

            // Test Contains
            CheckResult(cppArray.Contains(baseObject1));
            CheckResult(!cppArray.Contains(NewObject<UUnrealSharpTestsBaseObjectInCpp>()));

            // Test IndexOf
            CheckResult(cppArray.IndexOf(object1) == 1);

            // Test Insert
            var baseObject2 = NewObject<UUnrealSharpTestsBaseObjectInCpp>();
            cppArray.Insert(1, baseObject2);
            CheckResult(cppArray.Count == 3);
            CheckResult(Equals(cppArray[1], baseObject2));

            // Test Remove
            cppArray.Remove(baseObject2);
            CheckResult(cppArray.Count == 2);
            CheckResult(!cppArray.Contains(baseObject2));

            // Test RemoveAt
            cppArray.RemoveAt(1);
            CheckResult(cppArray.Count == 1);
            CheckResult(!cppArray.Contains(object1));

            // Test CopyFrom
            var objectArray = new[] { NewObject<UUnrealSharpTestsBaseObjectInCpp>(), NewObject<UUnrealSharpTestsObjectInCpp>() };
            cppArray.CopyFrom(objectArray);
            CheckResult(cppArray.Count == 2);
            CheckResult(Equals(cppArray[0], objectArray[0]));

            // Test Clear
            cppArray.Clear();
            CheckResult(cppArray.IsEmpty());
        }

        var csharpArray = csObject.ObjectArrayInCSharp;
        if (CheckResult(csharpArray != null))
        {
            // Test Add
            var baseObject1 = NewObject<UUnrealSharpTestsBaseObjectInCpp>();
            var object1 = NewObject<UUnrealSharpTestsObjectInCpp>();
            csharpArray!.Add(baseObject1);
            csharpArray.Add(object1);

            // Test Count
            CheckResult(csharpArray.Count == 2);

            // Test Indexer
            CheckResult(csharpArray[0] == baseObject1);
            CheckResult(csharpArray[1] == object1);

            // Test Contains
            CheckResult(csharpArray.Contains(baseObject1));
            CheckResult(!csharpArray.Contains(NewObject<UUnrealSharpTestsBaseObjectInCpp>()));

            // Test IndexOf
            CheckResult(csharpArray.IndexOf(object1) == 1);

            // Test Insert
            var baseObject2 = NewObject<UUnrealSharpTestsBaseObjectInCpp>();
            csharpArray.Insert(1, baseObject2);
            CheckResult(csharpArray.Count == 3);
            CheckResult(csharpArray[1] == baseObject2);

            // Test Remove
            csharpArray.Remove(baseObject2);
            CheckResult(csharpArray.Count == 2);
            CheckResult(!csharpArray.Contains(baseObject2));

            // Test RemoveAt
            csharpArray.RemoveAt(1);
            CheckResult(csharpArray.Count == 1);
            CheckResult(!csharpArray.Contains(object1));

            // Test AddRange
            var objectArray = new[] { NewObject<UUnrealSharpTestsBaseObjectInCpp>(), NewObject<UUnrealSharpTestsObjectInCpp>() };
            csharpArray.AddRange(objectArray);
            CheckResult(csharpArray.Count == 3);
            CheckResult(csharpArray.Contains(objectArray[0]));
            CheckResult(csharpArray.Contains(objectArray[1]));

            // Test Clear
            csharpArray.Clear();
            CheckResult(csharpArray.IsEmpty());
        }
    }
    #endregion

    #region Test FloatSet
    private static void CheckObjectPropertyAccess_FloatSet(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        var cppFloatSet = csObject.FloatSetInCpp;
        var csharpFloatSet = csObject.FloatSetInCSharp;
        var testData = new[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f, 10.0f };

        // Test for Cpp version
        if (CheckResult(cppFloatSet != null))
        {
            // Test Add
            foreach (var data in testData)
            {
                cppFloatSet!.Add(data);
            }

            // Test Count
            CheckResult(cppFloatSet!.Count == testData.Length);

            // Test Contains
            CheckResult(cppFloatSet.Contains(testData[0]));
            CheckResult(!cppFloatSet.Contains(11.0f));

            // Test Remove
            cppFloatSet.Remove(testData[0]);
            CheckResult(cppFloatSet.Count == testData.Length - 1);
            CheckResult(!cppFloatSet.Contains(testData[0]));

            // Test Clear
            cppFloatSet.Clear();
            CheckResult(cppFloatSet.IsEmpty());
        }

        // Test for CSharp version
        if (CheckResult(csharpFloatSet != null))
        {
            // Test Add
            foreach (var data in testData)
            {
                csharpFloatSet!.Add(data);
            }

            // Test Count
            CheckResult(csharpFloatSet!.Count == testData.Length);

            // Test Contains
            CheckResult(csharpFloatSet.Contains(testData[0]));
            CheckResult(!csharpFloatSet.Contains(11.0f));

            // Test Remove
            csharpFloatSet.Remove(testData[0]);
            CheckResult(csharpFloatSet.Count == testData.Length - 1);
            CheckResult(!csharpFloatSet.Contains(testData[0]));

            // Test Clear
            csharpFloatSet.Clear();
            CheckResult(csharpFloatSet.IsEmpty());
        }
    }
    #endregion

    #region Test StringSet
    private static void CheckObjectPropertyAccess_StringSet(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        var cppStringSet = csObject.StringSetInCpp;
        var csharpStringSet = csObject.StringSetInCSharp;
        var testData = new[] { "Hello", "Bonjour", "Hola", "你好", "こんにちは", "안녕하세요", "Guten Tag", "Ciao", "Olá", "Здравствуйте" };

        // Test for Cpp version
        if (CheckResult(cppStringSet != null))
        {
            // Test Add
            foreach (var data in testData)
            {
                cppStringSet!.Add(data);
            }

            // Test Count
            CheckResult(cppStringSet!.Count == testData.Length);

            // Test Contains
            CheckResult(cppStringSet.Contains(testData[0]));
            CheckResult(!cppStringSet.Contains("Namaste"));

            // Test Remove
            cppStringSet.Remove(testData[0]);
            CheckResult(cppStringSet.Count == testData.Length - 1);
            CheckResult(!cppStringSet.Contains(testData[0]));

            // Test Clear
            cppStringSet.Clear();
            CheckResult(cppStringSet.IsEmpty());
        }

        // Test for CSharp version
        if (CheckResult(csharpStringSet != null))
        {
            // Test Add
            foreach (var data in testData)
            {
                csharpStringSet!.Add(data);
            }

            // Test Count
            CheckResult(csharpStringSet!.Count == testData.Length);

            // Test Contains
            CheckResult(csharpStringSet.Contains(testData[0]));
            CheckResult(!csharpStringSet.Contains("Namaste"));

            // Test Remove
            csharpStringSet.Remove(testData[0]);
            CheckResult(csharpStringSet.Count == testData.Length - 1);
            CheckResult(!csharpStringSet.Contains(testData[0]));

            // Test Clear
            csharpStringSet.Clear();
            CheckResult(csharpStringSet.IsEmpty());
        }
    }
    #endregion

    #region Test ObjectSet
    private static void CheckObjectPropertyAccess_ObjectSet(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        var cppObjectSet = csObject.ObjectSetInCpp;
        var csharpObjectSet = csObject.ObjectSetInCSharp;

        // Test for Cpp version
        if (CheckResult(cppObjectSet != null))
        {
            // Test Add
            var object1 = NewObject<UUnrealSharpTestsObjectInCpp>();
            cppObjectSet!.Add(object1);

            // Test Count
            CheckResult(cppObjectSet.Count == 1);

            // Test Contains
            CheckResult(cppObjectSet.Contains(object1));
            CheckResult(!cppObjectSet.Contains(NewObject<UUnrealSharpTestsObjectInCpp>()));

            // Test Remove
            cppObjectSet.Remove(object1);
            CheckResult(cppObjectSet.Count == 0);
            CheckResult(!cppObjectSet.Contains(object1));

            // Test Clear
            cppObjectSet.Clear();
            CheckResult(cppObjectSet.IsEmpty());
        }

        // Test for CSharp version
        if (CheckResult(csharpObjectSet != null))
        {
            // Test Add
            var object1 = NewObject<UUnrealSharpTestsObjectInCpp>();
            csharpObjectSet!.Add(object1);

            // Test Count
            CheckResult(csharpObjectSet.Count == 1);

            // Test Contains
            CheckResult(csharpObjectSet.Contains(object1));
            CheckResult(!csharpObjectSet.Contains(NewObject<UUnrealSharpTestsObjectInCpp>()));

            // Test Remove
            csharpObjectSet.Remove(object1);
            CheckResult(csharpObjectSet.Count == 0);
            CheckResult(!csharpObjectSet.Contains(object1));

            // Test Clear
            csharpObjectSet.Clear();
            CheckResult(csharpObjectSet.IsEmpty());
        }
    }
    #endregion

    #region Test IntStringMap
    private static void CheckObjectPropertyAccess_IntStringMap(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        var cppIntStringMap = csObject.IntStringMapInCpp;
        var csharpIntStringMap = csObject.IntStringMapInCSharp;
        var testData = new Dictionary<int, string?> { { 1, "One" }, { 2, "Two" }, { 3, "Three" } };

        // Test for Cpp version
        if (CheckResult(cppIntStringMap != null))
        {
            // Test Add
            foreach (var data in testData)
            {
                cppIntStringMap![data.Key] = data.Value;
            }

            // Test Count
            CheckResult(cppIntStringMap!.Count == testData.Count);

            // Test Contains
            CheckResult(cppIntStringMap.ContainsKey(1));
            CheckResult(!cppIntStringMap.ContainsKey(4));

            // Test Remove
            cppIntStringMap.Remove(1);
            CheckResult(cppIntStringMap.Count == testData.Count - 1);
            CheckResult(!cppIntStringMap.ContainsKey(1));

            // Test Clear
            cppIntStringMap.Clear();
            CheckResult(cppIntStringMap.Count == 0);
        }

        // Test for CSharp version
        if (CheckResult(csharpIntStringMap != null))
        {
            // Test Add
            foreach (var data in testData)
            {
                csharpIntStringMap![data.Key] = data.Value;
            }

            // Test Count
            CheckResult(csharpIntStringMap!.Count == testData.Count);

            // Test Contains
            CheckResult(csharpIntStringMap.ContainsKey(1));
            CheckResult(!csharpIntStringMap.ContainsKey(4));

            // Test Remove
            csharpIntStringMap.Remove(1);
            CheckResult(csharpIntStringMap.Count == testData.Count - 1);
            CheckResult(!csharpIntStringMap.ContainsKey(1));

            // Test Clear
            csharpIntStringMap.Clear();
            CheckResult(csharpIntStringMap.Count == 0);
        }
    }
    #endregion

    #region Test NameVectorMap
    private static void CheckObjectPropertyAccess_NameVectorMap(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        var cppNameVectorMap = csObject.NameVectorMapInCpp;
        var csharpNameVectorMap = csObject.NameVectorMapInCSharp;
        var testData = new Dictionary<FName, FVector> { { new FName("Name1"), new FVector(1, 2, 3) }, { new FName("Name2"), new FVector(4, 5, 6) } };

        // Test for Cpp version
        if (CheckResult(cppNameVectorMap != null))
        {
            // Test Add
            foreach (var data in testData)
            {
                cppNameVectorMap![data.Key] = data.Value;
            }

            // Test Count
            CheckResult(cppNameVectorMap!.Count == testData.Count);

            // Test Contains
            CheckResult(cppNameVectorMap.ContainsKey(new FName("Name1")));
            CheckResult(!cppNameVectorMap.ContainsKey(new FName("Name3")));

            // Test Remove
            cppNameVectorMap.Remove(new FName("Name1"));
            CheckResult(cppNameVectorMap.Count == testData.Count - 1);
            CheckResult(!cppNameVectorMap.ContainsKey(new FName("Name1")));

            // Test Clear
            cppNameVectorMap.Clear();
            CheckResult(cppNameVectorMap.Count == 0);
        }

        // Test for CSharp version
        if (CheckResult(csharpNameVectorMap != null))
        {
            // Test Add
            foreach (var data in testData)
            {
                csharpNameVectorMap![data.Key] = data.Value;
            }

            // Test Count
            CheckResult(csharpNameVectorMap!.Count == testData.Count);

            // Test Contains
            CheckResult(csharpNameVectorMap.ContainsKey(new FName("Name1")));
            CheckResult(!csharpNameVectorMap.ContainsKey(new FName("Name3")));

            // Test Remove
            csharpNameVectorMap.Remove(new FName("Name1"));
            CheckResult(csharpNameVectorMap.Count == testData.Count - 1);
            CheckResult(!csharpNameVectorMap.ContainsKey(new FName("Name1")));

            // Test Clear
            csharpNameVectorMap.Clear();
            CheckResult(csharpNameVectorMap.Count == 0);
        }
    }
    #endregion

    #region Test FloatObjectMap
    private static void CheckObjectPropertyAccess_FloatObjectMap(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        var cppFloatObjectMap = csObject.FloatObjectMapInCpp;
        var csharpFloatObjectMap = csObject.FloatObjectMapInCSharp;
        var object1 = NewObject<UUnrealSharpTestsObjectInCpp>();
        var testData = new Dictionary<float, UObject?> { { 1.0f, object1 }, { 2.0f, null } };

        // Test for Cpp version
        if (CheckResult(cppFloatObjectMap != null))
        {
            // Test Add
            foreach (var data in testData)
            {
                cppFloatObjectMap![data.Key] = data.Value;
            }

            // Test Count
            CheckResult(cppFloatObjectMap!.Count == testData.Count);

            // Test Contains
            CheckResult(cppFloatObjectMap.ContainsKey(1.0f));
            CheckResult(!cppFloatObjectMap.ContainsKey(3.0f));

            // Test Remove
            cppFloatObjectMap.Remove(1.0f);
            CheckResult(cppFloatObjectMap.Count == testData.Count - 1);
            CheckResult(!cppFloatObjectMap.ContainsKey(1.0f));

            // Test Clear
            cppFloatObjectMap.Clear();
            CheckResult(cppFloatObjectMap.Count == 0);
        }

        // Test for CSharp version
        if (CheckResult(csharpFloatObjectMap != null))
        {
            // Test Add
            foreach (var data in testData)
            {
                csharpFloatObjectMap![data.Key] = data.Value;
            }

            // Test Count
            CheckResult(csharpFloatObjectMap!.Count == testData.Count);

            // Test Contains
            CheckResult(csharpFloatObjectMap.ContainsKey(1.0f));
            CheckResult(!csharpFloatObjectMap.ContainsKey(3.0f));

            // Test Remove
            csharpFloatObjectMap.Remove(1.0f);
            CheckResult(csharpFloatObjectMap.Count == testData.Count - 1);
            CheckResult(!csharpFloatObjectMap.ContainsKey(1.0f));

            // Test Clear
            csharpFloatObjectMap.Clear();
            CheckResult(csharpFloatObjectMap.Count == 0);
        }
    }
    #endregion

    #region Delegate Tests
    private static void CheckObjectDelegate(UUnrealSharpTestsObjectInheritCPPObjectInCSharp csObject)
    {
        csObject.DelegateInCpp!.Bind(csObject, nameof(csObject.OnCppDelegateCallback));
        csObject.MulticastDelegateInCpp!.Add(csObject, nameof(csObject.OnCppMulticastDelegateCallback));            
        csObject.MulticastDelegateInCSharp!.Add(csObject, nameof(csObject.OnCSharpMulticastDelegateCallback));

        CheckResult(csObject.OnCppDelegateCallbackCount == 0);
        CheckResult(csObject.OnCSharpDelegateCallbackCount == 0);

        var bParam = true;
        var iParam = 1024;
        var sParam = "Hello, UnrealSharp!!! 你好，虚幻！！！";
        FName nParam = FName.FromString("Unreal Engine 5!");
        var objParam = NewObject<UUnrealSharpTestsBaseObjectInCpp>();
        using ScopedPreventUnrealObjectGc scope = new ScopedPreventUnrealObjectGc(objParam!);

        FVector vecParam = new FVector(100, 200, 300);

        csObject.InvokeDelegateInCpp(iParam, sParam, nParam);

        CheckResult(csObject.OnCppDelegateCallbackCount == 1);
        CheckResult(csObject.LastCppDelegateParam.IntParam == iParam);
        CheckResult(csObject.LastCppDelegateParam.StringParam == sParam);
        CheckResult(csObject.LastCppDelegateParam.NameParam == nParam);

        csObject.BroadcastDelegateInCpp(bParam, vecParam, sParam, objParam);

        CheckResult(csObject.OnCppDelegateCallbackCount == 2);
        CheckResult(csObject.LastCppMulticastDelegateParam.BoolParam == bParam);
        CheckResult(csObject.LastCppMulticastDelegateParam.StringParam == sParam);
        CheckResult(csObject.LastCppMulticastDelegateParam.VecParam == vecParam);
        CheckResult(csObject.LastCppMulticastDelegateParam.ObjectParam == objParam);

        CheckResult(csObject.OnCSharpDelegateCallbackCount == 0);
                      
        csObject.MulticastDelegateInCSharp.Broadcast(iParam, sParam, objParam);

        CheckResult(csObject.OnCSharpDelegateCallbackCount == 1);
        CheckResult(csObject.LastCSharpMulticastDelegateParam.IntParam == iParam);
        CheckResult(csObject.LastCSharpMulticastDelegateParam.StringParam == sParam);
        CheckResult(csObject.LastCSharpMulticastDelegateParam.ObjectParam == objParam);

        // clear
        csObject.DelegateInCpp!.Unbind();
        csObject.MulticastDelegateInCpp!.Remove(csObject, nameof(csObject.OnCppMulticastDelegateCallback));
        csObject.MulticastDelegateInCSharp!.Remove(csObject, nameof(csObject.OnCSharpMulticastDelegateCallback));

        bParam = false;
        iParam = 2048;
        sParam = "Goodbye!!!";
        nParam = FName.FromString("See you!!");
        objParam = NewObject<UUnrealSharpTestsBaseObjectInCpp>();

        using ScopedPreventUnrealObjectGc scope2 = new ScopedPreventUnrealObjectGc(objParam!);

        // invoke again
        csObject.InvokeDelegateInCpp(iParam, sParam, nParam);
        csObject.BroadcastDelegateInCpp(bParam, vecParam, sParam, objParam);
        csObject.MulticastDelegateInCSharp.Broadcast(iParam, sParam, objParam);

        // data should not be changed.
        CheckResult(csObject.OnCppDelegateCallbackCount == 2);
        CheckResult(csObject.OnCSharpDelegateCallbackCount == 1);
    }

    #endregion

    #endregion
}