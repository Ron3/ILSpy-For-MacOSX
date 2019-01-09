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
			if (behaviorSource.EntryTask != null)
			{
				dictionary.Add("EntryTask", JSONSerialization.SerializeTask(behaviorSource.EntryTask, true, ref JSONSerialization.fieldSerializationData.unityObjects));
			}
			if (behaviorSource.RootTask != null)
			{
				dictionary.Add("RootTask", JSONSerialization.SerializeTask(behaviorSource.RootTask, true, ref JSONSerialization.fieldSerializationData.unityObjects));
			}
			if (behaviorSource.DetachedTasks != null && behaviorSource.DetachedTasks.Count > 0)
			{
				Dictionary<string, object>[] array = new Dictionary<string, object>[behaviorSource.DetachedTasks.Count];
				for (int i = 0; i < behaviorSource.DetachedTasks.Count; i++)
				{
					array[i] = JSONSerialization.SerializeTask(behaviorSource.DetachedTasks.get_Item(i), true, ref JSONSerialization.fieldSerializationData.unityObjects);
				}
				dictionary.Add("DetachedTasks", array);
			}
			if (behaviorSource.Variables != null && behaviorSource.Variables.Count > 0)
			{
				dictionary.Add("Variables", JSONSerialization.SerializeVariables(behaviorSource.Variables, ref JSONSerialization.fieldSerializationData.unityObjects));
			}
			JSONSerialization.taskSerializationData.Version = "1.5.11";
			JSONSerialization.taskSerializationData.JSONSerialization = MiniJSON.Serialize(dictionary);
			behaviorSource.TaskData=JSONSerialization.taskSerializationData;
			if (behaviorSource.Owner != null && !behaviorSource.Owner.Equals(null))
			{
				BehaviorDesignerUtility.SetObjectDirty(behaviorSource.Owner.GetObject());
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
			dictionary.Add("Variables", JSONSerialization.SerializeVariables(variables.Variables, ref JSONSerialization.fieldSerializationData.unityObjects));
			JSONSerialization.variableSerializationData.JSONSerialization = MiniJSON.Serialize(dictionary);
			variables.VariableData=JSONSerialization.variableSerializationData;
			variables.Version="1.5.11";
			BehaviorDesignerUtility.SetObjectDirty(variables);
		}

		private static Dictionary<string, object>[] SerializeVariables(List<SharedVariable> variables, ref List<UnityEngine.Object> unityObjects)
		{
			Dictionary<string, object>[] array = new Dictionary<string, object>[variables.Count];
			for (int i = 0; i < variables.Count; i++)
			{
				array[i] = JSONSerialization.SerializeVariable(variables.get_Item(i), ref unityObjects);
			}
			return array;
		}

		public static Dictionary<string, object> SerializeTask(Task task, bool serializeChildren, ref List<UnityEngine.Object> unityObjects)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Type", task.GetType());
			dictionary.Add("NodeData", JSONSerialization.SerializeNodeData(task.NodeData));
			dictionary.Add("ID", task.ID);
			dictionary.Add("Name", task.FriendlyName);
			dictionary.Add("Instant", task.IsInstant);
			if (task.Disabled)
			{
				dictionary.Add("Disabled", task.Disabled);
			}
			JSONSerialization.SerializeFields(task, ref dictionary, ref unityObjects);
			if (serializeChildren && task is ParentTask)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children != null && parentTask.Children.Count > 0)
				{
					Dictionary<string, object>[] array = new Dictionary<string, object>[parentTask.Children.Count];
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						array[i] = JSONSerialization.SerializeTask(parentTask.Children.get_Item(i), serializeChildren, ref unityObjects);
					}
					dictionary.Add("Children", array);
				}
			}
			return dictionary;
		}

		private static Dictionary<string, object> SerializeNodeData(NodeData nodeData)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Offset", nodeData.Offset);
			if (nodeData.Comment.Length > 0)
			{
				dictionary.Add("Comment", nodeData.Comment);
			}
			if (nodeData.IsBreakpoint)
			{
				dictionary.Add("IsBreakpoint", nodeData.IsBreakpoint);
			}
			if (nodeData.Collapsed)
			{
				dictionary.Add("Collapsed", nodeData.Collapsed);
			}
			if (nodeData.ColorIndex != 0)
			{
				dictionary.Add("ColorIndex", nodeData.ColorIndex);
			}
			if (nodeData.WatchedFieldNames != null && nodeData.WatchedFieldNames.Count > 0)
			{
				dictionary.Add("WatchedFields", nodeData.WatchedFieldNames);
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
			dictionary.Add("Name", sharedVariable.Name);
			if (sharedVariable.IsShared)
			{
				dictionary.Add("IsShared", sharedVariable.IsShared);
			}
			if (sharedVariable.IsGlobal)
			{
				dictionary.Add("IsGlobal", sharedVariable.IsGlobal);
			}
			if (sharedVariable.NetworkSync)
			{
				dictionary.Add("NetworkSync", sharedVariable.NetworkSync);
			}
			if (!string.IsNullOrEmpty(sharedVariable.PropertyMapping))
			{
				dictionary.Add("PropertyMapping", sharedVariable.PropertyMapping);
				if (!object.Equals(sharedVariable.PropertyMappingOwner, null))
				{
					dictionary.Add("PropertyMappingOwner", unityObjects.Count);
					unityObjects.Add(sharedVariable.PropertyMappingOwner);
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
				if (!BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(NonSerializedAttribute)) && ((!allFields[i].IsPrivate && !allFields[i].IsFamily) || BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(SerializeField))) && (!(obj is ParentTask) || !allFields[i].Name.Equals("children")))
				{
					if (allFields[i].GetValue(obj) != null)
					{
						string text = (allFields[i].FieldType.Name + allFields[i].Name).ToString();
						if (typeof(IList).IsAssignableFrom(allFields[i].FieldType))
						{
							IList list = allFields[i].GetValue(obj) as IList;
							if (list != null)
							{
								List<object> list2 = new List<object>();
								for (int j = 0; j < list.Count; j++)
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
											list2.Add(task.ID);
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
												list2.Add(unityObjects.Count);
												unityObjects.Add(@object);
											}
										}
										else if (type.Equals(typeof(LayerMask)))
										{
											list2.Add(((LayerMask)list.get_Item(j)).value);
										}
										else if (type.IsPrimitive || type.IsEnum || type.Equals(typeof(string)) || type.Equals(typeof(Vector2)) || type.Equals(typeof(Vector3)) || type.Equals(typeof(Vector4)) || type.Equals(typeof(Quaternion)) || type.Equals(typeof(Matrix4x4)) || type.Equals(typeof(Color)) || type.Equals(typeof(Rect)))
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
						else if (typeof(Task).IsAssignableFrom(allFields[i].FieldType))
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
									dict.Add(text, task2.ID);
								}
							}
						}
						else if (typeof(SharedVariable).IsAssignableFrom(allFields[i].FieldType))
						{
							if (!dict.ContainsKey(text))
							{
								dict.Add(text, JSONSerialization.SerializeVariable(allFields[i].GetValue(obj) as SharedVariable, ref unityObjects));
							}
						}
						else if (typeof(Object).IsAssignableFrom(allFields[i].FieldType))
						{
							Object object2 = allFields[i].GetValue(obj) as Object;
							if (!object.ReferenceEquals(object2, null) && object2 != null)
							{
								dict.Add(text, unityObjects.Count);
								unityObjects.Add(object2);
							}
						}
						else if (allFields[i].FieldType.Equals(typeof(LayerMask)))
						{
							dict.Add(text, ((LayerMask)allFields[i].GetValue(obj)).value);
						}
						else if (allFields[i].FieldType.IsPrimitive || allFields[i].FieldType.IsEnum || allFields[i].FieldType.Equals(typeof(string)) || allFields[i].FieldType.Equals(typeof(Vector2)) || allFields[i].FieldType.Equals(typeof(Vector3)) || allFields[i].FieldType.Equals(typeof(Vector4)) || allFields[i].FieldType.Equals(typeof(Quaternion)) || allFields[i].FieldType.Equals(typeof(Matrix4x4)) || allFields[i].FieldType.Equals(typeof(Color)) || allFields[i].FieldType.Equals(typeof(Rect)))
						{
							dict.Add(text, allFields[i].GetValue(obj));
						}
						else if (allFields[i].FieldType.Equals(typeof(AnimationCurve)))
						{
							AnimationCurve animationCurve = allFields[i].GetValue(obj) as AnimationCurve;
							Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
							if (animationCurve.keys != null)
							{
								Keyframe[] keys = animationCurve.keys;
								List<List<object>> list3 = new List<List<object>>();
								for (int k = 0; k < keys.Length; k++)
								{
									List<object> list4 = new List<object>();
									list4.Add(keys[k].time);
									list4.Add(keys[k].value);
									list4.Add(keys[k].inTangent);
									list4.Add(keys[k].outTangent);
									list4.Add(keys[k].tangentMode);
									list3.Add(list4);
								}
								dictionary3.Add("Keys", list3);
							}
							dictionary3.Add("PreWrapMode", animationCurve.preWrapMode);
							dictionary3.Add("PostWrapMode", animationCurve.postWrapMode);
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
