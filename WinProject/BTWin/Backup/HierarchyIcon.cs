// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.HierarchyIcon
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using System;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  [InitializeOnLoad]
  public class HierarchyIcon : ScriptableObject
  {
    private static Texture2D icon = AssetDatabase.LoadAssetAtPath("Assets/Gizmos/Behavior Designer Hier Icon.png", typeof (Texture2D)) as Texture2D;

    public HierarchyIcon()
    {
      base.\u002Ector();
    }

    static HierarchyIcon()
    {
      if (!Object.op_Inequality((Object) HierarchyIcon.icon, (Object) null))
        return;
      // ISSUE: method pointer
      EditorApplication.hierarchyWindowItemOnGUI = (__Null) Delegate.Combine((Delegate) EditorApplication.hierarchyWindowItemOnGUI, (Delegate) new EditorApplication.HierarchyWindowItemCallback((object) null, __methodptr(HierarchyWindowItemOnGUI)));
    }

    private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
      if (!BehaviorDesignerPreferences.GetBool(BDPreferences.ShowHierarchyIcon))
        return;
      GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
      if (!Object.op_Inequality((Object) gameObject, (Object) null) || !Object.op_Inequality((Object) gameObject.GetComponent<Behavior>(), (Object) null))
        return;
      Rect rect;
      ((Rect) ref rect).\u002Ector(selectionRect);
      ((Rect) ref rect).set_x(((Rect) ref rect).get_width() + (((Rect) ref selectionRect).get_x() - 16f));
      ((Rect) ref rect).set_width(16f);
      ((Rect) ref rect).set_height(16f);
      GUI.DrawTexture(rect, (Texture) HierarchyIcon.icon);
    }
  }
}
