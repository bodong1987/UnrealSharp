using System.Diagnostics.CodeAnalysis;
using UnrealSharp.Utils.CommandLine;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.TypeInfo;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Processors;

[Export("UnrealSharpTools", typeof(IBaseWorkModeProcessor))]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
internal class GenGuidProcessor() : AbstractBaseWorkModeProcessor<GenGuidOptions>("genGuid")
{
    [RequiresDynamicCode("With dynamic codes)")]
    protected override int Process(GenGuidOptions value)
    {
        foreach(var text in value.Strings)
        {
            var guid = PropertyDefinition.GenStableGuidString(text);

            Logger.Log($"{text}{Environment.NewLine}  {guid}");
        }

        return 0;
    }
}

public class GenGuidOptions
{
    [Option('s', "strings", Required = true, HelpText = "Input strings")]
    // ReSharper disable once CollectionNeverUpdated.Global
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public List<string> Strings { get; set; } = [];
}