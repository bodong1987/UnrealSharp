using System.Text;
using Mono.Cecil;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharpTool.Core.TypeInfo.MonoCecil;

/// <summary>
/// Class TypeInfoExtensions.
/// Mono.Cecil TypeDefinition fast access
/// </summary>
internal static class MonoTypeDefinitionExtensions
{
    #region Attribute Identification
    /// <summary>
    /// Check whether this TypeDefinition is an UnrealAttribute definition, including UFUNCTION, UPROPERTY, UCLASS, UENUM, and USTRUCT
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    private static bool IsUnrealAttributeType(this TypeDefinition? typeDefinition)
    {
        if (typeDefinition == null)
        {
            return false;
        }

        var fullName = typeDefinition.FullName;

        if(fullName == typeof(UCLASSAttribute).FullName ||
           fullName == typeof(USTRUCTAttribute).FullName ||
           fullName == typeof(UFUNCTIONAttribute).FullName ||
           fullName == typeof(UPROPERTYAttribute).FullName ||
           fullName == typeof(UENUMAttribute).FullName ||
           fullName == typeof(UEVENTAttribute).FullName
          )
        {
            return true;
        }

        if (typeDefinition.Name == typeof(UUnrealAttribute<>).Name)
        {
            return true;
        }

        var td = typeDefinition.Resolve();

        return td is { BaseType: not null } && td.BaseType.Resolve().IsUnrealAttributeType();
    }

    /// <summary>
    /// Is it an attribute tag of an unreal type defined by C#?
    /// </summary>
    /// <param name="customAttribute">The custom attribute.</param>
    public static bool IsCSharpImplementTypeAttribute(this CustomAttribute customAttribute)
    {
        return customAttribute.AttributeType.Resolve().IsUnrealAttributeType();
    }

    /// <summary>
    /// Determines whether [is c sharp implement method type attribute] [the specified custom attribute].
    /// </summary>
    /// <param name="customAttribute">The custom attribute.</param>
    /// <returns><c>true</c> if [is c sharp implement method type attribute] [the specified custom attribute]; otherwise, <c>false</c>.</returns>
    public static bool IsCSharpImplementMethodTypeAttribute(this CustomAttribute customAttribute)
    {
        return customAttribute.AttributeType.FullName == typeof(UFUNCTIONAttribute).FullName ||
               customAttribute.AttributeType.FullName == typeof(UEVENTAttribute).FullName;
    }

    /// <summary>
    /// is Binding Definition Attribute ?
    /// </summary>
    /// <param name="customAttribute">The custom attribute.</param>
    public static bool IsCSharpBindingDefinitionTypeAttribute(this CustomAttribute customAttribute)
    {
        return customAttribute.AttributeType.FullName == typeof(BindingDefinitionAttribute).FullName;
    }

    /// <summary>
    /// Is NativeBinding ?
    /// </summary>
    /// <param name="customAttribute">The custom attribute.</param>
    public static bool IsCSharpNativeBindingTypeAttribute(this CustomAttribute customAttribute)
    {
        return customAttribute.AttributeType.FullName == typeof(NativeBindingAttribute).FullName;
    }

    /// <summary>
    /// Is Blueprint Binding ?
    /// </summary>
    /// <param name="customAttribute">The custom attribute.</param>
    public static bool IsCSharpBlueprintBindingTypeAttribute(this CustomAttribute customAttribute)
    {
        return customAttribute.AttributeType.FullName == typeof(BlueprintBindingAttribute).FullName;
    }

    /// <summary>
    /// Determines whether [is c sharp unreal builtin type attribute] [the specified custom attribute].
    /// </summary>
    /// <param name="customAttribute">The custom attribute.</param>
    /// <returns><c>true</c> if [is c sharp unreal builtin type attribute] [the specified custom attribute]; otherwise, <c>false</c>.</returns>
    public static bool IsCSharpUnrealBuiltinTypeAttribute(this CustomAttribute customAttribute)
    {
        return customAttribute.AttributeType.FullName == typeof(UnrealBuiltinAttribute).FullName;
    }
    #endregion

    #region Type Identification
    /// <summary>
    /// Identify whether it is an Unreal type implemented on the C# side[include placeholder]
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    public static bool IsUnrealType(this TypeDefinition? typeDefinition)
    {
        var attrs = typeDefinition?.CustomAttributes.ToList();

        return attrs?.Find(x => x.IsCSharpBindingDefinitionTypeAttribute() || 
                                x.IsCSharpNativeBindingTypeAttribute() || 
                                x.IsCSharpImplementTypeAttribute() ||
                                x.IsCSharpBlueprintBindingTypeAttribute() ||
                                x.IsCSharpUnrealBuiltinTypeAttribute()
        ) != null;
    }

