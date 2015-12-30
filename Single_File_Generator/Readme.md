
# Single File Generator Sample
Create a Single File Generator that uses an XML source file to
generate a new C# file in Visual Studio.

* Technologies: Visual Studio 2015 SDK
* Topics: Visual Studio Shell, MSDBuild, VSX

**Description**

A single file generator pulls information from a source file and generates a
new file from the data. This sample single file generator uses an XML Schema
to pull data from an XML file and generate a C# file. The generator leverages
CodeDom to generate C# source code. It also demonstrates how to validate an
XML document against a schema and communicate errors through the Error List.

**Requirements**

[ Visual Studio 2015 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)



![image](C%23/Example.SingleFileGenerator.jpg)

**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample**

  1. To run the sample, hit **F5** or choose the **Debug &gt; Start Debugging** menu command. A new experimental instance of Visual Studio will launch. 
  2. Once loaded, create a new project. Ex:_ WpfApplication_
  3. Add a new XML file to the project. **File &gt; Add &gt; New File**
  4. Populate the new XML file with content using the schema from _XmlClassGeneratorSchema.xsd_. For a working example, Copy/Paste the code from _Example.XmlFile.xml_ into your new XML file. 
  5. Open the **Properties** window for the xml file and set **CustomTool** to _XmlClassGenerator_. 
  6. Save the XML file. This triggers the single file generator to run and generate a new C# file. 
  7. The new C# file is added to the project with the same name as the XML file. This new file appears in **Solution Explorer** as a dependent of the original XML file node. 
  8. Any errors will be reported in the **Error List** toolwindow. 



**Project Files**

* **AssemblyInfo.cs**

This file contains assembly custom attributes.

* **BaseCodeGenerator.cs**

Abstract class that implements the _IVsSingleFileGenerator_ interface.

* **BaseCodeGeneratorWithSite.cs**

Abstract class that inherits from BaseCodeGenerator and implements the
_IObjectWithSite_ interface.

* **XmlClassGenerator.cs**

The single file generator class.

* **SourceCodeGenerator.cs**

Static class that contains the CodeDom code used to generate source code.

* **XmlClassGeneratorSchema.xsd**

XML Schema document tells the generator how to convert source code.

* **Strings.resx**

Resource strings (localizable).



**Functional Tests**

  * Verify the sample builds in Debug Configuration
  * Verify the sample builds in Release Configuration
  * Verify that the generator is properly registered when building
  * Verify that the generator works
  * Verify that the generator fails for bad XML



** Related topics **

  * [ Editor Documentation ](https://msdn.microsoft.com/en-us/library/dd885242(v=vs.140).aspx)
  * [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.140).aspx)



