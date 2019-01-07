using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	[CustomEditor(typeof(Behavior))]
	public class BehaviorInspector : UnityEditor.Editor
	{
		private bool mShowOptions = true;

		private bool mShowVariables;

		private static List<float> variablePosition;

		private static int selectedVariableIndex = -1;

		private static string selectedVariableName;

		private static int selectedVariableTypeIndex;

		private void OnEnable()
		{
			Behavior behavior = this.get_target() as Behavior;
			if (behavior == null)
			{
				return;
			}
			GizmoManager.UpdateGizmo(behavior);
		}

		public override void OnInspectorGUI()
		{
			Behavior behavior = this.get_target() as Behavior;
			if (behavior == null)
			{
				return;
			}
			bool flag = false;
			if (BehaviorInspector.DrawInspectorGUI(behavior, base.get_serializedObject(), true, ref flag, ref this.mShowOptions, ref this.mShowVariables))
			{
				BehaviorDesignerUtility.SetObjectDirty(behavior);
				if (flag && BehaviorDesignerWindow.instance != null && behavior.GetBehaviorSource().get_BehaviorID() == BehaviorDesignerWindow.instance.ActiveBehaviorID)
				{
					BehaviorDesignerWindow.instance.LoadBehavior(behavior.GetBehaviorSource(), false, false);
				}
			}
		}

		public static bool DrawInspectorGUI(Behavior behavior, SerializedObject serializedObject, bool fromInspector, ref bool externalModification, ref bool showOptions, ref bool showVariables)
		{
			EditorGUI.BeginChangeCheck();
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUILayout.LabelField("Behavior Name", new GUILayoutOption[]
			{
				GUILayout.Width(120f)
			});
			behavior.GetBehaviorSource().behaviorName = EditorGUILayout.TextField(behavior.GetBehaviorSource().behaviorName, new GUILayoutOption[0]);
			if (fromInspector && GUILayout.Button("Open", new GUILayoutOption[0]))
			{
				BehaviorDesignerWindow.ShowWindow();
				BehaviorDesignerWindow.instance.LoadBehavior(behavior.GetBehaviorSource(), false, true);
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Behavior Description", new GUILayoutOption[0]);
			behavior.GetBehaviorSource().behaviorDescription = EditorGUILayout.TextArea(behavior.GetBehaviorSource().behaviorDescription, BehaviorDesignerUtility.TaskInspectorCommentGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Height(48f)
			});
			serializedObject.Update();
			GUI.set_enabled(PrefabUtility.GetPrefabType(behavior) != 3 || BehaviorDesignerPreferences.GetBool(BDPreferences.EditablePrefabInstances));
			SerializedProperty serializedProperty = serializedObject.FindProperty("externalBehavior");
			ExternalBehavior externalBehavior = serializedProperty.get_objectReferenceValue() as ExternalBehavior;
			EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
			serializedObject.ApplyModifiedProperties();
			if ((!object.ReferenceEquals(behavior.get_ExternalBehavior(), null) && !behavior.get_ExternalBehavior().Equals(externalBehavior)) || (!object.ReferenceEquals(externalBehavior, null) && !externalBehavior.Equals(behavior.get_ExternalBehavior())))
			{
				if (!object.ReferenceEquals(behavior.get_ExternalBehavior(), null))
				{
					behavior.get_ExternalBehavior().get_BehaviorSource().set_Owner(behavior.get_ExternalBehavior());
					behavior.get_ExternalBehavior().get_BehaviorSource().CheckForSerialization(true, behavior.GetBehaviorSource());
				}
				else
				{
					behavior.GetBehaviorSource().set_EntryTask(null);
					behavior.GetBehaviorSource().set_RootTask(null);
					behavior.GetBehaviorSource().set_DetachedTasks(null);
					behavior.GetBehaviorSource().set_Variables(null);
					behavior.GetBehaviorSource().CheckForSerialization(true, null);
					behavior.GetBehaviorSource().set_Variables(null);
					if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
					{
						BinarySerialization.Save(behavior.GetBehaviorSource());
					}
					else
					{
						JSONSerialization.Save(behavior.GetBehaviorSource());
					}
				}
				externalModification = true;
			}
			GUI.set_enabled(true);
			serializedProperty = serializedObject.FindProperty("group");
			EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
			string text;
			if (fromInspector)
			{
				text = "BehaviorDesigner.VariablesFoldout." + behavior.GetHashCode();
				if (showVariables = EditorGUILayout.Foldout(EditorPrefs.GetBool(text, true), "Variables"))
				{
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
					bool flag = false;
					BehaviorSource behaviorSource = behavior.GetBehaviorSource();
					List<SharedVariable> allVariables = behaviorSource.GetAllVariables();
					if (allVariables != null && allVariables.get_Count() > 0)
					{
						if (VariableInspector.DrawAllVariables(false, behaviorSource, ref allVariables, false, ref BehaviorInspector.variablePosition, ref BehaviorInspector.selectedVariableIndex, ref BehaviorInspector.selectedVariableName, ref BehaviorInspector.selectedVariableTypeIndex, false, true))
						{
							if (!EditorApplication.get_isPlayingOrWillChangePlaymode() && behavior.get_ExternalBehavior() != null)
							{
								BehaviorSource behaviorSource2 = behavior.get_ExternalBehavior().GetBehaviorSource();
								behaviorSource2.CheckForSerialization(true, null);
								if (VariableInspector.SyncVariables(behaviorSource2, allVariables))
								{
									if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
									{
										BinarySerialization.Save(behaviorSource2);
									}
									else
									{
										JSONSerialization.Save(behaviorSource2);
									}
								}
							}
							flag = true;
						}
					}
					else
					{
						EditorGUILayout.LabelField("There are no variables to display", new GUILayoutOption[0]);
					}
					if (flag)
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
			text = "BehaviorDesigner.OptionsFoldout." + behavior.GetHashCode();
			if (!fromInspector || (showOptions = EditorGUILayout.Foldout(EditorPrefs.GetBool(text, true), "Options")))
			{
				if (fromInspector)
				{
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				}
				serializedProperty = serializedObject.FindProperty("startWhenEnabled");
				EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
				serializedProperty = serializedObject.FindProperty("pauseWhenDisabled");
				EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
				serializedProperty = serializedObject.FindProperty("restartWhenComplete");
				EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
				serializedProperty = serializedObject.FindProperty("resetValuesOnRestart");
				EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
				serializedProperty = serializedObject.FindProperty("logTaskChanges");
				EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
				if (fromInspector)
				{
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
				}
			}
			if (fromInspector)
			{
				EditorPrefs.SetBool(text, showOptions);
			}
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
				return true;
			}
			return false;
		}
	}
}
