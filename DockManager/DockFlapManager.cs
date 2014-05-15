using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace CSharpControls.DockManager {
	internal class DockFlapManager {
		private CSSDockManager dockManager;

		public DockFlapManager (CSSDockManager dockManager) {
			this.dockManager = dockManager;
		}

		public void RegisterDockableForm (CSSDockableForm form) {
			form.DragMoved += onFormDragMoved;
			form.DragStopped += onFormDragStopped;
		}

		private void onFormDragMoved (object obj, EventArgs e) {
			if (dockManager.CursorOverControl ()) {
				
			} else {
				
			}
		}

		private void onFormDragStopped (object obj, EventArgs e) {
			//if (CursorOverControl (this)) {
				
			//}
		}
	}
}
