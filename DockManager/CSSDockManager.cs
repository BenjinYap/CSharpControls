using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;


namespace CSharpControls.DockManager {

	public class CSSDockManager:Panel {
		public bool AutoSaveLayout = true;

		internal bool AtLeastOneFormDocked = false;
		internal List <Panel> DockablePanels = new List<Panel> ();
		internal List <CSSDockableForm> DockableForms = new List<CSSDockableForm> ();

		private Panel basePanel = new Panel ();
		private DockFlapManager flapManager;
		private DockLayoutManager layoutManager;
		
		public CSSDockManager () {
			flapManager = new DockFlapManager (this);
			layoutManager = new DockLayoutManager (this);

			basePanel.Dock = DockStyle.Fill;
			this.Controls.Add (basePanel);
			DockablePanels.Add (basePanel);
		}

		public void RegisterDockableForm (string name, CSSDockableForm form) {
			form.Name = name;
			form.DragStopped += onFormDragStopped;
			form.TabShown += onFormTabShown;
			form.TabHidden += onFormTabHidden;
			form.Registered ();
			DockableForms.Add (form);

			flapManager.RegisterDockableForm (form);
		}

		public void UnregisterDockableForm (string name, Form form) {
			//dockableForms.Remove (form);
			//formSizes.Remove (form);
		}

		public void LoadLayout () {
			layoutManager.Load ();
		}

		public string SaveLayout () {
			return layoutManager.Save ();
		}
		
		internal void DockForm (Panel panel, CSSDockableForm form, DockDirection direction) {			
			if (AtLeastOneFormDocked == false) {
				AtLeastOneFormDocked = true;
				basePanel.Controls.Add (CreateTabControlPanel ());
				direction = DockDirection.Center;
			}

			DockTabControlPanel tabControlPanel = null;
			TabPage tab = null;

			if (direction == DockDirection.Center) {  //dock center, just add to the tab control
				tabControlPanel = (DockTabControlPanel) panel.Controls [0];
			} else {  //not dock center, do a lot of things
				SplitContainer split = new SplitContainer ();  //the new splitcontainer that will hold the old and new stuff
				split.Dock = DockStyle.Fill;
				SplitterPanel oldPanel = null;
				SplitterPanel newPanel = null;
				
				//set orientation and references based on direction
				if (direction == DockDirection.Top) {
					split.Orientation = Orientation.Horizontal;
					oldPanel = split.Panel2;
					newPanel = split.Panel1;
				} else if (direction == DockDirection.Bottom) {
					split.Orientation = Orientation.Horizontal;
					oldPanel = split.Panel1;
					newPanel = split.Panel2;
				} else if (direction == DockDirection.Left) {
					split.Orientation = Orientation.Vertical;
					oldPanel = split.Panel2;
					newPanel = split.Panel1;
				} else if (direction == DockDirection.Right) {
					split.Orientation = Orientation.Vertical;
					oldPanel = split.Panel1;
					newPanel = split.Panel2;
				}

				oldPanel.Controls.Add (panel.Controls [0]);  //add the panel's current controls to the "old" panel of the new split
				
				tabControlPanel = CreateTabControlPanel ();
				newPanel.Controls.Add (tabControlPanel);  //add the new form's controls to the "new" panel of the new split
				
				if (oldPanel.Controls [0] is DockTabControlPanel) {  //add the "old" panel to the dockable panels only if it has a tab control panel
					DockablePanels.Add (oldPanel);
				}

				DockablePanels.Add (newPanel);  //add the "new" panel to the dockable panels

				if (panel != DockablePanels [0]) {  //if the target panel is not the first panel
					DockablePanels.Remove (panel);  //remove the target panel from the dockable panels since it now contains a split
				}

				panel.Controls.Add (split);  //the target panel now contains the split instead of its original tab control panel
			}

			tab = new TabPage (form.Text);
				
			foreach (Control c in form.Controls) {
				tab.Controls.Add (c);
			}
				
			tabControlPanel.TabControl.TabPages.Add (tab);
			tabControlPanel.TabControl.SelectedTab = tab;
			
			form.Hide ();
			form.Docked = true;
			form.TabControlPanel = tabControlPanel;
			form.TabControl = tabControlPanel.TabControl;
			form.TabPage = tab;
			form.TabIndex = form.TabControl.SelectedIndex;

			if (AutoSaveLayout) {
				SaveLayout ();
			}
		}

		private void onFormDragStopped (object obj, EventArgs e) {
			if (this.CursorOverControl ()) {
				DragReleasedOnManager ((CSSDockableForm) obj);
			}
		}

		private void onFormTabShown (object obj, EventArgs e) {
			CSSDockableForm form = (CSSDockableForm) obj;
			Panel panel = (Panel) form.TabControlPanel.Parent;

			if (panel == basePanel) {

			} else {
				SplitContainer split = (SplitContainer) panel.Parent;

				if (panel == split.Panel1) {
					split.Panel1Collapsed = false;
				} else {
					split.Panel2Collapsed = false;
				}
			}
		}

