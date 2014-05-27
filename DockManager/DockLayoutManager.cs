using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace CSharpControls.DockManager {
	internal class DockLayoutManager {
		private CSSDockManager dockManager;

		public DockLayoutManager (CSSDockManager dockManager) {
			this.dockManager = dockManager;


		}

		public void Load () {
			if (File.Exists (layoutFile) == false) {
				return;
			}

			XmlDocument doc = new XmlDocument ();
			doc.Load (layoutFile);
			XmlNode root = doc ["cssDockManager"];

			if (root.ChildNodes.Count <= 0) {
				return;
			}

			XmlNode node = root.ChildNodes [0];
			
			if (node.Name == "tabControl") {
				string [] formNames = node.Attributes ["forms"].InnerText.Split (',');
				
				for (int i = 0; i < formNames.Length; i++) {
					dockManager.DockForm (dockManager.DockablePanels [0], GetForm (formNames [i]), DockDirection.Center);
				}
			} else {
				LoadLayout (dockManager.DockablePanels [0], DockDirection.Center, node);
			}
		}

		public string Save () {
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.OmitXmlDeclaration = true;
			settings.Indent = true;

			//using (XmlWriter w = XmlWriter.Create (sb, settings)) {
			using (XmlWriter w = XmlWriter.Create (layoutFile, settings)) {
				w.WriteStartElement ("cssDockManager");

				if (dockManager.AtLeastOneFormDocked) {
					if (dockManager.DockablePanels [0].Controls [0] is DockTabControlPanel) {
						SaveTabControlPanel (w, (DockTabControlPanel) dockManager.DockablePanels [0].Controls [0]);
					} else {
						SaveSplitContainer (w, (SplitContainer) dockManager.DockablePanels [0].Controls [0]);
					}
				}

				w.WriteEndElement ();
			}
			
			return "";
		}
		
		private Panel LoadLayout (Panel panel, DockDirection direction, XmlNode node) {
			Panel returnPanel = null;
			XmlNode node1 = node.ChildNodes [0];
			XmlNode node2 = node.ChildNodes [1];
			
			if (node1.Name == "tabControl") {
				List <string> formNames = node1.Attributes ["forms"].InnerText.Split (',').ToList ();
				formNames.RemoveAll (name => GetForm (name) == null);

				if (formNames.Count > 0) {
					dockManager.DockForm (panel, GetForm (formNames [0]), direction);
					returnPanel = GetDockablePanel (formNames [0]);
				
					for (int i = 1; i < formNames.Count; i++) {
						dockManager.DockForm (returnPanel, GetForm (formNames [i]), DockDirection.Center);
					}
				}
			} else {
				returnPanel = LoadLayout (panel, direction, node1);
			}

			direction = (node.Attributes ["orientation"].InnerText == "Vertical") ? DockDirection.Right : DockDirection.Bottom;
			
			if (node2.Name == "tabControl") {
				List <string> formNames = node2.Attributes ["forms"].InnerText.Split (',').ToList ();
				formNames.RemoveAll (name => GetForm (name) == null);

				if (formNames.Count > 0) {
					dockManager.DockForm (returnPanel, GetForm (formNames [0]), direction);
				
					for (int i = 1; i < formNames.Count; i++) {
						dockManager.DockForm (GetDockablePanel (formNames [0]), GetForm (formNames [i]), DockDirection.Center);
					}

					GetSplitContainer (formNames [0]).SplitterDistance = int.Parse (node.Attributes ["d"].InnerText);
				}
			} else {
				LoadLayout (returnPanel, direction, node2);
			}

			return returnPanel;
		}

		
		private void SaveTabControlPanel (XmlWriter w, DockTabControlPanel tabControlPanel) {
			w.WriteStartElement ("tabControl");
			TabControl tabControl = tabControlPanel.TabControl;
			string formNames = "";

			foreach (TabPage tab in tabControl.TabPages) {
				dockManager.DockableForms.ForEach (form => {
					if (form.TabPage == tab) {
						formNames += "," + form.Name;
					}
				});
			}

			formNames = formNames.Substring (1);
			w.WriteAttributeString ("forms", formNames);
			w.WriteEndElement ();
		}

		private void SaveSplitContainer (XmlWriter w, SplitContainer split) {
			w.WriteStartElement ("split");
			w.WriteAttributeString ("orientation", split.Orientation.ToString ());
			w.WriteAttributeString ("d", split.SplitterDistance.ToString ());

			new List <SplitterPanel> {split.Panel1, split.Panel2}.ForEach (panel => {
				if (panel.Controls [0] is DockTabControlPanel) {
					SaveTabControlPanel (w, (DockTabControlPanel) panel.Controls [0]);
				} else {
					SaveSplitContainer (w, (SplitContainer) panel.Controls [0]);
				}
			});
			
			w.WriteEndElement ();
		}


		private CSSDockableForm GetForm (string name) {
			return dockManager.DockableForms.Find (form => form.Name == name);
		}

		//name is the name of form that the panel should contain
		private Panel GetDockablePanel (string name) {
			CSSDockableForm form = GetForm (name);

			if (dockManager.DockablePanels [0].Controls [0] == form.TabControl.Parent) {
				return dockManager.DockablePanels [0];
			}

			for (int i = 1; i < dockManager.DockablePanels.Count; i++) {
				Panel panel = dockManager.DockablePanels [i];
				
				DockTabControlPanel tabControlPanel = (DockTabControlPanel) panel.Controls [0];

				if (tabControlPanel == form.TabControl.Parent) {
					return panel;
				}
			}

			return null;
		}

		private SplitContainer GetSplitContainer (string name) {
			CSSDockableForm form = GetForm (name);

			if (form == null) {
				return null;
			}

			return (SplitContainer) form.TabControlPanel.Parent.Parent;
		}

		private const string layoutFile = "cssDockManagerLayout.xml";
	}
}
