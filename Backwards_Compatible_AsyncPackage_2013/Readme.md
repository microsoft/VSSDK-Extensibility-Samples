# Visual Studio 2013 backwards compatible async package
A sample on how to create a Visual Studio 2013 VSIX that supports asynchronous loading in Visual Studio 2015 and later.

* Technologies: Visual Studio 2013 SDK
* Topics: VSX

**Description**

This sample Visual Studio 2013 extension uses Visual Studio 2015 interops and helper
methods to create a package that auto loads synchronously in Visual Studio 2013 and supports
asynchronous auto load for Visual Studio 2015 and later where AsyncPackage support was added.

This allows extension authors to create a single auto loading package that works both in Visual Studio 2013
and also in future Visual Studio 2017 updates where synchronously auto loading packages will no longer be
allowed by default.

**Requirements**

Visual Studio 2013, Visual Studio 2013 VS SDK

**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample**

  1. To run the sample, hit **F5** or choose the **Debug &gt; Start Debugging** menu command. A new experimental instance of Visual Studio will launch. 
  2. The sample package will load at startup automatically and will use the synchronous path as indicated by dialog box.
  3. Install VSIX on Visual Studio 2017 and run IDE.
  4. Now the package will use asynchronous loading path.
  
**Related topics**

 * [ Visual Studio SDK Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/visual-studio-sdk)
 * [ AsyncPackage Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-use-asyncpackage-to-load-vspackages-in-the-background)
 * [ Visual Studio Threading ](https://github.com/Microsoft/vs-threading/blob/master/doc/threading_rules.md)


