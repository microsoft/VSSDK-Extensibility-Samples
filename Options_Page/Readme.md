
# Title: Options Page Sample
**Abstract:** Add custom pages to the Visual Studio Options dialog.

* Technologies: Visual Studio 2015 SDK
* Topics: Visual Studio Shell, VSX
* Last Updated: 05/14/2015

**Description**

This sample adds two custom options pages into the standard Visual Studio
Options dialog. The sample demonstrates how to customize the presentation and
properties' persistence.

![image](C%23/Example.OptionsPage1.png)

**Requirements**

[ Visual Studio 2015 ](http://www.microsoft.com/visualstudio/en-us/try/default.mspx#download)

[ Visual Studio 2015 SDK ](https://www.visualstudio.com/en-us/downloads/visual-studio-2015-downloads-vs.aspx)

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



**Run the sample**

  1. To run the sample, hit F5 or choose the **Debug &gt; Start Debugging** menu command. A new instance of Visual Studio will launch under the experimental hive. 
  2. Once loaded, open the Options dialog window: **Tools &gt; Options**
  3. A new category is available, titled: **My Managed Options (C#)**
  4. The new category contains two new pages: My Options, and Custom 



**Related topics**

* [ Creating an Options Page ](https://msdn.microsoft.com/en-us/library/bb166195%28v=vs.140%29.aspx)

* [ Extending User Settings and Options ](https://msdn.microsoft.com/en-us/library/bb165657%28v=vs.140%29.aspx)

* [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.140).aspx)



