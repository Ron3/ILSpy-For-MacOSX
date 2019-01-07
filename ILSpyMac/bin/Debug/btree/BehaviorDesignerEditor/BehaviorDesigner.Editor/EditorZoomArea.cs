using System;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public class EditorZoomArea
	{
		private static Matrix4x4 _prevGuiMatrix;

		private static Rect groupRect = default(Rect);

		public static Rect Begin(Rect screenCoordsArea, float zoomScale)
		{
			GUI.EndGroup();
			Rect rect = screenCoordsArea.ScaleSizeBy(1f / zoomScale, screenCoordsArea.TopLeft());
			rect.set_y(rect.get_y() + 21f);
			GUI.BeginGroup(rect);
			EditorZoomArea._prevGuiMatrix = GUI.get_matrix();
			Matrix4x4 matrix4x = Matrix4x4.TRS(rect.TopLeft(), Quaternion.get_identity(), Vector3.get_one());
			Vector3 one = Vector3.get_one();
			one.y = zoomScale;
			one.x = zoomScale;
			Matrix4x4 matrix4x2 = Matrix4x4.Scale(one);
			GUI.set_matrix(matrix4x * matrix4x2 * matrix4x.get_inverse() * GUI.get_matrix());
			return rect;
		}

		public static void End()
		{
			GUI.set_matrix(EditorZoomArea._prevGuiMatrix);
			GUI.EndGroup();
			EditorZoomArea.groupRect.set_y(21f);
			EditorZoomArea.groupRect.set_width((float)Screen.get_width());
			EditorZoomArea.groupRect.set_height((float)Screen.get_height());
			GUI.BeginGroup(EditorZoomArea.groupRect);
		}
	}
}
