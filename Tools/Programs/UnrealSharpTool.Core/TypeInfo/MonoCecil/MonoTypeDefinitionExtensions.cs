using Mono.Cecil;
using System.Reflection;
using System.Text;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharpTool.Core.TypeInfo.MonoCecil
{
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
        private static bool IsUnrealAttributeType(this TypeDefinition typeDefinition)
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

            TypeDefinition td = typeDefinition.Resolve();

            if (td != null && td.BaseType != null)
            {
                return td.BaseType.Resolve().IsUnrealAttributeType();
            }

            return false;
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
        /// Identify whether it is an Unreal type implemented on the C# side[include place holder]
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        public static bool IsUnrealType(this TypeDefinition? typeDefinition)
        {
            if (typeDefinition == null)
            {
                return false;
            }

            var attrs = typeDefinition.CustomAttributes.ToList();

            if (attrs.Find(x => x.IsCSharpBindingDefinitionTypeAttribute() || 
                x.IsCSharpNativeBindingTypeAttribute() || 
                x.IsCSharpImplementTypeAttribute() ||
                x.IsCSharpBlueprintBindingTypeAttribute() ||
                x.IsCSharpUnrealBuiltinTypeAttribute()
                ) != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Identify whether it is an Unreal type implemented on the C# side[no place holder]
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        public static bool IsCSharpImplementType(this TypeDefinition? typeDefinition)
        {
            return typeDefinition != null && typeDefinition.CustomAttributes.FirstOrDefault(
                x => x.AttributeType.Resolve().IsUnrealAttributeType()
                ) != null;
        }

        /// <summary>
        /// Identify whether it is an Unreal type implemented on the C# side[just place holder]
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <returns><c>true</c> if [is c sharp placeholder type] [the specified type definition]; otherwise, <c>false</c>.</returns>
        public static bool IsCSharpBindingDefinitionType(this TypeDefinition? typeDefinition)
        {
            return typeDefinition != null && typeDefinition.CustomAttributes.FirstOrDefault(
                x => x.IsCSharpBindingDefinitionTypeAttribute()
                ) != null;
        }

        /// <summary>
        /// Identify whether it is an Unreal type implemented on the C# side[just used to bind to Unreal native types, no self implement UFUNCTION/UPROPERTY...]
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        public static bool IsCSharpNativeBindingType(this TypeDefinition? typeDefinition)
        {
            return typeDefinition != null && typeDefinition.CustomAttributes.FirstOrDefault(
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
            return typeDefinition != null && typeDefinition.CustomAttributes.FirstOrDefault(
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
            return typeDefinition != null && typeDefinition.CustomAttributes.FirstOrDefault(
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
        public static bool IsDelegateDefinition(this TypeReference typeReference)
        {
            return (typeReference != null && typeReference.FullName.Contains("TDelegate`1")) ||
                (typeReference?.Resolve() != null && typeReference.Resolve().BaseType != null && typeReference.Resolve().BaseType.FullName.Contains("TDelegate`1"));
        }

        /// <summary>
        /// Check if a multicast delegate is defined?
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        public static bool IsMulticastDelegateDefinition(this TypeReference typeReference)
        {
            return (typeReference != null && typeReference.FullName.Contains("TMulticastDelegate`1")) ||
                (typeReference?.Resolve() != null && typeReference.Resolve().BaseType != null && typeReference.Resolve().BaseType!.FullName.Contains("TMulticastDelegate`1")); 
        }

        /// <summary>
        /// Determines whether [is soft object PTR definition] [the specified type reference].
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <returns><c>true</c> if [is soft object PTR definition] [the specified type reference]; otherwise, <c>false</c>.</returns>
        public static bool IsSoftObjectPtrDefinition(this TypeReference typeReference)
        {
            return typeReference != null && typeReference.FullName.Contains("TSoftObjectPtr`1");
        }

        /// <summary>
        /// Determines whether [is soft class PTR definition] [the specified type reference].
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <returns><c>true</c> if [is soft class PTR definition] [the specified type reference]; otherwise, <c>false</c>.</returns>
        public static bool IsSoftClassPtrDefinition(this TypeReference typeReference)
        {
            return typeReference != null && typeReference.FullName.Contains("TSoftClassPtr`1");
        }

        /// <summary>
        /// Extract signature information from delegate definition template
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <returns>TypeDefinition.</returns>
        public static TypeDefinition GetDelegateSignatureType(this TypeReference typeReference)
        {
            var git4 = typeReference as GenericInstanceType;

            if (git4 == null)
            {
                git4 = typeReference.Resolve().BaseType as GenericInstanceType;
            }

            Logger.EnsureNotNull(git4);

            var genericArgument = git4.GenericArguments[0];

            return genericArgument.Resolve();
        }
        #endregion

        #region Builtin Types
        private static HashSet<string?> BuiltInTypes = new HashSet<string?>()
        {
            typeof(bool).FullName!,
            typeof(sbyte).FullName!,
            typeof(byte).FullName!,
            typeof(short).FullName!,
            typeof(ushort).FullName!,
            typeof(int).FullName!,
            typeof(uint).FullName!,
            typeof(Int64).FullName!,
            typeof(UInt64).FullName!,
            typeof(float).FullName!,
            typeof(double).FullName!,
            typeof(string).FullName!,
        };

        private static HashSet<string?> BlueprintableBuiltInTypes = new HashSet<string?>()
        {
            typeof(bool).FullName!,          
            typeof(byte).FullName!,            
            typeof(int).FullName!,            
            typeof(Int64).FullName!,
            typeof(float).FullName!,
            typeof(double).FullName!,
            typeof(string).FullName!
        };

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

            var TypeFullName = typeReference.GetElementType().FullName;

            if (blueprintable)
            {
                if (BlueprintableBuiltInTypes.Contains(TypeFullName))
                {
                    return true;
                }
            }
            else
            {
                if (BuiltInTypes.Contains(TypeFullName))
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

                    return (genericArgument.Resolve().IsClass) && genericArgument.IsSupportedType();
                }
            }
            else if (typeReference.Name.Contains("TSoftClassPtr`1"))
            {
                if (typeReference is GenericInstanceType git && git.GenericArguments.Count == 1)
                {
                    var genericArgument = git.GenericArguments[0];

                    return (genericArgument.Resolve().IsClass) && genericArgument.IsSupportedType();
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
                var git4 = typeReference as GenericInstanceType; 

                if(git4 == null)
                {
                    git4 = typeReference.Resolve().BaseType as GenericInstanceType;
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
            HashSet<TypeReference> Result = new HashSet<TypeReference>();

            HashSet<TypeReference> processedReferences = new HashSet<TypeReference>();

            if(typeReference == null)
            {
                return [];
            }

            GetElementTypes(typeReference, Result, processedReferences);

            return Result;
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

            int index = typeDefinition.Namespace.IndexOf('.', "UnrealSharp.".Length);

            if(index == -1)
            {
                return typeDefinition.Namespace;
            }

            return typeDefinition.Namespace.Substring(0, index);
        }

        /// <summary>
        /// Gets the name of the export project tiny.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <returns>System.String.</returns>
        public static string GetExportProjectTinyName(this TypeDefinition typeDefinition)
        {
            var projectName = GetExportProjectName(typeDefinition);

            int index = projectName.LastIndexOf(".");
            if(index != -1)
            {
                return projectName.Substring(index + 1);
            }

            return projectName;
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
            else if (field.FieldType.FullName == typeof(short).FullName ||
                field.FieldType.FullName == typeof(ushort).FullName)
            {
                return sizeof(ushort);
            }
            else if (field.FieldType.FullName == typeof(int).FullName ||
                field.FieldType.FullName == typeof(uint).FullName)
            {
                return sizeof(uint);
            }
            else if (field.FieldType.FullName == typeof(Int64).FullName ||
                field.FieldType.FullName == typeof(UInt64).FullName)
            {
                return sizeof(UInt64);
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
            return typeDefinition.IsValueType && !typeDefinition.IsEnum;
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

            if(typeDefinition.IsInterface)
            {
                return 8;             
            }

            return 0;
        }

        /// <summary>
        /// Determines whether [is nullable type] [the specified type reference].
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <returns>bool.</returns>
        public static bool IsNullableType(this TypeReference typeReference)
        {
            if(typeReference is GenericInstanceType git)
            {
                return git.ElementType.FullName == typeof(System.Nullable<>).FullName;
            }

            return false;
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
                methodDefinition.ReturnType.IsSupportedType(); ;
        }

        /// <summary>
        /// Gets the method signature.
        /// This can usually be used for exact search methods
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>System.String.</returns>
        public static string GetMethodSignature(this MethodDefinition method)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"{method.DeclaringType.FullName}:{method.Name} (");
            foreach (var param in method.Parameters)
            {
                stringBuilder.Append(GetFriendlyName(param));

                if (param != method.Parameters.Last())
                {
                    stringBuilder.Append(",");
                }
            }

            stringBuilder.Append(")");

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
            List<MethodDefinition> methods = new List<MethodDefinition>();

            foreach (var method in typeDefinition.Methods)
            {
                if (method.Name == name)
                {
                    foreach (var attribute in method.CustomAttributes)
                    {
                        if (attribute.IsCSharpImplementMethodTypeAttribute())
                        {
                            methods.Add(method);
                            break;
                        }
                    }
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
            TypeReference typeReference = parameter.ParameterType;

            return GetFriendlyName(typeReference);
        }

        /// <summary>
        /// Gets the friendly name  for signature
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <returns>System.String.</returns>
        private static string GetFriendlyName(TypeReference typeReference)
        {
            bool bIsReference = false;
            if (typeReference is ByReferenceType brt)
            {
                bIsReference = true;
                typeReference = brt.ElementType;
            }

            string RefTag = bIsReference ? "&" : "";

            var TypeName = typeReference.GetElementType().Name;
            var TypeFullName = typeReference.GetElementType().FullName;

            if (TypeName.Contains("TArray`1") || TypeName.Contains("List`1") || TypeName.Contains("IList`1"))
            {
                GenericInstanceType? type = typeReference as GenericInstanceType;

                Logger.EnsureNotNull(type);

                return $"{TypeFullName}<{GetFriendlyName(type.GenericArguments[0])}>{RefTag}";
            }
            else if (TypeName.Contains("TSet`1") || TypeName.Contains("HashSet`1") || TypeName.Contains("ISet`1"))
            {
                GenericInstanceType? type = typeReference as GenericInstanceType;

                Logger.EnsureNotNull(type);

                return $"{TypeFullName}<{GetFriendlyName(type.GenericArguments[0])}>{RefTag}";
            }
            else if (TypeName.Contains("TMap`2") || TypeName.Contains("Dictionary`2") || TypeName.Contains("IDictionary`2"))
            {
                GenericInstanceType? type = typeReference as GenericInstanceType;

                Logger.EnsureNotNull(type);

                return $"{TypeFullName}<{GetFriendlyName(type.GenericArguments[0])}, {GetFriendlyName(type.GenericArguments[1])}>{RefTag}";
            }

            return $"{GetFriendlyName(typeReference.FullName)}{RefTag}";
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
            if (method.CustomAttributes.FirstOrDefault(x =>
                x.AttributeType.FullName == typeof(UFUNCTIONAttribute).FullName ||
                x.AttributeType.FullName == typeof(UEVENTAttribute).FullName)
                != null
                )
            {
                return true;
            }

            return false;
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
            var TypeName = typeReference.GetElementType().Name;
            var TypeFullName = typeReference.GetElementType().FullName;

            if(TypeFullName == typeof(Boolean).FullName)
            {
                return "BoolProperty";
            }
            else if(TypeFullName == typeof(sbyte).FullName)
            {
                return "Int8Property";
            }
            else if(TypeFullName == typeof(byte).FullName)
            {
                return "ByteProperty";
            }
            else if(TypeFullName == typeof(short).FullName)
            {
                return "Int16Property";
            }
            else if(TypeFullName == typeof(ushort).FullName)
            {
                return "UInt16Property";
            }
            else if (TypeFullName == typeof(int).FullName)
            {
                return "IntProperty";
            }
            else if (TypeFullName == typeof(uint).FullName)
            {
                return "UInt32Property";
            }
            else if (TypeFullName == typeof(Int64).FullName)
            {
                return "Int64Property";
            }
            else if (TypeFullName == typeof(UInt64).FullName)
            {
                return "UInt64Property";
            }
            else if (TypeFullName == typeof(float).FullName)
            {
                return "FloatProperty";
            }
            else if (TypeFullName == typeof(double).FullName)
            {
                return "DoubleProperty";
            }
            else if(TypeName == "FName")
            {
                return "NameProperty";
            }
            else if(TypeName == "FText")
            {
                return "TextProperty";
            }           
            else if(TypeName.Contains("TSubclassOf`1"))
            {
                // UClass? is not supported, so you need always use TSubclassOf<>, use TSubclass<UObject> instead UClass?
                return "ClassProperty";
            }
            else if(typeReference.IsDelegateDefinition())
            {
                return "DelegateProperty";
            }
            else if(typeReference.IsMulticastDelegateDefinition())
            {
                return "MulticastInlineDelegateProperty";
            }
            else if(TypeName.Contains("TArray`1") || TypeName.Contains("List`1") || TypeName.Contains("IList`1"))
            {
                return "ArrayProperty";
            }
            else if(TypeName.Contains("TSet`1") || TypeName.Contains("HashSet`1") || TypeName.Contains("ISet`1"))
            {
                return "SetProperty";
            }
            else if(TypeName.Contains("TMap`2") || TypeName.Contains("Dictionary`2") || TypeName.Contains("IDictionary`2"))
            {
                return "MapProperty";
            }
            else if(TypeName.Contains("TSoftObjectPtr`1"))
            {
                return "SoftObjectProperty";
            }
            else if(TypeName.Contains("TSoftClassPtr`1"))
            {
                return "SoftClassProperty";
            }
            else if(TypeFullName == typeof(string).FullName)
            {
                return "StrProperty";
            }
            else if(typeReference.Resolve().IsStructType())
            {
                return "StructProperty";
            }
            else if(typeReference.Resolve().IsEnum)
            {
                return "EnumProperty";
            }
            else if(typeReference.Resolve().IsInterface || typeReference.Resolve().IsClass)
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

            var TypeName = typeReference.GetElementType().Name;
            var TypeFullName = typeReference.GetElementType().FullName;

            if (TypeFullName == typeof(bool).FullName)
            {
                return "bool";
            }
            else if (TypeFullName == typeof(sbyte).FullName)
            {
                return "int8";
            }
            else if (TypeFullName == typeof(byte).FullName)
            {
                return "uint8";
            }
            else if (TypeFullName == typeof(short).FullName)
            {
                return "int16";
            }
            else if (TypeFullName == typeof(ushort).FullName)
            {
                return "uint16";
            }
            else if (TypeFullName == typeof(int).FullName)
            {
                return "int32";
            }
            else if (TypeFullName == typeof(uint).FullName)
            {
                return "uint32";
            }
            else if (TypeFullName == typeof(Int64).FullName)
            {
                return "int64";
            }
            else if (TypeFullName == typeof(UInt64).FullName)
            {
                return "uint64";
            }
            else if (TypeFullName == typeof(float).FullName)
            {
                return "float";
            }
            else if (TypeFullName == typeof(double).FullName)
            {
                return "double";
            }
            else if(TypeFullName == typeof(string).FullName)
            {
                return "FString";
            }            
            else if (TypeName == "UClass")
            {
                return "UClass*";
            }                        
            else if (TypeName.Contains("TSubclassOf`1"))
            {
                GenericInstanceType? type = typeReference as GenericInstanceType;

                Logger.EnsureNotNull(type);

                var argName = type.GenericArguments[0].GetUnrealTypeName();
                if(argName.EndsWith("*"))
                {
                    argName = argName.Substring(0, argName.Length - 1);
                }
                return $"TSubclassOf<{argName}>";
            }
            else if (TypeName.Contains("TArray`1") || TypeName.Contains("List`1") || TypeName.Contains("IList`1"))
            {
                GenericInstanceType? type = typeReference as GenericInstanceType;

                Logger.EnsureNotNull(type);

                return $"TArray<{type.GenericArguments[0].GetUnrealTypeName()}>";
            }
            else if (TypeName.Contains("TSet`1") || TypeName.Contains("HashSet`1") || TypeName.Contains("ISet`1"))
            {
                GenericInstanceType? type = typeReference as GenericInstanceType;

                Logger.EnsureNotNull(type);

                return $"TSet<{type.GenericArguments[0].GetUnrealTypeName()}>";
            }
            else if (TypeName.Contains("TMap`2") || TypeName.Contains("Dictionary`2") || TypeName.Contains("IDictionary`2"))
            {
                GenericInstanceType? type = typeReference as GenericInstanceType;

                Logger.EnsureNotNull(type);

                return $"TMap<{type.GenericArguments[0].GetUnrealTypeName()}, {type.GenericArguments[1].GetUnrealTypeName()}>";
            }
            else if (TypeName.Contains("TSoftObjectPtr`1"))
            {
                GenericInstanceType? type = typeReference as GenericInstanceType;

                Logger.EnsureNotNull(type);

                return $"TSoftObjectPtr<{type.GenericArguments[0].GetUnrealTypeName()}>";
            }
            else if (TypeName.Contains("TSoftClassPtr`1"))
            {
                GenericInstanceType? type = typeReference as GenericInstanceType;

                Logger.EnsureNotNull(type);

                return $"TSoftClassPtr<{type.GenericArguments[0].GetUnrealTypeName()}>";
            }
            else if (typeReference.Resolve().IsInterface)
            {
                return $"{TypeName}*";
            }

            return TypeName;
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
            TypeDefinition? type = typeDefinition;

            while (type != null && (
                type.FullName != "UnrealSharp.UnrealEngine.UObject" ||
                type.FullName != "UnrealSharp.UnrealEngine.Bindings.Placeholders.UObject" ||
                type.FullName != "System.Object"
                ))
            {
                if (fullNames.Contains(type.FullName))
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
            TypeDefinition? type = typeDefinition;

            while (type != null && (
                type.FullName != "UnrealSharp.UnrealEngine.UObject" ||
                type.FullName != "UnrealSharp.UnrealEngine.Bindings.Placeholders.UObject" ||
                type.FullName != "System.Object"
                ))
            {
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
            TypeDefinition? type = typeDefinition;

            while (type != null && (
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
            if(memberReference is Mono.Cecil.PropertyDefinition property)
            {
                bool isStatic = (property.GetMethod != null && property.GetMethod.IsStatic) ||
                                (property.SetMethod != null && property.SetMethod.IsStatic);

                return isStatic;
            }
            else if(memberReference is Mono.Cecil.FieldDefinition field)
            {
                return field.IsStatic;
            }
            else if(memberReference is Mono.Cecil.MethodDefinition method)
            {
                return method.IsStatic;
            }

            throw new NotImplementedException();
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

        public static string ConvertEnumValueToEnumString(Mono.Cecil.TypeDefinition enumTypeDefinition, Int64 value)
        {
            bool isFlags = enumTypeDefinition.HasCustomAttributes && enumTypeDefinition.CustomAttributes.Any(ca => ca.AttributeType.FullName == typeof(FlagsAttribute).FullName);

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
            if(names.Count <= 0 || names.Count > 1)
            {
                return "";
            }

            return names.FirstOrDefault()??"";
        }
        #endregion
    }
}
