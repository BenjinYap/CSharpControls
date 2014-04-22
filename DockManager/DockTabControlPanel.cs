using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System;

namespace CSharpControls.DockManager {
	public class DockTabControlPanel:Panel {
		internal event DockEventHandler TabUndocking;
		public CSSTabControl TabControl = new CSSTabControl ();
		
		private bool tabUndocking = false;

		public DockTabControlPanel () {
			this.BackColor = Color.Red;

			TabControl.Dock = DockStyle.Fill;
			this.Controls.Add (TabControl);
			TabControl.TabPages.Add ("one");
			TabControl.TabPages.Add ("two");

			TabControl.MouseDown += onTabControlMouseDown;
			TabControl.MouseUp += onTabControlMouseUp;
			TabControl.LostFocus += onTabControlMouseUp;
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

		public DockEventArgs (TabPage tabPage) {
			TabPage = tabPage;
		}	
	}
}
