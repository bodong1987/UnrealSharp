using Mono.Cecil;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;
using UnrealSharpTool.Core.ErrorReports;

namespace UnrealSharpTool.Core.TypeInfo.MonoCecil;

/// <summary>
/// Class MonoTypeDefinitionFactory.
/// </summary>
internal static class MonoTypeDefinitionFactory
{
    #region Main Entry
    /// <summary>
    /// Creates the type definition.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="validator">The validator.</param>
    /// <returns>UnrealSharpTool.Core.TypeInfo.BaseTypeDefinition?.</returns>
    public static BaseTypeDefinition? CreateTypeDefinition(TypeDefinition typeDefinition, IMonoTypeValidator validator)
    {   
        if (typeDefinition.IsEnum)
        {
            return SetupEnumTypeDefinition(new EnumTypeDefinition(), typeDefinition, validator);
        }

        if (typeDefinition.IsStructType())
        {
            return SetupScriptStructTypeDefinition(new ScriptStructTypeDefinition(), typeDefinition, validator);
        }

        if (typeDefinition.IsInterface || typeDefinition.IsClass)
        {
            return SetupClassTypeDefinition(new ClassTypeDefinition(), typeDefinition, validator);
        }

        return null;
    }
    #endregion

    #region Base Type Definition

    /// <summary>
    /// Setups the base type definition.
    /// </summary>
    /// <param name="baseTypeDefinition">The base type definition.</param>
    /// <param name="typeDefinition">The type definition.</param>
    public static void SetupBaseTypeDefinition(BaseTypeDefinition baseTypeDefinition, TypeDefinition typeDefinition)
    {
        baseTypeDefinition.CppName = typeDefinition.Name;
        baseTypeDefinition.Name = baseTypeDefinition.ConvertCppNameToScriptName(baseTypeDefinition.CppName);

        baseTypeDefinition.Namespace = typeDefinition.GetDefaultExportNamespace();
        baseTypeDefinition.PathName = $"/Script/{typeDefinition.GetExportProjectTinyName()}.{baseTypeDefinition.Name}";
        baseTypeDefinition.ProjectName = typeDefinition.GetExportProjectName();
        baseTypeDefinition.PackageName = typeDefinition.Namespace["UnrealSharp.".Length..].Replace(".Bindings.Defs", "").Replace(".", "/");
        baseTypeDefinition.AssemblyName = typeDefinition.Module.Assembly.Name.Name + ".dll";

        baseTypeDefinition.CSharpFullName = typeDefinition.FullName;

        SetupMetaDefinition(baseTypeDefinition.Metas, typeDefinition);

        var mainAttr = typeDefinition.CustomAttributes.FirstOrDefault(x => x.IsCSharpImplementTypeAttribute());
        if (mainAttr != null)
        {
            if (mainAttr.ConstructorArguments.Count >= 1)
            {
                baseTypeDefinition.Flags = (ulong)Convert.ChangeType(mainAttr.ConstructorArguments[0].Value, typeof(ulong));
            }

            if (mainAttr.HasProperties)
            {
                var exportFlagsProperty = mainAttr.Properties.FirstOrDefault(x => x.Name == "ExportFlags");

                if (exportFlagsProperty.Argument.Value != null && exportFlagsProperty.Argument.Value.ToString().IsNotNullOrEmpty())
                {
                    if (int.TryParse(exportFlagsProperty.Argument.Value.ToString(), out var v))
                    {
                        baseTypeDefinition.ExportFlags = v;    
                    }
                }

                var guidProperty = mainAttr.Properties.FirstOrDefault(x => x.Name == "Guid");
                if(guidProperty.Argument.Value != null && guidProperty.Argument.Value.ToString().IsNotNullOrEmpty())
                {
                    baseTypeDefinition.Guid = guidProperty.Argument.Value.ToString();
                }
            }
        }

        if(baseTypeDefinition.Guid.IsNullOrEmpty() || baseTypeDefinition.Guid == "00000000-0000-0000-0000-000000000000")
        {
            baseTypeDefinition.Guid = PropertyDefinition.GenStableGuidString(typeDefinition.ToString());
        }
    }

    /// <summary>
    /// Loads the flags.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="customAttributes">The custom attributes.</param>
    /// <returns>T.</returns>
    public static T LoadFlags<T>(IEnumerable<CustomAttribute> customAttributes) where T : Enum
    {
        foreach (var customAttribute in customAttributes)
        {
            if (customAttribute.IsCSharpImplementTypeAttribute())
            {
                if (customAttribute.ConstructorArguments.Count > 0)
                {
                    var value = ulong.Parse(customAttribute.ConstructorArguments[0].Value.ToString()!);

                    return (T)Enum.ToObject(typeof(T), value);
                }

                break;
            }
        }

        return default!;
    }
    #endregion

    #region Enum Type Definition
    /// <summary>
    /// Setups the enum type definition.
    /// </summary>
    /// <param name="enumTypeDefinition">The enum type definition.</param>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="validator">The validator.</param>
    /// <returns>BaseTypeDefinition</returns>
    public static BaseTypeDefinition SetupEnumTypeDefinition(EnumTypeDefinition enumTypeDefinition, TypeDefinition typeDefinition, IMonoTypeValidator validator)
    {
        SetupBaseTypeDefinition(enumTypeDefinition, typeDefinition);

        if(!enumTypeDefinition.CppName!.StartsWith('E'))
        {
            ErrorReport.ReportError(
                ErrorCode.InvalidTypeName,
                $"Invalid enum Name : {enumTypeDefinition.CppName} : The name of the Unreal enumeration type defined in C# must start with 'E'",
                MonoTypeDefinitionSourceInfoBuilder.Build(typeDefinition, validator.DebugInformation!)
            );
        }

        enumTypeDefinition.Size = typeDefinition.GetEnumUnderlyingTypeSize();

        if (enumTypeDefinition.UnderlyingTypeSize != sizeof(byte))
        {
            ErrorReport.ReportError(
                ErrorCode.InvalidEnumSize,
                $"Invalid enum Size : {enumTypeDefinition.CppName} : Currently, the size of enumerations exported to Unreal can only be constrained to byte. Consider defining it as: public enum {enumTypeDefinition.CppName} : byte",
                MonoTypeDefinitionSourceInfoBuilder.Build(typeDefinition, validator.DebugInformation!)
            );
        }

        enumTypeDefinition.PathName = $"/Game/CSharpBlueprints/{typeDefinition.GetExportProjectTinyName()}/Enums/{enumTypeDefinition.Name}.{enumTypeDefinition.Name}";

        LoadEnumFields(enumTypeDefinition, typeDefinition, validator);

        return enumTypeDefinition;
    }

