

# Basic Source Control Provider Sample
Provides the basic requirements to implement a Source Control
Provider in Visual Studio.

* Technologies: Visual Studio 2017 SDK
* Topics: MSBuild, VSX

**Description**

This sample demonstrates how to register a source control provider with Visual
Studio that can be selected as active source control provider. NOTE: This
sample does not actually implement any source control, just provides the
framework necessary to implement one.

  * Implement a source control provider package 
  * Expose an Options page visible only when the provider is active 
  * Expose a tool window visible only when the provider is active 
  * Display menu items only when the provider is active 

![image](C%23/Example.BasicSccProvider.jpg)

**Requirements**

[ Visual Studio 2017 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)



**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`


**Run the sample**

  1. To build and execute the sample, open the .sln file, press **F5** after the sample is loaded  
  2. Once loaded, navigate to **Tools &gt; Options &gt; Source Control &gt; Plug-In Selection** and set _Managed Source Control Sample Basic Provider_ as the active source control provider. 
  3. Once the active source control provider has been selected, several changes will be automatically applied to Visual Studio: 
  4. The _Source control provider toolwindow_ automatically launches. The toolwindow can now be launched at any time by pressing **View &gt; **_Source Control Provider Toolwindow_
  5. The _Source control provider toolwindow_ contains a toolbar with a single button. Pressing the button causes the the text and text field to change colors. 
  6. A new page appears in the Options dialog. You can now navigate to **Tools &gt; Options &gt; Source Control &gt;** _SccProviderOptions_
  7. A new command appears in the **Tools** menu. Pressing _Scc Command_ will prompt a checkmark to be displayed next to the button. Pressing it again will cause the checkmark to disappear. 



**Project Files**

* **AssemblyInfo.cs**

This file contains assembly custom attributes.

* **BasicSccProvider.cs**

This file contains the Package implementation. It also handles the enabling
and execution of source control commands.

* **CommandId.cs**

This is a list of GUIDs specific to this sample, especially the package GUID
and the commands group GUID. It also includes GUIDs of other elements used by
the sample.

* **ProvideSourceControlProvider.cs**

This file contains the implementation of a custom registration attribute that
registers a source control provider. It is used to make the source control
provider visisble on the **Tools &gt; Options &gt; SourceControl &gt; Plug-Ins** page

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



**Related topics**

  * [SCC Provider Integration](https://docs.microsoft.com/en-us/visualstudio/extensibility/internals/creating-a-source-control-vspackage)
  * [Visual Studio SDK Documentation](https://docs.microsoft.com/en-us/visualstudio/extensibility/visual-studio-sdk)



