namespace UnrealSharp.Utils.UnrealEngine
{
    /// <summary>
    /// Class UMETAAttribute.
    /// Implements the <see cref="Attribute" />
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class UMETAAttribute : Attribute
    {
        /// <summary>
        /// The name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The value
        /// </summary>
        public readonly string Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="UMETAAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public UMETAAttribute(string name)
        {
            Name = name;
            Value = "";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UMETAAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public UMETAAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UMETAAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public UMETAAttribute(string name, bool value)
        {
            Name = name;
            Value = value.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UMETAAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public UMETAAttribute(string name, int value)
        {
            Name = name;
            Value = value.ToString();
        }
    }

    /// <summary>
    /// Class ToolTipAttribute.
    /// Implements the <see cref="UnrealSharp.Utils.UnrealEngine.UMETAAttribute" />
    /// </summary>
    /// <seealso cref="UnrealSharp.Utils.UnrealEngine.UMETAAttribute" />
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class ToolTipAttribute : UMETAAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolTipAttribute" /> class.
        /// </summary>
        /// <param name="tooltip">The tooltip.</param>
        public ToolTipAttribute(string tooltip) :
            base(MetaConstants.ToolTip, tooltip)
        {
        }
    }

    /// <summary>
    /// Class MetaConstants.
    /// </summary>
    public static class MetaConstants
    {
        // @reference https://docs.unrealengine.com/5.3/en-US/class-specifiers/
        #region Classes
        /// <summary>
        /// The blueprint spawnable component
        /// </summary>
        public const string BlueprintSpawnableComponent = nameof(BlueprintSpawnableComponent);
        /// <summary>
        /// The blueprint thread safe
        /// </summary>
        public const string BlueprintThreadSafe = nameof(BlueprintThreadSafe);
        /// <summary>
        /// The child cannot tick
        /// </summary>
        public const string ChildCannotTick = nameof(ChildCannotTick);
        /// <summary>
        /// The child can tick
        /// </summary>
        public const string ChildCanTick = nameof(ChildCanTick);
        /// <summary>
        /// The deprecated node
        /// </summary>
        public const string DeprecatedNode = nameof(DeprecatedNode);
        /// <summary>
        /// The deprecation message
        /// </summary>
        public const string DeprecationMessage = nameof(DeprecationMessage);
        /// <summary>
        /// The display name
        /// </summary>
        public const string DisplayName = nameof(DisplayName);
        /// <summary>
        /// The dont use generic spawn object
        /// </summary>
        public const string DontUseGenericSpawnObject = nameof(DontUseGenericSpawnObject);
        /// <summary>
        /// The exposed asynchronous proxy
        /// </summary>
        public const string ExposedAsyncProxy = nameof(ExposedAsyncProxy);
        /// <summary>
        /// The ignore category keywords in subclasses
        /// </summary>
        public const string IgnoreCategoryKeywordsInSubclasses = nameof(IgnoreCategoryKeywordsInSubclasses);
        /// <summary>
        /// The is blueprint base
        /// </summary>
        public const string IsBlueprintBase = nameof(IsBlueprintBase);

        /// <summary>
        /// The kismet hide overrides
        /// </summary>
        public const string KismetHideOverrides = nameof(KismetHideOverrides);
        /// <summary>
        /// The prohibited interfaces
        /// </summary>
        public const string ProhibitedInterfaces = nameof(ProhibitedInterfaces);
        /// <summary>
        /// The restricted to classes
        /// </summary>
        public const string RestrictedToClasses = nameof(RestrictedToClasses);
        /// <summary>
        /// The short tool tip
        /// </summary>
        public const string ShortToolTip = nameof(ShortToolTip);
        /// <summary>
        /// The show world context pin
        /// </summary>
        public const string ShowWorldContextPin = nameof(ShowWorldContextPin);
        /// <summary>
        /// The uses hierarchy
        /// </summary>
        public const string UsesHierarchy = nameof(UsesHierarchy);
        /// <summary>
        /// The tool tip
        /// </summary>
        public const string ToolTip = nameof(ToolTip);
        /// <summary>
        /// The comment
        /// </summary>
        public const string Comment = nameof(Comment);
        /// <summary>
        /// The module relative path
        /// </summary>
        public const string ModuleRelativePath = nameof(ModuleRelativePath);
        /// <summary>
        /// The bitflags
        /// </summary>
        public const string Bitflags = nameof(Bitflags);

        /// <summary>
        /// The abstract
        /// </summary>
        public const string Abstract = nameof(Abstract);
        #endregion

        // @reference https://docs.unrealengine.com/5.3/en-US/ufunctions-in-unreal-engine/
        #region Functions
        /// <summary>
        /// The advanced display
        /// </summary>
        public const string AdvancedDisplay = nameof(AdvancedDisplay);
        /// <summary>
        /// The array parm
        /// </summary>
        public const string ArrayParm = nameof(ArrayParm);
        /// <summary>
        /// The array type dependent parameters
        /// </summary>
        public const string ArrayTypeDependentParams = nameof(ArrayTypeDependentParams);
        /// <summary>
        /// The automatic create reference term
        /// </summary>
        public const string AutoCreateRefTerm = nameof(AutoCreateRefTerm);
        /// <summary>
        /// The blueprint autocast
        /// </summary>
        public const string BlueprintAutocast = nameof(BlueprintAutocast);
        /// <summary>
        /// The blueprint internal use only
        /// </summary>
        public const string BlueprintInternalUseOnly = nameof(BlueprintInternalUseOnly);
        /// <summary>
        /// The blueprint protected
        /// </summary>
        public const string BlueprintProtected = nameof(BlueprintProtected);
        /// <summary>
        /// The callable without world context
        /// </summary>
        public const string CallableWithoutWorldContext = nameof(CallableWithoutWorldContext);
        /// <summary>
        /// The commutative associative binary operator
        /// </summary>
        public const string CommutativeAssociativeBinaryOperator = nameof(CommutativeAssociativeBinaryOperator);
        /// <summary>
        /// The compact node title
        /// </summary>
        public const string CompactNodeTitle = nameof(CompactNodeTitle);
        /// <summary>
        /// The custom structure parameter
        /// </summary>
        public const string CustomStructureParam = nameof(CustomStructureParam);
        /// <summary>
        /// The default to self
        /// </summary>
        public const string DefaultToSelf = nameof(DefaultToSelf);
        /// <summary>
        /// The deprecated function
        /// </summary>
        public const string DeprecatedFunction = nameof(DeprecatedFunction);
        /// <summary>
        /// The determines output type
        /// </summary>
        public const string DeterminesOutputType = nameof(DeterminesOutputType);
        /// <summary>
        /// The development only
        /// </summary>
        public const string DevelopmentOnly = nameof(DevelopmentOnly);
        /// <summary>
        /// The expand enum as execs
        /// </summary>
        public const string ExpandEnumAsExecs = nameof(ExpandEnumAsExecs);
        /// <summary>
        /// The hide pin
        /// </summary>
        public const string HidePin = nameof(HidePin);
        /// <summary>
        /// The hide self pin
        /// </summary>
        public const string HideSelfPin = nameof(HideSelfPin);
        /// <summary>
        /// The internal use parameter
        /// </summary>
        public const string InternalUseParam = nameof(InternalUseParam);
        /// <summary>
        /// The key words
        /// </summary>
        public const string KeyWords = nameof(KeyWords);
        /// <summary>
        /// The latent
        /// </summary>
        public const string Latent = nameof(Latent);
        /// <summary>
        /// The latent information
        /// </summary>
        public const string LatentInfo = nameof(LatentInfo);
        /// <summary>
        /// The material parameter collection function
        /// </summary>
        public const string MaterialParameterCollectionFunction = nameof(MaterialParameterCollectionFunction);
        /// <summary>
        /// The native break function
        /// </summary>
        public const string NativeBreakFunc = nameof(NativeBreakFunc);
        /// <summary>
        /// The not blueprint thread safe
        /// </summary>
        public const string NotBlueprintThreadSafe = nameof(NotBlueprintThreadSafe);
        /// <summary>
        /// The unsafe during actor construction
        /// </summary>
        public const string UnsafeDuringActorConstruction = nameof(UnsafeDuringActorConstruction);
        /// <summary>
        /// The world context
        /// </summary>
        public const string WorldContext = nameof(WorldContext);
        /// <summary>
        /// The category
        /// </summary>
        public const string Category = nameof(Category);
        /// <summary>
        /// The allow private access
        /// </summary>
        public const string AllowPrivateAccess = nameof(AllowPrivateAccess);
        #endregion

        #region Properties
        /// <summary>
        /// The allow abstract
        /// </summary>
        public const string AllowAbstract = nameof(AllowAbstract);
        #endregion
    }
}
