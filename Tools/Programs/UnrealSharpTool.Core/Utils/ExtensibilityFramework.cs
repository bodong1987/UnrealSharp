using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.Serialization;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;

namespace UnrealSharpTool.Core.Utils;

#region Interfaces
/// <summary>
/// Class ExportIgnoreAttribute.
/// Implements the <see cref="System.Attribute" />
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
public class ExportIgnoreAttribute : Attribute;

/// <summary>
/// Interface IComposableTarget
/// </summary>
public interface IComposableTarget
{
    /// <summary>
    /// Gets the external parts.
    /// </summary>
    /// <value>The external parts.</value>
    IEnumerable<IImportable> ExternalParts { get; }

    /// <summary>
    /// Called when [composed].
    /// </summary>
    /// <param name="importable">The importable.</param>
    void OnComposed(IImportable importable);

    /// <summary>
    /// Called when [decomposing].
    /// </summary>
    /// <param name="importable">The importable.</param>
    void OnDecomposing(IImportable importable);

    /// <summary>
    /// Called when [compose finished].
    /// </summary>
    void OnComposeFinished();

    /// <summary>
    /// Called when [decompose finished].
    /// </summary>
    void OnDecomposeFinished();
}

/// <summary>
/// Interface IImportable
/// </summary>
[ExportIgnore]
public interface IImportable
{
    /// <summary>
    /// Gets the import priority.
    /// </summary>
    /// <value>The import priority.</value>
    [Browsable(false)]
    int ImportPriority { get; }

    /// <summary>
    /// Accepts the specified access token.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <returns><c>true</c> if this instance accept target token, <c>false</c> otherwise.</returns>
    // ReSharper disable once UnusedParameter.Global
    bool Accept(object accessToken);
}

/// <summary>
/// Class AbstractImportable.
/// </summary>
[ExportIgnore]
public abstract class AbstractImportable : IImportable
{
    /// <summary>
    /// The internal default priority
    /// </summary>
    public const int InternalDefaultPriority = 100;

    /// <summary>
    /// Gets the import priority.
    /// </summary>
    /// <value>The import priority.</value>
    [Browsable(false)]
    [XmlIgnore]
    public virtual int ImportPriority => InternalDefaultPriority;

    /// <summary>
    /// Accepts the specified access token.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <returns><c>true</c> if this instance accept target token, <c>false</c> otherwise.</returns>
    public virtual bool Accept(object accessToken)
    {
        return true;
    }

    /// <summary>
    /// Posts the composed.
    /// </summary>
    /// <param name="host">The host.</param>
    // ReSharper disable once VirtualMemberNeverOverridden.Global
    // ReSharper disable once UnusedParameter.Global
    public virtual void OnPostComposedTo(IComposableTarget host)
    {
    }

    /// <summary>
    /// Pres the decompose.
    /// </summary>
    /// <param name="host">The host.</param>
    // ReSharper disable once VirtualMemberNeverOverridden.Global
    // ReSharper disable once UnusedParameter.Global
    public virtual void OnPreComposingFrom(IComposableTarget host)
    {
    }
}


/// <summary>
/// Class AbstractComposableTarget.
/// </summary>
public abstract class AbstractComposableTarget : IComposableTarget
{
    /// <summary>
    /// The external parts core
    /// </summary>
    [NonSerialized]
    private readonly List<IImportable> _externalPartsCore = [];

    /// <summary>
    /// Gets the external parts.
    /// </summary>
    /// <value>The external parts.</value>
    [Browsable(false)]
    [XmlIgnore]
    public virtual IEnumerable<IImportable> ExternalParts => _externalPartsCore;

    /// <summary>
    /// Called when [composed].
    /// </summary>
    /// <param name="importable">The importable.</param>
    public virtual void OnComposed(IImportable importable)
    {
        if (!_externalPartsCore.Contains(importable))
        {
            _externalPartsCore.Add(importable);
        }
    }

    /// <summary>
    /// Called when [decomposing].
    /// </summary>
    /// <param name="importable">The importable.</param>
    public virtual void OnDecomposing(IImportable importable)
    {
        _externalPartsCore.Remove(importable);
    }

