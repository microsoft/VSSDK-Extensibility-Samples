

# Title: Menu and Commands Sample
**Abstract:** Create menu and command items and visualize them inside Visual
Studio’s menus and toolbars. [ View this sample online](https://github.com/Microsoft/VSSDK-Extensibility-Samples).

* Technologies: Visual Studio 2015 SDK
* Topics: Visual Studio Shell, VSX, Menu
* Last Updated: 05/21/2015

**Description**

This Visual Studio Package demonstrates how to create menu and command items
and visualize them inside Visual Studio’s menus and toolbars.

Goals:

  * Adding a menu item / command to Visual Studio and handling it 
  * Placing commands in various places (Solution Explorer toolbar, custom toolbar, Tools menu, editor context menu) 
  * Dynamic text in menu items 
  * Associating a keybinding (keyboard shortcut) to a menu item 



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

  1. To run the sample, hit F5 or choose the **Debug &gt; Start Debugging** menu command. A new instance of Visual Studio will launch under the experimental hive. 
  2. Once loaded, select the **View &gt; Output** menu command to display the **Output** window. 
  3. Select **Tools &gt; C# Command Sample**. A message appears in the **Output** window. 
  4. Select **Tools &gt; C# Text Changes**. 
  5. Open the **Tools** menu again and note that the text for that menu item has changed to indicate how many times you chose the command. 
  6. Select **Tools &gt; C# Dynamic Visibility 1**. 
  7. Click **Tools** menu again and note that the menu item has disappeared and been replaced by a **C# Dynamic Visibility 2** command. Click it and **C# Dynamic Visibility 1** returns. 



**Source Code Overview**

The main focus of this sample is the VSCT file containing the definition of
these elements. The code is minimal. The event handler functions that are
called when the user executes the commands simply write a message on the
Output window. The only exceptions are the callback for the menu items with
dynamic properties (text or visibility). In this case, the properties will be
changed according to some logic.

This sample is organized into four main areas:

  1. How to create simple menu and command items. 
  2. How to place them inside other elements provided by other packages (for example, default Visual Studio menus or toolbars) or by this same package. 
  3. How to modify the text or the visibility of a command at runtime. 
  4. How to associate a keyboard accelerator to a command. 



Inside the VSCT file, the command definition section defines a new toolbar,
some menu groups, and a few commands. This section contains all the
interesting parts about areas 1 and 3. The button subsection of the command
definition section includes the usage of different visibility flags. These
flags allow us to tell Visual Studio that we want to programmatically change a
specific set of command properties when our package is loaded.

The second section in the VSCT file, the command placement section, is of
interest for area 2 listed above. In this section, you can see how to place a
command inside a menu group or a menu group inside a menu.

The last section, the key binding section, allows associations between
commands and keyboard accelerators.



**Related topics**

* [ Menu and Command Documentation ](https://msdn.microsoft.com/en-
us/library/bb165937(v=vs.140).aspx)

* [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-
us/library/bb166441(v=vs.140).aspx)



