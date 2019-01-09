// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.GizmoManager
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BehaviorDesigner.Editor
{
  [InitializeOnLoad]
  public class GizmoManager
  {
    private static string currentScene;

    static GizmoManager()
    {
      Scene activeScene = SceneManager.GetActiveScene();
      GizmoManager.currentScene = ((Scene) ref activeScene).get_name();
      // ISSUE: method pointer
      EditorApplication.hierarchyWindowChanged = (__Null) Delegate.Combine((Delegate) EditorApplication.hierarchyWindowChanged, (Delegate) new EditorApplication.CallbackFunction((object) null, __methodptr(HierarchyChange)));
      if (Application.get_isPlaying())
        return;
      GizmoManager.UpdateAllGizmos();
      // ISSUE: method pointer
      EditorApplication.playmodeStateChanged = (__Null) Delegate.Combine((Delegate) EditorApplication.playmodeStateChanged, (Delegate) new EditorApplication.CallbackFunction((object) null, __methodptr(UpdateAllGizmos)));
    }

    public static void UpdateAllGizmos()
    {
      foreach (Behavior behavior in (Behavior[]) Object.FindObjectsOfType<Behavior>())
        GizmoManager.UpdateGizmo(behavior);
    }

    public static void UpdateGizmo(Behavior behavior)
    {
      behavior.gizmoViewMode = (__Null) BehaviorDesignerPreferences.GetInt(BDPreferences.GizmosViewMode);
      behavior.showBehaviorDesignerGizmo = (__Null) (BehaviorDesignerPreferences.GetBool(BDPreferences.ShowSceneIcon) ? 1 : 0);
    }

    public static void HierarchyChange()
    {
      BehaviorManager instance = (BehaviorManager) BehaviorManager.instance;
      if (Application.get_isPlaying())
      {
        if (!Object.op_Inequality((Object) instance, (Object) null))
          return;
        // ISSUE: method pointer
        instance.onEnableBehavior = (__Null) new BehaviorManager.BehaviorManagerHandler((object) null, __methodptr(UpdateBehaviorManagerGizmos));
      }
      else
      {
        Scene activeScene = SceneManager.GetActiveScene();
        string name = ((Scene) ref activeScene).get_name();
        if (!(GizmoManager.currentScene != name))
          return;
        GizmoManager.currentScene = name;
        GizmoManager.UpdateAllGizmos();
      }
    }

    private static void UpdateBehaviorManagerGizmos()
    {
      BehaviorManager instance = (BehaviorManager) BehaviorManager.instance;
      if (!Object.op_Inequality((Object) instance, (Object) null))
        return;
      for (int index = 0; index < instance.get_BehaviorTrees().Count; ++index)
        GizmoManager.UpdateGizmo((Behavior) instance.get_BehaviorTrees()[index].behavior);
    }
  }
}
