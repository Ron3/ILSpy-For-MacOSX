// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.EditorZoomArea
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using UnityEngine;

namespace BehaviorDesigner.Editor
{
  public class EditorZoomArea
  {
    private static Rect groupRect = (Rect) null;
    private static Matrix4x4 _prevGuiMatrix;

    public static Rect Begin(Rect screenCoordsArea, float zoomScale)
    {
      GUI.EndGroup();
      Rect rect = screenCoordsArea.ScaleSizeBy(1f / zoomScale, screenCoordsArea.TopLeft());
      ref Rect local = ref rect;
      ((Rect) ref local).set_y(((Rect) ref local).get_y() + 21f);
      GUI.BeginGroup(rect);
      EditorZoomArea._prevGuiMatrix = GUI.get_matrix();
      Matrix4x4 matrix4x4_1 = Matrix4x4.TRS(Vector2.op_Implicit(rect.TopLeft()), Quaternion.get_identity(), Vector3.get_one());
      Vector3 one = Vector3.get_one();
      one.x = (__Null) (double) (one.y = (__Null) zoomScale);
      Matrix4x4 matrix4x4_2 = Matrix4x4.Scale(one);
      GUI.set_matrix(Matrix4x4.op_Multiply(Matrix4x4.op_Multiply(Matrix4x4.op_Multiply(matrix4x4_1, matrix4x4_2), ((Matrix4x4) ref matrix4x4_1).get_inverse()), GUI.get_matrix()));
      return rect;
    }

    public static void End()
    {
      GUI.set_matrix(EditorZoomArea._prevGuiMatrix);
      GUI.EndGroup();
      ((Rect) ref EditorZoomArea.groupRect).set_y(21f);
      ((Rect) ref EditorZoomArea.groupRect).set_width((float) Screen.get_width());
      ((Rect) ref EditorZoomArea.groupRect).set_height((float) Screen.get_height());
      GUI.BeginGroup(EditorZoomArea.groupRect);
    }
  }
}
