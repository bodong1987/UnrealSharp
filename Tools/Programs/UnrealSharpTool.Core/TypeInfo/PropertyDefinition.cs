using Newtonsoft.Json;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;
using UnrealSharpTool.Core.Utils;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace UnrealSharpTool.Core.TypeInfo;

/// <summary>
/// Enum EReferenceType
/// </summary>
public enum EReferenceType
{
    /// <summary>
    /// The unknown
    /// </summary>
    Unknown,
    /// <summary>
    /// The builtin type
    /// </summary>
    BuiltInType,
    /// <summary>
    /// The unreal type
    /// </summary>
    UnrealType,
    /// <summary>
    /// The user type
    /// </summary>
    UserType
}

/// <summary>
/// Class PropertyDefinition.
/// </summary>
public class PropertyDefinition
{
    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    /// <value>The parent.</value>
    [JsonIgnore]
    public StructTypeDefinition? Parent { get; set; }

    /// <summary>
    /// Gets or sets the name of the CPP type.
    /// </summary>
    /// <value>The name of the CPP type.</value>
    public string? CppTypeName { get; set; } = "";

    /// <summary>
    /// Gets or sets the name of the type.
    /// </summary>
    /// <value>The name of the type.</value>
    public string? TypeName { get; set; } = "";

    /// <summary>
    /// Gets or sets the type class.
    /// </summary>
    /// <value>The type class.</value>
    public string? TypeClass { get; set; } = "";

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string? Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the offset.
    /// </summary>
    /// <value>The offset.</value>
    public int Offset { get; set; }

    /// <summary>
    /// Gets or sets the flags.
    /// Because Unreal Json has precision issues when dealing with large integers, string is forced to be used to store large integers here.
    /// </summary>
    /// <value>The flags.</value>
    [JsonIgnore]
    public ulong Flags { get; set; }

    /// <summary>
    /// get and set Flags text
    /// Because Unreal Json has precision issues when dealing with large integers, string is forced to be used to store large integers here.
    /// </summary>
    /// <value>The flags t.</value>
    public string FlagsT
    {
        get => Flags.ToString();
        set => Flags = ulong.Parse(value);
    }

    /// <summary>
    /// Gets or sets the size.
    /// </summary>
    /// <value>The size.</value>
    public int Size { get; set; }

    // only valid for BoolProperty
    /// <summary>
    /// Gets or sets the field mask.
    /// </summary>
    /// <value>The field mask.</value>
    public byte FieldMask { get; set; } = 0xFF;

    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    /// <value>The unique identifier.</value>
    public string? Guid { get; set; } = System.Guid.Empty.ToString();

    /// <summary>
    /// Gets or sets the class path.
    /// </summary>
    /// <value>The class path.</value>
    public string? ClassPath { get; set; } = "";

    /// <summary>
    /// Gets or sets the meta's class.
    /// </summary>
    /// <value>the meta's class type</value>
    public string? MetaClass { get; set; } = "";

    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    /// <value>The default value.</value>
    public string? DefaultValue { get; set; } = "";

    /// <summary>
    /// Gets or sets the name of the automatic attach target.
    /// </summary>
    /// <value>The name of the automatic attach target.</value>
    public string? AutoAttachTargetName { get; set; }

    /// <summary>
    /// Gets or sets the type of the reference.
    /// </summary>
    /// <value>The type of the reference.</value>
    public EReferenceType ReferenceType { get; set; }

    /// <summary>
    /// Gets or sets the inner properties.
    /// </summary>
    /// <value>The inner properties.</value>
    public List<PropertyDefinition> InnerProperties { get; set; } = [];

    /// <summary>
    /// Gets or sets the metas.
    /// </summary>
    /// <value>The metas.</value>
    public MetaDefinition Metas { get; set; } = new();

    /// <summary>
    /// Gets or sets the signature function.
    /// </summary>
    /// <value>The signature function.</value>
    public FunctionTypeDefinition? SignatureFunction { get; set; }

    /// <summary>
    /// Gets a value indicating whether this instance is enum.
    /// </summary>
    /// <value><c>true</c> if this instance is enum; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsEnum => TypeClass == "EnumProperty";

    /// <summary>
    /// Gets a value indicating whether this instance is byte enum.
    /// </summary>
    /// <value><c>true</c> if this instance is byte enum; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsByteEnum => (TypeClass == "ByteProperty" && CppTypeName.iStartsWith("TEnumAsByte<")) || (TypeClass == "EnumProperty" && CppTypeName.iStartsWith("TEnumAsByte<"));