    /// <summary>
    /// Identify whether it is an Unreal type implemented on the C# side[no placeholder]
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    public static bool IsCSharpImplementType(this TypeDefinition? typeDefinition)
    {
        return typeDefinition?.CustomAttributes?.FirstOrDefault(
            x => x.AttributeType.Resolve().IsUnrealAttributeType()
        ) != null;
    }

    /// <summary>
    /// Identify whether it is an Unreal type implemented on the C# side[just placeholder]
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <returns><c>true</c> if [is c sharp placeholder type] [the specified type definition]; otherwise, <c>false</c>.</returns>
    public static bool IsCSharpBindingDefinitionType(this TypeDefinition? typeDefinition)
    {
        return typeDefinition?.CustomAttributes?.FirstOrDefault(
            x => x.IsCSharpBindingDefinitionTypeAttribute()
        ) != null;
    }

    /// <summary>
    /// Identify whether it is an Unreal type implemented on the C# side[just used to bind to Unreal native types, no self implement UFUNCTION/UPROPERTY...]
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    public static bool IsCSharpNativeBindingType(this TypeDefinition? typeDefinition)
    {
        return typeDefinition?.CustomAttributes?.FirstOrDefault(
            x => x.IsCSharpNativeBindingTypeAttribute()
        ) != null;
    }

    /// <summary>
    /// Determines whether [is c sharp blueprint binding type] [the specified type definition].
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <returns><c>true</c> if [is c sharp blueprint binding type] [the specified type definition]; otherwise, <c>false</c>.</returns>
    public static bool IsCSharpBlueprintBindingType(this TypeDefinition? typeDefinition)
    {
        return typeDefinition?.CustomAttributes?.FirstOrDefault(
            x => x.IsCSharpBlueprintBindingTypeAttribute()
        ) != null;
    }

    /// <summary>
    /// Determines whether [is c sharp unreal builtin type] [the specified type definition].
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <returns><c>true</c> if [is c sharp unreal builtin type] [the specified type definition]; otherwise, <c>false</c>.</returns>
    public static bool IsCSharpUnrealBuiltinType(this TypeDefinition? typeDefinition)
    {
        return typeDefinition?.CustomAttributes?.FirstOrDefault(
            x => x.IsCSharpUnrealBuiltinTypeAttribute()
        ) != null;
    }

    /// <summary>
    /// Is Delegate definition or Multicast Delegate
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns><c>true</c> if [is kind of delegate definition] [the specified type reference]; otherwise, <c>false</c>.</returns>
    public static bool IsKindOfDelegateDefinition(this TypeReference typeReference)
    {
        return IsMulticastDelegateDefinition(typeReference) || IsDelegateDefinition(typeReference);
    }

    /// <summary>
    /// Check if a delegate is defined?
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    public static bool IsDelegateDefinition(this TypeReference? typeReference)
    {
        return (typeReference != null && typeReference.FullName.Contains("TDelegate`1")) ||
               (typeReference?.Resolve() != null && typeReference.Resolve().BaseType != null && typeReference.Resolve().BaseType.FullName.Contains("TDelegate`1"));
    }

    /// <summary>
    /// Check if a multicast delegate is defined?
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    public static bool IsMulticastDelegateDefinition(this TypeReference? typeReference)
    {
        return (typeReference != null && typeReference.FullName.Contains("TMulticastDelegate`1")) ||
               (typeReference?.Resolve() != null && typeReference.Resolve().BaseType != null && typeReference.Resolve().BaseType!.FullName.Contains("TMulticastDelegate`1")); 
    }

    /// <summary>
    /// Determines whether [is soft object PTR definition] [the specified type reference].
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns><c>true</c> if [is soft object PTR definition] [the specified type reference]; otherwise, <c>false</c>.</returns>
    public static bool IsSoftObjectPtrDefinition(this TypeReference? typeReference)
    {
        return typeReference != null && typeReference.FullName.Contains("TSoftObjectPtr`1");
    }

    /// <summary>
    /// Determines whether [is soft class PTR definition] [the specified type reference].
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns><c>true</c> if [is soft class PTR definition] [the specified type reference]; otherwise, <c>false</c>.</returns>
    public static bool IsSoftClassPtrDefinition(this TypeReference? typeReference)
    {
        return typeReference != null && typeReference.FullName.Contains("TSoftClassPtr`1");
    }

