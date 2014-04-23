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
		
		private List <SplitterPanel> dockPanels = new List<SplitterPanel> ();

		private bool dragStarted = false;
		private bool dragEnded = false;

		private CSSSplitContainer initialSplit = new CSSSplitContainer ();
		private bool initialDocked = false;

		public CSSDockManager () {
			PrepareFlaps ();
			
			BaseSplitContainer.Dock = DockStyle.Fill;
			BaseSplitContainer.Panel2Collapsed = true;
			this.Controls.Add (BaseSplitContainer);
			dockPanels.Add (BaseSplitContainer.Panel1);

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

		public void DockInitialForm (Form form) {
			if (initialDocked) throw new Exception ("initial form already docked");

			initialDocked = true;
			BaseSplitContainer.Panel1.Controls.Add (CreateTabControlPanel (form));
			//BaseSplitContainer.TabControl1 = (TabControl) BaseSplitContainer.Panel1.Controls [0];
		}

		public void DockForm (SplitterPanel panel, Form form, DockDirection direction) {
			if (initialDocked == false) throw new Exception ("dock initial form first");
			if (dockableForms.Contains (form) == false) throw new Exception ("not a dockable form");
			if (dockPanels.Contains (panel) == false) throw new Exception ("not a valid panel");

			if (direction == DockDirection.Center) {  //dock center, just add to the tab control
				DockTabControlPanel tabControlPanel = (DockTabControlPanel) panel.Controls [0];
				TabPage page = new TabPage (form.Text);
				
				foreach (Control c in form.Controls) {
					page.Controls.Add (c);
				}

				tabControlPanel.TabControl.TabPages.Add (page);
				tabControlPanel.TabControl.SelectedTab = page;
				form.Hide ();
			} else {  //not dock center, do a lot of things
				DockSplitContainer split = new DockSplitContainer ();  //the new splitcontainer that will hold the old and new stuff
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
				newPanel.Controls.Add (CreateTabControlPanel (form));  //add the new form's controls to the "new" panel of the new split

				if (oldPanel.Controls [0] is DockTabControlPanel) {  //add the "old" panel to the dockable panels only if it has a tab control panel
					dockPanels.Add (oldPanel);
				}

				dockPanels.Add (newPanel);  //add the "new" panel to the dockable panels

				if (split.Panel1.Controls [0] is SplitContainer) {
					//split.DockSplitContainer1 = (DockSplitContainer) split.Panel1.Controls [0];
					//split.TabControl1 = null;
				} else {
					//split.TabControl1 = (TabControl) split.Panel1.Controls [0];
					//split.DockSplitContainer1 = null;
				}
				
				if (split.Panel2.Controls [0] is SplitContainer) {
					//split.DockSplitContainer2 = (DockSplitContainer) split.Panel2.Controls [0];
					//split.TabControl2 = null;
				} else {
					//split.TabControl2 = (TabControl) split.Panel2.Controls [0];
					//split.DockSplitContainer2 = null;
				}

				if (panel != dockPanels [0]) {  //if the target panel is not the first panel
					dockPanels.Remove (panel);  //remove the target panel from the dockable panels since it now contains a split
				}

				panel.Controls.Add (split);  //the target panel now contains the split instead of its original tab control panel
				DockSplitContainer panelContainer = ((DockSplitContainer) panel.Parent);

				if (panel == panelContainer.Panel1) {
					//panelContainer.DockSplitContainer1 = split;
					//panelContainer.TabControl1 = null;
				} else {
					//panelContainer.DockSplitContainer2 = split;
					//panelContainer.TabControl2 = null;
				}
			}
		}

		public void UndockForm (Form form) {
			if (dockableForms.Contains (form) == false) throw new Exception ("not a dockable form");


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

		}

		private void onTabUndocking (object obj, DockEventArgs e) {
			
		}

		private void DragHoveredOnManager () {
			if (initialDocked) {
				ShowFarFlaps ();
				SplitterPanel panel = GetHoveredPanel ();
				
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
			if (initialDocked) {
				HideFarFlaps ();
				HidePanelFlaps ();

				int index = flaps.FindIndex (f => CursorOverControl (f));
				
				if (index >= 5 && index <= 8) {  //released over far flaps
					DockForm (dockPanels [0], form, (DockDirection) (index - 5));
				} else if (index > -1) {  //released over panel flaps
					DockForm (GetHoveredPanel (), form, (DockDirection) index);
				}
			} else {
				if (CursorOverControl (this)) {
					DockInitialForm (form);
				}
					
				this.BackColor = SystemColors.Control;
			}
		}

		private DockTabControlPanel CreateTabControlPanel (Form form) {
			form.Hide ();
			DockTabControlPanel panel = new DockTabControlPanel ();
			panel.Dock = DockStyle.Fill;
			panel.PanelUndocking += onPanelUndocking;
			panel.TabUndocking += onTabUndocking;
			TabPage page = new TabPage (form.Text);
			
			foreach (Control c in form.Controls) {
				page.Controls.Add (c);
			}

			panel.TabControl.TabPages.Add (page);
			return panel;
		}

		private TabControl CreateTabControl (Form form) {
			TabControl control = new TabControl ();
			control.Dock = DockStyle.Fill;
			
			form.Hide ();
			TabPage page = new TabPage (form.Text);
			
			foreach (Control c in form.Controls) {
				page.Controls.Add (c);
			}

			control.TabPages.Add (page);

			return control;
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

		private void ShowPanelFlaps (SplitterPanel panel) {
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

		private SplitterPanel GetHoveredPanel () {
			//if the first panel is the only dockable panel, check it for hover
			if (dockPanels.Count == 1 && CursorOverControl (dockPanels [0])) {
				return dockPanels [0];
			}

			//otherwise check every dockable panel except the first one
			for (int i = 1; i < dockPanels.Count; i++) {
				SplitterPanel p = dockPanels [i];

				if (CursorOverControl (p)) {
					return p;
				}
			}

			return null;
		}

		private bool CursorOverControl (Control control) {
			return control.ClientRectangle.Contains (control.PointToClient (Cursor.Position));
		}

		private const int flapSize = 40;

		public enum DockDirection {Top = 0, Bottom, Left, Right, Center, FarTop, FarBottom, FarLeft, FarRight}
	}
}
