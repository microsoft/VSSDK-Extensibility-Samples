/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using MsVsShell = Microsoft.VisualStudio.Shell;
using VsConstants = Microsoft.VisualStudio.VSConstants;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    /// <summary>
    /// Interaction logic for PersistedWindowWPFControl.xaml
    /// </summary>
    public partial class PersistedWindowWPFControl : UserControl
    {
        
        // List of tool windows
		private WindowList toolWindowsList = null;
		// Cached Selection Tracking service used to expose properties
		private ITrackSelection trackSelection = null;
		// Object holding the current selection properties
		private MsVsShell.SelectionContainer selectionContainer = new MsVsShell.SelectionContainer();
		// This allows us to prevent infinite recursion when we are changing the selection items ourselves
		private bool ignoreSelectedObjectsChanges = false;

        /// <summary>
		/// This constructor is the default for a user control
		/// </summary>
        public PersistedWindowWPFControl()
        {
            InitializeComponent();
            // Create an instance of our window list object
			toolWindowsList = new WindowList();
        }

		/// <summary>
		/// Track selection service for the tool window.
		/// This should be set by the tool window pane as soon as the tool
		/// window is created.
		/// </summary>
		internal ITrackSelection TrackSelection
		{
			get { return (ITrackSelection)trackSelection; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("TrackSelection");
				trackSelection = value;
				// Inititalize with an empty selection
				// Failure to do this would result in our later calls to 
				// OnSelectChange to be ignored (unless focus is lost
				// and regained).
				selectionContainer.SelectableObjects = null;
				selectionContainer.SelectedObjects = null;
				trackSelection.OnSelectChange(selectionContainer);
				selectionContainer.SelectedObjectsChanged += new EventHandler(selectionContainer_SelectedObjectsChanged);
			}
		}

		/// <summary>
		/// Repopulate the listview with the latest data.
		/// </summary>
		internal void RefreshData()
		{
			// Update the list
			toolWindowsList.RefreshList();
			// Update the listview
			PopulateListView();
		}

		/// <summary>
		/// Repopulate the listview with the data provided.
		/// </summary>
		private void PopulateListView()
		{

			// Empty the list
			listView1.Items.Clear();

			// Fill in the data
			foreach (string windowName in toolWindowsList.WindowNames)
		    {
				listView1.Items.Add(windowName);
			}

			// Unselect every thing
			listView1.SelectedItems.Clear();
			// Keep the property grid in sync
			listView1_SelectionChanged(this, null);
		}

		/// <summary>
		/// Handle change to the current selection is done throught the properties window
		/// drop down list.
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">Arguments</param>
		private void selectionContainer_SelectedObjectsChanged(object sender, EventArgs e)
		{
			// Set the flag letting us know we are changing the selection ourself
			ignoreSelectedObjectsChanges = true;
			try
			{
				// First clear the current selection
				this.listView1.SelectedItems.Clear();
				// See if we have something selected
				if (selectionContainer.SelectedObjects.Count > 0)
				{
					// We only support single selection, so pick the first one
					IEnumerator enumerator = selectionContainer.SelectedObjects.GetEnumerator();
					if (enumerator.MoveNext())
					{
						SelectionProperties newSelection = (SelectionProperties)enumerator.Current;
						int index = newSelection.Index;
						                        
                        // Select the corresponding item
                        this.listView1.SelectedItem = this.listView1.Items[index];						
					}
				}
			}
			finally
			{
				// make sure we react to future events
				ignoreSelectedObjectsChanges = false;
			}
		}


        /// <summary>
        /// Push properties for the selected item to the properties window.
        /// Note that throwing from a Windows Forms event handler would cause
        /// Visual Studio to crash. So if you expect your code to throw
        /// you should make sure to catch the exceptions you expect
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Arguments</param>
        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If the change originates from us setting the selection, ignore the event
            if (ignoreSelectedObjectsChanges)
                return;
            // Create the array that will hold the properties (one set of properties per item selected)
            ArrayList selectedObjects = new ArrayList();

            if (listView1.SelectedItems.Count > 0)
            {
                // Get the index of the selected item
                int index = listView1.Items.IndexOf(listView1.SelectedItems[0]);
                // Get the IVsWindowFrame for that item
                IVsWindowFrame frame = toolWindowsList[index];
                // Add the properties for the selected item
                SelectionProperties properties = toolWindowsList.GetFrameProperties(frame);
                // Keeping track of the index helps us know which tool window was selected
                // when the change is done through the property window drop-down.
                properties.Index = index;
                // This sample only supports single selection, but if multiple
                // selection is supported, multiple items could be added. The
                // properties that they had in common would then be shown.
                selectedObjects.Add(properties);
            }

            // Update our selection container
            selectionContainer.SelectedObjects = selectedObjects;
            // In order to enable the drop-down of the properties window to display
            // all our possible items, we need to provide the list
            selectionContainer.SelectableObjects = toolWindowsList.WindowsProperties;
            // Inform Visual Studio that we changed the selection and push the new list of properties
            TrackSelection.OnSelectChange(selectionContainer);
        }
	}

}
