namespace UnrealSharp.Utils.UnrealEngine
{
    /// <summary>
    /// Class BackendNameAttribute.
    /// Implements the <see cref="System.Attribute" />
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Field|AttributeTargets.Property|AttributeTargets.Class|AttributeTargets.Struct|AttributeTargets.Interface|AttributeTargets.Enum)]
    public class BackendNameAttribute : Attribute
    {
        /// <summary>
        /// The backend name
        /// </summary>
        public readonly string BackendName;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackendNameAttribute"/> class.
        /// </summary>
        /// <param name="backendNamne">The backend namne.</param>
        public BackendNameAttribute(string backendNamne)
        {
            BackendName = backendNamne;
        }
    }
}
