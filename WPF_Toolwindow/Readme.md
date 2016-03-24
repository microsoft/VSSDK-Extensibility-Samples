

# WPF Toolwindow Sample

Provides tool windows which host Windows Forms controls and WPF
controls in Visual Studio.


* Technologies: Visual Studio 2015 SDK
* Topics: Visual Studio Shell, VSX

**Description**

This sample demonstrates how to create a package that provides tool windows
which host Windows Forms controls and WPF controls.

![image](C%23/ToolWindow.jpg)

Goals:

  * Exposing properties in the Properties window based on the selected item 
  * Tool window toolbars 
  * Tool window with visibility controlled by a UI Context (solution loaded) 
  * Tool window docked to another window as default start position 
  * Usage of tool window events 


This sample has a package (PackageToolWindow) and two tool windows
(PersistedWindowPane and DynamicWindowPane). Each of the tool windows hosts a
UserControl (PersistedWindowControl and DynamicWindowControl).

The first window is persisted (its hidden/shown state is preserved when Visual
Studio is restarted). It hosts a toolbar and demonstrates how to display
properties in the Properties window based on the current selection inside the
tool window.

The second window has dynamic visibility (based on a UI Context). When a
solution exists, the window is displayed. When no solution exists, it is
hidden. Note that if one manually shows/hides the tool window, this mechanism
would be disabled. To restore it, one can create a solution, show the tool
window, close the solution, and finally hide the tool window. The window also
provides a view helper which enables it to subscribe to tool window events.
These include events such as moved, resized, shown, hidden, and so on.

**Requirements**

[ Visual Studio 2015 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)



**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample**

  1. To run the sample, hit F5 or choose the **Debug &gt; Start Debugging** menu command. A new instance of Visual Studio will launch under the experimental hive. 
  2. Launch the **Persisted Window**: 
    * **View &gt; Other Windows &gt; Persisted Window**: The **Persisted Tool Window** appears as a tabbed window docked with **Solution Explorer**. 
    * This tool window has a persistant state, meaning that Visual Studio will remember if the tool window is open, and where it is located. If you close and relaunch Visual Studio, the **Persisted Window** will still be in the same location. 
  3. Move the **Persisted Tool Window** to dock on the left side of the Visual Studio integrated development environment (IDE). 
  4. Exit Visual Studio. Press **F5** again to start Visual Studio from the experimental instance. The **Persisted Tool Window** appears where it was when you exited Visual Studio. 
  5. Display the **Properties Window**: 
    * **View &gt; Properties Window**
  6. Click any of the window titles listed in the **Persisted Tool Window**. Note that the **Persisted Tool Window** displays the titles of all tool windows in the IDE and might include some that are not visible. The **Properties** window displays data about the selected tool window. 
  7. Close one of the tool windows listed in the **Persisted Tool Window** and click the **Refresh** icon in the toolbar. The window titles list is updated to indicate that the window is no longer visible. 
  8. Launch the **Dynamic Visibility Window**: 
    * **View &gt; Other Windows &gt; Dynamic Visibility Window**
  9. Hide the **Dynamic Visibility** Window by closing it.
  
<br>

**Related topics**

[ Tool Window Documentation ](https://msdn.microsoft.com/en-us/library/bb165390(v=vs.140).aspx)

[ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.140).aspx)


