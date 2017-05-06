
# ErrorList Sample
A sample of a VSIX that generates errors in error list when you make spelling errors in the editor.

* Technologies: Visual Studio 2017 SDK
* Topics: Visual Studio Editor, VSX

**Description**

This sample generates the Messages in error list when you make spelling errors in the text that you type in the editor


![image](CS/Example_Spellingerror.png)

**Requirements**

[ Visual Studio 2015 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)



**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample**

  1. To run the sample, hit F5 or choose the **Debug &gt; Start Debugging** menu command. A new instance of Visual Studio will launch under the experimental hive. 
  2. Once loaded, create new project (e.g. a C# ConsoleApplication) 
  3. Add some comments in Program.cs file and make some spelling error in the comments
  4. Open Error List view, you will see the spelling errors
  


**Related topics**

* [ Toolbox Documentation ](https://msdn.microsoft.com/en-us/library/ee712574(v=vs.140).aspx)

* [ Editor Documentation ](https://msdn.microsoft.com/en-us/library/dd885242(v=vs.140).aspx)

* [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.140).aspx)



