
# High-DPI Images and Icons
Use these helpers to make your images/icons in *VS 2013 only* scale on high dense displays.

* Technologies: Visual Studio 2013
* Topics: Visual Studio Shell, VSX

**Description**

Visual Studio 2013 does not have built-in support for selecting different images for different DPI settings. 
Images are generally �scaled-up� at runtime to match the DPI scaling of the system. This sample provides 
you with the unmanaged helper libraries needed to enable high-DPI images and icons in your Visual Studio 2013 UI.
The managed DPI helper classes can be found in the Visual Studio 2013 SDK.

When building UI for Visual Studio 2015, you should use the Image Library/Catalog which is part of the 
Visual Studio 2015 SDK. Some of the samples in this repo have examples on how to utilize the Image Service/Catalog.

**Requirements**

[ Visual Studio 2013 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)

[ Visual Studio 2013 SDK ](http://www.microsoft.com/en-us/download/details.aspx?id=40758)


**Goals**

  * Make your images/icons scale properly on high dense displays


**How to incorporate these helpers into your code**

  * Clone the samples from the [ High-DPI_Images_Icons folder] (https://github.com/Microsoft/VSSDK-Extensibility-Samples/tree/master/High-DPI_Images_Icons)
  * Browse to the SDK documentation to understand how to incorporate these helpers into your extension/UI
    https://msdn.microsoft.com/en-us/library/bb166441.aspx 


**Related topics**

* [ Visual Studio User Experience Guidelines ](http://aka.ms/VSUXGuidelines)

* [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.120).aspx)



