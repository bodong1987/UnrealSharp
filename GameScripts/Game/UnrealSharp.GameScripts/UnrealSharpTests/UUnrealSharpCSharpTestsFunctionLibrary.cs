using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnrealSharp.UnrealEngine;
using UnrealSharp.UnrealEngine.Collections;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.GameScripts.UnrealSharpTests
{
    [UCLASS]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class UUnrealSharpCSharpTestsFunctionLibrary : UBlueprintFunctionLibrary
    {
        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static bool CSharpAddAndReturnByRef_Bool(bool a, bool b, ref bool outA, ref bool outB)
        {
            outA = a;
            outB = b;

            return a && b;
        }

        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static byte CSharpAddAndReturnByRef_UInt8(byte a, byte b, ref byte outA, ref byte outB)
        {
            outA = a; 
            outB = b;

            return (byte)(a + b);
        }

        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static int CSharpAddAndReturnByRef_Int32(int a, int b, ref int outA, ref int outB)
        {
            outA = a;
            outB = b;

            return (int)(a + b);
        }

		[UFUNCTION(Category = "UnrealSharp_CSharp")]
		public static Int64 CSharpAddAndReturnByRef_Int64(Int64 a, Int64 b, ref Int64 outA, ref Int64 outB)
		{
			outA = a;
			outB = b;

			return (Int64)(a + b);
		}

		[UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static float CSharpAddAndReturnByRef_Float(float a, float b, ref float outA, ref float outB)
        {
            outA = a;
            outB = b;

            return (float)(a + b);
        }

        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static double CSharpAddAndReturnByRef_Double(double a, double b, ref double outA, ref double outB)
        {
            outA = a;
            outB = b;

            return (double)(a + b);
        }

        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static string? CSharpAddAndReturnByRef_String(string? a, string? b, ref string? outA, ref string? outB)
        {
            outA = a;
            outB = b;

            return (a??"") + (b??"");
        }

        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static FName CSharpAddAndReturnByRef_Name(FName a, FName b, ref FName outA, ref FName outB)
        {
            outA = a;
            outB = b;

            return FName.FromString((a.ToString() ?? "") + (b.ToString() ?? ""));
        }

        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static FVector CSharpAddAndReturnByRef_Vector(FVector a, FVector b, ref FVector outA, ref FVector outB)
        {
            outA = a;
            outB = b;

            return a + b;
        }

        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static EUnrealSharpLanguageTypesInCpp CSharpAddAndReturnByRef_Enum(EUnrealSharpLanguageTypesInCpp a, EUnrealSharpLanguageTypesInCpp b, ref EUnrealSharpLanguageTypesInCpp outA, ref EUnrealSharpLanguageTypesInCpp outB)
        {
            outA = a;
            outB = b;

            return a | b;
        }

        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static EUnrealSharpProjectsEnumTypeInCpp CSharpAddAndReturnByRef_ByteEnum(EUnrealSharpProjectsEnumTypeInCpp a, EUnrealSharpProjectsEnumTypeInCpp b, ref EUnrealSharpProjectsEnumTypeInCpp outA, ref EUnrealSharpProjectsEnumTypeInCpp outB)
        {
            outA = a;
            outB = b;

            return a;
        }

		[UFUNCTION(Category = "UnrealSharp_CSharp")]
		public static EUnrealSharpCSharpEnumTypeInCSharp CSharpAddAndReturnByRef_CSharpEnum(EUnrealSharpCSharpEnumTypeInCSharp a, EUnrealSharpCSharpEnumTypeInCSharp b, ref EUnrealSharpCSharpEnumTypeInCSharp outA, ref EUnrealSharpCSharpEnumTypeInCSharp outB)
		{
            Logger.LogD("CSharpAddAndReturnByRef_CSharpEnum a={0} b={1}", a, b);
			outA = a;
			outB = b;

			return EUnrealSharpCSharpEnumTypeInCSharp.Method;
		}

		[UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static FUnrealSharpTestsBaseStructValueInCpp CSharpGetUserStructAndReturnByRef(FUnrealSharpTestsBaseStructValueInCpp a, FUnrealSharpTestsBaseStructValueInCpp b, ref FUnrealSharpTestsBaseStructValueInCpp outA, ref FUnrealSharpTestsBaseStructValueInCpp outB)
        {
            outA = a;
            outB = b;

            FUnrealSharpTestsBaseStructValueInCpp Return = new FUnrealSharpTestsBaseStructValueInCpp();

            Return.fValue = 1024;
            Return.dValue = 2048;
            Return.StrValue = "UnrealSharp:你好，朋友！";

            outA.StrValue = outB.StrValue = Return.StrValue;
            outA.NameValue = outB.NameValue = FName.FromString(Return.StrValue);

            return Return;
        }

		[UFUNCTION(Category = "UnrealSharp_CSharp")]
		public static FUnrealSharpTestsBaseStructValueInCSharp CSharpGetCSharpUserStructAndReturnByRef(FUnrealSharpTestsBaseStructValueInCSharp a, FUnrealSharpTestsBaseStructValueInCSharp b, ref FUnrealSharpTestsBaseStructValueInCSharp outA, ref FUnrealSharpTestsBaseStructValueInCSharp outB)
		{
			outA = a;
			outB = b;

			FUnrealSharpTestsBaseStructValueInCSharp Return = new FUnrealSharpTestsBaseStructValueInCSharp();

			Return.fValue = 1024;
			Return.dValue = 2048;
			Return.StrValue = "UnrealSharp:你好，朋友！";

			outA.StrValue = outB.StrValue = Return.StrValue;
			outA.NameValue = outB.NameValue = FName.FromString(Return.StrValue);

			return Return;
		}

		[UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static TSubclassOf<UUnrealSharpTestsBaseObjectInCpp> CSharpGetSubclassOfAndReturnByRef(TSubclassOf<UUnrealSharpTestsBaseObjectInCpp> a, TSubclassOf<UUnrealSharpTestsBaseObjectInCpp> b, ref TSubclassOf<UUnrealSharpTestsBaseObjectInCpp> outA, ref TSubclassOf<UUnrealSharpTestsBaseObjectInCpp> outB)
        {
			if (!a)
			{
				Logger.LogWarning("Invalid actor class a");
			}
			else
			{
				Logger.Log($"ActorClass={a.GetPathName()}");
			}

			if (!b)
			{
				Logger.LogWarning("Invalid object class b");
			}
			else
			{
				Logger.Log($"ObjectClass={b.GetPathName()}");
			}

			outA = a;
            outB = b;

            return (TSubclassOf<UUnrealSharpTestsBaseObjectInCpp>)UUnrealSharpTestsObjectInheritCPPObjectInCSharp.StaticClass();
        }

		[UFUNCTION(Category = "UnrealSharp_CSharp")]
		public static TSoftObjectPtr<UObject> CSharpPrintSoftObjectPtrAndReturnByRef(TSoftObjectPtr<UObject> a, ref TSoftObjectPtr<UObject> outA)
		{
			Logger.LogD($"SoftObjectPtr: IsPending={a.IsPending()} IsNull={a.IsNull()} IsValid={a.IsValid()} IsStale={a.IsStale()}");
			
			var softObject = a.Get();

			if (softObject == null)
			{
				Logger.LogWarning("Failed get soft object.");
			}
			else
			{
				Logger.Log($"Get soft object:{softObject.GetName()}");
			}

			Logger.LogD($"SoftObjectPtr: IsPending={a.IsPending()} IsNull={a.IsNull()} IsValid={a.IsValid()} IsStale={a.IsStale()}");

            outA = a;

			return a;
		}

		[UFUNCTION(Category = "UnrealSharp_CSharp")]
		public static TSoftClassPtr<UObject> CSharpPrintSoftClassPtrAndReturnByRef(TSoftClassPtr<UObject> b, ref TSoftClassPtr<UObject> outB)
		{			
			Logger.LogD($"SoftClassPtr: IsPending={b.IsPending()} IsNull={b.IsNull()} IsValid={b.IsValid()}");
            		
			var classPtr = b.Get();

			if (classPtr == null)
			{
				Logger.LogWarning("Failed get soft class");
			}
			else
			{
				Logger.Log($"Get soft class:{classPtr.GetPathName()}");
			}

			Logger.LogD($"SoftClassPtr: IsPending={b.IsPending()} IsNull={b.IsNull()} IsValid={b.IsValid()}");

			outB = b;

			return b;
		}

        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static UObject? CSharpGetObjectAndReturnByRef(UObject? a, UObject? b, ref UObject? outA, ref UObject? outB)
        {
            outA = a;
            outB = b;

            return NewObject<UUnrealSharpTestsObjectInheritCPPObjectInCSharp>();
        }

        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static IList<string?> CSharpGetStringArrayAndReturnByRef(IList<string?> a, IList<string?> b, ref IList<string?> outA, ref IList<string?> outB)
        {
			/*
            * You need to use retain here to obtain the complete resource ownership of the input parameters, 
            * because the ones that accept outA and outB may come from Unreal or C#. 
            * If they come from C# and are not retained, it may cause a crash caused by the loss of resource references. 
            * Unless you clearly know that neither a nor b are Views from Unreal data.
            */
			outA = a.Retain()!;  
            outB = b.Retain()!;

            List<string?> Result = [.. a, .. b];

            return Result;
        }

        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static ISet<FName> CSharpGetNameSetAndReturnByRef(ISet<FName> a, ISet<FName> b, ref ISet<FName> outA, ref ISet<FName> outB)
        {
			/*
            * You need to use retain here to obtain the complete resource ownership of the input parameters, 
            * because the ones that accept outA and outB may come from Unreal or C#. 
            * If they come from C# and are not retained, it may cause a crash caused by the loss of resource references. 
            * Unless you clearly know that neither a nor b are Views from Unreal data.
            */
			outA = a.Retain()!;
            outB = b.Retain()!;

            HashSet<FName> Result = [.. a, .. b];
            
            return Result;
        }

        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static IDictionary<Int64, double> CSharpGetInt64DoubleMapAndReturnByRef(IDictionary<Int64, double> a, IDictionary<Int64, double> b, ref IDictionary<Int64, double> outA, ref IDictionary<Int64, double> outB)
        {
			/*
            * You need to use retain here to obtain the complete resource ownership of the input parameters, 
            * because the ones that accept outA and outB may come from Unreal or C#. 
            * If they come from C# and are not retained, it may cause a crash caused by the loss of resource references. 
            * Unless you clearly know that neither a nor b are Views from Unreal data.
            */
			outA = a.Retain()!;
            outB = b.Retain()!;

            Dictionary<Int64, double> Result = new Dictionary<long, double>();

            foreach(var p in a)
            {
                Result.TryAdd(p.Key, p.Value);
            }

            foreach (var p in b)
            {
                Result.TryAdd(p.Key, p.Value);
            }

            return Result;
        }

        // invalid usage
#if false
        [UPROPERTY(Category = "Map")]
        public IDictionary<FName, FVector>? NameVectorMap;
#endif
    }
}
