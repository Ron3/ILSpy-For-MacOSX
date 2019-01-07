using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	[CustomEditor(typeof(ExternalBehavior))]
	public class ExternalBehaviorInspector : UnityEditor.Editor
	{
		private bool mShowVariables;

		private static List<float> variablePosition;

		private static int selectedVariableIndex = -1;

		private static string selectedVariableName;

		private static int selectedVariableTypeIndex;

		public override void OnInspectorGUI()
		{
			ExternalBehavior externalBehavior = this.get_target() as ExternalBehavior;
			if (externalBehavior == null)
			{
				return;
			}
			if (externalBehavior.get_BehaviorSource().get_Owner() == null)
			{
				externalBehavior.get_BehaviorSource().set_Owner(externalBehavior);
			}
			if (ExternalBehaviorInspector.DrawInspectorGUI(externalBehavior.get_BehaviorSource(), true, ref this.mShowVariables))
			{
				BehaviorDesignerUtility.SetObjectDirty(externalBehavior);
			}
		}

		public void Reset()
		{
			ExternalBehavior externalBehavior = this.get_target() as ExternalBehavior;
			if (externalBehavior == null)
			{
				return;
			}
			if (externalBehavior.get_BehaviorSource().get_Owner() == null)
			{
				externalBehavior.get_BehaviorSource().set_Owner(externalBehavior);
			}
		}

		public static bool DrawInspectorGUI(BehaviorSource behaviorSource, bool fromInspector, ref bool showVariables)
		{
			EditorGUI.BeginChangeCheck();
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUILayout.LabelField("Behavior Name", new GUILayoutOption[]
			{
				GUILayout.Width(120f)
			});
			behaviorSource.behaviorName = EditorGUILayout.TextField(behaviorSource.behaviorName, new GUILayoutOption[0]);
			if (fromInspector && GUILayout.Button("Open", new GUILayoutOption[0]))
			{
				BehaviorDesignerWindow.ShowWindow();
				BehaviorDesignerWindow.instance.LoadBehavior(behaviorSource, false, true);
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Behavior Description", new GUILayoutOption[0]);
			behaviorSource.behaviorDescription = EditorGUILayout.TextArea(behaviorSource.behaviorDescription, new GUILayoutOption[]
			{
				GUILayout.Height(48f)
			});
			if (fromInspector)
			{
				string text = "BehaviorDesigner.VariablesFoldout." + behaviorSource.GetHashCode();
				if (showVariables = EditorGUILayout.Foldout(EditorPrefs.GetBool(text, true), "Variables"))
				{
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
					List<SharedVariable> allVariables = behaviorSource.GetAllVariables();
					if (allVariables != null && VariableInspector.DrawAllVariables(false, behaviorSource, ref allVariables, false, ref ExternalBehaviorInspector.variablePosition, ref ExternalBehaviorInspector.selectedVariableIndex, ref ExternalBehaviorInspector.selectedVariableName, ref ExternalBehaviorInspector.selectedVariableTypeIndex, true, false))
					{
						if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
						{
							BinarySerialization.Save(behaviorSource);
						}
						else
						{
							JSONSerialization.Save(behaviorSource);
						}
					}
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
				}
				EditorPrefs.SetBool(text, showVariables);
			}
			return EditorGUI.EndChangeCheck();
		}
	}
}
