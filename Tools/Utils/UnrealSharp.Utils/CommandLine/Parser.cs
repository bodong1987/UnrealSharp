using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;

namespace UnrealSharp.Utils.CommandLine
{
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
        public readonly ParserSettings Settings;

        private static Dictionary<Type, TypeInfo> TypeInfos = new Dictionary<Type, TypeInfo>();

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
        /// <param name="defaultObject">The default object.</param>
        /// <returns>TypeInfo.</returns>
        public static TypeInfo? GetTypeInfo(object target, object? defaultObject)
        {
            if(target == null)
            {
                return null;
            }

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
            return Parse<T>(arguments, new T());
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
            return Parse<T>(Splitter.SplitCommandLineIntoArguments(arguments), value);
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
            ParserResult<T> result = new ParserResult<T>(GetTypeInfo(value, null));
            result.Result = ParserResultType.NotParsed;
            result.Value = value;

            IParserResult r = result;
            Parse(arguments, value, result.Type, ref r);

            return result;
        }

        #region Prase Details
        private void Parse(IEnumerable<string> arguments, object value, TypeInfo? typeInfo, ref IParserResult result)
        {
            int startPos = 0;
            string? key;
            List<string>? values = null;

            while(startPos != -1 && GetRange(arguments.ToList(), startPos, out key, out values, out startPos))
            {
                int i1 = key!.IndexOf('=');
                int i2 = key!.IndexOf('"');

                if(i1 != -1)
                {
                    // contain = 
                    if(i2 == -1 || i1 < i2)
                    {
                        string key2 = key.Substring(0, i1).Trim();
                        string value2 = key.Substring(i1 + 1).Trim();
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

        private bool GetRange(List<string> args, int startPos, out string? name, out List<string>? values, out int pos)
        {
            name = null;
            values = null;
            pos = startPos;

            int KeyPos = -1;
            for(int i=startPos; i<args.Count; ++i)
            {
                string value = args[i];

                if(value.StartsWith("-") || value.StartsWith("--"))
                {
                    if(KeyPos == -1)
                    {
                        KeyPos = i;
                        name = value;
                    }
                    else
                    {
                        pos = i;
                        values = args.GetRange(startPos + 1, i - startPos-1);

                        return KeyPos != -1;
                    }
                }
            }

            if(KeyPos != -1)
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

            string opName = name.StartsWith("--") ? name.Substring(2) : name.Substring(1);

            ReflectedPropertyInfo? info = null;
            
            if(opName.Length == 1)
            {
                info = typeInfo.FindShortProperty(opName, !Settings.CaseSensitive);
            }

            if(info == null)
            {
                info = typeInfo.FindLongProperty(opName, !Settings.CaseSensitive);
            }

            if(info == null)
            {
                if (!Settings.IgnoreUnknownArguments)
                {
                    result.AppendError($"Unknown property:{name}");
                    return false;
                }
                else
                {
                    return true;
                }
            }

            object? loadedValue = GetValues(value, typeInfo, info, values, ref result);

            if(loadedValue == null)
            {
                result.AppendError($"Failed convert value for property:{name}");
                return false;
            }

            info.SetValue(value, loadedValue);

            return true;
        }

        private object? GetValues(object value, TypeInfo? typeInfo, ReflectedPropertyInfo property, List<string> values, ref IParserResult result)
        {
            if(property.IsArray)
            {
                var list = Activator.CreateInstance(property.Type) as IList;

                Logger.Assert(list != null);

                if(list == null)
                {
                    return null;
                }
                
                foreach(var i in values)
                {
                    var obj = GetValue(property.GetElementType()!, i);
                    if(obj != null)
                    {
                        list.Add(obj);
                    }
                }

                return list;
            }         
            else if(property.IsFlags)
            {                
                string valueText = string.Join(", ", values.Select(x=>x.Trim()).ToArray());

                object? v;
                if(Enum.TryParse(property.Type, valueText, !Settings.CaseSensitive, out v))
                {
                    return Convert.ChangeType(v, property.Type);
                }

                result.AppendError($"Failed convert \"{valueText}\" to Enum {property.Type}");

                return null;
            }
            else if(property.Type == typeof(bool) && values.Count ==0)
            {
                // if type is bool
                // have flag means true
                // --param=True is same with --param
                return true;
            }
            
            if(values.Count <=0)
            {
                return ObjectCreator.Create(property.Type);
            }

            return GetValue(property.Type, values.FirstOrDefault()!);
        }

        private object? GetValue(Type type, string str)
        {
            str = TrimQuotation(str);
            if(type.IsEnum)
            {
                object? v;
                if(Enum.TryParse(type, str.Trim(), !Settings.CaseSensitive, out v))
                {
                    return v;
                }

                return null;
            }
            else if (type == typeof(bool))
            {
                return str.Trim().iEquals("true");
            }
            else if(type.IsNumericType())
            {
                return Convert.ChangeType(str.Trim(), type);
            }

            return Convert.ChangeType(str, type);
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
        public readonly static Parser Default = new Parser();

        #region Format Object To Command Line 
        /// <summary>
        /// Formats the command line.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="method">The method.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static string FormatCommandLine(object target, CommandLineFormatMethod method, object? defaultValue = null)
        {
            if(target == null)
            {
                return "";
            }

            var typeInfo = GetTypeInfo(target, defaultValue);

            if(typeInfo == null)
            {
                return "";
            }

            StringBuilder stringBuilder = new StringBuilder();
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

                string valueText = ToArguments(arguments);

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
            else if(propertyInfo.IsFlags)
            {
                return (value as Enum)!.GetUniqueFlags().Select(x => x.ToString()).ToArray();
            }

            return new string[] { value.ToString()! };
        }

        /// <summary>
        /// Formats the command line arguments.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="method">The method.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>System.String[].</returns>
        public static string[] FormatCommandLineArgs(object target, CommandLineFormatMethod method, object? defaultValue = null)
        {
            var text = FormatCommandLine(target, method, defaultValue);

            if(text.IsNullOrEmpty())
            {
                return new string[]{ };
            }

            return Splitter.SplitCommandLineIntoArguments(text).ToArray();
        }
        #endregion

        #region Help Text Generator
        /// <summary>
        /// The default indent
        /// </summary>
        public static readonly int DefaultIndent = 4;

        /// <summary>
        /// The default blank
        /// </summary>
        public static readonly int DefaultBlank = 43;

        /// <summary>
        /// Gets the help text.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="formatter">The formatter.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static string GetHelpText(object target, IFormatter? formatter = null)
        {
            if(target == null)
            {
                return string.Empty;
            }

            if(formatter == null)
            {
                formatter = new Formatter(DefaultIndent, DefaultBlank);
            }

            Logger.Assert(formatter != null);

            var typeInfo = GetTypeInfo(target, null);

            if(typeInfo == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();

            foreach(var property in typeInfo.Properties)
            {
                string name = property.Attribute.ShortName.IsNotNullOrEmpty() ? $"-{property.Attribute.ShortName}, --{property.Attribute.LongName}" : $"--{property.Attribute.LongName}";

                string attribute = GetAttribute(property);
                string usage = GenUsageHelp(property);

                formatter!.Append(builder, name, attribute, property.Attribute.HelpText!, usage);
            }

            return builder.ToString();
        }

        private static string GetAttribute(ReflectedPropertyInfo property)
        {
            List<string> list = new List<string>();
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
            else if(property.Type.IsEnum)
            {
                var names = Enum.GetNames(property.Property.PropertyType);
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < names.Length; i++)
                {                    
                    builder.Append(names[i]);
                    builder.Append(" ");
                }

                return $"--{property.Attribute.LongName} {builder.ToString()}";
            }
            // array support
            else if(property.IsArray)
            {
                return $"--{property.Attribute.LongName} {property.GetElementType()?.Name}1 {property.GetElementType()?.Name}2";
            }

            return "";
        }
        #endregion

        #endregion
    }
}
