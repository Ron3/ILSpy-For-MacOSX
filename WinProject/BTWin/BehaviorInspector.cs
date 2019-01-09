// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.BehaviorInspector
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  [CustomEditor(typeof (Behavior))]
  public class BehaviorInspector : UnityEditor.Editor
  {
    private static int selectedVariableIndex = -1;
    private bool mShowOptions;
    private bool mShowVariables;
    private static List<float> variablePosition;
    private static string selectedVariableName;
    private static int selectedVariableTypeIndex;

    public BehaviorInspector()
    {
      //base.\u002Ector();
    }

    private void OnEnable()
    {
      Behavior target = this.get_target() as Behavior;
      if (Object.op_Equality((Object) target, (Object) null))
        return;
      GizmoManager.UpdateGizmo(target);
    }

    public virtual void OnInspectorGUI()
    {
      Behavior target = this.get_target() as Behavior;
      if (Object.op_Equality((Object) target, (Object) null))
        return;
      bool externalModification = false;
      if (!BehaviorInspector.DrawInspectorGUI(target, this.get_serializedObject(), true, ref externalModification, ref this.mShowOptions, ref this.mShowVariables))
        return;
      BehaviorDesignerUtility.SetObjectDirty((Object) target);
      if (!externalModification || !Object.op_Inequality((Object) BehaviorDesignerWindow.instance, (Object) null) || target.GetBehaviorSource().get_BehaviorID() != BehaviorDesignerWindow.instance.ActiveBehaviorID)
        return;
      BehaviorDesignerWindow.instance.LoadBehavior(target.GetBehaviorSource(), false, false);
    }

    public static bool DrawInspectorGUI(
      Behavior behavior,
      SerializedObject serializedObject,
      bool fromInspector,
      ref bool externalModification,
      ref bool showOptions,
      ref bool showVariables)
    {
      EditorGUI.BeginChangeCheck();
      GUILayout.BeginHorizontal(new GUILayoutOption[0]);
      EditorGUILayout.LabelField("Behavior Name", new GUILayoutOption[1]
      {
        GUILayout.Width(120f)
      });
      behavior.GetBehaviorSource().behaviorName = (__Null) EditorGUILayout.TextField((string) behavior.GetBehaviorSource().behaviorName, new GUILayoutOption[0]);
      if (fromInspector && GUILayout.Button("Open", new GUILayoutOption[0]))
      {
        BehaviorDesignerWindow.ShowWindow();
        BehaviorDesignerWindow.instance.LoadBehavior(behavior.GetBehaviorSource(), false, true);
      }
      GUILayout.EndHorizontal();
      EditorGUILayout.LabelField("Behavior Description", new GUILayoutOption[0]);
      behavior.GetBehaviorSource().behaviorDescription = (__Null) EditorGUILayout.TextArea((string) behavior.GetBehaviorSource().behaviorDescription, BehaviorDesignerUtility.TaskInspectorCommentGUIStyle, new GUILayoutOption[1]
      {
        GUILayout.Height(48f)
      });
      serializedObject.Update();
      GUI.set_enabled(PrefabUtility.GetPrefabType((Object) behavior) != 3 || BehaviorDesignerPreferences.GetBool(BDPreferences.EditablePrefabInstances));
      SerializedProperty property = serializedObject.FindProperty("externalBehavior");
      ExternalBehavior objectReferenceValue = property.get_objectReferenceValue() as ExternalBehavior;
      EditorGUILayout.PropertyField(property, true, new GUILayoutOption[0]);
      serializedObject.ApplyModifiedProperties();
      if (!object.ReferenceEquals((object) behavior.get_ExternalBehavior(), (object) null) && !((Object) behavior.get_ExternalBehavior()).Equals((object) objectReferenceValue) || !object.ReferenceEquals((object) objectReferenceValue, (object) null) && !((Object) objectReferenceValue).Equals((object) behavior.get_ExternalBehavior()))
      {
        if (!object.ReferenceEquals((object) behavior.get_ExternalBehavior(), (object) null))
        {
          behavior.get_ExternalBehavior().get_BehaviorSource().set_Owner((IBehavior) behavior.get_ExternalBehavior());
          behavior.get_ExternalBehavior().get_BehaviorSource().CheckForSerialization(true, behavior.GetBehaviorSource());
        }
        else
        {
          behavior.GetBehaviorSource().set_EntryTask((Task) null);
          behavior.GetBehaviorSource().set_RootTask((Task) null);
          behavior.GetBehaviorSource().set_DetachedTasks((List<Task>) null);
          behavior.GetBehaviorSource().set_Variables((List<SharedVariable>) null);
          behavior.GetBehaviorSource().CheckForSerialization(true, (BehaviorSource) null);
          behavior.GetBehaviorSource().set_Variables((List<SharedVariable>) null);
          if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
            BinarySerialization.Save(behavior.GetBehaviorSource());
          else
            JSONSerialization.Save(behavior.GetBehaviorSource());
        }
        externalModification = true;
      }
      GUI.set_enabled(true);
      EditorGUILayout.PropertyField(serializedObject.FindProperty("group"), true, new GUILayoutOption[0]);
      if (fromInspector)
      {
        string str = "BehaviorDesigner.VariablesFoldout." + (object) ((Object) behavior).GetHashCode();
        if (showVariables = EditorGUILayout.Foldout(EditorPrefs.GetBool(str, true), "Variables"))
        {
          EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
          bool flag = false;
          BehaviorSource behaviorSource1 = behavior.GetBehaviorSource();
          List<SharedVariable> allVariables = behaviorSource1.GetAllVariables();
          if (allVariables != null && allVariables.Count > 0)
          {
            if (VariableInspector.DrawAllVariables(false, (IVariableSource) behaviorSource1, ref allVariables, false, ref BehaviorInspector.variablePosition, ref BehaviorInspector.selectedVariableIndex, ref BehaviorInspector.selectedVariableName, ref BehaviorInspector.selectedVariableTypeIndex, false, true))
            {
              if (!EditorApplication.get_isPlayingOrWillChangePlaymode() && Object.op_Inequality((Object) behavior.get_ExternalBehavior(), (Object) null))
              {
                BehaviorSource behaviorSource2 = behavior.get_ExternalBehavior().GetBehaviorSource();
                behaviorSource2.CheckForSerialization(true, (BehaviorSource) null);
                if (VariableInspector.SyncVariables(behaviorSource2, allVariables))
                {
                  if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
                    BinarySerialization.Save(behaviorSource2);
                  else
                    JSONSerialization.Save(behaviorSource2);
                }
              }
              flag = true;
            }
          }
          else
            EditorGUILayout.LabelField("There are no variables to display", new GUILayoutOption[0]);
          if (flag)
          {
            if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
              BinarySerialization.Save(behaviorSource1);
            else
              JSONSerialization.Save(behaviorSource1);
          }
          EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
        }
        EditorPrefs.SetBool(str, showVariables);
      }
      string str1 = "BehaviorDesigner.OptionsFoldout." + (object) ((Object) behavior).GetHashCode();
      if (!fromInspector || (showOptions = EditorGUILayout.Foldout(EditorPrefs.GetBool(str1, true), "Options")))
      {
        if (fromInspector)
          EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("startWhenEnabled"), true, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pauseWhenDisabled"), true, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("restartWhenComplete"), true, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("resetValuesOnRestart"), true, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("logTaskChanges"), true, new GUILayoutOption[0]);
        if (fromInspector)
          EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
      }
      if (fromInspector)
        EditorPrefs.SetBool(str1, showOptions);
      if (!EditorGUI.EndChangeCheck())
        return false;
      serializedObject.ApplyModifiedProperties();
      return true;
    }
  }
}
