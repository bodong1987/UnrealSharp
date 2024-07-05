using System.Collections;
using System.Text;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;

#if !NETSTANDARD2_1
using System.Diagnostics.CodeAnalysis;
#endif

namespace UnrealSharp.Utils.CommandLine;

/// <summary>
/// Enum CommandLineFormatMethod
/// How to get the command line
/// </summary>
public enum CommandLineFormatMethod
{
    /// <summary>
    /// The complete
    /// Output regardless of whether the parameter is the same as the default value
    /// </summary>
    Complete,

    /// <summary>
    /// The simplify
    /// Only parameters that differ from the default value are output
    /// </summary>
    Simplify
}

/// <summary>
/// Class Parser.
/// </summary>
public class Parser
{
    /// <summary>
    /// The settings
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public readonly ParserSettings Settings; 

    private static readonly Dictionary<Type, TypeInfo> TypeInfos = new();

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="Parser"/> class.
    /// </summary>
    public Parser()
    {
        Settings = new ParserSettings();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Parser"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public Parser(ParserSettings settings)
    {
        Settings = settings;
    }
    #endregion

    #region Parse Target
    /// <summary>
    /// Gets the type information.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>TypeInfo.</returns>
    public static TypeInfo GetTypeInfo(
#if !NETSTANDARD2_1
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        Type type)
    {
        if(TypeInfos.TryGetValue(type, out var info))
        {
            return info;
        }

        info = new TypeInfo(type);
        TypeInfos.Add(type, info);

        return info;
    }

    /// <summary>
    /// Gets the type information.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>TypeInfo.</returns>
    public static TypeInfo GetTypeInfo<
#if !NETSTANDARD2_1
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        T>()
    {
        return GetTypeInfo(typeof(T));
    }

    /// <summary>
    /// Gets the type information.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <returns>TypeInfo.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static TypeInfo GetTypeInfo(object target)
    {
        return GetTypeInfo(target.GetType());
    }

    /// <summary>
    /// Parses the specified arguments.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arguments">The arguments.</param>
    /// <returns>ParserResult&lt;T&gt;.</returns>
    public ParserResult<T> Parse<
#if !NETSTANDARD2_1
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        T>(string arguments) where T : class, new ()
    {
        return Parse<T>(Splitter.SplitCommandLineIntoArguments(arguments));
    }

    /// <summary>
    /// Parses the specified arguments.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arguments">The arguments.</param>
    /// <returns>ParserResult&lt;T&gt;.</returns>
    public ParserResult<T> Parse<
#if !NETSTANDARD2_1
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        T>(IEnumerable<string> arguments) where T : class, new ()
    {
        return Parse(arguments, new T());
    }

    /// <summary>
    /// Parses the specified arguments.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arguments">The arguments.</param>
    /// <param name="value">The value.</param>
    /// <returns>ParserResult&lt;T&gt;.</returns>
    public ParserResult<T> Parse<
#if !NETSTANDARD2_1
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        T>(string arguments, T value) where T : class
    {
        return Parse(Splitter.SplitCommandLineIntoArguments(arguments), value);
    }

    /// <summary>
    /// Parses the specified arguments.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arguments">The arguments.</param>
    /// <param name="value">The value.</param>
    /// <returns>ParserResult&lt;T&gt;.</returns>
    public ParserResult<T> Parse<
#if !NETSTANDARD2_1
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        T>(IEnumerable<string> arguments, T value) where T : class
    {
        var result = new ParserResult<T>(GetTypeInfo(value))
        {
            Result = ParserResultType.NotParsed,
            Value = value
        };

        IParserResult r = result;
        Parse(arguments, value, result.Type, ref r);

        return result;
    }

    #region Prase Details
    private void Parse(IEnumerable<string> arguments, object value, TypeInfo? typeInfo, ref IParserResult result)
    {
        var startPos = 0;

        var enumerable = arguments.ToList();
        while(startPos != -1 && GetRange(enumerable, startPos, out var key, out var values, out startPos))
        {
            var i1 = key!.IndexOf('=');
            var i2 = key.IndexOf('"');

            if(i1 != -1)
            {
                // contain = 
                if(i2 == -1 || i1 < i2)
                {
                    var key2 = key[..i1].Trim();
                    var value2 = key[(i1 + 1)..].Trim();
                    key = key2;
                    values!.Insert(0, value2);
                }                    
            }

            if(!ProcessValue(value, typeInfo, key, values!, ref result))
            {
                result.Result = ParserResultType.NotParsed;
                return;
            }
        }

        result.Result = ParserResultType.Parsed;
    }

    private static bool GetRange(List<string> args, int startPos, out string? name, out List<string>? values, out int pos)
    {
        name = null;
        values = null;
        pos = startPos;

        var keyPos = -1;
        for(var i=startPos; i<args.Count; ++i)
        {
            var value = args[i];

            if(value.StartsWith("-") || value.StartsWith("--"))
            {
                if(keyPos == -1)
                {
                    keyPos = i;
                    name = value;
                }
                else
                {
                    pos = i;
                    values = args.GetRange(startPos + 1, i - startPos-1);

                    return true;
                }
            }
        }

        if(keyPos != -1)
        {
            values = args.GetRange(startPos + 1, args.Count - startPos - 1);
            pos = args.Count;
        }

        return name.IsNotNullOrEmpty() && values != null;
    }

    private bool ProcessValue(object value, TypeInfo? typeInfo, string name, List<string> values, ref IParserResult result)
    {
        if(typeInfo == null)
        {
            return false;
        }

        var opName = name.StartsWith("--") ? name[2..] : name[1..];

        ReflectedPropertyInfo? info = null;
            
        if(opName.Length == 1)
        {
            info = typeInfo.FindShortProperty(opName, !Settings.CaseSensitive);
        }

        info ??= typeInfo.FindLongProperty(opName, !Settings.CaseSensitive);

        if(info == null)
        {
            if (!Settings.IgnoreUnknownArguments)
            {
                result.AppendError($"Unknown property:{name}");
                return false;
            }

            return true;
        }

        var loadedValue = GetValues(info, values, ref result);

        if(loadedValue == null)
        {
            result.AppendError($"Failed convert value for property:{name}");
            return false;
        }

        info.SetValue(value, loadedValue);

        return true;
    }

    private object? GetValues(ReflectedPropertyInfo property, List<string> values, ref IParserResult result)
    {
        if(property.IsArray)
        {
            var list = Activator.CreateInstance(property.Type) as IList;

            Logger.Assert(list != null);

            if(list == null)
            {
                return null;
            }
                
            foreach (var obj in values.Select(i => GetValue(property.GetElementType()!, i)).OfType<object>())
            {
                list.Add(obj);
            }

            return list;
        }

        if(property.IsFlags)
        {                
            var valueText = string.Join(", ", values.Select(x=>x.Trim()).ToArray());

            if(Enum.TryParse(property.Type, valueText, !Settings.CaseSensitive, out var v))
            {
                return Convert.ChangeType(v, property.Type);
            }

            result.AppendError($"Failed convert \"{valueText}\" to Enum {property.Type}");

            return null;
        }

        if(property.Type == typeof(bool) && values.Count ==0)
        {
            // if type is bool
            // have flag means true
            // --param=True is same with --param
            return true;
        }

        return values.Count <=0 ? ObjectCreator.Create(property.Type) : GetValue(property.Type, values.FirstOrDefault()!);
    }

    private object? GetValue(Type type, string str)
    {
        str = TrimQuotation(str);
        if(type.IsEnum)
        {
            return Enum.TryParse(type, str.Trim(), !Settings.CaseSensitive, out var v) ? v : null;
        }

        return type == typeof(bool) ? str.Trim().iEquals("true") : Convert.ChangeType(type.IsNumericType() ? str.Trim() : str, type);
    }

    private static string TrimQuotation(string text)
    {
        return text.Trim('"');
    }

    #endregion

    #endregion

    #region Static Entries
    /// <summary>
    /// The default
    /// </summary>
    public static readonly Parser Default = new();

    #region Format Object To Command Line 
    /// <summary>
    /// Formats the command line.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="method">The method.</param>
    /// <returns>System.String.</returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public static string FormatCommandLine(object target, CommandLineFormatMethod method)
    {
        var typeInfo = GetTypeInfo(target);
         
        var stringBuilder = new StringBuilder();
        foreach(var property in typeInfo.Properties)
        {
            var value = property.GetValue(target);

            if(value == null)
            {
                continue;
            }

            var arguments = GetValueString(property, value);

            if(arguments == null)
            {
                continue;
            }

            var valueText = ToArguments(arguments);

            if (method == CommandLineFormatMethod.Simplify && 
                !property.Attribute.Required && // if required = true, must set this command line
                typeInfo.DefaultObject != null &&
                ToArguments(GetValueString(property, property.GetValue(typeInfo.DefaultObject))) == valueText
               )
            {
                continue;
            }

            stringBuilder.Append($"--{property.Attribute.LongName} {valueText} ");
        }

        return stringBuilder.ToString();
    }

    private static string ToArguments(string[]? arguments)
    {
        if(arguments == null || arguments.Length == 0)
        {
            return "";
        }

        return string.Join(' ', arguments.Select(argument => argument.Contains(' ') ? $"\"{argument}\"" : argument)); 
    }

    private static string[]? GetValueString(ReflectedPropertyInfo propertyInfo, object? value)
    {
        if(value == null)
        {
            return null;
        }

        if(propertyInfo.IsArray)
        {
            return (value as IEnumerable)?.Select(x => x.ToString()!).ToArray();                
        }

        return propertyInfo.IsFlags ? (value as Enum)!.GetUniqueFlags().Select(x => x.ToString()).ToArray() : [value.ToString()!];
    }

    /// <summary>
    /// Formats the command line arguments.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="method">The method.</param>
    /// <returns>System.String[].</returns>
    public static string[] FormatCommandLineArgs(object target, CommandLineFormatMethod method)
    {
        var text = FormatCommandLine(target, method);

        return text.IsNullOrEmpty() ? [] : Splitter.SplitCommandLineIntoArguments(text).ToArray();
    }
    #endregion

    #region Help Text Generator

    /// <summary>
    /// The default indent
    /// </summary>
    public const int DefaultIndent = 4;

    /// <summary>
    /// The default blank
    /// </summary>
    public const int DefaultBlank = 43;

    /// <summary>
    /// Gets the help text.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="formatter">The formatter.</param>
    /// <returns>System.String.</returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public static string GetHelpText(object target, IFormatter? formatter = null)
    {
        formatter ??= new Formatter(DefaultIndent, DefaultBlank);

        var typeInfo = GetTypeInfo(target);

        var builder = new StringBuilder();

        foreach(var property in typeInfo.Properties)
        {
            var name = property.Attribute.ShortName.IsNotNullOrEmpty() ? $"-{property.Attribute.ShortName}, --{property.Attribute.LongName}" : $"--{property.Attribute.LongName}";

            var attribute = GetAttribute(property);
            var usage = GenUsageHelp(property);

            formatter.Append(builder, name, attribute, property.Attribute.HelpText, usage);
        }

        return builder.ToString();
    }

    private static string GetAttribute(ReflectedPropertyInfo property)
    {
        var list = new List<string>();
        if(!property.Attribute.Required)
        {
            list.Add("Optional");
        }

        if(property.IsArray)
        {
            list.Add("Array");
        }

        if(property.IsFlags)
        {
            list.Add("Flags");
        }
        else if(property.Type.IsEnum)
        {
            list.Add("Enum");
        }

        return string.Join(',', list.ToArray());
    }

    private static string GenUsageHelp(ReflectedPropertyInfo property)
    {
        // this is flags enum
        if(property.IsFlags)
        {
            var names = Enum.GetNames(property.Property.PropertyType);
                
            return $"--{property.Attribute.LongName} {string.Join(' ', names)}";
        }

        if(property.Type.IsEnum)
        {
            var names = Enum.GetNames(property.Property.PropertyType);
            var builder = new StringBuilder();
                
            foreach (var t in names)
            {
                builder.Append(t);
                builder.Append(' ');
            }

            return $"--{property.Attribute.LongName} {builder}";
        }

        // array support
        return property.IsArray ? $"--{property.Attribute.LongName} {property.GetElementType()?.Name}1 {property.GetElementType()?.Name}2" : "";
    }
    #endregion

    #endregion
}