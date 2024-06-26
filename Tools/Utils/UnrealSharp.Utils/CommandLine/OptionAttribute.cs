using UnrealSharp.Utils.Extensions;

namespace UnrealSharp.Utils.CommandLine
{
    /// <summary>
    /// Class OptionAttribute.
    /// Implements the <see cref="Attribute" />
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class OptionAttribute : Attribute
    {
        /// <summary>
        /// Gets the long name.
        /// </summary>
        /// <value>The long name.</value>
        public string? LongName { get; internal set; }
        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>The short name.</value>
        public string? ShortName { get; private set; }
        
        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>The help text.</value>
        public string HelpText { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="OptionAttribute"/> is required.
        /// </summary>
        /// <value><c>true</c> if required; otherwise, <c>false</c>.</value>
        public bool Required { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionAttribute"/> class.
        /// </summary>
        /// <param name="shortName">The short name.</param>
        /// <param name="longName">The long name.</param>
        private OptionAttribute(string shortName, string longName)
        {
            ShortName = shortName;
            LongName = longName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionAttribute"/> class.
        /// </summary>
        public OptionAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionAttribute"/> class.
        /// </summary>
        /// <param name="longName">The long name.</param>
        public OptionAttribute(string longName) :
            this(string.Empty, longName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionAttribute" /> class.
        /// </summary>
        /// <param name="shortName">The short name.</param>
        /// <param name="longName">The long name.</param>
        public OptionAttribute(char shortName, string longName) :
            this(new string(shortName,1), longName)
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            if(ShortName.IsNotNullOrEmpty())
            {
                if(LongName.IsNotNullOrEmpty())
                {
                    return $"-{ShortName} --{LongName}";
                }

                return $"-{ShortName}";
            }

            if(LongName.IsNotNullOrEmpty())
            {
                return $"--{LongName}";
            }

            return "";
        }
    }
}