    /// <summary>
    /// Loads the enum fields.
    /// </summary>
    /// <param name="enumTypeDefinition">The enum type definition.</param>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="validator">The validator.</param>
    private static void LoadEnumFields(EnumTypeDefinition enumTypeDefinition, TypeDefinition typeDefinition, IMonoTypeValidator validator)
    {
        var isNativeBindingEnumType = typeDefinition.IsCSharpNativeBindingType();

        var index = 0;
        foreach (var field in typeDefinition.Fields)
        {
            if (!field.IsStatic)
            {
                continue;
            }

            var value = new EnumFieldDefinition
            {
                Name = field.Name,
                Value = field.Constant != null ? (long)Convert.ChangeType(field.Constant.ToString()!, typeof(long)) : 0
            };

            if(!isNativeBindingEnumType && index != value.Value)
            {
                ErrorReport.ReportError(
                    ErrorCode.UEnumSpecifyFieldValueIsNotSupported,
                    $"Invalid enum field {field}, UEnum does not support the value of the specified field. Please make sure to assign the value automatically in order.",
                    MonoTypeDefinitionSourceInfoBuilder.Build(typeDefinition, validator.DebugInformation!)
                );
            }

            enumTypeDefinition.Fields.Add(value);

            var localDefine = new MetaDefinition();                
            SetupMetaDefinition(localDefine, field.CustomAttributes);

            foreach (var m in localDefine.Metas)
            {
                enumTypeDefinition.Metas.SetMeta($"{value.Name}.{m.Key}", m.Value);
            }

            ++index;
        }
    }
    #endregion

    #region Struct

    /// <summary>
    /// Setups the structure type definition.
    /// </summary>
    /// <param name="structTypeDefinition">The structure type definition.</param>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="checkedReferences">The checked type definitions.</param>
    public static void SetupStructTypeDefinition(StructTypeDefinition structTypeDefinition,
        TypeDefinition typeDefinition, IMonoTypeValidator validator, HashSet<TypeReference> checkedReferences)
    {
        if(typeDefinition.HasGenericParameters)
        {
            ErrorReport.ReportError(
                ErrorCode.GenericUnrealTypeIsNotSupported,
                $"Invalid Type: {typeDefinition}, All Unreal types cannot be generic types",
                MonoTypeDefinitionSourceInfoBuilder.Build(typeDefinition, validator.DebugInformation!)
            );
        }

        SetupBaseTypeDefinition(structTypeDefinition, typeDefinition);

        LoadProperties(structTypeDefinition, typeDefinition, validator, checkedReferences);
    }

    /// <summary>
    /// Adds the dependency type definition.
    /// </summary>
    /// <param name="structTypeDefinition">The structure type definition.</param>
    /// <param name="typeReference">The type reference.</param>
    /// <param name="checkedReferences">The checked type definitions.</param>
    public static void AddDependencyTypeDefinition(StructTypeDefinition structTypeDefinition, TypeReference typeReference, HashSet<TypeReference> checkedReferences)
    {
        if (!checkedReferences.Add(typeReference))
        {
            return;
        }

        var typeDefinition = typeReference.Resolve();

        if (typeDefinition != null && (typeDefinition.IsCSharpImplementType()||typeDefinition.IsCSharpBindingDefinitionType()))
        {
            var @namespace = typeDefinition.GetDefaultExportNamespace();

            structTypeDefinition.AddDependencyNamespace(@namespace);
        }

        var elements = typeReference.GetElementTypes();

        foreach (var e in elements)
        {
            AddDependencyTypeDefinition(structTypeDefinition, e, checkedReferences);
        }
    }

    private static void CheckActorComponentAttributeLimitation(
        StructTypeDefinition structTypeDefinition,
        TypeDefinition typeDefinition,
        TypeReference propertyType,
        MemberReference memberType,
        IMonoTypeValidator validator,
        IEnumerable<CustomAttribute> customAttributes
    )
    {
        var propertyAttribute = customAttributes.FirstOrDefault(x => x.AttributeType.FullName == typeof(UPROPERTYAttribute).FullName);

        Logger.EnsureNotNull(propertyAttribute);

        var isActorComponent = propertyAttribute.Properties.FirstOrDefault(p => p.Name == "IsActorComponent").Argument.Value != null;
        var attachToComponentName = propertyAttribute.Properties.FirstOrDefault(p => p.Name == "AttachToComponentName").Argument.Value != null;
        var attachToSocketName = propertyAttribute.Properties.FirstOrDefault(p => p.Name == "AttachToSocketName").Argument.Value != null;

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if(isActorComponent && (
               propertyType.Resolve() == null || 
               !propertyType.Resolve().IsChildrenTypeOf(
                   ["UnrealSharp.UnrealEngine.UActorComponent", 
                       "UnrealSharp.UnrealEngine.Bindings.Placeholders.UActorComponent"]
               )
           )
          )
        {
            ErrorReport.ReportError(
                ErrorCode.InvalidPropertyAttribute,
                $"{memberType}, IsActorComponent can only be used on UActorComponent or its children types.",
                MonoTypeDefinitionSourceInfoBuilder.Build(memberType, validator.DebugInformation!)
            );
        }
        else if(isActorComponent && (attachToComponentName||attachToSocketName) && (
                    propertyType.Resolve() == null ||
                    !propertyType.Resolve().IsChildrenTypeOf(
                        ["UnrealSharp.UnrealEngine.USceneComponent",
                            "UnrealSharp.UnrealEngine.Bindings.Placeholders.USceneComponent"]
                    )
                ) 
               )
        {
            ErrorReport.ReportError(
                ErrorCode.InvalidPropertyAttribute,
                $"{memberType}, AttachToComponentName or AttachToSocketName can only be used on USceneComponent or its children types.",
                MonoTypeDefinitionSourceInfoBuilder.Build(memberType, validator.DebugInformation!)
            );
        }
        else if((isActorComponent || attachToComponentName || attachToSocketName) && (structTypeDefinition.IsStruct||!typeDefinition.IsActorChildrenType()))
        {
            ErrorReport.ReportError(
                ErrorCode.InvalidPropertyAttribute,
                $"{memberType}, IsActorComponent, AttachToComponentName and AttachToSocketName can only be used in AActor or its children classes",
                MonoTypeDefinitionSourceInfoBuilder.Build(memberType, validator.DebugInformation!)
            );
        }
    }