    /// <summary>
    /// Called when [compose finished].
    /// </summary>
    public virtual void OnComposeFinished()
    {
    }

    /// <summary>
    /// Called when [decompose finished].
    /// </summary>
    public virtual void OnDecomposeFinished()
    {
    }


}

#endregion

#region Attributes
/// <summary>
/// Enum CreationPolicy
/// </summary>
public enum CreationPolicy
{
    /// <summary>
    /// The shared
    /// </summary>
    Shared = 1,
    /// <summary>
    /// The non shared
    /// </summary>
    NonShared = 2
}

/// <summary>
/// Class ImportAttribute.
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ImportAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the contract.
    /// </summary>
    /// <value>The name of the contract.</value>
    public string ContractName { get; }

    /// <summary>
    /// Gets or sets the type of the contract.
    /// </summary>
    /// <value>The type of the contract.</value>
    public Type? ContractType { get; }

    /// <summary>
    /// Gets or sets the required creation policy.
    /// </summary>
    /// <value>The required creation policy.</value>
    public CreationPolicy RequiredCreationPolicy { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportAttribute" /> class.
    /// </summary>
    /// <param name="contractName">Name of the contract.</param>
    /// <param name="contractType">Type of the contract.</param>
    /// <param name="requiredCreationPolicy">The required creation policy.</param>
    public ImportAttribute(string contractName, Type? contractType = null, CreationPolicy requiredCreationPolicy = CreationPolicy.NonShared)
    {
        ContractName = contractName;
        ContractType = contractType;
        RequiredCreationPolicy = requiredCreationPolicy;
    }
}

/// <summary>
/// Class ExportAttribute.
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class ExportAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the contract.
    /// </summary>
    /// <value>The name of the contract.</value>
    public string ContractName { get; } = "";

    /// <summary>
    /// Gets the type of the contract.
    /// </summary>
    /// <value>The type of the contract.</value>
    public Type? ContractType { get; }

    /// <summary>
    /// Gets or sets the required creation policy.
    /// </summary>
    /// <value>The required creation policy.</value>
    public CreationPolicy RequiredCreationPolicy { get; } = CreationPolicy.NonShared;

    /// <summary>
    /// Gets a value indicating whether this instance has contract type.
    /// </summary>
    /// <value><c>true</c> if this instance has contract type; otherwise, <c>false</c>.</value>

    public bool HasContractType => ContractType != null;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportAttribute"/> class.
    /// </summary>
    public ExportAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportAttribute"/> class.
    /// </summary>
    /// <param name="contractName">Name of the contract.</param>
    public ExportAttribute(string contractName)
    {
        ContractName = contractName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportAttribute" /> class.
    /// </summary>
    /// <param name="contractName">Name of the contract.</param>
    /// <param name="contractType">Type of the contract.</param>
    /// <param name="requiredCreationPolicy">The required creation policy.</param>
    public ExportAttribute(string contractName, Type contractType, CreationPolicy requiredCreationPolicy = CreationPolicy.NonShared) :
        this(contractName)
    {
        ContractType = contractType;
        RequiredCreationPolicy = requiredCreationPolicy;
    }
}
#endregion

#region Parts and Catalogs
/// <summary>
/// Class ExportPart.
/// </summary>
public class ExportPart
{
    /// <summary>
    /// The attribute
    /// </summary>
    public readonly ExportAttribute Attribute;

    /// <summary>
    /// The type
    /// </summary>
    public readonly Type Type;

    /// <summary>
    /// Gets or sets the default object.
    /// </summary>
    /// <value>The default object.</value>
    [Browsable(false)]
    public object? DefaultObject { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether this instance has default object.
    /// </summary>
    /// <value><c>true</c> if this instance has default object; otherwise, <c>false</c>.</value>
    [Browsable(false)]
    public bool HasDefaultObject => DefaultObject != null;

    /// <summary>
    /// Gets the source assembly.
    /// </summary>
    /// <value>The source assembly.</value>
    [Browsable(false)] 
    public readonly Assembly SourceAssembly;

