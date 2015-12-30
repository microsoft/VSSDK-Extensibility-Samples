
# Source Code Control Provider Sample
Provides a Source Code Control Provider in Visual Studio.


* Technologies: Visual Studio 2015 SDK
* Topics: MSBuild, VSX

**Description**

This sample demonstrates how to register a source control provider with Visual
Studio that can be selected as active source control provider.

  * Implement a source control provider package 
  * Adds an Options page visible only when the provider is active 
  * Adds a tool window visible only when the provider is active 
  * Menu items are only exposed when the provider is active 
  * Adds a source control commands toolbar. The buttons are disabled when when the provider is inactive 
  * Add source control command shortcuts to right-click menu in Solution Explorer 
  * Enable persisting and reading solution properties 
  * Enable persisting and reading user options in the .suo file 
  * Implement basic QueryEdit / QuerySave service functionality 
  * Demonstrate how to implement IVsTrackProjectDocumentsEvents2 functions 
  * Provide basic source control functionality, such as add to source control, check in, check out, and work offline 

![image](C%23/Resources/Example.SccProvider.png)

**Requirements**

[ Visual Studio 2015 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)



**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

** Run the sample

  1. To run the sample, hitF5** or choose theDebug &gt; Start Debugging** menu command. A new experimental instance of Visual Studio will launch. 
  2. Once loaded, navigate toTools &gt; Options &gt; Source Control &gt; Plug-In Selection** and set _Managed Source Control Sample Provider_ as the active source control provider. 
  3. Once the active source control provider has been selected, several changes will be automatically applied to Visual Studio: 
  4. The _Source control provider toolwindow_ automatically launches. The toolwindow can now be launched at any time by pressingView &gt;_Source Control Provider Toolwindow_
  5. The _Source control provider toolwindow_ contains a toolbar with a single button. Pressing the button causes the the text and text field to change colors. 
  6. A new page appears in the Options dialog. You can now navigate to Tools &gt; Options &gt; Source Control &gt; _SccProviderOptions_
  7. The Sample Source Control Toolbar provides AddToSourceControl, Checkin, Checkout, and Offline commands. The toolbar can be accessed fromView &gt; Toolbars &gt; Sample Source Control Toolbar**
  8. Toolbar commands have also been added to the right-click menu of the Solution Explorer 
  9. The visibility of menu commands, tool window, and Options page is controlled by the active state of the provider. They are not visible when the provider is not active. 



**Details**

The provider demonstrates basic source control functionality:

  1. The sample does not implement a real source control store. To track which files in a project are controlled, the sample will create a projectname.storage text file on the hard drive that contains the list of controlled files in that project. 
  2. To simplify things, if a project is added to the solution after the solution was added to source control, that project will not be automatically controlled. The user must explicitly use the _AddToSourceControl_ command to add it to source control. (Exceptions are solution folders, which are controlled automatically after closing and reopening the solution) 
  3. Controlled solutions may be opened from disk.
  4. Controlled files can be checked out and checked in, but these actions only toggle the read-write attribute of the files. Source control status of files will be inferred from the file attributes on local disk. 



The _SccProvider_ class derives from theIVsPersistSolutionProps** interface
to persist properties in the solution file. Uses:SolutionIsControlled**
indicates the controlled status of the solution.SolutionBindings** shows
the solution location in the source control database.

The _Use Scc Offline_ command is visible in the right-click menus of the
Solution Explorer. _Use Scc Offline_ acts like a toggle that allows projects
to be taken online or offline. The _Sample Source Control Provider_ options
key in the .suo file stores the offline status of a project. Other source
control commands are not affected by the offline status of a project.

The _SccProviderService_ class implements basic QueryEdit / QuerySave
functionality. _OnEdit_ prompts the user to check out the file, edit in
memory, or cancel the edit. _OnSave_ prompts the user to check out the file,
skip the save, save as a new file, or cancel the operation. _OnSave_ code
triggers only if files have been edited in memory or if the user forces the
file to be saved when it is already checked in.

