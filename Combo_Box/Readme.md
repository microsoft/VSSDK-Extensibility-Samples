# Combo Box Sample
Add a toolbar to Visual Studio that has four different kinds of
dropdown combo boxes.

* Technologies: Visual Studio 2017 SDK
* Topics: Visual Studio Editor, VSX


**Description**

This sample demonstrates how to place combo boxes in a Visual Studio toolbar,
and how to integrate a toolbar into the main menu bar. The sample implements
four different kinds of combo boxes; **DropDownCombo**, **IndexCombo**,
**MRUCombo** and **DynamicCombo**.

  * Add a Drop Down Combo to a Visual Studio toolbar
  * Add an Index Combo to a Visual Studio toolbar
  * Add a MRU Combo to Visual Studio toolbar
  * Add a Dynamic Combo to Visual Studio toolbar
  * Control the programmatic name of the combo box commands by placing them within the **Tools** submenu of the main menu bar

![image](C%23/Example.ComboBox.png)

There are four styles of Combo Boxes:

  1. **DropDownCombo**: A DropDownCombo only lets the user select from the predefined list of options. The user cannot type into the combo box. The string value of the element selected is returned. For an example, see **Solution Configurations** on the **Standard** toolbar.
  2. **IndexCombo**: An IndexCombo is similar to a DropDownCombo, the user can only pick from a predefined set of options. The difference is that an IndexCombo returns the selected value as an index into the list (0 based).
  3. **MRUCombo**: An MRUCombo allows the user to type into the edit box. The history of strings entered is automatically persisted by the IDE on a per-user/per-machine basis. An example of this combo type is the **Find** dropdown. (Ctrl + F)
  4. **DynamicCombo**: A DynamicCombo allows the user to type into the edit box or pick from the list. The list of choices is usually fixed and is managed by the command handler for the command. For an example, see the **Zoom** combo on the **Class Designer** toolbar.



**Requirements**

[ Visual Studio 2017 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)



**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample**

  1. To run the sample, hit F5 or choose the**Debug &gt; Start Debugging** menu command. A new instance of Visual Studio will launch under the experimental hive.
  2. Once loaded, display the Combo Box Sample toolbar:**View &gt; Toolbars &gt; Combo Box Sample**



**Source Code Overview**

The main focus of this sample is the VSCT file containing the definition of
these combo boxes and the command handling logic for managing these combo
boxes. The C# code is minimal. The event handler functions that are called
when the user selects or enters items into a combo box simply display a
message box.

In general, you need to handle 2 commands when managing a combo box. The main
command for the combo box returns the current combo value and accepts new
input from the user. The second command retrieves the list of items to be
displayed in the combo drop down list. This second command is referred to as a
**GetList** command.

**IOleCommandTarget::Exec** is called on the second command with a non-null out parameter, which returns the list of items as an array of strings. Think of this as an extended**IOleCommandTarget::QueryStatus** call. This is because**IOleCommandTarget::QueryStatus** is only able to return a single out parameter. The combo box needs two pieces of information: the current value and the list of all choices to fill the list.

**Inside the .vsct file:**

The **Command Definition** section defines a new toolbar, a new toolbar group,
and an example of each type of combo.

**NOTE**: We deliberatly define our toolbar group with a main menu location as its parent (in this case Tools menu -- "guidSHLMainMenu:IDMVSMENU_TOOLS"). This ensures that our commands have a Programatic name that begins with _Tools_. Our commands will be organized into the _Tools_ category of the** Add Command dialog accessible from **Tools &gt; Customize &gt; Commands**. Our combo box commands are defined with the **CommandWellOnly** flag, which will make our combo box commands not actually instantiated in the main menu UI. If the user customizes our commands onto the main menu, then they will be visible.

The **Command Placement** section, is used to actually place the toolbar group
with our combo boxes on our Toolbar.

Combo boxes are defined in the **Combos** section. The following strings can
be supplied with a command:

  1. **Button Text** (required): displayed as label of the command on a toolbar if the **IconAndText** flag is specified. If any of the following optional strings are missing then _Button Text_ is used instead.
  2. **Menu Text** (optional): displayed as label of the command on a menu if **IconAndText** flag is specified.
  3. **Tooltip Text** (optional): displayed when the mouse hovers over a command.
  4. **Command Well Name** (optional): displayed as name of command in command well.
  5. **Canonical Name** (optional): English programmatic name of command used in **Command Window** and ** b&gt;. In localized command/menu (CTO) resources, always provide the English canonical name so macros can be language independent.
  6. **Localized Canonical Name** (optional): localized programmatic name of command used in **Command Window**, **DTE.ExecuteCommand**, and **Tools &gt; Options &gt; Environment &gt; Keyboard** page.



**Programmatically execute Combo Box commands with the Command Window**

Try typing the following in the Command Window: (**View &gt; Other Windows &gt;
Command Window)**

  * &gt;Tools.DropDownCombo Apples
  * &gt;Tools.IndexCombo Tigers
  * &gt;Tools.IndexCombo 2
  * &gt;Tools.MRUCombo Hello
  * &gt;Tools.DynamicCombo 34
  * &gt;Tools.DynamicCombo ZoomToFit



**Related topics**

* [ Menu and Commands Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/extending-menus-and-commands)

* [ Visual Studio SDK Documentation ](https://docs.microsoft.com/en-us/visualstudio/extensibility/visual-studio-sdk)
