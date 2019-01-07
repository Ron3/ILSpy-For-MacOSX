using BehaviorDesigner.Runtime;
using System;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	[CustomEditor(typeof(BehaviorManager))]
	public class BehaviorManagerInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			BehaviorManager behaviorManager = this.get_target() as BehaviorManager;
			behaviorManager.set_UpdateInterval((UpdateIntervalType)EditorGUILayout.EnumPopup("Update Interval", behaviorManager.get_UpdateInterval(), new GUILayoutOption[0]));
			if (behaviorManager.get_UpdateInterval() == 1)
			{
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				behaviorManager.set_UpdateIntervalSeconds(EditorGUILayout.FloatField("Seconds", behaviorManager.get_UpdateIntervalSeconds(), new GUILayoutOption[0]));
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			}
			behaviorManager.set_ExecutionsPerTick((BehaviorManager.ExecutionsPerTickType)EditorGUILayout.EnumPopup("Task Execution Type", behaviorManager.get_ExecutionsPerTick(), new GUILayoutOption[0]));
			if (behaviorManager.get_ExecutionsPerTick() == 1)
			{
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				behaviorManager.set_MaxTaskExecutionsPerTick(EditorGUILayout.IntField("Max Execution Count", behaviorManager.get_MaxTaskExecutionsPerTick(), new GUILayoutOption[0]));
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			}
		}
	}
}