    private static void LoadPropertyInternal(
        StructTypeDefinition structTypeDefinition, 
        TypeDefinition typeDefinition, 
        TypeReference propertyType, 
        MemberReference memberType, 
        IMonoTypeValidator validator, 
        HashSet<TypeReference> checkedReferences, 
        MonoDefaultValueParser defaultValue, 
        IEnumerable<CustomAttribute> customAttributes
    )
    {
        var customAttributesList = customAttributes.ToList();
        var isFunctionLibrary = typeDefinition.IsFunctionLibrary();

        if(isFunctionLibrary)
        {
            ErrorReport.ReportError(
                ErrorCode.FunctionLibraryCannotAllowAnyProperties,
                $"{memberType}, The function library does not allow adding any properties",
                MonoTypeDefinitionSourceInfoBuilder.Build(memberType, validator.DebugInformation!)
            );
        }

        if (memberType.IsStatic())
        {
            ErrorReport.ReportError(
                ErrorCode.StaticUPropertyIsNotSupported,
                $"{memberType}, Static UProperty is not supported.",
                MonoTypeDefinitionSourceInfoBuilder.Build(memberType, validator.DebugInformation!)
            );
        }

        if (propertyType.IsNullableType())
        {
            ErrorReport.ReportError(
                ErrorCode.NullableValueTypeIsNotSupported,
                $"Invalid property: {memberType}. Nullable value types are not supported as exported properties.",
                MonoTypeDefinitionSourceInfoBuilder.Build(memberType, validator.DebugInformation!)
            );
        }

        if (propertyType.Name == "UClass")
        {
            ErrorReport.ReportError(
                ErrorCode.RawUClassPointerIsNotSupported,
                $"Invalid property: {memberType}. For performance reasons, UClass? As exported properties have been disabled, please consider using TSubclassOf<UObject> instead it.",
                MonoTypeDefinitionSourceInfoBuilder.Build(memberType, validator.DebugInformation!)
            );
        }

        if (propertyType.IsDelegateDefinition())
        {
            ErrorReport.ReportError(
                ErrorCode.SingleCastDelegateIsNotSupported,
                $"Invalid property: {memberType}. Single-cast delegate is not supported, please use TMulticastDelegate instead it.",
                MonoTypeDefinitionSourceInfoBuilder.Build(memberType, validator.DebugInformation!)
            );
        }

        if (!validator.IsSupportedType(propertyType, true))
        {
            ErrorReport.ReportError(
                ErrorCode.UnsupportedProperty,
                $"Unsupported property: {memberType}",
                MonoTypeDefinitionSourceInfoBuilder.Build(memberType, validator.DebugInformation!)
            );
        }

        CheckActorComponentAttributeLimitation(structTypeDefinition, typeDefinition, propertyType, memberType, validator, customAttributesList);

        if (typeDefinition.IsStructType())
        {
            if (propertyType.IsSoftObjectPtrDefinition())
            {
                ErrorReport.ReportError(
                    ErrorCode.SoftObjectPtrInStructIsNotSupported,
                    $"Unsupported property: {memberType}, TSoftObjectPtr<T> can only be used in UClass.",
                    MonoTypeDefinitionSourceInfoBuilder.Build(memberType, validator.DebugInformation!)
                );
            }

            if (propertyType.IsSoftClassPtrDefinition())
            {
                ErrorReport.ReportError(
                    ErrorCode.SoftClassPtrInStructIsNotSupported,
                    $"Unsupported property: {memberType}, TSoftClassPtr<T> can only be used in UClass.",
                    MonoTypeDefinitionSourceInfoBuilder.Build(memberType, validator.DebugInformation!)
                );
            }

            if (propertyType.IsKindOfDelegateDefinition())
            {
                ErrorReport.ReportError(
                    ErrorCode.DelegateInStructIsNotSupported,
                    $"Unsupported property: {memberType}, Delegate can only be used in UClass.",
                    MonoTypeDefinitionSourceInfoBuilder.Build(memberType, validator.DebugInformation!)
                );
            }
        }

        var bIsInterfaceDef = typeDefinition.IsInterface;

        if (bIsInterfaceDef || customAttributesList.FirstOrDefault(x => x.AttributeType.FullName == typeof(UPROPERTYAttribute).FullName) != null)
        {
            var propertyInfo = NewPropertyDefinitionInternal(structTypeDefinition, memberType, checkedReferences, validator);

            Logger.EnsureNotNull(propertyInfo.Parent);

            if (propertyInfo.DefaultValue.IsNullOrEmpty())
            {
                propertyInfo.DefaultValue = defaultValue.GetDefaultValueText(memberType);
            }

            structTypeDefinition.Properties.Add(propertyInfo);

            AddDependencyTypeDefinition(structTypeDefinition, propertyType, checkedReferences);
        }
    }

