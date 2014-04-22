using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System;
using System.Linq;
using System.Diagnostics;
//using System.ComponentModel;


namespace CSharpControls.DockManager {

	public class CSSDockManager:Panel {
		public DockSplitContainer BaseSplitContainer = new DockSplitContainer ();

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
		
		private Dictionary <string, SplitterPanel> splitPanels = new Dictionary<string,SplitterPanel> ();

		private Dictionary <int, SplitterPanel> dockPanels = new Dictionary<int,SplitterPanel> ();  //panels that can be docked on

		private bool dragStarted = false;
		private bool dragEnded = false;

		private CSSSplitContainer initialSplit = new CSSSplitContainer ();
		private bool initialDocked = false;

		public CSSDockManager () {
			PrepareFlaps ();
			
			BaseSplitContainer.Dock = DockStyle.Fill;
			BaseSplitContainer.Panel2Collapsed = true;
			this.Controls.Add (BaseSplitContainer);
			dockPanels [numDockPanels++] = BaseSplitContainer.Panel1;

			initialSplit.Panel2Collapsed = true;
			
			//this.Controls.Add (initialSplit);
			/*
			awd.Interval = 250;
			awd.Start ();
			awd.Tick += (a, b) => {
				SplitterPanel p = GetHoverSection ();

				if (p != null) {
					p.BackColor = Color.Green;
				}
			};*/
		}

		private Timer awd = new Timer ();

		public void RegisterDockableForm (Form form) {
			dockableForms.Add (form);
			formSizes.Add (form, new Size ());
			form.ResizeBegin += onResizeBegin;
			form.ResizeEnd += onResizeEnd;
		}

		public void UnregisterDockableForm (Form form) {
			dockableForms.Remove (form);
			formSizes.Remove (form);
		}

		public void DockInitialForm (Form form, string newSectionName) {
			splitPanels [newSectionName] = initialSplit.Panel1;
			initialDocked = true;
			initialSplit.Panel1.Controls.Add (CreateTabControl (form));
		}

		public void DockInitialForm (Form form) {
			if (initialDocked) throw new Exception ("initial form already docked");

			initialDocked = true;
			BaseSplitContainer.Panel1.Controls.Add (CreateTabControl (form));
			BaseSplitContainer.TabControl1 = (TabControl) BaseSplitContainer.Panel1.Controls [0];
		}

		public void DockForm (SplitterPanel panel, Form form, DockDirection direction) {
			if (initialDocked == false) throw new Exception ("dock initial form first");
			if (dockPanels.ContainsValue (panel) == false) throw new Exception ("not a valid panel");

			if (direction == DockDirection.Center) {
				TabControl tabControl = (TabControl) panel.Controls [0];
				TabPage page = new TabPage (form.Text);
				
				foreach (Control c in form.Controls) {
					page.Controls.Add (c);
				}

				tabControl.TabPages.Add (page);
				form.Hide ();
			} else {
				DockSplitContainer split = new DockSplitContainer ();
				split.Dock = DockStyle.Fill;
				SplitterPanel oldPanel = null;
				SplitterPanel newPanel = null;
				
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

				oldPanel.Controls.Add (panel.Controls [0]);
				newPanel.Controls.Add (CreateTabControl (form));

				if (oldPanel.Controls [0] is TabControl) {
					dockPanels [numDockPanels++] = oldPanel;
				}

				dockPanels [numDockPanels++] = newPanel;
				
				if (split.Panel1.Controls [0] is SplitContainer) {
					split.DockSplitContainer1 = (DockSplitContainer) split.Panel1.Controls [0];
					split.TabControl1 = null;
				} else {
					split.TabControl1 = (TabControl) split.Panel1.Controls [0];
					split.DockSplitContainer1 = null;
				}
				
				if (split.Panel2.Controls [0] is SplitContainer) {
					split.DockSplitContainer2 = (DockSplitContainer) split.Panel2.Controls [0];
					split.TabControl2 = null;
				} else {
					split.TabControl2 = (TabControl) split.Panel2.Controls [0];
					split.DockSplitContainer2 = null;
				}

				if (panel != dockPanels [0]) {
					dockPanels.Remove (dockPanels.Keys.ToList ().Find (key => dockPanels [key] == panel));
				}

				panel.Controls.Add (split);
				DockSplitContainer panelContainer = ((DockSplitContainer) panel.Parent);

				if (panel == panelContainer.Panel1) {
					panelContainer.DockSplitContainer1 = split;
					panelContainer.TabControl1 = null;
				} else {
					panelContainer.DockSplitContainer2 = split;
					panelContainer.TabControl2 = null;
				}
			}

			Debug.WriteLine (dockPanels.Count);
		}

