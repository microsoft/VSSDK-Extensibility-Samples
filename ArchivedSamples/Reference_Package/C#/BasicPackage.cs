/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.Samples.VisualStudio.IDE.BasicPackage
{
	/// <summary>
	/// The BasicPackage class demonstrates how to create the most basic package in C#.
	/// Because it is so simple, it doesn't actually do anything and the only way for
	/// it to be loaded is if another package calls IVsShell.LoadPackage().
	/// 
	/// The IVsPackage implementation (in order to be a package a class has to implement
	/// this) is inherited from the Package class. Additionally, inheriting from the 
	/// Package class means that BasicPackage has the PackageRegistration attribute.
	/// 
	/// There are also two attributes that are specified on the BasicPackage class
	/// directly:
	///		Guid is used as the package Guid. This is used in a number of places
	/// in the registration, the main one being under Package.
	/// 
	/// To register the package (so that Visual Studio knows about it) we use
	/// a tool called RegPkg.exe. That tool looks at the attribute on the classes
	/// implementing IVsPackage to determine what needs to be registered. Once
	/// the package is registered, we call Devenv.exe /rootsuffix Exp /setup.
	/// This is so that Visual Studio can update itself based on the newly
	/// registered (or unregistered) packages. The "/rootsuffix Exp" part lets
	/// Visual Studio know that we want to use the experimental registry hive.
	/// The "/setup" lets Visual Studio know that we are running in the mode where
	/// it is looking for added/removed components to update itself.
	/// </summary>

	[PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	[Guid("01069CDD-95CE-4620-AC21-DDFF6C57F012")]
	public class BasicPackage : Package
	{
		/// <summary>
		/// BasicPackage contructor.
		/// While we could have used the default constructor, adding the Trace makes it
		/// possible to verify that the package was created without having to set a break
		/// point in the debugger.
		/// </summary>
		public BasicPackage()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for class {0}.", GetType().Name));
		}

        /// <summary>
        /// Initialization of the package; This method is called right after the package is sited.
        /// Place any initialization code that relies on services provided by Visual Studio here.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            
            // Place additional initialization code here
        }
	}
}
