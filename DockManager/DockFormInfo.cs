using System.Windows.Forms;

namespace CSharpControls.DockManager {
	internal class DockFormInfo {
		public string Name = null;
		public Form Form = null;
		public bool Docked = false;
		public bool Visible = false;
		public TabPage TabPage = null;
		public TabControl TabControl = null;
		public int TabIndex = -1;
	}
}
