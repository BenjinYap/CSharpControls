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

		private Panel basePanel = new Panel ();
		private DockFlapManager flapManager;
		private List <CSSDockableForm> dockableForms = new List<CSSDockableForm> ();
		
		public CSSDockManager () {
			flapManager = new DockFlapManager (this);

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
			dockableForms.Add (form);

			flapManager.RegisterDockableForm (form);
		}

		public void UnregisterDockableForm (string name, Form form) {
			//dockableForms.Remove (form);
			//formSizes.Remove (form);
		}

		public void LoadLayout () {
			if (File.Exists (layoutFile) == false) {
				return;
			}

			XmlDocument doc = new XmlDocument ();
			doc.Load (layoutFile);
			XmlNode root = doc ["cssDockManager"];

			if (root.ChildNodes.Count <= 0) {
				return;
			}

			XmlNode node = root.ChildNodes [0];
			
			if (node.Name == "tabControl") {
				string [] formNames = node.Attributes ["forms"].InnerText.Split (',');
				
				for (int i = 0; i < formNames.Length; i++) {
					DockForm (basePanel, GetForm (formNames [i]), DockDirection.Center);
				}
			} else {
				LoadLayout (basePanel, DockDirection.Center, node);
			}
		}

		public string SaveLayout () {
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.OmitXmlDeclaration = true;
			settings.Indent = true;

			//using (XmlWriter w = XmlWriter.Create (sb, settings)) {
			using (XmlWriter w = XmlWriter.Create (layoutFile, settings)) {
				w.WriteStartElement ("cssDockManager");

				if (AtLeastOneFormDocked) {
					if (basePanel.Controls [0] is DockTabControlPanel) {
						SaveTabControlPanel (w, (DockTabControlPanel) basePanel.Controls [0]);
					} else {
						SaveSplitContainer (w, (SplitContainer) basePanel.Controls [0]);
					}
				}

				w.WriteEndElement ();
			}
			
			return "";
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
			
			if (dockableForms.Exists (f => f.TabControlPanel == form.TabControlPanel && f.TabVisible) == false) {
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
			UndockForm (dockableForms.Find (form => form.TabPage == e.TabPage));
		}

		private Panel LoadLayout (Panel panel, DockDirection direction, XmlNode node) {
			Panel returnPanel = null;
			XmlNode node1 = node.ChildNodes [0];
			XmlNode node2 = node.ChildNodes [1];
			
			if (node1.Name == "tabControl") {
				string [] formNames = node1.Attributes ["forms"].InnerText.Split (',');
				DockForm (panel, GetForm (formNames [0]), direction);
				returnPanel = GetDockablePanel (formNames [0]);
				
				for (int i = 1; i < formNames.Length; i++) {
					DockForm (returnPanel, GetForm (formNames [i]), DockDirection.Center);
				}
			} else {
				returnPanel = LoadLayout (panel, direction, node1);
			}

			direction = (node.Attributes ["orientation"].InnerText == "Vertical") ? DockDirection.Right : DockDirection.Bottom;
			
			if (node2.Name == "tabControl") {
				string [] formNames = node2.Attributes ["forms"].InnerText.Split (',');
				DockForm (returnPanel, GetForm (formNames [0]), direction);
				
				for (int i = 1; i < formNames.Length; i++) {
					DockForm (GetDockablePanel (formNames [0]), GetForm (formNames [i]), DockDirection.Center);
				}
			} else {
				LoadLayout (returnPanel, direction, node2);
			}

			return returnPanel;
		}

		private void SaveTabControlPanel (XmlWriter w, DockTabControlPanel tabControlPanel) {
			w.WriteStartElement ("tabControl");
			TabControl tabControl = tabControlPanel.TabControl;
			string formNames = "";

			foreach (TabPage tab in tabControl.TabPages) {
				dockableForms.ForEach (form => {
					if (form.TabPage == tab) {
						formNames += "," + form.Name;
					}
				});
			}

			formNames = formNames.Substring (1);
			w.WriteAttributeString ("forms", formNames);
			w.WriteEndElement ();
		}

		private void SaveSplitContainer (XmlWriter w, SplitContainer split) {
			w.WriteStartElement ("split");
			w.WriteAttributeString ("orientation", split.Orientation.ToString ());

			new List <SplitterPanel> {split.Panel1, split.Panel2}.ForEach (panel => {
				if (panel.Controls [0] is DockTabControlPanel) {
					SaveTabControlPanel (w, (DockTabControlPanel) panel.Controls [0]);
				} else {
					SaveSplitContainer (w, (SplitContainer) panel.Controls [0]);
				}
			});
			
			w.WriteEndElement ();
		}

		private void DockForm (Panel panel, CSSDockableForm form, DockDirection direction) {			
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
		
		private Panel GetDockablePanel (Point cursor) {
			foreach (Panel panel in DockablePanels) {
				if (panel.ClientRectangle.Contains (panel.PointToClient (cursor))) return panel;
			}

			return null;
		}

		//name is the name of form that the panel should contain
		private Panel GetDockablePanel (string name) {
			CSSDockableForm form = dockableForms.Find (f => f.Name == name);

			if (basePanel.Controls [0] == form.TabControl.Parent) {
				return basePanel;
			}

			for (int i = 1; i < DockablePanels.Count; i++) {
				Panel panel = DockablePanels [i];
				
				DockTabControlPanel tabControlPanel = (DockTabControlPanel) panel.Controls [0];

				if (tabControlPanel == form.TabControl.Parent) {
					return panel;
				}
			}

			return null;
		}

		private CSSDockableForm GetForm (string name) {
			return dockableForms.Find (form => form.Name == name);
		}

		[DllImportAttribute ("user32.dll")]
		public static extern int SendMessage (IntPtr hWnd, int Msg, int wParam, int lParam);
		[DllImportAttribute("user32.dll")]
		public static extern bool ReleaseCapture ();

		
		private const string layoutFile = "cssDockManagerLayout.xml";
	}

	public enum DockDirection {Top = 0, Bottom, Left, Right, Center, None}
}
