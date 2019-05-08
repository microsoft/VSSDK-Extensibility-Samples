# Open Folder Extensibility Sample

Enhance Visual Studio's "Open Folder" mode with support for custom file types and content.

* Technologies: Visual Studio 2017 SDK
* Topics: Open Folder


**Description**

This Visual Studio Package demonstrates how to extend "Open Folder" mode, new in
Visual Studio 2017, to support custom file types and content. This sample contains
three independent mini-samples that demonstrate how to:

 * Scan a custom source file format for symbols (to appear in Go To)
 * Attach actions to files in the Solution Explorer
 * Store and retrieve folder specific settings


**Requirements**

[ Visual Studio 2017 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)


**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`


**Run the sample**

_Note: "Open Folder" is a new feature in Visual Studio 2017.  This sample will not build under or work with earlier versions of Visual Studio._

 1. Prepare to run the sample by creating an empty folder to experiment with.  You will add a few text files to this folder to try out the sample.
 2. To run the sample, hit F5 or choose the **Debug &gt; Start Debugging** menu command. A new instance of Visual Studio will launch under the experimental hive.
 3. Select **File &gt; Open &gt; Folder...** and navigate to the folder you created earlier.
 4. Right click **Add &gt; New File** in the Solution Explorer and add a new `.txt` file. You can name the file anything but it must have the extension `.txt`.
 5. Right click the file you added and select **Word Count** or **Toggle Word Count Type** to try out the file action and settings samples.
 6. Enable **Show All Files** and check out the `.vs\VSWorkspaceSettings.json` file to see how settings are stored. This file will be updated whenever you click **Toggle Word Count Type**.
 7. Open and add some symbols to the file by prefixing lines with the backtick character. These symbols should appear in the GoTo window `Ctrl+Comma`.


**Source Code Overview**

This sample is organized into four folders:

 * **FileActionSample** Attaching actions to custom file types.
 * **SettingsSample** Store and retrieve folder specific settings.
 * **SymbolScannerSample** Extract symbols from custom file types.
 * **VSIX** Visual Studio package resources, including the VSCT file.


**Related topics**

* [ Visual Studio SDK Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/visual-studio-sdk)
