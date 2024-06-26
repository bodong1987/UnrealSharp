namespace UnrealSharp.Utils.UnrealEngine
{
    /// <summary>
    /// Class UCLASSAttribute.
    /// https://docs.unrealengine.com/5.3/en-US/class-specifiers/
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface|AttributeTargets.Class)]
    public class UCLASSAttribute : UUnrealAttribute<EClassFlags>
    {
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public string? Config { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UCLASSAttribute"/> class.
        /// </summary>
        public UCLASSAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UCLASSAttribute"/> class.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public UCLASSAttribute(EClassFlags flags) : base(flags)
        {
        }
    }
}
