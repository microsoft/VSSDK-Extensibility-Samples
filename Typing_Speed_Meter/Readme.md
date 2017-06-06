

# Typing Speed Meter Sample

Display an adornment with a typing speed meter in the Text
Editor
 


* Technologies: Visual Studio 2017 SDK

* Topics: Visual Studio Shell, VSX, Editor, Adornment



**Description**

This extension modifies the text editor to display a typing speed meter. The
sample uses an adornment to display the meter onscreen, and utilizes 
**IVsTextViewCreationListener** to track user keyboard input and update the
meter.

The binaries for this sample can also be installed via the extension manager.


**Requirements**

[ Visual Studio 2017 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)



**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample**

  1. To run the sample, hit F5 or choose the **Debug &gt; Start Debugging** menu command. A new instance of Visual Studio will launch under the experimental hive. 
  2. Once loaded, open any file in the Visual Studio Text Editor. 
  3. The typing speed meter can be seen in the top right corner of the screen. 


**Related topics**

 * [ Editor Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/editor-and-language-service-extensions)

* [ Visual Studio SDK Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/visual-studio-sdk)