		public void DockForm (string sectionName, Form form, string newSectionName, DockDirection direction) {
			if (initialDocked == false) throw new Exception ("MUST DO INITIAL DOCK");
			if (splitPanels.ContainsKey (sectionName) == false) throw new Exception ("SECTION DOESN'T EXIST");

			SplitterPanel parentPanel = null;
			SplitContainer split = new SplitContainer ();
			split.Dock = DockStyle.Fill;
			SplitterPanel oldPanel = null;
			SplitterPanel newPanel = null;
			
			if (direction == DockDirection.Top || direction == DockDirection.FarTop) {
				split.Orientation = Orientation.Horizontal;
				oldPanel = split.Panel2;
				newPanel = split.Panel1;
			} else if (direction == DockDirection.Bottom || direction == DockDirection.FarBottom) {
				split.Orientation = Orientation.Horizontal;
				oldPanel = split.Panel1;
				newPanel = split.Panel2;
			} else if (direction == DockDirection.Left || direction == DockDirection.FarLeft) {
				split.Orientation = Orientation.Vertical;
				oldPanel = split.Panel2;
				newPanel = split.Panel1;
			} else if (direction == DockDirection.Right || direction == DockDirection.FarRight) {
				split.Orientation = Orientation.Vertical;
				oldPanel = split.Panel1;
				newPanel = split.Panel2;
			}

			if (direction >= DockDirection.Top && direction <= DockDirection.Right) {
				parentPanel = splitPanels [sectionName];
				splitPanels [sectionName] = oldPanel;
			} else if (direction >= DockDirection.FarTop && direction <= DockDirection.FarRight) {
				parentPanel = (SplitterPanel) splitPanels [sectionName].Parent.Parent;
			}

			splitPanels [newSectionName] = newPanel;

			if (direction != DockDirection.Center) {
				
				oldPanel.Controls.Add (parentPanel.Controls [0]);
				newPanel.Controls.Add (CreateTabControl (form));
				parentPanel.Controls.Add (split);
			} else {

			}
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
			
			if (formSizes [form] == form.Size) {
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

		private void DragHoveredOnManager () {
			if (initialDocked) {
				ShowFarFlaps ();
				SplitterPanel panel = GetHoveredPanel ();
				
				if (panel == null) {
					HidePanelFlaps ();
					centerFlap.Location = new Point ((this.Width - flapSize) / 2, (this.Height - flapSize) / 2);
					centerFlap.Show ();
				} else {
					ShowPanelFlaps (panel);
				}

				flaps.ForEach (flap => {
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
		}

		private void DragReleasedOnManager (Form form) {
			if (initialDocked) {
				HideFarFlaps ();
				HidePanelFlaps ();

				int index = flaps.FindIndex (f => CursorOverControl (f));
				
				if (index >= 5 && index <= 8) {
					DockForm (dockPanels [0], form, (DockDirection) (index - 5));
				} else if (index > -1) {
					DockForm (GetHoveredPanel (), form, (DockDirection) index);
				}
			} else {
				if (CursorOverControl (this)) {
					DockInitialForm (form);
				}
					
				this.BackColor = SystemColors.Control;
			}
		}

		private TabControl CreateTabControl (Form form) {
			TabControl control = new TabControl ();
			control.Dock = DockStyle.Fill;
			
			if (form != null) {
				form.Hide ();
				TabPage page = new TabPage (form.Text);
			
				foreach (Control c in form.Controls) {
					page.Controls.Add (c);
				}

				control.TabPages.Add (page);
			}

			return control;
		}

		private TabControl CreateTabControl () {
			return CreateTabControl (null);
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
			Point [] flapPos = {
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

		private void ShowPanelFlaps (SplitterPanel panel) {
			Point panelPos = this.PointToClient (panel.PointToScreen (new Point (0)));
			Point [] flapPos = {
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

		/*private void ShowFlaps (SplitterPanel panel) {
			Point panelPos = this.PointToClient (panel.PointToScreen (new Point (0)));
			Point [] flapPos = {
				new Point (panelPos.X + (panel.Width - flapSize) / 2, panelPos.Y + (panel.Height - flapSize) / 2 - flapSize - 5),
				new Point (panelPos.X + (panel.Width - flapSize) / 2, panelPos.Y + (panel.Height - flapSize) / 2 + flapSize + 5),
				new Point (panelPos.X + (panel.Width - flapSize) / 2 - flapSize - 5, panelPos.Y + (panel.Height - flapSize) / 2),
				new Point (panelPos.X + (panel.Width - flapSize) / 2 + flapSize + 5, panelPos.Y + (panel.Height - flapSize) / 2),
				new Point (panelPos.X + (panel.Width - flapSize) / 2, panelPos.Y + (panel.Height - flapSize) / 2),
				new Point ((this.Width - flapSize) / 2, 0),
				new Point ((this.Width - flapSize) / 2, this.Height - flapSize),
				new Point (0, (this.Height - flapSize) / 2),
				new Point (this.Width - flapSize, (this.Height - flapSize) / 2),
			};

			for (int i = 0; i < flaps.Count; i++) {
				flaps [i].Show ();
				flaps [i].Location = flapPos [i];
			}
		}*/

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

		/*private void HideFlaps () {
			flaps.ForEach (flap => flap.Hide ());
		}*/

		private CSSSplitContainer GetSplitterPanelParent (CSSSplitContainer split, string name) {
			CSSSplitContainer result = null;
			
			if (split == null) {
				return null;
			}

			if (split.Panel1Name == name || split.Panel2Name == name) {
				return split;
			} else {
				if (split.Panel1.Controls.Count > 0) {
					result = GetSplitterPanelParent ((CSSSplitContainer) split.Panel1.Controls [0], name);
				}

				if (split == null && split.Panel2.Controls.Count > 0) {
					result = GetSplitterPanelParent ((CSSSplitContainer) split.Panel2.Controls [0], name);
				}
			}

			return result;
		}

		private SplitterPanel GetHoveredPanel () {
			/*if (dockPanels.Count == 1) {
				if (CursorOverControl (dockPanels [0])) {
					return dockPanels [0];
				}
			} else if (dockPanels.Count > 1) {
				for (int i = 1; i < dockPanels.Count; i++) {
					SplitterPanel p = dockPanels.Values.ToArray ()[i];
					if (CursorOverControl (p)) {
						return p;
					}
				}
			}*/

			for (int i = 1; i < dockPanels.Count; i++) {
				SplitterPanel p = dockPanels.Values.ToArray ()[i];
				if (CursorOverControl (p)) {
					return p;
				}
			}

			return null;
		}

		private SplitterPanel GetHoverSection () {
			SplitterPanel [] panels = splitPanels.Values.ToArray ();

			foreach (SplitterPanel p in panels) {
				if (CursorOverControl (p)) {
					return p;
				}
			}
			
			return null;
		}

		private bool CursorOverControl (Control control) {
			return control.ClientRectangle.Contains (control.PointToClient (Cursor.Position));
		}

		private static int numDockPanels = 0;
		private const int flapSize = 40;

		public enum DockDirection {Top = 0, Bottom, Left, Right, Center, FarTop, FarBottom, FarLeft, FarRight}
	}
}
