
# Reference Package Sample

Provides the minimum requirements that a package needs to work
as an extension in Visual Studio.

* Technologies: Visual Studio 2015 SDK
* Topics: Visual Studio Shell, VSX

** Description**

This sample demonstrates how to add an entry into the Visual Studio Help
dialog. This is the bare minimum requirement for a functional Visual Studio
Package.

![image](C%23/Package.jpg)

**Requirements**

[ Visual Studio 2015 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)



**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample**

  1. To run the sample, hit **F5** or choose the **Debug &gt; Start Debugging** menu command. A new experimental instance of Visual Studio will launch. 
  2. Once loaded, navigate to **Help &gt; About Microsoft Visual Studio**. 
  3. There is a new entry on the list titled _C# Package Reference Sample_. 



**Project Files**

* **AssemblyInfo.cs**

This file contains assembly custom attributes.

* **BasicPackage.cs**

This file contains the Package implementation. Adds a new entry in the
Help&gt;About dialog.



**Functional Tests**

  * Verify the sample builds in all configurations
  * Verify that the sample was registered. The About box should list the product as installed
  * Verify that the example can be uninstalled from **Tools &gt; Extensions and Updates**



**Related topics**

  * [ Editor Documentation ](https://msdn.microsoft.com/en-us/library/dd885242(v=vs.140).aspx)
  * [ Menu Documentation ](https://msdn.microsoft.com/en-us/library/bb165937(v=vs.140).aspx)
  * [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.140).aspx)



