using System.Windows.Forms;
using System;
using System.Drawing;
using System.Diagnostics;

namespace CSharpControls {
	public class CSSTabControl:TabControl {
		/// <summary>
		/// Gets or sets a value indicating whether the tabs can be dragged when the cursor is not on the tab bar.
		/// </summary>
		public bool AllowOutOfBoundsDragging = true;

		public CSSTabControl () {
			this.MouseLeave += onMouseLeave;
			this.MouseDown += onMouseDown;
			this.MouseUp += onMouseUp;
		}

		public void StopTabDrag () {
			this.MouseMove -= onMouseMove;
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
			bool withinBounds = false;
			
			for (int i = 0; i < this.TabPages.Count; i++) {
				Rectangle rect = this.GetTabRect (i);

				if (i != this.SelectedIndex) {
					if (AllowOutOfBoundsDragging) {
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

				if (rect.Contains (pos)) withinBounds = true;
			}

			if (withinBounds == false && TabDraggedOutOfBounds != null) TabDraggedOutOfBounds (this, EventArgs.Empty);
		}

		public event EventHandler TabDraggedOutOfBounds;
	}
}