    /// <summary>
    /// Gets the type of the contract.
    /// </summary>
    /// <value>The type of the contract.</value>
    [Browsable(false)]
    public Type? ContractType => Attribute.ContractType;

    /// <summary>
    /// The contract names core
    /// </summary>
    private readonly List<string> _contractNamesCore = [];

    /// <summary>
    /// Gets the contract names.
    /// </summary>
    /// <value>The contract names.</value>
    [Browsable(false)]
    public string[] ContractNames => _contractNamesCore.ToArray();

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportPart" /> class.
    /// </summary>
    /// <param name="attribute">The attribute.</param>
    /// <param name="type">The type.</param>
    /// <param name="assembly">The assembly.</param>
    /// <exception cref="Exception"></exception>
    public ExportPart(ExportAttribute attribute, Type type, Assembly assembly)
    {
        Attribute = attribute;
        Type = type;
        SourceAssembly = assembly;

        Logger.Assert(Attribute.ContractType == null || Attribute.ContractType.IsAssignableFrom(type));

        if (Attribute.ContractType != null && !Attribute.ContractType.IsAssignableFrom(type))
        {
            throw new Exception($"export type {Type.FullName} must implement {ContractType!.FullName!}");
        }

        if (attribute.ContractName.IsNotNullOrEmpty())
        {
            _contractNamesCore.Add(attribute.ContractName);
        }
        else
        {
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (!typeof(IImportable).IsAssignableFrom(interfaceType) ||
                    interfaceType.IsDefined<ExportIgnoreAttribute>())
                {
                    continue;
                }

                _contractNamesCore.Add(interfaceType.FullName!);
            }
        }
    }

    /// <summary>
    /// Creates the part object.
    /// </summary>
    /// <param name="requiredCreationPolicy">The required creation policy.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>System.Object.</returns>
    public object? CreatePartObject(CreationPolicy requiredCreationPolicy, params object[] parameters)
    {
        if (requiredCreationPolicy == CreationPolicy.Shared ||
            Attribute.RequiredCreationPolicy == CreationPolicy.Shared)
        {
            if (!HasDefaultObject)
            {
                CreateDefaultObject(parameters);
            }

            return DefaultObject;
        }

        return Activator.CreateInstance(Type, parameters);
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
    /// </summary>
    /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is ExportPart part && part.Attribute.ContractName == Attribute.ContractName && part.Type == Type;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode()
    {
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        return base.GetHashCode();
    }

    /// <summary>
    /// Creates the default object.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    private void CreateDefaultObject(params object[] parameters)
    {
        DefaultObject = Activator.CreateInstance(Type, parameters);
    }
}

/// <summary>
/// Class Catalog.
/// </summary>
public class Catalog
{
    /// <summary>
    /// The parts private
    /// </summary>
    protected readonly List<ExportPart> PartsPrivate = [];

    /// <summary>
    /// Gets the parts.
    /// </summary>
    /// <value>The parts.</value>
    public ExportPart[] Parts => PartsPrivate.ToArray();

    /// <summary>
    /// Adds the part.
    /// </summary>
    /// <param name="part">The part.</param>
    // ReSharper disable once MemberCanBeProtected.Global
    public void AddPart(ExportPart part)
    {
        if (!PartsPrivate.Contains(part))
        {
            PartsPrivate.Add(part);
        }
    }

    /// <summary>
    /// Removes the part.
    /// </summary>
    /// <param name="part">The part.</param>
    public void RemovePart(ExportPart part)
    {
        PartsPrivate.Remove(part);
    }
}

/// <summary>
/// Class AssemblyCatalog.
/// </summary>
public class AssemblyCatalog : Catalog
{
    /// <summary>
    /// Gets or sets the assembly.
    /// </summary>
    /// <value>The assembly.</value>
    public readonly Assembly Assembly;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyCatalog"/> class.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    public AssemblyCatalog(Assembly assembly)
    {
        Assembly = assembly;

        foreach (var t in Assembly.GetTypes())
        {
            if (t is { IsClass: true, IsAbstract: false } && t.IsDefined<ExportAttribute>())
            {
                var attr = t.GetAnyCustomAttribute<ExportAttribute>()!;

                if (!t.GetInterfaces().Contains(typeof(IImportable)))
                {
                    Logger.Assert(false, "Failed add export part:{0} must implement from IImportable!", t.FullName!);
                    continue;
                }

                AddPart(new ExportPart(attr, t, Assembly)); //-V3080
            }
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is AssemblyCatalog catalog)
        {
            return catalog.Assembly == Assembly;
        }

        return false;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode()
    {
        return Assembly.GetHashCode();
    }
}
#endregion

#region Extensibility Framework
/// <summary>
/// Class ExtensibilityFramework.
/// </summary>    
public static class ExtensibilityFramework
{
    #region Fields
    /// <summary>
    /// The catalogs
    /// </summary>
    private static readonly List<Catalog> Catalogs = [];