		private void onFormTabHidden (object obj, EventArgs e) {
			CSSDockableForm form = (CSSDockableForm) obj;
			Panel panel = (Panel) form.TabControlPanel.Parent;
			
			if (DockableForms.Exists (f => f.TabControlPanel == form.TabControlPanel && f.TabVisible) == false) {
				if (panel == basePanel) {

				} else {
					SplitContainer split = (SplitContainer) panel.Parent;

					if (panel == split.Panel1) {
						split.Panel1Collapsed = true;
					} else {
						split.Panel2Collapsed = true;
					}
				}
			}
		}

		private void onPanelUndocking (object obj, EventArgs e) {
			Debug.Write ("A");
		}

		private void onTabUndocking (object obj, DockEventArgs e) {
			UndockForm (DockableForms.Find (form => form.TabPage == e.TabPage));
		}

		private void UndockForm (CSSDockableForm form) {
			DockTabControlPanel tabControlPanel = (DockTabControlPanel) form.TabControl.Parent;
			TabControl tabControl = form.TabControl;
			tabControl.TabPages.Remove (form.TabPage);
			
			foreach (Control c in form.TabPage.Controls) {
				form.Controls.Add (c);
			}
			
			form.Location = new Point (Cursor.Position.X - 80, Cursor.Position.Y - 15);
			form.Docked = false;
			form.Show ();
			ReleaseCapture ();
			form.TabControlPanel = null;
			form.TabControl = null;
			form.TabPage = null;
			
			if (tabControl.TabPages.Count <= 0) {
				if (tabControlPanel.Parent == basePanel) {
					tabControlPanel.PanelUndocking -= onPanelUndocking;
					tabControlPanel.TabUndocking -= onTabUndocking;
					tabControlPanel.Parent.Controls.Remove (tabControlPanel);
					AtLeastOneFormDocked = false;
				} else {
					SplitContainer split = (SplitContainer) tabControlPanel.Parent.Parent;
					Panel parentPanel = (Panel) split.Parent;
					Panel oldPanel = null;
					Panel deadPanel = null;

					if (tabControlPanel.Parent == split.Panel2) {
						oldPanel = split.Panel1;
						deadPanel = split.Panel2;
					} else {
						oldPanel = split.Panel2;
						deadPanel = split.Panel1;
					}
				
					tabControlPanel.PanelUndocking -= onPanelUndocking;
					tabControlPanel.TabUndocking -= onTabUndocking;
					tabControlPanel.Parent.Controls.Remove (tabControlPanel);
					parentPanel.Controls.Remove (split);
					parentPanel.Controls.Add (oldPanel.Controls [0]);
					DockablePanels.Remove (oldPanel);
					DockablePanels.Remove (deadPanel);

					if (parentPanel != basePanel) DockablePanels.Add (parentPanel);
				}
			}
			
			if (AutoSaveLayout) {
				SaveLayout ();
			}

			SendMessage (form.Handle, 0xA1, 0x2, 0);
		}

		private DockTabControlPanel CreateTabControlPanel () {
			DockTabControlPanel panel = new DockTabControlPanel ();
			panel.PanelUndocking += onPanelUndocking;
			panel.TabUndocking += onTabUndocking;
			return panel;
		}

		private void DragReleasedOnManager (CSSDockableForm form) {
			if (AtLeastOneFormDocked) {
				DockDirection dir = flapManager.GetDockDirection ();
				
				if (dir != DockDirection.None) {
					DockForm (GetHoveredPanel (), form, dir);
					return;
				}

				dir = flapManager.GetFarDockDirection ();
				
				if (dir != DockDirection.None) {
					DockForm (basePanel, form, dir);
				}

			} else {
				DockDirection dir = flapManager.GetFarDockDirection ();
				
				if (dir != DockDirection.None) {
					DockForm (basePanel, form, DockDirection.Center);
				}
			}
		}
		
		private Panel GetHoveredPanel () {
			//if the first panel is the only dockable panel, check it for hover
			if (DockablePanels.Count == 1 && basePanel.CursorOverControl ()) {
				return basePanel;
			}
			
			//otherwise check every dockable panel except the first one
			for (int i = 1; i < DockablePanels.Count; i++) {
				if (DockablePanels [i].CursorOverControl ()) {
					return DockablePanels [i];
				}
			}
			
			return null;
		}
		
		[DllImportAttribute ("user32.dll")]
		public static extern int SendMessage (IntPtr hWnd, int Msg, int wParam, int lParam);
		[DllImportAttribute("user32.dll")]
		public static extern bool ReleaseCapture ();
	}

	public enum DockDirection {Top = 0, Bottom, Left, Right, Center, None}
}
