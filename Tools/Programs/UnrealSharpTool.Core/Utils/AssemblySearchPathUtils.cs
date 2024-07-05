using System.Runtime.InteropServices;
using UnrealSharp.Utils.CommandLine;
using UnrealSharp.Utils.Extensions.IO;
using UnrealSharpTool.Core.TypeInfo;

// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable InconsistentNaming

namespace UnrealSharpTool.Core.Utils;

/// <summary>
/// Enum ERuntimeType
/// </summary>
public enum ERuntimeType
{
    /// <summary>
    /// The mono
    /// </summary>
    Mono,
        
    /// <summary>
    /// The core color
    /// </summary>
    CoreCLR
}

/// <summary>
/// Enum EConfigurationType
/// </summary>
public enum EConfigurationType
{
    /// <summary>
    /// The debug
    /// </summary>
    Debug,
    /// <summary>
    /// The release
    /// </summary>
    Release
}

/// <summary>
/// Enum EPlatformType
/// </summary>
public enum EPlatformType
{
    /// <summary>
    /// The windows
    /// </summary>
    Windows,
    /// <summary>
    /// The osx
    /// </summary>
    OSX,
    /// <summary>
    /// The linux
    /// </summary>
    Linux,
    /// <summary>
    /// The free BSD
    /// </summary>
    FreeBSD,
    /// <summary>
    /// The ios
    /// </summary>
    IOS,
    /// <summary>
    /// The android
    /// </summary>
    Android
}

/// <summary>
/// Enum EArchitectureType
/// </summary>
public enum EArchitectureType
{
    /// <summary>
    /// The X64
    /// </summary>
    X64,
    /// <summary>
    /// The arm64
    /// </summary>
    Arm64
}


/// <summary>
/// Class AssemblySearchOptions.
/// </summary>
public class AssemblySearchOptions : ITypeDefinitionDocumentInitializeOptions
{
    /// <summary>
    /// Gets or sets the project directory.
    /// </summary>
    /// <value>The project directory.</value>
    [Option('p', "project", Required = true, HelpText = "Your unreal project directory path.")]
    public string UnrealProjectDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the runtime.
    /// </summary>
    /// <value>The type of the runtime.</value>
    [Option("runtime", Required = false, HelpText = "Mono/CoreCLR")]
    public ERuntimeType RuntimeType { get; set; } = ERuntimeType.Mono;

    /// <summary>
    /// Gets or sets the type of the configuration.
    /// </summary>
    /// <value>The type of the configuration.</value>
    [Option("configuration", Required = false, HelpText = "Debug or Release")]
    public EConfigurationType ConfigurationType { get; set; }

    /// <summary>
    /// Gets or sets the type of the platform.
    /// </summary>
    /// <value>The type of the platform.</value>
    [Option("platform", Required = false, HelpText = "Your target platform")]
    public EPlatformType PlatformType { get; set; } = EPlatformType.Windows;

    /// <summary>
    /// Gets or sets the type of the architecture.
    /// </summary>
    /// <value>The type of the architecture.</value>
    [Option("arch", Required = false, HelpText = "Your architecture type")]
    public EArchitectureType ArchitectureType { get; set; }

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    /// <value>The version.</value>
    [Option("version", Required = false, HelpText = "Your .net version, eg:net8.0")]
    public string Version { get; set; } = "net8.0";

    /// <summary>
    /// Gets or sets a value indicating whether [b ignore binding definition].
    /// </summary>
    /// <value><c>true</c> if [b ignore binding definition]; otherwise, <c>false</c>.</value>
    public bool IgnoreBindingDefinition { get; set; } = true;

    /// <summary>
    /// The custom search directories
    /// </summary>
    public readonly List<string> CustomSearchDirectories = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblySearchOptions"/> class.
    /// </summary>
    public AssemblySearchOptions()
    {
#if DEBUG
        ConfigurationType = EConfigurationType.Debug;
#else
            ConfigurationType = EConfigurationType.Release;
#endif

        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            PlatformType = EPlatformType.Windows;
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            PlatformType = EPlatformType.OSX;
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            PlatformType = EPlatformType.Linux;
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            PlatformType = EPlatformType.FreeBSD;
        }

        if(RuntimeInformation.OSArchitecture == Architecture.Arm || RuntimeInformation.OSArchitecture == Architecture.Arm64)
        {
            ArchitectureType = EArchitectureType.Arm64;
        }
        else
        {
            ArchitectureType = EArchitectureType.X64;
        }
    }

    /// <summary>
    /// Gets the configuration based runtime search paths.
    /// </summary>
    /// <returns>string[].</returns>
    public string[] GetConfigurationBasedRuntimeSearchPaths()
    {
        return
        [
            $"{UnrealProjectDirectory}/Plugins/UnrealSharp/ThirdParty/runtime/{Version.ToLower()}-{PlatformType.ToString().ToLower()}-{ConfigurationType}-{ArchitectureType.ToString().ToLower()}".CanonicalPath(),
            $"{UnrealProjectDirectory}/Plugins/UnrealSharp/ThirdParty/{RuntimeType.ToString().ToLower()}/{PlatformType.ToString().ToLower()}.{ArchitectureType.ToString().ToLower()}.{ConfigurationType}".CanonicalPath()
        ];
    }
}