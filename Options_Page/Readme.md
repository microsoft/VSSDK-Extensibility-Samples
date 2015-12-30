
# Options Page Sample
Add custom pages to the Visual Studio Options dialog.

* Technologies: Visual Studio 2015 SDK
* Topics: Visual Studio Shell, VSX

**Description**

This sample adds two custom options pages into the standard Visual Studio
Options dialog. The sample demonstrates how to customize the presentation and
properties' persistence.

![image](C%23/Example.OptionsPage1.png)

**Requirements**

[ Visual Studio 2015 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)



**Goals**

  * Integrate custom options pages into the Visual Studios Options dialog window 
  * Properties persistence 
  * Custom user control as a UI for the property page 



The _OptionsPage_ sample contains classes that provide a Visual Studio Package
and custom Options Pages integrated into the Visual Studio IDE. The
OptionsPagePackage class provides custom options pages via the
**ProvideOptionsPages** attribute.

Use the **Microsoft.VisualStudio.Shell.DialogPage** class to implement an
options page. The sample implements two pages, both of which allow the user to
provide custom properties. _OptionsPageGeneral_ uses a standard Property
editor control for presentation. _OptionsPageCustom_ uses a custom control
(**OptionsCompositeControl**) for the UI.

The **ProvideProfile** attribute is used to provide persistence for the
package. The **DesignerSerializationVisibility** attribute is used to allow
persistence for each property of the options page.

**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample**

  1. To run the sample, hit F5 or choose the **Debug &gt; Start Debugging** menu command. A new instance of Visual Studio will launch under the experimental hive. 
  2. Once loaded, open the Options dialog window: **Tools &gt; Options**
  3. A new category is available, titled: **My Managed Options (C#)**
  4. The new category contains two new pages: My Options, and Custom 



**Related topics**

* [ Creating an Options Page ](https://msdn.microsoft.com/en-us/library/bb166195%28v=vs.140%29.aspx)

* [ Extending User Settings and Options ](https://msdn.microsoft.com/en-us/library/bb165657%28v=vs.140%29.aspx)

* [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.140).aspx)



