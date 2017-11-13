# Code Sweep Sample
Sweep a solution looking for any words that match search terms
in an XML Schema. Display any matches on the Task List in Visual Studio.

* Technologies: Visual Studio 2017 SDK
* Topics: MSBuild, VSX

**Description**

This sample allows the user to specify a set of terms to search for by
specifying a set of XML files containing the term definitions. The user-
configurable settings are stored in the project file. The scan can be invoked
either on command or as an integrated part of the build process. When the scan
is performed, a custom task provider causes hits, if any, to be shown in the
task list.

  * Demonstrates writing MSBuild tasks, which run as part of the build
  * Uses .NET remoting to communicate information from an MSBuild task back to the host IDE
  * Store and retrieve information in both MSBuild and non-MSBuild projects
  * Implements a custom task provider, including a custom toolbar and shortcut menu
  * Place commands in the Project menu and Solution Explorer shortcut menu, and handle them
  * Includes an algorithm to search for multiple terms across multiple streams of characters

![image](C%23/Example.CodeSweep.png)

**Requirements**

[ Visual Studio 2017 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)




**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample**

  1. To run the sample, hit **F5** or choose the **Debug &gt; Start Debugging** menu command. A new experimental instance of Visual Studio will launch.
  2. Once loaded, open an existing project or create a new one.
  3. Right-click on the project in the **Solution Explorer** window .
  4. Click the **CodeSweep** button. A dialog box should appear.
  5. In the dialog box, click **Add** to add files containing additional search terms to the sweep.
  6. Click **Scan Now** to scan the project.
  7. View scan results in the **Task List** window.



**Details**

This sample is divided into four projects:

  1. **Scanner**: class library that implements the scanning functionality
  2. **BuildTask**: MSBuild task implementation that allows the scan to run as part of the build process
  3. **VsPackage**: Visual Studio Package (VSPackage) implementation that provides a UI for the scanning functionality
  4. **Utilities**: generally useful utility functions used by other projects in the solution



Define search terms for the code sweep in XML files. See
_VsPackage\sample_term_table.xml _for an example of the supported format. Each
term may have zero or more **exclusions**, contexts in which the term will not
be counted as a hit. The list of term tables to use is specified on a per-
project basis, and can be configured in the dialog box invoked by the
_CodeSweep_ command. (available in the **Project** menu or **Solution
Explorer** shortcut menu)

Only files with supported extensions can be scanned; others are ignored. This
restriction exists so that large binary files can be avoided if desired. The
list of supported extensions is found in _extensions.xml_. Add new extensions
here to support them. Extensions.xml is copied to the user's _Application
Data\Microsoft\CodeSweep_ folder on first run.

The user can explicitly invoke a scan at any time, or configure MSBuild to run
a scan at any time. Use the configuration dialog box in either case. If the
scan is enabled to run during build, it will be run in command-line builds as
well as builds in the IDE.

Scan results are sent to the Task List. To see them, click _CodeSweep _in the
provider drop-down list on the **Task List** toolbar. Double-click a result in
the list to go to its location.

The CodeSweep Task List toolbar contains four buttons:

  1. **Stop Scan**:
  2. **Repeat Last Scan**:
  3. **Ignore**: Mark the selected result(s) as ignored, they will no longer be shown in the task list. (Including future scans)
  4. **Show Ignored Instances**: Add ignored instances back to task list



**Related topics**

  * [ Toolwindow Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/extending-and-customizing-tool-windows)
  * [ Menu Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/extending-menus-and-commands)
  * [ Visual Studio SDK Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/visual-studio-sdk)
