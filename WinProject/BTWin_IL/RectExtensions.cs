// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.RectExtensions
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using UnityEngine;

namespace BehaviorDesigner.Editor
{
  public static class RectExtensions
  {
    public static Vector2 TopLeft(this Rect rect)
    {
      return new Vector2(((Rect) ref rect).get_xMin(), ((Rect) ref rect).get_yMin());
    }

    public static Rect ScaleSizeBy(this Rect rect, float scale)
    {
      return rect.ScaleSizeBy(scale, ((Rect) ref rect).get_center());
    }

    public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
    {
      Rect rect1 = rect;
      ref Rect local1 = ref rect1;
      ((Rect) ref local1).set_x(((Rect) ref local1).get_x() - (float) pivotPoint.x);
      ref Rect local2 = ref rect1;
      ((Rect) ref local2).set_y(((Rect) ref local2).get_y() - (float) pivotPoint.y);
      ref Rect local3 = ref rect1;
      ((Rect) ref local3).set_xMin(((Rect) ref local3).get_xMin() * scale);
      ref Rect local4 = ref rect1;
      ((Rect) ref local4).set_xMax(((Rect) ref local4).get_xMax() * scale);
      ref Rect local5 = ref rect1;
      ((Rect) ref local5).set_yMin(((Rect) ref local5).get_yMin() * scale);
      ref Rect local6 = ref rect1;
      ((Rect) ref local6).set_yMax(((Rect) ref local6).get_yMax() * scale);
      ref Rect local7 = ref rect1;
      ((Rect) ref local7).set_x(((Rect) ref local7).get_x() + (float) pivotPoint.x);
      ref Rect local8 = ref rect1;
      ((Rect) ref local8).set_y(((Rect) ref local8).get_y() + (float) pivotPoint.y);
      return rect1;
    }
  }
}
