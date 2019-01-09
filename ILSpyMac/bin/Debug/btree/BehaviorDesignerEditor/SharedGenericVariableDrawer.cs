using BehaviorDesigner.Editor;
using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomObjectDrawer(typeof(GenericVariable))]
public class SharedGenericVariableDrawer : ObjectDrawer
{
	private static string[] variableNames;

	public override void OnGUI(GUIContent label)
	{
		GenericVariable genericVariable = this.value as GenericVariable;
		EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
		if (FieldInspector.DrawFoldout(genericVariable.GetHashCode(), label))
		{
			EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
			if (SharedGenericVariableDrawer.variableNames == null)
			{
				List<Type> list = VariableInspector.FindAllSharedVariableTypes(true);
				SharedGenericVariableDrawer.variableNames = new string[list.get_Count()];
				for (int i = 0; i < list.get_Count(); i++)
				{
					SharedGenericVariableDrawer.variableNames[i] = list.get_Item(i).get_Name().Remove(0, 6);
				}
			}
			int num = 0;
			string text = genericVariable.type.Remove(0, 6);
			for (int j = 0; j < SharedGenericVariableDrawer.variableNames.Length; j++)
			{
				if (SharedGenericVariableDrawer.variableNames[j].Equals(text))
				{
					num = j;
					break;
				}
			}
			int num2 = EditorGUILayout.Popup("Type", num, SharedGenericVariableDrawer.variableNames, BehaviorDesignerUtility.SharedVariableToolbarPopup, new GUILayoutOption[0]);
			Type type = VariableInspector.FindAllSharedVariableTypes(true).get_Item(num2);
			if (num2 != num)
			{
				num = num2;
				genericVariable.value = (Activator.CreateInstance(type) as SharedVariable);
			}
			GUILayout.Space(3f);
			genericVariable.type = "Shared" + SharedGenericVariableDrawer.variableNames[num];
			genericVariable.value = FieldInspector.DrawSharedVariable(null, new GUIContent("Value"), null, type, genericVariable.value);
			EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
		}
		EditorGUILayout.EndVertical();
	}
}