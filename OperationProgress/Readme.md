# Operation Progress Extensibility Sample

Simulates IntelliSense operations in progress and displays the status

* Technologies: Visual Studio 2019.1 SDK
* Topics: Operation Progress


## Description

This Visual Studio Package demonstrates how to
 * Simulate IntelliSense work in progress by submitting test work to the "IntelliSense" Operation Progress Stage
 * Get the status of an Operation Progress Stage
 * Receive notifications about Operation Progress Stage changes
 * Simulates awaiting on Operation Stage completion


## Requirements

[ Visual Studio 2019 Update 1 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)


## Get all samples

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`


## Run the sample

_Note: "Operation Progress" is a new feature in Visual Studio 2019 Update 1. This sample will not build under or work with earlier versions of Visual Studio._

### Opening **Operation Progress Tool Window**
 1. To run the sample, hit F5 or choose the **Debug &gt; Start Debugging** menu command. A new instance of Visual Studio will launch under the experimental hive.
 2. In the **Start window** dialog click the **Continue without code ->** link in the bottom right corner of the dialog (alternatively you can also open an existing project or create a new one).
 3. Select **View &gt; Other Windows &gt; Operation Progress Tool Window** - this opens a new tool window named **Operation Progress Tool Window**. Optionally you can dock the window in a convenient location (e.g. bottom right corner under the Solution Explorer)
### Simulating operations in progress
 1. Open the **Operation Progress Tool Window** using the instructions above
 2. To simulate operation in progress check the **IntelliSense Stage Work** checkbox. The **IntelliSense Stage Status** field below will switch from **Complete** to **In Progress**
 3. In the bottom left corner of the main window, click on **Background tasks (Ctrl+E, Ctrl+T)** button to open the **Task Status Center**. It shows **Loading IntelliSense** in progress, with 1 pending item **Loading 1/1: Test**
 4. _Optional: to simulate waiting for operation in progress, click on the button **Wait for IntelliSense Stage** in the **Operation Progress Tool Window**. The button will change its text to **Waiting...** and become disabled._
 5. To simulate the completion of the operation in progress started at step 2, uncheck the **IntelliSense Stage Work** checkbox. The **Task Status Center** will update accordingly. If you clicked the button at step 4, it will be reverted to the initial state.

## Operation Progress Tool Window Usage
 * **IntelliSense Stage Work** CheckBox - Check to simulate starting operation in progress, uncheck to simulate completion
 * **Intellisense Stage Status (data version)** - Displays the state of the IntelliSense Operation Progress stage (Complete or In Progress) followed by the data version number provided by the API.
 * **Wait For IntelliSense Stage** Button - While IntelliSense stage is in progress, click to simulate awaiting for completion. The button will be disabled and its text will be changed to **Waiting...**. When the operation in progress completes, the button will return to the original state. Note that this behavior is not noticeable if there is no operation in progress (as there is nothing to await on, so it reverts instantly to the original state).

## Source Code Overview

The sample is based on a new **VSIX Project** to which it was added a new item **Custom Async Tool Window**.
Notable changes are in the following files:

 * **OperationProgressPackage.cs** - Retrieves IVsOperationProgress and IVsOperationProgressStatusService.
 * **OperationProgressToolWindowControl.xaml** - Defines the UI elements used by the tool window
 * **OperationProgressToolWindowControl.xaml.cs** - Implements the main logic of the sample
   * **OperationProgressToolWindowControl()** - initializes IVsOperationProgressStageStatus and subscribes to events from Operation Progress
   * **IntelliSenseCheckBox_Checked** - Registers work in progress with Operation Progress service by creating a JoinableTask object.
   * **IntelliSenseCheckBox_Unchecked** - Completes the work registered in IntelliSenseCheckBox_Checked
   * **IntelliSenseStatus_InProgressChanged** - Receives notifications when the status of the IntelliSense stage has changed and updates the UI. Note that events are received on background threads, so it needs to switch to the UI thread in order to update the UI.
   * **UpdateintelliSenseStatusTextBlock** - Helper method that displays the status in the corresponding TextBlock.
   * **WaitForIntelliSenseStage_Click** - Simulates awaiting for completion of operation in progress by disabling the button and updating its text. When the operation completes, it switches to the UI thread and reverts the button to the initial state.

**Related topics**

* [ Visual Studio SDK Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/visual-studio-sdk)
