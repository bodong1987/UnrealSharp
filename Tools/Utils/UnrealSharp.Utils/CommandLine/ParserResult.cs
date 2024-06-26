using System.Text;

namespace UnrealSharp.Utils.CommandLine
{
    /// <summary>
    /// Enum ParserResultType
    /// </summary>
    public enum ParserResultType
    {
        /// <summary>
        /// The parsed
        /// </summary>
        Parsed,

        /// <summary>
        /// The not parsed
        /// </summary>
        NotParsed,
    }

    /// <summary>
    /// Interface IParserResult
    /// </summary>
    public interface IParserResult
    {
        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>The result.</value>
        ParserResultType Result { get; internal set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        TypeInfo? Type { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        object? Value { get; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>The error message.</value>
        string ErrorMessage { get; }

        /// <summary>
        /// Appends the error.
        /// </summary>
        /// <param name="message">The message.</param>
        void AppendError(string message);
    }

    /// <summary>
    /// Class ParserResult.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ParserResult<T> : IParserResult
    {
        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>The result.</value>
        public ParserResultType Result { get; set; }
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public T? Value { get; set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public TypeInfo? Type { get; set; }

        /// <summary>
        /// The errors
        /// </summary>
        public StringBuilder Errors = new StringBuilder();

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        object? IParserResult.Value => this.Value;

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage => Errors.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserResult{T}"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public ParserResult(TypeInfo? type)
        {
            Type = type;
        }

        /// <summary>
        /// Appends the error.
        /// </summary>
        /// <param name="message">The message.</param>
        public void AppendError(string message)
        {
            Errors.AppendLine(message);
        }
    }
}
