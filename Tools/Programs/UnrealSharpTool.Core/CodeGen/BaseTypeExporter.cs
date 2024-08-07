﻿using System.Text;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.UnrealEngine;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;
// ReSharper disable VirtualMemberNeverOverridden.Global

// ReSharper disable MemberCanBeProtected.Global

namespace UnrealSharpTool.Core.CodeGen;

/// <summary>
/// Class BaseTypeExporter.
/// Provides a basic framework for outputting C# binding code
/// </summary>
public abstract class BaseTypeExporter
{
    /// <summary>
    /// The context
    /// </summary>
    public readonly BindingContext Context;

    /// <summary>
    /// The target directory
    /// </summary>
    public readonly string TargetDirectory;

    /// <summary>
    /// The type
    /// </summary>
    public readonly BaseTypeDefinition Type;

    /// <summary>
    /// The writer
    /// </summary>
    public readonly CSharpCodeWriter Writer = new();

    /// <summary>
    /// Gets or sets the target file.
    /// </summary>
    /// <value>The target file.</value>
    public string TargetFile { get; protected init; }

    /// <summary>
    /// Gets a value indicating whether this instance is native binding.
    /// </summary>
    /// <value><c>true</c> if this instance is native binding; otherwise, <c>false</c>.</value>
    public bool IsNativeBinding => Context.IsNativeBinding;

    /// <summary>
    /// Gets a value indicating whether this instance is script binding.
    /// </summary>
    /// <value><c>true</c> if this instance is script binding; otherwise, <c>false</c>.</value>
    public bool IsCSharpBinding => Context.IsCSharpBinding;

    /// <summary>
    /// Gets a value indicating whether this instance is blueprint binding.
    /// </summary>
    /// <value><c>true</c> if this instance is blueprint binding; otherwise, <c>false</c>.</value>
    public bool IsBlueprintBinding => Context.IsBlueprintBinding;

    /// <summary>
    /// Gets a value indicating whether this instance is binding to unreal implement.
    /// </summary>
    /// <value><c>true</c> if this instance is binding to unreal implement; otherwise, <c>false</c>.</value>
    public bool IsBindingToUnrealImplement => Context.IsBindingToUnrealImplement;

