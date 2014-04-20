using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System;
using System.Linq;
using System.Diagnostics;
//using System.ComponentModel;


namespace CSharpControls {
	public class CSSDockManager:Panel {
		private Color flapColor = Color.FromArgb (128, Color.Red);
		private TableLayoutPanel flapTable = new TableLayoutPanel ();
		private Panel [] flaps = new Panel [9];
		
		private List <Form> dockableForms = new List<Form> ();
		private Dictionary <Form, Size> formSizes = new Dictionary<Form,Size> ();
		
		private Dictionary <string, SplitterPanel> splitPanels = new Dictionary<string,SplitterPanel> ();

		private bool dragStarted = false;
		private bool dragEnded = false;

		private CSSSplitContainer initialSplit = new CSSSplitContainer ();
		private bool initialDocked = false;

		public CSSDockManager () {
			PrepareFlaps ();
			

			initialSplit.Panel2Collapsed = true;
			
			this.Controls.Add (initialSplit);
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
				
				if (initialDocked) {
					HideFlapTable ();
				} else {
					if (CursorOverControl (this)) {
						DockInitialForm (form, "a");
					}

					this.BackColor = SystemColors.Control;
				}
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

				if (initialDocked) {
					SplitterPanel panel = GetHoverSection ();
					
					if (panel != null) {
						ShowFlapTable (panel);
					} else {
						HideFlapTable ();
					}
				} else {
					if (CursorOverControl (this)) {
						this.BackColor = flapColor;
					} else {
						this.BackColor = SystemColors.Control;
					}
				}
				
			}
		}

		private TabControl CreateTabControl (Form form) {
			form.Hide ();
			TabControl control = new TabControl ();
			control.Dock = DockStyle.Fill;
			
			TabPage page = new TabPage (form.Text);
			
			foreach (Control c in form.Controls) {
				page.Controls.Add (c);
			}

			control.TabPages.Add (page);
			return control;
		}

		private void ShowFlapTable (SplitterPanel panel) {
			Point pos = this.PointToClient (panel.PointToScreen (new Point (0)));
			flapTable.Location = new Point (pos.X + (panel.Width - flapTable.Width) / 2, pos.Y + (panel.Height - flapTable.Height) / 2);
			flapTable.Show ();
			
			SplitterPanel parentPanel = panel.Parent.Parent as SplitterPanel;

			if (parentPanel != null) {
				flaps [(int) DockDirection.FarTop].Show ();
				flaps [(int) DockDirection.FarBottom].Show ();
				flaps [(int) DockDirection.FarLeft].Show ();
				flaps [(int) DockDirection.FarRight].Show ();
				int size = flaps [0].Width;
				SplitContainer split = (SplitContainer) panel.Parent;
				pos = this.PointToClient (split.PointToScreen (new Point (0)));
				flaps [(int) DockDirection.FarTop].Location = new Point (pos.X + (split.Width - size) / 2, pos.Y);
				flaps [(int) DockDirection.FarBottom].Location = new Point (pos.X + (split.Width - size) / 2, pos.Y + split.Height - size);
				flaps [(int) DockDirection.FarLeft].Location = new Point (pos.X, pos.Y + (split.Height - size) / 2);
				flaps [(int) DockDirection.FarRight].Location = new Point (pos.X + split.Width - size, pos.Y + (split.Height - size) / 2);
			}
		}

		private void HideFlapTable () {
			flapTable.Hide ();
			flaps [(int) DockDirection.FarTop].Hide ();
			flaps [(int) DockDirection.FarBottom].Hide ();
			flaps [(int) DockDirection.FarLeft].Hide ();
			flaps [(int) DockDirection.FarRight].Hide ();
		}

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

		private SplitterPanel GetHoverSection () {
			SplitterPanel [] panels = splitPanels.Values.ToArray ();

			foreach (SplitterPanel p in panels) {
				if (CursorOverControl (p)) {
					return p;
				}
			}
			
			return null;
		}

		private SplitterPanel GetHoverSplitterPanel (SplitContainer split) {
			SplitterPanel panel = null;
			
			if (split.Panel1.Controls.Count > 0 && split.Panel1.Controls [0] is SplitContainer) {
				panel = GetHoverSplitterPanel ((CSSSplitContainer) split.Panel1.Controls [0]);
			} else {
				
				if (CursorOverControl (split.Panel1)) {
					return split.Panel1;
				}

				if (split.Panel2.Controls.Count > 0 && split.Panel2.Controls [0] is SplitContainer) {
					panel = GetHoverSplitterPanel ((CSSSplitContainer) split.Panel2.Controls [0]);
				} else {
					if (CursorOverControl (split.Panel2)) {
						return split.Panel2;
					}
				}
			}

			return panel;
		}

		private bool CursorOverControl (Control control) {
			return control.ClientRectangle.Contains (control.PointToClient (Cursor.Position));
		}

		private void PrepareFlaps () {
			flapTable.Width = 150;
			flapTable.Height = 150;
			flapTable.ColumnCount = 3;
			flapTable.RowCount = 3;
			flapTable.BackColor = Color.Transparent;
			this.Controls.Add (flapTable);
			
			for (int i = 0; i < 3; i++) {
				flapTable.RowStyles.Add (new RowStyle (SizeType.Percent, 33));
				flapTable.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 33));
			}
			
			for (int i = 0; i < flaps.Length; i++) {
				flaps [i] = new Panel ();
				flaps [i].BackColor = flapColor;
			}

			flapTable.Controls.Add (flaps [(int) DockDirection.Top], 1, 0);
			flapTable.Controls.Add (flaps [(int) DockDirection.Bottom], 1, 2);
			flapTable.Controls.Add (flaps [(int) DockDirection.Left], 0, 1);
			flapTable.Controls.Add (flaps [(int) DockDirection.Right], 2, 1);
			flapTable.Controls.Add (flaps [(int) DockDirection.Center], 1, 1);
			
			for (int i = (int) DockDirection.FarTop; i <= (int) DockDirection.FarRight; i++) {
				flaps [i].Size = flaps [0].Size;
				this.Controls.Add (flaps [i]);
			}

			HideFlapTable ();
		}

		private const string initialSectionName = "CSSDockManagerInitialSection";

		public enum DockDirection {Top = 0, Bottom, Left, Right, Center, FarTop, FarBottom, FarLeft, FarRight}
	}
}
