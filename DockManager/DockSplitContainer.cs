
using System.Windows.Forms;

namespace CSharpControls.DockManager {
	public class DockSplitContainer:SplitContainer {
		public DockSplitContainer DockSplitContainer1 {get; internal set;}
		public DockSplitContainer DockSplitContainer2 {get; internal set;}

		public TabControl TabControl1 {get; internal set;}
		public TabControl TabControl2 {get; internal set;}
	}
}
