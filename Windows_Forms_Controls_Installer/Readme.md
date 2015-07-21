

# Title: Windows Forms Controls Installer Sample
**Abstract:** Load custom Windows Forms controls into the Toolbox.  View this sample online ](https://github.com/Microsoft/VSSDK-Extensibility-Samples)

* Technologies: Visual Studio 2015 SDK
* Topics: Visual Studio Shell, VSX
* Last Updated: 05/21/2015

**Description**

This sample demonstrates how to create a Visual Studio package (VSPackage)
that loads custom Windows Forms controls into the Toolbox. The toolbox has two
new items when a Windows Form is open in the Editor:

  * **MyCustomTextBox**: A normal Windows Forms control that adds a blank text field 
  * **MyCustomTextBoxWithPopup**: A custom ToolboxItem that adds a text box and pops up a dialogue box when added. 


**Requirements**

* [ Visual Studio 2015 ](http://www.microsoft.com/visualstudio/en-
us/try/default.mspx#download)

* [ Visual Studio 2015 SDK ](https://www.visualstudio.com/en-us/downloads
/visual-studio-2015-downloads-vs.aspx)


**Build the sample**

  * Download the zip file associated with the sample 
  * Unzip the sample to your machine 
  * Double click on the .sln file to launch the solution 


**Run the sample**

  1. To build and execute the sample, press **F5** after the sample is loaded 
  2. Create a new **Windows Forms Application** in C#, Visual Basic, or C++
  3. Open the **Windows Forms Designer** (Default: Form1.cs)
  4. Open the Toolbox: **View &gt; Toolbox**
  5. If your Windows Forms Designer is open, you should see a new tab at the bottom of the toolbox called **MyOwnTab**
  6. **MyOwnTab** contains two new toolbox items. Double-click on each of them to add them to the form 



**Related topics**

* [ Tool Window Documentation ](https://msdn.microsoft.com/en-
us/library/bb165390(v=vs.140).aspx)

* [ Editor Documentation ](https://msdn.microsoft.com/en-
us/library/dd885118(v=vs.140).aspx)

* [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-
us/library/bb166441(v=vs.140).aspx)



