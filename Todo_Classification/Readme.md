

# ToDo Classification Sample
Highlight any instances of the text 'ToDo' in the Visual Studio
Editor and display a glyph in the corresponding line.

* Technologies: Visual Studio 2017 SDK

* Topics: Visual Studio Shell, VSX
 

** Description**

This sample provides a general purpose TodoTagger as well as classification
and a glyph factory. Anytime _ToDo_ appears in the text editor, it will be
highlighted and a glyph will be displayed on that line.

  * The binaries for this sample can also be installed via the extension manager. 
  * This is the sample code written during the Visual Studio Ecosystem Summit presentation ** I Want Coloring: A scenario based look at the new editor**

![image](C%23/Example.ToDoClassification.png)

**Requirements** 

[ Visual Studio 2017 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)



**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample** 

  1. To run the sample, hit F5 or choose the ** Debug &gt; Start Debugging** menu command. A new instance of Visual Studio will launch under the experimental hive. 
  2. Once loaded, open any file in the Text Editor. 
  3. Type _ToDo_ anywhere in the Text Editor. 
  4. _ToDo_ is highlighted, and a glyph is displayed on that line in the glyph column. 



**Related topics** 

* [ Editor Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/editor-and-language-service-extensions)

* [ Visual Studio SDK Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/visual-studio-sdk)


