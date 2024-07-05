using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

#if !NETSTANDARD2_1
using System.Diagnostics.CodeAnalysis;
#endif

namespace UnrealSharp.Utils.Extensions;

/// <summary>
/// Class TypeExtensions.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Gets any custom attributes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="memberInfo">The member information.</param>
    /// <param name="inherit">if set to <c>true</c> [inherit].</param>
    /// <returns>T.</returns>
    public static T? GetAnyCustomAttribute<T>(this MemberInfo memberInfo, bool inherit = true)
        where T : Attribute
    {
        var attrs = memberInfo.GetCustomAttributes(typeof(T), inherit);

        return attrs.Length > 0 ? attrs[0] as T : null;
    }

    /// <summary>
    /// Gets the custom attributes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="memberInfo">The member information.</param>
    /// <param name="inherit">if set to <c>true</c> [inherit].</param>
    /// <returns>T[].</returns>
    public static T[] GetCustomAttributes<T>(this MemberInfo memberInfo, bool inherit = true)
        where T : Attribute
    {
        var attrs = memberInfo.GetCustomAttributes(typeof(T), inherit);

        return attrs.Length > 0 ? attrs.Cast<T>().ToArray() : [];
    }

    /// <summary>
    /// check if this member info has defined an Attribute
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="memberInfo">The member information.</param>
    /// <param name="inherit">if set to <c>true</c> [inherit].</param>
    /// <returns><c>true</c> if the specified inherit is defined; otherwise, <c>false</c>.</returns>
    public static bool IsDefined<T>(this MemberInfo memberInfo, bool inherit = true)
        where T : Attribute
    {
        return memberInfo.IsDefined(typeof(T), inherit);
    }

    /// <summary>
    /// Gets the type of the underlying.
    /// </summary>
    /// <param name="memberInfo">The member information.</param>
    /// <returns>Type.</returns>
    /// <exception cref="System.ArgumentException">Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo</exception>
    public static Type GetUnderlyingType(this MemberInfo memberInfo)
    {
        switch (memberInfo.MemberType)
        {
            case MemberTypes.Event:
                return ((EventInfo)memberInfo).EventHandlerType!;
            case MemberTypes.Field:
                return ((FieldInfo)memberInfo).FieldType;
            case MemberTypes.Method:
                return ((MethodInfo)memberInfo).ReturnType;
            case MemberTypes.Property:
                return ((PropertyInfo)memberInfo).PropertyType;
            default:
                throw new ArgumentException
                (
                    "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                );
        }
    }

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    /// <param name="memberInfo">The member information.</param>
    /// <param name="target">The target.</param>
    /// <returns>System.Object.</returns>
    /// <exception cref="System.ArgumentException">Input MemberInfo must be if type FieldInfo, or PropertyInfo</exception>
    public static object? GetUnderlyingValue(this MemberInfo memberInfo, object target)
    {
        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (memberInfo.MemberType)
        {
            case MemberTypes.Field:
                return ((FieldInfo)memberInfo).GetValue(target);
            case MemberTypes.Property:
                return ((PropertyInfo)memberInfo).GetValue(target, null);
            default:
                throw new ArgumentException
                (
                    "Input MemberInfo must be if type FieldInfo, or PropertyInfo"
                );
        }
    }

    /// <summary>
    /// Sets the underlying value.
    /// </summary>
    /// <param name="memberInfo">The member information.</param>
    /// <param name="target">The target.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="System.ArgumentException">Input MemberInfo must be if type FieldInfo, or PropertyInfo</exception>
    public static void SetUnderlyingValue(this MemberInfo memberInfo, object? target, object? value)
    {
        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (memberInfo.MemberType)
        {
            case MemberTypes.Field:
                ((FieldInfo)memberInfo).SetValue(target, value);
                break;
            case MemberTypes.Property:
                ((PropertyInfo)memberInfo).SetValue(target, value, null);
                break;
            default:
                throw new ArgumentException
                (
                    "Input MemberInfo must be if type FieldInfo, or PropertyInfo"
                );
        }
    }

    /// <summary>
    /// Determines whether the specified information is public.
    /// </summary>
    /// <param name="info">The information.</param>
    /// <returns><c>true</c> if the specified information is public; otherwise, <c>false</c>.</returns>
    public static bool IsPublic(this MemberInfo info)
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (info.MemberType == MemberTypes.Field)
        {
            return (info as FieldInfo)!.IsPublic;
        }

        if (info.MemberType == MemberTypes.Property)
        {
            var getMethod = (info as PropertyInfo)!.GetGetMethod();
            var setMethod = (info as PropertyInfo)!.GetSetMethod();
            return getMethod != null && setMethod != null && getMethod.IsPublic;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified information is static.
    /// </summary>
    /// <param name="info">The information.</param>
    /// <returns><c>true</c> if the specified information is static; otherwise, <c>false</c>.</returns>
    public static bool IsStatic(this MemberInfo info)
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (info.MemberType == MemberTypes.Field)
        {
            return (info as FieldInfo)!.IsStatic;
        }

        if (info.MemberType == MemberTypes.Property)
        {
            var getMethod = (info as PropertyInfo)!.GetGetMethod();
            var setMethod = (info as PropertyInfo)!.GetSetMethod();
            return getMethod != null && setMethod != null && getMethod.IsStatic;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified information is constant.
    /// </summary>
    /// <param name="info">The information.</param>
    /// <returns><c>true</c> if the specified information is constant; otherwise, <c>false</c>.</returns>
    public static bool IsConstant(this MemberInfo info)
    {
        return info is FieldInfo { IsInitOnly: true };
    }

    /// <summary>
    /// check if this type implement form target interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type">The type.</param>
    /// <returns><c>true</c> if the specified type is implement; otherwise, <c>false</c>.</returns>
    public static bool IsImplementFrom<T>(
#if !NETSTANDARD2_1
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif      
        this Type type)
        where T : class
    {
        Debug.Assert(typeof(T).IsInterface);
        return type.GetInterfaces().Contains(typeof(T));
    }

    /// <summary>
    /// check if this type implement form target interface
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="interfaceType">Type of the interface.</param>
    /// <returns><c>true</c> if the specified interface type is implement; otherwise, <c>false</c>.</returns>
        
    public static bool IsImplementFrom(
#if !NETSTANDARD2_1
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif
        this Type type, Type interfaceType)
    {
        Debug.Assert(interfaceType.IsInterface);

        return type.GetInterfaces().Contains(interfaceType);
    }

    /// <summary>
    /// Determines whether [is child of] [the specified type].
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type">The type.</param>
    /// <returns><c>true</c> if [is child of] [the specified type]; otherwise, <c>false</c>.</returns>
    public static bool IsChildOf<T>(this Type type)
        where T : class
    {
        return typeof(T).IsAssignableFrom(type);
    }

    /// <summary>
    /// Determines whether [is child of] [the specified base type].
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="baseType">Type of the base.</param>
    /// <returns><c>true</c> if [is child of] [the specified base type]; otherwise, <c>false</c>.</returns>
    public static bool IsChildOf(this Type type, Type baseType)
    {
        return baseType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Gets the type of the converter.
    /// </summary>
    /// <param name="attribute">The input attribute.</param>
    /// <returns>System.Type.</returns>
    public static Type? GetConverterType(this TypeConverterAttribute attribute)
    {
        TryGetTypeByName(attribute.ConverterTypeName, out var result, AppDomain.CurrentDomain.GetAssemblies());
        return result;
    }

    /// <summary>
    /// Gets the type associated with the specified name.
    /// </summary>
    /// <param name="typeName">Full name of the type.</param>
    /// <param name="type">The type.</param>
    /// <param name="customAssemblies">Additional loaded assemblies (optional).</param>
    /// <returns>Returns <c>true</c> if the type was found; otherwise <c>false</c>.</returns>
#if !NETSTANDARD2_1
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCode")]        
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2057:Unrecognized value")]
#endif
    private static bool TryGetTypeByName(string typeName, out Type? type, params Assembly[] customAssemblies)
    {
        if (typeName.Contains("Version=")
            && !typeName.Contains('`'))
        {
            // remove full qualified assembly type name
            typeName = typeName[..typeName.IndexOf(',')];
        }

        type = Type.GetType(typeName) ?? GetTypeFromAssemblies(typeName, customAssemblies);

        // try get generic types
        if (type == null
            && typeName.Contains('`'))
        {
            var match = Regex.Match(typeName, @"(?<MainType>.+`(?<ParamCount>[0-9]+))\[(?<Types>.*)\]");

            if (match.Success)
            {
                var genericParameterCount = int.Parse(match.Groups["ParamCount"].Value);
                var genericDef = match.Groups["Types"].Value;
                var typeArgs = new List<string>(genericParameterCount);
                foreach (Match typeArgMatch in Regex.Matches(genericDef, @"\[(?<Type>.*?)\],?"))
                {
                    if (typeArgMatch.Success)
                    {
                        typeArgs.Add(typeArgMatch.Groups["Type"].Value.Trim());
                    }
                }

                var genericArgumentTypes = new Type?[typeArgs.Count];
                for (var genTypeIndex = 0; genTypeIndex < typeArgs.Count; genTypeIndex++)
                {
                    if (TryGetTypeByName(typeArgs[genTypeIndex], out var genericType, customAssemblies))
                    {
                        genericArgumentTypes[genTypeIndex] = genericType;
                    }
                    else
                    {
                        // cant find generic type
                        return false;
                    }
                }

                var genericTypeString = match.Groups["MainType"].Value;
                if (TryGetTypeByName(genericTypeString, out var genericMainType))
                {
                    // make generic type                        
                    type = genericMainType?.MakeGenericType(genericArgumentTypes!);
                }
            }
        }

        return type != null;
    }

    /// <summary>
    /// Gets the type from assemblies.
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    /// <param name="customAssemblies">The custom assemblies.</param>
    /// <returns>Type.</returns>
#if !NETSTANDARD2_1
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2057:Unrecognized value")]
#endif
    private static Type? GetTypeFromAssemblies(string typeName, params Assembly[] customAssemblies)
    {
        Type? type = null;

        if (customAssemblies.Length > 0)
        {
            foreach (var assembly in customAssemblies)
            {
                type = assembly.GetType(typeName);

                if (type != null)
                    return type;
            }
        }

        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in loadedAssemblies)
        {
            type = assembly.GetType(typeName);

            if (type != null)
                return type;
        }

        return type;
    }

    /// <summary>
    /// Gets the type.
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    /// <returns>Type.</returns>
#if !NETSTANDARD2_1
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2057:Unrecognized value")]
#endif
    public static Type? GetType(string typeName)
    {
        var type = Type.GetType(typeName);
        if (type != null)
        {
            return type;
        }
        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = a.GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether [is numeric type] [the specified type].
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns><c>true</c> if [is numeric type] [the specified type]; otherwise, <c>false</c>.</returns>
    public static bool IsNumericType(this Type type)
    {
        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Gets the unique flags.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="flags">The flags.</param>
    /// <returns>IEnumerable&lt;T&gt;.</returns>
#if !NETSTANDARD2_1
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCode")]
#endif
    public static IEnumerable<T> GetUniqueFlags<T>(this T flags)
        where T : Enum
    {
        // aot warning
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (Enum value in Enum.GetValues(flags.GetType()))
        {
            if (flags.HasFlag(value))
            {
                yield return (T)value;
            }
        }
    }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    /// <param name="propertyInfo">The property information.</param>
    /// <returns>System.String.</returns>
    public static string GetDisplayName(this PropertyInfo propertyInfo)
    {
        var attr = propertyInfo.GetAnyCustomAttribute<DisplayNameAttribute>();

        return attr?.DisplayName ?? propertyInfo.Name;
    }

    /// <summary>
    /// Determines whether [is read only] [the specified property information].
    /// </summary>
    /// <param name="propertyInfo">The property information.</param>
    /// <returns><c>true</c> if [is read only] [the specified property information]; otherwise, <c>false</c>.</returns>
    public static bool IsReadOnly(this PropertyInfo propertyInfo)
    {
        var attr = propertyInfo.GetAnyCustomAttribute<ReadOnlyAttribute>();

        return attr is { IsReadOnly: true };
    }

    /// <summary>
    /// Determines whether the specified property information is browsable.
    /// </summary>
    /// <param name="propertyInfo">The property information.</param>
    /// <returns><c>true</c> if the specified property information is browsable; otherwise, <c>false</c>.</returns>
    public static bool IsBrowsable(this PropertyInfo propertyInfo)
    {
        var attr = propertyInfo.GetAnyCustomAttribute<BrowsableAttribute>();

        return attr == null || attr.Browsable;
    }

    #region For ProeprtyDescriptor
    /// <summary>
    /// Determines whether the specified property descriptor is defined.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyDescriptor">The property descriptor.</param>
    /// <returns><c>true</c> if the specified property descriptor is defined; otherwise, <c>false</c>.</returns>
    public static bool IsDefined<T>(this PropertyDescriptor propertyDescriptor) where T : Attribute
    {
        return propertyDescriptor.Attributes.OfType<T>().Any();
    }

    /// <summary>
    /// Gets the custom attribute.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyDescriptor">The property descriptor.</param>
    /// <returns>T.</returns>
    public static T? GetCustomAttribute<T>(this PropertyDescriptor propertyDescriptor) where T : Attribute
    {
        foreach (var attr in propertyDescriptor.Attributes)
        {
            if (attr is T t)
            {
                return t;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the custom attributes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyDescriptor">The property descriptor.</param>
    /// <returns>T[].</returns>
    public static T[] GetCustomAttributes<T>(this PropertyDescriptor propertyDescriptor) where T : Attribute
    {
        var list = new List<T>();
        foreach (var attr in propertyDescriptor.Attributes)
        {
            if (attr is T t)
            {
                list.Add(t);
            }
        }

        return list.ToArray();
    }                
    #endregion

    /// <summary>
    /// Gets the type of the return.
    /// </summary>
    /// <param name="mi">The mi.</param>
    /// <returns>Type.</returns>
    public static Type? GetReturnType(this MethodBase mi)
    {
        return mi.IsConstructor ? mi.DeclaringType : ((MethodInfo)mi).ReturnType;
    }
}