using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public class VariableInspector : ScriptableObject
	{
		private class SelectedPropertyMapping
		{
			private string mProperty;

			private GameObject mGameObject;

			public string Property
			{
				get
				{
					return this.mProperty;
				}
			}

			public GameObject GameObject
			{
				get
				{
					return this.mGameObject;
				}
			}

			public SelectedPropertyMapping(string property, GameObject gameObject)
			{
				this.mProperty = property;
				this.mGameObject = gameObject;
			}
		}

		private static string[] sharedVariableStrings;

		private static List<Type> sharedVariableTypes;

		private static Dictionary<string, int> sharedVariableTypesDict;

		private string mVariableName = string.Empty;

		private int mVariableTypeIndex;

		private Vector2 mScrollPosition = Vector2.get_zero();

		private bool mFocusNameField;

		[SerializeField]
		private float mVariableStartPosition = -1f;

		[SerializeField]
		private List<float> mVariablePosition;

		[SerializeField]
		private int mSelectedVariableIndex = -1;

		[SerializeField]
		private string mSelectedVariableName;

		[SerializeField]
		private int mSelectedVariableTypeIndex;

		private static SharedVariable mPropertyMappingVariable;

		private static BehaviorSource mPropertyMappingBehaviorSource;

		private static GenericMenu mPropertyMappingMenu;

		public void ResetSelectedVariableIndex()
		{
			this.mSelectedVariableIndex = -1;
			this.mVariableStartPosition = -1f;
			if (this.mVariablePosition != null)
			{
				this.mVariablePosition.Clear();
			}
		}

		public void OnEnable()
		{
			base.set_hideFlags(61);
		}

		public static List<Type> FindAllSharedVariableTypes(bool removeShared)
		{
			if (VariableInspector.sharedVariableTypes != null)
			{
				return VariableInspector.sharedVariableTypes;
			}
			VariableInspector.sharedVariableTypes = new List<Type>();
			Assembly[] assemblies = AppDomain.get_CurrentDomain().GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Type[] types = assemblies[i].GetTypes();
				for (int j = 0; j < types.Length; j++)
				{
					if (types[j].IsSubclassOf(typeof(SharedVariable)) && !types[j].get_IsAbstract())
					{
						VariableInspector.sharedVariableTypes.Add(types[j]);
					}
				}
			}
			VariableInspector.sharedVariableTypes.Sort(new AlphanumComparator<Type>());
			VariableInspector.sharedVariableStrings = new string[VariableInspector.sharedVariableTypes.get_Count()];
			VariableInspector.sharedVariableTypesDict = new Dictionary<string, int>();
			for (int k = 0; k < VariableInspector.sharedVariableTypes.get_Count(); k++)
			{
				string text = VariableInspector.sharedVariableTypes.get_Item(k).get_Name();
				VariableInspector.sharedVariableTypesDict.Add(text, k);
				if (removeShared && text.get_Length() > 6 && text.Substring(0, 6).Equals("Shared"))
				{
					text = text.Substring(6, text.get_Length() - 6);
				}
				VariableInspector.sharedVariableStrings[k] = text;
			}
			return VariableInspector.sharedVariableTypes;
		}

		public bool ClearFocus(bool addVariable, BehaviorSource behaviorSource)
		{
			GUIUtility.set_keyboardControl(0);
			bool result = false;
			if (addVariable && !string.IsNullOrEmpty(this.mVariableName) && VariableInspector.VariableNameValid(behaviorSource, this.mVariableName))
			{
				result = VariableInspector.AddVariable(behaviorSource, this.mVariableName, this.mVariableTypeIndex, false);
				this.mVariableName = string.Empty;
			}
			return result;
		}

		public bool HasFocus()
		{
			return GUIUtility.get_keyboardControl() != 0;
		}

		public void FocusNameField()
		{
			this.mFocusNameField = true;
		}

		public bool LeftMouseDown(IVariableSource variableSource, BehaviorSource behaviorSource, Vector2 mousePosition)
		{
			return VariableInspector.LeftMouseDown(variableSource, behaviorSource, mousePosition, this.mVariablePosition, this.mVariableStartPosition, this.mScrollPosition, ref this.mSelectedVariableIndex, ref this.mSelectedVariableName, ref this.mSelectedVariableTypeIndex);
		}

		public static bool LeftMouseDown(IVariableSource variableSource, BehaviorSource behaviorSource, Vector2 mousePosition, List<float> variablePosition, float variableStartPosition, Vector2 scrollPosition, ref int selectedVariableIndex, ref string selectedVariableName, ref int selectedVariableTypeIndex)
		{
			if (variablePosition != null && mousePosition.y > variableStartPosition && variableSource != null)
			{
				List<SharedVariable> allVariables;
				if (!Application.get_isPlaying() && behaviorSource != null && behaviorSource.get_Owner() is Behavior)
				{
					Behavior behavior = behaviorSource.get_Owner() as Behavior;
					if (behavior.get_ExternalBehavior() != null)
					{
						BehaviorSource behaviorSource2 = behavior.GetBehaviorSource();
						behaviorSource2.CheckForSerialization(true, null);
						allVariables = behaviorSource2.GetAllVariables();
						ExternalBehavior externalBehavior = behavior.get_ExternalBehavior();
						externalBehavior.get_BehaviorSource().set_Owner(externalBehavior);
						externalBehavior.get_BehaviorSource().CheckForSerialization(true, behaviorSource);
					}
					else
					{
						allVariables = variableSource.GetAllVariables();
					}
				}
				else
				{
					allVariables = variableSource.GetAllVariables();
				}
				if (allVariables == null || allVariables.get_Count() != variablePosition.get_Count())
				{
					return false;
				}
				int i = 0;
				while (i < variablePosition.get_Count())
				{
					if (mousePosition.y < variablePosition.get_Item(i) - scrollPosition.y)
					{
						if (i == selectedVariableIndex)
						{
							return false;
						}
						selectedVariableIndex = i;
						selectedVariableName = allVariables.get_Item(i).get_Name();
						selectedVariableTypeIndex = VariableInspector.sharedVariableTypesDict.get_Item(allVariables.get_Item(i).GetType().get_Name());
						return true;
					}
					else
					{
						i++;
					}
				}
			}
			if (selectedVariableIndex != -1)
			{
				selectedVariableIndex = -1;
				return true;
			}
			return false;
		}

		public bool DrawVariables(BehaviorSource behaviorSource)
		{
			return VariableInspector.DrawVariables(behaviorSource, behaviorSource, ref this.mVariableName, ref this.mFocusNameField, ref this.mVariableTypeIndex, ref this.mScrollPosition, ref this.mVariablePosition, ref this.mVariableStartPosition, ref this.mSelectedVariableIndex, ref this.mSelectedVariableName, ref this.mSelectedVariableTypeIndex);
		}

		public static bool DrawVariables(IVariableSource variableSource, BehaviorSource behaviorSource, ref string variableName, ref bool focusNameField, ref int variableTypeIndex, ref Vector2 scrollPosition, ref List<float> variablePosition, ref float variableStartPosition, ref int selectedVariableIndex, ref string selectedVariableName, ref int selectedVariableTypeIndex)
		{
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[0]);
			bool flag = false;
			bool flag2 = false;
			List<SharedVariable> list = (variableSource == null) ? null : variableSource.GetAllVariables();
			if (VariableInspector.DrawHeader(variableSource, behaviorSource == null, ref variableStartPosition, ref variableName, ref focusNameField, ref variableTypeIndex, ref selectedVariableIndex, ref selectedVariableName, ref selectedVariableTypeIndex))
			{
				flag = true;
			}
			list = ((variableSource == null) ? null : variableSource.GetAllVariables());
			if (list != null && list.get_Count() > 0)
			{
				GUI.set_enabled(!flag2);
				if (VariableInspector.DrawAllVariables(true, variableSource, ref list, true, ref variablePosition, ref selectedVariableIndex, ref selectedVariableName, ref selectedVariableTypeIndex, true, true))
				{
					flag = true;
				}
			}
			if (flag && variableSource != null)
			{
				variableSource.SetAllVariables(list);
			}
			GUI.set_enabled(true);
			GUILayout.EndScrollView();
			if (flag && !EditorApplication.get_isPlayingOrWillChangePlaymode() && behaviorSource != null && behaviorSource.get_Owner() is Behavior)
			{
				Behavior behavior = behaviorSource.get_Owner() as Behavior;
				if (behavior.get_ExternalBehavior() != null)
				{
					if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
					{
						BinarySerialization.Save(behaviorSource);
					}
					else
					{
						JSONSerialization.Save(behaviorSource);
					}
					BehaviorSource behaviorSource2 = behavior.get_ExternalBehavior().GetBehaviorSource();
					behaviorSource2.CheckForSerialization(true, null);
					VariableInspector.SyncVariables(behaviorSource2, list);
				}
			}
			return flag;
		}

		public static bool SyncVariables(BehaviorSource localBehaviorSource, List<SharedVariable> variables)
		{
			List<SharedVariable> list = localBehaviorSource.GetAllVariables();
			if (variables != null)
			{
				bool result = false;
				if (list == null)
				{
					list = new List<SharedVariable>();
					localBehaviorSource.SetAllVariables(list);
					result = true;
				}
				for (int i = 0; i < variables.get_Count(); i++)
				{
					if (list.get_Count() - 1 < i)
					{
						SharedVariable sharedVariable = Activator.CreateInstance(variables.get_Item(i).GetType()) as SharedVariable;
						sharedVariable.set_Name(variables.get_Item(i).get_Name());
						sharedVariable.set_IsShared(true);
						sharedVariable.SetValue(variables.get_Item(i).GetValue());
						list.Add(sharedVariable);
						result = true;
					}
					else if (list.get_Item(i).get_Name() != variables.get_Item(i).get_Name() || list.get_Item(i).GetType() != variables.get_Item(i).GetType())
					{
						SharedVariable sharedVariable2 = Activator.CreateInstance(variables.get_Item(i).GetType()) as SharedVariable;
						sharedVariable2.set_Name(variables.get_Item(i).get_Name());
						sharedVariable2.set_IsShared(true);
						sharedVariable2.SetValue(variables.get_Item(i).GetValue());
						list.set_Item(i, sharedVariable2);
						result = true;
					}
				}
				for (int j = list.get_Count() - 1; j > variables.get_Count() - 1; j--)
				{
					list.RemoveAt(j);
					result = true;
				}
				return result;
			}
			if (list != null && list.get_Count() > 0)
			{
				list.Clear();
				return true;
			}
			return false;
		}

		private static bool DrawHeader(IVariableSource variableSource, bool fromGlobalVariablesWindow, ref float variableStartPosition, ref string variableName, ref bool focusNameField, ref int variableTypeIndex, ref int selectedVariableIndex, ref string selectedVariableName, ref int selectedVariableTypeIndex)
		{
			if (VariableInspector.sharedVariableStrings == null)
			{
				VariableInspector.FindAllSharedVariableTypes(true);
			}
			EditorGUIUtility.set_labelWidth(150f);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Name", new GUILayoutOption[]
			{
				GUILayout.Width(70f)
			});
			GUI.SetNextControlName("Name");
			variableName = EditorGUILayout.TextField(variableName, new GUILayoutOption[]
			{
				GUILayout.Width(212f)
			});
			if (focusNameField)
			{
				GUI.FocusControl("Name");
				focusNameField = false;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(2f);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Space(4f);
			GUILayout.Label("Type", new GUILayoutOption[]
			{
				GUILayout.Width(70f)
			});
			variableTypeIndex = EditorGUILayout.Popup(variableTypeIndex, VariableInspector.sharedVariableStrings, EditorStyles.get_toolbarPopup(), new GUILayoutOption[]
			{
				GUILayout.Width(163f)
			});
			GUILayout.Space(8f);
			bool flag = false;
			bool flag2 = VariableInspector.VariableNameValid(variableSource, variableName);
			bool enabled = GUI.get_enabled();
			GUI.set_enabled(flag2 && enabled);
			GUI.SetNextControlName("Add");
			if (GUILayout.Button("Add", EditorStyles.get_toolbarButton(), new GUILayoutOption[]
			{
				GUILayout.Width(40f)
			}) && flag2)
			{
				if (fromGlobalVariablesWindow && variableSource == null)
				{
					GlobalVariables globalVariables = ScriptableObject.CreateInstance(typeof(GlobalVariables)) as GlobalVariables;
					string text = BehaviorDesignerUtility.GetEditorBaseDirectory(null).Substring(6, BehaviorDesignerUtility.GetEditorBaseDirectory(null).get_Length() - 13);
					string text2 = text + "/Resources/BehaviorDesignerGlobalVariables.asset";
					if (!Directory.Exists(Application.get_dataPath() + text + "/Resources"))
					{
						Directory.CreateDirectory(Application.get_dataPath() + text + "/Resources");
					}
					if (!File.Exists(Application.get_dataPath() + text2))
					{
						AssetDatabase.CreateAsset(globalVariables, "Assets" + text2);
						EditorUtility.DisplayDialog("Created Global Variables", "Behavior Designer Global Variables asset created:\n\nAssets" + text + "/Resources/BehaviorDesignerGlobalVariables.asset\n\nNote: Copy this file to transfer global variables between projects.", "OK");
					}
					variableSource = globalVariables;
				}
				flag = VariableInspector.AddVariable(variableSource, variableName, variableTypeIndex, fromGlobalVariablesWindow);
				if (flag)
				{
					selectedVariableIndex = variableSource.GetAllVariables().get_Count() - 1;
					selectedVariableName = variableName;
					selectedVariableTypeIndex = variableTypeIndex;
					variableName = string.Empty;
				}
			}
			GUILayout.Space(6f);
			GUILayout.EndHorizontal();
			if (!fromGlobalVariablesWindow)
			{
				GUI.set_enabled(true);
				GUILayout.Space(3f);
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				GUILayout.Space(5f);
				if (GUILayout.Button("Global Variables", EditorStyles.get_toolbarButton(), new GUILayoutOption[]
				{
					GUILayout.Width(284f)
				}))
				{
					GlobalVariablesWindow.ShowWindow();
				}
				GUILayout.EndHorizontal();
			}
			BehaviorDesignerUtility.DrawContentSeperator(2);
			GUILayout.Space(4f);
			if (variableStartPosition == -1f && Event.get_current().get_type() == 7)
			{
				variableStartPosition = GUILayoutUtility.GetLastRect().get_yMax();
			}
			GUI.set_enabled(enabled);
			return flag;
		}

		private static bool AddVariable(IVariableSource variableSource, string variableName, int variableTypeIndex, bool fromGlobalVariablesWindow)
		{
			SharedVariable sharedVariable = VariableInspector.CreateVariable(variableTypeIndex, variableName, fromGlobalVariablesWindow);
			List<SharedVariable> list = (variableSource == null) ? null : variableSource.GetAllVariables();
			if (list == null)
			{
				list = new List<SharedVariable>();
			}
			list.Add(sharedVariable);
			variableSource.SetAllVariables(list);
			return true;
		}

		public static bool DrawAllVariables(bool showFooter, IVariableSource variableSource, ref List<SharedVariable> variables, bool canSelect, ref List<float> variablePosition, ref int selectedVariableIndex, ref string selectedVariableName, ref int selectedVariableTypeIndex, bool drawRemoveButton, bool drawLastSeparator)
		{
			if (variables == null)
			{
				return false;
			}
			bool result = false;
			if (canSelect && variablePosition == null)
			{
				variablePosition = new List<float>();
			}
			for (int i = 0; i < variables.get_Count(); i++)
			{
				SharedVariable sharedVariable = variables.get_Item(i);
				if (sharedVariable != null)
				{
					if (canSelect && selectedVariableIndex == i)
					{
						if (i == 0)
						{
							GUILayout.Space(2f);
						}
						bool flag = false;
						if (VariableInspector.DrawSelectedVariable(variableSource, ref variables, sharedVariable, ref selectedVariableIndex, ref selectedVariableName, ref selectedVariableTypeIndex, ref flag))
						{
							result = true;
						}
						if (flag)
						{
							if (BehaviorDesignerWindow.instance != null)
							{
								BehaviorDesignerWindow.instance.RemoveSharedVariableReferences(sharedVariable);
							}
							variables.RemoveAt(i);
							if (selectedVariableIndex == i)
							{
								selectedVariableIndex = -1;
							}
							else if (selectedVariableIndex > i)
							{
								selectedVariableIndex--;
							}
							result = true;
							break;
						}
					}
					else
					{
						GUILayout.BeginHorizontal(new GUILayoutOption[0]);
						if (VariableInspector.DrawSharedVariable(variableSource, sharedVariable, false))
						{
							result = true;
						}
						if (drawRemoveButton && GUILayout.Button(BehaviorDesignerUtility.VariableDeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
						{
							GUILayout.Width(19f)
						}) && EditorUtility.DisplayDialog("Delete Variable", "Are you sure you want to delete this variable?", "Yes", "No"))
						{
							if (BehaviorDesignerWindow.instance != null)
							{
								BehaviorDesignerWindow.instance.RemoveSharedVariableReferences(sharedVariable);
							}
							variables.RemoveAt(i);
							if (canSelect)
							{
								if (selectedVariableIndex == i)
								{
									selectedVariableIndex = -1;
								}
								else if (selectedVariableIndex > i)
								{
									selectedVariableIndex--;
								}
							}
							result = true;
							break;
						}
						if (BehaviorDesignerWindow.instance != null && BehaviorDesignerWindow.instance.ContainsError(null, variables.get_Item(i).get_Name()))
						{
							GUILayout.Box(BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, new GUILayoutOption[]
							{
								GUILayout.Width(20f)
							});
						}
						GUILayout.Space(10f);
						GUILayout.EndHorizontal();
						if (i != variables.get_Count() - 1 || drawLastSeparator)
						{
							BehaviorDesignerUtility.DrawContentSeperator(2, 7);
						}
					}
					GUILayout.Space(4f);
					if (canSelect && Event.get_current().get_type() == 7)
					{
						if (variablePosition.get_Count() <= i)
						{
							variablePosition.Add(GUILayoutUtility.GetLastRect().get_yMax());
						}
						else
						{
							variablePosition.set_Item(i, GUILayoutUtility.GetLastRect().get_yMax());
						}
					}
				}
			}
			if (canSelect && variables.get_Count() < variablePosition.get_Count())
			{
				for (int j = variablePosition.get_Count() - 1; j >= variables.get_Count(); j--)
				{
					variablePosition.RemoveAt(j);
				}
			}
			if (showFooter && variables.get_Count() > 0)
			{
				GUI.set_enabled(true);
				GUILayout.Label("Select a variable to change its properties.", BehaviorDesignerUtility.LabelWrapGUIStyle, new GUILayoutOption[0]);
			}
			return result;
		}

		private static bool DrawSharedVariable(IVariableSource variableSource, SharedVariable sharedVariable, bool selected)
		{
			if (sharedVariable == null || sharedVariable.GetType().GetProperty("Value") == null)
			{
				return false;
			}
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			bool result = false;
			if (!string.IsNullOrEmpty(sharedVariable.get_PropertyMapping()))
			{
				if (selected)
				{
					GUILayout.Label("Property", new GUILayoutOption[0]);
				}
				else
				{
					GUILayout.Label(sharedVariable.get_Name(), new GUILayoutOption[0]);
				}
				string[] array = sharedVariable.get_PropertyMapping().Split(new char[]
				{
					'.'
				});
				GUILayout.Label(array[array.Length - 1].Replace('/', '.'), new GUILayoutOption[0]);
			}
			else
			{
				EditorGUI.BeginChangeCheck();
				FieldInspector.DrawFields(null, sharedVariable, new GUIContent(sharedVariable.get_Name()));
				result = EditorGUI.EndChangeCheck();
			}
			if (!sharedVariable.get_IsGlobal() && GUILayout.Button(BehaviorDesignerUtility.VariableMapButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Width(19f)
			}))
			{
				VariableInspector.ShowPropertyMappingMenu(variableSource as BehaviorSource, sharedVariable);
			}
			GUILayout.EndHorizontal();
			return result;
		}

		private static bool DrawSelectedVariable(IVariableSource variableSource, ref List<SharedVariable> variables, SharedVariable sharedVariable, ref int selectedVariableIndex, ref string selectedVariableName, ref int selectedVariableTypeIndex, ref bool deleted)
		{
			bool result = false;
			GUILayout.BeginVertical(BehaviorDesignerUtility.SelectedBackgroundGUIStyle, new GUILayoutOption[0]);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label("Name", new GUILayoutOption[]
			{
				GUILayout.Width(70f)
			});
			EditorGUI.BeginChangeCheck();
			selectedVariableName = GUILayout.TextField(selectedVariableName, new GUILayoutOption[]
			{
				GUILayout.Width(140f)
			});
			if (EditorGUI.EndChangeCheck())
			{
				if (VariableInspector.VariableNameValid(variableSource, selectedVariableName))
				{
					variableSource.UpdateVariableName(sharedVariable, selectedVariableName);
				}
				result = true;
			}
			GUILayout.Space(10f);
			bool enabled = GUI.get_enabled();
			GUI.set_enabled(enabled && selectedVariableIndex < variables.get_Count() - 1);
			if (GUILayout.Button(BehaviorDesignerUtility.DownArrowButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Width(19f)
			}))
			{
				SharedVariable sharedVariable2 = variables.get_Item(selectedVariableIndex + 1);
				variables.set_Item(selectedVariableIndex + 1, variables.get_Item(selectedVariableIndex));
				variables.set_Item(selectedVariableIndex, sharedVariable2);
				selectedVariableIndex++;
				result = true;
			}
			GUI.set_enabled(enabled && (selectedVariableIndex < variables.get_Count() - 1 || selectedVariableIndex != 0));
			GUILayout.Box(string.Empty, BehaviorDesignerUtility.ArrowSeparatorGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Width(1f),
				GUILayout.Height(18f)
			});
			GUI.set_enabled(enabled && selectedVariableIndex != 0);
			if (GUILayout.Button(BehaviorDesignerUtility.UpArrowButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Width(20f)
			}))
			{
				SharedVariable sharedVariable3 = variables.get_Item(selectedVariableIndex - 1);
				variables.set_Item(selectedVariableIndex - 1, variables.get_Item(selectedVariableIndex));
				variables.set_Item(selectedVariableIndex, sharedVariable3);
				selectedVariableIndex--;
				result = true;
			}
			GUI.set_enabled(enabled);
			if (GUILayout.Button(BehaviorDesignerUtility.VariableDeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Width(19f)
			}) && EditorUtility.DisplayDialog("Delete Variable", "Are you sure you want to delete this variable?", "Yes", "No"))
			{
				deleted = true;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(2f);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label("Type", new GUILayoutOption[]
			{
				GUILayout.Width(70f)
			});
			EditorGUI.BeginChangeCheck();
			selectedVariableTypeIndex = EditorGUILayout.Popup(selectedVariableTypeIndex, VariableInspector.sharedVariableStrings, EditorStyles.get_toolbarPopup(), new GUILayoutOption[]
			{
				GUILayout.Width(200f)
			});
			if (EditorGUI.EndChangeCheck() && VariableInspector.sharedVariableTypesDict.get_Item(sharedVariable.GetType().get_Name()) != selectedVariableTypeIndex)
			{
				if (BehaviorDesignerWindow.instance != null)
				{
					BehaviorDesignerWindow.instance.RemoveSharedVariableReferences(sharedVariable);
				}
				sharedVariable = VariableInspector.CreateVariable(selectedVariableTypeIndex, sharedVariable.get_Name(), sharedVariable.get_IsGlobal());
				variables.set_Item(selectedVariableIndex, sharedVariable);
				result = true;
			}
			GUILayout.EndHorizontal();
			EditorGUI.BeginChangeCheck();
			GUILayout.Space(4f);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUI.set_enabled(VariableInspector.CanNetworkSync(sharedVariable.GetType().GetProperty("Value").get_PropertyType()));
			EditorGUI.BeginChangeCheck();
			sharedVariable.set_NetworkSync(EditorGUILayout.Toggle(new GUIContent("Network Sync", "Sync this variable over the network. Requires Unity 5.1 or greator. A NetworkIdentity must be attached to the behavior tree GameObject."), sharedVariable.get_NetworkSync(), new GUILayoutOption[0]));
			if (EditorGUI.EndChangeCheck())
			{
				result = true;
			}
			GUILayout.EndHorizontal();
			GUI.set_enabled(enabled);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			if (VariableInspector.DrawSharedVariable(variableSource, sharedVariable, true))
			{
				result = true;
			}
			if (BehaviorDesignerWindow.instance != null && BehaviorDesignerWindow.instance.ContainsError(null, variables.get_Item(selectedVariableIndex).get_Name()))
			{
				GUILayout.Box(BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, new GUILayoutOption[]
				{
					GUILayout.Width(20f)
				});
			}
			GUILayout.EndHorizontal();
			BehaviorDesignerUtility.DrawContentSeperator(4, 7);
			GUILayout.EndVertical();
			GUILayout.Space(3f);
			return result;
		}

		private static bool VariableNameValid(IVariableSource variableSource, string variableName)
		{
			return !variableName.Equals(string.Empty) && (variableSource == null || variableSource.GetVariable(variableName) == null);
		}

		private static SharedVariable CreateVariable(int index, string name, bool global)
		{
			SharedVariable sharedVariable = Activator.CreateInstance(VariableInspector.sharedVariableTypes.get_Item(index)) as SharedVariable;
			sharedVariable.set_Name(name);
			sharedVariable.set_IsShared(true);
			sharedVariable.set_IsGlobal(global);
			return sharedVariable;
		}

		private static bool CanNetworkSync(Type type)
		{
			return type == typeof(bool) || type == typeof(Color) || type == typeof(float) || type == typeof(GameObject) || type == typeof(int) || type == typeof(Quaternion) || type == typeof(Rect) || type == typeof(string) || type == typeof(Transform) || type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4);
		}

		private static void ShowPropertyMappingMenu(BehaviorSource behaviorSource, SharedVariable sharedVariable)
		{
			VariableInspector.mPropertyMappingVariable = sharedVariable;
			VariableInspector.mPropertyMappingBehaviorSource = behaviorSource;
			VariableInspector.mPropertyMappingMenu = new GenericMenu();
			List<string> list = new List<string>();
			List<GameObject> list2 = new List<GameObject>();
			list.Add("None");
			list2.Add(null);
			int num = 0;
			if (behaviorSource.get_Owner().GetObject() is Behavior)
			{
				GameObject gameObject = (behaviorSource.get_Owner().GetObject() as Behavior).get_gameObject();
				int num2;
				if ((num2 = VariableInspector.AddPropertyName(sharedVariable, gameObject, ref list, ref list2, true)) != -1)
				{
					num = num2;
				}
				GameObject[] array;
				if (AssetDatabase.GetAssetPath(gameObject).get_Length() == 0)
				{
					array = Object.FindObjectsOfType<GameObject>();
				}
				else
				{
					Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
					array = new GameObject[componentsInChildren.Length];
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						array[i] = componentsInChildren[i].get_gameObject();
					}
				}
				for (int j = 0; j < array.Length; j++)
				{
					if (!array[j].Equals(gameObject) && (num2 = VariableInspector.AddPropertyName(sharedVariable, array[j], ref list, ref list2, false)) != -1)
					{
						num = num2;
					}
				}
			}
			for (int k = 0; k < list.get_Count(); k++)
			{
				string[] array2 = list.get_Item(k).Split(new char[]
				{
					'.'
				});
				if (list2.get_Item(k) != null)
				{
					array2[array2.Length - 1] = VariableInspector.GetFullPath(list2.get_Item(k).get_transform()) + "/" + array2[array2.Length - 1];
				}
				VariableInspector.mPropertyMappingMenu.AddItem(new GUIContent(array2[array2.Length - 1]), k == num, new GenericMenu.MenuFunction2(VariableInspector.PropertySelected), new VariableInspector.SelectedPropertyMapping(list.get_Item(k), list2.get_Item(k)));
			}
			VariableInspector.mPropertyMappingMenu.ShowAsContext();
		}

		private static string GetFullPath(Transform transform)
		{
			if (transform.get_parent() == null)
			{
				return transform.get_name();
			}
			return VariableInspector.GetFullPath(transform.get_parent()) + "/" + transform.get_name();
		}

		private static int AddPropertyName(SharedVariable sharedVariable, GameObject gameObject, ref List<string> propertyNames, ref List<GameObject> propertyGameObjects, bool behaviorGameObject)
		{
			int result = -1;
			if (gameObject != null)
			{
				Component[] components = gameObject.GetComponents(typeof(Component));
				Type propertyType = sharedVariable.GetType().GetProperty("Value").get_PropertyType();
				for (int i = 0; i < components.Length; i++)
				{
					if (!(components[i] == null))
					{
						PropertyInfo[] properties = components[i].GetType().GetProperties(20);
						for (int j = 0; j < properties.Length; j++)
						{
							if (properties[j].get_PropertyType().Equals(propertyType) && !properties[j].get_IsSpecialName())
							{
								string text = components[i].GetType().get_FullName() + "/" + properties[j].get_Name();
								if (text.Equals(sharedVariable.get_PropertyMapping()) && (object.Equals(sharedVariable.get_PropertyMappingOwner(), gameObject) || (object.Equals(sharedVariable.get_PropertyMappingOwner(), null) && behaviorGameObject)))
								{
									result = propertyNames.get_Count();
								}
								propertyNames.Add(text);
								propertyGameObjects.Add(gameObject);
							}
						}
					}
				}
			}
			return result;
		}

		private static void PropertySelected(object selected)
		{
			VariableInspector.SelectedPropertyMapping selectedPropertyMapping = selected as VariableInspector.SelectedPropertyMapping;
			if (selectedPropertyMapping.Property.Equals("None"))
			{
				VariableInspector.mPropertyMappingVariable.set_PropertyMapping(string.Empty);
				VariableInspector.mPropertyMappingVariable.set_PropertyMappingOwner(null);
			}
			else
			{
				VariableInspector.mPropertyMappingVariable.set_PropertyMapping(selectedPropertyMapping.Property);
				VariableInspector.mPropertyMappingVariable.set_PropertyMappingOwner(selectedPropertyMapping.GameObject);
			}
			if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
			{
				BinarySerialization.Save(VariableInspector.mPropertyMappingBehaviorSource);
			}
			else
			{
				JSONSerialization.Save(VariableInspector.mPropertyMappingBehaviorSource);
			}
		}
	}
}