// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.ExternalBehaviorInspector
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  [CustomEditor(typeof (ExternalBehavior))]
  public class ExternalBehaviorInspector : UnityEditor.Editor
  {
    private static int selectedVariableIndex = -1;
    private bool mShowVariables;
    private static List<float> variablePosition;
    private static string selectedVariableName;
    private static int selectedVariableTypeIndex;

    public ExternalBehaviorInspector()
    {
      //base.\u002Ector();
    }

    public virtual void OnInspectorGUI()
    {
      ExternalBehavior target = this.get_target() as ExternalBehavior;
      if (Object.op_Equality((Object) target, (Object) null))
        return;
      if (target.get_BehaviorSource().get_Owner() == null)
        target.get_BehaviorSource().set_Owner((IBehavior) target);
      if (!ExternalBehaviorInspector.DrawInspectorGUI(target.get_BehaviorSource(), true, ref this.mShowVariables))
        return;
      BehaviorDesignerUtility.SetObjectDirty((Object) target);
    }

    public void Reset()
    {
      ExternalBehavior target = this.get_target() as ExternalBehavior;
      if (Object.op_Equality((Object) target, (Object) null) || target.get_BehaviorSource().get_Owner() != null)
        return;
      target.get_BehaviorSource().set_Owner((IBehavior) target);
    }

    public static bool DrawInspectorGUI(
      BehaviorSource behaviorSource,
      bool fromInspector,
      ref bool showVariables)
    {
      EditorGUI.BeginChangeCheck();
      GUILayout.BeginHorizontal(new GUILayoutOption[0]);
      EditorGUILayout.LabelField("Behavior Name", new GUILayoutOption[1]
      {
        GUILayout.Width(120f)
      });
      behaviorSource.behaviorName = (__Null) EditorGUILayout.TextField((string) behaviorSource.behaviorName, new GUILayoutOption[0]);
      if (fromInspector && GUILayout.Button("Open", new GUILayoutOption[0]))
      {
        BehaviorDesignerWindow.ShowWindow();
        BehaviorDesignerWindow.instance.LoadBehavior(behaviorSource, false, true);
      }
      GUILayout.EndHorizontal();
      EditorGUILayout.LabelField("Behavior Description", new GUILayoutOption[0]);
      behaviorSource.behaviorDescription = (__Null) EditorGUILayout.TextArea((string) behaviorSource.behaviorDescription, new GUILayoutOption[1]
      {
        GUILayout.Height(48f)
      });
      if (fromInspector)
      {
        string str = "BehaviorDesigner.VariablesFoldout." + (object) ((object) behaviorSource).GetHashCode();
        if (showVariables = EditorGUILayout.Foldout(EditorPrefs.GetBool(str, true), "Variables"))
        {
          EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
          List<SharedVariable> allVariables = behaviorSource.GetAllVariables();
          if (allVariables != null && VariableInspector.DrawAllVariables(false, (IVariableSource) behaviorSource, ref allVariables, false, ref ExternalBehaviorInspector.variablePosition, ref ExternalBehaviorInspector.selectedVariableIndex, ref ExternalBehaviorInspector.selectedVariableName, ref ExternalBehaviorInspector.selectedVariableTypeIndex, true, false))
          {
            if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
              BinarySerialization.Save(behaviorSource);
            else
              JSONSerialization.Save(behaviorSource);
          }
          EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
        }
        EditorPrefs.SetBool(str, showVariables);
      }
      return EditorGUI.EndChangeCheck();
    }
  }
}
