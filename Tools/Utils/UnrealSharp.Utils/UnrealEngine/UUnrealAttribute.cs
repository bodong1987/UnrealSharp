namespace UnrealSharp.Utils.UnrealEngine;

/// <summary>
/// Enum EBindingExportFlags
/// </summary>
[Flags]
public enum EBindingExportFlags
{
    /// <summary>
    /// The none
    /// </summary>
    None = 0,

    /// <summary>
    /// The no constructor
    /// </summary>
    NoConstructor = 1 << 0,

    /// <summary>
    /// The no meta data
    /// </summary>
    NoMetaData = 1 << 1,

    /// <summary>
    /// The no from native
    /// </summary>
    NoFromNative = 1 << 2,

    /// <summary>
    /// The no to native
    /// </summary>
    NoToNative = 1 << 3,

    /// <summary>
    /// The with structure view
    /// </summary>
    WithStructView = 1 << 4,

    /// <summary>
    /// The no properties
    /// </summary>
    NoProperties = 1 << 5
}

/// <summary>
/// Class UUnrealAttribute.
/// Implements the <see cref="Attribute" />
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="Attribute" />
public abstract class UUnrealAttribute<T> : Attribute where T : Enum
{
    /// <summary>
    /// Gets or sets the flags.
    /// </summary>
    /// <value>The flags.</value>
    // ReSharper disable once NotAccessedField.Global
    public readonly T Flags;

    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    /// <value>The category.</value>
    public string? Category { get; set; } = null;

    /// <summary>
    /// Gets or sets the tool tip.
    /// </summary>
    /// <value>The tool tip.</value>
    public string? ToolTip { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether this instance is deprecated.
    /// </summary>
    /// <value><c>true</c> if this instance is deprecated; otherwise, <c>false</c>.</value>
    public bool IsDeprecated { get; set; } = false;

    /// <summary>
    /// Gets or sets the deprecated message.
    /// </summary>
    /// <value>The deprecated message.</value>
    public string? DeprecatedMessage { get; set; } = null;

    /// <summary>
    /// Gets or sets the unique identifier.
    /// This GUID is used to force the naming of an Unreal exported type GUID
    /// </summary>
    /// <value>The unique identifier.</value>
    public string Guid { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the export flags.
    /// </summary>
    /// <value>The export flags.</value>
    public EBindingExportFlags ExportFlags { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UUnrealAttribute{T}"/> class.
    /// </summary>
    protected UUnrealAttribute()
    {
        Flags = default!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UUnrealAttribute{T}"/> class.
    /// </summary>
    /// <param name="flags">The flags.</param>
    protected UUnrealAttribute(T flags)
    {
        Flags = flags;
    }
}