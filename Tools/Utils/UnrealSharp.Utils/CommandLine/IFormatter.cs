using UnrealSharp.Utils.Extensions;
using System.Text;

namespace UnrealSharp.Utils.CommandLine
{
    /// <summary>
    /// Interface IFormatter
    /// </summary>
    public interface IFormatter
    {
        /// <summary>
        /// Appends the specified builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="name">The name.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="helpText">The help text.</param>
        /// <param name="usage">The usage.</param>
        void Append(
            StringBuilder builder, 
            string name, 
            string attribute, 
            string helpText, 
            string usage
            );
    }

    /// <summary>
    /// Class Formatter.
    /// Implements the <see cref="UnrealSharp.Utils.CommandLine.IFormatter" />
    /// </summary>
    /// <seealso cref="UnrealSharp.Utils.CommandLine.IFormatter" />
    public class Formatter : IFormatter
    {
        /// <summary>
        /// The indent
        /// </summary>
        public int Indent;

        /// <summary>
        /// The blank
        /// </summary>
        public int Blank;

        /// <summary>
        /// Initializes a new instance of the <see cref="Formatter"/> class.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <param name="blank">The blank.</param>
        public Formatter(int indent, int blank)
        {
            this.Indent = indent;
            this.Blank = blank;
        }

        /// <summary>
        /// Appends the specified builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="name">The name.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="helpText">The help text.</param>
        /// <param name="usage">The usage.</param>
        public void Append(
            StringBuilder builder, 
            string name, 
            string attribute, 
            string helpText, 
            string usage
            )
        {
            int blank = name.Length < Blank ? Blank - name.Length : 1;
            string attr = attribute.IsNotNullOrEmpty() ? $"[{attribute}]" : "";
            string dot = !helpText.EndsWith('.') ? ". " : " ";

            builder.AppendLine($"{BuildBlankText(Indent)}{name}{BuildBlankText(blank)}{attr}{helpText}{dot}{(usage.IsNotNullOrEmpty()?"usage:":"")}{usage}");
        }

        /// <summary>
        /// Builds the blank text.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns>System.String.</returns>
        public static string BuildBlankText(int count)
        {
            return new string(' ', count);
        }
    }
}