    /// <summary>
    /// The part changed
    /// </summary>
    // ReSharper disable once UnassignedField.Global
#pragma warning disable CA2211
    public static EventHandler? PartChanged;
#pragma warning restore CA2211

    /// <summary>
    /// The export parts
    /// </summary>
    private static readonly Dictionary<string, List<ExportPart>> ExportParts = new();
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes static members of the <see cref="ExtensibilityFramework"/> class.
    /// </summary>
    static ExtensibilityFramework()
    {
        AddPart(typeof(ExtensibilityFramework).Assembly);
    }
    #endregion

    #region Base Interfaces
    /// <summary>
    /// Determines whether the specified assembly has part.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns><c>true</c> if the specified assembly has part; otherwise, <c>false</c>.</returns>
    public static bool HasPart(Assembly assembly)
    {
        return Catalogs.Find(
                   x => x is AssemblyCatalog catalog && catalog.Assembly == assembly)
               != null;
    }

    /// <summary>
    /// Determines whether the specified contract name has part.
    /// </summary>
    /// <param name="contractName">Name of the contract.</param>
    /// <returns><c>true</c> if the specified contract name has part; otherwise, <c>false</c>.</returns>
    public static bool HasPart(string contractName)
    {
        return ExportParts.ContainsKey(contractName);
    }

    /// <summary>
    /// Adds the part.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    public static void AddPart(Assembly assembly)
    {
        if (!HasPart(assembly))
        {
            var catalog = new AssemblyCatalog(assembly);

            Catalogs.Add(catalog);

            foreach (var part in catalog.Parts)
            {
                foreach (var name in part.ContractNames)
                {
                    if (!ExportParts.TryGetValue(name, out var targets))
                    {
                        targets = [];
                        ExportParts.Add(name, targets);
                    }

                    targets.Add(part);
                }
            }

            PartChanged?.Invoke(typeof(ExtensibilityFramework), EventArgs.Empty);
        }
    }

    /// <summary>
    /// Adds the parts.
    /// </summary>
    /// <param name="assemblies">The assemblies.</param>
    public static void AddParts(Assembly[] assemblies)
    {
        foreach (var asm in assemblies)
        {
            AddPart(asm);
        }
    }

    /// <summary>
    /// Removes the part.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    public static void RemovePart(Assembly assembly)
    {
        var catalogs = Catalogs.FindAll(x => x is AssemblyCatalog catalog && catalog.Assembly == assembly);

        foreach (var catalog in catalogs)
        {
            foreach (var part in catalog.Parts)
            {
                foreach (var contractName in part.ContractNames)
                {
                    if (ExportParts.TryGetValue(contractName, out var targets))
                    {
                        targets.Remove(part);
                    }

                    if (targets == null || targets.Count == 0)
                    {
                        ExportParts.Remove(contractName);
                    }
                }
            }
        }

        var val = Catalogs.RemoveAll(x => x is AssemblyCatalog catalog && catalog.Assembly == assembly);

        if (val > 0)
        {
            PartChanged?.Invoke(typeof(ExtensibilityFramework), EventArgs.Empty);
        }
    }
    #endregion

    #region Get Export Parts
    /// <summary>
    /// Gets the export parts.
    /// </summary>
    /// <param name="contractName">The contractName.</param>
    /// <returns>ExportPart[].</returns>
    public static ExportPart[]? GetExportParts(string contractName)
    {
        return ExportParts.TryGetValue(contractName, out var parts) ? parts.ToArray() : null;
    }

