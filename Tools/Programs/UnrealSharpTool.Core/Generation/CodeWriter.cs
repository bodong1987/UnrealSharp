using System.Diagnostics;
using System.Text;
using UnrealSharp.Utils.Extensions;

namespace UnrealSharpTool.Core.Generation;

/// <summary>
/// Enum ECodeWriterSaveResult
/// </summary>
public enum ECodeWriterSaveResult
{
    /// <summary>
    /// The failure
    /// </summary>
    Failure,
    /// <summary>
    /// The success
    /// </summary>
    Success,
    /// <summary>
    /// ignore when no changes
    /// </summary>
    IgnoreWhenNoChanges
}


/// <summary>
/// Class CodeWriter.
/// Help you write C++/C# codes
/// </summary>
public class CodeWriter
{
    /// <summary>
    /// Gets the target path.
    /// </summary>
    /// <value>The target path.</value>
    public string? TargetPath { get; private set; }

    /// <summary>
    /// The buffer
    /// </summary>
    private readonly StringBuilder _buffer = new();

    /// <summary>
    /// The indent level
    /// </summary>
    private int _indentLevel;

    /// <summary>
    /// The z fast access string
    /// </summary>
    private static readonly string[] ZFastAccessString =
    [
        "",
        "    ",
        "        ",
        "            ",
        "                ",
        "                    ",
        "                        ",
        "                            ",
        "                                ",
        "                                    ",
        "                                        "
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeWriter" /> class.
    /// </summary>
    // ReSharper disable once MemberCanBeProtected.Global
    public CodeWriter()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeWriter" /> class.
    /// </summary>
    /// <param name="targetPath">The target path.</param>
    // ReSharper disable once MemberCanBeProtected.Global
    public CodeWriter(string targetPath)
    {
        TargetPath = targetPath;
    }

    /// <summary>
    /// Begins the ident.
    /// </summary>
    /// <param name="withBrace">if set to <c>true</c> [with brace].</param>
    public void BeginIdent(bool withBrace)
    {
        if (withBrace)
        {
            WriteLeftBrace();
        }

        ++_indentLevel;
    }

    /// <summary>
    /// Ends the ident.
    /// </summary>
    /// <param name="withBrace">if set to <c>true</c> [with brace].</param>
    /// <param name="withSemicolon">if set to <c>true</c> [with semicolon].</param>
    public void EndIdent(bool withBrace, bool withSemicolon)
    {
        --_indentLevel;

        if (withBrace)
        {
            WriteRightBrace(withSemicolon);
        }
    }

    /// <summary>
    /// Gets the scoped space.
    /// </summary>
    /// <returns>System.String.</returns>
    public string GetScopedSpace()
    {
        if (_indentLevel >= 0 && _indentLevel < ZFastAccessString.Length)
        {
            return ZFastAccessString[_indentLevel];
        }

        return "".PadRight(_indentLevel * 4);
    }

    /// <summary>
    /// Writes the indent.
    /// </summary>
    private void WriteIndent()
    {
        _buffer.Append(GetScopedSpace());
    }

    /// <summary>
    /// Writes the right brace.
    /// </summary>
    /// <param name="withSemicolon">if set to <c>true</c> [with semicolon].</param>
    private void WriteRightBrace(bool withSemicolon)
    {
        Write(withSemicolon ? "};" : "}");
    }

    /// <summary>
    /// Writes the left brace.
    /// </summary>
    private void WriteLeftBrace()
    {
        Write("{");
    }

    /// <summary>
    /// Writes the specified text.
    /// </summary>
    /// <param name="text">The text.</param>
    public void Write(string text)
    {
        WriteIndent();
        _buffer.Append(text);
        WriteNewLine();
    }

    /// <summary>
    /// Writes the new line.
    /// </summary>
    public void WriteNewLine()
    {
        // don't use Environment.NewLine
        _buffer.Append('\n');
    }

    /// <summary>
    /// Writes the property.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="name">The name.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="padding">The padding.</param>
    public void WriteProperty(string type, string name, string defaultValue, int padding)
    {
        Write($"{type.PadRight(padding)} {name}{(!string.IsNullOrEmpty(defaultValue) ? "=" : "")}{(!string.IsNullOrEmpty(defaultValue) ? defaultValue : "")}");
    }

    /// <summary>
    /// The reserve symbols
    /// </summary>
    private static readonly KeyValuePair<string, string>[] ReserveSymbols =
    [
        new KeyValuePair<string, string>("<see cref", "__see_ref_symbol__"),
        new KeyValuePair<string, string>("/>", "__right_gt_symbol__")
    ];

    /// <summary>
    /// Writes C# style comments
    /// </summary>
    /// <param name="comment">The comment.</param>
    /// <param name="tag">The tag.</param>
    /// <param name="attribute">The attribute.</param>
    public void WriteComment(string comment, string tag = "summary", string attribute = "")
    {
        if(comment.IsNotNullOrEmpty() || attribute.IsNotNullOrEmpty())
        {
            comment = ReserveSymbols.Aggregate(comment, (current, r) => current.Replace(r.Key, r.Value));

            // remove xml special character                
            comment = comment.Replace("&", "&amp;");
            comment = comment.Replace("<", "&lt;");
            comment = comment.Replace(">", "&gt;");
            //comment = comment.Replace("'", "&apos;");
            //comment = comment.Replace("\"", "&quot;");

            comment = ReserveSymbols.Aggregate(comment, (current, r) => current.Replace(r.Value, r.Key));

            var lines = comment.Split('\n').ToList();
            lines.RemoveAll(x=>x.Trim().IsNullOrEmpty());

            if(lines.Count > 1 || tag.iEquals("summary"))
            {
                Write($"/// <{tag}{(attribute.IsNullOrEmpty()?"":" " + attribute)}>");

                foreach (var text in lines)
                {
                    Write($"/// {text.Trim()}");
                }

                Write($"/// </{tag}>");
            }
            else
            {
                if (lines.Count <= 0 || lines[0].IsNullOrEmpty())
                {
                    Write($"/// <{tag}{(attribute.IsNullOrEmpty() ? "" : " " + attribute)}/>");
                }
                else
                {
                    Write($"/// <{tag}{(attribute.IsNullOrEmpty() ? "" : " " + attribute)}>{lines[0]}</{tag}>");
                }
                    
            }
        }
    }

    /// <summary>
    /// Writes the common comment.
    /// </summary>
    /// <param name="comment">The comment.</param>
    public void WriteCommonComment(string comment)
    {
        if (comment.IsNotNullOrEmpty())
        {                
            var lines = comment.Split('\n').ToList();
            lines.RemoveAll(x => x.Trim().IsNullOrEmpty());

            foreach (var text in lines)
            {
                Write($"// {text.Trim()}");
            }
        }
    }

    /// <summary>
    /// Saves the specified target path.
    /// </summary>
    /// <param name="targetPath">The target path.</param>
    /// <param name="forceSave">if set to <c>true</c> [force save].</param>
    /// <returns>ECodeWriterSaveResult.</returns>
    public ECodeWriterSaveResult Save(string targetPath, bool forceSave = false)
    {
        TargetPath = targetPath;
        return Save(forceSave);
    }

    /// <summary>
    /// Bytes the arrays equal.
    /// </summary>
    /// <param name="a1">The a1.</param>
    /// <param name="a2">The a2.</param>
    /// <returns><c>true</c> is equal, <c>false</c> otherwise.</returns>
    /// <font color="red">Badly formed XML comment.</font>
    public static bool ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
    {
        return a1.SequenceEqual(a2);
    }

    /// <summary>
    /// Saves the specified force save.
    /// </summary>
    /// <param name="forceSave">if set to <c>true</c> [force save].</param>
    /// <returns>ECodeWriterSaveResult.</returns>
    public virtual ECodeWriterSaveResult Save(bool forceSave = false)
    {
        Debug.Assert(TargetPath != null);

        if(string.IsNullOrEmpty(TargetPath))
        {
            return ECodeWriterSaveResult.Failure;
        }

        // check is there any changed?
        var text = _buffer.ToString();
        var utf8Codes = Encoding.UTF8.GetBytes(text);
        if (!forceSave && File.Exists(TargetPath))
        {
            var existsBytes = File.ReadAllBytes(TargetPath!);
                
            if(ByteArraysEqual(existsBytes, utf8Codes))
            {
                // same file, skip write
                return ECodeWriterSaveResult.IgnoreWhenNoChanges;
            }
        }

        File.WriteAllBytes(TargetPath!, utf8Codes);

        return ECodeWriterSaveResult.Success;
    }
}

/// <summary>
/// Class ScopedCodeWriter.
/// Implements the <see cref="System.IDisposable" />
/// </summary>
/// <seealso cref="System.IDisposable" />
internal class ScopedCodeWriter : IDisposable
{
    /// <summary>
    /// The writer
    /// </summary>
    private readonly CodeWriter _writer;
        
    /// <summary>
    /// The with brace
    /// </summary>
    private readonly bool _withBrace, _withSemicolon;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScopedCodeWriter" /> class.
    /// </summary>
    /// <param name="codeWriter">The code writer.</param>
    /// <param name="withBrace">if set to <c>true</c> [with brace].</param>
    /// <param name="withSemicolon">if set to <c>true</c> [with semicolon].</param>
    public ScopedCodeWriter(CodeWriter codeWriter, bool withBrace = true, bool withSemicolon = false)
    {
        _writer = codeWriter;
        _writer.BeginIdent(withBrace);
        _withBrace = withBrace;
        _withSemicolon = withSemicolon;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _writer.EndIdent(_withBrace, _withSemicolon);
    }
}