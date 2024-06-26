namespace UnrealSharp.Utils.UnrealEngine
{
    /// <summary>
    /// Class FastAccessableAttribute.
    /// Implements the <see cref="System.Attribute" />
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Struct|AttributeTargets.Enum|AttributeTargets.Method)]
    public class FastAccessableAttribute : Attribute
    {
        /// <summary>
        /// The native size
        /// </summary>
        public readonly int NativeSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="FastAccessableAttribute"/> class.
        /// </summary>
        public FastAccessableAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FastAccessableAttribute"/> class.
        /// </summary>
        /// <param name="nativeSize">Size of the native.</param>
        public FastAccessableAttribute(int nativeSize)
        {
            NativeSize = nativeSize;
        }
    }
}
