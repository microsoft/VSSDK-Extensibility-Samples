
# Build Progress Bar Sample
Display a Progress Bar inside a Toolwindow in Visual Studio.

* Technologies: Visual Studio 2015 SDK
* Topics: Visual Studio Shell, VSX


**Description**

This Visual Studio Package provides a new tool window called "Build Progress".
The window displays a WPF ProgressBar that indicates percentage completion of
the current solution build.

![image](C%23/BuildProgressBar/Example.BuildProgressBar.png)

**Requirements**

[ Visual Studio 2015 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)



**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample**

  1. To build and execute the sample, open the .sln file, press **F5** after the sample is loaded   
  2. Once loaded, choose the **View &gt; Other Windows &gt; Build Progress** menu command.
  3. A new tool window called "Build Progress" will open, displaying a WPF ProgressBar control. You can move, resize, or dock this tool window however you like. 
  4. Open an existing buildable solution or create a new one using the **File &gt; New &gt; Project** menu command. 
  5. Once the solution has loaded, build the solution by running the **Build &gt; Build Solution** menu command. 
  6. You should see the progress bar's value and text change as each project in the solution is built. The file is displayed in the editor with colored text 



**Source Code Overview**

The source code in this sample demonstrates several techniques you can use to
write your own packages:

  * How to add a menu command to the **View &gt; Other Windows** menu group. 
  * How to display WPF content on a tool window 
  * How to monitor solution load/unload events 
  * How to monitor solution build events 
  * How to check and monitor the value of a Visual Studio Shell property (VisualEffectsAllowed)

Our Visual Studio Package implements the interfaces
**IVsShellPropertyEvents**, **IVsSolutionEvents**, and
**IVsUpdateSolutionEvents2** so that it can receive notification of shell
property changes, solution load/unload events, and solution build events. In
order to receive calls to these interface methods, it must advise the
appropriate service providers that we want to be notified, as demonstrated in
ProgressBarPackage's Initialize method.

We monitor the value of the VisualEffectsAllowed shell property in order to
modify how the progress bar is displayed. We also monitor solution load/unload
events to keep track of how many projects are currently loaded in the
solution. Lastly, we monitor solution build events in order to update the
value and text of the progress bar.



**Project Files**

* **BuildProgressBar.vsct**

Defines layout and type of commands in the package, namely the Build Progress
menu command

* **BuildProgressToolWindow.cs**

Implements the tool window functionality, and owns an instance of the
ProgressBarControl

* **ProgressBarControl.xaml**

Defines the XAML for the ProgressBarControl, which is the content hosted on
the tool window

* **ProgressBarControl.xaml.cs**

Defines the code-behind for the ProgressBarControl. This gives us greater
control over the behavior of the ProgressBarControl

* **ProgressBarPackage.cs**

Implements the Visual Studio Package, where we monitor events



**Related topics **

* [ Tool Window Documentation ](https://msdn.microsoft.com/en-us/library/bb165390(v=vs.140).aspx)

* [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.140).aspx)