    /// <summary>
    /// Extract signature information from delegate definition template
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns>TypeDefinition.</returns>
    public static TypeDefinition GetDelegateSignatureType(this TypeReference? typeReference)
    {
        var git4 = typeReference as GenericInstanceType ?? typeReference?.Resolve().BaseType as GenericInstanceType;

        Logger.EnsureNotNull(git4);

        var genericArgument = git4.GenericArguments[0];

        return genericArgument.Resolve();
    }
    #endregion

    #region Builtin Types
    private static readonly HashSet<string?> BuiltInTypes =
    [
        typeof(bool).FullName!,
        typeof(sbyte).FullName!,
        typeof(byte).FullName!,
        typeof(short).FullName!,
        typeof(ushort).FullName!,
        typeof(int).FullName!,
        typeof(uint).FullName!,
        typeof(long).FullName!,
        typeof(ulong).FullName!,
        typeof(float).FullName!,
        typeof(double).FullName!,
        typeof(string).FullName!
    ];

    private static readonly HashSet<string?> BlueprintableBuiltInTypes =
    [
        typeof(bool).FullName!,
        typeof(byte).FullName!,
        typeof(int).FullName!,
        typeof(long).FullName!,
        typeof(float).FullName!,
        typeof(double).FullName!,
        typeof(string).FullName!
    ];

    /// <summary>
    /// Check whether the type is a C# built-in type
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    public static bool IsCSharpBuiltInType(this TypeReference typeDefinition)
    {
        return BuiltInTypes.Contains(typeDefinition.FullName);
    }

    /// <summary>
    /// Although we support a large number of C# built-in type bindings, there are actually fewer types that are truly Blueprintable. 
    /// This is a limitation of Unreal Blueprints.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    public static bool IsCSharpBlueprintableBuiltInType(this TypeReference typeDefinition)
    {
        return BlueprintableBuiltInTypes.Contains(typeDefinition.FullName);
    }
    #endregion

    #region Check Type Avaiable
    /// <summary>
    /// Check whether this type can be used to interact with Unreal. If an unsupported type appears in the export code, an error should be reported.
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <param name="blueprintable">if set to <c>true</c> [blueprintable].</param>
    public static bool IsSupportedType(this TypeReference? typeReference, bool blueprintable = false)
    {
        if (typeReference == null)
        {
            return false;
        }

        // all pointer type is not supported.
        if(typeReference.IsPointer)
        {
            return false;
        }

        if (typeReference is ByReferenceType brt)
        {
            typeReference = brt.ElementType;
        }

        var typeFullName = typeReference.GetElementType().FullName;

        if (blueprintable)
        {
            if (BlueprintableBuiltInTypes.Contains(typeFullName))
            {
                return true;
            }
        }
        else
        {
            if (BuiltInTypes.Contains(typeFullName))
            {
                return true;
            }
        }

        var typeDefinition = typeReference.Resolve();

        if (typeDefinition.IsUnrealType())
        {
            return true;
        }

        // check generic types
        if (typeReference.Name.Contains("TSubclassOf`1"))
        {
            if (typeReference is GenericInstanceType git && git.GenericArguments.Count == 1)
            {
                var genericArgument = git.GenericArguments[0];

                return (genericArgument.Resolve().IsClass || genericArgument.Resolve().IsInterface) && genericArgument.IsSupportedType();
            }
        }
        else if (typeReference.Name.Contains("TSoftObjectPtr`1"))
        {
            if (typeReference is GenericInstanceType git && git.GenericArguments.Count == 1)
            {
                var genericArgument = git.GenericArguments[0];

                return genericArgument.Resolve().IsClass && genericArgument.IsSupportedType();
            }
        }
        else if (typeReference.Name.Contains("TSoftClassPtr`1"))
        {
            if (typeReference is GenericInstanceType git && git.GenericArguments.Count == 1)
            {
                var genericArgument = git.GenericArguments[0];

                return genericArgument.Resolve().IsClass && genericArgument.IsSupportedType();
            }
        }
        else if (typeDefinition.Name.Contains("IList`1") || typeDefinition.Name.Contains("ISet`1") ||
                 typeDefinition.Name.Contains("List`1") || typeDefinition.Name.Contains("Set`1") ||
                 typeDefinition.Name.Contains("TArray`1") || typeDefinition.Name.Contains("TSet`1")
                )
        {
            if (typeReference is GenericInstanceType git2 && git2.GenericArguments.Count == 1)
            {
                var genericArgument = git2.GenericArguments[0];

                // ReSharper disable once TailRecursiveCall
                return genericArgument.IsSupportedType();
            }
        }
        else if ( typeReference.Name.Contains("IDictionary`2") || typeReference.Name.Contains("Dictionary`2") || typeReference.Name.Contains("TMap`2"))
        {
            if(typeReference is GenericInstanceType git3 && git3.GenericArguments.Count == 2)
            {
                var genericArgument0 = git3.GenericArguments[0];
                var genericArgument1 = git3.GenericArguments[1];

                return genericArgument0.IsSupportedType() && genericArgument1.IsSupportedType();
            }                
        }
        else if (typeReference.IsMulticastDelegateDefinition() || 
                 (typeReference.Resolve().BaseType != null && typeReference.Resolve().BaseType.IsMulticastDelegateDefinition()))
        {
            // single broadcast delegate is not supported in blueprint

            if(typeReference is not GenericInstanceType git4)
            {
                git4 = (typeReference.Resolve().BaseType as GenericInstanceType)!;
            }

            Logger.EnsureNotNull(git4);

            var genericArgument = git4.GenericArguments[0];

            var type = genericArgument.Resolve();
            var typeBase = type?.BaseType;

            return typeBase != null && (typeBase.FullName == typeof(Delegate).FullName || typeBase.FullName == typeof(MulticastDelegate).FullName);
        }

        return false;
    }

