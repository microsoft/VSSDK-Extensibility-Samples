/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	internal sealed class LocDisplayNameAttribute : DisplayNameAttribute
	{
		private string name;

		public LocDisplayNameAttribute(string name)
		{
			this.name = name;
		}

		public override string DisplayName
		{
			get
			{
                string result = SR.GetString(this.name);

				if(result == null)
				{
					Debug.Assert(false, "String resource '" + this.name + "' is missing");
					result = this.name;
				}

				return result;
			}
		}
	}

	internal enum GeneralPropertyPageTag
	{
		AssemblyName,
		OutputType,
		RootNamespace,
		StartupObject,
		ApplicationIcon,
		TargetPlatform,
		TargetPlatformLocation
	}

	/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage"]/*' />
	//[ComVisible(true), Guid("B7406511-3C20-407a-ABF8-03965E5025EB")]
	public class GeneralPropertyPage : SettingsPage, EnvDTE80.IInternalExtenderProvider
	{
		#region fields
		private string assemblyName;
		private OutputType outputType;
		private string defaultNamespace;
		private string startupObject;
		private string applicationIcon;
        // TODO: The TargetPlatform and Location have been replaced by FrameworkName
        //private PlatformType targetPlatform = PlatformType.v2;
		//private string targetPlatformLocation;
		#endregion

		/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage.GeneralPropertyPage"]/*' />
		public GeneralPropertyPage()
		{
            this.Name = SR.GetString(SR.GeneralCaption);
		}

		#region overriden methods
		/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage.GetClassName"]/*' />
		public override string GetClassName()
		{
			return this.GetType().FullName;
		}

		/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage.BindProperties"]/*' />
		protected override void BindProperties()
		{
			if(this.ProjectMgr == null)
			{
				Debug.Assert(false);
				return;
			}

			this.assemblyName = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.AssemblyName.ToString(), true);

			string outputType = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.OutputType.ToString(), false);

			if(outputType != null && outputType.Length > 0)
			{
				try
				{
					this.outputType = (OutputType)Enum.Parse(typeof(OutputType), outputType);
				}
				catch
				{ } //Should only fail if project file is corrupt
			}

			this.defaultNamespace = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.RootNamespace.ToString(), false);
			this.startupObject = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.StartupObject.ToString(), false);
			this.applicationIcon = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.ApplicationIcon.ToString(), false);

            // TODO: The TargetPlatform and Location have been replaced by FrameworkName
            //string targetPlatform = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.TargetPlatform.ToString(), false);

            //if(targetPlatform != null && targetPlatform.Length > 0)
            //{
            //    try
            //    {
            //        this.targetPlatform = (PlatformType)Enum.Parse(typeof(PlatformType), targetPlatform);
            //    }
            //    catch
            //    { }
            //}

            //this.targetPlatformLocation = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.TargetPlatformLocation.ToString(), false);
		}

		/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage.ApplyChanges"]/*' />
		protected override int ApplyChanges()
		{
			if(this.ProjectMgr == null)
			{
				Debug.Assert(false);
				return VSConstants.E_INVALIDARG;
			}

			ValidateProperties();

			this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.AssemblyName.ToString(), this.assemblyName);
			this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.OutputType.ToString(), this.outputType.ToString());
			this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.RootNamespace.ToString(), this.defaultNamespace);
			this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.StartupObject.ToString(), this.startupObject);
			this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.ApplicationIcon.ToString(), this.applicationIcon);
            // TODO: The TargetPlatform and Location have been replaced by FrameworkName
            //this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.TargetPlatform.ToString(), this.targetPlatform.ToString());
            //this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.TargetPlatformLocation.ToString(), this.targetPlatformLocation);
			this.IsDirty = false;

			return VSConstants.S_OK;
		}
		#endregion

		#region exposed properties
		/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage.AssemblyName"]/*' />
		[SRCategoryAttribute(SR.Application)]
		[LocDisplayName(SR.AssemblyName)]
		[SRDescriptionAttribute(SR.AssemblyNameDescription)]
		public string AssemblyName
		{
			get { return this.assemblyName; }
			set { this.assemblyName = value; this.IsDirty = true; }
		}

		/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage.OutputType"]/*' />
		[SRCategoryAttribute(SR.Application)]
		[LocDisplayName(SR.OutputType)]
		[SRDescriptionAttribute(SR.OutputTypeDescription)]
		public OutputType OutputType
		{
			get { return this.outputType; }
			set { this.outputType = value; this.IsDirty = true; }
		}

		/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage.DefaultNamespace"]/*' />
		[SRCategoryAttribute(SR.Application)]
		[LocDisplayName(SR.DefaultNamespace)]
		[SRDescriptionAttribute(SR.DefaultNamespaceDescription)]
		public string DefaultNamespace
		{
			get { return this.defaultNamespace; }
			set { this.defaultNamespace = value; this.IsDirty = true; }
		}

		/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage.StartupObject"]/*' />
		[SRCategoryAttribute(SR.Application)]
		[LocDisplayName(SR.StartupObject)]
		[SRDescriptionAttribute(SR.StartupObjectDescription)]
		public string StartupObject
		{
			get { return this.startupObject; }
			set { this.startupObject = value; this.IsDirty = true; }
		}

		/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage.ApplicationIcon"]/*' />
		[SRCategoryAttribute(SR.Application)]
		[LocDisplayName(SR.ApplicationIcon)]
		[SRDescriptionAttribute(SR.ApplicationIconDescription)]
		public string ApplicationIcon
		{
			get { return this.applicationIcon; }
			set { this.applicationIcon = value; this.IsDirty = true; }
		}

		/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage.ProjectFile"]/*' />
		[SRCategoryAttribute(SR.Project)]
		[LocDisplayName(SR.ProjectFile)]
		[SRDescriptionAttribute(SR.ProjectFileDescription)]
		[AutomationBrowsable(false)]
		public string ProjectFile
		{
			get { return Path.GetFileName(this.ProjectMgr.ProjectFile); }
		}

		/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage.ProjectFolder"]/*' />
		[SRCategoryAttribute(SR.Project)]
		[LocDisplayName(SR.ProjectFolder)]
		[SRDescriptionAttribute(SR.ProjectFolderDescription)]
		[AutomationBrowsable(false)]
		public string ProjectFolder
		{
			get { return Path.GetDirectoryName(this.ProjectMgr.ProjectFolder); }
		}

		/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage.OutputFile"]/*' />
		[SRCategoryAttribute(SR.Project)]
		[LocDisplayName(SR.OutputFile)]
		[SRDescriptionAttribute(SR.OutputFileDescription)]
		[AutomationBrowsable(false)]
		public string OutputFile
		{
			get
			{
				return this.assemblyName + PythonProjectNode.GetOuputExtension(this.outputType);
			}
		}

		/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage.TargetPlatform"]/*' />
        // TODO: The TargetPlatform and Location have been replaced by FrameworkName
        //[SRCategoryAttribute(SR.Project)]
        //[LocDisplayName(SR.TargetPlatform)]
        //[SRDescriptionAttribute(SR.TargetPlatformDescription)]
        //[AutomationBrowsable(false)]
        //public PlatformType TargetPlatform
        //{
        //    get { return this.targetPlatform; }
        //    set { this.targetPlatform = value; IsDirty = true; }
        //}

		/// <include file='doc\PropertyPages.uex' path='docs/doc[@for="GeneralPropertyPage.TargetPlatformLocation"]/*' />
        //[SRCategoryAttribute(SR.Project)]
        //[LocDisplayName(SR.TargetPlatformLocation)]
        //[SRDescriptionAttribute(SR.TargetPlatformLocationDescription)]
        //[AutomationBrowsable(false)]
        //public string TargetPlatformLocation
        //{
        //    get { return this.targetPlatformLocation; }
        //    set { this.targetPlatformLocation = value; IsDirty = true; }
        //}
		#endregion

        #region IInternalExtenderProvider Members

        bool EnvDTE80.IInternalExtenderProvider.CanExtend(string extenderCATID, string extenderName, object extendeeObject)
        {
            IVsHierarchy outerHierarchy = HierarchyNode.GetOuterHierarchy(this.ProjectMgr);
            if (outerHierarchy is EnvDTE80.IInternalExtenderProvider)
                return ((EnvDTE80.IInternalExtenderProvider)outerHierarchy).CanExtend(extenderCATID, extenderName, extendeeObject);
            return false;
        }

        object EnvDTE80.IInternalExtenderProvider.GetExtender(string extenderCATID, string extenderName, object extendeeObject, EnvDTE.IExtenderSite extenderSite, int cookie)
        {
            IVsHierarchy outerHierarchy = HierarchyNode.GetOuterHierarchy(this.ProjectMgr);
            if (outerHierarchy is EnvDTE80.IInternalExtenderProvider)
                return ((EnvDTE80.IInternalExtenderProvider)outerHierarchy).GetExtender(extenderCATID, extenderName, extendeeObject, extenderSite, cookie);
            return null;
        }

        object EnvDTE80.IInternalExtenderProvider.GetExtenderNames(string extenderCATID, object extendeeObject)
        {
            IVsHierarchy outerHierarchy = HierarchyNode.GetOuterHierarchy(this.ProjectMgr);
            if (outerHierarchy is EnvDTE80.IInternalExtenderProvider)
                return ((EnvDTE80.IInternalExtenderProvider)outerHierarchy).GetExtenderNames(extenderCATID, extendeeObject);
            return null;
        }

        #endregion

		#region ExtenderSupport
		[Browsable(false)]
		[AutomationBrowsable(false)]
		public virtual string ExtenderCATID
		{
			get
			{
				Guid catid = this.ProjectMgr.ProjectMgr.GetCATIDForType(this.GetType());
				if(Guid.Empty.CompareTo(catid) == 0)
					throw new NotImplementedException();
				return catid.ToString("B");
			}
		}
		[Browsable(false)]
		[AutomationBrowsable(false)]
		public object ExtenderNames
		{
			get
			{
				EnvDTE.ObjectExtenders extenderService = (EnvDTE.ObjectExtenders)this.ProjectMgr.GetService(typeof(EnvDTE.ObjectExtenders));
				return extenderService.GetExtenderNames(this.ExtenderCATID, this);
			}
		}
		public object get_Extender(string extenderName)
		{
			EnvDTE.ObjectExtenders extenderService = (EnvDTE.ObjectExtenders)this.ProjectMgr.GetService(typeof(EnvDTE.ObjectExtenders));
			return extenderService.GetExtender(this.ExtenderCATID, extenderName, this);
		}
		#endregion

		#region helper methods
		private void ValidateProperties()
		{
			ValidateRootnamespace();
		}

		private void ValidateRootnamespace()
		{
			String invalidChars = @"([/?:&\\*<>|#%!" + '\"' + "])";
			Regex invalidCharactersRegex = new Regex(invalidChars, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

			if(invalidCharactersRegex.IsMatch(this.defaultNamespace))
			{
				throw new ArgumentException("Default Namespace:\nThe string for the default namespace must be a valid identifier");
			}

		}
		#endregion

	}
}