_SccProviderService_ implements theIVsTrackProjectDocumentsEvents2**
interface to intercept project-change events in the integrated development
environment (IDE). Uses:OnAfterRenameFiles** renames files in source
control whenever they are renamed in the project.OnQueryRemoveFiles** warns
the user when they are deleting a file that is under source control.

**Project Files**

* **AssemblyInfo.cs**

This file contains assembly custom attributes.

* **SccProvider.cs*

This file contains the Package implementation. It also handles the enabling
and execution of source control commands.

* **CommandId.cs**

This is a list of GUIDs specific to this sample, especially the package GUID
and the commands group GUID. It also includes GUIDs of other elements used by
the sample.

* **DataStreamFromComStream.cs**

Implements a Stream object that wraps an IStream COM object. Facilitates the
read/write of solution user options from an IStream by using a BinaryFormatter
object that serializes data to and from Stream objects

* **DlgQueryEditCheckedInFile.cs**

A Form that is displayed when a checked in file is edited. The user can choose
to check out the file, continue editing the file in memory, or cancel the edit

* **DlgQuerySaveCheckedInFile.cs**

A Form that is displayed when a checked in file is saved. The user can choose
to checkout the file, discard the in-memory changes and skip saving it, save
the file with a different name, or cancel the operation.

* **ProvideSolutionProps.cs**

Contains the implementation of a custom registration attribute that declares
the key used by the package to persist solution properties. When encountering
a solution containing this key, the IDE will know which package it has to call
to read that block of solution properties.

* **ProvideSourceControlProvider.cs**

This file contains the implementation of a custom registration attribute that
registers a source control provider. It is used to make the source control
provider visisble on theTools &gt; Options &gt; SourceControl &gt; Plug-Ins page

* **ProvideToolsOptionsPageVisibility.cs**

This file contains the implementation of a custom registration attribute that
defines the visibility of a tool window. It is used to make the tool window
implemented by the provider visible only when the provider is active (that is,
when the provider context UI has been asserted)

* **SccProviderOptions.cs**

This class derives from MsVsShell.DialogPage and provides the Options page. It
is responsible for handling Option page events, such as activation, apply, and
close. It hosts the SccProviderOptionsControl.

* **SccProviderOptionsControl.cs**

This class is a UserControl that will be hosted on the Options page. It has a
label to demonstrate display of controls in the page.

* **SccProviderService.cs**

Implementation of Sample Source Control Provider Service.

* **SccProviderStorage.cs**

Contains the implementation of a pseudo-source control storage. The class
creates a *.storage text file for each controlled project in the solution and
stores in it the list of the controlled files. The checked in and checked out
status of controlled files is inferred from the their attributes on disk.

* **SccProviderToolWindow.cs**

This class derives from ToolWindowPane, which provides the IVsWindowPane
implementation. It is responsible for defining the window frame properties
such as caption and bitmap. It hosts the SccProviderToolWindowControl.

* **SccProviderToolWindowControl.cs**

This class is a UserControl that will be hosted in the tool window. It has a
label to demonstrate display of controls in the page.

* **PkgCmd.vsct**

This file describes the menu structure and commands for this sample.

* **Resources.resx**

Resource strings (localizable).



**Functional Tests**

  * Verify the sample builds in all configurations
  * Verify that the sample was registered. The About box should list the product as installed
  * Verify that the provider is accessible in Tools, Options, SourceControl and can be made the active source control provider 
  * Verify that the menu commands are visible only when the provider is active, after it was displayed once 
  * Verify that the Options page is visible only when the provider is active
  * Verify that Sample Source Control Toolbar can be displayed
  * Verify prompts are dispayed when saving or editing a file that is currently checked in
  * Verify source control buttons are displayed in the right-click menu of the Solution Explorer



**Related topics**

  * [ SCC Provider Integration ](https://msdn.microsoft.com/en-us/library/bb166434(v=vs.140).aspx)
  * [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.140).aspx)