    /// <summary>
    /// Gets the assemblies.
    /// </summary>
    /// <returns>Assembly[].</returns>
    public static Assembly[] GetAssemblies()
    {
        return Catalogs.FindAll(x => x is AssemblyCatalog).Cast<AssemblyCatalog>().Select(x => x.Assembly).ToArray();
    }
    #endregion

    #region Compose
    /// <summary>
    /// Composes the parts.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target">The target.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="args">The arguments.</param>
    [RequiresDynamicCode("Call ComposePartsCore")]
    public static void ComposeParts<T>(T target, object? accessToken, params object[] args)
        where T : IComposableTarget
    {
        ComposePartsCore(target, accessToken ?? target, args);
    }

    /// <summary>
    /// Composes the parts core.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target">The target.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="args">The arguments.</param>
    [RequiresDynamicCode("Calls UnrealSharpTool.Core.Utils.ExtensibilityFramework.ComposeMember<T>(T, Object, MemberInfo, ImportAttribute, params Object[])")]
    private static void ComposePartsCore<T>(T target, object accessToken, params object[] args)
        where T : IComposableTarget
    {
        var type = target.GetType();

        foreach (var m in type.GetMembers())
        {
            if (m.MemberType is MemberTypes.Property or MemberTypes.Field
                && m.IsDefined<ImportAttribute>()
               )
            {
                var attr = m.GetAnyCustomAttribute<ImportAttribute>();

                ComposeMember(target, accessToken, m, attr!, args); //-V3080
            }
        }

        target.OnComposeFinished();
    }

    /// <summary>
    /// Composes the member.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target">The target.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="memberInfo">The member information.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="args">The arguments.</param>
    /// <exception cref="ArgumentException"></exception>
    [RequiresDynamicCode("Calls UnrealSharpTool.Core.Utils.ExtensibilityFramework.CheckContractType(MemberInfo, ImportAttribute, out Type)")]
    private static void ComposeMember<T>(T target, object accessToken, MemberInfo memberInfo, ImportAttribute attribute, params object[] args)
        where T : IComposableTarget
    {
        var interfaceType = CheckContractType(memberInfo, attribute, out var listType);

        var list = (Activator.CreateInstance(listType!) as IList)!;
            
        ComposeList(list, interfaceType, attribute.ContractName, attribute.RequiredCreationPolicy, accessToken, args);

        var memberType = memberInfo.GetUnderlyingType();
        var isManyImports = memberType.IsImplementFrom<IList>();

        if (!isManyImports)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (list.Count > 1)
            {
                var errorMessage = $"ComposeError: Too more import parts for {memberInfo.DeclaringType!.FullName}:{memberInfo.Name}";
                Logger.LogError(errorMessage);

                throw new ArgumentException(errorMessage);
            }

            if (list.Count == 1)
            {
                var importable = (list[0] as IImportable)!;
                    
                memberInfo.SetUnderlyingValue(target, list[0]);
                target.OnComposed(importable);

                if (importable is AbstractImportable ai)
                {
                    ai.OnPostComposedTo(target);
                }
            }
        }
        else
        {
            // many imports...
            if (memberInfo.GetUnderlyingValue(target) is not IList existsList)
            {
                if (!memberType.IsInstanceOfType(list))
                {
                    if (memberType.IsAbstract)
                    {
                        var errorMessage = string.Format("ComposeError: Can't create {0} or convert {1} to {0}", memberType.FullName, list.GetType().FullName);
                        Logger.LogError(errorMessage);
                        throw new ArgumentException(errorMessage);
                    }

                    existsList = (Activator.CreateInstance(memberType) as IList)!;
                            
                    memberType.SetUnderlyingValue(target, existsList);
                }
                else
                {
                    existsList = list;
                    memberType.SetUnderlyingValue(target, existsList);
                }
            }

            if (!Equals(existsList, list))
            {
                foreach (var i in list)
                {
                    existsList.Add(i);
                }
            }

            foreach (IImportable importable in existsList)
            {
                target.OnComposed(importable);

                if (importable is AbstractImportable ai)
                {
                    ai.OnPostComposedTo(target);
                }
            }
        }
    }

