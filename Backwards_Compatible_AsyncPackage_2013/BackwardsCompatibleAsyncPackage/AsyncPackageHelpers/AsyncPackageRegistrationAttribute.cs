//------------------------------------------------------------------------------
// <copyright file="PackageRegistrationAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace Microsoft.VisualStudio.AsyncPackageHelpers {

    using System;
    using System.Globalization;
    using System.IO;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <devdoc>
    ///     This attribute is defined on a package to get it to be registered.  It
    ///     is internal because packages are meant to be registered, so it is
    ///     implicit just by having a package in the assembly.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class, Inherited=true, AllowMultiple=false)]
    public sealed class AsyncPackageRegistrationAttribute : RegistrationAttribute
    {
        private RegistrationMethod registrationMethod = RegistrationMethod.Default;
        private bool useManagedResources = false;
        private bool allowsBackgroundLoad = false;
        private string satellitePath = null;
        
        /// <devdoc>
        ///     Select between specifying the Codebase entry or the Assembly entry in the registry.
        ///     This can be overriden during registration
        /// </devdoc>
        public RegistrationMethod RegisterUsing
        {
            get
            {
                return registrationMethod;
            }
            set
            {
                registrationMethod = value;
            }
        }

        /// <summary>
        /// For managed resources, there should not be a native ui dll registered.
        /// </summary>
        public bool UseManagedResourcesOnly
        {
            get { return useManagedResources; }
            set { useManagedResources = value; }
        }

        /// <summary>
        /// Package is safe to load on a background thread.
        /// </summary>
        public bool AllowsBackgroundLoading
        {
            get { return allowsBackgroundLoad; }
            set { allowsBackgroundLoad = value; }
        }

        /// <summary>
        /// To specify a resource dll located in a different location then the default,
        /// set this property. This can be useful if your package is installed in the GAC.
        /// If this is not set, the directory where the package is located will be use.
        /// 
        /// Note that the dll should be located at the following path:
        ///        SatellitePath\lcid\PackageDllNameUI.dll
        /// </summary>
        public string SatellitePath
        {
            get { return satellitePath; }
            set { satellitePath = value; }
        }

        private string RegKeyName(RegistrationContext context)
        {
            return String.Format(CultureInfo.InvariantCulture, "Packages\\{0}", context.ComponentType.GUID.ToString("B"));
        }

        /// <devdoc>
        ///     Called to register this attribute with the given context.  The context
        ///     contains the location where the registration information should be placed.
        ///     it also contains such as the type being registered, and path information.
        ///
        ///     This method is called both for registration and unregistration.  The difference is
        ///     that unregistering just uses a hive that reverses the changes applied to it.
        /// </devdoc>
        /// <param name="context">
        ///     Contains the location where the registration information should be placed.
        ///     It also contains other information such as the type being registered 
        ///     and path of the assembly.
        /// </param>
        public override void Register(RegistrationContext context) {
            Type t = context.ComponentType;

            Key packageKey = null;
            try
            {
                packageKey = context.CreateKey(RegKeyName(context));

                //use a friendly description if it exists.
                DescriptionAttribute attr = TypeDescriptor.GetAttributes(t)[typeof(DescriptionAttribute)] as DescriptionAttribute;
                if (attr != null && !String.IsNullOrEmpty(attr.Description)) {
                    packageKey.SetValue(string.Empty, attr.Description);
                }
                else {
                    packageKey.SetValue(string.Empty, t.Name);
                }

                packageKey.SetValue("InprocServer32", context.InprocServerPath);
                packageKey.SetValue("Class", t.FullName);

                // If specified on the command line, let the command line option override
                if (context.RegistrationMethod != RegistrationMethod.Default)
                {
                    registrationMethod = context.RegistrationMethod;
                }

                // Select registration method
                switch (registrationMethod)
                {
                    case RegistrationMethod.Assembly:
                    case RegistrationMethod.Default:
                        packageKey.SetValue("Assembly", t.Assembly.FullName);
                        break;

                    case RegistrationMethod.CodeBase:
                        packageKey.SetValue("CodeBase", context.CodeBase);
                        break;
                }

                Key childKey = null;
                if (!useManagedResources)
                {
                    try
                    {
                        childKey = packageKey.CreateSubkey("SatelliteDll");

                        // Register the satellite dll
                        string satelliteDllPath;
                        if (SatellitePath != null)
                        {
                            // Use provided path
                            satelliteDllPath = context.EscapePath(SatellitePath);
                        }
                        else
                        {
                            // Default to package path
                            satelliteDllPath = context.ComponentPath;
                        }
                        childKey.SetValue("Path", satelliteDllPath);
                        childKey.SetValue("DllName", String.Format(CultureInfo.InvariantCulture, "{0}UI.dll", Path.GetFileNameWithoutExtension(t.Assembly.ManifestModule.Name)));
                    }
                    finally
                    {
                        if (childKey != null)
                            childKey.Close();
                    }
                }

                if (allowsBackgroundLoad)
                {
                    packageKey.SetValue("AllowsBackgroundLoad", true);
                }

                if (typeof(IVsPackageDynamicToolOwner).IsAssignableFrom(context.ComponentType) ||
                    typeof(IVsPackageDynamicToolOwnerEx).IsAssignableFrom(context.ComponentType))
                {
                    packageKey.SetValue("SupportsDynamicToolOwner", Microsoft.VisualStudio.PlatformUI.Boxes.BooleanTrue);
                }
            }
            finally
            {
                if (packageKey != null)
                    packageKey.Close();
            }
        }

        /// <devdoc>
        ///     Unregister this package.
        /// </devdoc>
        /// <param name="context"></param>
        public override void Unregister(RegistrationContext context) 
        {
            context.RemoveKey(RegKeyName(context));
        }

    }
}

