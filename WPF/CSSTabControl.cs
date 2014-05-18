using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Controls;

namespace CSharpControls.WPF {
	public class CSSTabControl:TabControl {
		/// <summary>
		/// Gets or sets a value indicating whether the tabs can be dragged when the cursor is not on the tab bar.
		/// </summary>
		public bool AllowTabReorderingOutsideHeader = true;

		public CSSTabControl () {
			
			this.MouseLeave += onMouseLeave;
			this.MouseDown += onMouseDown;
			this.MouseUp += onMouseUp;
		}

		public void StopTabReordering () {
			this.MouseMove -= onMouseMove;
		}

		protected override void Dispose (bool disposing) {
			base.Dispose (disposing);

			this.MouseLeave -= onMouseLeave;
			this.MouseDown -= onMouseDown;
			this.MouseUp -= onMouseUp;
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
				Rectangle rect = this.GetTabRect (i);
				rect.Inflate (-1, 0);

				if (i != this.SelectedIndex) {
					if (AllowTabReorderingOutsideHeader) {
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
