# LightBulb API Sample
This sample provides custom LightBulb functionality for txt
files in Visual Studio.

* Technologies: Visual Studio 2017 SDK
* Topics: Visual Studio Editor, VSX

**Description**

This sample for Visual Studio 2017 modifies the LightBulb API to provide
custom content for .txt files. Whenever the text caret is placed over a word,
the lightbulb adornment appears. The lightbulb will provide to options, you
can choose to make the word entirely uppercase or entirely lowercase.

![image](TestLightBulb/Resources/Example.LightBulb.png)

**Requirements**

[ Visual Studio 2017 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)




**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample**

  * To build and execute the sample, press F5 after the sample is loaded 
  * Open a .txt file in the Visual Studio Editor
  * Click on a word in the text file. (place the text caret on that word)
  * A lightbulb appears on the same line as the selected word
  * Click on the lightbulb to see the list of suggested actions
  * The selected word can be converted to all uppercase or all lowercase characters



**Related topics**

* [ Light Bulb Documentation ](https://docs.microsoft.com/en-us/visualstudio/ide/quick-actions)
* [ Editor Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/editor-and-language-service-extensions)
* [ Visual Studio SDK Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/visual-studio-sdk)


