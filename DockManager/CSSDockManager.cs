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

		private Panel basePanel = new Panel ();

		private Color flapColor = Color.FromArgb (128, Color.Red);
		private TableLayoutPanel flapTable = new TableLayoutPanel ();
		private List <Panel> flaps;
		private Panel topFlap = new Panel ();
		private Panel bottomFlap = new Panel ();
		private Panel leftFlap = new Panel ();
		private Panel rightFlap = new Panel ();
		private Panel centerFlap = new Panel ();
		private Panel farTopFlap = new Panel ();
		private Panel farBottomFlap = new Panel ();
		private Panel farLeftFlap = new Panel ();
		private Panel farRightFlap = new Panel ();

		private List <Form> dockableForms = new List<Form> ();
		private Dictionary <Form, Size> formSizes = new Dictionary<Form,Size> ();
		
		private List <DockFormInfo> formInfos = new List<DockFormInfo> ();

		private List <Panel> dockablePanels = new List<Panel> ();

		private bool dragStarted = false;
		private bool dragEnded = false;

		private bool atLeastOneFormDocked = false;

		public CSSDockManager () {
			PrepareFlaps ();
			
			basePanel.Dock = DockStyle.Fill;
			this.Controls.Add (basePanel);
			dockablePanels.Add (basePanel);
		}

		

		public void RegisterDockableForm (string name, Form form) {
			dockableForms.Add (form);
			formSizes.Add (form, new Size ());
			form.ResizeBegin += onResizeBegin;
			form.ResizeEnd += onResizeEnd;

			DockFormInfo info = new DockFormInfo ();
			info.Name = name;
			info.Form = form;
			info.Visible = form.Visible;
			info.Docked = false;
			formInfos.Add (info);
		}

		public void UnregisterDockableForm (string name, Form form) {
			dockableForms.Remove (form);
			formSizes.Remove (form);
		}

		public void LoadLayout () {
			if (File.Exists (layoutFile) == false) return;

			XmlDocument doc = new XmlDocument ();
			doc.Load (layoutFile);
			XmlNode root = doc ["cssDockManager"];

			if (root.ChildNodes.Count <= 0) return;

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

				if (atLeastOneFormDocked) {
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

		public void awd () {
			Debug.WriteLine (formInfos [2].TabPage);
		}

		private void onResizeBegin (object obj, EventArgs e) {
			Form form = (Form) obj;
			formSizes [form] = form.Size;
			form.LocationChanged += onLocationChanged;
		}

		private void onResizeEnd (object obj, EventArgs e) {
			Form form = (Form) obj;
			form.LocationChanged -= onLocationChanged;

			if (dragEnded == false) {
				dragStarted = false;
				dragEnded = true;
				form.Opacity = 1;
				DragReleasedOnManager (form);
			}
		}

		private void onLocationChanged (object obj, EventArgs e) {
			Form form = (Form) obj;
			
			if (formSizes [form] == form.Size) {  //this means the form was actually moved, not resized
				if (dragStarted == false) {
					dragStarted = true;
					dragEnded = false;
					form.Opacity = 0.7;
				}
				
				if (CursorOverControl (this)) {
					DragHoveredOnManager ();
				} else {
					DragLeftManager ();
				}
			}
		}

		private void onPanelUndocking (object obj, EventArgs e) {
			Debug.Write ("A");
		}

		private void onTabUndocking (object obj, DockEventArgs e) {
			UndockForm (formInfos.Find (info => info.TabPage == e.TabPage));
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
				returnPanel = LoadLayout (panel, DockDirection.Center, node1);
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
				formInfos.ForEach (info => {
					if (info.TabPage == tab) formNames += "," + info.Name;
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

		private void DockForm (Panel panel, Form form, DockDirection direction) {
			//if (atLeastOneFormDocked == false) throw new Exception ("dock initial form first");
			if (formInfos.Find (info => info.Form == form) == null) throw new Exception ("not a dockable form");
			if (dockablePanels.Contains (panel) == false) throw new Exception ("not a valid panel");
			
			if (atLeastOneFormDocked == false) {
				atLeastOneFormDocked = true;
				basePanel.Controls.Add (CreateTabControlPanel ());
				direction = DockDirection.Center;
			}

			DockFormInfo formInfo = GetFormInfo (form);
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
					dockablePanels.Add (oldPanel);
				}

				dockablePanels.Add (newPanel);  //add the "new" panel to the dockable panels

				if (panel != dockablePanels [0]) {  //if the target panel is not the first panel
					dockablePanels.Remove (panel);  //remove the target panel from the dockable panels since it now contains a split
				}

				panel.Controls.Add (split);  //the target panel now contains the split instead of its original tab control panel
			}

			tab = new TabPage (form.Text);
				
			foreach (Control c in form.Controls) {
				tab.Controls.Add (c);
			}
				
			tabControlPanel.TabControl.TabPages.Add (tab);
			tabControlPanel.TabControl.SelectedTab = tab;
			
			formInfo.Docked = true;
			form.Hide ();
			formInfo.TabPage = tab;
			formInfo.TabControl = tabControlPanel.TabControl;
			formInfo.TabIndex =	formInfo.TabControl.SelectedIndex;

			if (AutoSaveLayout) SaveLayout ();
		}

		private void UndockForm (DockFormInfo formInfo) {
			DockTabControlPanel tabControlPanel = (DockTabControlPanel) formInfo.TabControl.Parent;
			TabControl tabControl = formInfo.TabControl;
			tabControl.TabPages.Remove (formInfo.TabPage);
			
			foreach (Control c in formInfo.TabPage.Controls) {
				formInfo.Form.Controls.Add (c);
			}
			
			formInfo.Form.Location = new Point (Cursor.Position.X - 80, Cursor.Position.Y - 15);
			formInfo.Form.Show ();
			ReleaseCapture ();
			formInfo.TabControl = null;
			formInfo.TabPage = null;
			formInfo.Docked = false;

			if (tabControl.TabPages.Count <= 0) {
				if (tabControlPanel.Parent == basePanel) {
					tabControlPanel.PanelUndocking -= onPanelUndocking;
					tabControlPanel.TabUndocking -= onTabUndocking;
					tabControlPanel.Parent.Controls.Remove (tabControlPanel);
					atLeastOneFormDocked = false;
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
					dockablePanels.Remove (oldPanel);
					dockablePanels.Remove (deadPanel);

					if (parentPanel != basePanel) dockablePanels.Add (parentPanel);
				}
			}
			
			if (AutoSaveLayout) SaveLayout ();

			SendMessage (formInfo.Form.Handle, 0xA1, 0x2, 0);
		}

		private DockTabControlPanel CreateTabControlPanel () {
			DockTabControlPanel panel = new DockTabControlPanel ();
			panel.PanelUndocking += onPanelUndocking;
			panel.TabUndocking += onTabUndocking;
			return panel;
		}

		private void DragHoveredOnManager () {
			if (atLeastOneFormDocked) {
				ShowFarFlaps ();
				Panel panel = GetHoveredPanel ();
				
				if (panel == null) {
					HidePanelFlaps ();
				} else {
					ShowPanelFlaps (panel);
				}

				flaps.ForEach (flap => {  //make flaps that are hovered over fully opaque
					if (CursorOverControl (flap)) {
						flap.BackColor = Color.FromArgb (255, flapColor);
					} else {
						flap.BackColor = flapColor;
					}
				});
			} else {
				if (CursorOverControl (this)) {
					this.BackColor = flapColor;
				} else {
					this.BackColor = SystemColors.Control;
				}
			}
		}

		private void DragLeftManager () {
			HidePanelFlaps ();
			HideFarFlaps ();
			this.BackColor = SystemColors.Control;
		}

		private void DragReleasedOnManager (Form form) {
			if (atLeastOneFormDocked) {
				HideFarFlaps ();
				HidePanelFlaps ();

				int index = flaps.FindIndex (f => CursorOverControl (f));
				
				if (index >= 5 && index <= 8) {  //released over far flaps
					DockForm (basePanel, form, (DockDirection) (index - 5));
				} else if (index > -1) {  //released over panel flaps
					DockForm (GetHoveredPanel (), form, (DockDirection) index);
				}
			} else {
				if (CursorOverControl (this)) {
					DockForm (basePanel, form, DockDirection.Center);
				}
					
				this.BackColor = SystemColors.Control;
			}
		}
		
		private void PrepareFlaps () {
			flaps = new List<Panel> {topFlap, bottomFlap, leftFlap, rightFlap, centerFlap, farTopFlap, farBottomFlap, farLeftFlap, farRightFlap};

			flaps.ForEach (flap => {
				flap.Size = new Size (flapSize, flapSize);
				flap.BackColor = flapColor;
				this.Controls.Add (flap);
			});

			HideFarFlaps ();
			HidePanelFlaps ();
		}

		private void ShowFarFlaps () {
			Point [] flapPos = {  //flap positions are at the edges of the dock manager
				new Point ((this.Width - flapSize) / 2, 0),
				new Point ((this.Width - flapSize) / 2, this.Height - flapSize),
				new Point (0, (this.Height - flapSize) / 2),
				new Point (this.Width - flapSize, (this.Height - flapSize) / 2),
			};

			for (int i = 5; i < 9; i++) {
				flaps [i].Location = flapPos [i - 5];
				flaps [i].Show ();
			}
		}

		private void ShowPanelFlaps (Panel panel) {
			Point panelPos = this.PointToClient (panel.PointToScreen (new Point (0)));
			
			Point [] flapPos = {  //flap positions are a cross shape in the center of the target panel
				new Point (panelPos.X + (panel.Width - flapSize) / 2, panelPos.Y + (panel.Height - flapSize) / 2 - flapSize - 5),
				new Point (panelPos.X + (panel.Width - flapSize) / 2, panelPos.Y + (panel.Height - flapSize) / 2 + flapSize + 5),
				new Point (panelPos.X + (panel.Width - flapSize) / 2 - flapSize - 5, panelPos.Y + (panel.Height - flapSize) / 2),
				new Point (panelPos.X + (panel.Width - flapSize) / 2 + flapSize + 5, panelPos.Y + (panel.Height - flapSize) / 2),
				new Point (panelPos.X + (panel.Width - flapSize) / 2, panelPos.Y + (panel.Height - flapSize) / 2),
			};

			for (int i = 0; i < 5; i++) {
				flaps [i].Location = flapPos [i];
				flaps [i].Show ();
			}
		}

		private void HidePanelFlaps () {
			for (int i = 0; i < 5; i++) {
				flaps [i].Hide ();
				flaps [i].BackColor = flapColor;
			}
		}

		private void HideFarFlaps () {
			for (int i = 5; i < 9; i++) {
				flaps [i].Hide ();
				flaps [i].BackColor = flapColor;
			}
		}

		private Panel GetHoveredPanel () {
			//if the first panel is the only dockable panel, check it for hover
			if (dockablePanels.Count == 1 && CursorOverControl (basePanel)) return basePanel;
			
			//otherwise check every dockable panel except the first one
			for (int i = 1; i < dockablePanels.Count; i++) {
				if (CursorOverControl (dockablePanels [i])) return dockablePanels [i];
			}
			
			return null;
		}

		private Panel GetDockablePanel (Point cursor) {
			foreach (Panel panel in dockablePanels) {
				if (panel.ClientRectangle.Contains (panel.PointToClient (cursor))) return panel;
			}

			return null;
		}

		//name is the name of form that the panel should contain
		private Panel GetDockablePanel (string name) {
			DockFormInfo formInfo = formInfos.Find (info => info.Name == name);

			if (basePanel.Controls [0] == formInfo.TabControl.Parent) return basePanel;

			for (int i = 1; i < dockablePanels.Count; i++) {
				Panel panel = dockablePanels [i];
				
				DockTabControlPanel tabControlPanel = (DockTabControlPanel) panel.Controls [0];

				if (tabControlPanel == formInfo.TabControl.Parent) return panel;
			}

			return null;
		}

		private DockFormInfo GetFormInfo (Form form) {
			return formInfos.Find (info => info.Form == form);
		}

		private Form GetForm (string name) {
			return formInfos.Find (info => info.Name == name).Form;
		}

		private bool CursorOverControl (Control control) {
			return control.ClientRectangle.Contains (control.PointToClient (Cursor.Position));
		}

		[DllImportAttribute ("user32.dll")]
		public static extern int SendMessage (IntPtr hWnd, int Msg, int wParam, int lParam);
		[DllImportAttribute("user32.dll")]
		public static extern bool ReleaseCapture ();

		private const int flapSize = 40;
		private const string layoutFile = "cssDockManagerLayout.xml";

		public enum DockDirection {Top = 0, Bottom, Left, Right, Center, FarTop, FarBottom, FarLeft, FarRight}
	}
}
