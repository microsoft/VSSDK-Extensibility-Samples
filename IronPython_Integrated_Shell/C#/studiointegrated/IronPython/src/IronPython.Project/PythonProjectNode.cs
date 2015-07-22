/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.Samples.VisualStudio.IronPython.Project.WPFProviders;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Windows.Design.Host;
using VSConstants = Microsoft.VisualStudio.VSConstants;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using IronPython.EditorExtensions;
using Microsoft.Samples.VisualStudio.IronPython.Project.Library;

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{
    /// <summary>
    /// The Python Project
    /// </summary>
    [Guid("5DADABD3-6A4C-455a-8450-C8ABD3CA9F9D")]
    public class PythonProjectNode : ProjectNode, IVsProjectSpecificEditorMap2
    {
        #region fields
        private PythonProjectPackage package;
        private Guid GUID_MruPage = new Guid("{19B97F03-9594-4c1c-BE28-25FF030113B3}");
        private VSLangProj.VSProject vsProject = null;
        private Microsoft.VisualStudio.Designer.Interfaces.IVSMDCodeDomProvider codeDomProvider;
        private static ImageList pythonImageList;
        private ProjectDocumentsListenerForMainFileUpdates projectDocListenerForMainFileUpdates;
        internal static int ImageOffset;
        private DesignerContext designerContext;
        #endregion

        #region enums

        public enum PythonImageName
        {
            PyFile = 0,
            PyProject = 1,
        }

        #endregion

        #region Properties
        /// <summary>
        /// Returns the outputfilename based on the output type
        /// </summary>
        public string OutputFileName
        {
            get
            {
                string assemblyName = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.AssemblyName.ToString(), true);

                string outputTypeAsString = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.OutputType.ToString(), false);
                OutputType outputType = (OutputType)Enum.Parse(typeof(OutputType), outputTypeAsString);

                return assemblyName + GetOuputExtension(outputType);
            }
        }
        /// <summary>
        /// Retreive the CodeDOM provider
        /// </summary>
        protected internal Microsoft.VisualStudio.Designer.Interfaces.IVSMDCodeDomProvider CodeDomProvider
        {
            get
            {
                if (codeDomProvider == null)
                    codeDomProvider = new VSMDPythonProvider(this.VSProject);
                return codeDomProvider;
            }
        }
        protected internal Microsoft.Windows.Design.Host.DesignerContext DesignerContext
        {
            get
            {
                if (designerContext == null)
                {
                    designerContext = new DesignerContext();
                }
                return designerContext;
            }
        }
        /// <summary>
        /// Get the VSProject corresponding to this project
        /// </summary>
        protected internal VSLangProj.VSProject VSProject
        {
            get
            {
                if (vsProject == null)
                    vsProject = new OAVSProject(this);
                return vsProject;
            }
        }
        private IVsHierarchy InteropSafeHierarchy
        {
            get
            {
                IntPtr unknownPtr = Utilities.QueryInterfaceIUnknown(this);
                if (IntPtr.Zero == unknownPtr)
                {
                    return null;
                }
                IVsHierarchy hier = Marshal.GetObjectForIUnknown(unknownPtr) as IVsHierarchy;
                return hier;
            }
        }

        /// <summary>
        /// Python specific project images
        /// </summary>
        public static ImageList PythonImageList
        {
            get
            {
                return pythonImageList;
            }
            set
            {
                pythonImageList = value;
            }
        }
        #endregion

        #region ctor

        static PythonProjectNode()
        {
            PythonImageList = Utilities.GetImageList(typeof(PythonProjectNode).Assembly.GetManifestResourceStream("Microsoft.Samples.VisualStudio.IronPython.Project.Resources.PythonImageList.bmp"));
        }

        public PythonProjectNode(PythonProjectPackage pkg)
        {
            this.package = pkg;
            this.CanFileNodesHaveChilds = true;
            this.OleServiceProvider.AddService(typeof(VSLangProj.VSProject), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
            this.SupportsProjectDesigner = true;

            //Store the number of images in ProjectNode so we know the offset of the python icons.
            ImageOffset = this.ImageHandler.ImageList.Images.Count;
            foreach (Image img in PythonImageList.Images)
            {
                this.ImageHandler.AddImage(img);
            }

            InitializeCATIDs();
        }

        /// <summary>
        /// Provide mapping from our browse objects and automation objects to our CATIDs
        /// </summary>
        private void InitializeCATIDs()
        {
            // The following properties classes are specific to python so we can use their GUIDs directly
            this.AddCATIDMapping(typeof(PythonProjectNodeProperties), typeof(PythonProjectNodeProperties).GUID);
            this.AddCATIDMapping(typeof(PythonFileNodeProperties), typeof(PythonFileNodeProperties).GUID);
            this.AddCATIDMapping(typeof(OAIronPythonFileItem), typeof(OAIronPythonFileItem).GUID);
            // The following are not specific to python and as such we need a separate GUID (we simply used guidgen.exe to create new guids)
            this.AddCATIDMapping(typeof(FolderNodeProperties), new Guid("A3273B8E-FDF8-4ea8-901B-0D66889F645F"));
            // This one we use the same as python file nodes since both refer to files
            this.AddCATIDMapping(typeof(FileNodeProperties), typeof(PythonFileNodeProperties).GUID);
            // Because our property page pass itself as the object to display in its grid, we need to make it have the same CATID
            // as the browse object of the project node so that filtering is possible.
            this.AddCATIDMapping(typeof(GeneralPropertyPage), typeof(PythonProjectNodeProperties).GUID);

            // We could also provide CATIDs for references and the references container node, if we wanted to.
        }

        #endregion

        #region overridden properties

        /// <summary>
        /// Since we appended the python images to the base image list in the constructor,
        /// this should be the offset in the ImageList of the python project icon.
        /// </summary>
        public override int ImageIndex
        {
            get
            {
                return ImageOffset + (int)PythonImageName.PyProject;
            }
        }

        public override Guid ProjectGuid
        {
            get
            {
                return typeof(PythonProjectFactory).GUID;
            }
        }
        public override string ProjectType
        {
            get
            {
                return "PythonProject";
            }
        }
        internal override object Object
        {
            get
            {
                return this.VSProject;
            }
        }
        #endregion

        #region overridden methods
        protected override ReferenceContainerNode CreateReferenceContainerNode()
        {
            return new PythonReferenceContainerNode(this);
        }

        public override int GetGuidProperty(int propid, out Guid guid)
        {
            if ((__VSHPROPID)propid == __VSHPROPID.VSHPROPID_PreferredLanguageSID)
            {
                guid = new Guid(GuidList.guidIronPythonLanguageString);
            }
            else
            {
                return base.GetGuidProperty(propid, out guid);
            }
            return VSConstants.S_OK;
        }

        protected override bool IsItemTypeFileType(string type)
        {
            if (!base.IsItemTypeFileType(type))
            {
                if (String.Compare(type, "Page", StringComparison.OrdinalIgnoreCase) == 0
                || String.Compare(type, "ApplicationDefinition", StringComparison.OrdinalIgnoreCase) == 0
                || String.Compare(type, "Resource", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //This is a well known item node type, so return true.
                return true;
            }
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new PythonProjectNodeProperties(this);
        }

        public override int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider site)
        {
            base.SetSite(site);

            //Initialize a new object to track project document changes so that we can update the MainFile Property accordingly
            this.projectDocListenerForMainFileUpdates = new ProjectDocumentsListenerForMainFileUpdates((ServiceProvider)this.Site, this);
            this.projectDocListenerForMainFileUpdates.Init();

            return VSConstants.S_OK;
        }

        public override int Close()
        {
            if (null != this.projectDocListenerForMainFileUpdates)
            {
                this.projectDocListenerForMainFileUpdates.Dispose();
                this.projectDocListenerForMainFileUpdates = null;
            }

            if (null != Site)
            {
                IPythonLibraryManager libraryManager = Site.GetService(typeof(IPythonLibraryManager)) as IPythonLibraryManager;
                if (null != libraryManager)
                {
                    libraryManager.UnregisterHierarchy(this.InteropSafeHierarchy);
                }
            }

            return base.Close();
        }
        public override void Load(string filename, string location, string name, uint flags, ref Guid iidProject, out int canceled)
        {
            base.Load(filename, location, name, flags, ref iidProject, out canceled);
            // WAP ask the designer service for the CodeDomProvider corresponding to the project node.
            this.OleServiceProvider.AddService(typeof(SVSMDCodeDomProvider), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
            this.OleServiceProvider.AddService(typeof(System.CodeDom.Compiler.CodeDomProvider), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);

            IPythonLibraryManager libraryManager = Site.GetService(typeof(IPythonLibraryManager)) as IPythonLibraryManager;
            if (null != libraryManager)
            {
                libraryManager.RegisterHierarchy(this.InteropSafeHierarchy);
            }

            //If this is a WPFFlavor-ed project, then add a project-level DesignerContext service to provide
            //event handler generation (EventBindingProvider) for the XAML designer.
            this.OleServiceProvider.AddService(typeof(DesignerContext), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);

        }
        /// <summary>
        /// Overriding to provide project general property page
        /// </summary>
        /// <returns></returns>
        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            Guid[] result = new Guid[1];
            result[0] = typeof(GeneralPropertyPage).GUID;
            return result;
        }

        /// <summary>
        /// Returns the configuration dependent property pages.
        /// Specify here a property page. By returning no property page the configuartion dependent properties will be neglected.
        /// Overriding, but current implementation does nothing
        /// To provide configuration specific page project property page, this should return an array bigger then 0
        /// (you can make it do the same as GetPropertyPageGuids() to see its impact)
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        protected override Guid[] GetConfigurationDependentPropertyPages()
        {
            Guid[] result = new Guid[1];
            result[0] = typeof(IronPythonBuildPropertyPage).GUID;
            return result;
        }

        public override object GetAutomationObject()
        {
            return new OAIronPythonProject(this);
        }


        /// <summary>
        /// Overriding to provide customization of files on add files.
        /// This will replace tokens in the file with actual value (namespace, class name,...)
        /// </summary>
        /// <param name="source">Full path to template file</param>
        /// <param name="target">Full path to destination file</param>
        public override void AddFileFromTemplate(string source, string target)
        {
            if (!System.IO.File.Exists(source))
                throw new FileNotFoundException(String.Format("Template file not found: {0}", source));

            // We assume that there is no token inside the file because the only
            // way to add a new element should be through the template wizard that
            // take care of expanding and replacing the tokens.
            // The only task to perform is to copy the source file in the
            // target location.
            string targetFolder = Path.GetDirectoryName(target);
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            File.Copy(source, target);
        }
        /// <summary>
        /// Evaluates if a file is an IronPython code file based on is extension
        /// </summary>
        /// <param name="strFileName">The filename to be evaluated</param>
        /// <returns>true if is a code file</returns>
        public override bool IsCodeFile(string strFileName)
        {
            // We do not want to assert here, just return silently.
            if (String.IsNullOrEmpty(strFileName))
            {
                return false;
            }
            return (String.Compare(Path.GetExtension(strFileName), ".py", StringComparison.OrdinalIgnoreCase) == 0);

        }

        /// <summary>
        /// Create a file node based on an msbuild item.
        /// </summary>
        /// <param name="item">The msbuild item to be analyzed</param>
        /// <returns>PythonFileNode or FileNode</returns>
        public override FileNode CreateFileNode(ProjectElement item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            string include = item.GetMetadata(ProjectFileConstants.Include);
            PythonFileNode newNode = new PythonFileNode(this, item);
            newNode.OleServiceProvider.AddService(typeof(EnvDTE.Project), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
            newNode.OleServiceProvider.AddService(typeof(EnvDTE.ProjectItem), newNode.ServiceCreator, false);
            if (!string.IsNullOrEmpty(include) && Path.GetExtension(include).Equals(".xaml", StringComparison.OrdinalIgnoreCase))
            {
                //Create a DesignerContext for the XAML designer for this file
                newNode.OleServiceProvider.AddService(typeof(DesignerContext), newNode.ServiceCreator, false);
            }
            newNode.OleServiceProvider.AddService(typeof(VSLangProj.VSProject), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
            if (IsCodeFile(include))
            {
                newNode.OleServiceProvider.AddService(
                    typeof(SVSMDCodeDomProvider), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
            }

            return newNode;
        }

        public override DependentFileNode CreateDependentFileNode(ProjectElement item)
        {
            DependentFileNode node = base.CreateDependentFileNode(item);
            if (null != node)
            {
                string include = item.GetMetadata(ProjectFileConstants.Include);
                if (IsCodeFile(include))
                {
                    node.OleServiceProvider.AddService(
                        typeof(SVSMDCodeDomProvider), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
                }
            }

            return node;
        }

        /// <summary>
        /// Creates the format list for the open file dialog
        /// </summary>
        /// <param name="formatlist">The formatlist to return</param>
        /// <returns>Success</returns>
        public override int GetFormatList(out string formatlist)
        {
            formatlist = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.ProjectFileExtensionFilter), "\0", "\0");
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This overrides the base class method to show the VS 2005 style Add reference dialog. The ProjectNode implementation
        /// shows the VS 2003 style Add Reference dialog.
        /// </summary>
        /// <returns>S_OK if succeeded. Failure other wise</returns>
        public override int AddProjectReference()
        {
            IVsComponentSelectorDlg2 componentDialog;
            Guid guidEmpty = Guid.Empty;
            VSCOMPONENTSELECTORTABINIT[] tabInit = new VSCOMPONENTSELECTORTABINIT[5];
            string strBrowseLocations = Path.GetDirectoryName(this.BaseURI.Uri.LocalPath);

            //Add the .NET page
            tabInit[0].dwSize = (uint)Marshal.SizeOf(typeof(VSCOMPONENTSELECTORTABINIT));
            tabInit[0].varTabInitInfo = 0;
            tabInit[0].guidTab = VSConstants.GUID_COMPlusPage;

            //Add the COM page
            tabInit[1].dwSize = (uint)Marshal.SizeOf(typeof(VSCOMPONENTSELECTORTABINIT));
            tabInit[1].varTabInitInfo = 0;
            tabInit[1].guidTab = VSConstants.GUID_COMClassicPage;

            //Add the Project page
            tabInit[2].dwSize = (uint)Marshal.SizeOf(typeof(VSCOMPONENTSELECTORTABINIT));
            // Tell the Add Reference dialog to call hierarchies GetProperty with the following
            // propID to enablefiltering out ourself from the Project to Project reference
            tabInit[2].varTabInitInfo = (int)__VSHPROPID.VSHPROPID_ShowProjInSolutionPage;
            tabInit[2].guidTab = VSConstants.GUID_SolutionPage;

            // Add the Browse page			
            tabInit[3].dwSize = (uint)Marshal.SizeOf(typeof(VSCOMPONENTSELECTORTABINIT));
            tabInit[3].guidTab = VSConstants.GUID_BrowseFilePage;
            tabInit[3].varTabInitInfo = 0;

            //// Add the Recent page			
            tabInit[4].dwSize = (uint)Marshal.SizeOf(typeof(VSCOMPONENTSELECTORTABINIT));
            tabInit[4].guidTab = GUID_MruPage;
            tabInit[4].varTabInitInfo = 0;

            uint pX = 0, pY = 0;


            componentDialog = this.GetService(typeof(SVsComponentSelectorDlg)) as IVsComponentSelectorDlg2;
            try
            {
                // call the container to open the add reference dialog.
                if (componentDialog != null)
                {
                    // Let the project know not to show itself in the Add Project Reference Dialog page
                    this.ShowProjectInSolutionPage = false;

                    // call the container to open the add reference dialog.
                    ErrorHandler.ThrowOnFailure(componentDialog.ComponentSelectorDlg2(
                        (System.UInt32)(__VSCOMPSELFLAGS.VSCOMSEL_MultiSelectMode | __VSCOMPSELFLAGS.VSCOMSEL_IgnoreMachineName),
                        (IVsComponentUser)this,
                        0,
                        null,
                Microsoft.VisualStudio.Project.SR.GetString(Microsoft.VisualStudio.Project.SR.AddReferenceDialogTitle),   // Title
                        "VS.AddReference",						  // Help topic
                        ref pX,
                        ref pY,
                        (uint)tabInit.Length,
                        tabInit,
                        ref guidEmpty,
                        "*.dll",
                        ref strBrowseLocations));
                }
            }
            catch (COMException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
                return e.ErrorCode;
            }
            finally
            {
                // Let the project know it can show itself in the Add Project Reference Dialog page
                this.ShowProjectInSolutionPage = true;
            }
            return VSConstants.S_OK;
        }

        protected override ConfigProvider CreateConfigProvider()
        {
            return new PythonConfigProvider(this);
        }

        #endregion

        #region Methods
        /// <summary>
        /// Creates the services exposed by this project.
        /// </summary>
        private object CreateServices(Type serviceType)
        {
            object service = null;
            if (typeof(SVSMDCodeDomProvider) == serviceType)
            {
                service = this.CodeDomProvider;
            }
            else if (typeof(System.CodeDom.Compiler.CodeDomProvider) == serviceType)
            {
                service = this.CodeDomProvider.CodeDomProvider;
            }
            else if (typeof(DesignerContext) == serviceType)
            {
                service = this.DesignerContext;
            }
            else if (typeof(VSLangProj.VSProject) == serviceType)
            {
                service = this.VSProject;
            }
            else if (typeof(EnvDTE.Project) == serviceType)
            {
                service = this.GetAutomationObject();
            }
            return service;
        }
        #endregion

        #region IVsProjectSpecificEditorMap2 Members

        public int GetSpecificEditorProperty(string mkDocument, int propid, out object result)
        {
            // initialize output params
            result = null;

            //Validate input
            if (string.IsNullOrEmpty(mkDocument))
                throw new ArgumentException("Was null or empty", "mkDocument");

            // Make sure that the document moniker passed to us is part of this project
            // We also don't care if it is not a python file node
            uint itemid;
            ErrorHandler.ThrowOnFailure(ParseCanonicalName(mkDocument, out itemid));
            HierarchyNode hierNode = NodeFromItemId(itemid);
            if (hierNode == null || ((hierNode as PythonFileNode) == null))
                return VSConstants.E_NOTIMPL;

            switch (propid)
            {
                case (int)__VSPSEPROPID.VSPSEPROPID_UseGlobalEditorByDefault:
                    // we do not want to use global editor for form files
                    result = true;
                    break;
                case (int)__VSPSEPROPID.VSPSEPROPID_ProjectDefaultEditorName:
                    result = "Python Form Editor";
                    break;
            }

            return VSConstants.S_OK;
        }

        public int GetSpecificEditorType(string mkDocument, out Guid guidEditorType)
        {
            // Ideally we should at this point initalize a File extension to EditorFactory guid Map e.g.
            // in the registry hive so that more editors can be added without changing this part of the
            // code. Iron Python only makes usage of one Editor Factory and therefore we will return 
            // that guid
            guidEditorType = EditorFactory.guidEditorFactory;
            return VSConstants.S_OK;
        }

        public int GetSpecificLanguageService(string mkDocument, out Guid guidLanguageService)
        {
            guidLanguageService = Guid.Empty;
            return VSConstants.E_NOTIMPL;
        }

        public int SetSpecificEditorProperty(string mkDocument, int propid, object value)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region static methods
        public static string GetOuputExtension(OutputType outputType)
        {
            if (outputType == OutputType.Library)
            {
                return "." + OutputFileExtension.dll.ToString();
            }
            else
            {
                return "." + OutputFileExtension.exe.ToString();
            }
        }
        #endregion

        public override int QueryStatusCommand(uint itemId, ref Guid guidCmdGroup, uint cCmds, Microsoft.VisualStudio.OLE.Interop.OLECMD[] cmds, IntPtr pCmdText)
        {
            if (guidCmdGroup == typeof(VSConstants.VSStd2KCmdID).GUID)
            {
                if (cCmds == (uint)VSConstants.VSStd2KCmdID.INSERTSNIPPET || cCmds == (uint)VSConstants.VSStd2KCmdID.SURROUNDWITH)
                {
                    cmds[0].cmdf = (int)QueryStatusResult.ENABLED | (int)QueryStatusResult.SUPPORTED;

                    return VSConstants.S_OK;
                }
            }

            return base.QueryStatusCommand(itemId, ref guidCmdGroup, cCmds, cmds, pCmdText);
        }
    }
}
