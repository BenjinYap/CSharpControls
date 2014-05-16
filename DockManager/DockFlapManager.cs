using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CSharpControls.DockManager {
	internal class DockFlapManager {
		private CSSDockManager dockManager;

		private Color flapColor = Color.FromArgb (128, Color.Red);
		private List <Panel> flaps;
		private Panel topFlap = new Panel ();
		private Panel bottomFlap = new Panel ();
		private Panel leftFlap = new Panel ();
		private Panel rightFlap = new Panel ();
		private Panel centerFlap = new Panel ();
		private Panel farTopFlap = new Panel ();
		private Panel farBottomFlap = new Panel ();
		private Panel farLeftFlap = new Panel ();
		private Panel farRightFlap = new Panel ();
		private Panel farCenterFlap = new Panel ();

		public DockFlapManager (CSSDockManager dockManager) {
			this.dockManager = dockManager;

			PrepareFlaps ();
		}

		public void RegisterDockableForm (CSSDockableForm form) {
			form.DragMoved += onFormDragMoved;
			form.DragStopped += onFormDragStopped;
		}

		public DockDirection GetDockDirection () {
			for (int i = 0; i < 5; i++) {
				if (flaps [i].CursorOverControl ()) {
					return (DockDirection) i;
				}
			}

			return DockDirection.None;
		}

		public DockDirection GetFarDockDirection () {
			for (int i = 0; i < 5; i++) {
				if (flaps [i + 5].CursorOverControl ()) {
					return (DockDirection) i;
				}
			}

			return DockDirection.None;
		}

		private void onFormDragMoved (object obj, EventArgs e) {
			if (dockManager.CursorOverControl ()) {
				if (dockManager.AtLeastOneFormDocked) {
					Panel panel = GetHoveredPanel ();

					if (panel != null) {
						ShowPanelFlaps (panel);
					}

					ShowFarFlaps ();
				} else {
					farCenterFlap.Location = new Point ((dockManager.Width - flapSize) / 2, (dockManager.Height - flapSize) / 2);
					farCenterFlap.Show ();
				}

				flaps.ForEach (flap => {  //make flaps that are hovered over fully opaque
					if (flap.CursorOverControl ()) {
						flap.BackColor = Color.FromArgb (255, flapColor);
					} else {
						flap.BackColor = flapColor;
					}
				});
			} else {
				HidePanelFlaps ();
				HideFarFlaps ();
				farCenterFlap.Hide ();
			}
		}

		private void onFormDragStopped (object obj, EventArgs e) {
			if (dockManager.CursorOverControl ()) {
				
			}

			HidePanelFlaps ();
			HideFarFlaps ();
			farCenterFlap.Hide ();
		}

		private Panel GetHoveredPanel () {
			//if the first panel is the only dockable panel, check it for hover
			if (dockManager.DockablePanels.Count == 1 && dockManager.DockablePanels [0].CursorOverControl ()) {
				return dockManager.DockablePanels [0];
			}
			
			//otherwise check every dockable panel except the first one
			for (int i = 1; i < dockManager.DockablePanels.Count; i++) {
				if (dockManager.DockablePanels [i].CursorOverControl ()) {
					return dockManager.DockablePanels [i];
				}
			}
			
			return null;
		}

		private void PrepareFlaps () {
			flaps = new List<Panel> {topFlap, bottomFlap, leftFlap, rightFlap, centerFlap, farTopFlap, farBottomFlap, farLeftFlap, farRightFlap, farCenterFlap};

			flaps.ForEach (flap => {
				flap.Size = new Size (flapSize, flapSize);
				flap.BackColor = flapColor;
				dockManager.Controls.Add (flap);
			});

			HideFarFlaps ();
			HidePanelFlaps ();
			farCenterFlap.Hide ();
		}

		private void ShowFarFlaps () {
			Point [] flapPos = {  //flap positions are at the edges of the dock manager
				new Point ((dockManager.Width - flapSize) / 2, 0),
				new Point ((dockManager.Width - flapSize) / 2, dockManager.Height - flapSize),
				new Point (0, (dockManager.Height - flapSize) / 2),
				new Point (dockManager.Width - flapSize, (dockManager.Height - flapSize) / 2),
			};

			for (int i = 5; i < 9; i++) {
				flaps [i].Location = flapPos [i - 5];
				flaps [i].Show ();
			}
		}

		private void ShowPanelFlaps (Panel panel) {
			Point panelPos = dockManager.PointToClient (panel.PointToScreen (new Point (0)));
			
			Point [] flapPos = {  //flap positions are a cross shape in the center of the target panel
				new Point (panelPos.X + (panel.Width - flapSize) / 2, panelPos.Y + (panel.Height - flapSize) / 2 - flapSize - 5),
				new Point (panelPos.X + (panel.Width - flapSize) / 2, panelPos.Y + (panel.Height - flapSize) / 2 + flapSize + 5),
				new Point (panelPos.X + (panel.Width - flapSize) / 2 - flapSize - 5, panelPos.Y + (panel.Height - flapSize) / 2),
				new Point (panelPos.X + (panel.Width - flapSize) / 2 + flapSize + 5, panelPos.Y + (panel.Height - flapSize) / 2),
				new Point (panelPos.X + (panel.Width - flapSize) / 2, panelPos.Y + (panel.Height - flapSize) / 2),
			};

			for (int i = 0; i < 5; i++) {
				flaps [i].Location = flapPos [i];
				flaps [i].Show ();
			}
		}

		private void HidePanelFlaps () {
			for (int i = 0; i < 5; i++) {
				flaps [i].Hide ();
				flaps [i].BackColor = flapColor;
			}
		}

		private void HideFarFlaps () {
			for (int i = 5; i < 9; i++) {
				flaps [i].Hide ();
				flaps [i].BackColor = flapColor;
			}
		}

		private const int flapSize = 40;
	}
}
