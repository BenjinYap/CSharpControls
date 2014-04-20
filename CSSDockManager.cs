using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System;
using System.Diagnostics;
using System.ComponentModel;


namespace CSharpControls {
	public class CSSDockManager:Panel {
		private Color flapColor = Color.FromArgb (128, Color.Red);
		private TableLayoutPanel flapTable = new TableLayoutPanel ();
		private Panel [] flaps;
		private Panel leftFlap = new Panel ();
		private Panel rightFlap = new Panel ();
		private Panel topFlap = new Panel ();
		private Panel bottomFlap = new Panel ();
		private Panel centerFlap = new Panel ();

		private List <Form> dockableForms = new List<Form> ();
		private Dictionary <Form, Size> formSizes = new Dictionary<Form,Size> ();
		
		private Dictionary <string, SplitterPanel> splitPanels = new Dictionary<string,SplitterPanel> ();

		private bool dragStarted = false;
		private bool dragEnded = false;

		private CSSSplitContainer initialSplit = new CSSSplitContainer ();
		private bool initialDocked = false;

		public CSSDockManager () {
			flapTable.Width = 150;
			flapTable.Height = 150;
			flapTable.ColumnCount = 3;
			flapTable.RowCount = 3;
			this.Controls.Add (flapTable);
			
			for (int i = 0; i < 3; i++) {
				flapTable.RowStyles.Add (new RowStyle (SizeType.Percent, 33));
				flapTable.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 33));
			}

			flaps = new Panel [] {leftFlap, rightFlap, topFlap, bottomFlap, centerFlap};
			
			foreach (Panel flap in flaps) {
				flap.BackColor = flapColor;
			}

			flapTable.Controls.Add (topFlap, 1, 0);
			flapTable.Controls.Add (bottomFlap, 1, 2);
			flapTable.Controls.Add (leftFlap, 0, 1);
			flapTable.Controls.Add (rightFlap, 2, 1);
			flapTable.Controls.Add (centerFlap, 1, 1);

			flapTable.Hide ();

			initialSplit.Panel2Collapsed = true;
			
			this.Controls.Add (initialSplit);
			/*
			awd.Interval = 250;
			awd.Start ();
			awd.Tick += (a, b) => {
				SplitterPanel p = GetHoverSplitterPanel (initialSplit);

				if (p != null) {
					p.BackColor = Color.Green;
				}
			};
			 */
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

		/*
		public void DockForm (string parentPanelName, Form form, string newPanelName, DockDirection direction) {
			if (dockableForms.Contains (form) == false) {
				throw new Exception ("RAAAAAAR");
			}

			if (initialDocked == false) {
				initialDocked = true;
				initialSplit.Panel1Name = newPanelName;
				initialSplit.Panel1.Controls.Add (CreateTabControl (form));
				//initialSplit.InitialDock (form, newPanelName);
			} else {
				CSSSplitContainer split = GetSplitterPanelParent (initialSplit, parentPanelName);

				if (split == null) {
					throw new Exception ("NO PANEL");
				}
				
				SplitterPanel panel = (split.Panel1Name == parentPanelName) ? split.Panel1 : split.Panel2;
				CSSSplitContainer childSplit = new CSSSplitContainer ();
			}
		}
		*/
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
					flapTable.Hide ();
				} else {
					if (CursorOverControl (this)) {
						DockForm (null, form, "", DockDirection.Bottom);
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
					for (int i = 1; i < this.Controls.Count; i++) {
						Control control = this.Controls [i];
						/*SplitterPanel p = GetHoverSplitterPanel (initialSplit);

						if (p != null) {
							p.BackColor = Color.Green;
						}*/
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

		private void ShowFlapTable (Control control) {
			flapTable.Location = new Point ((control.Width - flapTable.Width) / 2, (control.Height - flapTable.Height) / 2);
			flapTable.Show ();
		}

		/*private SplitterPanel GetTabControlParentPanel (SplitContainer split, string name) {
			SplitterPanel panel = null;
			SplitterPanel panel1 = split.Panel1;
			SplitterPanel panel2 = split.Panel2;

			if (panel1.Controls.Count == 1) {
				if (panel1.Controls [0] is TabControl) {
					if (panel1.Controls [0].Name == name) {
						return panel1;
					}
				} else {
					panel = GetTabControlParentPanel ((SplitContainer) panel1.Controls [0], name);
				}
			}
			if (panel1.Controls.Count == 1 && panel1.Controls [0] is TabControl && panel1.Controls [0].Name == name) {
				return panel1;
			} else if (panel2.Controls.Count == 1 && panel2.Controls [0] is TabControl && panel2.Controls [0].Name == name) {
				return panel2;
			}

			return panel;
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

		private const string initialSectionName = "CSSDockManagerInitialSection";

		public enum DockDirection {Top = 0, Bottom, Left, Right, Center, FarTop, FarBottom, FarLeft, FarRight}
	}
}
