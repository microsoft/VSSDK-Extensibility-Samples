# Visual Studio Extensibility Samples
These samples demonstrate how to customize the Visual Studio editor using the 
[Visual Studio SDK](https://msdn.microsoft.com/en-us/library/bb166441.aspx). You 
can use editor extensions to customize the appearance and behavior of Visual 
Studio. The following are some of the ways in which you can extend Visual Studio: 

* Add commands, buttons, menus, and other UI elements to the IDE
* Add tool windows for new functionality
* Add support in Visual Studio for new programming languages
* Add refactoring or language analyzers to fix and improve code
* Add a custom project type or new project or item templates
* Reach millions of developers via the Visual Studio Gallery

A quick summary of the samples included here:

|                              Sample Name | Description                                                                 |
| ---------------------------------------- | --------------------------------------------------------------------------- |
|            Basic_Source_Control_Provider | Shows how to add hooks for a simple source code provider                    |  
|                       Build_Progress_Bar | Displays a tool window written in WPF showing build progress                |
|                           Caret_Fish_Eye | Uses line transformation API to zoom lines in proportion to cursor distance |
|                               Code_Sweep | Searches for words matching terms in an XML schema across a solution        |
|                                Combo_Box | Place combo boxes in a Visual Studio toolbar                                |
|                         CommandTargetRGB | Shows how to create a multi-instance tool window                            |
|         Completion_Tooltip_Customization | Replaces the completion tooltip UI                                          |
|                          Diff_Classifier | Classifier with color highlighting                                          |
|                      Editor_With_Toolbox | Creates a custom toolbox associated with a specific file extension          |
|                           Highlight_Word | Highlight any words that match the word currently under the text cursor     |
|                     Intra-text_Adornment | Text adornment that replaces hexadecimal color values with color swatches   |
|                                Lightbulb | Creates a custom lightbulb to set text case in text files                   |
|                               MSDNSearch | Implements MSDN search functionality directly into Quick Search             |
|                        Menu_And_Commands | Demonstrates how to add commands to various places in the IDE               |
|                 Ook_Language_Integration | Implements language support for a simple programming language               |
|                             Options_Page | Shows how to add custom pages to the Tools / Options dialog                 |
|                        Reference_Package | Boilerplate containing minimum requirements for a functional extension      |
|                       Reference_Services | Shows how to create and consume services as a service provider              |
| RunningDocumentTable(RDT)_Event_Explorer | Creates an explorer grid to log events in a tool window                     |
|                    Single_File_Generator | Creates a file generator that uses XML as basis for creating a new C# file  |
|             Source_Code_Control_Provider | More complex example of a source code provider                              |
|                      Todo_Classification | Classifier that highlights TODO comments and displays a matching glyph      |
|                       Typing_Speed_Meter | Displays an adornment with a typing speed indicator in the Text Editor      |
|                          WPFDesigner_XML | WPF-based visual designer for editing .vstemplate XML files                 |
|                           WPF_Toolwindow | Provides a sample toolwindow that can host a WPF or WinForms control        |
|         Windows_Forms_Controls_Installer | Loads custom Windows Forms controls inside the toolbox                      |

<!-- |          IronPython_Integrated_Shell |Demonstrates how to create an integrated shell for IronPython  
    |   IronPython_Integration | Demonstrates a custom project type and WinForms designer for IronPython
|   IronPython_Studio_VS_Shell_Isolated | Example of using the isolated shell to host a language service -->

Some other useful resources:

* If you've never written a Visual Studio extension before, you can find more 
information about developing features at 
[Starting to Develop Visual Studio Extensions](https://msdn.microsoft.com/en-us/library/bb166030.aspx).
* Writing an extension for Visual Studio Online? Check out the 
[VSO extension samples](https://github.com/Microsoft/vso-extension-samples) at 
the dedicated repository.  
* In addition to these samples, many completed extensions have been published to 
GitHub or other repositories. Here's a [list of other community Visual Studio 
extensions](http://microsoft.github.io/extendvs/).
* You can publish your completed extension to the 
[Visual Studio Gallery](http://visualstudiogallery.com), where you can also get
more information about joining the [Visual Studio Partner Program](https://vsipprogram.com/).  

Thanks for your support for Visual Studio - we're excited to see what extensions
*you* build!
