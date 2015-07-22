

# Title: Highlight Word Sample
**Abstract:**Highlight any words that match the word currently under the text
caret. 
[ View this sample online](https://github.com/Microsoft/VSSDK-Extensibility-Samples).

* Technologies: Visual Studio 2015 SDK
* Topics: Visual Studio Editor, VSX
* Last Updated: 05/18/2015

**Description**

This Visual Studio 2015 sample extension uses tagging and adornments to
highlight all occurences of the word currently under the caret.



**Requirements**

[ Visual Studio 2015 ](http://www.microsoft.com/visualstudio/en-
us/try/default.mspx#download)

[ Visual Studio 2015 SDK ](https://www.visualstudio.com/en-us/downloads
/visual-studio-2015-downloads-vs.aspx)



**Build the sample**

  * Download the zip file associated with the sample 
  * Unzip the sample to your machine 
  * Double click on the .sln file to launch the solution 



**Run the sample**

  1. To run the sample, hit **F5** or choose the **Debug &gt; Start Debugging** menu command. A new experimental instance of Visual Studio will launch. 
  2. Once loaded, open any file in the **Editor** window. 
  3. Place the text caret on a word that appears multiple times in the text file. The word and all other instances of that word will be highlighted blue 



**Project Files**

* **AssemblyInfo.cs**

This file contains assembly custom attributes.

* **HighlightWordTagger.cs**

This file provides the tagger class that will highlight the word under the
text caret and any additional instances.

* **HighlightWordTaggerProvider.cs**

This file is called by Visual Studio to generate the tagger



**Functional Tests**

  * Verify the sample builds in all configurations
  * Verify that the sample was registered. The About box should list the product as installed
  * Verify that the tagger highlights any additional instances of the currently highlighted word 


**Related topics**

  * [ Editor Documentation ](https://msdn.microsoft.com/en-us/library/dd885242(v=vs.140).aspx)
  * [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.140).aspx)



