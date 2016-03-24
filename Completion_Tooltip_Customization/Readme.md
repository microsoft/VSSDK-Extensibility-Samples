
# Completion Tooltip Customization Sample
Adjust the appearance of Completion Tooltips in Visual Studio.

* Technologies: Visual Studio 2015 SDK
* Topics: Visual Studio Editor, VSX

**Description**

This is a sample extension for the Visual Studio 2013 editor that replaces the
completion tooltip UI.

![image](C%23/bin/Debug/Example.CompletionTooltipCustomization.png)

**Requirements**

[ Visual Studio 2015 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)




**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample**

  1. To run the sample, hitF5** or choose the **Debug &gt; Start Debugging** menu command. A new experimental instance of Visual Studio will launch. 
  2. Once loaded, create an new project or load an existing one. 
  3. Type out a namespace or function name. eg: **System**
  4. While typing, observe the completion tooltip. The font should be italic and size 24. 


**Project Files**

* **AssemblyInfo.cs**

This file contains assembly custom attributes.

**CompletionTooltipCustomization.cs**

This file contains the Package implementation. Constructs new tooltips with
the desired values.



**Related topics**

  * [ Editor Documentation ](https://msdn.microsoft.com/en-us/library/dd885118(v=vs.140).aspx)
  * [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.140).aspx)



