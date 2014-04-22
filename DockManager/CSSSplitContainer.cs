using System.Windows.Forms;

namespace CSharpControls.DockManager {
	public class CSSSplitContainer:SplitContainer {
		public string Panel1Name = null;
		public string Panel2Name = null;

		public CSSSplitContainer Split1 = null;
		public CSSSplitContainer Split2 = null;

		public TabControl TabControl1 = null;
		public TabControl TabControl2 = null;

		public CSSSplitContainer () {
			this.Dock = DockStyle.Fill;
		}

		public void InitialDockForm (Form form, string panelName) {
			Panel1Name = panelName;
			this.Panel1.Controls.Add (CreateTabControl (form));
		}

		public void DockForm (string parentPanelName, Form form, string newPanelName, CSSDockManager.DockDirection direction) {
			SplitterPanel panel = GetPanel (this, parentPanelName);
		}

		private TabControl CreateTabControl (Form form) {
			form.Hide ();
			TabControl control = new TabControl ();
			control.Dock = DockStyle.Fill;
			TabPage page = new TabPage (form.Text);
			
			foreach (Control c in form.Controls) {
				page.Controls.Add (c);
			}

			control.TabPages.Add (page);
			return control;
		}

		private SplitterPanel GetPanel (CSSSplitContainer split, string name) {
			SplitterPanel panel = null;
			
			if (split == null) {
				return null;
			}

			if (split.Panel1Name == name) {
				return split.Panel1;
			} else if (split.Panel2Name == name) {
				return split.Panel2;
			} else {
				if (split.Panel1.Controls.Count > 0) {
					panel = GetPanel ((CSSSplitContainer) split.Panel1.Controls [0], name);
				}

				if (panel == null && split.Panel2.Controls.Count > 0) {
					panel = GetPanel ((CSSSplitContainer) split.Panel2.Controls [0], name);
				}
			}

			return panel;
		}
	}
}
