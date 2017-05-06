# Visual Studio Extensibility Samples

[![Join the chat at https://gitter.im/Microsoft/extendvs](https://badges.gitter.im/Join%20Chat.svg)](http://aka.ms/dyofat)
[![Build status](https://ci.appveyor.com/api/projects/status/7gjewm7eiwoa2ees?svg=true)](https://ci.appveyor.com/project/AlexEyler/vssdk-extensibility-samples)

These samples demonstrate how to customize the appearance and behavior of the 
Visual Studio IDE and editor. 
The following are some of the ways in which you can extend Visual Studio: 

* Add commands, buttons, menus, and other UI elements to the IDE
* Add tool windows for new functionality
* Add support in Visual Studio for new programming languages
* Add refactoring or language analyzers to fix and improve code
* Add a custom project type or new project or item templates
* Enable high-DPI/high resolutuion, theme-aware, and high contrast images and icons in your UI
  * For Visual Studio 2013, only high-DPI images/icons are supported; use the below sample High-DPI_Images_Icons
  * For Visual Studio 2015, samples that demonstrate using the Image Service/Catalog (for high-DPI, theme-aware, 
    and high contrast support) are denoted below with * and docs can be found [here] (http://aka.ms/VSImageService)

If you've never written a Visual Studio extension before, you can find out how to get started at [VisualStudio.com/integrate](https://aka.ms/vvnr38).

## Getting started

1. Clone the repo to download all samples ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository))

    `git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`
2. Find samples in the table below
3. Read the readme included with each sample and try it in Visual Studio

## Samples Overview
For more details see the readme included with each sample.

|                              Sample Name | Description                                                                                |
| ---------------------------------------- | ------------------------------------------------------------------------------------------ |
|           [Basic_Source_Control_Provider*](Basic_Source_Control_Provider/) | Shows how to add hooks for a simple source code provider                                   |  
|                      [Build_Progress_Bar*](Build_Progress_Bar/) | Displays a tool window written in WPF showing build progress                               |
|                           [Caret_Fish_Eye](Caret_Fish_Eye/) | Uses line transformation API to zoom lines in proportion to cursor distance                |
|                               [Code_Sweep](Code_Sweep/) | Searches for words matching terms in an XML schema across a solution                       |
|                                [Combo_Box](Combo_Box/) | Place combo boxes in a Visual Studio toolbar                                               |
|                         [CommandTargetRGB](CommandTargetRGB/) | Shows how to create a multi-instance tool window                                           |
|         [Completion_Tooltip_Customization](Completion_Tooltip_Customization/) | Replaces the completion tooltip UI                                                         |
|                          [Diff_Classifier](Diff_Classifier/) | Classifier with color highlighting                                                         |
|                      [Editor_With_Toolbox](Editor_With_Toolbox/) | Creates a custom toolbox associated with a specific file extension                         |
|                    [High-DPI_Images_Icons](High-DPI_Images_Icons/) | Use these helpers to make your images/icons in *VS 2013 only* scale on high dense displays |
|                           [Highlight_Word](Highlight_Word/) | Highlight any words that match the word currently under the text cursor                    |
|                     [Intra-text_Adornment](Intra-text_Adornment/) | Text adornment that replaces hexadecimal color values with color swatches                  |
|                               [LightBulb*](LightBulb/) | Creates a custom lightbulb to set text case in text files                                  |
|                               [MSDNSearch](MSDNSearch/) | Implements MSDN search functionality directly into Quick Search                            |
|                       [Menu_And_Commands*](Menu_And_Commands/) | Demonstrates how to add commands to various places in the IDE                              |
|                 [Ook_Language_Integration](Ook_Language_Integration/) | Implements language support for a simple programming language                              |
|                            [Options_Page*](Options_Page/) | Shows how to add custom pages to the Tools / Options dialog                                |
|                        [Reference_Package](Reference_Package/) | Boilerplate containing minimum requirements for a functional extension                     |
|                       [Reference_Services](Reference_Services/) | Shows how to create and consume services as a service provider                             |
| [RunningDocumentTable(RDT)_Event_Explorer](RunningDocumentTable%28RDT%29_Event_Explorer/) | Creates an explorer grid to log events in a tool window                                    |
|                    [Single_File_Generator](Single_File_Generator/) | Creates a file generator that uses XML as basis for creating a new C# file                 |
|            [Source_Code_Control_Provider*](Source_Code_Control_Provider/) | More complex example of a source code provider                                             |
|            [Source_Control_Provider_Status_Bar_Integration](Source_Control_Provider_Status_Bar_Integration/) | Display source control information in the Status Bar.                                             |
|                      [Todo_Classification](Todo_Classification/) | Classifier that highlights TODO comments and displays a matching glyph                     |
|                       [Typing_Speed_Meter](Typing_Speed_Meter/) | Displays an adornment with a typing speed indicator in the Text Editor                     |
|                          [WPFDesigner_XML](WPFDesigner_XML/) | WPF-based visual designer for editing .vstemplate XML files                                |
|                          [WPF_Toolwindow*](WPF_Toolwindow/) | Provides a sample toolwindow that can host a WPF or WinForms control                       |
|         [Windows_Forms_Controls_Installer](Windows_Forms_Controls_Installer/) | Loads custom Windows Forms controls inside the toolbox                                     |
|                                [ErrorList](ErrorList/) | generates errors in error list when the user spelling errors in the editor                       |

<!-- |          IronPython_Integrated_Shell |Demonstrates how to create an integrated shell for IronPython  
    |   IronPython_Integration | Demonstrates a custom project type and WinForms designer for IronPython
|   IronPython_Studio_VS_Shell_Isolated | Example of using the isolated shell to host a language service -->

## Other useful resources

* For details on how to create user experiences that are seamless and consistent within the IDE, visit
 the [Visual Studio User Experience Guidelines](http://aka.ms/o111mv). You will also find information
 about the common user models and interaction patterns that are used and how you can utilize them as well.

* You can publish your completed extension to the 
[Visual Studio Gallery](http://aka.ms/g4brw3).  

* Writing an extension for Visual Studio Team Services? Check out the 
[VSO extension samples](http://aka.ms/ph0rr5) at 
the dedicated repository.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

Thank you for your support for Visual Studio - we're excited to see what extensions
*you* build!
