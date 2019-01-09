// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.BehaviorDesignerPreferences
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using System;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  public class BehaviorDesignerPreferences : UnityEditor.Editor
  {
    private static string[] serializationString = new string[2]
    {
      "Binary",
      "JSON"
    };
    private static string[] prefString;

    public BehaviorDesignerPreferences() : base()
    {
      ////base.\u002Ector();
    }

    private static string[] PrefString
    {
      get
      {
        if (BehaviorDesignerPreferences.prefString == null)
          BehaviorDesignerPreferences.InitPrefString();
        return BehaviorDesignerPreferences.prefString;
      }
    }

    public static void InitPrefernces()
    {
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[0]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.ShowWelcomeScreen, true);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[1]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.ShowSceneIcon, true);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[2]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.ShowHierarchyIcon, true);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[3]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.OpenInspectorOnTaskSelection, false);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[3]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.OpenInspectorOnTaskSelection, false);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[5]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.FadeNodes, true);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[6]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.EditablePrefabInstances, false);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[7]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.PropertiesPanelOnLeft, true);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[8]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.MouseWhellScrolls, false);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[9]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.FoldoutFields, true);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[10]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.CompactMode, false);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[11]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.SnapToGrid, true);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[12]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.ShowTaskDescription, true);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[13]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.BinarySerialization, true);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[14]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.ErrorChecking, true);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[15]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.SelectOnBreakpoint, false);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[16]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.UpdateCheck, true);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[17]))
        BehaviorDesignerPreferences.SetBool(BDPreferences.AddGameGUIComponent, false);
      if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[18]))
        BehaviorDesignerPreferences.SetInt(BDPreferences.GizmosViewMode, 2);
      if (!BehaviorDesignerPreferences.GetBool(BDPreferences.EditablePrefabInstances) || !BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
        return;
      BehaviorDesignerPreferences.SetBool(BDPreferences.BinarySerialization, false);
    }

    private static void InitPrefString()
    {
      BehaviorDesignerPreferences.prefString = new string[19];
      for (int index = 0; index < BehaviorDesignerPreferences.prefString.Length; ++index)
        BehaviorDesignerPreferences.prefString[index] = string.Format("BehaviorDesigner{0}", (object) (BDPreferences) index);
    }

    public static void DrawPreferencesPane(PreferenceChangeHandler callback)
    {
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.ShowWelcomeScreen, "Show welcome screen", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.ShowSceneIcon, "Show Behavior Designer icon in the scene", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.ShowHierarchyIcon, "Show Behavior Designer icon in the hierarchy window", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.OpenInspectorOnTaskSelection, "Open inspector on single task selection", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.OpenInspectorOnTaskDoubleClick, "Open inspector on task double click", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.FadeNodes, "Fade tasks after they are done running", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.EditablePrefabInstances, "Allow edit of prefab instances", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.PropertiesPanelOnLeft, "Position properties panel on the left", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.MouseWhellScrolls, "Mouse wheel scrolls graph view", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.FoldoutFields, "Grouped fields start visible", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.CompactMode, "Compact mode", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.SnapToGrid, "Snap to grid", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.ShowTaskDescription, "Show selected task description", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.ErrorChecking, "Realtime error checking", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.SelectOnBreakpoint, "Select GameObject if a breakpoint is hit", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.UpdateCheck, "Check for updates", callback);
      BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.AddGameGUIComponent, "Add Game GUI Component", callback);
      bool flag = BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization);
      if (EditorGUILayout.Popup("Serialization", !flag ? 1 : 0, BehaviorDesignerPreferences.serializationString, new GUILayoutOption[0]) != (!flag ? 1 : 0))
      {
        BehaviorDesignerPreferences.SetBool(BDPreferences.BinarySerialization, !flag);
        callback(BDPreferences.BinarySerialization, (object) !flag);
      }
      int num1 = BehaviorDesignerPreferences.GetInt(BDPreferences.GizmosViewMode);
      int num2 = (int) (Behavior.GizmoViewMode) EditorGUILayout.EnumPopup("Gizmos View Mode", (Enum) (object) (Behavior.GizmoViewMode) num1, new GUILayoutOption[0]);
      if (num2 != num1)
      {
        BehaviorDesignerPreferences.SetInt(BDPreferences.GizmosViewMode, num2);
        callback(BDPreferences.GizmosViewMode, (object) num2);
      }
      if (!GUILayout.Button("Restore to Defaults", EditorStyles.get_miniButtonMid(), new GUILayoutOption[0]))
        return;
      BehaviorDesignerPreferences.ResetPrefs();
    }

    private static void DrawBoolPref(
      BDPreferences pref,
      string text,
      PreferenceChangeHandler callback)
    {
      bool flag1 = BehaviorDesignerPreferences.GetBool(pref);
      bool flag2 = GUILayout.Toggle(flag1, text, new GUILayoutOption[0]);
      if (flag2 == flag1)
        return;
      BehaviorDesignerPreferences.SetBool(pref, flag2);
      callback(pref, (object) flag2);
    }

    private static void ResetPrefs()
    {
      BehaviorDesignerPreferences.SetBool(BDPreferences.ShowWelcomeScreen, true);
      BehaviorDesignerPreferences.SetBool(BDPreferences.ShowSceneIcon, true);
      BehaviorDesignerPreferences.SetBool(BDPreferences.ShowHierarchyIcon, true);
      BehaviorDesignerPreferences.SetBool(BDPreferences.OpenInspectorOnTaskSelection, false);
      BehaviorDesignerPreferences.SetBool(BDPreferences.OpenInspectorOnTaskDoubleClick, false);
      BehaviorDesignerPreferences.SetBool(BDPreferences.FadeNodes, true);
      BehaviorDesignerPreferences.SetBool(BDPreferences.EditablePrefabInstances, false);
      BehaviorDesignerPreferences.SetBool(BDPreferences.PropertiesPanelOnLeft, true);
      BehaviorDesignerPreferences.SetBool(BDPreferences.MouseWhellScrolls, false);
      BehaviorDesignerPreferences.SetBool(BDPreferences.FoldoutFields, true);
      BehaviorDesignerPreferences.SetBool(BDPreferences.CompactMode, false);
      BehaviorDesignerPreferences.SetBool(BDPreferences.SnapToGrid, true);
      BehaviorDesignerPreferences.SetBool(BDPreferences.ShowTaskDescription, true);
      BehaviorDesignerPreferences.SetBool(BDPreferences.BinarySerialization, true);
      BehaviorDesignerPreferences.SetBool(BDPreferences.ErrorChecking, true);
      BehaviorDesignerPreferences.SetBool(BDPreferences.SelectOnBreakpoint, false);
      BehaviorDesignerPreferences.SetBool(BDPreferences.UpdateCheck, true);
      BehaviorDesignerPreferences.SetBool(BDPreferences.AddGameGUIComponent, false);
      BehaviorDesignerPreferences.SetInt(BDPreferences.GizmosViewMode, 2);
    }

    public static void SetBool(BDPreferences pref, bool value)
    {
      EditorPrefs.SetBool(BehaviorDesignerPreferences.PrefString[(int) pref], value);
    }

    public static bool GetBool(BDPreferences pref)
    {
      return EditorPrefs.GetBool(BehaviorDesignerPreferences.PrefString[(int) pref]);
    }

    public static void SetInt(BDPreferences pref, int value)
    {
      EditorPrefs.SetInt(BehaviorDesignerPreferences.PrefString[(int) pref], value);
    }

    public static int GetInt(BDPreferences pref)
    {
      return EditorPrefs.GetInt(BehaviorDesignerPreferences.PrefString[(int) pref]);
    }
  }
}