    private static void GetElementTypes(TypeReference typeReference, HashSet<TypeReference> result, HashSet<TypeReference> processedTypes)
    {
        if(processedTypes.Contains(typeReference))
        {
            return;
        }

        if (typeReference is ByReferenceType brt)
        {
            typeReference = brt.ElementType;
        }

        if (typeReference is GenericInstanceType genericInstance)
        {
            foreach (var i in genericInstance.GenericArguments)
            {
                result.Add(i);

                processedTypes.Add(i);

                GetElementTypes(i, result, processedTypes);
            }
        }
    }

    /// <summary>
    /// Gets the element types.
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns>System.Collections.Generic.IEnumerable&lt;Mono.Cecil.TypeReference&gt;.</returns>
    public static IEnumerable<TypeReference> GetElementTypes(this TypeReference? typeReference)
    {
        if(typeReference == null)
        {
            return [];
        }
        
        HashSet<TypeReference> result = [];
        HashSet<TypeReference> processedReferences = [];

        GetElementTypes(typeReference, result, processedReferences);

        return result;
    }
    #endregion

    #region Export Configuration
    /// <summary>
    /// Gets the name of the export project.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <returns>System.String.</returns>
    public static string GetExportProjectName(this TypeDefinition typeDefinition)
    {
        Logger.Ensure<Exception>(typeDefinition.Namespace.StartsWith("UnrealSharp."), $"The namespace of the Unreal type you implement in C# must start with the UnrealSharp+ project name. Error Type:{typeDefinition}");

        var index = typeDefinition.Namespace.IndexOf('.', "UnrealSharp.".Length);

        return index == -1 ? typeDefinition.Namespace : typeDefinition.Namespace[..index];
    }

    /// <summary>
    /// Gets the name of the export project tiny.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <returns>System.String.</returns>
    public static string GetExportProjectTinyName(this TypeDefinition typeDefinition)
    {
        var projectName = GetExportProjectName(typeDefinition);

        var index = projectName.LastIndexOf('.');
        return index != -1 ? projectName[(index + 1)..] : projectName;
    }

    /// <summary>
    /// Gets the default export namespace.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <returns>System.String.</returns>
    public static string GetDefaultExportNamespace(this TypeDefinition typeDefinition)
    {
        // remove Defs in namespace.
        return typeDefinition.Namespace.Replace(".Bindings.Defs", "").Replace(".Bindings.Placeholders", "");
    }
    #endregion

    #region Enum
    /// <summary>
    /// Gets the underlying type of the enum
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <returns>System.String.</returns>
    public static string GetEnumUnderlyingType(this TypeDefinition typeDefinition)
    {
        if (!typeDefinition.IsEnum)
        {
            return "";
        }

        var field = typeDefinition.Fields.FirstOrDefault(x => x.Name == "value__");

        return field != null ? field.FieldType.FullName : "System.Int32";
    }

    /// <summary>
    /// Gets the size of the enum underlying type.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <returns>System.Int32.</returns>
    public static int GetEnumUnderlyingTypeSize(this TypeDefinition typeDefinition)
    {
        if (!typeDefinition.IsEnum)
        {
            return sizeof(int);
        }

        var field = typeDefinition.Fields.FirstOrDefault(x => x.Name == "value__");

        if (field == null)
        {
            return sizeof(int);
        }

        if(field.FieldType.FullName == typeof(sbyte).FullName ||
           field.FieldType.FullName == typeof(byte).FullName)
        {
            return sizeof(byte);
        }

        if (field.FieldType.FullName == typeof(short).FullName ||
            field.FieldType.FullName == typeof(ushort).FullName)
        {
            return sizeof(ushort);
        }

        if (field.FieldType.FullName == typeof(int).FullName ||
            field.FieldType.FullName == typeof(uint).FullName)
        {
            return sizeof(uint);
        }

        if (field.FieldType.FullName == typeof(long).FullName ||
            field.FieldType.FullName == typeof(ulong).FullName)
        {
            return sizeof(ulong);
        }

        return sizeof(int);
    }
    #endregion

