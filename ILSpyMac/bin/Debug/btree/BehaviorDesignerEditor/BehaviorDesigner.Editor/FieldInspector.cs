﻿using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public static class FieldInspector
	{
		private const string c_EditorPrefsFoldoutKey = "BehaviorDesigner.Editor.Foldout.";

		private static int currentKeyboardControl = -1;

		private static bool editingArray = false;

		private static int savedArraySize = -1;

		private static int editingFieldHash;

		public static BehaviorSource behaviorSource;

		private static HashSet<int> drawnObjects = new HashSet<int>();

		private static string[] layerNames;

		private static int[] maskValues;

		public static void Init()
		{
			FieldInspector.InitLayers();
		}

		public static bool DrawFoldout(int hash, GUIContent guiContent)
		{
			string text = string.Concat(new object[]
			{
				"BehaviorDesigner.Editor.Foldout..",
				hash,
				".",
				guiContent.text
			});
			bool @bool = EditorPrefs.GetBool(text, true);
			bool flag = EditorGUILayout.Foldout(@bool, guiContent);
			if (flag != @bool)
			{
				EditorPrefs.SetBool(text, flag);
			}
			return flag;
		}

		public static object DrawFields(Task task, object obj)
		{
			return FieldInspector.DrawFields(task, obj, null);
		}

		public static object DrawFields(Task task, object obj, GUIContent guiContent)
		{
			if (obj == null)
			{
				return null;
			}
			List<Type> baseClasses = FieldInspector.GetBaseClasses(obj.GetType());
			BindingFlags bindingFlags = 54;
			for (int i = baseClasses.Count - 1; i > -1; i--)
			{
				FieldInfo[] fields = baseClasses[i].GetFields(bindingFlags);
				for (int j = 0; j < fields.Length; j++)
				{
					if (!BehaviorDesignerUtility.HasAttribute(fields[j], typeof(NonSerializedAttribute)) && !BehaviorDesignerUtility.HasAttribute(fields[j], typeof(HideInInspector)) && ((!fields[j].IsPrivate && !fields[j].IsFamily) || BehaviorDesignerUtility.HasAttribute(fields[j], typeof(SerializeField))) && (!(obj is ParentTask) || !fields[j].Name.Equals("children")))
					{
						if (guiContent == null)
						{
							string name = fields[j].Name;
							TooltipAttribute[] array;
							if ((array = (fields[j].GetCustomAttributes(typeof(TooltipAttribute), false) as TooltipAttribute[])).Length > 0)
							{
								guiContent = new GUIContent(BehaviorDesignerUtility.SplitCamelCase(name), array[0].Tooltip);
							}
							else
							{
								guiContent = new GUIContent(BehaviorDesignerUtility.SplitCamelCase(name));
							}
						}
						EditorGUI.BeginChangeCheck();
						object obj2 = FieldInspector.DrawField(task, guiContent, fields[j], fields[j].GetValue(obj));
						if (EditorGUI.EndChangeCheck())
						{
							fields[j].SetValue(obj, obj2);
							GUI.changed=true;
						}
						guiContent = null;
					}
				}
			}
			return obj;
		}

		public static List<Type> GetBaseClasses(Type t)
		{
			List<Type> list = new List<Type>();
			while (t != null && !t.Equals(typeof(ParentTask)) && !t.Equals(typeof(Task)) && !t.Equals(typeof(SharedVariable)))
			{
				list.Add(t);
				t = t.BaseType;
			}
			return list;
		}

		public static object DrawField(Task task, GUIContent guiContent, FieldInfo field, object value)
		{
			ObjectDrawer objectDrawer;
			if ((objectDrawer = ObjectDrawerUtility.GetObjectDrawer(task, field)) != null)
			{
				if (value == null && !field.FieldType.IsAbstract)
				{
					value = Activator.CreateInstance(field.FieldType, true);
				}
				objectDrawer.Value = value;
				objectDrawer.OnGUI(guiContent);
				if (objectDrawer.Value != value)
				{
					value = objectDrawer.Value;
					GUI.changed=true;
				}
				return value;
			}
			ObjectDrawerAttribute[] array;
			if ((array = (field.GetCustomAttributes(typeof(ObjectDrawerAttribute), true) as ObjectDrawerAttribute[])).Length > 0 && (objectDrawer = ObjectDrawerUtility.GetObjectDrawer(task, array[0])) != null)
			{
				if (value == null)
				{
					value = Activator.CreateInstance(field.FieldType, true);
				}
				objectDrawer.Value = value;
				objectDrawer.OnGUI(guiContent);
				if (objectDrawer.Value != value)
				{
					value = objectDrawer.Value;
					GUI.changed=true;
				}
				return value;
			}
			return FieldInspector.DrawField(task, guiContent, field, field.FieldType, value);
		}

		private static object DrawField(Task task, GUIContent guiContent, FieldInfo fieldInfo, Type fieldType, object value)
		{
			if (typeof(IList).IsAssignableFrom(fieldType))
			{
				return FieldInspector.DrawArrayField(task, guiContent, fieldInfo, fieldType, value);
			}
			return FieldInspector.DrawSingleField(task, guiContent, fieldInfo, fieldType, value);
		}

		private static object DrawArrayField(Task task, GUIContent guiContent, FieldInfo fieldInfo, Type fieldType, object value)
		{
			Type type;
			if (fieldType.IsArray)
			{
				type = fieldType.GetElementType();
			}
			else
			{
				Type type2 = fieldType;
				while (!type2.IsGenericType)
				{
					type2 = type2.BaseType;
				}
				type = type2.GetGenericArguments()[0];
			}
			IList list;
			if (value == null)
			{
				if (fieldType.IsGenericType || fieldType.IsArray)
				{
					list = (Activator.CreateInstance(typeof(List).MakeGenericType(new Type[]
					{
						type
					}), true) as IList);
				}
				else
				{
					list = (Activator.CreateInstance(fieldType, true) as IList);
				}
				if (fieldType.IsArray)
				{
					Array array = Array.CreateInstance(type, list.Count);
					list.CopyTo(array, 0);
					list = array;
				}
				GUI.changed=true;
			}
			else
			{
				list = (IList)value;
			}
			EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
			if (FieldInspector.DrawFoldout(guiContent.text.GetHashCode(), guiContent))
			{
				EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
				bool flag = guiContent.text.GetHashCode() == FieldInspector.editingFieldHash;
				int num = (!flag) ? list.Count : FieldInspector.savedArraySize;
				int num2 = EditorGUILayout.IntField("Size", num, new GUILayoutOption[0]);
				if (flag && FieldInspector.editingArray && (GUIUtility.keyboardControl != FieldInspector.currentKeyboardControl || Event.current.keyCode == 13))
				{
					if (num2 != list.Count)
					{
						Array array2 = Array.CreateInstance(type, num2);
						int num3 = -1;
						for (int i = 0; i < num2; i++)
						{
							if (i < list.Count)
							{
								num3 = i;
							}
							if (num3 == -1)
							{
								break;
							}
							object obj = list.get_Item(num3);
							if (i >= list.Count && !typeof(Object).IsAssignableFrom(type) && !typeof(string).IsAssignableFrom(type))
							{
								obj = Activator.CreateInstance(list.get_Item(num3).GetType(), true);
							}
							array2.SetValue(obj, i);
						}
						if (fieldType.IsArray)
						{
							list = array2;
						}
						else
						{
							if (fieldType.IsGenericType)
							{
								list = (Activator.CreateInstance(typeof(List).MakeGenericType(new Type[]
								{
									type
								}), true) as IList);
							}
							else
							{
								list = (Activator.CreateInstance(fieldType, true) as IList);
							}
							for (int j = 0; j < array2.Length; j++)
							{
								list.Add(array2.GetValue(j));
							}
						}
					}
					FieldInspector.editingArray = false;
					FieldInspector.savedArraySize = -1;
					FieldInspector.editingFieldHash = -1;
					GUI.changed=true;
				}
				else if (num2 != num)
				{
					if (!FieldInspector.editingArray)
					{
						FieldInspector.currentKeyboardControl = GUIUtility.keyboardControl;
						FieldInspector.editingArray = true;
						FieldInspector.editingFieldHash = guiContent.text.GetHashCode();
					}
					FieldInspector.savedArraySize = num2;
				}
				for (int k = 0; k < list.Count; k++)
				{
					GUILayout.BeginHorizontal(new GUILayoutOption[0]);
					guiContent.text="Element " + k;
					// list.Item=k, FieldInspector.DrawField(task, guiContent, fieldInfo, type, list.Item[k]);
					list[k] = FieldInspector.DrawField(task, guiContent, fieldInfo, type, list[k]);
					GUILayout.Space(6f);
					GUILayout.EndHorizontal();
				}
				EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
			}
			EditorGUILayout.EndVertical();
			return list;
		}

		private static object DrawSingleField(Task task, GUIContent guiContent, FieldInfo fieldInfo, Type fieldType, object value)
		{
			if (fieldType.Equals(typeof(int)))
			{
				return EditorGUILayout.IntField(guiContent, (int)value, new GUILayoutOption[0]);
			}
			if (fieldType.Equals(typeof(float)))
			{
				return EditorGUILayout.FloatField(guiContent, (float)value, new GUILayoutOption[0]);
			}
			if (fieldType.Equals(typeof(double)))
			{
				return EditorGUILayout.FloatField(guiContent, Convert.ToSingle((double)value), new GUILayoutOption[0]);
			}
			if (fieldType.Equals(typeof(long)))
			{
				return (long)EditorGUILayout.IntField(guiContent, Convert.ToInt32((long)value), new GUILayoutOption[0]);
			}
			if (fieldType.Equals(typeof(bool)))
			{
				return EditorGUILayout.Toggle(guiContent, (bool)value, new GUILayoutOption[0]);
			}
			if (fieldType.Equals(typeof(string)))
			{
				return EditorGUILayout.TextField(guiContent, (string)value, new GUILayoutOption[0]);
			}
			if (fieldType.Equals(typeof(byte)))
			{
				return Convert.ToByte(EditorGUILayout.IntField(guiContent, Convert.ToInt32(value), new GUILayoutOption[0]));
			}
			if (fieldType.Equals(typeof(Vector2)))
			{
				return EditorGUILayout.Vector2Field(guiContent, (Vector2)value, new GUILayoutOption[0]);
			}
			if (fieldType.Equals(typeof(Vector3)))
			{
				return EditorGUILayout.Vector3Field(guiContent, (Vector3)value, new GUILayoutOption[0]);
			}
			if (fieldType.Equals(typeof(Vector4)))
			{
				return EditorGUILayout.Vector4Field(guiContent.text, (Vector4)value, new GUILayoutOption[0]);
			}
			if (fieldType.Equals(typeof(Quaternion)))
			{
				Quaternion quaternion = (Quaternion)value;
				Vector4 vector = Vector4.zero;
				vector.Set(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
				vector = EditorGUILayout.Vector4Field(guiContent.text, vector, new GUILayoutOption[0]);
				quaternion.Set(vector.x, vector.y, vector.z, vector.w);
				return quaternion;
			}
			if (fieldType.Equals(typeof(Color)))
			{
				return EditorGUILayout.ColorField(guiContent, (Color)value, new GUILayoutOption[0]);
			}
			if (fieldType.Equals(typeof(Rect)))
			{
				return EditorGUILayout.RectField(guiContent, (Rect)value, new GUILayoutOption[0]);
			}
			if (fieldType.Equals(typeof(Matrix4x4)))
			{
				GUILayout.BeginVertical(new GUILayoutOption[0]);
				if (FieldInspector.DrawFoldout(guiContent.text.GetHashCode(), guiContent))
				{
					EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
					Matrix4x4 matrix4x = (Matrix4x4)value;
					for (int i = 0; i < 4; i++)
					{
						for (int j = 0; j < 4; j++)
						{
							EditorGUI.BeginChangeCheck();
							// matrix4x.Item=i, j, EditorGUILayout.FloatField("E" + i.ToString() + j.ToString(), matrix4x.get_Item(i, j), new GUILayoutOption[0]);
							matrix4x[i, j] = EditorGUILayout.FloatField("E" + i.ToString() + j.ToString(), matrix4x[i, j], new GUILayoutOption[0]);
							if (EditorGUI.EndChangeCheck())
							{
								GUI.changed=true;
							}
						}
					}
					value = matrix4x;
					EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
				}
				GUILayout.EndVertical();
				return value;
			}
			if (fieldType.Equals(typeof(AnimationCurve)))
			{
				if (value == null)
				{
					value = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
					GUI.changed=true;
				}
				return EditorGUILayout.CurveField(guiContent, (AnimationCurve)value, new GUILayoutOption[0]);
			}
			if (fieldType.Equals(typeof(LayerMask)))
			{
				return FieldInspector.DrawLayerMask(guiContent, (LayerMask)value);
			}
			if (typeof(SharedVariable).IsAssignableFrom(fieldType))
			{
				return FieldInspector.DrawSharedVariable(task, guiContent, fieldInfo, fieldType, value as SharedVariable);
			}
			if (typeof(Object).IsAssignableFrom(fieldType))
			{
				return EditorGUILayout.ObjectField(guiContent, (Object)value, fieldType, true, new GUILayoutOption[0]);
			}
			if (fieldType.IsEnum)
			{
				return EditorGUILayout.EnumPopup(guiContent, (Enum)value, new GUILayoutOption[0]);
			}
			if (fieldType.IsClass || (fieldType.IsValueType && !fieldType.IsPrimitive))
			{
				if (typeof(Delegate).IsAssignableFrom(fieldType))
				{
					return null;
				}
				int hashCode = guiContent.text.GetHashCode();
				if (FieldInspector.drawnObjects.Contains(hashCode))
				{
					return null;
				}
				try
				{
					FieldInspector.drawnObjects.Add(hashCode);
					GUILayout.BeginVertical(new GUILayoutOption[0]);
					if (value == null)
					{
						if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable))
						{
							fieldType = Nullable.GetUnderlyingType(fieldType);
						}
						value = Activator.CreateInstance(fieldType, true);
					}
					if (FieldInspector.DrawFoldout(hashCode, guiContent))
					{
						EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
						value = FieldInspector.DrawFields(task, value);
						EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
					}
					FieldInspector.drawnObjects.Remove(hashCode);
					GUILayout.EndVertical();
					object result = value;
					return result;
				}
				catch (Exception)
				{
					GUILayout.EndVertical();
					FieldInspector.drawnObjects.Remove(hashCode);
					object result = null;
					return result;
				}
			}
			EditorGUILayout.LabelField("Unsupported Type: " + fieldType, new GUILayoutOption[0]);
			return null;
		}

		public static SharedVariable DrawSharedVariable(Task task, GUIContent guiContent, FieldInfo fieldInfo, Type fieldType, SharedVariable sharedVariable)
		{
			if (!fieldType.Equals(typeof(SharedVariable)) && sharedVariable == null)
			{
				sharedVariable = (Activator.CreateInstance(fieldType, true) as SharedVariable);
				if (TaskUtility.HasAttribute(fieldInfo, typeof(RequiredFieldAttribute)) || TaskUtility.HasAttribute(fieldInfo, typeof(SharedRequiredAttribute)))
				{
					sharedVariable.IsShared=true;
				}
				GUI.changed=true;
			}
			if (sharedVariable == null || sharedVariable.IsShared)
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				string[] array = null;
				int num = -1;
				int num2 = FieldInspector.GetVariablesOfType((sharedVariable == null) ? null : sharedVariable.GetType().GetProperty("Value").PropertyType, sharedVariable != null && sharedVariable.IsGlobal, (sharedVariable == null) ? string.Empty : sharedVariable.Name, FieldInspector.behaviorSource, out array, ref num, fieldType.Equals(typeof(SharedVariable)));
				Color backgroundColor = GUI.backgroundColor;
				if (num2 == 0 && !TaskUtility.HasAttribute(fieldInfo, typeof(SharedRequiredAttribute)))
				{
					GUI.backgroundColor=Color.red;
				}
				int num3 = num2;
				num2 = EditorGUILayout.Popup(guiContent.text, num2, array, BehaviorDesignerUtility.SharedVariableToolbarPopup, new GUILayoutOption[0]);
				GUI.backgroundColor=backgroundColor;
				if (num2 != num3)
				{
					if (num2 == 0)
					{
						if (fieldType.Equals(typeof(SharedVariable)))
						{
							sharedVariable = null;
						}
						else
						{
							sharedVariable = (Activator.CreateInstance(fieldType, true) as SharedVariable);
							sharedVariable.IsShared=true;
						}
					}
					else if (num != -1 && num2 >= num)
					{
						sharedVariable = GlobalVariables.Instance.GetVariable(array[num2].Substring(8, array[num2].Length - 8));
					}
					else
					{
						sharedVariable = FieldInspector.behaviorSource.GetVariable(array[num2]);
					}
					GUI.changed=true;
				}
				if (!fieldType.Equals(typeof(SharedVariable)) && !TaskUtility.HasAttribute(fieldInfo, typeof(RequiredFieldAttribute)) && !TaskUtility.HasAttribute(fieldInfo, typeof(SharedRequiredAttribute)))
				{
					sharedVariable = FieldInspector.DrawSharedVariableToggleSharedButton(sharedVariable);
					GUILayout.Space(-3f);
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(3f);
			}
			else
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				ObjectDrawerAttribute[] array2;
				ObjectDrawer objectDrawer;
				if (fieldInfo != null && (array2 = (fieldInfo.GetCustomAttributes(typeof(ObjectDrawerAttribute), true) as ObjectDrawerAttribute[])).Length > 0 && (objectDrawer = ObjectDrawerUtility.GetObjectDrawer(task, array2[0])) != null)
				{
					objectDrawer.Value = sharedVariable;
					objectDrawer.OnGUI(guiContent);
				}
				else
				{
					FieldInspector.DrawFields(task, sharedVariable, guiContent);
				}
				if (!TaskUtility.HasAttribute(fieldInfo, typeof(RequiredFieldAttribute)) && !TaskUtility.HasAttribute(fieldInfo, typeof(SharedRequiredAttribute)))
				{
					sharedVariable = FieldInspector.DrawSharedVariableToggleSharedButton(sharedVariable);
				}
				GUILayout.EndHorizontal();
			}
			return sharedVariable;
		}

		public static int GetVariablesOfType(Type valueType, bool isGlobal, string name, BehaviorSource behaviorSource, out string[] names, ref int globalStartIndex, bool getAll)
		{
			if (behaviorSource == null)
			{
				names = new string[0];
				return 0;
			}
			List<SharedVariable> variables = behaviorSource.Variables;
			int result = 0;
			List<string> list = new List<string>();
			list.Add("None");
			if (variables != null)
			{
				for (int i = 0; i < variables.Count; i++)
				{
					if (variables[i] != null)
					{
						Type propertyType = variables[i].GetType().GetProperty("Value").PropertyType;
						if (valueType == null || getAll || valueType.IsAssignableFrom(propertyType))
						{
							list.Add(variables[i].Name);
							if (!isGlobal && variables[i].Name.Equals(name))
							{
								result = list.Count - 1;
							}
						}
					}
				}
			}
			GlobalVariables instance;
			if ((instance = GlobalVariables.Instance) != null)
			{
				globalStartIndex = list.Count;
				variables = instance.Variables;
				if (variables != null)
				{
					for (int j = 0; j < variables.Count; j++)
					{
						if (variables[j] != null)
						{
							Type propertyType2 = variables[j].GetType().GetProperty("Value").PropertyType;
							if (valueType == null || getAll || propertyType2.Equals(valueType))
							{
								list.Add("Globals/" + variables[j].Name);
								if (isGlobal && variables[j].Name.Equals(name))
								{
									result = list.Count - 1;
								}
							}
						}
					}
				}
			}
			names = list.ToArray();
			return result;
		}

		internal static SharedVariable DrawSharedVariableToggleSharedButton(SharedVariable sharedVariable)
		{
			if (sharedVariable == null)
			{
				return null;
			}
			if (GUILayout.Button((!sharedVariable.IsShared) ? BehaviorDesignerUtility.VariableButtonTexture : BehaviorDesignerUtility.VariableButtonSelectedTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Width(15f)
			}))
			{
				bool isShared = !sharedVariable.IsShared;
				if (sharedVariable.GetType().Equals(typeof(SharedVariable)))
				{
					sharedVariable = (Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(sharedVariable.GetType().GetProperty("Value").PropertyType), true) as SharedVariable);
				}
				else
				{
					sharedVariable = (Activator.CreateInstance(sharedVariable.GetType(), true) as SharedVariable);
				}
				sharedVariable.IsShared=isShared;
			}
			return sharedVariable;
		}

		internal static Type FriendlySharedVariableName(Type type)
		{
			if (type.Equals(typeof(bool)))
			{
				return TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedBool");
			}
			if (type.Equals(typeof(int)))
			{
				return TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedInt");
			}
			if (type.Equals(typeof(float)))
			{
				return TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedFloat");
			}
			if (type.Equals(typeof(string)))
			{
				return TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString");
			}
			if (typeof(Object).IsAssignableFrom(type))
			{
				Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Shared" + type.Name);
				if (typeWithinAssembly != null)
				{
					return typeWithinAssembly;
				}
			}
			else
			{
				Type typeWithinAssembly2 = TaskUtility.GetTypeWithinAssembly("Shared" + type.Name);
				if (typeWithinAssembly2 != null)
				{
					return typeWithinAssembly2;
				}
			}
			return type;
		}

		private static LayerMask DrawLayerMask(GUIContent guiContent, LayerMask layerMask)
		{
			if (FieldInspector.layerNames == null)
			{
				FieldInspector.InitLayers();
			}
			int num = 0;
			for (int i = 0; i < FieldInspector.layerNames.Length; i++)
			{
				if ((layerMask.value & FieldInspector.maskValues[i]) == FieldInspector.maskValues[i])
				{
					num |= 1 << i;
				}
			}
			int num2 = EditorGUILayout.MaskField(guiContent, num, FieldInspector.layerNames, new GUILayoutOption[0]);
			if (num2 != num)
			{
				num = 0;
				for (int j = 0; j < FieldInspector.layerNames.Length; j++)
				{
					if ((num2 & 1 << j) != 0)
					{
						num |= FieldInspector.maskValues[j];
					}
				}
				layerMask.value=num;
			}
			return layerMask;
		}

		private static void InitLayers()
		{
			List<string> list = new List<string>();
			List<int> list2 = new List<int>();
			for (int i = 0; i < 32; i++)
			{
				string text = LayerMask.LayerToName(i);
				if (!string.IsNullOrEmpty(text))
				{
					list.Add(text);
					list2.Add(1 << i);
				}
			}
			FieldInspector.layerNames = list.ToArray();
			FieldInspector.maskValues = list2.ToArray();
		}
	}
}
