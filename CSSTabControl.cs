using System.Windows.Forms;
using System;
using System.Drawing;

namespace CSharpControls {
	public class CSSTabControl:TabControl {
		public bool allowOutOfBoundsOrdering = true;

		public CSSTabControl () {
			this.MouseLeave += onMouseLeave;
			this.MouseDown += onMouseDown;
			this.MouseUp += onMouseUp;
		}

		private void onMouseLeave (object obj, EventArgs e) {
			this.MouseMove -= onMouseMove;
		}

		private void onMouseDown (object obj, EventArgs e) {
			if (this.TabPages.Count > 0) {
				this.MouseMove += onMouseMove;
			}
		}

		private void onMouseUp (object obj, EventArgs e) {
			this.MouseMove -= onMouseMove;
		}

		private void onMouseMove (object obj, EventArgs e) {
			Point pos = this.PointToClient (Cursor.Position);

			for (int i = 0; i < this.TabPages.Count; i++) {
				if (i != this.SelectedIndex) {
					Rectangle rect = this.GetTabRect (i);

					if (allowOutOfBoundsOrdering) {
						if (pos.X >= rect.Left && pos.X <= rect.Right) {
							TabPage selectedTab = this.SelectedTab;
							this.TabPages.Remove (selectedTab);
							this.TabPages.Insert (i, selectedTab);
							this.SelectedTab = selectedTab;
						}
					} else {
						if (rect.Contains (pos)) {
							TabPage selectedTab = this.SelectedTab;
							this.TabPages.Remove (selectedTab);
							this.TabPages.Insert (i, selectedTab);
							this.SelectedTab = selectedTab;
						}
					}
				}
			}
		}
	}
}
