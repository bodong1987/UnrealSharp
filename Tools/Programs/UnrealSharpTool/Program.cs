using System.Diagnostics.CodeAnalysis;
using UnrealSharp.Utils.CommandLine;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool;

/// <summary>
/// Class Program.
/// </summary>
internal class Program
{
    /// <summary>
    /// Defines the entry point of the application.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>System.Int32.</returns>
    [RequiresDynamicCode("Calls UnrealSharpTool.Core.Utils.ExtensibilityFramework.ComposeParts<T>(T, Object, params Object[])")]
    private static int Main(string[] args)
    {
        var result = Parser.Default.Parse<UnrealSharpToolBaseOptions>(args);

        if(result.Value == null)
        {
            Logger.LogError("Invalid command line arguments. use -m/--m set work mode.");
            Logger.LogError(Parser.GetHelpText(new UnrealSharpToolBaseOptions()));
                            
            return 2;
        }

        Logger.Log("Work Mode:{0}", result.Value.Mode!);
        if(result.Value.Mode.IsNullOrEmpty())
        {
            Logger.LogError("Invalid command line arguments, mode is invalid. use -m/--m set work mode.");
            Logger.LogError(Parser.GetHelpText(new UnrealSharpToolBaseOptions()));

            return 7;
        }

        ExtensibilityFramework.AddPart(typeof(Program).Assembly);

        var provider = new WorkModeProvider();
        ExtensibilityFramework.ComposeParts(provider, provider);

        var processor = provider.Processors.Find(x => x.Mode.iEquals(result.Value.Mode));

        if(processor == null)
        {
            Logger.LogError($"No work mode:{result.Value.Mode}");
            return 3;
        }

        Logger.Log("Start work mode:{0}", result.Value.Mode);

#if !DEBUG
            try
#endif
        {
            var exitCode = processor.Main(args);

            Logger.Log("UnrealSharpTool Finished, Exit Code: {0}{1}", exitCode, Environment.NewLine);

            return exitCode;
        }
#if !DEBUG
            catch(Exception ex)
            {
                Logger.LogError($"{ex.Message}\n{ex.StackTrace}");
                return 5;
            }
#endif
    }
}

#region Base Framework
/// <summary>
/// Class WorkModeProvider.
/// Implements the <see cref="AbstractComposableTarget" />
/// </summary>
/// <seealso cref="AbstractComposableTarget" />
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class WorkModeProvider : AbstractComposableTarget
{
    /// <summary>
    /// Gets or sets the processors.
    /// </summary>
    /// <value>The processors.</value>
    [Import("UnrealSharpTools")]
    public List<IBaseWorkModeProcessor> Processors { get; set; } = [];
}

/// <summary>
/// Class UnrealSharpToolBaseOptions.
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class UnrealSharpToolBaseOptions
{
    /// <summary>
    /// Gets or sets the mode.
    /// </summary>
    /// <value>The mode.</value>
    // ReSharper disable StringLiteralTypo
    [Option('m', "mode", Required =true, HelpText = "what do you want to do? eg:typegen/codegen")]
    public string? Mode { get; set; }
}

/// <summary>
/// Interface IBaseWorkModeProcessor
/// Implements the <see cref="IImportable" />
/// </summary>
/// <seealso cref="IImportable" />
public interface IBaseWorkModeProcessor : IImportable
{
    /// <summary>
    /// Gets the mode.
    /// </summary>
    /// <value>The mode.</value>
    string Mode { get; }

    /// <summary>
    /// Mains the specified arguments.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>System.Int32.</returns>
    [RequiresDynamicCode("Dynmiac Invoke")]
    int Main(string[] args);
}

/// <summary>
/// Class AbstractBaseWorkModeProcessor.
/// Implements the <see cref="AbstractImportable" />
/// Implements the <see cref="UnrealSharpTool.IBaseWorkModeProcessor" />
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="AbstractImportable" />
/// <seealso cref="UnrealSharpTool.IBaseWorkModeProcessor" />
public abstract class AbstractBaseWorkModeProcessor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : AbstractImportable, IBaseWorkModeProcessor
    where T : class, new()
{
    /// <summary>
    /// Gets or sets the mode.
    /// </summary>
    /// <value>The mode.</value>
    public string Mode { get; }

    /// <summary>
    /// Gets or sets the command arguments.
    /// </summary>
    /// <value>The command arguments.</value>
    // ReSharper disable once MemberCanBeProtected.Global
    public string[]? CommandArguments { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractBaseWorkModeProcessor{T}"/> class.
    /// </summary>
    /// <param name="mode">The mode.</param>
    protected AbstractBaseWorkModeProcessor(string mode)
    {
        Mode = mode;
    }

    /// <summary>
    /// Mains the specified arguments.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>System.Int32.</returns>        
    [RequiresDynamicCode("Invoke Process")]
    public int Main(string[] args)
    {
        CommandArguments = args;
        var result = Parser.Default.Parse<T>(args);
        var helpText = Parser.GetHelpText(new T());

        if (result.Result == ParserResultType.NotParsed || result.Value == null)
        {
            Logger.LogError("{0}, see help:{2}{1}", result.ErrorMessage, helpText, Environment.NewLine);
            return -1;
        }

        if(!CheckOptions(result.Value))
        {                
            Logger.LogError("Invalid command arguments, see help:{1}{0}", helpText, Environment.NewLine);
            return -1;
        }

        return Process(result.Value);
    }

    /// <summary>
    /// Processes the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>System.Int32.</returns>
    [RequiresDynamicCode("With dynmiac codes)")]
    protected abstract int Process(T value);
    /// <summary>
    /// Checks the options.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> success, <c>false</c> otherwise.</returns>
    protected virtual bool CheckOptions(T value)
    {
        return true;
    }

    /// <summary>
    /// Loads the extra options.
    /// </summary>
    /// <typeparam name="TExtraOptionType">The type of the t extra option type.</typeparam>
    /// <returns>System.Nullable&lt;TExtraOptionType&gt;.</returns>
    protected TExtraOptionType? LoadExtraOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TExtraOptionType>() where TExtraOptionType : class, new()
    {
        Logger.EnsureNotNull(CommandArguments);

        var result = Parser.Default.Parse<TExtraOptionType>(CommandArguments);

        var helpText = Parser.GetHelpText(new TExtraOptionType());

        if (result.Result == ParserResultType.NotParsed || result.Value == null)
        {
            Logger.LogError("{0}, see help:{2}{1}", result.ErrorMessage, helpText, Environment.NewLine);
            return null;
        }

        return result.Value;
    }
}
#endregion