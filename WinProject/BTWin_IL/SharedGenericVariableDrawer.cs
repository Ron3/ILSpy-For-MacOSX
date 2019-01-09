// Decompiled with JetBrains decompiler
// Type: SharedGenericVariableDrawer
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Editor;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomObjectDrawer(typeof (GenericVariable))]
public class SharedGenericVariableDrawer : ObjectDrawer
{
  private static string[] variableNames;

  public override void OnGUI(GUIContent label)
  {
    GenericVariable genericVariable = this.value as GenericVariable;
    EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
    if (FieldInspector.DrawFoldout(((object) genericVariable).GetHashCode(), label))
    {
      EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
      if (SharedGenericVariableDrawer.variableNames == null)
      {
        List<Type> sharedVariableTypes = VariableInspector.FindAllSharedVariableTypes(true);
        SharedGenericVariableDrawer.variableNames = new string[sharedVariableTypes.Count];
        for (int index = 0; index < sharedVariableTypes.Count; ++index)
          SharedGenericVariableDrawer.variableNames[index] = sharedVariableTypes[index].Name.Remove(0, 6);
      }
      int index1 = 0;
      string str = ((string) genericVariable.type).Remove(0, 6);
      for (int index2 = 0; index2 < SharedGenericVariableDrawer.variableNames.Length; ++index2)
      {
        if (SharedGenericVariableDrawer.variableNames[index2].Equals(str))
        {
          index1 = index2;
          break;
        }
      }
      int index3 = EditorGUILayout.Popup("Type", index1, SharedGenericVariableDrawer.variableNames, BehaviorDesignerUtility.SharedVariableToolbarPopup, new GUILayoutOption[0]);
      Type sharedVariableType = VariableInspector.FindAllSharedVariableTypes(true)[index3];
      if (index3 != index1)
      {
        index1 = index3;
        genericVariable.value = (__Null) (Activator.CreateInstance(sharedVariableType) as SharedVariable);
      }
      GUILayout.Space(3f);
      genericVariable.type = (__Null) ("Shared" + SharedGenericVariableDrawer.variableNames[index1]);
      genericVariable.value = (__Null) FieldInspector.DrawSharedVariable((Task) null, new GUIContent("Value"), (FieldInfo) null, sharedVariableType, (SharedVariable) genericVariable.value);
      EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
    }
    EditorGUILayout.EndVertical();
  }
}
