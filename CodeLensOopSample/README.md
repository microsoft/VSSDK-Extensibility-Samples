# CodeLensOopProvider example 
A example for demonstrating how to use the public CodeLens API to create an out-of-proc extesnion that provides a CodeLens indictor showing most recent Git commits made to the source code.

* Technologies: Visual Studio 2017 SDK
* Topics: CodeLens

**Description**

This example generates a VSIX extension that packs two components:
* CodeLensOopProvider.dll: This assembly contains a CodeLens data point provider that retrieves most recent commits from git repo where the source code are commited. This assembly is loaded by the CodeLens service which runs out of the Visual Studio process.
* CodeLensOopProvidervsix.dll: This assembly provides a VSPackage which handles the command invoked when a users clicks on a commit from the commit indicator detail pane. This assembly is loaded by the Visual Studio process.

![image](src/CodeLensOopProvider.jpg)

**Getting Started**
1. Clone the repo
   `git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`
2. To run the example, hit F5 or choose the **Debug &gt; Start Debugging** menu command. A new instance of Visual Studio will launch under the experimental hive.
3. Open a solution from a local git repo, for example, this example solution.
4. Open a source code file, you will see the git commit lens indictor along with other CodeLens indicators in the editor.

**How it works**
1. 