    /// <summary>
    /// Gets the base type access permission.
    /// </summary>
    /// <value>The base type access permission.</value>
    public string BaseTypeAccessPermission => IsBlueprintBinding? "internal" : "public";

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseTypeExporter"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="targetDirectory">The target directory.</param>
    /// <param name="typeDefinition">The type definition.</param>
    protected BaseTypeExporter(BindingContext context, string targetDirectory, BaseTypeDefinition typeDefinition)
    {
        Context = context;
        TargetDirectory = targetDirectory;
        Type = typeDefinition;
        TargetFile = Path.Combine(targetDirectory, $"{typeDefinition.CppName}.gen.cs");

        try
        {
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// The default namespaces
    /// Output common dependency namespaces, which may be redundant
    /// </summary>
    public static readonly SortedSet<string> DefaultNamespaces =
    [
        "System.Diagnostics",
        "System.Diagnostics.CodeAnalysis",
        "System.Runtime.InteropServices",
        "System.Runtime.CompilerServices",
        "UnrealSharp.Utils.Misc",
        "UnrealSharp.Utils.UnrealEngine",
        "UnrealSharp.UnrealEngine",
        "UnrealSharp.UnrealEngine.Collections",
        "UnrealSharp.UnrealEngine.InteropService"
    ];

    /// <summary>
    /// Exports this instance.
    /// </summary>
    /// <returns>ECodeWriterSaveResult.</returns>
    public virtual ECodeWriterSaveResult Export()
    {
        // Write copyright first            
        Writer.Write(CodegenCopyright.GetCopyright(TargetFile));

        // provide them by .editorconfig
        
        // disable some rider static code check types 
        Writer.Write("// ReSharper disable GrammarMistakeInComment");
        // Writer.Write("// ReSharper disable RedundantUsingDirective");
        // Writer.Write("// ReSharper disable InconsistentNaming");
        Writer.Write("// ReSharper disable IdentifierTypo");
        // Writer.Write("// ReSharper disable RedundantUnsafeContext");
        // Writer.Write("// ReSharper disable RedundantArgumentDefaultValue");
        // Writer.Write("// ReSharper disable ArrangeAccessorOwnerBody");
        Writer.Write("// ReSharper disable StringLiteralTypo");
        // Writer.Write("// ReSharper disable RedundantTypeArgumentsOfMethod");
        // Writer.Write("// ReSharper disable RedundantCast");
        // Writer.Write("// ReSharper disable ParameterHidesMember");
        // Writer.Write("// ReSharper disable RedundantNameQualifier");
        // Writer.Write("// ReSharper disable UseObjectOrCollectionInitializer");
        // Writer.Write("// ReSharper disable BuiltInTypeReferenceStyle");
        
        
        // write common namespace using
        foreach (var ns in DefaultNamespaces)
        {
            Writer.Write($"using {ns};");
        }

        // write extra namespace
        WriteExtraNamespaces();

        Writer.WriteNewLine();

        // use C# 10.0 File-Scoped namespaces
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/file-scoped-namespaces
        Writer.Write($"namespace {Type.Namespace};");
        Writer.WriteNewLine();

        // write common main comments
        WriteMainCommentText();
            
        // export internal
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (!ExportInternal())
        {
            return ECodeWriterSaveResult.Failure;
        }

        return Writer.Save(TargetFile);
    }

    /// <summary>
    /// Writes the extra namespaces.
    /// </summary>
    protected virtual void WriteExtraNamespaces() 
    {
    }

    /// <summary>
    /// Exports the internal.
    /// </summary>
    /// <returns><c>true</c> if success, <c>false</c> otherwise.</returns>
    protected abstract bool ExportInternal();

    /// <summary>
    /// Queries the base type main comment text.
    /// </summary>
    /// <returns>System.String.</returns>
    protected virtual string QueryBaseTypeMainCommentText()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"Export for {Type.PathName}");

        var tooltip = Type.Metas.GetMeta(MetaConstants.ToolTip);

        if (tooltip.IsNotNullOrEmpty())
        {
            stringBuilder.AppendLine(tooltip);
        }
        else
        {
            var comment = Type.Metas.GetMeta(MetaConstants.Comment);

            if(comment.IsNotNullOrEmpty())
            {
                stringBuilder.AppendLine(comment);
            }
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Writes the main comment text.
    /// </summary>
    protected virtual void WriteMainCommentText()
    {
        Writer.WriteComment(QueryBaseTypeMainCommentText());

        foreach (var i in Type.Metas.Metas.Where(i => i.Key != "Comment" && i.Key != "ToolTip" && IsMetaAcceptInMainComment(i.Key)))
        {
            Writer.WriteComment(i.Value, "meta", $"name=\"{i.Key}\"");
        }
    }
    /// <summary>
    /// Determines whether [is meta accept in main comment] [the specified key].
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns><c>true</c> if [is meta accept in main comment] [the specified key]; otherwise, <c>false</c>.</returns>
    protected virtual bool IsMetaAcceptInMainComment(string key)
    {
        return true;
    }

    /// <summary>
    /// Writes the meta comments.
    /// </summary>
    /// <param name="metaDefinition">The meta definition.</param>
    /// <returns><c>true</c> if success, <c>false</c> otherwise.</returns>
    protected virtual bool WriteMetaComments(MetaDefinition metaDefinition)
    {
        var bHasSummary = false;

        var tooltip = metaDefinition.GetMeta(MetaConstants.ToolTip);

        if (tooltip.IsNotNullOrEmpty())
        {
            Writer.WriteComment(tooltip);

            bHasSummary = true;
        }

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var i in metaDefinition.Metas)
        {
            if (i.Key != MetaConstants.Comment && 
                i.Key != MetaConstants.ToolTip && 
                i.Key != MetaConstants.ModuleRelativePath && 
                IsMetaAcceptInMainComment(i.Key)
               )
            {
                Writer.WriteComment(i.Value, "meta", $"name=\"{i.Key}\"");
            }
        }

        return bHasSummary;
    }

    /// <summary>
    /// Writes the attributes comments.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="flags">The flags.</param>
    /// <param name="attributeTypeName">Name of the attribute type.</param>
    /// <param name="metaDefinition">The meta definition.</param>
    protected virtual void WriteAttributesComments<T>(T flags, string attributeTypeName, MetaDefinition? metaDefinition)
        where T : Enum
    {
        var list = flags.GetUniqueFlags().ToList();
        list.RemoveAll(x => x.ToString() == "None" || x.ToString() == "NoFlags");

        var flagsText = string.Join('|', list.Select(x => $"{typeof(T).Name}.{x}"));
            
        Writer.WriteComment(flagsText, attributeTypeName, "name=\"Flags\"");

        if (metaDefinition != null)
        {
            WriteMetaAttributesComments(metaDefinition);
        }
    }

    /// <summary>
    /// Writes the meta attributes comments.
    /// </summary>
    /// <param name="metas">The metas.</param>
    protected virtual void WriteMetaAttributesComments(MetaDefinition metas)
    {
        foreach (var meta in metas.Metas)
        {
            // ReSharper disable once MergeIntoLogicalPattern
            if ((Context.IsNativeBinding || Context.IsBlueprintBinding) &&
                (meta.Key == MetaConstants.ToolTip || meta.Key == MetaConstants.Comment || meta.Key == MetaConstants.ModuleRelativePath))
            {
                continue;
            }

            var v = meta.Value.Replace("\"", "\\\"");
            v = v.Replace("\r", "\\r");
            v = v.Replace("\n", "\\n");
            Writer.WriteComment(v, "meta", $"name=\"{meta.Key}\"");
        }
    }

    /// <summary>
    /// Writes the attributes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="flags">The flags.</param>
    /// <param name="attributeTypeName">Name of the attribute type.</param>
    /// <param name="metaDefinition">The meta definition.</param>
    /// <param name="extraParamListDelegate">The extra parameter list delegate.</param>
    protected virtual void WriteAttributes<T>(T flags, string attributeTypeName, MetaDefinition? metaDefinition, Func<IEnumerable<string>?>? extraParamListDelegate = null)
        where T : Enum
    {
        var list = flags.GetUniqueFlags().ToList();
        list.RemoveAll(x => x.ToString() == "None" || x.ToString() == "NoFlags");

        var flagsText = string.Join('|', list.Select(x => $"{typeof(T).Name}.{x}"));

        var extraParamList = extraParamListDelegate?.Invoke();
        var extraParamText = "";
                        
        if(extraParamList != null)
        {
            var extraParams = new StringBuilder();

            var paramList = extraParamList as string[] ?? extraParamList.ToArray();
            foreach (var p in paramList)
            {
                if (flagsText.IsNotNullOrEmpty() || p != paramList.First())
                {
                    extraParams.Append(", ");
                }

                extraParams.Append(p);
            }

            extraParamText = extraParams.ToString();
        }

        var attrList = $"{flagsText}{extraParamText}";
        Writer.Write(attrList.Length > 0 ? $"[{attributeTypeName}({attrList})]" : $"[{attributeTypeName}]");

        if (metaDefinition != null)
        {
            WriteMetaAttributes(metaDefinition);
        }
    }

    /// <summary>
    /// Writes the meta attributes.
    /// </summary>
    /// <param name="metas">The metas.</param>
    protected virtual void WriteMetaAttributes(MetaDefinition metas)
    {
        foreach (var meta in metas.Metas)
        {
            // ReSharper disable once MergeIntoLogicalPattern
            if ((Context.IsNativeBinding || Context.IsBlueprintBinding) &&
                (meta.Key == MetaConstants.ToolTip ||
                 meta.Key == MetaConstants.Comment || 
                 meta.Key == MetaConstants.ModuleRelativePath
                ))
            {
                continue;
            }

            // skip internal fields
            if(typeof(UPROPERTYAttribute).GetProperty(meta.Key) != null ||
               typeof(UCLASSAttribute).GetProperty(meta.Key) != null ||
               typeof(USTRUCTAttribute).GetProperty(meta.Key) != null ||
               typeof(UENUMAttribute).GetProperty(meta.Key) != null ||
               typeof(UEVENTAttribute).GetProperty(meta.Key) != null
              )
            {
                continue;
            }

            var v = meta.Value.Replace("\"", "\\\"");
            v = v.Replace("\r", "\\r");
            v = v.Replace("\n", "\\n");
            // ReSharper disable once StringLiteralTypo
            Writer.Write($"[UMETA(\"{meta.Key}\", \"{v}\")]"); 
        }
    }
}