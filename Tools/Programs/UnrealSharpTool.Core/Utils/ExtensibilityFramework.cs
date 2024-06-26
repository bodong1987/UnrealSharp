using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.Serialization;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;

namespace UnrealSharpTool.Core.Utils
{
    #region Interfaces
    /// <summary>
    /// Class ExportIgnoreAttribute.
    /// Implements the <see cref="System.Attribute" />
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false)]
    public class ExportIgnoreAttribute : Attribute
    {
    }


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
    [ExportIgnoreAttribute]
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
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool Accept(object accessToken);
    }

    /// <summary>
    /// Class AbstractImportable.
    /// Implements the <see cref="BuildPipeline.Core.Framework.IImportable" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Framework.IImportable" />
    [ExportIgnoreAttribute]
    public abstract class AbstractImportable : IImportable
    {
        /// <summary>
        /// The internal default priority
        /// </summary>
        public static readonly int InternalDefaultPriority = 100;

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
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public virtual bool Accept(object accessToken)
        {
            return true;
        }

        /// <summary>
        /// Posts the composed.
        /// </summary>
        /// <param name="host">The host.</param>
        public virtual void OnPostComposedTo(IComposableTarget host)
        {
        }

        /// <summary>
        /// Pres the decompose.
        /// </summary>
        /// <param name="host">The host.</param>
        public virtual void OnPreComposingFrom(IComposableTarget host)
        {
        }
    }


    /// <summary>
    /// Class AbstractComposableTarget.
    /// Implements the <see cref="BuildPipeline.Core.Framework.IComposableTarget" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Framework.IComposableTarget" />
    public abstract class AbstractComposableTarget : IComposableTarget
    {
        /// <summary>
        /// The external parts core
        /// </summary>
        [NonSerialized]
        private List<IImportable> ExternalPartsCore = new List<IImportable>();

        /// <summary>
        /// Gets the external parts.
        /// </summary>
        /// <value>The external parts.</value>
        [Browsable(false)]
        [XmlIgnore]
        public virtual IEnumerable<IImportable> ExternalParts => ExternalPartsCore;

        /// <summary>
        /// Called when [composed].
        /// </summary>
        /// <param name="importable">The importable.</param>
        public virtual void OnComposed(IImportable importable)
        {
            if (!ExternalPartsCore.Contains(importable))
            {
                ExternalPartsCore.Add(importable);
            }
        }

        /// <summary>
        /// Called when [decomposing].
        /// </summary>
        /// <param name="importable">The importable.</param>
        public virtual void OnDecomposing(IImportable importable)
        {
            ExternalPartsCore.Remove(importable);
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
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ImportAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the contract.
        /// </summary>
        /// <value>The name of the contract.</value>
        public string ContractName { get; protected set; }

        /// <summary>
        /// Gets or sets the type of the contract.
        /// </summary>
        /// <value>The type of the contract.</value>
        public Type? ContractType { get; protected set; }

        /// <summary>
        /// Gets or sets the required creation policy.
        /// </summary>
        /// <value>The required creation policy.</value>
        public CreationPolicy RequiredCreationPolicy { get; set; } = CreationPolicy.NonShared;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportAttribute"/> class.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        public ImportAttribute(string contractName) :
            this(contractName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportAttribute" /> class.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        /// <param name="contractType">Type of the contract.</param>
        /// <param name="requiredCreationPolicy">The required creation policy.</param>
        public ImportAttribute(string contractName, Type? contractType, CreationPolicy requiredCreationPolicy = CreationPolicy.NonShared)
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
        public string ContractName { get; protected set; } = "";

        /// <summary>
        /// Gets the type of the contract.
        /// </summary>
        /// <value>The type of the contract.</value>
        public Type? ContractType { get; private set; }

        /// <summary>
        /// Gets or sets the required creation policy.
        /// </summary>
        /// <value>The required creation policy.</value>
        public CreationPolicy RequiredCreationPolicy { get; set; } = CreationPolicy.NonShared;

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
        public Assembly SourceAssembly { get; private set; }

        /// <summary>
        /// Gets the type of the contract.
        /// </summary>
        /// <value>The type of the contract.</value>
        [Browsable(false)]
        public Type? ContractType => Attribute.ContractType;

        /// <summary>
        /// The contract names core
        /// </summary>
        private readonly List<string> ContractNamesCore = new List<string>();

        /// <summary>
        /// Gets the contract names.
        /// </summary>
        /// <value>The contract names.</value>
        [Browsable(false)]
        public string[] ContractNames => ContractNamesCore.ToArray();

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
                throw new Exception(string.Format("export type {0} must implement {1}", Type.FullName, ContractType!.FullName!));
            }

            if (attribute.ContractName.IsNotNullOrEmpty())
            {
                ContractNamesCore.Add(attribute.ContractName);
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

                    ContractNamesCore.Add(interfaceType.FullName!);
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
            else
            {
                return Activator.CreateInstance(Type, parameters);
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            ExportPart? part = obj as ExportPart;

            if (part != null)
            {
                return part.Attribute.ContractName == Attribute.ContractName && part.Type == Type;
            }

            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
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
        protected List<ExportPart> PartsPrivate = new List<ExportPart>();

        /// <summary>
        /// Gets the parts.
        /// </summary>
        /// <value>The parts.</value>
        public ExportPart[] Parts => PartsPrivate.ToArray();

        /// <summary>
        /// Adds the part.
        /// </summary>
        /// <param name="part">The part.</param>
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
    /// <seealso cref="BuildPipeline.Core.Framework.Catalog" />
    public class AssemblyCatalog : Catalog
    {
        /// <summary>
        /// Gets or sets the assembly.
        /// </summary>
        /// <value>The assembly.</value>
        public System.Reflection.Assembly Assembly { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyCatalog"/> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public AssemblyCatalog(System.Reflection.Assembly assembly)
        {
            Assembly = assembly;

            foreach (var t in Assembly.GetTypes())
            {
                if (t.IsClass && !t.IsAbstract && t.IsDefined<ExportAttribute>())
                {
                    ExportAttribute attr = t.GetAnyCustomAttribute<ExportAttribute>()!;

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
            AssemblyCatalog? catalog = obj as AssemblyCatalog;

            if (catalog != null)
            {
                return catalog.Assembly == Assembly;
            }

            return base.Equals(obj);
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
        private static readonly List<Catalog> Catalogs = new List<Catalog>();

        /// <summary>
        /// The part changed
        /// </summary>
        public static EventHandler? PartChanged;

        /// <summary>
        /// The export parts
        /// </summary>
        private static Dictionary<string, List<ExportPart>> ExportParts = new Dictionary<string, List<ExportPart>>();
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
                x =>
                {
                    return x is AssemblyCatalog && ((AssemblyCatalog)x).Assembly == assembly;
                })
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
                    foreach (var contractname in part.ContractNames)
                    {
                        List<ExportPart>? targets;
                        if (!ExportParts.TryGetValue(contractname, out targets))
                        {
                            targets = new List<ExportPart>();
                            ExportParts.Add(contractname, targets);
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
            var catalogs = Catalogs.FindAll(x => { return x is AssemblyCatalog && ((AssemblyCatalog)x).Assembly == assembly; });

            foreach (var catalog in catalogs)
            {
                foreach (var part in catalog.Parts)
                {
                    foreach (var contractName in part.ContractNames)
                    {
                        List<ExportPart>? targets;
                        if (ExportParts.TryGetValue(contractName, out targets))
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

            int val = Catalogs.RemoveAll(x =>
            {
                return x is AssemblyCatalog && ((AssemblyCatalog)x).Assembly == assembly;
            });

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
        /// <param name="contractname">The contractname.</param>
        /// <returns>ExportPart[].</returns>
        public static ExportPart[]? GetExportParts(string contractname)
        {
            List<ExportPart>? parts;

            if (ExportParts.TryGetValue(contractname, out parts))
            {
                return parts.ToArray();
            }

            return null;
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
        public static void ComposeParts<T>(T target, object accessToken, params object[] args)
            where T : IComposableTarget
        {
            Logger.Assert(target != null);

            if (target == null)
            {
                return;
            }

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
                if ((m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field)
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
            Type? listType;
            Type interfaceType = CheckContractType(memberInfo, attribute, out listType);

            if (interfaceType == null)
            {
                return;
            }

            IList list = (Activator.CreateInstance(listType!) as IList)!;
            Logger.Assert(list != null);

            ComposeList(list!, interfaceType, attribute.ContractName, attribute.RequiredCreationPolicy, memberInfo, accessToken, args);

            var memberType = memberInfo.GetUnderlyingType();
            bool isManyImports = memberType.IsImplementFrom<IList>();

            if (!isManyImports)
            {
                if (list!.Count > 1)
                {
                    string errorMessage = string.Format("ComposeError: Too more import parts for {0}:{1}", memberInfo.DeclaringType!.FullName, memberInfo.Name);
                    Logger.LogError(errorMessage);

                    throw new ArgumentException(errorMessage);
                }
                else if (list.Count == 1)
                {
                    IImportable importable = (list[0] as IImportable)!;
                    Logger.Assert(importable != null);

                    memberInfo.SetUnderlyingValue(target, list[0]);
                    target.OnComposed(importable!);

                    if (importable is AbstractImportable ai)
                    {
                        ai.OnPostComposedTo(target);
                    }
                }
            }
            else
            {
                // many imports...
                var existsList = memberInfo.GetUnderlyingValue(target) as IList;

                if (existsList == null)
                {
                    if (!memberType.IsAssignableFrom(list!.GetType()))
                    {
                        if (memberType.IsAbstract)
                        {
                            string errorMessage = string.Format("ComposeError: Can't create {0} or convert {1} to {0}", memberType.FullName, list.GetType().FullName);
                            Logger.LogError(errorMessage);
                            throw new ArgumentException(errorMessage);
                        }
                        else
                        {
                            existsList = Activator.CreateInstance(memberType) as IList;
                            Logger.Assert(existsList != null);

                            memberType.SetUnderlyingValue(target, existsList);
                        }
                    }
                    else
                    {
                        existsList = list;
                        memberType.SetUnderlyingValue(target, existsList);
                    }
                }

                if (existsList != list)
                {
                    foreach (var i in list!)
                    {
                        existsList!.Add(i);
                    }
                }

                foreach (IImportable importable in existsList!)
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
            bool isManyImports = memberType.IsImplementFrom<IList>();
            listType = null;

            Type? declareContractType = null;

            if (isManyImports)
            {
                declareContractType = memberType.GetGenericArguments()[0];
            }
            else
            {
                declareContractType = memberType;
            }

            Logger.Assert(declareContractType != null);

            if (!declareContractType!.IsImplementFrom<IImportable>())
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

            listType = typeof(List<>).MakeGenericType(declareContractType!);

            return declareContractType!;
        }

        /// <summary>
        /// Composes the list.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="contractType">Type of the contract.</param>
        /// <param name="contractName">Name of the contract.</param>
        /// <param name="requiredCreationPolicy">The required creation policy.</param>
        /// <param name="memberInfo">The member information.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="args">The arguments.</param>
        private static void ComposeList(
            IList list,
            Type contractType,
            string contractName,
            CreationPolicy requiredCreationPolicy,
            MemberInfo memberInfo,
            object accessToken,
            params object[] args
            )
        {
            List<IImportable> Results = new List<IImportable>();
            List<ExportPart>? TargetParts;
            if (ExportParts.TryGetValue(contractName, out TargetParts))
            {
                foreach (var part in TargetParts!)
                {
                    if (contractType == null ||
                        (part.ContractType != null && contractType.IsAssignableFrom(part.ContractType)) ||
                        (part.ContractType == null && contractType.IsAssignableFrom(part.Type))
                        )
                    {
                        try
                        {
                            var obj = part.CreatePartObject(requiredCreationPolicy, args) as IImportable;

                            Logger.Assert(obj != null);

                            if (obj!.Accept(accessToken))
                            {
                                Results.Add(obj);
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
            }

            SortImportable(ref Results);

            foreach (var t in Results)
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
            list.Sort((x, y) =>
            {
                T xx = x;
                T yy = y;
                Logger.Assert(xx != null && yy != null);

                // greater
                return Comparer<int>.Default.Compare(xx!.ImportPriority, yy!.ImportPriority) * -1;
            });
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
            if (target == null)
            {
                return;
            }

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
}
