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

If you've never written a Visual Studio extension before, [we've got some great resources to get started](https://aka.ms/extendvs).

## Getting started

1. Clone the repo to download all samples ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository))

    `git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`
2. Read the readme included with each sample and try it in Visual Studio

## Other extensibility samples

* If you're building XAML controls for UWP that are deployed as NuGet packages, you can add design-time support 
  so that they automatically appear in the toolbox when they are referenced. This sample is in 
  the [NuGet repo](https://github.com/NuGet/Samples/tree/master/ExtensionSDKasNuGetPackage).
  
* We have separate repositories and documentation for writing extensions for other members of the Visual Studio family:
     - [Visual Studio for Mac](https://docs.microsoft.com/en-us/visualstudio/mac/extending-visual-studio-mac)
     - [Visual Studio Code](https://code.visualstudio.com/docs/extensions/overview)
     - [Visual Studio Team Services](http://aka.ms/ph0rr5)

## Other useful resources

* Publish your completed extension to the [Visual Studio Marketplace](https://marketplace.visualstudio.com/), 
  which provides a convenient place for developers to find and install your extension. 
  
* Join the [Visual Studio Partner Program](https://vspartner.com/) for free to get access to dedicated resources 
  to support you as an extension publisher.

* For details on how to create user experiences that are seamless and consistent within the IDE, visit
  the [Visual Studio User Experience Guidelines](http://aka.ms/o111mv). You will also find information
  about the common user models and interaction patterns that are used and how you can utilize them as well.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

Thank you for your support for Visual Studio - we're excited to see what extensions *you* build!
