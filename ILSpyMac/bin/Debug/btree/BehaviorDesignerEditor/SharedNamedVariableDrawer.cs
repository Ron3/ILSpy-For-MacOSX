using BehaviorDesigner.Editor;
using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomObjectDrawer(typeof(NamedVariable))]
public class SharedNamedVariableDrawer : ObjectDrawer
{
	private static string[] variableNames;

	public override void OnGUI(GUIContent label)
	{
		NamedVariable namedVariable = this.value as NamedVariable;
		EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
		if (FieldInspector.DrawFoldout(namedVariable.GetHashCode(), label))
		{
			EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
			if (SharedNamedVariableDrawer.variableNames == null)
			{
				List<Type> list = VariableInspector.FindAllSharedVariableTypes(true);
				SharedNamedVariableDrawer.variableNames = new string[list.get_Count()];
				for (int i = 0; i < list.get_Count(); i++)
				{
					SharedNamedVariableDrawer.variableNames[i] = list.get_Item(i).get_Name().Remove(0, 6);
				}
			}
			int num = 0;
			string text = namedVariable.type.Remove(0, 6);
			for (int j = 0; j < SharedNamedVariableDrawer.variableNames.Length; j++)
			{
				if (SharedNamedVariableDrawer.variableNames[j].Equals(text))
				{
					num = j;
					break;
				}
			}
			namedVariable.name = EditorGUILayout.TextField("Name", namedVariable.name, new GUILayoutOption[0]);
			int num2 = EditorGUILayout.Popup("Type", num, SharedNamedVariableDrawer.variableNames, BehaviorDesignerUtility.SharedVariableToolbarPopup, new GUILayoutOption[0]);
			Type type = VariableInspector.FindAllSharedVariableTypes(true).get_Item(num2);
			if (num2 != num)
			{
				num = num2;
				namedVariable.value = (Activator.CreateInstance(type) as SharedVariable);
			}
			GUILayout.Space(3f);
			namedVariable.type = "Shared" + SharedNamedVariableDrawer.variableNames[num];
			namedVariable.value = FieldInspector.DrawSharedVariable(null, new GUIContent("Value"), null, type, namedVariable.value);
			EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
		}
		EditorGUILayout.EndVertical();
	}
}