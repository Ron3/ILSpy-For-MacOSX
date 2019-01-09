using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	[CustomEditor(typeof(VariableSynchronizer))]
	public class VariableSynchronizerInspector : UnityEditor.Editor
	{
		public enum ComponentListType
		{
			Instant = 0,
			Popup = 1,
			BehaviorDesignerGroup = 2,
			None = 3
		}

		[Serializable]
		public class Synchronizer
		{
			public GameObject gameObject;

			public Component component;

			public string targetName;

			public bool global;

			public int componentGroup;

			public string componentName;
		}

		[SerializeField]
		private VariableSynchronizerInspector.Synchronizer sharedVariableSynchronizer = new VariableSynchronizerInspector.Synchronizer();

		[SerializeField]
		private string sharedVariableValueTypeName;

		private Type sharedVariableValueType;

		[SerializeField]
		private VariableSynchronizer.SynchronizationType synchronizationType;

		[SerializeField]
		private bool setVariable;

		[SerializeField]
		private VariableSynchronizerInspector.Synchronizer targetSynchronizer;

		private Action<VariableSynchronizerInspector.Synchronizer, Type> thirdPartySynchronizer;

		private Type playMakerSynchronizationType;

		private Type uFrameSynchronizationType;

		public override void OnInspectorGUI()
		{
			VariableSynchronizer variableSynchronizer = this.get_target() as VariableSynchronizer;
			if (variableSynchronizer == null)
			{
				return;
			}
			GUILayout.Space(5f);
			variableSynchronizer.set_UpdateInterval((UpdateIntervalType)EditorGUILayout.EnumPopup("Update Interval", variableSynchronizer.get_UpdateInterval(), new GUILayoutOption[0]));
			if (variableSynchronizer.get_UpdateInterval() == 1)
			{
				variableSynchronizer.set_UpdateIntervalSeconds(EditorGUILayout.FloatField("Seconds", variableSynchronizer.get_UpdateIntervalSeconds(), new GUILayoutOption[0]));
			}
			GUILayout.Space(5f);
			GUI.set_enabled(!Application.get_isPlaying());
			this.DrawSharedVariableSynchronizer(this.sharedVariableSynchronizer, null);
			if (string.IsNullOrEmpty(this.sharedVariableSynchronizer.targetName))
			{
				this.DrawSynchronizedVariables(variableSynchronizer);
				return;
			}
			EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUILayout.LabelField("Direction", new GUILayoutOption[]
			{
				GUILayout.MaxWidth(146f)
			});
			if (GUILayout.Button(BehaviorDesignerUtility.LoadTexture((!this.setVariable) ? "RightArrowButton.png" : "LeftArrowButton.png", true, this), BehaviorDesignerUtility.ButtonGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Width(22f)
			}))
			{
				this.setVariable = !this.setVariable;
			}
			EditorGUILayout.EndHorizontal();
			EditorGUI.BeginChangeCheck();
			this.synchronizationType = (VariableSynchronizer.SynchronizationType)EditorGUILayout.EnumPopup("Type", this.synchronizationType, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				this.targetSynchronizer = new VariableSynchronizerInspector.Synchronizer();
			}
			if (this.targetSynchronizer == null)
			{
				this.targetSynchronizer = new VariableSynchronizerInspector.Synchronizer();
			}
			if (this.sharedVariableValueType == null && !string.IsNullOrEmpty(this.sharedVariableValueTypeName))
			{
				this.sharedVariableValueType = TaskUtility.GetTypeWithinAssembly(this.sharedVariableValueTypeName);
			}
			switch (this.synchronizationType)
			{
			case 0:
				this.DrawSharedVariableSynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
				break;
			case 1:
				this.DrawPropertySynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
				break;
			case 2:
				this.DrawAnimatorSynchronizer(this.targetSynchronizer);
				break;
			case 3:
				this.DrawPlayMakerSynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
				break;
			case 4:
				this.DrawuFrameSynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
				break;
			}
			if (string.IsNullOrEmpty(this.targetSynchronizer.targetName))
			{
				GUI.set_enabled(false);
			}
			if (GUILayout.Button("Add", new GUILayoutOption[0]))
			{
				VariableSynchronizer.SynchronizedVariable synchronizedVariable = new VariableSynchronizer.SynchronizedVariable(this.synchronizationType, this.setVariable, this.sharedVariableSynchronizer.component as Behavior, this.sharedVariableSynchronizer.targetName, this.sharedVariableSynchronizer.global, this.targetSynchronizer.component, this.targetSynchronizer.targetName, this.targetSynchronizer.global);
				variableSynchronizer.get_SynchronizedVariables().Add(synchronizedVariable);
				BehaviorDesignerUtility.SetObjectDirty(variableSynchronizer);
				this.sharedVariableSynchronizer = new VariableSynchronizerInspector.Synchronizer();
				this.targetSynchronizer = new VariableSynchronizerInspector.Synchronizer();
			}
			GUI.set_enabled(true);
			this.DrawSynchronizedVariables(variableSynchronizer);
		}

		public static void DrawComponentSelector(VariableSynchronizerInspector.Synchronizer synchronizer, Type componentType, VariableSynchronizerInspector.ComponentListType listType)
		{
			bool flag = false;
			EditorGUI.BeginChangeCheck();
			synchronizer.gameObject = (EditorGUILayout.ObjectField("GameObject", synchronizer.gameObject, typeof(GameObject), true, new GUILayoutOption[0]) as GameObject);
			if (EditorGUI.EndChangeCheck())
			{
				flag = true;
			}
			if (synchronizer.gameObject == null)
			{
				GUI.set_enabled(false);
			}
			switch (listType)
			{
			case VariableSynchronizerInspector.ComponentListType.Instant:
				if (flag)
				{
					if (synchronizer.gameObject != null)
					{
						synchronizer.component = synchronizer.gameObject.GetComponent(componentType);
					}
					else
					{
						synchronizer.component = null;
					}
				}
				break;
			case VariableSynchronizerInspector.ComponentListType.Popup:
			{
				int num = 0;
				List<string> list = new List<string>();
				Component[] array = null;
				list.Add("None");
				if (synchronizer.gameObject != null)
				{
					array = synchronizer.gameObject.GetComponents(componentType);
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].Equals(synchronizer.component))
						{
							num = list.get_Count();
						}
						string text = BehaviorDesignerUtility.SplitCamelCase(array[i].GetType().get_Name());
						int num2 = 0;
						for (int j = 0; j < list.get_Count(); j++)
						{
							if (list.get_Item(i).Equals(text))
							{
								num2++;
							}
						}
						if (num2 > 0)
						{
							text = text + " " + num2;
						}
						list.Add(text);
					}
				}
				EditorGUI.BeginChangeCheck();
				num = EditorGUILayout.Popup("Component", num, list.ToArray(), new GUILayoutOption[0]);
				if (EditorGUI.EndChangeCheck())
				{
					if (num != 0)
					{
						synchronizer.component = array[num - 1];
					}
					else
					{
						synchronizer.component = null;
					}
				}
				break;
			}
			case VariableSynchronizerInspector.ComponentListType.BehaviorDesignerGroup:
				if (synchronizer.gameObject != null)
				{
					Behavior[] components = synchronizer.gameObject.GetComponents<Behavior>();
					if (components != null && components.Length > 1)
					{
						synchronizer.componentGroup = EditorGUILayout.IntField("Behavior Tree Group", synchronizer.componentGroup, new GUILayoutOption[0]);
					}
					synchronizer.component = VariableSynchronizerInspector.GetBehaviorWithGroup(components, synchronizer.componentGroup);
				}
				break;
			}
		}

		private bool DrawSharedVariableSynchronizer(VariableSynchronizerInspector.Synchronizer synchronizer, Type valueType)
		{
			VariableSynchronizerInspector.DrawComponentSelector(synchronizer, typeof(Behavior), VariableSynchronizerInspector.ComponentListType.BehaviorDesignerGroup);
			int num = 0;
			int num2 = -1;
			string[] array = null;
			if (synchronizer.component != null)
			{
				Behavior behavior = synchronizer.component as Behavior;
				num = FieldInspector.GetVariablesOfType(valueType, synchronizer.global, synchronizer.targetName, behavior.GetBehaviorSource(), out array, ref num2, valueType == null);
			}
			else
			{
				array = new string[]
				{
					"None"
				};
			}
			EditorGUI.BeginChangeCheck();
			num = EditorGUILayout.Popup("Shared Variable", num, array, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				if (num != 0)
				{
					if (num2 != -1 && num >= num2)
					{
						synchronizer.targetName = array[num].Substring(8, array[num].get_Length() - 8);
						synchronizer.global = true;
					}
					else
					{
						synchronizer.targetName = array[num];
						synchronizer.global = false;
					}
					if (valueType == null)
					{
						SharedVariable variable;
						if (synchronizer.global)
						{
							variable = GlobalVariables.get_Instance().GetVariable(synchronizer.targetName);
						}
						else
						{
							Behavior behavior2 = synchronizer.component as Behavior;
							variable = behavior2.GetVariable(array[num]);
						}
						this.sharedVariableValueTypeName = variable.GetType().GetProperty("Value").get_PropertyType().get_FullName();
						this.sharedVariableValueType = null;
					}
				}
				else
				{
					synchronizer.targetName = null;
				}
			}
			if (string.IsNullOrEmpty(synchronizer.targetName))
			{
				GUI.set_enabled(false);
			}
			return GUI.get_enabled();
		}

		private static Behavior GetBehaviorWithGroup(Behavior[] behaviors, int group)
		{
			if (behaviors == null || behaviors.Length == 0)
			{
				return null;
			}
			if (behaviors.Length == 1)
			{
				return behaviors[0];
			}
			for (int i = 0; i < behaviors.Length; i++)
			{
				if (behaviors[i].get_Group() == group)
				{
					return behaviors[i];
				}
			}
			return behaviors[0];
		}

		private void DrawPropertySynchronizer(VariableSynchronizerInspector.Synchronizer synchronizer, Type valueType)
		{
			VariableSynchronizerInspector.DrawComponentSelector(synchronizer, typeof(Component), VariableSynchronizerInspector.ComponentListType.Popup);
			int num = 0;
			List<string> list = new List<string>();
			list.Add("None");
			if (synchronizer.component != null)
			{
				PropertyInfo[] properties = synchronizer.component.GetType().GetProperties(20);
				for (int i = 0; i < properties.Length; i++)
				{
					if (properties[i].get_PropertyType().Equals(valueType) && !properties[i].get_IsSpecialName())
					{
						if (properties[i].get_Name().Equals(synchronizer.targetName))
						{
							num = list.get_Count();
						}
						list.Add(properties[i].get_Name());
					}
				}
			}
			EditorGUI.BeginChangeCheck();
			num = EditorGUILayout.Popup("Property", num, list.ToArray(), new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				if (num != 0)
				{
					synchronizer.targetName = list.get_Item(num);
				}
				else
				{
					synchronizer.targetName = string.Empty;
				}
			}
		}

		private void DrawAnimatorSynchronizer(VariableSynchronizerInspector.Synchronizer synchronizer)
		{
			VariableSynchronizerInspector.DrawComponentSelector(synchronizer, typeof(Animator), VariableSynchronizerInspector.ComponentListType.Instant);
			synchronizer.targetName = EditorGUILayout.TextField("Parameter Name", synchronizer.targetName, new GUILayoutOption[0]);
		}

		private void DrawPlayMakerSynchronizer(VariableSynchronizerInspector.Synchronizer synchronizer, Type valueType)
		{
			if (this.playMakerSynchronizationType == null)
			{
				this.playMakerSynchronizationType = Type.GetType("BehaviorDesigner.Editor.VariableSynchronizerInspector_PlayMaker, Assembly-CSharp-Editor");
				if (this.playMakerSynchronizationType == null)
				{
					EditorGUILayout.LabelField("Unable to find PlayMaker inspector task.", new GUILayoutOption[0]);
					return;
				}
			}
			if (this.thirdPartySynchronizer == null)
			{
				MethodInfo method = this.playMakerSynchronizationType.GetMethod("DrawPlayMakerSynchronizer");
				if (method != null)
				{
					this.thirdPartySynchronizer = (Action<VariableSynchronizerInspector.Synchronizer, Type>)Delegate.CreateDelegate(typeof(Action<VariableSynchronizerInspector.Synchronizer, Type>), method);
				}
			}
			this.thirdPartySynchronizer.Invoke(synchronizer, valueType);
		}

		private void DrawuFrameSynchronizer(VariableSynchronizerInspector.Synchronizer synchronizer, Type valueType)
		{
			if (this.uFrameSynchronizationType == null)
			{
				this.uFrameSynchronizationType = Type.GetType("BehaviorDesigner.Editor.VariableSynchronizerInspector_uFrame, Assembly-CSharp-Editor");
				if (this.uFrameSynchronizationType == null)
				{
					EditorGUILayout.LabelField("Unable to find uFrame inspector task.", new GUILayoutOption[0]);
					return;
				}
			}
			if (this.thirdPartySynchronizer == null)
			{
				MethodInfo method = this.uFrameSynchronizationType.GetMethod("DrawSynchronizer");
				if (method != null)
				{
					this.thirdPartySynchronizer = (Action<VariableSynchronizerInspector.Synchronizer, Type>)Delegate.CreateDelegate(typeof(Action<VariableSynchronizerInspector.Synchronizer, Type>), method);
				}
			}
			this.thirdPartySynchronizer.Invoke(synchronizer, valueType);
		}

		private void DrawSynchronizedVariables(VariableSynchronizer variableSynchronizer)
		{
			GUI.set_enabled(true);
			if (variableSynchronizer.get_SynchronizedVariables() == null || variableSynchronizer.get_SynchronizedVariables().get_Count() == 0)
			{
				return;
			}
			Rect lastRect = GUILayoutUtility.GetLastRect();
			lastRect.set_x(-5f);
			lastRect.set_y(lastRect.get_y() + (lastRect.get_height() + 1f));
			lastRect.set_height(2f);
			lastRect.set_width(lastRect.get_width() + 20f);
			GUI.DrawTexture(lastRect, BehaviorDesignerUtility.LoadTexture("ContentSeparator.png", true, this));
			GUILayout.Space(6f);
			for (int i = 0; i < variableSynchronizer.get_SynchronizedVariables().get_Count(); i++)
			{
				VariableSynchronizer.SynchronizedVariable synchronizedVariable = variableSynchronizer.get_SynchronizedVariables().get_Item(i);
				if (synchronizedVariable.global)
				{
					if (GlobalVariables.get_Instance().GetVariable(synchronizedVariable.variableName) == null)
					{
						variableSynchronizer.get_SynchronizedVariables().RemoveAt(i);
						break;
					}
				}
				else if (synchronizedVariable.behavior.GetVariable(synchronizedVariable.variableName) == null)
				{
					variableSynchronizer.get_SynchronizedVariables().RemoveAt(i);
					break;
				}
				EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
				EditorGUILayout.LabelField(synchronizedVariable.variableName, new GUILayoutOption[]
				{
					GUILayout.MaxWidth(120f)
				});
				if (GUILayout.Button(BehaviorDesignerUtility.LoadTexture((!synchronizedVariable.setVariable) ? "RightArrowButton.png" : "LeftArrowButton.png", true, this), BehaviorDesignerUtility.ButtonGUIStyle, new GUILayoutOption[]
				{
					GUILayout.Width(22f)
				}) && !Application.get_isPlaying())
				{
					synchronizedVariable.setVariable = !synchronizedVariable.setVariable;
				}
				EditorGUILayout.LabelField(string.Format("{0} ({1})", synchronizedVariable.targetName, synchronizedVariable.synchronizationType.ToString()), new GUILayoutOption[]
				{
					GUILayout.MinWidth(120f)
				});
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(BehaviorDesignerUtility.LoadTexture("DeleteButton.png", true, this), BehaviorDesignerUtility.ButtonGUIStyle, new GUILayoutOption[]
				{
					GUILayout.Width(22f)
				}))
				{
					variableSynchronizer.get_SynchronizedVariables().RemoveAt(i);
					EditorGUILayout.EndHorizontal();
					break;
				}
				GUILayout.Space(2f);
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(2f);
			}
			GUILayout.Space(4f);
		}
	}
}