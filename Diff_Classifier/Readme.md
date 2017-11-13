# Diff Classifier Sample
Provides a classifier with color highlighting that affects files
with .diff or .patch extensions.

* Technologies: Visual Studio 2017 SDK
* Topics: Visual Studio Editor, VSX



**Description**

This Visual Studio 2015 sample classifier provides color formatting for files
with .diff or .patch filename extensions. Uses the classification API.

![image](C%23/Example.DiffClassifier.png)

**Requirements**

[ Visual Studio 2017 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)



**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample**

  1. To run the sample, hit **F5** or choose the **Debug &gt; Start Debugging** menu command. A new experimental instance of Visual Studio will launch. 
  2. Once loaded, open a file that has the _.diff_ or _.patch_ filename extension. 
  3. Observe text coloring on lines that are prefixed with the defined sumbols: **---**, **+++**, **-**, **+**, **&lt;**,**&gt;** and **@@**. 
  4. This sample includes two files to test this functionality: _Test.diff_ and _Test.patch_. 



**Project Files**

**AssemblyInfo.cs**

This file contains assembly custom attributes.

**DiffClassificationDefinitions.cs**

This file contains the definitions for the various classifications, as well as
the coloring to apply to each classification type.

**DiffClassifier.cs**

This file defines the class that searches a given span of text to locate and
evaluate identifiers.

**DiffClassifierProvider.cs**

This file implements the provider to classify .diff and .patch files

**Test.diff / Test.patch**

Test files used to verify that the classifier is working properly. The text in
these files will display in color



**Functional Tests**

  * Verify the sample builds in all configurations
  * Verify that the sample was registered. The About box should list the product as installed
  * Verify that files with the .diff or .patch file extension display colored text as defined in _DiffClassificationDefinitions.cs_



**Related topics**

  * [ Editor Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/editor-and-language-service-extensions)
  * [ Visual Studio SDK Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/visual-studio-sdk)