    /// <summary>
    /// Checks the type of the contract.
    /// </summary>
    /// <param name="memberInfo">The member information.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="listType">Type of the list.</param>
    /// <returns>Type.</returns>
    /// <exception cref="ArgumentException"></exception>
    [RequiresDynamicCode("Calls System.Type.MakeGenericType(params Type[])")]
    private static Type CheckContractType(MemberInfo memberInfo, ImportAttribute attribute, out Type? listType)
    {
        var memberType = memberInfo.GetUnderlyingType();
        var isManyImports = memberType.IsImplementFrom<IList>();
        listType = null;

        var declareContractType = isManyImports ? memberType.GetGenericArguments()[0] : memberType;

        if (!declareContractType.IsImplementFrom<IImportable>())
        {
            var errorMessage = $"ContractError: ContractType must be implement from IImportable.{Environment.NewLine} Error Fields:{memberInfo.DeclaringType!.FullName}:{memberInfo.Name}";
            Logger.LogError(errorMessage);
            throw new ArgumentException(errorMessage);
        }

        if (attribute.ContractType != null && !attribute.ContractType.IsAssignableFrom(declareContractType))
        {
            var errorMessage = $"ContractError: ContractType must be implement from {attribute.ContractType.FullName}.{Environment.NewLine} Error Fields:{memberInfo.DeclaringType!.FullName}:{memberInfo.Name}";
            Logger.LogError(errorMessage);
            throw new ArgumentException(errorMessage);
        }

        listType = typeof(List<>).MakeGenericType(declareContractType);

        return declareContractType;
    }

    /// <summary>
    /// Composes the list.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="contractType">Type of the contract.</param>
    /// <param name="contractName">Name of the contract.</param>
    /// <param name="requiredCreationPolicy">The required creation policy.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="args">The arguments.</param>
    private static void ComposeList(
        IList list,
        Type contractType,
        string contractName,
        CreationPolicy requiredCreationPolicy,
        object accessToken,
        params object[] args
    )
    {
        var results = new List<IImportable>();
        if (ExportParts.TryGetValue(contractName, out var targetParts))
        {
            foreach (var part in targetParts.Where(part => (part.ContractType != null && contractType.IsAssignableFrom(part.ContractType)) ||
                                                           (part.ContractType == null && contractType.IsAssignableFrom(part.Type))))
            {
                try
                {
                    var obj = part.CreatePartObject(requiredCreationPolicy, args) as IImportable;

                    Logger.Assert(obj != null);

                    if (obj!.Accept(accessToken))
                    {
                        results.Add(obj);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed create import part: {0}=>{1}, targetType = {2}\n {3}\n{4}", contractName, part.ContractType!.FullName!, part.Type.FullName!, e.Message, e.StackTrace!);
#if DEBUG
                    throw;
#endif
                }
            }
        }

        SortImportable(ref results);

        foreach (var t in results)
        {
            list.Add(t);
        }
    }

    /// <summary>
    /// Sorts the importable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list.</param>
    internal static void SortImportable<T>(ref List<T> list)
        where T : class, IImportable
    {
        list.Sort((x, y) => Comparer<int>.Default.Compare(x.ImportPriority, y.ImportPriority) * -1);
    }

    #endregion

    #region Decompose

    /// <summary>
    /// Decomposes the parts.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target">The target.</param>
    public static void DecomposeParts<T>(params T[] target)
        where T : IComposableTarget
    {
        foreach (var i in target)
        {
            PreDecompose(i);
        }
    }

    /// <summary>
    /// Pres the decompose.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target">The target.</param>
    private static void PreDecompose<T>(T target)
        where T : IComposableTarget
    {
        foreach (var i in target.ExternalParts.ToArray())
        {
            if (i is AbstractImportable ai)
            {
                ai.OnPreComposingFrom(target);
            }

            target.OnDecomposing(i);
        }

        target.OnDecomposeFinished();
    }
    #endregion
}
#endregion