    #region Value Type
    /// <summary>
    /// Determines whether Struct type.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    public static bool IsStructType(this TypeDefinition typeDefinition)
    {
        return typeDefinition is { IsValueType: true, IsEnum: false };
    }

    /// <summary>
    /// Gets the size of the type.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <returns>System.Int32.</returns>
    public static int GetTypeSize(this TypeDefinition typeDefinition)
    {
        if(typeDefinition.IsEnum)
        {
            return typeDefinition.GetEnumUnderlyingTypeSize();
        }

        return typeDefinition.IsInterface ? 8 : 0;
    }

    /// <summary>
    /// Determines whether [is nullable type] [the specified type reference].
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns>bool.</returns>
    public static bool IsNullableType(this TypeReference typeReference)
    {
        return typeReference is GenericInstanceType git && git.ElementType.FullName == typeof(Nullable<>).FullName;
    }
    #endregion

    #region Method
    /// <summary>
    /// Check if this method has valid return type.
    /// </summary>
    /// <param name="methodDefinition">The method definition.</param>
    /// <returns><c>true</c> if [has valid return type] [the specified method definition]; otherwise, <c>false</c>.</returns>
    public static bool HasValidReturnType(this MethodDefinition methodDefinition)
    {
        return methodDefinition.ReturnType == null ||
               methodDefinition.ReturnType.FullName == typeof(void).FullName ||
               methodDefinition.ReturnType.IsSupportedType();
    }

    /// <summary>
    /// Gets the method signature.
    /// This can usually be used for exact search methods
    /// </summary>
    /// <param name="method">The method.</param>
    /// <returns>System.String.</returns>
    public static string GetMethodSignature(this MethodDefinition method)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append($"{method.DeclaringType.FullName}:{method.Name} (");
        foreach (var param in method.Parameters)
        {
            stringBuilder.Append(GetFriendlyName(param));

            if (param != method.Parameters.Last())
            {
                stringBuilder.Append(',');
            }
        }

        stringBuilder.Append(')');

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Lookups the u functions.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="name">The name.</param>
    /// <returns>Mono.Cecil.MethodDefinition[].</returns>
    public static MethodDefinition[] LookupUFunctions(this TypeDefinition typeDefinition, string name)
    {
        var methods = new List<MethodDefinition>();

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var method in typeDefinition.Methods.Where(method => method.Name == name))
        {
            if (method.CustomAttributes.Any(attribute => attribute.IsCSharpImplementMethodTypeAttribute()))
            {
                methods.Add(method);
            }
        }

