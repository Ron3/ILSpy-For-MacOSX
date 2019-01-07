using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	[Serializable]
	public class TaskInspector : ScriptableObject
	{
		private class TaskColor
		{
			public Task task;

			public int colorIndex;

			public TaskColor(Task task, int colorIndex)
			{
				this.task = task;
				this.colorIndex = colorIndex;
			}
		}

		private BehaviorDesignerWindow behaviorDesignerWindow;

		private Task activeReferenceTask;

		private FieldInfo activeReferenceTaskFieldInfo;

		private Task mActiveMenuSelectionTask;

		private Vector2 mScrollPosition = Vector2.get_zero();

		public Task ActiveReferenceTask
		{
			get
			{
				return this.activeReferenceTask;
			}
		}

		public FieldInfo ActiveReferenceTaskFieldInfo
		{
			get
			{
				return this.activeReferenceTaskFieldInfo;
			}
		}

		public void OnEnable()
		{
			base.set_hideFlags(61);
		}

		public void ClearFocus()
		{
			GUIUtility.set_keyboardControl(0);
		}

		public bool HasFocus()
		{
			return GUIUtility.get_keyboardControl() != 0;
		}

		public bool DrawTaskInspector(BehaviorSource behaviorSource, TaskList taskList, Task task, bool enabled)
		{
			if (task == null || (task.get_NodeData().get_NodeDesigner() as NodeDesigner).IsEntryDisplay)
			{
				return false;
			}
			this.mScrollPosition = GUILayout.BeginScrollView(this.mScrollPosition, new GUILayoutOption[0]);
			GUI.set_enabled(enabled);
			if (this.behaviorDesignerWindow == null)
			{
				this.behaviorDesignerWindow = BehaviorDesignerWindow.instance;
			}
			EditorGUIUtility.set_labelWidth(150f);
			EditorGUI.BeginChangeCheck();
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUILayout.LabelField("Name", new GUILayoutOption[]
			{
				GUILayout.Width(90f)
			});
			task.set_FriendlyName(EditorGUILayout.TextField(task.get_FriendlyName(), new GUILayoutOption[0]));
			if (GUILayout.Button(BehaviorDesignerUtility.DocTexture, BehaviorDesignerUtility.TransparentButtonGUIStyle, new GUILayoutOption[0]))
			{
				this.OpenHelpURL(task);
			}
			if (GUILayout.Button(BehaviorDesignerUtility.ColorSelectorTexture(task.get_NodeData().get_ColorIndex()), BehaviorDesignerUtility.TransparentButtonOffsetGUIStyle, new GUILayoutOption[0]))
			{
				GenericMenu genericMenu = new GenericMenu();
				this.AddColorMenuItem(ref genericMenu, task, "Default", 0);
				this.AddColorMenuItem(ref genericMenu, task, "Red", 1);
				this.AddColorMenuItem(ref genericMenu, task, "Pink", 2);
				this.AddColorMenuItem(ref genericMenu, task, "Brown", 3);
				this.AddColorMenuItem(ref genericMenu, task, "Orange", 4);
				this.AddColorMenuItem(ref genericMenu, task, "Turquoise", 5);
				this.AddColorMenuItem(ref genericMenu, task, "Cyan", 6);
				this.AddColorMenuItem(ref genericMenu, task, "Blue", 7);
				this.AddColorMenuItem(ref genericMenu, task, "Purple", 8);
				genericMenu.ShowAsContext();
			}
			if (GUILayout.Button(BehaviorDesignerUtility.GearTexture, BehaviorDesignerUtility.TransparentButtonGUIStyle, new GUILayoutOption[0]))
			{
				GenericMenu genericMenu2 = new GenericMenu();
				genericMenu2.AddItem(new GUIContent("Edit Script"), false, new GenericMenu.MenuFunction2(TaskInspector.OpenInFileEditor), task);
				genericMenu2.AddItem(new GUIContent("Locate Script"), false, new GenericMenu.MenuFunction2(TaskInspector.SelectInProject), task);
				genericMenu2.AddItem(new GUIContent("Reset"), false, new GenericMenu.MenuFunction2(this.ResetTask), task);
				genericMenu2.ShowAsContext();
			}
			GUILayout.EndHorizontal();
			string text = BehaviorDesignerUtility.SplitCamelCase(task.GetType().get_Name().ToString());
			if (!task.get_FriendlyName().Equals(text))
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				EditorGUILayout.LabelField("Type", new GUILayoutOption[]
				{
					GUILayout.Width(90f)
				});
				EditorGUILayout.LabelField(text, new GUILayoutOption[]
				{
					GUILayout.MaxWidth(170f)
				});
				GUILayout.EndHorizontal();
			}
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUILayout.LabelField("Instant", new GUILayoutOption[]
			{
				GUILayout.Width(90f)
			});
			task.set_IsInstant(EditorGUILayout.Toggle(task.get_IsInstant(), new GUILayoutOption[0]));
			GUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Comment", new GUILayoutOption[0]);
			task.get_NodeData().set_Comment(EditorGUILayout.TextArea(task.get_NodeData().get_Comment(), BehaviorDesignerUtility.TaskInspectorCommentGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Height(48f)
			}));
			if (EditorGUI.EndChangeCheck())
			{
				GUI.set_changed(true);
			}
			BehaviorDesignerUtility.DrawContentSeperator(2);
			GUILayout.Space(6f);
			if (this.DrawTaskFields(behaviorSource, taskList, task, enabled))
			{
				BehaviorUndo.RegisterUndo("Inspector", behaviorSource.get_Owner().GetObject());
				GUI.set_changed(true);
			}
			GUI.set_enabled(true);
			GUILayout.EndScrollView();
			return GUI.get_changed();
		}

		private bool DrawTaskFields(BehaviorSource behaviorSource, TaskList taskList, Task task, bool enabled)
		{
			if (task == null)
			{
				return false;
			}
			EditorGUI.BeginChangeCheck();
			FieldInspector.behaviorSource = behaviorSource;
			this.DrawObjectFields(behaviorSource, taskList, task, task, enabled, true);
			return EditorGUI.EndChangeCheck();
		}

		private void DrawObjectFields(BehaviorSource behaviorSource, TaskList taskList, Task task, object obj, bool enabled, bool drawWatch)
		{
			if (obj == null)
			{
				return;
			}
			List<Type> baseClasses = FieldInspector.GetBaseClasses(obj.GetType());
			BindingFlags bindingFlags = 54;
			bool flag = this.IsReflectionTask(obj.GetType());
			for (int i = baseClasses.get_Count() - 1; i > -1; i--)
			{
				FieldInfo[] fields = baseClasses.get_Item(i).GetFields(bindingFlags);
				for (int j = 0; j < fields.Length; j++)
				{
					if (!BehaviorDesignerUtility.HasAttribute(fields[j], typeof(NonSerializedAttribute)) && !BehaviorDesignerUtility.HasAttribute(fields[j], typeof(HideInInspector)) && ((!fields[j].get_IsPrivate() && !fields[j].get_IsFamily()) || BehaviorDesignerUtility.HasAttribute(fields[j], typeof(SerializeField))) && (!(obj is ParentTask) || !fields[j].get_Name().Equals("children")) && (!flag || (!fields[j].get_FieldType().Equals(typeof(SharedVariable)) && !fields[j].get_FieldType().IsSubclassOf(typeof(SharedVariable))) || this.CanDrawReflectedField(obj, fields[j])))
					{
						string s = fields[j].get_Name();
						if (flag && (fields[j].get_FieldType().Equals(typeof(SharedVariable)) || fields[j].get_FieldType().IsSubclassOf(typeof(SharedVariable))))
						{
							s = this.InvokeParameterName(obj, fields[j]);
						}
						TooltipAttribute[] array;
						GUIContent guiContent;
						if ((array = (fields[j].GetCustomAttributes(typeof(TooltipAttribute), false) as TooltipAttribute[])).Length > 0)
						{
							guiContent = new GUIContent(BehaviorDesignerUtility.SplitCamelCase(s), array[0].get_Tooltip());
						}
						else
						{
							guiContent = new GUIContent(BehaviorDesignerUtility.SplitCamelCase(s));
						}
						object value = fields[j].GetValue(obj);
						Type fieldType = fields[j].get_FieldType();
						if (typeof(Task).IsAssignableFrom(fieldType) || (typeof(IList).IsAssignableFrom(fieldType) && (typeof(Task).IsAssignableFrom(fieldType.GetElementType()) || (fieldType.get_IsGenericType() && typeof(Task).IsAssignableFrom(fieldType.GetGenericArguments()[0])))))
						{
							EditorGUI.BeginChangeCheck();
							this.DrawTaskValue(behaviorSource, taskList, fields[j], guiContent, task, value as Task, enabled);
							if (BehaviorDesignerWindow.instance.ContainsError(task, fields[j].get_Name()))
							{
								GUILayout.Space(-3f);
								GUILayout.Box(BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, new GUILayoutOption[]
								{
									GUILayout.Width(20f)
								});
							}
							if (EditorGUI.EndChangeCheck())
							{
								GUI.set_changed(true);
							}
						}
						else if (fieldType.Equals(typeof(SharedVariable)) || fieldType.IsSubclassOf(typeof(SharedVariable)))
						{
							GUILayout.BeginHorizontal(new GUILayoutOption[0]);
							EditorGUI.BeginChangeCheck();
							if (drawWatch)
							{
								this.DrawWatchedButton(task, fields[j]);
							}
							SharedVariable sharedVariable = this.DrawSharedVariableValue(behaviorSource, fields[j], guiContent, task, value as SharedVariable, flag, enabled, drawWatch);
							if (BehaviorDesignerWindow.instance.ContainsError(task, fields[j].get_Name()))
							{
								GUILayout.Space(-3f);
								GUILayout.Box(BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, new GUILayoutOption[]
								{
									GUILayout.Width(20f)
								});
							}
							GUILayout.EndHorizontal();
							GUILayout.Space(4f);
							if (EditorGUI.EndChangeCheck())
							{
								fields[j].SetValue(obj, sharedVariable);
								GUI.set_changed(true);
							}
						}
						else
						{
							GUILayout.BeginHorizontal(new GUILayoutOption[0]);
							EditorGUI.BeginChangeCheck();
							if (drawWatch)
							{
								this.DrawWatchedButton(task, fields[j]);
							}
							object obj2 = FieldInspector.DrawField(task, guiContent, fields[j], value);
							if (BehaviorDesignerWindow.instance.ContainsError(task, fields[j].get_Name()))
							{
								GUILayout.Space(-3f);
								GUILayout.Box(BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, new GUILayoutOption[]
								{
									GUILayout.Width(20f)
								});
							}
							if (EditorGUI.EndChangeCheck())
							{
								fields[j].SetValue(obj, obj2);
								GUI.set_changed(true);
							}
							if (TaskUtility.HasAttribute(fields[j], typeof(RequiredFieldAttribute)) && !ErrorCheck.IsRequiredFieldValid(fieldType, value))
							{
								GUILayout.Space(-3f);
								GUILayout.Box(BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, new GUILayoutOption[]
								{
									GUILayout.Width(20f)
								});
							}
							GUILayout.EndHorizontal();
							GUILayout.Space(4f);
						}
					}
				}
			}
		}

		private bool DrawWatchedButton(Task task, FieldInfo field)
		{
			GUILayout.Space(3f);
			bool flag = task.get_NodeData().ContainsWatchedField(field);
			if (GUILayout.Button((!flag) ? BehaviorDesignerUtility.VariableWatchButtonTexture : BehaviorDesignerUtility.VariableWatchButtonSelectedTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Width(15f)
			}))
			{
				if (flag)
				{
					task.get_NodeData().RemoveWatchedField(field);
				}
				else
				{
					task.get_NodeData().AddWatchedField(field);
				}
				return true;
			}
			return false;
		}

		private void DrawTaskValue(BehaviorSource behaviorSource, TaskList taskList, FieldInfo field, GUIContent guiContent, Task parentTask, Task task, bool enabled)
		{
			if (BehaviorDesignerUtility.HasAttribute(field, typeof(InspectTaskAttribute)))
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				GUILayout.Label(guiContent, new GUILayoutOption[]
				{
					GUILayout.Width(144f)
				});
				if (GUILayout.Button((task == null) ? "Select" : BehaviorDesignerUtility.SplitCamelCase(task.GetType().get_Name().ToString()), EditorStyles.get_toolbarPopup(), new GUILayoutOption[]
				{
					GUILayout.Width(134f)
				}))
				{
					GenericMenu genericMenu = new GenericMenu();
					genericMenu.AddItem(new GUIContent("None"), task == null, new GenericMenu.MenuFunction2(this.InspectedTaskCallback), null);
					taskList.AddConditionalTasksToMenu(ref genericMenu, (task == null) ? null : task.GetType(), string.Empty, new GenericMenu.MenuFunction2(this.InspectedTaskCallback));
					genericMenu.ShowAsContext();
					this.mActiveMenuSelectionTask = parentTask;
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(2f);
				this.DrawObjectFields(behaviorSource, taskList, task, task, enabled, false);
			}
			else
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				this.DrawWatchedButton(parentTask, field);
				GUILayout.Label(guiContent, BehaviorDesignerUtility.TaskInspectorGUIStyle, new GUILayoutOption[]
				{
					GUILayout.Width(165f)
				});
				bool flag = this.behaviorDesignerWindow.IsReferencingField(field);
				Color backgroundColor = GUI.get_backgroundColor();
				if (flag)
				{
					GUI.set_backgroundColor(new Color(0.5f, 1f, 0.5f));
				}
				if (GUILayout.Button((!flag) ? "Select" : "Done", EditorStyles.get_miniButtonMid(), new GUILayoutOption[]
				{
					GUILayout.Width(80f)
				}))
				{
					if (this.behaviorDesignerWindow.IsReferencingTasks() && !flag)
					{
						this.behaviorDesignerWindow.ToggleReferenceTasks();
					}
					this.behaviorDesignerWindow.ToggleReferenceTasks(parentTask, field);
				}
				GUI.set_backgroundColor(backgroundColor);
				EditorGUILayout.EndHorizontal();
				if (typeof(IList).IsAssignableFrom(field.get_FieldType()))
				{
					IList list = field.GetValue(parentTask) as IList;
					if (list == null || list.get_Count() == 0)
					{
						GUILayout.Label("No Tasks Referenced", BehaviorDesignerUtility.TaskInspectorGUIStyle, new GUILayoutOption[0]);
					}
					else
					{
						for (int i = 0; i < list.get_Count(); i++)
						{
							if (list.get_Item(i) is Task)
							{
								EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
								GUILayout.Label((list.get_Item(i) as Task).get_NodeData().get_NodeDesigner().ToString(), BehaviorDesignerUtility.TaskInspectorGUIStyle, new GUILayoutOption[]
								{
									GUILayout.Width(232f)
								});
								if (GUILayout.Button(BehaviorDesignerUtility.DeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
								{
									GUILayout.Width(14f)
								}))
								{
									this.ReferenceTasks(parentTask, ((list.get_Item(i) as Task).get_NodeData().get_NodeDesigner() as NodeDesigner).Task, field);
									GUI.set_changed(true);
								}
								GUILayout.Space(3f);
								if (GUILayout.Button(BehaviorDesignerUtility.IdentifyButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
								{
									GUILayout.Width(14f)
								}))
								{
									this.behaviorDesignerWindow.IdentifyNode((list.get_Item(i) as Task).get_NodeData().get_NodeDesigner() as NodeDesigner);
								}
								EditorGUILayout.EndHorizontal();
							}
						}
					}
				}
				else
				{
					EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
					Task task2 = field.GetValue(parentTask) as Task;
					GUILayout.Label((task2 == null) ? "No Tasks Referenced" : task2.get_NodeData().get_NodeDesigner().ToString(), BehaviorDesignerUtility.TaskInspectorGUIStyle, new GUILayoutOption[]
					{
						GUILayout.Width(232f)
					});
					if (task2 != null)
					{
						if (GUILayout.Button(BehaviorDesignerUtility.DeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
						{
							GUILayout.Width(14f)
						}))
						{
							this.ReferenceTasks(parentTask, null, field);
							GUI.set_changed(true);
						}
						GUILayout.Space(3f);
						if (GUILayout.Button(BehaviorDesignerUtility.IdentifyButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
						{
							GUILayout.Width(14f)
						}))
						{
							this.behaviorDesignerWindow.IdentifyNode(task2.get_NodeData().get_NodeDesigner() as NodeDesigner);
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
		}

		private SharedVariable DrawSharedVariableValue(BehaviorSource behaviorSource, FieldInfo field, GUIContent guiContent, Task task, SharedVariable sharedVariable, bool isReflectionTask, bool enabled, bool drawWatch)
		{
			if (isReflectionTask)
			{
				if (!field.get_FieldType().Equals(typeof(SharedVariable)) && sharedVariable == null)
				{
					sharedVariable = (Activator.CreateInstance(field.get_FieldType()) as SharedVariable);
					if (TaskUtility.HasAttribute(field, typeof(RequiredFieldAttribute)) || TaskUtility.HasAttribute(field, typeof(SharedRequiredAttribute)))
					{
						sharedVariable.set_IsShared(true);
					}
					GUI.set_changed(true);
				}
				bool drawComponentField;
				if (sharedVariable.get_IsShared())
				{
					GUILayout.Label(guiContent, new GUILayoutOption[]
					{
						GUILayout.Width(126f)
					});
					string[] array = null;
					int num = -1;
					int num2 = FieldInspector.GetVariablesOfType(sharedVariable.GetType().GetProperty("Value").get_PropertyType(), sharedVariable.get_IsGlobal(), sharedVariable.get_Name(), behaviorSource, out array, ref num, false);
					Color backgroundColor = GUI.get_backgroundColor();
					if (num2 == 0 && !TaskUtility.HasAttribute(field, typeof(SharedRequiredAttribute)))
					{
						GUI.set_backgroundColor(Color.get_red());
					}
					int num3 = num2;
					num2 = EditorGUILayout.Popup(num2, array, EditorStyles.get_toolbarPopup(), new GUILayoutOption[0]);
					GUI.set_backgroundColor(backgroundColor);
					if (num2 != num3)
					{
						if (num2 == 0)
						{
							if (field.get_FieldType().Equals(typeof(SharedVariable)))
							{
								sharedVariable = (Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(sharedVariable.GetType().GetProperty("Value").get_PropertyType())) as SharedVariable);
							}
							else
							{
								sharedVariable = (Activator.CreateInstance(field.get_FieldType()) as SharedVariable);
							}
							sharedVariable.set_IsShared(true);
						}
						else if (num != -1 && num2 >= num)
						{
							sharedVariable = GlobalVariables.get_Instance().GetVariable(array[num2].Substring(8, array[num2].get_Length() - 8));
						}
						else
						{
							sharedVariable = behaviorSource.GetVariable(array[num2]);
						}
					}
					GUILayout.Space(8f);
				}
				else if ((drawComponentField = field.get_Name().Equals("componentName")) || field.get_Name().Equals("methodName") || field.get_Name().Equals("fieldName") || field.get_Name().Equals("propertyName"))
				{
					this.DrawReflectionField(task, guiContent, drawComponentField, field);
				}
				else
				{
					FieldInspector.DrawFields(task, sharedVariable, guiContent);
				}
				if (!TaskUtility.HasAttribute(field, typeof(RequiredFieldAttribute)) && !TaskUtility.HasAttribute(field, typeof(SharedRequiredAttribute)))
				{
					sharedVariable = FieldInspector.DrawSharedVariableToggleSharedButton(sharedVariable);
				}
				else if (!sharedVariable.get_IsShared())
				{
					sharedVariable.set_IsShared(true);
				}
			}
			else
			{
				sharedVariable = FieldInspector.DrawSharedVariable(null, guiContent, field, field.get_FieldType(), sharedVariable);
			}
			GUILayout.Space(8f);
			return sharedVariable;
		}

		private void InspectedTaskCallback(object obj)
		{
			if (this.mActiveMenuSelectionTask != null)
			{
				FieldInfo field = this.mActiveMenuSelectionTask.GetType().GetField("conditionalTask");
				if (obj == null)
				{
					field.SetValue(this.mActiveMenuSelectionTask, null);
				}
				else
				{
					Type type = (Type)obj;
					Task task = Activator.CreateInstance(type, true) as Task;
					field.SetValue(this.mActiveMenuSelectionTask, task);
					FieldInfo[] allFields = TaskUtility.GetAllFields(type);
					for (int i = 0; i < allFields.Length; i++)
					{
						if (allFields[i].get_FieldType().IsSubclassOf(typeof(SharedVariable)) && !BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(HideInInspector)) && !BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(NonSerializedAttribute)) && ((!allFields[i].get_IsPrivate() && !allFields[i].get_IsFamily()) || BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(SerializeField))))
						{
							SharedVariable sharedVariable = Activator.CreateInstance(allFields[i].get_FieldType()) as SharedVariable;
							sharedVariable.set_IsShared(false);
							allFields[i].SetValue(task, sharedVariable);
						}
					}
				}
			}
			BehaviorDesignerWindow.instance.SaveBehavior();
		}

		public void SetActiveReferencedTasks(Task referenceTask, FieldInfo fieldInfo)
		{
			this.activeReferenceTask = referenceTask;
			this.activeReferenceTaskFieldInfo = fieldInfo;
		}

		public bool ReferenceTasks(Task referenceTask)
		{
			return this.ReferenceTasks(this.activeReferenceTask, referenceTask, this.activeReferenceTaskFieldInfo);
		}

		private bool ReferenceTasks(Task sourceTask, Task referenceTask, FieldInfo sourceFieldInfo)
		{
			bool flag = false;
			bool showReferenceIcon = false;
			if (TaskInspector.ReferenceTasks(sourceTask, referenceTask, sourceFieldInfo, ref flag, ref showReferenceIcon, true, false))
			{
				if (referenceTask != null)
				{
					(referenceTask.get_NodeData().get_NodeDesigner() as NodeDesigner).ShowReferenceIcon = showReferenceIcon;
					if (flag)
					{
						this.PerformFullSync(this.activeReferenceTask);
					}
				}
				return true;
			}
			return false;
		}

		public static bool ReferenceTasks(Task sourceTask, Task referenceTask, FieldInfo sourceFieldInfo, ref bool fullSync, ref bool doReference, bool synchronize, bool unreferenceAll)
		{
			if (referenceTask == null)
			{
				Task task = sourceFieldInfo.GetValue(sourceTask) as Task;
				if (task != null)
				{
					(task.get_NodeData().get_NodeDesigner() as NodeDesigner).ShowReferenceIcon = false;
				}
				sourceFieldInfo.SetValue(sourceTask, null);
				return true;
			}
			if (referenceTask.Equals(sourceTask) || sourceFieldInfo == null || (!typeof(IList).IsAssignableFrom(sourceFieldInfo.get_FieldType()) && !sourceFieldInfo.get_FieldType().IsAssignableFrom(referenceTask.GetType())) || (typeof(IList).IsAssignableFrom(sourceFieldInfo.get_FieldType()) && ((sourceFieldInfo.get_FieldType().get_IsGenericType() && !sourceFieldInfo.get_FieldType().GetGenericArguments()[0].IsAssignableFrom(referenceTask.GetType())) || (!sourceFieldInfo.get_FieldType().get_IsGenericType() && !sourceFieldInfo.get_FieldType().GetElementType().IsAssignableFrom(referenceTask.GetType())))))
			{
				return false;
			}
			if (synchronize && !TaskInspector.IsFieldLinked(sourceFieldInfo))
			{
				synchronize = false;
			}
			if (unreferenceAll)
			{
				sourceFieldInfo.SetValue(sourceTask, null);
				(sourceTask.get_NodeData().get_NodeDesigner() as NodeDesigner).ShowReferenceIcon = false;
			}
			else
			{
				doReference = true;
				bool flag = false;
				if (typeof(IList).IsAssignableFrom(sourceFieldInfo.get_FieldType()))
				{
					Task[] array = sourceFieldInfo.GetValue(sourceTask) as Task[];
					Type type;
					if (sourceFieldInfo.get_FieldType().get_IsArray())
					{
						type = sourceFieldInfo.get_FieldType().GetElementType();
					}
					else
					{
						Type type2 = sourceFieldInfo.get_FieldType();
						while (!type2.get_IsGenericType())
						{
							type2 = type2.get_BaseType();
						}
						type = type2.GetGenericArguments()[0];
					}
					IList list = Activator.CreateInstance(typeof(List).MakeGenericType(new Type[]
					{
						type
					})) as IList;
					if (array != null)
					{
						for (int i = 0; i < array.Length; i++)
						{
							if (referenceTask.Equals(array[i]))
							{
								doReference = false;
							}
							else
							{
								list.Add(array[i]);
							}
						}
					}
					if (synchronize)
					{
						if (array != null && array.Length > 0)
						{
							for (int j = 0; j < array.Length; j++)
							{
								TaskInspector.ReferenceTasks(array[j], referenceTask, array[j].GetType().GetField(sourceFieldInfo.get_Name()), ref flag, ref doReference, false, false);
								if (doReference)
								{
									TaskInspector.ReferenceTasks(referenceTask, array[j], referenceTask.GetType().GetField(sourceFieldInfo.get_Name()), ref flag, ref doReference, false, false);
								}
							}
						}
						else if (doReference)
						{
							FieldInfo field = referenceTask.GetType().GetField(sourceFieldInfo.get_Name());
							if (field != null)
							{
								array = (field.GetValue(referenceTask) as Task[]);
								if (array != null)
								{
									for (int k = 0; k < array.Length; k++)
									{
										list.Add(array[k]);
										(array[k].get_NodeData().get_NodeDesigner() as NodeDesigner).ShowReferenceIcon = true;
										TaskInspector.ReferenceTasks(array[k], sourceTask, array[k].GetType().GetField(sourceFieldInfo.get_Name()), ref doReference, ref flag, false, false);
									}
									doReference = true;
								}
							}
						}
						TaskInspector.ReferenceTasks(referenceTask, sourceTask, referenceTask.GetType().GetField(sourceFieldInfo.get_Name()), ref flag, ref doReference, false, !doReference);
					}
					if (doReference)
					{
						list.Add(referenceTask);
					}
					if (sourceFieldInfo.get_FieldType().get_IsArray())
					{
						Array array2 = Array.CreateInstance(sourceFieldInfo.get_FieldType().GetElementType(), list.get_Count());
						list.CopyTo(array2, 0);
						sourceFieldInfo.SetValue(sourceTask, array2);
					}
					else
					{
						sourceFieldInfo.SetValue(sourceTask, list);
					}
				}
				else
				{
					Task task2 = sourceFieldInfo.GetValue(sourceTask) as Task;
					doReference = !referenceTask.Equals(task2);
					if (TaskInspector.IsFieldLinked(sourceFieldInfo) && task2 != null)
					{
						TaskInspector.ReferenceTasks(task2, sourceTask, task2.GetType().GetField(sourceFieldInfo.get_Name()), ref flag, ref doReference, false, true);
					}
					if (synchronize)
					{
						TaskInspector.ReferenceTasks(referenceTask, sourceTask, referenceTask.GetType().GetField(sourceFieldInfo.get_Name()), ref flag, ref doReference, false, !doReference);
					}
					sourceFieldInfo.SetValue(sourceTask, (!doReference) ? null : referenceTask);
				}
				if (synchronize)
				{
					(referenceTask.get_NodeData().get_NodeDesigner() as NodeDesigner).ShowReferenceIcon = doReference;
				}
				fullSync = (doReference && synchronize);
			}
			return true;
		}

		public bool IsActiveTaskArray()
		{
			return this.activeReferenceTaskFieldInfo.get_FieldType().get_IsArray();
		}

		public bool IsActiveTaskNull()
		{
			return this.activeReferenceTaskFieldInfo.GetValue(this.activeReferenceTask) == null;
		}

		public static bool IsFieldLinked(FieldInfo field)
		{
			return BehaviorDesignerUtility.HasAttribute(field, typeof(LinkedTaskAttribute));
		}

		public static List<Task> GetReferencedTasks(Task task)
		{
			List<Task> list = new List<Task>();
			FieldInfo[] allFields = TaskUtility.GetAllFields(task.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				if ((!allFields[i].get_IsPrivate() && !allFields[i].get_IsFamily()) || BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(SerializeField)))
				{
					if (typeof(IList).IsAssignableFrom(allFields[i].get_FieldType()) && (typeof(Task).IsAssignableFrom(allFields[i].get_FieldType().GetElementType()) || (allFields[i].get_FieldType().get_IsGenericType() && typeof(Task).IsAssignableFrom(allFields[i].get_FieldType().GetGenericArguments()[0]))))
					{
						Task[] array = allFields[i].GetValue(task) as Task[];
						if (array != null)
						{
							for (int j = 0; j < array.Length; j++)
							{
								list.Add(array[j]);
							}
						}
					}
					else if (allFields[i].get_FieldType().IsSubclassOf(typeof(Task)) && allFields[i].GetValue(task) != null)
					{
						list.Add(allFields[i].GetValue(task) as Task);
					}
				}
			}
			return (list.get_Count() <= 0) ? null : list;
		}

		private void PerformFullSync(Task task)
		{
			List<Task> referencedTasks = TaskInspector.GetReferencedTasks(task);
			if (referencedTasks != null)
			{
				FieldInfo[] allFields = TaskUtility.GetAllFields(task.GetType());
				for (int i = 0; i < allFields.Length; i++)
				{
					if (!TaskInspector.IsFieldLinked(allFields[i]))
					{
						for (int j = 0; j < referencedTasks.get_Count(); j++)
						{
							FieldInfo field;
							if ((field = referencedTasks.get_Item(j).GetType().GetField(allFields[i].get_Name())) != null)
							{
								field.SetValue(referencedTasks.get_Item(j), allFields[i].GetValue(task));
							}
						}
					}
				}
			}
		}

		public static void OpenInFileEditor(object task)
		{
			MonoScript[] array = (MonoScript[])Resources.FindObjectsOfTypeAll(typeof(MonoScript));
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null && array[i].GetClass() != null && array[i].GetClass().Equals(task.GetType()))
				{
					AssetDatabase.OpenAsset(array[i]);
					break;
				}
			}
		}

		public static void SelectInProject(object task)
		{
			MonoScript[] array = (MonoScript[])Resources.FindObjectsOfTypeAll(typeof(MonoScript));
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null && array[i].GetClass() != null && array[i].GetClass().Equals(task.GetType()))
				{
					Selection.set_activeObject(array[i]);
					break;
				}
			}
		}

		private void ResetTask(object task)
		{
			(task as Task).OnReset();
			List<Type> baseClasses = FieldInspector.GetBaseClasses(task.GetType());
			BindingFlags bindingFlags = 54;
			for (int i = baseClasses.get_Count() - 1; i > -1; i--)
			{
				FieldInfo[] fields = baseClasses.get_Item(i).GetFields(bindingFlags);
				for (int j = 0; j < fields.Length; j++)
				{
					if (typeof(SharedVariable).IsAssignableFrom(fields[j].get_FieldType()))
					{
						SharedVariable sharedVariable = fields[j].GetValue(task) as SharedVariable;
						if (TaskUtility.HasAttribute(fields[j], typeof(RequiredFieldAttribute)) && sharedVariable != null && !sharedVariable.get_IsShared())
						{
							sharedVariable.set_IsShared(true);
						}
					}
				}
			}
		}

		private void AddColorMenuItem(ref GenericMenu menu, Task task, string color, int index)
		{
			menu.AddItem(new GUIContent(color), task.get_NodeData().get_ColorIndex() == index, new GenericMenu.MenuFunction2(this.SetTaskColor), new TaskInspector.TaskColor(task, index));
		}

		private void SetTaskColor(object value)
		{
			TaskInspector.TaskColor taskColor = value as TaskInspector.TaskColor;
			if (taskColor.task.get_NodeData().get_ColorIndex() != taskColor.colorIndex)
			{
				taskColor.task.get_NodeData().set_ColorIndex(taskColor.colorIndex);
				BehaviorDesignerWindow.instance.SaveBehavior();
			}
		}

		private void OpenHelpURL(Task task)
		{
			HelpURLAttribute[] array;
			if ((array = (task.GetType().GetCustomAttributes(typeof(HelpURLAttribute), false) as HelpURLAttribute[])).Length > 0)
			{
				Application.OpenURL(array[0].get_URL());
			}
		}

		private bool IsReflectionTask(Type type)
		{
			return this.IsInvokeMethodTask(type) || this.IsFieldReflectionTask(type) || this.IsPropertyReflectionTask(type);
		}

		private bool IsInvokeMethodTask(Type type)
		{
			return TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.InvokeMethod");
		}

		private bool IsFieldReflectionTask(Type type)
		{
			return TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetFieldValue") || TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.SetFieldValue") || TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.CompareFieldValue");
		}

		private bool IsPropertyReflectionTask(Type type)
		{
			return TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetPropertyValue") || TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.SetPropertyValue") || TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.ComparePropertyValue");
		}

		private bool IsReflectionGetterTask(Type type)
		{
			return TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetFieldValue") || TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetPropertyValue");
		}

		private void DrawReflectionField(Task task, GUIContent guiContent, bool drawComponentField, FieldInfo field)
		{
			FieldInfo field2 = task.GetType().GetField("targetGameObject");
			SharedVariable sharedVariable = field2.GetValue(task) as SharedVariable;
			if (drawComponentField)
			{
				GUILayout.Label(guiContent, new GUILayoutOption[]
				{
					GUILayout.Width(146f)
				});
				SharedVariable sharedVariable2 = field.GetValue(task) as SharedVariable;
				string text = string.Empty;
				if (string.IsNullOrEmpty((string)sharedVariable2.GetValue()))
				{
					text = "Select";
				}
				else
				{
					string text2 = (string)sharedVariable2.GetValue();
					string[] array = text2.Split(new char[]
					{
						'.'
					});
					text = array[array.Length - 1];
				}
				if (GUILayout.Button(text, EditorStyles.get_toolbarPopup(), new GUILayoutOption[]
				{
					GUILayout.Width(92f)
				}))
				{
					GenericMenu genericMenu = new GenericMenu();
					genericMenu.AddItem(new GUIContent("None"), string.IsNullOrEmpty((string)sharedVariable2.GetValue()), new GenericMenu.MenuFunction2(this.ComponentSelectionCallback), null);
					GameObject gameObject = null;
					if (sharedVariable == null || (GameObject)sharedVariable.GetValue() == null)
					{
						if (task.get_Owner() != null)
						{
							gameObject = task.get_Owner().get_gameObject();
						}
					}
					else
					{
						gameObject = (GameObject)sharedVariable.GetValue();
					}
					if (gameObject != null)
					{
						Component[] components = gameObject.GetComponents<Component>();
						for (int i = 0; i < components.Length; i++)
						{
							genericMenu.AddItem(new GUIContent(components[i].GetType().get_Name()), components[i].GetType().get_FullName().Equals((string)sharedVariable2.GetValue()), new GenericMenu.MenuFunction2(this.ComponentSelectionCallback), components[i].GetType().get_FullName());
						}
						genericMenu.ShowAsContext();
						this.mActiveMenuSelectionTask = task;
					}
				}
			}
			else
			{
				GUILayout.Label(guiContent, new GUILayoutOption[]
				{
					GUILayout.Width(146f)
				});
				FieldInfo field3 = task.GetType().GetField("componentName");
				SharedVariable sharedVariable3 = field3.GetValue(task) as SharedVariable;
				SharedVariable sharedVariable4 = field.GetValue(task) as SharedVariable;
				string text3 = string.Empty;
				if (string.IsNullOrEmpty((string)sharedVariable3.GetValue()))
				{
					text3 = "Component Required";
				}
				else if (string.IsNullOrEmpty((string)sharedVariable4.GetValue()))
				{
					text3 = "Select";
				}
				else
				{
					text3 = (string)sharedVariable4.GetValue();
				}
				if (GUILayout.Button(text3, EditorStyles.get_toolbarPopup(), new GUILayoutOption[]
				{
					GUILayout.Width(92f)
				}) && !string.IsNullOrEmpty((string)sharedVariable3.GetValue()))
				{
					GenericMenu genericMenu2 = new GenericMenu();
					genericMenu2.AddItem(new GUIContent("None"), string.IsNullOrEmpty((string)sharedVariable4.GetValue()), new GenericMenu.MenuFunction2(this.SecondaryReflectionSelectionCallback), null);
					GameObject gameObject2 = null;
					if (sharedVariable == null || (GameObject)sharedVariable.GetValue() == null)
					{
						if (task.get_Owner() != null)
						{
							gameObject2 = task.get_Owner().get_gameObject();
						}
					}
					else
					{
						gameObject2 = (GameObject)sharedVariable.GetValue();
					}
					if (gameObject2 != null)
					{
						Component component = gameObject2.GetComponent(TaskUtility.GetTypeWithinAssembly((string)sharedVariable3.GetValue()));
						List<Type> sharedVariableTypes = VariableInspector.FindAllSharedVariableTypes(false);
						if (this.IsInvokeMethodTask(task.GetType()))
						{
							MethodInfo[] methods = component.GetType().GetMethods(20);
							for (int j = 0; j < methods.Length; j++)
							{
								if (!methods[j].get_IsSpecialName() && !methods[j].get_IsGenericMethod() && methods[j].GetParameters().Length <= 4)
								{
									ParameterInfo[] parameters = methods[j].GetParameters();
									bool flag = true;
									for (int k = 0; k < parameters.Length; k++)
									{
										if (!this.SharedVariableTypeExists(sharedVariableTypes, parameters[k].get_ParameterType()))
										{
											flag = false;
											break;
										}
									}
									if (flag && (methods[j].get_ReturnType().Equals(typeof(void)) || this.SharedVariableTypeExists(sharedVariableTypes, methods[j].get_ReturnType())))
									{
										genericMenu2.AddItem(new GUIContent(methods[j].get_Name()), methods[j].get_Name().Equals((string)sharedVariable4.GetValue()), new GenericMenu.MenuFunction2(this.SecondaryReflectionSelectionCallback), methods[j]);
									}
								}
							}
						}
						else if (this.IsFieldReflectionTask(task.GetType()))
						{
							FieldInfo[] fields = component.GetType().GetFields(20);
							for (int l = 0; l < fields.Length; l++)
							{
								if (!fields[l].get_IsSpecialName())
								{
									if (this.SharedVariableTypeExists(sharedVariableTypes, fields[l].get_FieldType()))
									{
										genericMenu2.AddItem(new GUIContent(fields[l].get_Name()), fields[l].get_Name().Equals((string)sharedVariable4.GetValue()), new GenericMenu.MenuFunction2(this.SecondaryReflectionSelectionCallback), fields[l]);
									}
								}
							}
						}
						else
						{
							PropertyInfo[] properties = component.GetType().GetProperties(20);
							for (int m = 0; m < properties.Length; m++)
							{
								if (!properties[m].get_IsSpecialName())
								{
									if (this.SharedVariableTypeExists(sharedVariableTypes, properties[m].get_PropertyType()))
									{
										genericMenu2.AddItem(new GUIContent(properties[m].get_Name()), properties[m].get_Name().Equals((string)sharedVariable4.GetValue()), new GenericMenu.MenuFunction2(this.SecondaryReflectionSelectionCallback), properties[m]);
									}
								}
							}
						}
						genericMenu2.ShowAsContext();
						this.mActiveMenuSelectionTask = task;
					}
				}
			}
			GUILayout.Space(8f);
		}

		private void ComponentSelectionCallback(object obj)
		{
			if (this.mActiveMenuSelectionTask != null)
			{
				FieldInfo field = this.mActiveMenuSelectionTask.GetType().GetField("componentName");
				SharedVariable sharedVariable = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as SharedVariable;
				if (obj == null)
				{
					field.SetValue(this.mActiveMenuSelectionTask, sharedVariable);
					sharedVariable = (Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as SharedVariable);
					FieldInfo field2;
					if (this.IsInvokeMethodTask(this.mActiveMenuSelectionTask.GetType()))
					{
						field2 = this.mActiveMenuSelectionTask.GetType().GetField("methodName");
						this.ClearInvokeVariablesTask();
					}
					else if (this.IsFieldReflectionTask(this.mActiveMenuSelectionTask.GetType()))
					{
						field2 = this.mActiveMenuSelectionTask.GetType().GetField("fieldName");
					}
					else
					{
						field2 = this.mActiveMenuSelectionTask.GetType().GetField("propertyName");
					}
					field2.SetValue(this.mActiveMenuSelectionTask, sharedVariable);
				}
				else
				{
					string text = (string)obj;
					SharedVariable sharedVariable2 = field.GetValue(this.mActiveMenuSelectionTask) as SharedVariable;
					if (!text.Equals((string)sharedVariable2.GetValue()))
					{
						FieldInfo field3;
						FieldInfo field5;
						if (this.IsInvokeMethodTask(this.mActiveMenuSelectionTask.GetType()))
						{
							field3 = this.mActiveMenuSelectionTask.GetType().GetField("methodName");
							for (int i = 0; i < 4; i++)
							{
								FieldInfo field4 = this.mActiveMenuSelectionTask.GetType().GetField("parameter" + (i + 1));
								field4.SetValue(this.mActiveMenuSelectionTask, null);
							}
							field5 = this.mActiveMenuSelectionTask.GetType().GetField("storeResult");
						}
						else if (this.IsFieldReflectionTask(this.mActiveMenuSelectionTask.GetType()))
						{
							field3 = this.mActiveMenuSelectionTask.GetType().GetField("fieldName");
							field5 = this.mActiveMenuSelectionTask.GetType().GetField("fieldValue");
							if (field5 == null)
							{
								field5 = this.mActiveMenuSelectionTask.GetType().GetField("compareValue");
							}
						}
						else
						{
							field3 = this.mActiveMenuSelectionTask.GetType().GetField("propertyName");
							field5 = this.mActiveMenuSelectionTask.GetType().GetField("propertyValue");
							if (field5 == null)
							{
								field5 = this.mActiveMenuSelectionTask.GetType().GetField("compareValue");
							}
						}
						field3.SetValue(this.mActiveMenuSelectionTask, sharedVariable);
						field5.SetValue(this.mActiveMenuSelectionTask, null);
					}
					sharedVariable = (Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as SharedVariable);
					sharedVariable.SetValue(text);
					field.SetValue(this.mActiveMenuSelectionTask, sharedVariable);
				}
			}
			BehaviorDesignerWindow.instance.SaveBehavior();
		}

		private void SecondaryReflectionSelectionCallback(object obj)
		{
			if (this.mActiveMenuSelectionTask != null)
			{
				SharedVariable sharedVariable = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as SharedVariable;
				FieldInfo field;
				if (this.IsInvokeMethodTask(this.mActiveMenuSelectionTask.GetType()))
				{
					this.ClearInvokeVariablesTask();
					field = this.mActiveMenuSelectionTask.GetType().GetField("methodName");
				}
				else if (this.IsFieldReflectionTask(this.mActiveMenuSelectionTask.GetType()))
				{
					field = this.mActiveMenuSelectionTask.GetType().GetField("fieldName");
				}
				else
				{
					field = this.mActiveMenuSelectionTask.GetType().GetField("propertyName");
				}
				if (obj == null)
				{
					field.SetValue(this.mActiveMenuSelectionTask, sharedVariable);
				}
				else if (this.IsInvokeMethodTask(this.mActiveMenuSelectionTask.GetType()))
				{
					MethodInfo methodInfo = (MethodInfo)obj;
					sharedVariable.SetValue(methodInfo.get_Name());
					field.SetValue(this.mActiveMenuSelectionTask, sharedVariable);
					ParameterInfo[] parameters = methodInfo.GetParameters();
					for (int i = 0; i < 4; i++)
					{
						FieldInfo field2 = this.mActiveMenuSelectionTask.GetType().GetField("parameter" + (i + 1));
						if (i < parameters.Length)
						{
							sharedVariable = (Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(parameters[i].get_ParameterType())) as SharedVariable);
							field2.SetValue(this.mActiveMenuSelectionTask, sharedVariable);
						}
						else
						{
							field2.SetValue(this.mActiveMenuSelectionTask, null);
						}
					}
					if (!methodInfo.get_ReturnType().Equals(typeof(void)))
					{
						FieldInfo field3 = this.mActiveMenuSelectionTask.GetType().GetField("storeResult");
						sharedVariable = (Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(methodInfo.get_ReturnType())) as SharedVariable);
						sharedVariable.set_IsShared(true);
						field3.SetValue(this.mActiveMenuSelectionTask, sharedVariable);
					}
				}
				else if (this.IsFieldReflectionTask(this.mActiveMenuSelectionTask.GetType()))
				{
					FieldInfo fieldInfo = (FieldInfo)obj;
					sharedVariable.SetValue(fieldInfo.get_Name());
					field.SetValue(this.mActiveMenuSelectionTask, sharedVariable);
					FieldInfo field4 = this.mActiveMenuSelectionTask.GetType().GetField("fieldValue");
					if (field4 == null)
					{
						field4 = this.mActiveMenuSelectionTask.GetType().GetField("compareValue");
					}
					sharedVariable = (Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(fieldInfo.get_FieldType())) as SharedVariable);
					sharedVariable.set_IsShared(this.IsReflectionGetterTask(this.mActiveMenuSelectionTask.GetType()));
					field4.SetValue(this.mActiveMenuSelectionTask, sharedVariable);
				}
				else
				{
					PropertyInfo propertyInfo = (PropertyInfo)obj;
					sharedVariable.SetValue(propertyInfo.get_Name());
					field.SetValue(this.mActiveMenuSelectionTask, sharedVariable);
					FieldInfo field5 = this.mActiveMenuSelectionTask.GetType().GetField("propertyValue");
					if (field5 == null)
					{
						field5 = this.mActiveMenuSelectionTask.GetType().GetField("compareValue");
					}
					sharedVariable = (Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(propertyInfo.get_PropertyType())) as SharedVariable);
					sharedVariable.set_IsShared(this.IsReflectionGetterTask(this.mActiveMenuSelectionTask.GetType()));
					field5.SetValue(this.mActiveMenuSelectionTask, sharedVariable);
				}
			}
			BehaviorDesignerWindow.instance.SaveBehavior();
		}

		private void ClearInvokeVariablesTask()
		{
			for (int i = 0; i < 4; i++)
			{
				FieldInfo field = this.mActiveMenuSelectionTask.GetType().GetField("parameter" + (i + 1));
				field.SetValue(this.mActiveMenuSelectionTask, null);
			}
			FieldInfo field2 = this.mActiveMenuSelectionTask.GetType().GetField("storeResult");
			field2.SetValue(this.mActiveMenuSelectionTask, null);
		}

		private bool CanDrawReflectedField(object task, FieldInfo field)
		{
			if (!field.get_Name().Contains("parameter") && !field.get_Name().Contains("storeResult") && !field.get_Name().Contains("fieldValue") && !field.get_Name().Contains("propertyValue") && !field.get_Name().Contains("compareValue"))
			{
				return true;
			}
			if (this.IsInvokeMethodTask(task.GetType()))
			{
				if (field.get_Name().Contains("parameter"))
				{
					FieldInfo field2 = task.GetType().GetField(field.get_Name());
					return field2.GetValue(task) != null;
				}
				MethodInfo invokeMethodInfo;
				return (invokeMethodInfo = this.GetInvokeMethodInfo(task)) != null && (!field.get_Name().Equals("storeResult") || !invokeMethodInfo.get_ReturnType().Equals(typeof(void)));
			}
			else
			{
				if (this.IsFieldReflectionTask(task.GetType()))
				{
					FieldInfo field3 = task.GetType().GetField("fieldName");
					SharedVariable sharedVariable = field3.GetValue(task) as SharedVariable;
					return sharedVariable != null && !string.IsNullOrEmpty((string)sharedVariable.GetValue());
				}
				FieldInfo field4 = task.GetType().GetField("propertyName");
				SharedVariable sharedVariable2 = field4.GetValue(task) as SharedVariable;
				return sharedVariable2 != null && !string.IsNullOrEmpty((string)sharedVariable2.GetValue());
			}
		}

		private string InvokeParameterName(object task, FieldInfo field)
		{
			if (!field.get_Name().Contains("parameter"))
			{
				return field.get_Name();
			}
			MethodInfo invokeMethodInfo;
			if ((invokeMethodInfo = this.GetInvokeMethodInfo(task)) == null)
			{
				return field.get_Name();
			}
			ParameterInfo[] parameters = invokeMethodInfo.GetParameters();
			int num = int.Parse(field.get_Name().Substring(9)) - 1;
			if (num < parameters.Length)
			{
				return parameters[num].get_Name();
			}
			return field.get_Name();
		}

		private MethodInfo GetInvokeMethodInfo(object task)
		{
			FieldInfo field = task.GetType().GetField("targetGameObject");
			SharedVariable sharedVariable = field.GetValue(task) as SharedVariable;
			GameObject gameObject = null;
			if (sharedVariable == null || (GameObject)sharedVariable.GetValue() == null)
			{
				if ((task as Task).get_Owner() != null)
				{
					gameObject = (task as Task).get_Owner().get_gameObject();
				}
			}
			else
			{
				gameObject = (GameObject)sharedVariable.GetValue();
			}
			if (gameObject == null)
			{
				return null;
			}
			FieldInfo field2 = task.GetType().GetField("componentName");
			SharedVariable sharedVariable2 = field2.GetValue(task) as SharedVariable;
			if (sharedVariable2 == null || string.IsNullOrEmpty((string)sharedVariable2.GetValue()))
			{
				return null;
			}
			FieldInfo field3 = task.GetType().GetField("methodName");
			SharedVariable sharedVariable3 = field3.GetValue(task) as SharedVariable;
			if (sharedVariable3 == null || string.IsNullOrEmpty((string)sharedVariable3.GetValue()))
			{
				return null;
			}
			List<Type> list = new List<Type>();
			for (int i = 0; i < 4; i++)
			{
				FieldInfo field4 = task.GetType().GetField("parameter" + (i + 1));
				SharedVariable sharedVariable4;
				if ((sharedVariable4 = (field4.GetValue(task) as SharedVariable)) == null)
				{
					break;
				}
				list.Add(sharedVariable4.GetType().GetProperty("Value").get_PropertyType());
			}
			Component component = gameObject.GetComponent(TaskUtility.GetTypeWithinAssembly((string)sharedVariable2.GetValue()));
			if (component == null)
			{
				return null;
			}
			return component.GetType().GetMethod((string)sharedVariable3.GetValue(), list.ToArray());
		}

		private bool SharedVariableTypeExists(List<Type> sharedVariableTypes, Type type)
		{
			for (int i = 0; i < sharedVariableTypes.get_Count(); i++)
			{
				if (FieldInspector.FriendlySharedVariableName(type).Equals(sharedVariableTypes.get_Item(i)))
				{
					return true;
				}
			}
			return false;
		}
	}
}
