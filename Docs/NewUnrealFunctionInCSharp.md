# New UFunction in UClass

## UFunctions of UClass
Use UFUNCTIONAttribute or UEVENTAttribute to mark a method so that it is considered a UFunction.  
```C#
    // ASharpGameCharacter.def.cs
    [UCLASS(Config = "Game")]
    public class ASharpGameCharacter : ACharacter
    {
        [UEVENT(EFunctionFlags.Net | EFunctionFlags.NetServer)]
        public void ServerIncJumpCount() { }

        [UFUNCTION]
        public void OnRep_JumpCount() { }

    }
```
**`[UEVENT]` is equivalent to `[UFUNCTION(UFunctionFlags.BlueprintEvent|UFunctionFlags.Event]`. UEVENT is provided just to reduce the amount of input and be more intuitive.**  

Only UEVENT must start from def.cs. Ordinary UFunction can be implemented directly in the specific class implementation. If you write the corresponding declaration in def.cs, the tool will automatically generate an interface for your class. This interface will contain the basic information of the necessary functions to avoid missing content that cannot be ignored when implementing a specific class.   
```C#
// ASharpGameCharacter.gen.cs
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[UCLASS(EClassFlags.Config, Config = "Game", Guid = "f0122c2a-5b49-6190-e02b-a2ca3acc045e")]
public partial class ASharpGameCharacter : ACharacter, ISharpGameCharacter
{
    // ...
} 

#region Interface ISharpGameCharacter
public partial interface ISharpGameCharacter
{
    #region Properties
    USpringArmComponent? CameraBoom{ get; }
    UCameraComponent? FollowCamera{ get; }
    UInputMappingContext? DefaultMappingContext{ get; }
    UInputAction? JumpAction{ get; }
    UInputAction? MoveAction{ get; }
    UInputAction? LookAction{ get; }
    int JumpCount{ get; }
    #endregion

    #region Methods
    /// <summary>
    /// Interface method of ReceiveBeginPlay
    /// </summary>
    void ReceiveBeginPlay_Implementation();
    /// <summary>
    /// Interface method of ServerIncJumpCount
    /// </summary>
    void ServerIncJumpCount_Implementation();
    /// <summary>
    /// Interface method of OnRep_JumpCount
    /// </summary>
    void OnRep_JumpCount();
    #endregion
}
#endregion
```

## UFunction of FunctionLibrary
Similar to C++'s UBlueprintFunctionLibrary, implementing a function library in C# is a bit special. Generally speaking, it does not require a corresponding def.cs file, and all its UFunctions must be static.  
```C#
// UUnrealSharpCSharpTestsFunctionLibrary.cs
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
            outA = a.Retain()!;
            outB = b.Retain()!;

            List<string?> Result = [.. a, .. b];

            return Result;
        }

        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static ISet<FName> CSharpGetNameSetAndReturnByRef(ISet<FName> a, ISet<FName> b, ref ISet<FName> outA, ref ISet<FName> outB)
        {
            outA = a.Retain()!;
            outB = b.Retain()!;

            HashSet<FName> Result = [.. a, .. b];
            
            return Result;
        }

        [UFUNCTION(Category = "UnrealSharp_CSharp")]
        public static IDictionary<Int64, double> CSharpGetInt64DoubleMapAndReturnByRef(IDictionary<Int64, double> a, IDictionary<Int64, double> b, ref IDictionary<Int64, double> outA, ref IDictionary<Int64, double> outB)
        {
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
    }
}
```





