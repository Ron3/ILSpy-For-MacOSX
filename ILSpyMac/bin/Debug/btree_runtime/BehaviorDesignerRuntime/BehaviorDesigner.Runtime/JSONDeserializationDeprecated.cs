using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using Object = UnityEngine.Object;

namespace BehaviorDesigner.Runtime
{
	public class JSONDeserializationDeprecated : Object
	{
		private struct TaskField
		{
			public Task task;

			public FieldInfo fieldInfo;

			public TaskField(Task t, FieldInfo f)
			{
				this.task = t;
				this.fieldInfo = f;
			}
		}

		private static Dictionary<JSONDeserializationDeprecated.TaskField, List<int>> taskIDs = null;

		private static GlobalVariables globalVariables = null;

		private static Dictionary<int, Dictionary<string, object>> serializationCache = new Dictionary<int, Dictionary<string, object>>();

		public static void Load(TaskSerializationData taskData, BehaviorSource behaviorSource)
		{
			behaviorSource.EntryTask = null;
			behaviorSource.RootTask = null;
			behaviorSource.DetachedTasks = null;
			behaviorSource.Variables = null;
			Dictionary<string, object> dictionary;
			if (!JSONDeserializationDeprecated.serializationCache.TryGetValue(taskData.JSONSerialization.GetHashCode(), ref dictionary))
			{
				dictionary = (MiniJSON.Deserialize(taskData.JSONSerialization) as Dictionary<string, object>);
				JSONDeserializationDeprecated.serializationCache.Add(taskData.JSONSerialization.GetHashCode(), dictionary);
			}
			if (dictionary == null)
			{
				Debug.Log("Failed to deserialize");
				return;
			}
			JSONDeserializationDeprecated.taskIDs = new Dictionary<JSONDeserializationDeprecated.TaskField, List<int>>();
			Dictionary<int, Task> dictionary2 = new Dictionary<int, Task>();
			JSONDeserializationDeprecated.DeserializeVariables(behaviorSource, dictionary, taskData.fieldSerializationData.unityObjects);
			if (dictionary.ContainsKey("EntryTask"))
			{
				behaviorSource.EntryTask = JSONDeserializationDeprecated.DeserializeTask(behaviorSource, dictionary.get_Item("EntryTask") as Dictionary<string, object>, ref dictionary2, taskData.fieldSerializationData.unityObjects);
			}
			if (dictionary.ContainsKey("RootTask"))
			{
				behaviorSource.RootTask = JSONDeserializationDeprecated.DeserializeTask(behaviorSource, dictionary.get_Item("RootTask") as Dictionary<string, object>, ref dictionary2, taskData.fieldSerializationData.unityObjects);
			}
			if (dictionary.ContainsKey("DetachedTasks"))
			{
				List<Task> list = new List<Task>();
				IEnumerator enumerator = (dictionary.get_Item("DetachedTasks") as IEnumerable).GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Dictionary<string, object> dict = (Dictionary<string, object>)enumerator.Current;
						list.Add(JSONDeserializationDeprecated.DeserializeTask(behaviorSource, dict, ref dictionary2, taskData.fieldSerializationData.unityObjects));
					}
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				behaviorSource.DetachedTasks = list;
			}
			if (JSONDeserializationDeprecated.taskIDs != null && JSONDeserializationDeprecated.taskIDs.Count > 0)
			{
				using (Dictionary<JSONDeserializationDeprecated.TaskField, List<int>>.KeyCollection.Enumerator enumerator2 = JSONDeserializationDeprecated.taskIDs.Keys.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						JSONDeserializationDeprecated.TaskField current = enumerator2.Current;
						List<int> list2 = JSONDeserializationDeprecated.taskIDs.get_Item(current);
						Type fieldType = current.fieldInfo.FieldType;
						if (current.fieldInfo.FieldType.IsArray)
						{
							int num = 0;
							for (int i = 0; i < list2.Count; i++)
							{
								Task task = dictionary2.get_Item(list2[i]);
								if (task.GetType().Equals(fieldType.GetElementType()) || task.GetType().IsSubclassOf(fieldType.GetElementType()))
								{
									num++;
								}
							}
							Array array = Array.CreateInstance(fieldType.GetElementType(), num);
							int num2 = 0;
							for (int j = 0; j < list2.Count; j++)
							{
								Task task2 = dictionary2.get_Item(list2.get_Item(j));
								if (task2.GetType().Equals(fieldType.GetElementType()) || task2.GetType().IsSubclassOf(fieldType.GetElementType()))
								{
									array.SetValue(task2, num2);
									num2++;
								}
							}
							current.fieldInfo.SetValue(current.task, array);
						}
						else
						{
							Task task3 = dictionary2.get_Item(list2.get_Item(0));
							if (task3.GetType().Equals(current.fieldInfo.FieldType) || task3.GetType().IsSubclassOf(current.fieldInfo.FieldType))
							{
								current.fieldInfo.SetValue(current.task, task3);
							}
						}
					}
				}
				JSONDeserializationDeprecated.taskIDs = null;
			}
		}

		public static void Load(string serialization, GlobalVariables globalVariables)
		{
			if (globalVariables == null)
			{
				return;
			}
			Dictionary<string, object> dictionary = MiniJSON.Deserialize(serialization) as Dictionary<string, object>;
			if (dictionary == null)
			{
				Debug.Log("Failed to deserialize");
				return;
			}
			if (globalVariables.VariableData == null)
			{
				globalVariables.VariableData = new VariableSerializationData();
			}
			JSONDeserializationDeprecated.DeserializeVariables(globalVariables, dictionary, globalVariables.VariableData.fieldSerializationData.unityObjects);
		}

		private static void DeserializeVariables(IVariableSource variableSource, Dictionary<string, object> dict, List<Object> unityObjects)
		{
			object obj;
			if (dict.TryGetValue("Variables", ref obj))
			{
				List<SharedVariable> list = new List<SharedVariable>();
				IList list2 = obj as IList;
				for (int i = 0; i < list2.Count; i++)
				{
					SharedVariable sharedVariable = JSONDeserializationDeprecated.DeserializeSharedVariable(list2[i] as Dictionary<string, object>, variableSource, true, unityObjects);
					list.Add(sharedVariable);
				}
				variableSource.SetAllVariables(list);
			}
		}

		public static Task DeserializeTask(BehaviorSource behaviorSource, Dictionary<string, object> dict, ref Dictionary<int, Task> IDtoTask, List<Object> unityObjects)
		{
			Task task = null;
			try
			{
				Type type = TaskUtility.GetTypeWithinAssembly(dict.get_Item("ObjectType") as string);
				if (type == null)
				{
					if (dict.ContainsKey("Children"))
					{
						type = typeof(UnknownParentTask);
					}
					else
					{
						type = typeof(UnknownTask);
					}
				}
				task = (TaskUtility.CreateInstance(type) as Task);
			}
			catch (Exception)
			{
			}
			if (task == null)
			{
				return null;
			}
			task.Owner = (behaviorSource.Owner.GetObject() as Behavior);
			task.ID = Convert.ToInt32(dict.get_Item("ID"));
			object obj;
			if (dict.TryGetValue("Name", ref obj))
			{
				task.FriendlyName = (string)obj;
			}
			if (dict.TryGetValue("Instant", ref obj))
			{
				task.IsInstant = Convert.ToBoolean(obj);
			}
			if (dict.TryGetValue("Disabled", ref obj))
			{
				task.Disabled = Convert.ToBoolean(obj);
			}
			IDtoTask.Add(task.ID, task);
			task.NodeData = JSONDeserializationDeprecated.DeserializeNodeData(dict.get_Item("NodeData") as Dictionary<string, object>, task);
			if (task.GetType().Equals(typeof(UnknownTask)) || task.GetType().Equals(typeof(UnknownParentTask)))
			{
				if (!task.FriendlyName.Contains("Unknown "))
				{
					task.FriendlyName = string.Format("Unknown {0}", task.FriendlyName);
				}
				if (!task.NodeData.Comment.Contains("Loaded from an unknown type. Was a task renamed or deleted?"))
				{
					task.NodeData.Comment = string.Format("Loaded from an unknown type. Was a task renamed or deleted?{0}", (!task.NodeData.Comment.Equals(string.Empty)) ? string.Format("\0{0}", task.NodeData.Comment) : string.Empty);
				}
			}
			JSONDeserializationDeprecated.DeserializeObject(task, task, dict, behaviorSource, unityObjects);
			if (task is ParentTask && dict.TryGetValue("Children", ref obj))
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask != null)
				{
					IEnumerator enumerator = (obj as IEnumerable).GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							Dictionary<string, object> dict2 = (Dictionary<string, object>)enumerator.Current;
							Task child = JSONDeserializationDeprecated.DeserializeTask(behaviorSource, dict2, ref IDtoTask, unityObjects);
							int index = (parentTask.Children != null) ? parentTask.Children.Count : 0;
							parentTask.AddChild(child, index);
						}
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
			return task;
		}

		private static NodeData DeserializeNodeData(Dictionary<string, object> dict, Task task)
		{
			NodeData nodeData = new NodeData();
			object obj;
			if (dict.TryGetValue("Offset", ref obj))
			{
				nodeData.Offset = JSONDeserializationDeprecated.StringToVector2((string)obj);
			}
			if (dict.TryGetValue("FriendlyName", ref obj))
			{
				task.FriendlyName = (string)obj;
			}
			if (dict.TryGetValue("Comment", ref obj))
			{
				nodeData.Comment = (string)obj;
			}
			if (dict.TryGetValue("IsBreakpoint", ref obj))
			{
				nodeData.IsBreakpoint = Convert.ToBoolean(obj);
			}
			if (dict.TryGetValue("Collapsed", ref obj))
			{
				nodeData.Collapsed = Convert.ToBoolean(obj);
			}
			if (dict.TryGetValue("ColorIndex", ref obj))
			{
				nodeData.ColorIndex = Convert.ToInt32(obj);
			}
			if (dict.TryGetValue("WatchedFields", ref obj))
			{
				nodeData.WatchedFieldNames = new List<string>();
				nodeData.WatchedFields = new List<FieldInfo>();
				IList list = obj as IList;
				for (int i = 0; i < list.Count; i++)
				{
					FieldInfo field = task.GetType().GetField((string)list[i], 52);
					if (field != null)
					{
						nodeData.WatchedFieldNames.Add(field.Name);
						nodeData.WatchedFields.Add(field);
					}
				}
			}
			return nodeData;
		}

		private static SharedVariable DeserializeSharedVariable(Dictionary<string, object> dict, IVariableSource variableSource, bool fromSource, List<Object> unityObjects)
		{
			if (dict == null)
			{
				return null;
			}
			SharedVariable sharedVariable = null;
			object obj;
			if (!fromSource && variableSource != null && dict.TryGetValue("Name", ref obj))
			{
				object obj2;
				dict.TryGetValue("IsGlobal", ref obj2);
				if (!dict.TryGetValue("IsGlobal", ref obj2) || !Convert.ToBoolean(obj2))
				{
					sharedVariable = variableSource.GetVariable(obj as string);
				}
				else
				{
					if (JSONDeserializationDeprecated.globalVariables == null)
					{
						JSONDeserializationDeprecated.globalVariables = GlobalVariables.Instance;
					}
					if (JSONDeserializationDeprecated.globalVariables != null)
					{
						sharedVariable = JSONDeserializationDeprecated.globalVariables.GetVariable(obj as string);
					}
				}
			}
			Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(dict.get_Item("Type") as string);
			if (typeWithinAssembly == null)
			{
				return null;
			}
			bool flag = true;
			if (sharedVariable == null || !(flag = sharedVariable.GetType().Equals(typeWithinAssembly)))
			{
				sharedVariable = (TaskUtility.CreateInstance(typeWithinAssembly) as SharedVariable);
				sharedVariable.Name = (dict.get_Item("Name") as string);
				object obj3;
				if (dict.TryGetValue("IsShared", ref obj3))
				{
					sharedVariable.IsShared = Convert.ToBoolean(obj3);
				}
				if (dict.TryGetValue("IsGlobal", ref obj3))
				{
					sharedVariable.IsGlobal = Convert.ToBoolean(obj3);
				}
				if (dict.TryGetValue("NetworkSync", ref obj3))
				{
					sharedVariable.NetworkSync = Convert.ToBoolean(obj3);
				}
				if (!sharedVariable.IsGlobal && dict.TryGetValue("PropertyMapping", ref obj3))
				{
					sharedVariable.PropertyMapping = (obj3 as string);
					if (dict.TryGetValue("PropertyMappingOwner", ref obj3))
					{
						sharedVariable.PropertyMappingOwner = (JSONDeserializationDeprecated.IndexToUnityObject(Convert.ToInt32(obj3), unityObjects) as GameObject);
					}
					sharedVariable.InitializePropertyMapping(variableSource as BehaviorSource);
				}
				if (!flag)
				{
					sharedVariable.IsShared = true;
				}
				JSONDeserializationDeprecated.DeserializeObject(null, sharedVariable, dict, variableSource, unityObjects);
			}
			return sharedVariable;
		}

		private static void DeserializeObject(Task task, object obj, Dictionary<string, object> dict, IVariableSource variableSource, List<Object> unityObjects)
		{
			if (dict == null)
			{
				return;
			}
			FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				object obj2;
				if (dict.TryGetValue(allFields[i].FieldType + "," + allFields[i].Name, ref obj2) || dict.TryGetValue(allFields[i].Name, ref obj2))
				{
					if (typeof(IList).IsAssignableFrom(allFields[i].FieldType))
					{
						IList list = obj2 as IList;
						if (list != null)
						{
							Type type;
							if (allFields[i].FieldType.IsArray)
							{
								type = allFields[i].FieldType.GetElementType();
							}
							else
							{
								Type type2 = allFields[i].FieldType;
								while (!type2.IsGenericType)
								{
									type2 = type2.BaseType;
								}
								type = type2.GetGenericArguments()[0];
							}
							bool flag = type.Equals(typeof(Task)) || type.IsSubclassOf(typeof(Task));
							if (flag)
							{
								if (JSONDeserializationDeprecated.taskIDs != null)
								{
									List<int> list2 = new List<int>();
									for (int j = 0; j < list.Count; j++)
									{
										list2.Add(Convert.ToInt32(list.get_Item(j)));
									}
									JSONDeserializationDeprecated.taskIDs.Add(new JSONDeserializationDeprecated.TaskField(task, allFields[i]), list2);
								}
							}
							else if (allFields[i].FieldType.IsArray)
							{
								Array array = Array.CreateInstance(type, list.Count);
								for (int k = 0; k < list.Count; k++)
								{
									array.SetValue(JSONDeserializationDeprecated.ValueToObject(task, type, list.get_Item(k), variableSource, unityObjects), k);
								}
								allFields[i].SetValue(obj, array);
							}
							else
							{
								IList list3;
								if (allFields[i].FieldType.IsGenericType)
								{
									list3 = (TaskUtility.CreateInstance(typeof(List).MakeGenericType(new Type[]
									{
										type
									})) as IList);
								}
								else
								{
									list3 = (TaskUtility.CreateInstance(allFields[i].FieldType) as IList);
								}
								for (int l = 0; l < list.Count; l++)
								{
									list3.Add(JSONDeserializationDeprecated.ValueToObject(task, type, list.get_Item(l), variableSource, unityObjects));
								}
								allFields[i].SetValue(obj, list3);
							}
						}
					}
					else
					{
						Type fieldType = allFields[i].FieldType;
						if (fieldType.Equals(typeof(Task)) || fieldType.IsSubclassOf(typeof(Task)))
						{
							if (TaskUtility.HasAttribute(allFields[i], typeof(InspectTaskAttribute)))
							{
								Dictionary<string, object> dictionary = obj2 as Dictionary<string, object>;
								Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(dictionary.get_Item("ObjectType") as string);
								if (typeWithinAssembly != null)
								{
									Task task2 = TaskUtility.CreateInstance(typeWithinAssembly) as Task;
									JSONDeserializationDeprecated.DeserializeObject(task2, task2, dictionary, variableSource, unityObjects);
									allFields[i].SetValue(task, task2);
								}
							}
							else if (JSONDeserializationDeprecated.taskIDs != null)
							{
								List<int> list4 = new List<int>();
								list4.Add(Convert.ToInt32(obj2));
								JSONDeserializationDeprecated.taskIDs.Add(new JSONDeserializationDeprecated.TaskField(task, allFields[i]), list4);
							}
						}
						else
						{
							allFields[i].SetValue(obj, JSONDeserializationDeprecated.ValueToObject(task, fieldType, obj2, variableSource, unityObjects));
						}
					}
				}
				else if (typeof(SharedVariable).IsAssignableFrom(allFields[i].FieldType) && !allFields[i].FieldType.IsAbstract)
				{
					if (dict.TryGetValue(allFields[i].FieldType + "," + allFields[i].Name, ref obj2))
					{
						SharedVariable sharedVariable = TaskUtility.CreateInstance(allFields[i].FieldType) as SharedVariable;
						sharedVariable.SetValue(JSONDeserializationDeprecated.ValueToObject(task, allFields[i].FieldType, obj2, variableSource, unityObjects));
						allFields[i].SetValue(obj, sharedVariable);
					}
					else
					{
						SharedVariable sharedVariable2 = TaskUtility.CreateInstance(allFields[i].FieldType) as SharedVariable;
						allFields[i].SetValue(obj, sharedVariable2);
					}
				}
			}
		}

		private static object ValueToObject(Task task, Type type, object obj, IVariableSource variableSource, List<Object> unityObjects)
		{
			if (type.Equals(typeof(SharedVariable)) || type.IsSubclassOf(typeof(SharedVariable)))
			{
				SharedVariable sharedVariable = JSONDeserializationDeprecated.DeserializeSharedVariable(obj as Dictionary<string, object>, variableSource, false, unityObjects);
				if (sharedVariable == null)
				{
					sharedVariable = (TaskUtility.CreateInstance(type) as SharedVariable);
				}
				return sharedVariable;
			}
			if (type.Equals(typeof(Object)) || type.IsSubclassOf(typeof(Object)))
			{
				return JSONDeserializationDeprecated.IndexToUnityObject(Convert.ToInt32(obj), unityObjects);
			}
			if (!type.IsPrimitive)
			{
				if (!type.Equals(typeof(string)))
				{
					goto IL_C5;
				}
			}
			try
			{
				object result = Convert.ChangeType(obj, type);
				return result;
			}
			catch (Exception)
			{
				object result = null;
				return result;
			}
			IL_C5:
			if (type.IsSubclassOf(typeof(Enum)))
			{
				try
				{
					object result = Enum.Parse(type, (string)obj);
					return result;
				}
				catch (Exception)
				{
					object result = null;
					return result;
				}
			}
			if (type.Equals(typeof(Vector2)))
			{
				return JSONDeserializationDeprecated.StringToVector2((string)obj);
			}
			if (type.Equals(typeof(Vector3)))
			{
				return JSONDeserializationDeprecated.StringToVector3((string)obj);
			}
			if (type.Equals(typeof(Vector4)))
			{
				return JSONDeserializationDeprecated.StringToVector4((string)obj);
			}
			if (type.Equals(typeof(Quaternion)))
			{
				return JSONDeserializationDeprecated.StringToQuaternion((string)obj);
			}
			if (type.Equals(typeof(Matrix4x4)))
			{
				return JSONDeserializationDeprecated.StringToMatrix4x4((string)obj);
			}
			if (type.Equals(typeof(Color)))
			{
				return JSONDeserializationDeprecated.StringToColor((string)obj);
			}
			if (type.Equals(typeof(Rect)))
			{
				return JSONDeserializationDeprecated.StringToRect((string)obj);
			}
			if (type.Equals(typeof(LayerMask)))
			{
				return JSONDeserializationDeprecated.ValueToLayerMask(Convert.ToInt32(obj));
			}
			if (type.Equals(typeof(AnimationCurve)))
			{
				return JSONDeserializationDeprecated.ValueToAnimationCurve((Dictionary<string, object>)obj);
			}
			object obj2 = TaskUtility.CreateInstance(type);
			JSONDeserializationDeprecated.DeserializeObject(task, obj2, obj as Dictionary<string, object>, variableSource, unityObjects);
			return obj2;
		}

		private static Vector2 StringToVector2(string vector2String)
		{
			string[] array = vector2String.Substring(1, vector2String.Length - 2).Split(new char[]
			{
				','
			});
			return new Vector2(float.Parse(array[0]), float.Parse(array[1]));
		}

		private static Vector3 StringToVector3(string vector3String)
		{
			string[] array = vector3String.Substring(1, vector3String.Length - 2).Split(new char[]
			{
				','
			});
			return new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
		}

		private static Vector4 StringToVector4(string vector4String)
		{
			string[] array = vector4String.Substring(1, vector4String.Length - 2).Split(new char[]
			{
				','
			});
			return new Vector4(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
		}

		private static Quaternion StringToQuaternion(string quaternionString)
		{
			string[] array = quaternionString.Substring(1, quaternionString.Length - 2).Split(new char[]
			{
				','
			});
			return new Quaternion(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
		}

		private static Matrix4x4 StringToMatrix4x4(string matrixString)
		{
			string[] array = matrixString.Split(null);
			return new Matrix4x4
			{
				m00 = float.Parse(array[0]),
				m01 = float.Parse(array[1]),
				m02 = float.Parse(array[2]),
				m03 = float.Parse(array[3]),
				m10 = float.Parse(array[4]),
				m11 = float.Parse(array[5]),
				m12 = float.Parse(array[6]),
				m13 = float.Parse(array[7]),
				m20 = float.Parse(array[8]),
				m21 = float.Parse(array[9]),
				m22 = float.Parse(array[10]),
				m23 = float.Parse(array[11]),
				m30 = float.Parse(array[12]),
				m31 = float.Parse(array[13]),
				m32 = float.Parse(array[14]),
				m33 = float.Parse(array[15])
			};
		}

		private static Color StringToColor(string colorString)
		{
			string[] array = colorString.Substring(5, colorString.Length - 6).Split(new char[]
			{
				','
			});
			return new Color(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
		}

		private static Rect StringToRect(string rectString)
		{
			string[] array = rectString.Substring(1, rectString.Length - 2).Split(new char[]
			{
				','
			});
			return new Rect(float.Parse(array[0].Substring(2, array[0].Length - 2)), float.Parse(array[1].Substring(3, array[1].Length - 3)), float.Parse(array[2].Substring(7, array[2].Length - 7)), float.Parse(array[3].Substring(8, array[3].Length - 8)));
		}

		private static LayerMask ValueToLayerMask(int value)
		{
			LayerMask result = default(LayerMask);
			result.value=value;
			return result;
		}

		private static AnimationCurve ValueToAnimationCurve(Dictionary<string, object> value)
		{
			AnimationCurve animationCurve = new AnimationCurve();
			object obj;
			if (value.TryGetValue("Keys", ref obj))
			{
				List<object> list = obj as List<object>;
				for (int i = 0; i < list.Count; i++)
				{
					List<object> list2 = list[i] as List<object>;
					Keyframe keyframe = new Keyframe((float)Convert.ChangeType(list2.get_Item(0), typeof(float)), (float)Convert.ChangeType(list2.get_Item(1), typeof(float)), (float)Convert.ChangeType(list2.get_Item(2), typeof(float)), (float)Convert.ChangeType(list2.get_Item(3), typeof(float)));
					keyframe.tangentMode=(int)Convert.ChangeType(list2.get_Item(4), typeof(int));
					animationCurve.AddKey(keyframe);
				}
			}
			if (value.TryGetValue("PreWrapMode", ref obj))
			{
				animationCurve.preWrapMode=(int)Enum.Parse(typeof(WrapMode), (string)obj);
			}
			if (value.TryGetValue("PostWrapMode", ref obj))
			{
				animationCurve.postWrapMode=(int)Enum.Parse(typeof(WrapMode), (string)obj);
			}
			return animationCurve;
		}

		private static Object IndexToUnityObject(int index, List<Object> unityObjects)
		{
			if (index < 0 || index >= unityObjects.Count)
			{
				return null;
			}
			return unityObjects.get_Item(index);
		}
	}
}
