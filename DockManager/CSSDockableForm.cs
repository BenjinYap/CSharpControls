using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpControls.DockManager {
	public class CSSDockableForm:Form {
		/// <summary>
		/// Registered name in the DockManager.
		/// </summary>
		internal new string Name = String.Empty;

		/// <summary>
		/// Whether or not the Form is currently docked.
		/// </summary>
		internal bool Docked = false;

		/// <summary>
		/// The TabPage this Form belongs to if docked.
		/// </summary>
		internal TabPage TabPage = null;

		/// <summary>
		/// 
		/// </summary>
		internal TabControl TabControl = null;

		/// <summary>
		/// The TabIndex this Form has in its TabControl if docked.
		/// </summary>
		internal new int TabIndex = -1;
		
		internal event EventHandler DragMoved;
		internal event EventHandler DragStopped;

		private Size oldSize;

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
