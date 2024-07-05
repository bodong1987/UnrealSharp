using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen;

/// <summary>
/// Class EnumTypeExporter.
/// Implements the <see cref="UnrealSharpTool.Core.CodeGen.BaseTypeExporter" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.CodeGen.BaseTypeExporter" />
public class EnumTypeExporter : BaseTypeExporter
{
    #region Properties
    /// <summary>
    /// The enum type
    /// </summary>
    public readonly EnumTypeDefinition EnumType;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumTypeExporter"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="targetDirectory">The target directory.</param>
    /// <param name="typeDefinition">The type definition.</param>
    public EnumTypeExporter(BindingContext context, string targetDirectory, EnumTypeDefinition typeDefinition) :
        base(context, targetDirectory, typeDefinition)
    {
        EnumType = typeDefinition;
    }
    #endregion

    #region Template Methods Override
    /// <summary>
    /// Queries the base type main comment text.
    /// </summary>
    /// <returns>System.String.</returns>
    protected override string QueryBaseTypeMainCommentText()
    {
        return $"Enum {EnumType.CppName}{Environment.NewLine}" + base.QueryBaseTypeMainCommentText();
    }

    /// <summary>
    /// Determines whether [is meta accept in main comment] [the specified key].
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns><c>true</c> if [is meta accept in main comment] [the specified key]; otherwise, <c>false</c>.</returns>
    protected override bool IsMetaAcceptInMainComment(string key)
    {
        return EnumType.Fields.Find(x => key.StartsWith(x.Name!)) == null;
    }

    /// <summary>
    /// Exports the internal.
    /// </summary>
    /// <returns><c>true</c> success, <c>false</c> otherwise.</returns>
    protected override bool ExportInternal()
    {
        Logger.Assert(Type.IsEnum);

        if (EnumType.IsFlags)
        {
            Writer.Write("[Flags]");
        }

        if (IsNativeBinding)
        {
            Writer.Write($"[NativeBinding(\"{Type.Name}\", \"{Type.CppName}\", \"{Type.PathName}\")]");
        }
        else if(IsBlueprintBinding)
        {
            Writer.Write($"[BlueprintBinding(\"{Type.PathName}\")]");
        }
            
        WriteAttributes((EEnumFlags)Type.Flags, "UENUM", IsBindingToUnrealImplement ? null:Type.Metas);

        Writer.Write($"{BaseTypeAccessPermission} enum {Type.Name} : {EnumType.UnderlyingTypeName}");
        {
            using var enumScope = new ScopedCodeWriter(Writer);
            {
                var padding = CalcPadLength();

                // if (EnumType.Fields.Count > 0)
                // {
                //     Writer.Write("#pragma warning disable CA1069");
                // }
                
                var lastField = EnumType.Fields.LastOrDefault();
                
                foreach (var field in EnumType.Fields)
                {
                    var anyValidComment = false;

                    var tooltip = EnumType.GetFieldTooltip(field.Name!);

                    if (tooltip.IsNotNullOrEmpty())
                    {
                        Writer.WriteComment(tooltip);

                        anyValidComment = true;
                    }

                    var displayName = EnumType.GetFieldDisplayName(field.Name!);

                    if (displayName.IsNotNullOrEmpty())
                    {
                        if (tooltip.IsNotNullOrEmpty())
                        {
                            Writer.WriteComment(displayName, "meta", "name=\"DisplayName\"");
                        }
                        else
                        {
                            Writer.WriteComment(displayName);
                        }

                        anyValidComment = true;
                    }

                    var value = $"0x{field.Value:x}";

                    if (field.Value == -1)
                    {
                        value = $"{EnumType.UnderlyingTypeName}.MaxValue";
                    }

                    if(!anyValidComment)
                    {
                        Writer.WriteComment(field.Name!);
                    }

                    Writer.Write($"{field.Name!.PadRight(padding)} = ({EnumType.UnderlyingTypeName}){value}{(lastField == field ? "":",")}");
                }
                
                // if (EnumType.Fields.Count > 0)
                // {
                //     Writer.Write("#pragma warning restore CA1069");
                // }
            }
        }            

        Writer.WriteNewLine();

        Writer.WriteComment($"Class {Type.Name}InteropPolicy");
        Writer.Write("[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]");
        Writer.Write($"{BaseTypeAccessPermission} class {Type.Name}InteropPolicy : TEnumInteropPolicy<{Type.Name}>");
        {
            using var enumInteropPolicyScope = new ScopedCodeWriter(Writer);
        }

        return true;
    }
    #endregion

    #region Misc
    /// <summary>
    /// Calculates the length of the pad.
    /// </summary>
    /// <returns>System.Int32.</returns>
    private int CalcPadLength()
    {
        return EnumType.Fields.Count > 0 ? Math.Max(24, EnumType.Fields.Select(x => x.Name!.Length).Max() + 1) : 24;
    }
    #endregion
}