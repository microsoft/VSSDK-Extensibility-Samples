//------------------------------------------------------------------------------
// <copyright from='2003' to='2004' company='Microsoft Corporation'>           
//  Copyright (c) Microsoft Corporation, All rights reserved.             
//  This code sample is provided "AS IS" without warranty of any kind, 
//  it is not recommended for use in a production environment.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace Microsoft.VisualStudio.AsyncPackageHelpers
{

    using System;
    using System.Globalization;
    using Microsoft.VisualStudio.Shell;

    [Flags]
    public enum PackageAutoLoadFlags
    {
        /// <summary>
        /// Indicates no special auto-load behavior. This is the default flag value if not specified.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// When set package will not auto load in newer Visual Studio versions with rule based UI contexts
        /// </summary>
        SkipWhenUIContextRulesActive = 0x1,

        /// <summary>
        /// When set, if the associated package is marked as allowing background loads (via the <see cref="AsyncPackageRegistrationAttribute"/>),
        /// then the package will be loaded asynchronously, in the background, when the associated UI context is triggered.
        /// </summary>
        BackgroundLoad = 0x2
    }

    /// <summary>
    ///     This attribute registers the package as an extender.  The GUID passed in determines
    ///     what is being extended. The attributes on a package do not control the behavior of
    ///     the package, but they can be used by registration tools to register the proper
    ///     information with Visual Studio.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ProvideAutoLoadAttribute : RegistrationAttribute
    {

        private Guid loadGuid = Guid.Empty;

        /// <summary>
        /// Specify that the package should get loaded when this context is active.
        /// </summary>
        /// <param name="cmdUiContextGuid">Context which should trigger the loading of your package.</param>
        public ProvideAutoLoadAttribute(string cmdUiContextGuid) : this(cmdUiContextGuid, PackageAutoLoadFlags.None)
        {
        }

        /// <summary>
        /// Specify that the package should get loaded when this context is active.
        /// </summary>
        /// <param name="cmdUiContextGuid">Context which should trigger the loading of your package.</param>
        public ProvideAutoLoadAttribute(string cmdUiContextGuid, PackageAutoLoadFlags flags = PackageAutoLoadFlags.None)
        {
            loadGuid = new Guid(cmdUiContextGuid);
            Flags = flags;
        }

        /// <summary>
        /// Context Guid which triggers the loading of the package.
        /// </summary>
        public Guid LoadGuid
        {
            get
            {
                return loadGuid;
            }
        }

        /// <summary>
        /// Specifies the options for package auto load entry
        /// </summary>
        public PackageAutoLoadFlags Flags
        {
            get;
            private set;
        }

        /// <summary>
        /// The reg key name of this AutoLoad.
        /// </summary>
        private string RegKeyName
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "AutoLoadPackages\\{0}", loadGuid.ToString("B"));
            }
        }

        /// <summary>
        /// Called to register this attribute with the given context.  The context
        /// contains the location where the registration information should be placed.
        /// it also contains such as the type being registered, and path information.
        /// </summary>
        public override void Register(RegistrationContext context)
        {
            using (Key childKey = context.CreateKey(RegKeyName))
            {
                childKey.SetValue(context.ComponentType.GUID.ToString("B"), (int)Flags);
            }
        }

        /// <summary>
        /// Unregister this AutoLoad specification.
        /// </summary>
        public override void Unregister(RegistrationContext context)
        {
            context.RemoveValue(RegKeyName, context.ComponentType.GUID.ToString("B"));
        }
    }
}

