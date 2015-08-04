

# Title: Designer View Over XML Editor Sample

**Abstract:** Provides a WPF Editor when editing XML files that have the '.vstemplate' file extension.[ View this sample online ](https://github.com/Microsoft/VSSDK-Extensibility-Samples/)

* Technologies: Visual Studio 2015 SDK
* Topics: Visual Studio Shell
* Last Updated: 06/24/2015

**Description**

This sample demonstrates how to create an extension with a WPF-based Visual
Designer for editing XML files with a specific schema (XSD) in coordination
with the Visual Studio XML Editor. In this sample we implement a basic view
for .vstemplate files.


**Requirements**

[ Visual Studio 2015 ](http://www.microsoft.com/visualstudio/en-us/try/default.mspx#download)

[ Visual Studio 2015 SDK ](https://www.visualstudio.com/en-us/downloads/visual-studio-2015-downloads-vs.aspx)



**Build the sample**

  * Download the zip file associated with the sample 
  * Unzip the sample to your machine 
  * Double click on the .sln file to launch the solution 



**Run the sample**

  1. To run the sample, hit**F5** or choose the**Debug &gt; Start Debugging** menu command. A new experimental instance of Visual Studio will launch. 
  2. Once loaded, press the**Open File** button. (Ctrl + O) 
  3. Browse to the TestTemplates sub-directory within the solution and open a file with the '.vstemplate' file extension. 
  4. A new tab opens with the contents of the file laid out in the fields of a WPF form 
  5. Navigate to the**View** menu and click on**Code**. 
  6. An additional tab opens with the contents of the file formatted by the XmlEditor 



**Project Files**

 * **AssemblyInfo.cs**  
 
This file contains assembly custom attributes.

 * **VsTemplateDesignerPackage.cs** 
 
Registers the designer, via ProvideXmlEditorChooserDesignerView, as the preferred editor view for files with the .vstemplate extension and indicated schema .

* **EditorFactory.cs** 

Determines if the document to be edited already exists (was already opened in the Xml Editor view), rather than assuming it must be created; creates the designer’s EditorPane as the new Editor.

* **EditorPane.cs**

Creates the sited designer control and associated XmlModel for the file and
text buffer.

* **IViewModel.cs**

Expresses the interface needed to bind the designer controls to the XmlSchema.

* **ViewModel.cs**

Implements IViewModel and manages the events needed to synchronize data
between the fields in the designer and the underlying XML document, which may
also be seen in the XML Editor.

* **VsDesignerControl.xaml[.cs]**

Implements the WPF controls expressing the designer interface and binds them
to the ViewModel.

* **VsTemplateSchema.cs**

XML schema file generated via xsd.exe vstemplate.xsd /classes /e
/n:MyNameSpace.


**Related topics**

  * [ WPF Designer Overview ](https://msdn.microsoft.com/en-us/library/bb514528(v=vs.90).aspx)
  * [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.140).aspx)