    /// <summary>
    /// Loads the properties.
    /// </summary>
    /// <param name="structTypeDefinition">The structure type definition.</param>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="checkedReferences">The checked type definitions.</param>
    public static void LoadProperties(StructTypeDefinition structTypeDefinition, TypeDefinition typeDefinition, IMonoTypeValidator validator, HashSet<TypeReference> checkedReferences)
    {
        var defaultValues = new MonoDefaultValueParser(typeDefinition);

        foreach (var property in typeDefinition.Properties)
        {
            LoadPropertyInternal(structTypeDefinition, typeDefinition, property.PropertyType, property, validator, checkedReferences, defaultValues, property.CustomAttributes);
        }

        foreach (var field in typeDefinition.Fields)
        {
            if (field.IsSpecialName || field.IsStatic || field.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == typeof(UPROPERTYAttribute).FullName) == null)
            {
                continue;
            }

            LoadPropertyInternal(structTypeDefinition, typeDefinition, field.FieldType, field, validator, checkedReferences, defaultValues, field.CustomAttributes);
        }
    }
    #endregion

    #region Script Struct
    /// <summary>
    /// Setups the script structure type definition.
    /// </summary>
    /// <param name="scriptStructTypeDefinition">The script structure type definition.</param>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="validator">The validator.</param>
    /// <returns>System.Nullable&lt;BaseTypeDefinition&gt;.</returns>
    public static BaseTypeDefinition SetupScriptStructTypeDefinition(ScriptStructTypeDefinition scriptStructTypeDefinition, TypeDefinition typeDefinition, IMonoTypeValidator validator)
    {
        var checkedReferences = new HashSet<TypeReference>();
        SetupStructTypeDefinition(scriptStructTypeDefinition, typeDefinition, validator, checkedReferences);

        if (!scriptStructTypeDefinition.CppName!.StartsWith('F'))
        {
            ErrorReport.ReportError(
                ErrorCode.InvalidTypeName,
                $"Invalid struct Name : {scriptStructTypeDefinition.CppName} : The name of the Unreal struct type defined in C# must start with 'F'",
                MonoTypeDefinitionSourceInfoBuilder.Build(typeDefinition, validator.DebugInformation!)
            );
        }

        scriptStructTypeDefinition.PathName = $"/Game/CSharpBlueprints/{typeDefinition.GetExportProjectTinyName()}/Structs/{scriptStructTypeDefinition.Name}.{scriptStructTypeDefinition.Name}";

        return scriptStructTypeDefinition;
    }
    #endregion

    #region Class
    /// <summary>
    /// Setups the class type definition.
    /// </summary>
    /// <param name="classTypeDefinition">The class type definition.</param>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="validator">The validator.</param>
    /// <returns>System.Nullable&lt;BaseTypeDefinition&gt;.</returns>
    /// <exception cref="System.Exception">Invalid definition code for{typeDefinition}, invalid parent interface/class.</exception>
    public static BaseTypeDefinition SetupClassTypeDefinition(ClassTypeDefinition classTypeDefinition, TypeDefinition typeDefinition, IMonoTypeValidator validator)
    {
        var checkedReferences = new HashSet<TypeReference>();

        SetupStructTypeDefinition(classTypeDefinition, typeDefinition, validator, checkedReferences);

        if(typeDefinition.IsActorChildrenType())
        {
            if (!classTypeDefinition.CppName!.StartsWith('A'))
            {
                ErrorReport.ReportError(
                    ErrorCode.InvalidTypeName,
                    $"Invalid Actor Type Name : {classTypeDefinition.CppName} : The name of the Unreal Actor class type defined in C# must start with 'A'",
                    MonoTypeDefinitionSourceInfoBuilder.Build(typeDefinition, validator.DebugInformation!)
                );
            }                
        }
        else
        {
            if (!classTypeDefinition.CppName!.StartsWith('U'))
            {
                ErrorReport.ReportError(
                    ErrorCode.InvalidTypeName,
                    $"Invalid Object Type Name : {classTypeDefinition.CppName} : The name of the Unreal Object class type defined in C# must start with 'U'",
                    MonoTypeDefinitionSourceInfoBuilder.Build(typeDefinition, validator.DebugInformation!)
                );
            }
        }

        if(typeDefinition is { IsAbstract: true, IsSealed: true })
        {
            ErrorReport.ReportError(
                ErrorCode.UClassCannotBeStatic,
                $"UClass {classTypeDefinition.CppName} can't be marked static, please remove it.",
                MonoTypeDefinitionSourceInfoBuilder.Build(typeDefinition, validator.DebugInformation!)
            );
        }
        else if(typeDefinition.IsAbstract)
        {
            ErrorReport.ReportError(
                ErrorCode.UClassCannotBeAbstract,
                $"UClass {classTypeDefinition.CppName} can't be marked abstract, If you need a Unreal abstract UClass, you can use the Attribute tag: [UCLASS(EClassFlags.Abstract)]",
                MonoTypeDefinitionSourceInfoBuilder.Build(typeDefinition, validator.DebugInformation!)
            );
        }
        else if(typeDefinition.IsSealed)
        {
            ErrorReport.ReportError(
                ErrorCode.UClassCannotBeSealed,
                $"UClass {classTypeDefinition.CppName} can't be marked sealed, please remove it.",
                MonoTypeDefinitionSourceInfoBuilder.Build(typeDefinition, validator.DebugInformation!)
            );
        }
            
        classTypeDefinition.PathName = $"/Game/CSharpBlueprints/{typeDefinition.GetExportProjectTinyName()}/Classes/{classTypeDefinition.Name}.{classTypeDefinition.Name}_C";

        if (typeDefinition.IsClass)
        {
            var baseType = typeDefinition.BaseType.Resolve();

            if (baseType == null || !baseType.IsUnrealType())
            {
                ErrorReport.ReportError(
                    ErrorCode.InvalidBaseClass,
                    "The base class of the exported type must be an explicit Unreal type, at least UObject rather than System.Object",
                    MonoTypeDefinitionSourceInfoBuilder.Build(typeDefinition, validator.DebugInformation!)
                );
            }
            classTypeDefinition.SuperName = typeDefinition.BaseType.Name;

            AddDependencyTypeDefinition(classTypeDefinition, typeDefinition.BaseType, checkedReferences);

            classTypeDefinition.IsNativeBindingSuperType = typeDefinition.BaseType.Resolve().IsCSharpBindingDefinitionType();
        }
        else
        {
            if (!typeDefinition.HasInterfaces)
            {
                classTypeDefinition.SuperName = "UObject";
            }
            else if (typeDefinition.Interfaces.First().InterfaceType.Resolve().IsUnrealType())
            {
                classTypeDefinition.SuperName = typeDefinition.Interfaces.First().InterfaceType.Name;

                AddDependencyTypeDefinition(classTypeDefinition, typeDefinition.Interfaces.First().InterfaceType, checkedReferences);

                classTypeDefinition.IsNativeBindingSuperType = typeDefinition.Interfaces.First().InterfaceType.Resolve().IsCSharpBindingDefinitionType();
            }
            else
            {
                throw new Exception($"Invalid definition code for{typeDefinition}, invalid parent interface/class.");
            }
        }

        // get config name
        if (classTypeDefinition.Metas.TryGetMeta("Config", out string? config))
        {
            classTypeDefinition.ConfigName = config;
            classTypeDefinition.Flags |= (ulong)EClassFlags.Config;
        }

        LoadFunctions(classTypeDefinition, typeDefinition, validator, checkedReferences);

        CheckClassReplicateUsingProperties(classTypeDefinition, typeDefinition, validator);

        return classTypeDefinition;
    }

    public static void CheckClassReplicateUsingProperties(ClassTypeDefinition classTypeDefinition, TypeDefinition typeDefinition, IMonoTypeValidator validator)
    {
        foreach (var property in classTypeDefinition.Properties)
        {
            if (property.Metas.TryGetMeta("ReplicatedUsing", out string? replicatedUsingName))
            {
                if (classTypeDefinition.Functions.Find(x => x.Name == replicatedUsingName) == null)
                {
                    var method = typeDefinition.Methods.FirstOrDefault(x => x.Name == replicatedUsingName);
                    TypeDefinitionSourceInfo info;

                    if (method != null)
                    {
                        info = MonoTypeDefinitionSourceInfoBuilder.Build(method, validator.DebugInformation!);
                    }
                    else
                    {
                        var field = typeDefinition.Fields.FirstOrDefault(x => x.Name == property.Name);
                        info = field != null ? MonoTypeDefinitionSourceInfoBuilder.Build(field, validator.DebugInformation!) : MonoTypeDefinitionSourceInfoBuilder.Build(typeDefinition.Properties.FirstOrDefault(x => x.Name == property.Name)!, validator.DebugInformation!);
                    }

                    // invalid implement
                    ErrorReport.ReportError(
                        ErrorCode.MissingUFunctionAttribute,
                        $"property {property.Name} specifies ReplicatedUsing, but the corresponding UFunction named {replicatedUsingName} does not exist. Please consider adding [UFUNCTION] attribute to function {replicatedUsingName}.",
                        info
                    );
                }
            }
        }
    }

    /// <summary>
    /// Loads the functions.
    /// </summary>
    /// <param name="classTypeDefinition">The class type definition.</param>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="checkedReferences">The checked type definitions.</param>
    public static void LoadFunctions(ClassTypeDefinition classTypeDefinition, TypeDefinition typeDefinition, IMonoTypeValidator validator, HashSet<TypeReference> checkedReferences)
    {
        var isFunctionLibrary = typeDefinition.IsFunctionLibrary();

        foreach (var method in typeDefinition.Methods.Where(method => !method.IsSpecialName).Where(method => method.IsUFunction()))
        {
            if(method.HasGenericParameters)
            {
                ErrorReport.ReportError(
                    ErrorCode.GenericUFunctionIsNotSupported,
                    $"{method}, generic UFunction is not supported.",
                    MonoTypeDefinitionSourceInfoBuilder.Build(method, validator.DebugInformation!)
                );
            }

            if(!isFunctionLibrary && method.IsStatic)
            {
                ErrorReport.ReportError(
                    ErrorCode.StaticUFunctionCanOnlyBeUsedInFunctionLibrary,
                    $"{method}, static UFunction can only be used in the children of UBlueprintFunctionLibrary",
                    MonoTypeDefinitionSourceInfoBuilder.Build(method, validator.DebugInformation!)
                );
            }

            var overloadMethods = typeDefinition.LookupUFunctions(method.Name);

            if(overloadMethods.Length > 1)
            {
                var overloadMethod = overloadMethods.First(x => x != method);

                ErrorReport.ReportError(ErrorCode.UFunctionOverloadIsNotSupported,
                    $"UFunction overload found:{Environment.NewLine}    " +
                    string.Join(Environment.NewLine + "    ",
                        overloadMethods.Select(x => x.ToString())
                    ),
                    MonoTypeDefinitionSourceInfoBuilder.Build(overloadMethod, validator.DebugInformation!)
                );
            }

            CheckFunctionAvailable(method, validator);

            if(isFunctionLibrary && !method.IsStatic)
            {
                ErrorReport.ReportError(ErrorCode.UFunctionInFunctionLibraryMustBeStatic,
                    $"UFunction {method.Name} in {classTypeDefinition.CppName} must be static function.",
                    MonoTypeDefinitionSourceInfoBuilder.Build(method, validator.DebugInformation!)
                );
            }
                
            var functionInfo = NewFunctionTypeDefinition(classTypeDefinition, method, validator, checkedReferences);

            classTypeDefinition.Functions.Add(functionInfo);

            foreach (var ns in functionInfo.DependNamespaces)
            {
                classTypeDefinition.AddDependencyNamespace(ns);
            }
        }
    }

    /// <summary>
    /// Checks the function available.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="validator">The validator.</param>
    /// <returns>ErrorCode.</returns>
    private static void CheckFunctionAvailable(MethodDefinition method, IMonoTypeValidator validator)
    {
        if (!method.IsUFunction())
        {
            ErrorReport.ReportError(ErrorCode.MissingUFunctionAttribute, $"Method {method} should add [UFUNCTION] or [UEVENT]", MonoTypeDefinitionSourceInfoBuilder.Build(method, validator.DebugInformation!));
        }

        if (method.ReturnType.FullName != typeof(void).FullName)
        {
            if (method.ReturnType.IsValueType && method.ReturnType.IsNullableType())
            {
                ErrorReport.ReportError(
                    ErrorCode.NullableValueTypeIsNotSupported,
                    $"Invalid Return type: {method.ReturnType}. Nullable value types are not supported as exported method return value.",
                    MonoTypeDefinitionSourceInfoBuilder.Build(method, validator.DebugInformation!)
                );
            }

            if (method.ReturnType.Name == "UClass")
            {
                ErrorReport.ReportError(
                    ErrorCode.RawUClassPointerIsNotSupported,
                    $"Invalid Return type: {method.ReturnType}. For performance reasons, UClass? As method return type have been disabled, please consider using TSubclassOf<UObject> instead it.",
                    MonoTypeDefinitionSourceInfoBuilder.Build(method, validator.DebugInformation!)
                );
            }

            if(!validator.IsSupportedType(method.ReturnType, true))
            {
                ErrorReport.ReportError(
                    ErrorCode.UnsupportedMethodReturnType,
                    $"Unsupported return type:{method.ReturnType} in {method}",
                    MonoTypeDefinitionSourceInfoBuilder.Build(method, validator.DebugInformation!)
                );
            }
        }

        foreach (var parameter in method.Parameters)
        {
            if (parameter.ParameterType.IsValueType && parameter.ParameterType.IsNullableType())
            {
                ErrorReport.ReportError(
                    ErrorCode.NullableValueTypeIsNotSupported,
                    $"Invalid method parameter: {parameter.ParameterType}. Nullable value types are not supported as exported method parameter.",
                    MonoTypeDefinitionSourceInfoBuilder.Build(method, validator.DebugInformation!)
                );
            }

            if (parameter.ParameterType.Name == "UClass")
            {
                ErrorReport.ReportError(
                    ErrorCode.RawUClassPointerIsNotSupported,
                    $"Invalid method parameter: {parameter.ParameterType}. For performance reasons, UClass? As method parameter have been disabled, please consider using TSubclassOf<UObject> instead it.",
                    MonoTypeDefinitionSourceInfoBuilder.Build(method, validator.DebugInformation!)
                );
            }

            if (!validator.IsSupportedType(parameter.ParameterType, true))
            {
                ErrorReport.ReportError(
                    ErrorCode.UnsupportedMethodParameter,
                    $"Unsupported parameter: {parameter.ParameterType} {parameter} in {method}",
                    MonoTypeDefinitionSourceInfoBuilder.Build(method, validator.DebugInformation!)
                );
            }
        }
    }
    #endregion

    #region Meta Definition
    /// <summary>
    /// Setups the meta definition.
    /// </summary>
    /// <param name="metaDefinition">The meta definition.</param>
    /// <param name="typeDefinition">The type definition.</param>
    public static void SetupMetaDefinition(MetaDefinition metaDefinition, TypeDefinition typeDefinition)
    {
        SetupMetaDefinition(metaDefinition, typeDefinition.CustomAttributes);
    }

    /// <summary>
    /// Setups the meta definition.
    /// </summary>
    /// <param name="metaDefinition">The meta definition.</param>
    /// <param name="customAttributes">The custom attributes.</param>
    public static void SetupMetaDefinition(MetaDefinition metaDefinition, IEnumerable<CustomAttribute> customAttributes)
    {
        foreach (var attr in customAttributes)
        {
            if (attr.AttributeType.FullName == typeof(UMETAAttribute).FullName && attr.HasConstructorArguments)
            {
                switch (attr.ConstructorArguments.Count)
                {
                    case 1:
                    {
                        var name = attr.ConstructorArguments[0].Value.ToString()!;

                        metaDefinition.SetMeta(name, "true");
                        break;
                    }
                    case 2:
                    {
                        var name = attr.ConstructorArguments[0].Value.ToString()!;
                        var value = attr.ConstructorArguments[1].Value.ToString()!;

                        metaDefinition.SetMeta(name, value);
                        break;
                    }
                }
            }
            else if (attr.AttributeType.FullName == typeof(ToolTipAttribute).FullName && attr.HasConstructorArguments)
            {
                if (attr.ConstructorArguments.Count == 1)
                {
                    var tooltip = attr.ConstructorArguments[0].Value.ToString()!;

                    metaDefinition.SetMeta(MetaConstants.ToolTip, tooltip);
                }
            }
            else if (attr.IsCSharpImplementTypeAttribute())
            {
                if (attr.HasProperties)
                {
                    foreach (var property in attr.Properties)
                    {
                        var name = property.Name;
                        var value = property.Argument.Value?.ToString();

                        metaDefinition.SetMeta(name, value ?? "");
                    }
                }
            }
        }
    }
    #endregion

    #region Properties
    /// <summary>
    /// Loads the extra flags.
    /// </summary>
    /// <param name="metas">The metas.</param>
    /// <returns>UInt64.</returns>
    private static ulong LoadExtraFlags(MetaDefinition metas)
    {
        ulong flags = 0;

        if (metas.HasMeta("IsReplicated"))
        {
            flags |= (ulong)EPropertyFlags.Net;
        }

        if (metas.HasMeta("ReplicatedUsing"))
        {
            flags |= (ulong)EPropertyFlags.Net;
            flags |= (ulong)EPropertyFlags.RepNotify;
        }

        return flags;
    }

    /// <summary>
    /// Caches the meta's class.
    /// </summary>
    /// <param name="propertyDefinition">The property definition.</param>
    /// <param name="type">The type.</param>
    private static void CacheMetaClass(PropertyDefinition propertyDefinition, TypeReference type)
    {
        if (type.FullName.Contains("TSubclassOf`1"))
        {
            if(type is ByReferenceType brt)
            {
                type = brt.ElementType;
            }

            propertyDefinition.MetaClass = (type as GenericInstanceType)!.GenericArguments[0].Name;
        }
    }

    /// <summary>
    /// Gets the type of the reference.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <returns>EReferenceType.</returns>
    public static EReferenceType GetReferenceType(TypeDefinition typeDefinition)
    {
        if (typeDefinition.IsCSharpBuiltInType())
        {
            return EReferenceType.BuiltInType;
        }

        return typeDefinition.IsCSharpNativeBindingType() ? EReferenceType.UnrealType : EReferenceType.UserType;
    }

    /// <summary>
    /// Gets the class path.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <returns>System.Nullable&lt;System.String&gt;.</returns>
    public static string? GetClassPath(TypeDefinition typeDefinition)
    {
        var attr = typeDefinition.CustomAttributes.FirstOrDefault(x => x.IsCSharpNativeBindingTypeAttribute());

        if (attr == null)
        {
            attr = typeDefinition.CustomAttributes.FirstOrDefault(x => x.IsCSharpBlueprintBindingTypeAttribute());

            return attr != null ? attr.ConstructorArguments[0].Value.ToString() : "";
        }

        return attr.ConstructorArguments.Count < 3 ? "" : attr.ConstructorArguments[2].Value.ToString();
    }


    /// <summary>
    /// Loads the inner properties.
    /// </summary>
    /// <param name="propertyDefinition">The property definition.</param>
    /// <param name="typeReference">The type reference.</param>
    /// <param name="guidSource">The unique identifier source.</param>
    /// <param name="checkedReferences">The checked type definitions.</param>
    /// <param name="validator">The validator.</param>
    private static void LoadInnerProperties(PropertyDefinition propertyDefinition, TypeReference typeReference, string guidSource, HashSet<TypeReference> checkedReferences, IMonoTypeValidator validator)
    {
        if (typeReference is ByReferenceType brt)
        {
            typeReference = brt.ElementType;
        }

        if (typeReference is GenericInstanceType type)
        {
            foreach (var genericArg in type.GenericArguments)
            {
                Logger.EnsureNotNull(propertyDefinition.Parent);
                var property = NewPropertyDefinition(
                    propertyDefinition.Parent,
                    genericArg,
                    genericArg.Name,
                    guidSource + genericArg.FullName, 
                    null, 
                    checkedReferences, 
                    validator
                    );
                
                propertyDefinition.InnerProperties.Add(property);
            }
        }
    }

    /// <summary>
    /// Loads the delegate signature function.
    /// </summary>
    /// <param name="propertyDefinition">The property definition.</param>
    /// <param name="typeReference">The type reference.</param>
    /// <param name="checkedReferences">The checked type definitions.</param>
    /// <param name="validator">The validator.</param>
    private static void LoadDelegateSignatureFunction(PropertyDefinition propertyDefinition, TypeReference typeReference, HashSet<TypeReference> checkedReferences, IMonoTypeValidator validator)
    {
        var delegateType = typeReference.GetDelegateSignatureType();

        Logger.EnsureNotNull(delegateType);

        var method = delegateType.Methods.ToList().Find(x => x.Name == "Invoke");
        Logger.EnsureNotNull(method, "this type is not a delegate type? {0}", delegateType);

        Logger.EnsureNotNull(propertyDefinition.Parent);

        propertyDefinition.SignatureFunction = NewFunctionTypeDefinition(propertyDefinition.Parent!, method, validator, checkedReferences);

        // change internal properties
        propertyDefinition.SignatureFunction.Name = $"{propertyDefinition.Name}__DelegateSignature";
        propertyDefinition.SignatureFunction.CSharpFullName = $"{propertyDefinition.Parent!.Namespace}.{propertyDefinition.Parent.CppName}.{propertyDefinition.SignatureFunction.Name}";
        propertyDefinition.SignatureFunction.PathName = $"{propertyDefinition.Parent!.PathName}.{propertyDefinition.SignatureFunction.Name}";
    }


    /// <summary>
    /// Creates new property definition.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="typeReference">The type reference.</param>
    /// <param name="name">The name.</param>
    /// <param name="guidSource">The unique identifier source.</param>
    /// <param name="customAttributes">The custom attributes.</param>
    /// <param name="checkedReferences">The checked type definitions.</param>
    /// <param name="validator">The validator.</param>
    /// <returns>PropertyDefinition.</returns>
    public static PropertyDefinition NewPropertyDefinition(StructTypeDefinition parent, TypeReference typeReference, string name, string guidSource, IEnumerable<CustomAttribute>? customAttributes, HashSet<TypeReference> checkedReferences, IMonoTypeValidator validator)        
    {
        var propertyDefinition = new PropertyDefinition
        {
            Parent = parent
        };

        Logger.Ensure<Exception>(typeReference.IsSupportedType(), $"Unsupported property:{typeReference}");

        var typeDefinition = typeReference.Resolve();

        Logger.EnsureNotNull(typeDefinition);

        propertyDefinition.TypeName = propertyDefinition.CppTypeName = typeReference.GetUnrealTypeName();
        propertyDefinition.TypeClass = typeReference.GetTypeClass();
        propertyDefinition.Size = typeDefinition.GetTypeSize();
        propertyDefinition.Name = name;

        propertyDefinition.ReferenceType = GetReferenceType(typeDefinition);
        propertyDefinition.ClassPath = GetClassPath(typeDefinition);

        propertyDefinition.Guid = PropertyDefinition.GenStableGuidString(guidSource);

        if (customAttributes != null)
        {
            var list = customAttributes.ToList();
                
            SetupMetaDefinition(propertyDefinition.Metas, list);

            propertyDefinition.Flags |= (ulong)LoadFlags<EPropertyFlags>(list);

            var defaultValueAttribute = list.FirstOrDefault(x => x.AttributeType.FullName == typeof(DefaultValueTextAttribute).FullName);

            if (defaultValueAttribute != null && defaultValueAttribute.ConstructorArguments.Count > 0)
            {
                propertyDefinition.DefaultValue = defaultValueAttribute.ConstructorArguments[0].Value.ToString();
            }
        }

        propertyDefinition.Flags |= LoadExtraFlags(propertyDefinition.Metas);

        propertyDefinition.ResetScriptType();
        CacheMetaClass(propertyDefinition, typeReference);

        if (!typeReference.IsKindOfDelegateDefinition())
        {
            LoadInnerProperties(propertyDefinition, typeReference, guidSource, checkedReferences, validator);
        }
        else
        {
            LoadDelegateSignatureFunction(propertyDefinition, typeReference, checkedReferences, validator);
        }

        return propertyDefinition;
    }

    /// <summary>
    /// Creates new property definition.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="property">The property.</param>
    /// <param name="checkedReferences">The checked type definitions.</param>
    /// <param name="validator">The validator.</param>
    /// <returns>PropertyDefinition.</returns>
    public static PropertyDefinition NewPropertyDefinition(StructTypeDefinition parent, Mono.Cecil.PropertyDefinition property, HashSet<TypeReference> checkedReferences, IMonoTypeValidator validator)
    {
        var propertyDefinition = NewPropertyDefinition(parent, property.PropertyType, property.Name, property.ToString(), property.CustomAttributes, checkedReferences, validator);

        var accessMark = property.GetMethod.Attributes & MethodAttributes.MemberAccessMask;

        switch (accessMark)
        {
            case MethodAttributes.Public:
                propertyDefinition.Flags |= (ulong)EPropertyFlags.NativeAccessSpecifierPublic;
                break;
            case MethodAttributes.Family:
            case MethodAttributes.FamORAssem:
            case MethodAttributes.Assembly:
                propertyDefinition.Flags |= (ulong)EPropertyFlags.Protected;
                propertyDefinition.Flags |= (ulong)EPropertyFlags.NativeAccessSpecifierProtected;
                break;
            default:
                propertyDefinition.Flags |= (ulong)EPropertyFlags.NativeAccessSpecifierPrivate;
                break;
        }

        return propertyDefinition;
    }

    /// <summary>
    /// Creates new property definition.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="fieldDefinition">The field definition.</param>
    /// <param name="checkedReferences">The checked type definitions.</param>
    /// <param name="validator">The validator.</param>
    /// <returns>PropertyDefinition.</returns>
    public static PropertyDefinition NewPropertyDefinition(StructTypeDefinition parent, FieldDefinition fieldDefinition, HashSet<TypeReference> checkedReferences, IMonoTypeValidator validator)
    {
        var propertyDefinition = NewPropertyDefinition(parent, fieldDefinition.FieldType, fieldDefinition.Name, fieldDefinition.ToString(), fieldDefinition.CustomAttributes, checkedReferences, validator);

        var accessMark = fieldDefinition.Attributes & FieldAttributes.FieldAccessMask;

        switch (accessMark)
        {
            case FieldAttributes.Public:
                propertyDefinition.Flags |= (ulong)EPropertyFlags.NativeAccessSpecifierPublic;
                break;
            case FieldAttributes.Family:
            case FieldAttributes.FamORAssem:
            case FieldAttributes.Assembly:
                propertyDefinition.Flags |= (ulong)EPropertyFlags.Protected;
                propertyDefinition.Flags |= (ulong)EPropertyFlags.NativeAccessSpecifierProtected;
                break;
            default:
                propertyDefinition.Flags |= (ulong)EPropertyFlags.NativeAccessSpecifierPrivate;
                break;
        }

        return propertyDefinition;
    }

    /// <summary>
    /// Creates new property definition.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="methodDefinition">The method definition.</param>
    /// <param name="checkedReferences">The checked type definitions.</param>
    /// <param name="validator">The validator.</param>
    /// <returns>PropertyDefinition.</returns>
    public static PropertyDefinition NewPropertyDefinition(StructTypeDefinition parent, MethodDefinition methodDefinition, HashSet<TypeReference> checkedReferences, IMonoTypeValidator validator)
    {
        var propertyDefinition = NewPropertyDefinition(parent, methodDefinition.ReturnType, "ReturnValue", methodDefinition.ToString(), null, checkedReferences, validator);

        propertyDefinition.Flags |= (ulong)EPropertyFlags.ReturnParm;

        return propertyDefinition;
    }

    /// <summary>
    /// Creates new property definition.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="parameterDefinition">The parameter definition.</param>
    /// <param name="checkedReferences">The checked type definitions.</param>
    /// <param name="validator">The validator.</param>
    /// <returns>PropertyDefinition.</returns>
    public static PropertyDefinition NewPropertyDefinition(StructTypeDefinition parent, ParameterDefinition parameterDefinition, HashSet<TypeReference> checkedReferences, IMonoTypeValidator validator)
    {
        var propertyDefinition = NewPropertyDefinition(parent, parameterDefinition.ParameterType, parameterDefinition.Name, parameterDefinition.Method + ":" + parameterDefinition, parameterDefinition.CustomAttributes, checkedReferences, validator);

        if (parameterDefinition.ParameterType.IsByReference)
        {
            propertyDefinition.Flags |= (ulong)EPropertyFlags.ReferenceParm;
            propertyDefinition.Flags |= (ulong)EPropertyFlags.OutParm;
        }

        return propertyDefinition;
    }

    /// <summary>
    /// Creates new property definition.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="memberReference">The member reference.</param>
    /// <param name="checkedReferences">The checked references.</param>
    /// <param name="validator">The validator.</param>
    /// <returns>PropertyDefinition.</returns>
    /// <exception cref="System.NotImplementedException"></exception>
    private static PropertyDefinition NewPropertyDefinitionInternal(StructTypeDefinition parent, MemberReference memberReference, HashSet<TypeReference> checkedReferences, IMonoTypeValidator validator)
    {
        return memberReference switch
        {
            Mono.Cecil.PropertyDefinition pd => NewPropertyDefinition(parent, pd, checkedReferences, validator),
            FieldDefinition fd => NewPropertyDefinition(parent, fd, checkedReferences, validator),
            _ => throw new NotImplementedException()
        };
    }
    #endregion

    #region Functions
    /// <summary>
    /// Creates new function type definition.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="method">The method.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="checkedReferences">The checked type definitions.</param>
    /// <returns>FunctionTypeDefinition.</returns>
    public static FunctionTypeDefinition NewFunctionTypeDefinition(StructTypeDefinition parent, MethodDefinition method, IMonoTypeValidator validator, HashSet<TypeReference> checkedReferences)
    {
        var functionTypeDefinition = new FunctionTypeDefinition
        {
            Parent = parent,
            Name = method.Name,
            CppName = method.Name,
            Namespace = parent.Namespace,
            PackageName = parent.PackageName,
            ProjectName = parent.ProjectName
        };

        functionTypeDefinition.PathName = $"{parent.PathName}:{functionTypeDefinition.Name}";
        functionTypeDefinition.CSharpFullName = method.FullName;

        foreach (var paramDefinition in method.Parameters)
        {
            var propertyDefinition = NewPropertyDefinition(functionTypeDefinition, paramDefinition, checkedReferences, validator);
            Logger.EnsureNotNull(propertyDefinition.Parent);

            functionTypeDefinition.Properties.Add(propertyDefinition);

            AddDependencyTypeDefinition(parent, paramDefinition.ParameterType, checkedReferences);
        }

        if (method.ReturnType.FullName != typeof(void).FullName)
        {
            // have return value
            var returnDefinition = NewPropertyDefinition(functionTypeDefinition, method, checkedReferences, validator);
            Logger.EnsureNotNull(returnDefinition.Parent);

            functionTypeDefinition.Properties.Add(returnDefinition);

            AddDependencyTypeDefinition(parent, method.ReturnType, checkedReferences);
        }

        functionTypeDefinition.Flags |= (uint)LoadFlags<EFunctionFlags>(method.CustomAttributes);

        if (method.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == typeof(UEVENTAttribute).FullName) != null)
        {
            functionTypeDefinition.Flags |= (uint)EFunctionFlags.BlueprintEvent | (uint)EFunctionFlags.Event;
        }

        if (method.IsPublic)
        {
            functionTypeDefinition.Flags |= (uint)EFunctionFlags.Public;
        }
        else if (method.IsFamily)
        {
            functionTypeDefinition.Flags |= (uint)EFunctionFlags.Protected;
        }
        else if (method.IsPrivate)
        {
            functionTypeDefinition.Flags |= (uint)EFunctionFlags.Private;
        }

        functionTypeDefinition.IsOverrideFunction = method is { IsVirtual: true, IsReuseSlot: true } || method.IsNewSlot;

        functionTypeDefinition.Signature = method.GetMethodSignature();

        if (functionTypeDefinition.IsEvent)
        {
            var implementationMethodName = method.Name + "_Implementation";

            var implementMethod = method.DeclaringType.Methods.FirstOrDefault(x => x.Name == implementationMethodName);

            if (implementMethod != null)
            {
                functionTypeDefinition.Signature = implementMethod.GetMethodSignature();
            }
        }

        SetupMetaDefinition(functionTypeDefinition.Metas, method.CustomAttributes);

        if (functionTypeDefinition.Metas.TryGetMeta("Replicates", out FunctionReplicateType replicates))
        {
            functionTypeDefinition.Flags |= (uint)replicates;

            if (functionTypeDefinition.Metas.TryGetMeta("IsReliable", out bool isReliable) && isReliable)
            {
                functionTypeDefinition.Flags |= (uint)EFunctionFlags.NetReliable;
            }
        }

        return functionTypeDefinition;
    }
    #endregion
}