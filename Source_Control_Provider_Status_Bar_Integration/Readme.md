
# Source Control Provider Information in the Status Bar
Provides a sample which allows Source Control Providers to display information in the Visual Studio Status Bar.


* Technologies: Visual Studio 2015 Update 2 SDK
* Topics: MSBuild, VSX

**Description**

This sample builds upon the Source Control Provider sample. Apart from the functionality mentioned in that sample, this sample demonstrates the following.

  * Display the active Branch of the repository on the Status Bar 
  * Display the active Repository name on the Status Bar 
  * Display the number of pending changes in the repository
  * Display the number of unpublished commits in the repository (Note: Only for Distributed Source Control Systems like Git) 
  * Begin Publish of a solution not under source control into a supported Source Control system 

![image](C%23/Resources/Example.StatusBarPublish.png)

![image](C%23/Resources/Example.StatusBarRepo.png)

**Requirements**

[ Visual Studio 2015 Update 2](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)

**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

** Run the sample

1. To build and execute the sample, open the .sln file, press F5 after the sample is loaded
2. Once loaded, create a new solution that is not under Source Control
3. Click the Publish button
4. Select “Managed Source Control Sample Provider (C#)”
5. This adds the solution to the sample Source Control.
6. Clicking on “Sample Branch” on the status bar will display a menu with a list of sample branches and sample actions
7. Clicking on “Sample Repository” on the status bar will display a dialog with the coordinates of the repository button.
8. Clicking on the Pending Changes button (the button with a pencil icon) will display a dialog with the coordinates of the button.
9. Clicking on the Unpublished Commits button (the button with an arrow icon) will display a dialog with the coordinates of the button.


**Details**

The provider demonstrates basic source control functionality:

  1. The sample does not implement a real source control store. To track which files in a project are controlled, the sample will create a projectname.storage text file on the hard drive that contains the list of controlled files in that project. 
  2. To simplify things, if a project is added to the solution after the solution was added to source control, that project will not be automatically controlled. The user must explicitly use the _AddToSourceControl_ command to add it to source control. (Exceptions are solution folders, which are controlled automatically after closing and reopening the solution) 
  3. Controlled solutions may be opened from disk.
  4. Controlled files can be checked out and checked in, but these actions only toggle the read-write attribute of the files. Source control status of files will be inferred from the file attributes on local disk. 



The _SccProvider_ class derives from theIVsPersistSolutionProps** interface 
to persist properties in the solution file. Uses:SolutionIsControlled**
indicates the controlled status of the solution.SolutionBindings** shows
the solution location in the source control database.

The _SccProviderService_ class implements basic QueryEdit / QuerySave
functionality. _OnEdit_ prompts the user to check out the file, edit in
memory, or cancel the edit. _OnSave_ prompts the user to check out the file,
skip the save, save as a new file, or cancel the operation. _OnSave_ code
triggers only if files have been edited in memory or if the user forces the
file to be saved when it is already checked in.

_SccProviderService_ implements theIVsTrackProjectDocumentsEvents2**
interface to intercept project-change events in the integrated development
environment (IDE). Uses:OnAfterRenameFiles** renames files in source
control whenever they are renamed in the project.OnQueryRemoveFiles** warns
the user when they are deleting a file that is under source control.

The _SccProviderService_ implements the IVsSccCurrentBranch** interface
to display information about the current branch in the active repository
in the Status Bar.

The _SccProviderService_ implements the IVsSccCurrentRepository** interface
to display information about the current repository
in the Status Bar.

The _SccProviderService_ implements the IVsSccChanges** interface
to display information about the pending changes in the active repository
in the Status Bar.

The _SccProviderService_ implements the IVsSccUnpublishedCommits** interface
to display information about the number of unpublished commits in the active repository
in the Status Bar. Note: This is only for Distributed Source Control Systems.

The _SccProviderService_ implements the IVsSccPublish** interface
to indicate that a Source Control System supports a Publish operation which enables 
users to add a solution to Source Control right from the Status Bar.

**Project Files**

* **SccProviderService-IVsSccChanges.cs**

Implementation of the Pending Changes display information

* **SccProviderService-IVsSccCurrentBranch.cs**

Implementation of the Current Branch display information

* **SccProviderService-IVsSccCurrentRepository.cs**

Implementation of the Current Repository display information

* **SccProviderService-IVsSccUnpublishedCommits.cs**

Implementation of the Unpublished Commits display information. This interface should only be implemented by Distributed Version Control Systems.

* **SccProviderService-IVsSccPublish.cs**

Implementation of the interface which enables a solution not under Source Control to be published into a remote server


**Functional Tests**

* Verify the sample builds in all configurations
* Verify that the sample was registered. The About box should list the product as installed
* Verify that the Source Control System name is available for selection on the Publish menu when the solution is not under Source Control
* Verify that after clicking the Publish menu item, the solution is added to Source Control and glyphs are seen next to files in Solution Explorer and the following buttons are displayed on the Status Bar - “Sample Repository”, “Sample Branch”, “0” with a pencil icon, “0” with a up arrow.

**Related topics**

  * [ SCC Provider Integration ](https://msdn.microsoft.com/en-us/library/bb166434(v=vs.140).aspx)
  * [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.140).aspx)



