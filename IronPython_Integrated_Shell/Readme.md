# Title: IronPython Integrated Shell Sample

**Abstract:** Create a Visual Studio Integrated Shell for IronPython
components.[ View this sample online](https://github.com/Microsoft/VSSDK-Extensibility-Samples).

* Technologies: Visual Studio 2015 SDK
* Topics: MSBuild, VSX
* Last Updated: 06/29/2015


**Description**

This sample is a component of IronPython integration inside Visual Studio and
it demostrates how to create a project type and a winforms designer. The
sample is based on the Project Framework and references the source code
installed with the VSSDK. Support is also included for the WPF designer. .

Create a Visual Studio project type for:

  * Building Windows Forms applications using Iron Python 
  * Building Web Application Projects using Iron Python 
  * Building Web Site Projects using Iron Python 
  * Building WPF Applications using Iron Python 



** Requirements **

[ Visual Studio 2015 ](http://www.microsoft.com/visualstudio/en-
us/try/default.mspx#download)

[ Visual Studio 2015 SDK ](https://www.visualstudio.com/en-us/downloads
/visual-studio-2015-downloads-vs.aspx)

[ Visual Studio 2015 Integrated Shell ](https://www.microsoft.com/en-
us/download/details.aspx?id=46886)

[ Visual Studio 2015 Isolated Shell ](https://www.microsoft.com/en-
us/download/details.aspx?id=46884)

[ Wix 3.x toolset ](http://wixtoolset.org/) You will need version 3.10 or
newer in order to be compatible with VS2015

[ Python Tools for VS2015
](https://visualstudiogallery.msdn.microsoft.com/9ea113de-a009-46cd-
99f5-65ef0595f937) Current version is 2.2

&nbsp;_place_holder;

**Build the sample**

  * Download the zip file associated with the sample 
  * Unzip the sample to your machine 
  * Double click on the .sln file to launch the solution 



**Run the sample**

  * To build and execute the sample, press F5 after the sample is loaded 
  * In the Experimental Instance, navigate toFile &gt; New &gt; Project**
  * In theNew Project** dialog, click on the _IronPython_ tab under Templates on the left edge of the window
  * A list of IronPython projects are displayed
  * Create a new _IronPython Console Application_ project
  * Press F5 to build and run the IronPython program _Program.py_
  * A console window opens, displays "Hello VSX!", and closes



**Functional Tests**

  * Make sure that the project can add new IronPython items. 
  * Make sure that the project can add a reference. 



**Project Files**

* **Automation.cs**

Contains classes that enable automation of the IronPython project and py files
within. The project object enables CodeModel.

* **ConfigurationPropertyPages.cs**

Defines the build property page in the Project Designer.

* **EditorFactory.cs**

Defines the editor factory that creates the code/design editor views for
editing iron python code files.

* **Enums.cs**

Contains enumerations defined for the Iron Python Project.

* **Guids.cs**

Defines the Package and Project guids.

* **PkgCmd.vsct**

Defines the layout for IronPython-specific commands.

* **ProjectDocumentsListenerForMainFileUpdates.cs**

A project listener that updates the mainfile project property in the
IronPython project whenever files are renamed/added/deleted.

* **PropertyPages.cs**

Implements the General Tab in the Project Designer.

* **PythonConfigProvider.cs**

Enables the Any CPU Platform name for IronPython Projects.

* **PythonFileNode.cs**

Contains IronPython specific implementation details of FileNode.

* **PythonFileNodeProperties.cs**

Defines the Iron Python specific Node properties. The class derives from
SingleFileNodeProperties, meaning a Custom Tool can be associated with a py
file.

* **PythonMenus.cs**

Defines CommandIDs matching the commands defined symbols in PkgCmd.vsct.

* **PythonProjectFactory.cs**

Defines the IronPython project factory.

* **PythonProjectFileConstants.cs**

Defines constants used by the IronPython project file.

* **PythonProjectNode.cs**

Contains the project node implementation in the IronPython Project.

* **PythonProjectNodeProperties.cs**

Defines IronPython-specific Node Properties for the ProjectNode object.

* **PythonProjectPackage.cs**

Defines the package object for IronPython project package.

* **PythonProjectReferenceNode.cs**

Defines IronPython Project specific requirements for project-to-project
references.

* **PythonReferenceContainerNode.cs**

Defines the reference container node for IronPython projects.

* **SelectionElementValueChangedListener.cs**

Defines a Selection changed listener that enables the RunGenerator on a python
file node to be triggered when focus is removed from an active IronPython
document window.

* **VSMDPythonProvider.cs**

Defines the IronPython CodeDOM Provider. The provider listens for a reference
event in order to stay in sync with the reference list in the IronPython
project.

* **PythonEventBindingProvider.cs**

PythonEventBindingProvider provides the communication between the WPF designer
and the associated code file for adding and navigating to event handlers.

* **PythonRuntimeNameProvider.cs**

PythonRuntimeNameFactory contains logic to generate uniquely named code fields
for WPF designer scenarios.

* **PythonWPFFlavor.cs**

Defines the IronPython project flavor of the WPF Flavor.

* **PythonWPFProjectFactory.cs**

Defines the factory object for the IronPython project flavor of the WPF
Flavor.



**Related topics**

[ Integrated and Isolated Shell Documentation ](https://msdn.microsoft.com/en-
us/library/bb685612.aspx)

