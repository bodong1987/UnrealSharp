using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;

namespace UnrealSharp.Utils.CommandLine;

/// <summary>
/// Class TypeInfo.
/// </summary>
public class TypeInfo
{
    /// <summary>
    /// The type
    /// </summary>
#if !NETSTANDARD2_1
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    public readonly Type Type;

    /// <summary>
    /// The properties core
    /// </summary>
    private readonly List<ReflectedPropertyInfo> _propertiesCore = [];

    /// <summary>
    /// Gets the properties.
    /// </summary>
    /// <value>The properties.</value>
    public ReflectedPropertyInfo[] Properties => _propertiesCore.ToArray();

    /// <summary>
    /// Gets a value indicating whether this instance is command line object.
    /// </summary>
    /// <value><c>true</c> if this instance is command line object; otherwise, <c>false</c>.</value>
    public bool IsCommandLineObject => _propertiesCore.Count > 0;

    /// <summary>
    /// The default object
    /// </summary>
    public readonly object? DefaultObject;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeInfo"/> class.
    /// </summary>
    /// <param name="type">The type.</param>
    public TypeInfo(
#if !NETSTANDARD2_1
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        Type type)        
    {
        Type = type;

        Logger.Assert(!type.IsAbstract);

        try
        {
            DefaultObject = ObjectCreator.Create(type);
        }
        catch(Exception ex)
        {
            Logger.LogError("Failed create default for {0}, then this target will not support format command line in simplify mode.\n{1}", type, ex.Message);
        }

        ParseMeta();
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string? ToString()
    {
        return Type.FullName;
    }

    private void ParseMeta()
    {
        var properties = Type.GetProperties();

        // sort by meta data token
        foreach(var property in properties)
        {
            if(property.IsDefined<OptionAttribute>())
            {
                var info = new ReflectedPropertyInfo(property, property.GetCustomAttribute<OptionAttribute>()!);
                _propertiesCore.Add(info);
            }
        }
    }

    /// <summary>
    /// Finds the short property.
    /// </summary>
    /// <param name="shortName">The short name.</param>
    /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
    /// <returns>PropertyInfo.</returns>
    public ReflectedPropertyInfo? FindShortProperty(string shortName, bool ignoreCase)
    {
        return _propertiesCore.Find(x =>
        {
            if(x.Attribute.ShortName.IsNullOrEmpty())
            {
                return false;
            }

            return ignoreCase ? x.Attribute.ShortName.iEquals(shortName) : x.Attribute.ShortName!.Equals(shortName);
        });
    }

    /// <summary>
    /// Finds the long property.
    /// </summary>
    /// <param name="longName">The long name.</param>
    /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
    /// <returns>PropertyInfo.</returns>
    public ReflectedPropertyInfo? FindLongProperty(string longName, bool ignoreCase)
    {
        return _propertiesCore.Find(x =>
        {
            if (x.Attribute.LongName.IsNullOrEmpty())
            {
                return false;
            }

            return ignoreCase ? x.Attribute.LongName.iEquals(longName) : x.Attribute.LongName!.Equals(longName);
        });
    }
}

#region ReflectedPropertyInfo
/// <summary>    
/// Cache PropertyInfo and Options
/// </summary>
public class ReflectedPropertyInfo
{
    /// <summary>
    /// The property
    /// </summary>
    public readonly PropertyInfo Property;

    /// <summary>
    /// The attribute
    /// </summary>
    public readonly OptionAttribute Attribute;

    /// <summary>
    /// Gets the type.
    /// </summary>
    /// <value>The type.</value>
#if !NETSTANDARD2_1
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    public Type Type { get; }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return Attribute.ToString();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyInfo"/> class.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="attribute">The attribute.</param>
    [SuppressMessage("Trimming", "IL2072:Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.", Justification = "<挂起>")]
    public ReflectedPropertyInfo(PropertyInfo property, OptionAttribute attribute)
    {
        Property = property;
        Attribute = attribute;

        Type = Property.PropertyType;

        if(attribute.LongName.IsNullOrEmpty())
        {
            attribute.LongName = property.Name;
        }
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <returns>System.Object.</returns>
    public object? GetValue(object target)
    {
        return Property.GetValue(target);
    }

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="value">The value.</param>
    public void SetValue(object? target, object? value)
    {
        Property.SetValue(target, value);
    }


    /// <summary>
    /// Gets a value indicating whether this instance is array.
    /// </summary>
    /// <value><c>true</c> if this instance is array; otherwise, <c>false</c>.</value>
    public bool IsArray => Property.PropertyType.IsGenericType && Property.PropertyType.GetGenericTypeDefinition() == typeof(List<>);

    /// <summary>
    /// Gets a value indicating whether this instance is flags.
    /// </summary>
    /// <value><c>true</c> if this instance is flags; otherwise, <c>false</c>.</value>
    public bool IsFlags => Property.PropertyType.IsEnum && Property.PropertyType.IsDefined<FlagsAttribute>();

    /// <summary>
    /// Gets the type of the element.
    /// </summary>
    /// <returns>Type.</returns>
    public Type? GetElementType()
    {
        if(!IsArray)
        {
            return null;
        }

        return Property.PropertyType.IsGenericType
            ? Type.GetGenericArguments()[0]
            : Property.PropertyType.GetElementType();
    }
}
#endregion