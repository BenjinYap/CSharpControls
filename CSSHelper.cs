using System.Windows.Forms;

namespace CSharpControls {
	public static class CSSHelper {
		public static bool CursorOverControl (this Control control) {
			return control.ClientRectangle.Contains (control.PointToClient (Cursor.Position));
		}
	}
}
