using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public class JSONSerialization : UnityEngine.Object
	{
		private static TaskSerializationData taskSerializationData;

		private static FieldSerializationData fieldSerializationData;

		private static VariableSerializationData variableSerializationData;

		public static void Save(BehaviorSource behaviorSource)
		{
			behaviorSource.CheckForSerialization(false, null);
			JSONSerialization.taskSerializationData = new TaskSerializationData();
			JSONSerialization.fieldSerializationData = JSONSerialization.taskSerializationData.fieldSerializationData;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (behaviorSource.get_EntryTask() != null)
			{
				dictionary.Add("EntryTask", JSONSerialization.SerializeTask(behaviorSource.get_EntryTask(), true, ref JSONSerialization.fieldSerializationData.unityObjects));
			}
			if (behaviorSource.get_RootTask() != null)
			{
				dictionary.Add("RootTask", JSONSerialization.SerializeTask(behaviorSource.get_RootTask(), true, ref JSONSerialization.fieldSerializationData.unityObjects));
			}
			if (behaviorSource.get_DetachedTasks() != null && behaviorSource.get_DetachedTasks().get_Count() > 0)
			{
				Dictionary<string, object>[] array = new Dictionary<string, object>[behaviorSource.get_DetachedTasks().get_Count()];
				for (int i = 0; i < behaviorSource.get_DetachedTasks().get_Count(); i++)
				{
					array[i] = JSONSerialization.SerializeTask(behaviorSource.get_DetachedTasks().get_Item(i), true, ref JSONSerialization.fieldSerializationData.unityObjects);
				}
				dictionary.Add("DetachedTasks", array);
			}
			if (behaviorSource.get_Variables() != null && behaviorSource.get_Variables().get_Count() > 0)
			{
				dictionary.Add("Variables", JSONSerialization.SerializeVariables(behaviorSource.get_Variables(), ref JSONSerialization.fieldSerializationData.unityObjects));
			}
			JSONSerialization.taskSerializationData.Version = "1.5.11";
			JSONSerialization.taskSerializationData.JSONSerialization = MiniJSON.Serialize(dictionary);
			behaviorSource.set_TaskData(JSONSerialization.taskSerializationData);
			if (behaviorSource.get_Owner() != null && !behaviorSource.get_Owner().Equals(null))
			{
				BehaviorDesignerUtility.SetObjectDirty(behaviorSource.get_Owner().GetObject());
			}
		}

		public static void Save(GlobalVariables variables)
		{
			if (variables == null)
			{
				return;
			}
			JSONSerialization.variableSerializationData = new VariableSerializationData();
			JSONSerialization.fieldSerializationData = JSONSerialization.variableSerializationData.fieldSerializationData;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Variables", JSONSerialization.SerializeVariables(variables.get_Variables(), ref JSONSerialization.fieldSerializationData.unityObjects));
			JSONSerialization.variableSerializationData.JSONSerialization = MiniJSON.Serialize(dictionary);
			variables.set_VariableData(JSONSerialization.variableSerializationData);
			variables.set_Version("1.5.11");
			BehaviorDesignerUtility.SetObjectDirty(variables);
		}

		private static Dictionary<string, object>[] SerializeVariables(List<SharedVariable> variables, ref List<UnityEngine.Object> unityObjects)
		{
			Dictionary<string, object>[] array = new Dictionary<string, object>[variables.get_Count()];
			for (int i = 0; i < variables.get_Count(); i++)
			{
				array[i] = JSONSerialization.SerializeVariable(variables.get_Item(i), ref unityObjects);
			}
			return array;
		}

		public static Dictionary<string, object> SerializeTask(Task task, bool serializeChildren, ref List<UnityEngine.Object> unityObjects)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Type", task.GetType());
			dictionary.Add("NodeData", JSONSerialization.SerializeNodeData(task.get_NodeData()));
			dictionary.Add("ID", task.get_ID());
			dictionary.Add("Name", task.get_FriendlyName());
			dictionary.Add("Instant", task.get_IsInstant());
			if (task.get_Disabled())
			{
				dictionary.Add("Disabled", task.get_Disabled());
			}
			JSONSerialization.SerializeFields(task, ref dictionary, ref unityObjects);
			if (serializeChildren && task is ParentTask)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.get_Children() != null && parentTask.get_Children().get_Count() > 0)
				{
					Dictionary<string, object>[] array = new Dictionary<string, object>[parentTask.get_Children().get_Count()];
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						array[i] = JSONSerialization.SerializeTask(parentTask.get_Children().get_Item(i), serializeChildren, ref unityObjects);
					}
					dictionary.Add("Children", array);
				}
			}
			return dictionary;
		}

		private static Dictionary<string, object> SerializeNodeData(NodeData nodeData)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Offset", nodeData.get_Offset());
			if (nodeData.get_Comment().get_Length() > 0)
			{
				dictionary.Add("Comment", nodeData.get_Comment());
			}
			if (nodeData.get_IsBreakpoint())
			{
				dictionary.Add("IsBreakpoint", nodeData.get_IsBreakpoint());
			}
			if (nodeData.get_Collapsed())
			{
				dictionary.Add("Collapsed", nodeData.get_Collapsed());
			}
			if (nodeData.get_ColorIndex() != 0)
			{
				dictionary.Add("ColorIndex", nodeData.get_ColorIndex());
			}
			if (nodeData.get_WatchedFieldNames() != null && nodeData.get_WatchedFieldNames().get_Count() > 0)
			{
				dictionary.Add("WatchedFields", nodeData.get_WatchedFieldNames());
			}
			return dictionary;
		}

		private static Dictionary<string, object> SerializeVariable(SharedVariable sharedVariable, ref List<UnityEngine.Object> unityObjects)
		{
			if (sharedVariable == null)
			{
				return null;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Type", sharedVariable.GetType());
			dictionary.Add("Name", sharedVariable.get_Name());
			if (sharedVariable.get_IsShared())
			{
				dictionary.Add("IsShared", sharedVariable.get_IsShared());
			}
			if (sharedVariable.get_IsGlobal())
			{
				dictionary.Add("IsGlobal", sharedVariable.get_IsGlobal());
			}
			if (sharedVariable.get_NetworkSync())
			{
				dictionary.Add("NetworkSync", sharedVariable.get_NetworkSync());
			}
			if (!string.IsNullOrEmpty(sharedVariable.get_PropertyMapping()))
			{
				dictionary.Add("PropertyMapping", sharedVariable.get_PropertyMapping());
				if (!object.Equals(sharedVariable.get_PropertyMappingOwner(), null))
				{
					dictionary.Add("PropertyMappingOwner", unityObjects.get_Count());
					unityObjects.Add(sharedVariable.get_PropertyMappingOwner());
				}
			}
			JSONSerialization.SerializeFields(sharedVariable, ref dictionary, ref unityObjects);
			return dictionary;
		}

		private static void SerializeFields(object obj, ref Dictionary<string, object> dict, ref List<UnityEngine.Object> unityObjects)
		{
			FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				if (!BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(NonSerializedAttribute)) && ((!allFields[i].get_IsPrivate() && !allFields[i].get_IsFamily()) || BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(SerializeField))) && (!(obj is ParentTask) || !allFields[i].get_Name().Equals("children")))
				{
					if (allFields[i].GetValue(obj) != null)
					{
						string text = (allFields[i].get_FieldType().get_Name() + allFields[i].get_Name()).ToString();
						if (typeof(IList).IsAssignableFrom(allFields[i].get_FieldType()))
						{
							IList list = allFields[i].GetValue(obj) as IList;
							if (list != null)
							{
								List<object> list2 = new List<object>();
								for (int j = 0; j < list.get_Count(); j++)
								{
									if (list.get_Item(j) == null)
									{
										list2.Add(null);
									}
									else
									{
										Type type = list.get_Item(j).GetType();
										if (list.get_Item(j) is Task)
										{
											Task task = list.get_Item(j) as Task;
											list2.Add(task.get_ID());
										}
										else if (list.get_Item(j) is SharedVariable)
										{
											list2.Add(JSONSerialization.SerializeVariable(list.get_Item(j) as SharedVariable, ref unityObjects));
										}
										else if (list.get_Item(j) is Object)
										{
											Object @object = list.get_Item(j) as Object;
											if (!object.ReferenceEquals(@object, null) && @object != null)
											{
												list2.Add(unityObjects.get_Count());
												unityObjects.Add(@object);
											}
										}
										else if (type.Equals(typeof(LayerMask)))
										{
											list2.Add(((LayerMask)list.get_Item(j)).get_value());
										}
										else if (type.get_IsPrimitive() || type.get_IsEnum() || type.Equals(typeof(string)) || type.Equals(typeof(Vector2)) || type.Equals(typeof(Vector3)) || type.Equals(typeof(Vector4)) || type.Equals(typeof(Quaternion)) || type.Equals(typeof(Matrix4x4)) || type.Equals(typeof(Color)) || type.Equals(typeof(Rect)))
										{
											list2.Add(list.get_Item(j));
										}
										else
										{
											Dictionary<string, object> dictionary = new Dictionary<string, object>();
											JSONSerialization.SerializeFields(list.get_Item(j), ref dictionary, ref unityObjects);
											list2.Add(dictionary);
										}
									}
								}
								if (list2 != null)
								{
									dict.Add(text, list2);
								}
							}
						}
						else if (typeof(Task).IsAssignableFrom(allFields[i].get_FieldType()))
						{
							Task task2 = allFields[i].GetValue(obj) as Task;
							if (task2 != null)
							{
								if (BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(InspectTaskAttribute)))
								{
									Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
									dictionary2.Add("Type", task2.GetType());
									JSONSerialization.SerializeFields(task2, ref dictionary2, ref unityObjects);
									dict.Add(text, dictionary2);
								}
								else
								{
									dict.Add(text, task2.get_ID());
								}
							}
						}
						else if (typeof(SharedVariable).IsAssignableFrom(allFields[i].get_FieldType()))
						{
							if (!dict.ContainsKey(text))
							{
								dict.Add(text, JSONSerialization.SerializeVariable(allFields[i].GetValue(obj) as SharedVariable, ref unityObjects));
							}
						}
						else if (typeof(Object).IsAssignableFrom(allFields[i].get_FieldType()))
						{
							Object object2 = allFields[i].GetValue(obj) as Object;
							if (!object.ReferenceEquals(object2, null) && object2 != null)
							{
								dict.Add(text, unityObjects.get_Count());
								unityObjects.Add(object2);
							}
						}
						else if (allFields[i].get_FieldType().Equals(typeof(LayerMask)))
						{
							dict.Add(text, ((LayerMask)allFields[i].GetValue(obj)).get_value());
						}
						else if (allFields[i].get_FieldType().get_IsPrimitive() || allFields[i].get_FieldType().get_IsEnum() || allFields[i].get_FieldType().Equals(typeof(string)) || allFields[i].get_FieldType().Equals(typeof(Vector2)) || allFields[i].get_FieldType().Equals(typeof(Vector3)) || allFields[i].get_FieldType().Equals(typeof(Vector4)) || allFields[i].get_FieldType().Equals(typeof(Quaternion)) || allFields[i].get_FieldType().Equals(typeof(Matrix4x4)) || allFields[i].get_FieldType().Equals(typeof(Color)) || allFields[i].get_FieldType().Equals(typeof(Rect)))
						{
							dict.Add(text, allFields[i].GetValue(obj));
						}
						else if (allFields[i].get_FieldType().Equals(typeof(AnimationCurve)))
						{
							AnimationCurve animationCurve = allFields[i].GetValue(obj) as AnimationCurve;
							Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
							if (animationCurve.get_keys() != null)
							{
								Keyframe[] keys = animationCurve.get_keys();
								List<List<object>> list3 = new List<List<object>>();
								for (int k = 0; k < keys.Length; k++)
								{
									List<object> list4 = new List<object>();
									list4.Add(keys[k].get_time());
									list4.Add(keys[k].get_value());
									list4.Add(keys[k].get_inTangent());
									list4.Add(keys[k].get_outTangent());
									list4.Add(keys[k].get_tangentMode());
									list3.Add(list4);
								}
								dictionary3.Add("Keys", list3);
							}
							dictionary3.Add("PreWrapMode", animationCurve.get_preWrapMode());
							dictionary3.Add("PostWrapMode", animationCurve.get_postWrapMode());
							dict.Add(text, dictionary3);
						}
						else
						{
							Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
							JSONSerialization.SerializeFields(allFields[i].GetValue(obj), ref dictionary4, ref unityObjects);
							dict.Add(text, dictionary4);
						}
					}
				}
			}
		}
	}
}
