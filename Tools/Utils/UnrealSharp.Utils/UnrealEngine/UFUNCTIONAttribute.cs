namespace UnrealSharp.Utils.UnrealEngine
{
    /// <summary>
    /// Enum FunctionReplicateType
    /// </summary>
    public enum FunctionReplicateType
    {
        /// <summary>
        /// The not replicated
        /// </summary>
        NotReplicated = 0,

        /// <summary>
        /// The multicast
        /// </summary>
        Multicast = (int)EFunctionFlags.NetMulticast|(int)EFunctionFlags.Net,

        /// <summary>
        /// The client
        /// </summary>
        Client = (int)EFunctionFlags.NetClient | (int)EFunctionFlags.Net,

        /// <summary>
        /// The server
        /// </summary>
        Server = (int)EFunctionFlags.NetServer | (int)EFunctionFlags.Net,
    }

    /// <summary>
    /// Class UFUNCTIONAttribute.
    /// https://docs.unrealengine.com/5.3/en-US/ufunctions-in-unreal-engine/
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UFUNCTIONAttribute : UUnrealAttribute<EFunctionFlags>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UFUNCTIONAttribute"/> class.
        /// </summary>
        public UFUNCTIONAttribute() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="UFUNCTIONAttribute"/> class.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public UFUNCTIONAttribute(EFunctionFlags flags) : base(flags)
        {
        }
    }

    /// <summary>
    /// Class UEVENTAttribute.
    /// Implements the <see cref="UnrealSharp.Utils.UnrealEngine.UFUNCTIONAttribute" />
    /// </summary>
    /// <seealso cref="UnrealSharp.Utils.UnrealEngine.UFUNCTIONAttribute" />
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UEVENTAttribute : UFUNCTIONAttribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is reliable.
        /// </summary>
        /// <value><c>true</c> if this instance is reliable; otherwise, <c>false</c>.</value>
        public bool IsReliable { get; set; }

        /// <summary>
        /// Gets or sets the replicates.
        /// </summary>
        /// <value>The replicates.</value>
        public FunctionReplicateType Replicates { get; set; } = FunctionReplicateType.NotReplicated;

        /// <summary>
        /// Initializes a new instance of the <see cref="UEVENTAttribute"/> class.
        /// </summary>
        public UEVENTAttribute() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="UEVENTAttribute"/> class.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public UEVENTAttribute(EFunctionFlags flags) 
            : base(flags |= (EFunctionFlags.BlueprintEvent|EFunctionFlags.Event))
        {
        }
    }
}
