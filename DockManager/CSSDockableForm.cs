using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpControls.DockManager {
	public class CSSDockableForm:Form {
		internal new string Name = String.Empty;
		internal bool Docked = false;
		internal TabPage TabPage = null;
		internal TabControl TabControl = null;
		internal int TabIndex = -1;
	}
}
