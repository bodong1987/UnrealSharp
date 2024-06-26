using System.Diagnostics.CodeAnalysis;
using UnrealSharp.Utils.CommandLine;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Extensions.IO;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.TypeInfo;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Processors
{
    /// <summary>
    /// Class TypeDatabaseGenerateProcessor.
    /// Implements the <see cref="UnrealSharpTool.AbstractBaseWorkModeProcessor{UnrealSharpTool.Processors.TypeDatabaseGenerationOptions}" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.AbstractBaseWorkModeProcessor{UnrealSharpTool.Processors.TypeDatabaseGenerationOptions}" />
    [Export("UnrealSharpTools", typeof(IBaseWorkModeProcessor))]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    class TypeDatabaseGenerateProcessor : AbstractBaseWorkModeProcessor<TypeDatabaseGenerationOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefTypeExportProcessor" /> class.
        /// </summary>
        public TypeDatabaseGenerateProcessor() :
            base("typegen")
        {
        }

        /// <summary>
        /// Checks the options.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected override bool CheckOptions(TypeDatabaseGenerationOptions value)
        {
            if (!value.AssemblyPath.IsFileExists())
            {
                Logger.LogError($"input assembly file <{value.AssemblyPath}> is not exists.");
                return false;
            }

            if (value.OutputPath.IsNullOrEmpty())
            {
                Logger.LogError("generate type info need an output path. add argument by -o or --output");
                return false;
            }

            return base.CheckOptions(value);
        }

        /// <summary>
        /// Processes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.Int32.</returns>
        [RequiresDynamicCode("With dynmiac codes)")]
        protected override int Process(TypeDatabaseGenerationOptions value)
        {
            Logger.EnsureNotNull(CommandArguments);

            Logger.Log("Start Export TypeDefinitionDocument(Type Database)");
            Logger.Log("Input Path: {0}", value.AssemblyPath);
            Logger.Log("Source Path: {0}", value.SourceDirectory);
            Logger.Log("Target Path: {0}", value.OutputPath!);
            Logger.Log("Source Ignore Regex: {0}", value.SourceFileIgnoreRegex);

            Logger.Log("Start Create TypeDefinitionDocument, Please wait...");

            var sourceFiles = EnumerateSourceFileUtils.EnumerateSourceFiles(value.SourceDirectory, value.SourceFileIgnoreRegex);

            var extraOptions = LoadExtraOptions<AssemblySearchOptions>();

            var document = TypeDefinitionDocument.CreateDocument(
                BindingCodeGenerateSourceType.Assembly,
                value.AssemblyPath,
                extraOptions,
                sourceFiles
                );

            if (document == null)
            {
                Logger.LogError("Failed create from assembly paths.");
                return -1;
            }

            Logger.Log("Start Export to File: {0}, Please wait...", value.OutputPath!);

            document.SaveToFile(value.OutputPath!);

            Logger.Log("Success.");

            return 0;
        }
    }

    /// <summary>
    /// Class DefTypeExportOptions.
    /// </summary>
    class TypeDatabaseGenerationOptions
    {
        /// <summary>
        /// Gets or sets the assembly paths.
        /// </summary>
        /// <value>The assembly paths.</value>
        [Option('a', "assembly", Required = true, HelpText = "Assembly Path")]
        public string AssemblyPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source directory.
        /// </summary>
        /// <value>The source directory.</value>
        [Option("sourceDirectory", Required = true, HelpText = "source file directory")]
        public string SourceDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source file match regex.
        /// </summary>
        /// <value>The source file match regex.</value>
        [Option("sourceFileIgnoreRegex", Required = false, HelpText = "used to filter source files")]
        public string SourceFileIgnoreRegex { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        /// <value>The output path.</value>
        [Option('o', "output", Required = true, HelpText = "output path")]
        public string? OutputPath { get; set; }
    }
}