        return methods.ToArray();
    }

    /// <summary>
    /// Gets the friendly name for signature
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>System.String.</returns>
    private static string GetFriendlyName(ParameterDefinition parameter)
    {
        var typeReference = parameter.ParameterType;

        return GetFriendlyName(typeReference);
    }

    /// <summary>
    /// Gets the friendly name  for signature
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns>System.String.</returns>
    private static string GetFriendlyName(TypeReference typeReference)
    {
        var bIsReference = false;
        if (typeReference is ByReferenceType brt)
        {
            bIsReference = true;
            typeReference = brt.ElementType;
        }

        var refTag = bIsReference ? "&" : "";

        var typeName = typeReference.GetElementType().Name;
        var typeFullName = typeReference.GetElementType().FullName;

        if (typeName.Contains("TArray`1") || typeName.Contains("List`1") || typeName.Contains("IList`1"))
        {
            var type = typeReference as GenericInstanceType;

            Logger.EnsureNotNull(type);

            return $"{typeFullName}<{GetFriendlyName(type.GenericArguments[0])}>{refTag}";
        }

        if (typeName.Contains("TSet`1") || typeName.Contains("HashSet`1") || typeName.Contains("ISet`1"))
        {
            var type = typeReference as GenericInstanceType;

            Logger.EnsureNotNull(type);

            return $"{typeFullName}<{GetFriendlyName(type.GenericArguments[0])}>{refTag}";
        }

        if (typeName.Contains("TMap`2") || typeName.Contains("Dictionary`2") || typeName.Contains("IDictionary`2"))
        {
            var type = typeReference as GenericInstanceType;

            Logger.EnsureNotNull(type);

            return $"{typeFullName}<{GetFriendlyName(type.GenericArguments[0])}, {GetFriendlyName(type.GenericArguments[1])}>{refTag}";
        }

        return $"{GetFriendlyName(typeReference.FullName)}{refTag}";
    }

    /// <summary>
    /// convert C# full name to signature type name
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    /// <returns>System.String.</returns>
    private static string GetFriendlyName(string typeName)
    {
        switch (typeName)
        {
            case "System.Int32": return "int";
            case "System.UInt32": return "uint";
            case "System.Int64": return "long";
            case "System.UInt64": return "ulong";
            case "System.String": return "string";
            case "System.Boolean": return "bool";
            case "System.Single": return "single";
            case "System.Double": return "double";
            case "System.Byte": return "byte";
            case "System.SByte": return "sbyte";
            case "System.Char": return "char";
            case "System.Int16": return "short";
            case "System.UInt16": return "ushort";
            case "System.Decimal": return "decimal";
            case "System.Int32[]": return "int[]";
            case "System.String[]": return "string[]";

            default: return typeName;
        }
    }

    /// <summary>
    /// Determines whether [is u function] [the specified method].
    /// </summary>
    /// <param name="method">The method.</param>
    /// <returns><c>true</c> if [is u function] [the specified method]; otherwise, <c>false</c>.</returns>
    public static bool IsUFunction(this MethodDefinition method)
    {
        return method.CustomAttributes.FirstOrDefault(x =>
                   x.AttributeType.FullName == typeof(UFUNCTIONAttribute).FullName ||
                   x.AttributeType.FullName == typeof(UEVENTAttribute).FullName)
               != null;
    }
    #endregion

    #region Unreal Interop Service
    /// <summary>
    /// Get the Unreal FProperty type behind a type.
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns>System.String.</returns>
    /// <exception cref="System.Exception">Unsupported type :{typeReference}</exception>
    public static string GetTypeClass(this TypeReference typeReference)
    {
        var typeName = typeReference.GetElementType().Name;
        var typeFullName = typeReference.GetElementType().FullName;

        if(typeFullName == typeof(bool).FullName)
        {
            return "BoolProperty";
        }
        
        if(typeFullName == typeof(sbyte).FullName)
        {
            return "Int8Property";
        }

        if(typeFullName == typeof(byte).FullName)
        {
            return "ByteProperty";
        }

        if(typeFullName == typeof(short).FullName)
        {
            return "Int16Property";
        }

        if(typeFullName == typeof(ushort).FullName)
        {
            return "UInt16Property";
        }

        if (typeFullName == typeof(int).FullName)
        {
            return "IntProperty";
        }

        if (typeFullName == typeof(uint).FullName)
        {
            return "UInt32Property";
        }

        if (typeFullName == typeof(long).FullName)
        {
            return "Int64Property";
        }

        if (typeFullName == typeof(ulong).FullName)
        {
            return "UInt64Property";
        }

        if (typeFullName == typeof(float).FullName)
        {
            return "FloatProperty";
        }

        if (typeFullName == typeof(double).FullName)
        {
            return "DoubleProperty";
        }

        switch (typeName)
        {
            case "FName":
                return "NameProperty";
            case "FText":
                return "TextProperty";
        }

        if(typeName.Contains("TSubclassOf`1"))
        {
            // UClass? is not supported, so you need always use TSubclassOf<>, use TSubclass<UObject> instead UClass?
            return "ClassProperty";
        }

        if(typeReference.IsDelegateDefinition())
        {
            return "DelegateProperty";
        }

        if(typeReference.IsMulticastDelegateDefinition())
        {
            return "MulticastInlineDelegateProperty";
        }

        if(typeName.Contains("TArray`1") || typeName.Contains("List`1") || typeName.Contains("IList`1"))
        {
            return "ArrayProperty";
        }

        if(typeName.Contains("TSet`1") || typeName.Contains("HashSet`1") || typeName.Contains("ISet`1"))
        {
            return "SetProperty";
        }

        if(typeName.Contains("TMap`2") || typeName.Contains("Dictionary`2") || typeName.Contains("IDictionary`2"))
        {
            return "MapProperty";
        }

        if(typeName.Contains("TSoftObjectPtr`1"))
        {
            return "SoftObjectProperty";
        }

        if(typeName.Contains("TSoftClassPtr`1"))
        {
            return "SoftClassProperty";
        }

        if(typeFullName == typeof(string).FullName)
        {
            return "StrProperty";
        }

        if(typeReference.Resolve().IsStructType())
        {
            return "StructProperty";
        }

        if(typeReference.Resolve().IsEnum)
        {
            return "EnumProperty";
        }

        if(typeReference.Resolve().IsInterface || typeReference.Resolve().IsClass)
        {
            return "ObjectProperty";
        }

        throw new Exception($"Unsupported type :{typeReference}");
    }

    /// <summary>
    /// Gets the name of the unreal type in C++
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns>System.String.</returns>
    public static string GetUnrealTypeName(this TypeReference typeReference)
    {
        if (typeReference is ByReferenceType brt)
        {
            typeReference = brt.ElementType;
        }

        var typeName = typeReference.GetElementType().Name;
        var typeFullName = typeReference.GetElementType().FullName;

        if (typeFullName == typeof(bool).FullName)
        {
            return "bool";
        }

        if (typeFullName == typeof(sbyte).FullName)
        {
            return "int8";
        }

        if (typeFullName == typeof(byte).FullName)
        {
            return "uint8";
        }

        if (typeFullName == typeof(short).FullName)
        {
            return "int16";
        }

        if (typeFullName == typeof(ushort).FullName)
        {
            return "uint16";
        }

        if (typeFullName == typeof(int).FullName)
        {
            return "int32";
        }

        if (typeFullName == typeof(uint).FullName)
        {
            return "uint32";
        }

        if (typeFullName == typeof(long).FullName)
        {
            return "int64";
        }

        if (typeFullName == typeof(ulong).FullName)
        {
            return "uint64";
        }

        if (typeFullName == typeof(float).FullName)
        {
            return "float";
        }

        if (typeFullName == typeof(double).FullName)
        {
            return "double";
        }

        if(typeFullName == typeof(string).FullName)
        {
            return "FString";
        }

        if (typeName == "UClass")
        {
            return "UClass*";
        }

        if (typeName.Contains("TSubclassOf`1"))
        {
            var type = typeReference as GenericInstanceType;

            Logger.EnsureNotNull(type);

            var argName = type.GenericArguments[0].GetUnrealTypeName();
            if(argName.EndsWith('*'))
            {
                argName = argName[..^1];
            }
            return $"TSubclassOf<{argName}>";
        }

        if (typeName.Contains("TArray`1") || typeName.Contains("List`1") || typeName.Contains("IList`1"))
        {
            var type = typeReference as GenericInstanceType;

            Logger.EnsureNotNull(type);

            return $"TArray<{type.GenericArguments[0].GetUnrealTypeName()}>";
        }

        if (typeName.Contains("TSet`1") || typeName.Contains("HashSet`1") || typeName.Contains("ISet`1"))
        {
            var type = typeReference as GenericInstanceType;

            Logger.EnsureNotNull(type);

            return $"TSet<{type.GenericArguments[0].GetUnrealTypeName()}>";
        }

        if (typeName.Contains("TMap`2") || typeName.Contains("Dictionary`2") || typeName.Contains("IDictionary`2"))
        {
            var type = typeReference as GenericInstanceType;

            Logger.EnsureNotNull(type);

            return $"TMap<{type.GenericArguments[0].GetUnrealTypeName()}, {type.GenericArguments[1].GetUnrealTypeName()}>";
        }

        if (typeName.Contains("TSoftObjectPtr`1"))
        {
            var type = typeReference as GenericInstanceType;

            Logger.EnsureNotNull(type);

            return $"TSoftObjectPtr<{type.GenericArguments[0].GetUnrealTypeName()}>";
        }

        if (typeName.Contains("TSoftClassPtr`1"))
        {
            var type = typeReference as GenericInstanceType;

            Logger.EnsureNotNull(type);

            return $"TSoftClassPtr<{type.GenericArguments[0].GetUnrealTypeName()}>";
        }

        return typeReference.Resolve().IsInterface ? $"{typeName}*" : typeName;
    }
    #endregion

    #region Type Check
    /// <summary>
    /// Determines whether [is children type of] [the specified type definition].
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="fullNames">The full names.</param>
    /// <returns>bool.</returns>
    public static bool IsChildrenTypeOf(this TypeDefinition typeDefinition, IEnumerable<string> fullNames)
    {
        var type = typeDefinition;

        var names = fullNames.ToList();
            
        // ReSharper disable once MergeIntoLogicalPattern
        while (type != null && (
                   type.FullName != "UnrealSharp.UnrealEngine.UObject" ||
                   type.FullName != "UnrealSharp.UnrealEngine.Bindings.Placeholders.UObject" ||
                   type.FullName != "System.Object"
               ))
        {
            if (names.Contains(type.FullName))
            {
                return true;
            }

            type = type.BaseType?.Resolve();
        }

        return false;
    }

    /// <summary>
    /// Determines whether [is actor children type] [the specified type definition].
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <returns>bool.</returns>
    public static bool IsActorChildrenType(this TypeDefinition typeDefinition)
    {
        var type = typeDefinition;

        while (type != null && (
                   // ReSharper disable once MergeIntoLogicalPattern
                   type.FullName != "UnrealSharp.UnrealEngine.UObject" ||
                   type.FullName != "UnrealSharp.UnrealEngine.Bindings.Placeholders.UObject" ||
                   type.FullName != "System.Object"
               ))
        {
            // ReSharper disable once MergeIntoLogicalPattern
            if (type.FullName == "UnrealSharp.UnrealEngine.AActor" ||
                type.FullName == "UnrealSharp.UnrealEngine.Bindings.Placeholders.AActor"
               )
            {
                return true;
            }

            type = type.BaseType?.Resolve();
        }

        return false;
    }

    /// <summary>
    /// Determines whether [is function library] [the specified type definition].
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <returns>bool.</returns>
    public static bool IsFunctionLibrary(this TypeDefinition typeDefinition)
    {
        var type = typeDefinition;

        while (type != null && (
                   // ReSharper disable twice MergeIntoLogicalPattern
                   type.FullName != "UnrealSharp.UnrealEngine.UObject" ||
                   type.FullName != "UnrealSharp.UnrealEngine.Bindings.Placeholders.UObject" ||
                   type.FullName != "System.Object"
               ))
        {
            if (type.FullName == "UnrealSharp.UnrealEngine.UBlueprintFunctionLibrary" ||
                type.FullName == "UnrealSharp.UnrealEngine.Bindings.Placeholders.UBlueprintFunctionLibrary")
            {
                return true;
            }

            type = type.BaseType?.Resolve();
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified member reference is static.
    /// </summary>
    /// <param name="memberReference">The member reference.</param>
    /// <returns>bool.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public static bool IsStatic(this MemberReference memberReference)
    {
        switch (memberReference)
        {
            case Mono.Cecil.PropertyDefinition property:
            {
                var isStatic = property.GetMethod is { IsStatic: true } ||
                               property.SetMethod is { IsStatic: true };

                return isStatic;
            }
            case FieldDefinition field:
                return field.IsStatic;
            case MethodDefinition method:
                return method.IsStatic;
            default:
                throw new NotImplementedException();
        }
    }
    #endregion

    #region Default Value Text
    /// <summary>
    /// Gets the attribute default value text.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>string?.</returns>
    public static string? GetAttributeDefaultValueText(this FieldDefinition field)
    {
        var defaultValueAttribute = field.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == typeof(DefaultValueTextAttribute).FullName);

        if(defaultValueAttribute != null && defaultValueAttribute.ConstructorArguments.Count > 0)
        {
            return defaultValueAttribute.ConstructorArguments[0].Value.ToString();
        }

        return "";
    }

    /// <summary>
    /// Gets the attribute default value text.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>string?.</returns>
    public static string? GetAttributeDefaultValueText(this Mono.Cecil.PropertyDefinition property)
    {
        var defaultValueAttribute = property.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == typeof(DefaultValueTextAttribute).FullName);

        if (defaultValueAttribute != null && defaultValueAttribute.ConstructorArguments.Count > 0)
        {
            return defaultValueAttribute.ConstructorArguments[0].Value.ToString();
        }

        return "";
    }

    public static string ConvertEnumValueToEnumString(TypeDefinition enumTypeDefinition, long value)
    {
        var isFlags = enumTypeDefinition.HasCustomAttributes && enumTypeDefinition.CustomAttributes.Any(ca => ca.AttributeType.FullName == typeof(FlagsAttribute).FullName);

        var names = new List<string>();

        foreach (var field in enumTypeDefinition.Fields)
        {
            // Skip special fields like 'value__'
            if (field.IsRuntimeSpecialName)
                continue;

            // Get the value of the field
            var constantValue = Convert.ToInt64(field.Constant);

            if (isFlags)
            {
                // For [Flags] enums, check if the flag is part of the value
                if ((value & constantValue) == constantValue)
                {
                    if(constantValue != 0)
                    {
                        names.Add(field.Name);
                    }                        
                }
            }
            else
            {
                // For regular enums, check if the value matches the field
                if (value == constantValue)
                {
                    return field.Name;
                }
            }
        }

        // unreal does not support BitFlags combine default value.
        // so disable it
        return names.Count is <= 0 or > 1 ? "" : names.FirstOrDefault() ?? "";
    }
    #endregion
}