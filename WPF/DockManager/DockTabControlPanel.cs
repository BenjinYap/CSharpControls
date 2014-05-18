using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System;

namespace CSharpControls.WinForms.DockManager {
	internal class DockTabControlPanel:Panel {
		public CSSTabControl TabControl = new CSSTabControl ();
		
		internal event DockEventHandler PanelUndocking;
		internal event DockEventHandler TabUndocking;

		private bool panelUndocking = false;
		private bool tabUndocking = false;

		public DockTabControlPanel () {
			this.Dock = DockStyle.Fill;
			this.MouseDown += onMouseDown;
			this.MouseUp += onMouseUp;
			this.LostFocus += onMouseUp;

			TabControl.Dock = DockStyle.Fill;
			this.Controls.Add (TabControl);

			TabControl.MouseDown += onTabControlMouseDown;
			TabControl.MouseUp += onTabControlMouseUp;
			TabControl.LostFocus += onTabControlMouseUp;
		}

		protected override void Dispose (bool disposing) {
			base.Dispose (disposing);

			this.MouseDown -= onMouseDown;
			this.MouseUp -= onMouseUp;
			this.LostFocus -= onMouseUp;

			TabControl.MouseDown -= onTabControlMouseDown;
			TabControl.MouseUp -= onTabControlMouseUp;
			TabControl.LostFocus -= onTabControlMouseUp;
		}

		private void onMouseDown (object obj, EventArgs e) {
			this.MouseMove += onMouseMove;
		}

		private void onMouseUp (object obj, EventArgs e) {
			this.MouseMove -= onMouseMove;
			panelUndocking = false;
		}

		private void onMouseMove (object obj, EventArgs e) {
			if (panelUndocking == false && this.ClientRectangle.Contains (this.PointToClient (Cursor.Position)) == false) {
				panelUndocking = true;
				
				if (PanelUndocking != null) PanelUndocking (this, new DockEventArgs ());
			}
		}

		private void onTabControlMouseDown (object obj, EventArgs e) {
			TabControl.MouseMove += onTabControlMouseMove;
		}

		private void onTabControlMouseUp (object obj, EventArgs e) {
			TabControl.MouseMove -= onTabControlMouseMove;
			tabUndocking = false;
		}

		private void onTabControlMouseMove (object obj, EventArgs e) {
			if (TabControl.TabPages.Count <= 0) return;

			Point pos = TabControl.PointToClient (Cursor.Position);

			for (int i = 0; i < TabControl.TabPages.Count; i++) {
				Rectangle rect = TabControl.GetTabRect (i);

				if (rect.Contains (pos)) return;
			}

			if (tabUndocking == false) {
				tabUndocking = true;

				if (TabUndocking != null) TabUndocking (this, new DockEventArgs (TabControl.SelectedTab));
			}
		}

		internal delegate void DockEventHandler (object obj, DockEventArgs e);
	}

	internal class DockEventArgs:EventArgs {
		public TabPage TabPage;

		public DockEventArgs () {

		}

		public DockEventArgs (TabPage tabPage) {
			TabPage = tabPage;
		}	
	}
}
