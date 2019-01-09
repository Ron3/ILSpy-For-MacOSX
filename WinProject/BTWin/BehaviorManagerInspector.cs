// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.BehaviorManagerInspector
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using System;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  [CustomEditor(typeof (BehaviorManager))]
  public class BehaviorManagerInspector : UnityEditor.Editor
  {
    public BehaviorManagerInspector()
    {
      //base.\u002Ector();
    }

    public virtual void OnInspectorGUI()
    {
      BehaviorManager target = this.get_target() as BehaviorManager;
      target.set_UpdateInterval((UpdateIntervalType) EditorGUILayout.EnumPopup("Update Interval", (Enum) (object) target.get_UpdateInterval(), new GUILayoutOption[0]));
      if (target.get_UpdateInterval() == 1)
      {
        EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
        target.set_UpdateIntervalSeconds(EditorGUILayout.FloatField("Seconds", target.get_UpdateIntervalSeconds(), new GUILayoutOption[0]));
        EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
      }
      target.set_ExecutionsPerTick((BehaviorManager.ExecutionsPerTickType) EditorGUILayout.EnumPopup("Task Execution Type", (Enum) (object) target.get_ExecutionsPerTick(), new GUILayoutOption[0]));
      if (target.get_ExecutionsPerTick() != 1)
        return;
      EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
      target.set_MaxTaskExecutionsPerTick(EditorGUILayout.IntField("Max Execution Count", target.get_MaxTaskExecutionsPerTick(), new GUILayoutOption[0]));
      EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
    }
  }
}
