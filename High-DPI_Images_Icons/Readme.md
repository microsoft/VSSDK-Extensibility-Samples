
# Title: High-DPI Images and Icons
**Abstract:** Use these helpers to make your images/icons in *VS 2013 only* scale on high dense displays.[ View this sample online](https://github.com/Microsoft/VSSDK-Extensibility-Samples).

* Technologies: Visual Studio 2013
* Topics: Visual Studio Shell, VSX
* Last Updated: 08/28/2015

**Description**

Visual Studio 2013 does not have built-in support for selecting different images for different DPI settings. 
Images are generally “scaled-up” at runtime to match the DPI scaling of the system. This sample provides 
you with the unmanaged helper libraries needed to enable high-DPI images and icons in your Visual Studio 2013 UI.
The managed DPI helper classes can be found in the Visual Studio 2013 SDK.

When building UI for Visual Studio 2015, you should use the Image Library/Catalog which is part of the 
Visual Studio 2015 SDK.

**Requirements**

[ Visual Studio 2013 ](http://www.microsoft.com/visualstudio/en-us/try/default.mspx#download)

[ Visual Studio 2013 SDK ](http://www.microsoft.com/en-us/download/details.aspx?id=40758)


**Goals**

  * Make your images/icons scale on high dense displays



The _OptionsPage_ sample contains classes that provide a Visual Studio Package
and custom Options Pages integrated into the Visual Studio IDE. The
OptionsPagePackage class provides custom options pages via the
**ProvideOptionsPages** attribute.



**Build the sample**

  * Download the zip file associated with the sample 
  * Unzip the sample to your machine 
  * Open the individual sample files to inspect them



**Run the sample**

  1. To run the sample, hit F5 or choose the **Debug &gt; Start Debugging** menu command. A new instance of Visual Studio will launch under the experimental hive. 
  2. Once loaded, open the Options dialog window: **Tools &gt; Options**
  3. A new category is available, titled: **My Managed Options (C#)**
  4. The new category contains two new pages: My Options, and Custom 



**Related topics**

* [ Visual Studio User Experience Guidelines ](http://aka.ms/VSUXGuidelines)

* [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.120).aspx)