    /// <summary>
    /// Gets the name of the byte enum.
    /// </summary>
    /// <value>The name of the byte enum.</value>
    [JsonIgnore]
    public string ByteEnumName
    {
        get
        {
            if(!CppTypeName!.Contains("TEnumAsByte<"))
            {
                return CppTypeName;
            }

            var type = CppTypeName!.Substring("TEnumAsByte<".Length, CppTypeName.Length - "TEnumAsByte<".Length - 1);

            var index = type.IndexOf(':');

            return index != -1 ? type[..index] : type;
        }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is boolean.
    /// </summary>
    /// <value><c>true</c> if this instance is boolean; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsBoolean => TypeClass == "BoolProperty";

    /// <summary>
    /// Gets a value indicating whether this instance is native boolean.
    /// </summary>
    /// <value><c>true</c> if this instance is native boolean; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsNativeBoolean => IsBoolean && FieldMask == byte.MaxValue;

    [JsonIgnore]
    public bool IsNumeric =>
        // ReSharper disable once MergeIntoLogicalPattern
        TypeClass == "IntProperty" ||
        TypeClass == "ByteProperty" ||
        TypeClass == "Int64Property" ||
        TypeClass == "FloatProperty" ||
        TypeClass == "DoubleProperty";

    [JsonIgnore]
    public bool IsName => TypeClass == "NameProperty";

    /// <summary>
    /// Gets a value indicating whether this instance is string.
    /// </summary>
    /// <value><c>true</c> if this instance is string; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsString => TypeClass == "StrProperty";

    /// <summary>
    /// Gets a value indicating whether this instance is structure.
    /// </summary>
    /// <value><c>true</c> if this instance is structure; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsStruct => TypeClass == "StructProperty";

    /// <summary>
    /// Gets a value indicating whether this instance is u object.
    /// </summary>
    /// <value><c>true</c> if this instance is u object; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsUObject => TypeClass == "ObjectProperty";

    /// <summary>
    /// Gets a value indicating whether this instance is class.
    /// </summary>
    /// <value><c>true</c> if this instance is class; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsClass => TypeClass is "ClassProperty" or "ClassPtrProperty";

    /// <summary>
    /// Gets a value indicating whether this instance is reference.
    /// </summary>
    /// <value><c>true</c> if this instance is reference; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsReference => (Flags & (ulong)EPropertyFlags.ReferenceParm) != 0;

    /// <summary>
    /// Gets a value indicating whether this instance is out parameter.
    /// </summary>
    /// <value><c>true</c> if this instance is out parameter; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsOutParam => (Flags & (ulong)EPropertyFlags.OutParm) != 0 && !IsReturnParam && !IsConstParam;

    /// <summary>
    /// Gets a value indicating whether this instance is pass by pointer in CPP.
    /// </summary>
    /// <value><c>true</c> if this instance is pass by pointer in CPP; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsPassByReferenceInCpp => IsReference || IsOutParam;

    /// <summary>
    /// Gets a value indicating whether this instance is constant parameter.
    /// </summary>
    /// <value><c>true</c> if this instance is constant parameter; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsConstParam => (Flags & (ulong)EPropertyFlags.ConstParm) != 0;

    /// <summary>
    /// Gets a value indicating whether this instance is return parameter.
    /// </summary>
    /// <value><c>true</c> if this instance is return parameter; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsReturnParam => (Flags & (ulong)EPropertyFlags.ReturnParm) != 0;

    /// <summary>
    /// Gets a value indicating whether this instance is parameter property.
    /// </summary>
    /// <value><c>true</c> if this instance is parameter property; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsParamProperty => (Flags & (ulong)EPropertyFlags.ParmFlags) != 0;

    /// <summary>
    /// Gets a value indicating whether this instance is public setter.
    /// </summary>
    /// <value><c>true</c> if this instance is public setter; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsPublicSetter => !IsProtectedSetter && !IsPrivateSetter;

    /// <summary>
    /// Gets a value indicating whether this instance is protected setter.
    /// </summary>
    /// <value><c>true</c> if this instance is protected setter; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsProtectedSetter => (Flags & (ulong)EPropertyFlags.NativeAccessSpecifierProtected) != 0;

    /// <summary>
    /// Gets a value indicating whether this instance is private setter.
    /// </summary>
    /// <value><c>true</c> if this instance is private setter; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsPrivateSetter => (Flags & (ulong)EPropertyFlags.NativeAccessSpecifierPrivate) != 0;

    /// <summary>
    /// Gets a value indicating whether this instance is subclass of structure.
    /// </summary>
    /// <value><c>true</c> if this instance is subclass of structure; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsSubclassOfStructure => IsClass && CppTypeName == "TSubclassOf`1";

    /// <summary>
    /// Gets a value indicating whether this instance is structure property.
    /// </summary>
    /// <value><c>true</c> if this instance is structure property; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsStructProperty => Parent is { IsStruct: true };

    /// <summary>
    /// Gets a value indicating whether this instance is class property.
    /// </summary>
    /// <value><c>true</c> if this instance is class property; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsClassProperty => Parent is { IsClass: true };

    /// <summary>
    /// Gets a value indicating whether this instance is function property.
    /// </summary>
    /// <value><c>true</c> if this instance is function property; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsFunctionProperty => Parent is { IsFunction: true };

    /// <summary>
    /// Gets a value indicating whether this instance is collection property.
    /// </summary>
    /// <value><c>true</c> if this instance is collection property; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsCollectionProperty => TypeClass is "ArrayProperty" or "SetProperty" or "MapProperty";

    /// <summary>
    /// Gets a value indicating whether this instance is delegate property.
    /// </summary>
    /// <value><c>true</c> if this instance is delegate property; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsDelegateProperty => TypeClass == "DelegateProperty";

    /// <summary>
    /// Gets a value indicating whether this instance is multicast delegate property.
    /// </summary>
    /// <value><c>true</c> if this instance is multicast delegate property; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsMulticastDelegateProperty => TypeClass is "MulticastInlineDelegateProperty" or "MulticastSparseDelegateProperty";

    /// <summary>
    /// Gets a value indicating whether this instance is delegate relevance property.
    /// </summary>
    /// <value><c>true</c> if this instance is delegate relevance property; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsDelegateRelevanceProperty => IsDelegateProperty || IsMulticastDelegateProperty;

    /// <summary>
    /// Gets a value indicating whether this instance is soft object property.
    /// </summary>
    /// <value><c>true</c> if this instance is soft object property; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsSoftObjectProperty => TypeClass == "SoftObjectProperty";

    /// <summary>
    /// Gets a value indicating whether this instance is soft class property.
    /// </summary>
    /// <value><c>true</c> if this instance is soft class property; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsSoftClassProperty => TypeClass == "SoftClassProperty";

    /// <summary>
    /// Gets the name of the safe.
    /// </summary>
    /// <value>The name of the safe.</value>
    [JsonIgnore]
    public string SafeName => Name!.IsCSharpKeywords() ? $"@{Name}" : Name!;

    /// <summary>
    /// Gens the stable unique identifier.
    /// </summary>
    /// <param name="fullPath">The full path.</param>
    /// <returns>Guid.</returns>
    public static Guid GenStableGuid(string fullPath)
    {
        var stableSeed = fullPath.GetDeterministicHashCode();

        var random = new Random((int)stableSeed);

        var guidBytes = new byte[16];
        random.NextBytes(guidBytes);

        var guid = new Guid(guidBytes);

        return guid;
    }

    /// <summary>
    /// Gens the stable unique identifier string.
    /// </summary>
    /// <param name="fullPath">The full path.</param>
    /// <returns>System.String.</returns>
    public static string GenStableGuidString(string fullPath)
    {
        var guid = GenStableGuid(fullPath);
        return guid.ToString();
    }

    /// <summary>
    /// Resets the type of the script.
    /// </summary>
    internal void ResetScriptType()
    {
        switch (TypeClass)
        {
            case "ObjectProperty":
            {
                if(CppTypeName!.StartsWith('A') || CppTypeName!.StartsWith('U') || CppTypeName!.StartsWith('S'))
                {
                    TypeName = CppTypeName[1..];
                }

                break;
            }
            case "StructProperty":
            {
                if(CppTypeName!.StartsWith('F'))
                {
                    TypeName = CppTypeName[1..];
                }

                break;
            }
        }
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return $"{CppTypeName} {Name}";
    }

    /// <summary>
    /// Gets the inner property.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>PropertyDefinition.</returns>
    public PropertyDefinition GetInnerProperty(int index)
    {
        Logger.Assert(index >= 0 && index < InnerProperties.Count);

        return InnerProperties[index];
    }
}