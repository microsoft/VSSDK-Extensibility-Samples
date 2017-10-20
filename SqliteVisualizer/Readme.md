# SQLite native Debugger Visualizer Sample
Visualize a SQLite instance while native debugging in Visual Studio.

* Technologies: Visual Studio 2017 SDK

* Topics: Visual Studio Debugger, SQLite
 

**Description**

This sample provides a complete example of how to create a native debug visualizer in Visual Studio.

  * The project sample will create a [VSIX](https://docs.microsoft.com/en-us/visualstudio/extensibility/shipping-visual-studio-extensions) that can be used to installed via the extension manager. 

**Requirements** 

[ Visual Studio 2017 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)

**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample** 

  1. To run the sample, hit F5 or choose the **Debug &gt; Start Debugging** menu command. A new instance of Visual Studio will launch under the experimental hive. 
  2. Once loaded, open a native project in the newly launched Visual Studio that uses [SQLite](https://sqlite.org/).
  3. Set a breakpoint near a variable of type ```sqlite3 *```.
  4. Hit F5 or choose the **Debug &gt; Start Debugging** menu command in the newly launched Visual Studio to launch the loaded project.
  5. When the breakpoint set above is hit, press the small magnifying glass in the [Locals window](https://docs.microsoft.com/en-us/visualstudio/debugger/autos-and-locals-windows). 

**Related topics** 

* [ Viewing Data In The Debugger ](https://docs.microsoft.com/en-us/visualstudio/debugger/viewing-data-in-the-debugger)

* [ Visualizer Interface Documentation ](https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.debugger.interop.ivscppdebuguivisualizer)

* [ Natviz Documentation ](https://docs.microsoft.com/en-us/visualstudio/debugger/create-custom-views-of-native-objects)

* [ Visual Studio SDK Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/visual-studio-sdk)


