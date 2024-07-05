namespace UnrealSharp.Utils.UnrealEngine;

/// <summary>
/// Enum LifetimeCondition
/// </summary>
public enum LifetimeCondition
{
    /// <summary>
    /// The none
    /// </summary>
    None = 0,
    /// <summary>
    /// The initial only
    /// </summary>
    InitialOnly = 1,
    /// <summary>
    /// The owner only
    /// </summary>
    OwnerOnly = 2,
    /// <summary>
    /// The skip owner
    /// </summary>
    SkipOwner = 3,
    /// <summary>
    /// The simulated only
    /// </summary>
    SimulatedOnly = 4,
    /// <summary>
    /// The autonomous only
    /// </summary>
    AutonomousOnly = 5,
    /// <summary>
    /// The simulated or physics
    /// </summary>
    SimulatedOrPhysics = 6,
    /// <summary>
    /// The initial or owner
    /// </summary>
    InitialOrOwner = 7,
    /// <summary>
    /// The custom
    /// </summary>
    Custom = 8,
    /// <summary>
    /// The replay or owner
    /// </summary>
    ReplayOrOwner = 9,
    /// <summary>
    /// The replay only
    /// </summary>
    ReplayOnly = 10,
    /// <summary>
    /// The simulated only no replay
    /// </summary>
    SimulatedOnlyNoReplay = 11,
    /// <summary>
    /// The simulated or physics no replay
    /// </summary>
    SimulatedOrPhysicsNoReplay = 12,
    /// <summary>
    /// The skip replay
    /// </summary>
    SkipReplay = 13,
    /// <summary>
    /// The dynamic
    /// </summary>
    Dynamic = 14,
    /// <summary>
    /// The never
    /// </summary>
    Never = 15,
    /// <summary>
    /// The net group
    /// </summary>
    NetGroup = 16,
    /// <summary>
    /// Determines the maximum of the parameters.
    /// </summary>
    Max = 17
}


/// <summary>
/// Class UPROPERTYAttribute.
/// https://docs.unrealengine.com/5.3/en-US/unreal-engine-uproperties/
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
// ReSharper disable once InconsistentNaming
public class UPROPERTYAttribute : UUnrealAttribute<EPropertyFlags>
{
    /// <summary>
    /// Gets or sets a value indicating whether [allow private access].
    /// </summary>
    /// <value><c>true</c> if [allow private access]; otherwise, <c>false</c>.</value>
    public bool AllowPrivateAccess { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is replicated.
    /// Used to specify whether a variable is replicated
    /// </summary>
    /// <value><c>true</c> if this instance is replicated; otherwise, <c>false</c>.</value>
    public bool IsReplicated { get; set; }

    /// <summary>
    /// Gets or sets the replicated using.
    /// </summary>
    /// <value>The replicated using.</value>
    public string? ReplicatedUsing { get; set; }

    /// <summary>
    /// Gets or sets the replication condition.
    /// </summary>
    /// <value>The replication condition.</value>
    public LifetimeCondition ReplicationCondition { get; set; } = LifetimeCondition.None;

    /// <summary>
    /// Gets or sets a value indicating whether this instance is actor component.
    /// If you set this property, then this property will be treated as a Component attached to the Actor. 
    /// Please note: This field can only be used on attributes of ActorComponent subclasses, otherwise it will have no effect.
    /// </summary>
    /// <value><c>true</c> if this instance is actor component; otherwise, <c>false</c>.</value>
    public bool IsActorComponent { get; set; }

    /// <summary>
    /// set the attachment target component name.
    /// @warning: Please note that the name of the Component is filled in here, not the name of the Field.
    /// You can choose to attach a SceneComponent to another component through the field.
    /// Note that this property can only be used on the properties of the SceneComponent subclass, otherwise it will have no effect.
    /// </summary>
    /// <value>which component do you want to attach</value>
    public string? AttachToComponentName { get; set; }

    /// <summary>
    /// Gets or sets the name of the target socket
    /// </summary>
    /// <value>The name of the target socket.</value>
    public string? AttachToSocketName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UPROPERTYAttribute"/> class.
    /// </summary>
    public UPROPERTYAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UPROPERTYAttribute"/> class.
    /// </summary>
    /// <param name="flags">The flags.</param>
    public UPROPERTYAttribute(EPropertyFlags flags) : base(flags)
    {
    }
}