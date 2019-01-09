// Decompiled with JetBrains decompiler
// Type: SharedNamedVariableDrawer
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

[CustomObjectDrawer(typeof (NamedVariable))]
public class SharedNamedVariableDrawer : ObjectDrawer
{
  private static string[] variableNames;

  public override void OnGUI(GUIContent label)
  {
    NamedVariable namedVariable = this.value as NamedVariable;
    EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
    if (FieldInspector.DrawFoldout(((object) namedVariable).GetHashCode(), label))
    {
      EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
      if (SharedNamedVariableDrawer.variableNames == null)
      {
        List<Type> sharedVariableTypes = VariableInspector.FindAllSharedVariableTypes(true);
        SharedNamedVariableDrawer.variableNames = new string[sharedVariableTypes.Count];
        for (int index = 0; index < sharedVariableTypes.Count; ++index)
          SharedNamedVariableDrawer.variableNames[index] = sharedVariableTypes[index].Name.Remove(0, 6);
      }
      int index1 = 0;
      string str = ((string) ((GenericVariable) namedVariable).type).Remove(0, 6);
      for (int index2 = 0; index2 < SharedNamedVariableDrawer.variableNames.Length; ++index2)
      {
        if (SharedNamedVariableDrawer.variableNames[index2].Equals(str))
        {
          index1 = index2;
          break;
        }
      }
      namedVariable.name = (__Null) EditorGUILayout.TextField("Name", (string) namedVariable.name, new GUILayoutOption[0]);
      int index3 = EditorGUILayout.Popup("Type", index1, SharedNamedVariableDrawer.variableNames, BehaviorDesignerUtility.SharedVariableToolbarPopup, new GUILayoutOption[0]);
      Type sharedVariableType = VariableInspector.FindAllSharedVariableTypes(true)[index3];
      if (index3 != index1)
      {
        index1 = index3;
        ((GenericVariable) namedVariable).value = (__Null) (Activator.CreateInstance(sharedVariableType) as SharedVariable);
      }
      GUILayout.Space(3f);
      ((GenericVariable) namedVariable).type = (__Null) ("Shared" + SharedNamedVariableDrawer.variableNames[index1]);
      ((GenericVariable) namedVariable).value = (__Null) FieldInspector.DrawSharedVariable((Task) null, new GUIContent("Value"), (FieldInfo) null, sharedVariableType, (SharedVariable) ((GenericVariable) namedVariable).value);
      EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
    }
    EditorGUILayout.EndVertical();
  }
}
