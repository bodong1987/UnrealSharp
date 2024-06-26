using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using UnrealSharp.Utils.CommandLine;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.TypeInfo;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Processors
{
    [Export("UnrealSharpTools", typeof(IBaseWorkModeProcessor))]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    class GenGuidProcessor : AbstractBaseWorkModeProcessor<GenGuidOptions>
    {
        public GenGuidProcessor() : base("genGuid")
        {
        }

        [RequiresDynamicCode("With dynmiac codes)")]
        protected override int Process(GenGuidOptions value)
        {
            foreach(var text in value.Strings)
            {
                string guid = PropertyDefinition.GenStableGuidString(text);

                Logger.Log($"{text}{Environment.NewLine}  {guid}");
            }

            return 0;
        }
    }

    public class GenGuidOptions
    {
        [Option('s', "strings", Required = true, HelpText = "Input strings")]
        public List<string> Strings { get; set; } = [];
    }
}
