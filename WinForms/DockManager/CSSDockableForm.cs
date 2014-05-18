using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CSharpControls.WinForms.DockManager {
	public class CSSDockableForm:Form {
		internal new string Name = String.Empty;
		internal bool Docked = false;
		internal Panel Panel = null;
		internal DockTabControlPanel TabControlPanel = null;
		internal TabControl TabControl = null;
		internal TabPage TabPage = null;
		internal new int TabIndex = -1;
		internal bool TabVisible = true;

		internal event EventHandler DragMoved;
		internal event EventHandler DragStopped;
		internal event EventHandler TabShown;
		internal event EventHandler TabHidden;

		private Size oldSize;
		

		public new bool Visible {
			get {
				if (Docked) {
					return TabVisible;
				} else {
					return base.Visible;
				}
			}
			set {
				if (Docked) {
					if (TabVisible == false && value) {
						TabVisible = true;

						if (TabIndex > TabControl.TabPages.Count) {
							TabControl.TabPages.Insert (TabControl.TabPages.Count, TabPage);
						} else {
							TabControl.TabPages.Insert (TabIndex, TabPage);
						}
						
						TabControl.SelectedTab = TabPage;

						if (TabShown != null) {
							TabShown (this, EventArgs.Empty);
						}
					} else if (TabVisible && value == false) {
						TabVisible = false;
						TabControl.TabPages.Remove (TabPage);

						if (TabHidden != null) {
							TabHidden (this, EventArgs.Empty);
						}
					}
				} else {
					base.Visible = value;
				}
			}
		}

		public void Registered () {
			this.ResizeBegin += onResizeBegin;
			this.ResizeEnd += onResizeEnd;
		}

		public void Deregistered () {

		}

		private void onResizeBegin (object obj, EventArgs e) {
			oldSize = this.Size;
			this.LocationChanged += onLocationChanged;
		}

		private void onResizeEnd (object obj, EventArgs e) {
			this.LocationChanged -= onLocationChanged;
			this.Opacity = 1;

			if (oldSize == this.Size) {
				if (DragStopped != null) {
					DragStopped (this, EventArgs.Empty);
				}
			}
		}

		private void onLocationChanged (object obj, EventArgs e) {
			if (oldSize == this.Size) {
				this.Opacity = 0.7;

				if (DragMoved != null) {
					DragMoved (this, EventArgs.Empty);
				}
			}
		}
	}
}
