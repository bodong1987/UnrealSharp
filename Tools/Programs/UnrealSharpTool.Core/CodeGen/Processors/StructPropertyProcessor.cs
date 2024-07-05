using System.Text;
using System.Text.RegularExpressions;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors;

/// <summary>
/// Class StructPropertyProcessor.
/// Implements the <see cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
[PropertyProcessPolicy]
internal partial class StructPropertyProcessor : PropertyProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StructPropertyProcessor"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public StructPropertyProcessor(BindingContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets the matched type class.
    /// </summary>
    /// <returns>System.String[].</returns>
    public override string[] GetMatchedTypeClass()
    {
        return [
            "StructProperty"
        ];
    }

    /// <summary>
    /// Gets the interop get value text.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="variable">The variable.</param>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>System.String.</returns>
    public override string GetInteropGetValueText(
        PropertyDefinition property,
        string variable,
        string address,
        string offset,
        ELocalUsageScenarioType usage)
    {
        return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}{property.GetCSharpTypeName(Context)}.FromNative({address}, {offset})";
    }

    /// <summary>
    /// Gets the interop set value text.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="variable">The variable.</param>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>System.String.</returns>
    public override string GetInteropSetValueText(
        PropertyDefinition property,
        string variable,
        string address,
        string offset,
        ELocalUsageScenarioType usage)
    {
        return $"{property.GetCSharpTypeName(Context)}.ToNative({address}, {offset}, ref {variable})";
    }

    /// <summary>
    /// Decorates the default value text.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="value">The value.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>System.Nullable&lt;System.String&gt;.</returns>
    public override string? DecorateDefaultValueText(PropertyDefinition property, string value, ELocalUsageScenarioType usage)
    {
        if(value == "" && !property.IsReference)
        {
            return "default";
        }

        if(usage.HasFlag(ELocalUsageScenarioType.Field))
        {
            if(property.CppTypeName == "FGuid")
            {
                return value == "00000000000000000000000000000000" ? null : $"new(\"{value}\")";
            }                
        }

        if(value.StartsWith('(') && value.EndsWith(')') && usage.HasFlag(ELocalUsageScenarioType.Field))
        {
            return GetStructConstructText(property, GetDefaultValuesInDefaultText(value));
        }

        return base.DecorateDefaultValueText(property, value, usage);
    }

    /// <summary>
    /// Gets the default values in default text.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>IEnumerable&lt;KeyValuePair&lt;System.String, System.String&gt;&gt;.</returns>
    public static IEnumerable<KeyValuePair<string, string>> GetDefaultValuesInDefaultText(string value)
    {
        var regex = GenUnrealDefaultValueTextMatchRegex();
        var matches = regex.Matches(value);

        var values = new List<KeyValuePair<string, string>>();

        foreach (Match match in matches)
        {
            var propertyName = match.Groups[1].Value;
            var propertyDefaultValue = match.Groups[3].Value;

            values.Add(new KeyValuePair<string, string> (propertyName, propertyDefaultValue));
        }

        return values;
    }

    /// <summary>
    /// Gets the structure construct text.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="values">The values.</param>
    /// <returns>System.String.</returns>
    public string GetStructConstructText(PropertyDefinition property, IEnumerable<KeyValuePair<string, string>> values)
    {
        var bAnyData = false;
        var bFirst = true;
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("new(){");
        foreach (var pair in values)
        {
            var propertyName = pair.Key;
            var propertyDefaultValue = pair.Value;

            if(propertyDefaultValue.StartsWith('(') && propertyDefaultValue.EndsWith(')'))
            {
                // Recursion is not supported
                // PropertyDefaultValue = GetStructConstructText("", GetDefaultValuesInDefaultText(PropertyDefaultValue));
                continue;
            }

            var propertyType = Context.FindType(property.CppTypeName!);

            if(propertyType is StructTypeDefinition propertyStructType)
            {
                var targetProperty = propertyStructType.Properties.Find(x => x.Name == propertyName);

                if (targetProperty != null)
                {
                    var processor = Context.GetProcessor(targetProperty);

                    Logger.EnsureNotNull(processor);

                    propertyDefaultValue = processor.DecorateDefaultValueText(targetProperty, propertyDefaultValue, ELocalUsageScenarioType.Struct | ELocalUsageScenarioType.Field);

                    if (propertyName.IsNotNullOrEmpty() && propertyDefaultValue.IsNotNullOrEmpty())
                    {
                        if (!bFirst)
                        {
                            stringBuilder.Append(", ");
                        }

                        stringBuilder.Append($"{propertyName} = {propertyDefaultValue}");

                        bFirst = false;
                        bAnyData = true;
                    }
                }
            }
        }

        if(!bAnyData)
        {
            return "";
        }

        stringBuilder.Append('}');

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Gets the type of the CPP function declaration.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>string.</returns>
    public override string GetCppFunctionDeclarationType(PropertyDefinition property, ELocalUsageScenarioType usage)
    {
        var constTag = property.IsConstParam & !usage.HasFlag(ELocalUsageScenarioType.ReturnValue) ? "const " : "";

        return $"{constTag}{property.CppTypeName}&";
        // return base.GetCppFunctionDeclarationType(property);
    }

    /// <summary>
    /// Gets the name of the fast invoke delegate parameter type.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="localUsageScenarioType">Type of the local usage scenario.</param>
    /// <returns>string.</returns>
    public override string GetFastInvokeDelegateParameterTypeName(PropertyDefinition property, ELocalUsageScenarioType localUsageScenarioType)
    {
        // struct force pass by pointer 
        if(localUsageScenarioType.HasFlag(ELocalUsageScenarioType.ReturnValue))
        {
            return base.GetFastInvokeDelegateParameterTypeName(property, localUsageScenarioType);
        }

        var cSharpType = GetCSharpTypeName(property, localUsageScenarioType);

        return $"{cSharpType}*";
    }

    /// <summary>
    /// Gets the name of the fast invoke parameter.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>string.</returns>
    public override string GetFastInvokeParameterName(PropertyDefinition property)
    {
        return property.IsPassByReferenceInCpp ? $"&__{property.Name}" : $"&{property.Name!}";
    }

    [GeneratedRegex(@"(\w+(\[\d+\])?)=(((\w+)|(\(([^()]+|(?<Level>\()|(?<-Level>\)))+(?(Level)(?!))\)))?)")]
    private static partial Regex GenUnrealDefaultValueTextMatchRegex();
}