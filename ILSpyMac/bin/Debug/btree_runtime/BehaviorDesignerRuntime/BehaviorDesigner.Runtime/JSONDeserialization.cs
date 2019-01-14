using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

using Object = UnityEngine.Object;

namespace BehaviorDesigner.Runtime
{
	public class JSONDeserialization : Object
	{
		public struct TaskField
		{
			public Task task;

			public FieldInfo fieldInfo;

			public TaskField(Task t, FieldInfo f)
			{
				this.task = t;
				this.fieldInfo = f;
			}
		}

		private static Dictionary<JSONDeserialization.TaskField, List<int>> taskIDs = null;

		private static GlobalVariables globalVariables = null;

		public static bool updatedSerialization = true;

		private static Dictionary<int, Dictionary<string, object>> serializationCache = new Dictionary<int, Dictionary<string, object>>();

		public static Dictionary<JSONDeserialization.TaskField, List<int>> TaskIDs
		{
			get
			{
				return JSONDeserialization.taskIDs;
			}
			set
			{
				JSONDeserialization.taskIDs = value;
			}
		}

		public static void Load(TaskSerializationData taskData, BehaviorSource behaviorSource)
		{
			if (taskData != null && string.IsNullOrEmpty(taskData.Version))
			{
				JSONDeserializationDeprecated.Load(taskData, behaviorSource);
				return;
			}
			behaviorSource.EntryTask = null;
			behaviorSource.RootTask = null;
			behaviorSource.DetachedTasks = null;
			behaviorSource.Variables = null;
			Dictionary<string, object> dictionary;
			if (!JSONDeserialization.serializationCache.TryGetValue(taskData.JSONSerialization.GetHashCode(), ref dictionary))
			{
				dictionary = (MiniJSON.Deserialize(taskData.JSONSerialization) as Dictionary<string, object>);
				JSONDeserialization.serializationCache.Add(taskData.JSONSerialization.GetHashCode(), dictionary);
			}
			if (dictionary == null)
			{
				Debug.Log("Failed to deserialize");
				return;
			}
			JSONDeserialization.taskIDs = new Dictionary<JSONDeserialization.TaskField, List<int>>();
			Version version = new Version(taskData.Version);
			JSONDeserialization.updatedSerialization = (version.CompareTo(new Version("1.5.7")) >= 0);
			Dictionary<int, Task> dictionary2 = new Dictionary<int, Task>();
			JSONDeserialization.DeserializeVariables(behaviorSource, dictionary, taskData.fieldSerializationData.unityObjects);
			if (dictionary.ContainsKey("EntryTask"))
			{
				behaviorSource.EntryTask = JSONDeserialization.DeserializeTask(behaviorSource, dictionary["EntryTask"] as Dictionary<string, object>, ref dictionary2, taskData.fieldSerializationData.unityObjects);
			}
			if (dictionary.ContainsKey("RootTask"))
			{
				behaviorSource.RootTask = JSONDeserialization.DeserializeTask(behaviorSource, dictionary["RootTask"] as Dictionary<string, object>, ref dictionary2, taskData.fieldSerializationData.unityObjects);
			}
			if (dictionary.ContainsKey("DetachedTasks"))
			{
				List<Task> list = new List<Task>();
				IEnumerator enumerator = (dictionary["DetachedTasks"] as IEnumerable).GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Dictionary<string, object> dict = (Dictionary<string, object>)enumerator.Current;
						list.Add(JSONDeserialization.DeserializeTask(behaviorSource, dict, ref dictionary2, taskData.fieldSerializationData.unityObjects));
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
			if (JSONDeserialization.taskIDs != null && JSONDeserialization.taskIDs.Count > 0)
			{
				using (Dictionary<JSONDeserialization.TaskField, List<int>>.KeyCollection.Enumerator enumerator2 = JSONDeserialization.taskIDs.Keys.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						JSONDeserialization.TaskField current = enumerator2.Current;
						List<int> list2 = JSONDeserialization.taskIDs[current];
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
								Task task2 = dictionary2.get_Item(list2[j]);
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
							Task task3 = dictionary2.get_Item(list2[0]);
							if (task3.GetType().Equals(current.fieldInfo.FieldType) || task3.GetType().IsSubclassOf(current.fieldInfo.FieldType))
							{
								current.fieldInfo.SetValue(current.task, task3);
							}
						}
					}
				}
				JSONDeserialization.taskIDs = null;
			}
		}

		public static void Load(string serialization, GlobalVariables globalVariables, string version)
		{
			if (globalVariables == null)
			{
				return;
			}
			if (string.IsNullOrEmpty(version))
			{
				JSONDeserializationDeprecated.Load(serialization, globalVariables);
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
			Version version2 = new Version(globalVariables.Version);
			JSONDeserialization.updatedSerialization = (version2.CompareTo(new Version("1.5.7")) >= 0);
			JSONDeserialization.DeserializeVariables(globalVariables, dictionary, globalVariables.VariableData.fieldSerializationData.unityObjects);
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
					SharedVariable sharedVariable = JSONDeserialization.DeserializeSharedVariable(list2[i] as Dictionary<string, object>, variableSource, true, unityObjects);
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
				Type type = TaskUtility.GetTypeWithinAssembly(dict["Type"] as string);
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
				if (task is UnknownTask)
				{
					UnknownTask unknownTask = task as UnknownTask;
					unknownTask.JSONSerialization = MiniJSON.Serialize(dict);
				}
			}
			catch (Exception)
			{
			}
			if (task == null)
			{
				return null;
			}
			task.Owner = (behaviorSource.Owner.GetObject() as Behavior);
			task.ID = Convert.ToInt32(dict["ID"], CultureInfo.InvariantCulture);
			object obj;
			if (dict.TryGetValue("Name", out obj))
			{
				task.FriendlyName = (string)obj;
			}
			if (dict.TryGetValue("Instant", out obj))
			{
				task.IsInstant = Convert.ToBoolean(obj, CultureInfo.InvariantCulture);
			}
			if (dict.TryGetValue("Disabled", out obj))
			{
				task.Disabled = Convert.ToBoolean(obj, CultureInfo.InvariantCulture);
			}
			IDtoTask.Add(task.ID, task);
			task.NodeData = JSONDeserialization.DeserializeNodeData(dict["NodeData"] as Dictionary<string, object>, task);
			if (task.GetType().Equals(typeof(UnknownTask)) || task.GetType().Equals(typeof(UnknownParentTask)))
			{
				if (!task.FriendlyName.Contains("Unknown "))
				{
					task.FriendlyName = string.Format("Unknown {0}", task.FriendlyName);
				}
				task.NodeData.Comment = "Unknown Task. Right click and Replace to locate new task.";
			}
			JSONDeserialization.DeserializeObject(task, task, dict, behaviorSource, unityObjects);
			if (task is ParentTask && dict.TryGetValue("Children", out obj))
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
							Task child = JSONDeserialization.DeserializeTask(behaviorSource, dict2, ref IDtoTask, unityObjects);
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
				nodeData.Offset = JSONDeserialization.StringToVector2((string)obj);
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
				nodeData.IsBreakpoint = Convert.ToBoolean(obj, CultureInfo.InvariantCulture);
			}
			if (dict.TryGetValue("Collapsed", ref obj))
			{
				nodeData.Collapsed = Convert.ToBoolean(obj, CultureInfo.InvariantCulture);
			}
			if (dict.TryGetValue("ColorIndex", ref obj))
			{
				nodeData.ColorIndex = Convert.ToInt32(obj, CultureInfo.InvariantCulture);
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
				if (!dict.TryGetValue("IsGlobal", ref obj2) || !Convert.ToBoolean(obj2, CultureInfo.InvariantCulture))
				{
					sharedVariable = variableSource.GetVariable(obj as string);
				}
				else
				{
					if (JSONDeserialization.globalVariables == null)
					{
						JSONDeserialization.globalVariables = GlobalVariables.Instance;
					}
					if (JSONDeserialization.globalVariables != null)
					{
						sharedVariable = JSONDeserialization.globalVariables.GetVariable(obj as string);
					}
				}
			}
			Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(dict["Type"] as string);
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
					sharedVariable.IsShared = Convert.ToBoolean(obj3, CultureInfo.InvariantCulture);
				}
				if (dict.TryGetValue("IsGlobal", ref obj3))
				{
					sharedVariable.IsGlobal = Convert.ToBoolean(obj3, CultureInfo.InvariantCulture);
				}
				if (dict.TryGetValue("NetworkSync", ref obj3))
				{
					sharedVariable.NetworkSync = Convert.ToBoolean(obj3, CultureInfo.InvariantCulture);
				}
				if (!sharedVariable.IsGlobal && dict.TryGetValue("PropertyMapping", ref obj3))
				{
					sharedVariable.PropertyMapping = (obj3 as string);
					if (dict.TryGetValue("PropertyMappingOwner", ref obj3))
					{
						sharedVariable.PropertyMappingOwner = (JSONDeserialization.IndexToUnityObject(Convert.ToInt32(obj3, CultureInfo.InvariantCulture), unityObjects) as GameObject);
					}
					sharedVariable.InitializePropertyMapping(variableSource as BehaviorSource);
				}
				if (!flag)
				{
					sharedVariable.IsShared = true;
				}
				JSONDeserialization.DeserializeObject(null, sharedVariable, dict, variableSource, unityObjects);
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
				string text = (!JSONDeserialization.updatedSerialization) ? (allFields[i].FieldType.Name.GetHashCode() + allFields[i].Name.GetHashCode()).ToString() : (allFields[i].FieldType.Name + allFields[i].Name);
				object obj2;
				if (dict.TryGetValue(text, ref obj2))
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
								if (JSONDeserialization.taskIDs != null)
								{
									List<int> list2 = new List<int>();
									for (int j = 0; j < list.Count; j++)
									{
										list2.Add(Convert.ToInt32(list[j], CultureInfo.InvariantCulture));
									}
									JSONDeserialization.taskIDs.Add(new JSONDeserialization.TaskField(task, allFields[i]), list2);
								}
							}
							else if (allFields[i].FieldType.IsArray)
							{
								Array array = Array.CreateInstance(type, list.Count);
								for (int k = 0; k < list.Count; k++)
								{
									if (list[k] == null)
									{
										array.SetValue(null, k);
									}
									else
									{
										array.SetValue(JSONDeserialization.ValueToObject(task, type, list[k], variableSource, unityObjects), k);
									}
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
									if (list[l] == null)
									{
										list3.Add(null);
									}
									else
									{
										list3.Add(JSONDeserialization.ValueToObject(task, type, list[l], variableSource, unityObjects));
									}
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
								Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(dictionary["Type"] as string);
								if (typeWithinAssembly != null)
								{
									Task task2 = TaskUtility.CreateInstance(typeWithinAssembly) as Task;
									JSONDeserialization.DeserializeObject(task2, task2, dictionary, variableSource, unityObjects);
									allFields[i].SetValue(task, task2);
								}
							}
							else if (JSONDeserialization.taskIDs != null)
							{
								List<int> list4 = new List<int>();
								list4.Add(Convert.ToInt32(obj2, CultureInfo.InvariantCulture));
								JSONDeserialization.taskIDs.Add(new JSONDeserialization.TaskField(task, allFields[i]), list4);
							}
						}
						else
						{
							allFields[i].SetValue(obj, JSONDeserialization.ValueToObject(task, fieldType, obj2, variableSource, unityObjects));
						}
					}
				}
				else if (typeof(SharedVariable).IsAssignableFrom(allFields[i].FieldType) && !allFields[i].FieldType.IsAbstract)
				{
					if (dict.TryGetValue((allFields[i].FieldType.Name.GetHashCode() + allFields[i].Name.GetHashCode()).ToString(), ref obj2))
					{
						SharedVariable sharedVariable = TaskUtility.CreateInstance(allFields[i].FieldType) as SharedVariable;
						sharedVariable.SetValue(JSONDeserialization.ValueToObject(task, allFields[i].FieldType, obj2, variableSource, unityObjects));
						allFields[i].SetValue(obj, sharedVariable);
					}
					else
					{
						SharedVariable sharedVariable2 = TaskUtility.CreateInstance(allFields[i].FieldType) as SharedVariable;
						SharedVariable sharedVariable3 = allFields[i].GetValue(obj) as SharedVariable;
						if (sharedVariable3 != null)
						{
							sharedVariable2.SetValue(sharedVariable3.GetValue());
						}
						allFields[i].SetValue(obj, sharedVariable2);
					}
				}
			}
		}

		private static object ValueToObject(Task task, Type type, object obj, IVariableSource variableSource, List<Object> unityObjects)
		{
			if (typeof(SharedVariable).IsAssignableFrom(type))
			{
				SharedVariable sharedVariable = JSONDeserialization.DeserializeSharedVariable(obj as Dictionary<string, object>, variableSource, false, unityObjects);
				if (sharedVariable == null)
				{
					sharedVariable = (TaskUtility.CreateInstance(type) as SharedVariable);
				}
				return sharedVariable;
			}
			if (type.Equals(typeof(Object)) || type.IsSubclassOf(typeof(Object)))
			{
				return JSONDeserialization.IndexToUnityObject(Convert.ToInt32(obj, CultureInfo.InvariantCulture), unityObjects);
			}
			if (!type.IsPrimitive)
			{
				if (!type.Equals(typeof(string)))
				{
					goto IL_B5;
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
			IL_B5:
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
				return JSONDeserialization.StringToVector2((string)obj);
			}
			if (type.Equals(typeof(Vector3)))
			{
				return JSONDeserialization.StringToVector3((string)obj);
			}
			if (type.Equals(typeof(Vector4)))
			{
				return JSONDeserialization.StringToVector4((string)obj);
			}
			if (type.Equals(typeof(Quaternion)))
			{
				return JSONDeserialization.StringToQuaternion((string)obj);
			}
			if (type.Equals(typeof(Matrix4x4)))
			{
				return JSONDeserialization.StringToMatrix4x4((string)obj);
			}
			if (type.Equals(typeof(Color)))
			{
				return JSONDeserialization.StringToColor((string)obj);
			}
			if (type.Equals(typeof(Rect)))
			{
				return JSONDeserialization.StringToRect((string)obj);
			}
			if (type.Equals(typeof(LayerMask)))
			{
				return JSONDeserialization.ValueToLayerMask(Convert.ToInt32(obj, CultureInfo.InvariantCulture));
			}
			if (type.Equals(typeof(AnimationCurve)))
			{
				return JSONDeserialization.ValueToAnimationCurve((Dictionary<string, object>)obj);
			}
			object obj2 = TaskUtility.CreateInstance(type);
			JSONDeserialization.DeserializeObject(task, obj2, obj as Dictionary<string, object>, variableSource, unityObjects);
			return obj2;
		}

		private static Vector2 StringToVector2(string vector2String)
		{
			string[] array = vector2String.Substring(1, vector2String.Length - 2).Split(new char[]
			{
				','
			});
			return new Vector2(float.Parse(array[0], CultureInfo.InvariantCulture), float.Parse(array[1], CultureInfo.InvariantCulture));
		}

		private static Vector3 StringToVector3(string vector3String)
		{
			string[] array = vector3String.Substring(1, vector3String.Length - 2).Split(new char[]
			{
				','
			});
			return new Vector3(float.Parse(array[0], CultureInfo.InvariantCulture), float.Parse(array[1], CultureInfo.InvariantCulture), float.Parse(array[2], CultureInfo.InvariantCulture));
		}

		private static Vector4 StringToVector4(string vector4String)
		{
			string[] array = vector4String.Substring(1, vector4String.Length - 2).Split(new char[]
			{
				','
			});
			return new Vector4(float.Parse(array[0], CultureInfo.InvariantCulture), float.Parse(array[1], CultureInfo.InvariantCulture), float.Parse(array[2], CultureInfo.InvariantCulture), float.Parse(array[3], CultureInfo.InvariantCulture));
		}

		private static Quaternion StringToQuaternion(string quaternionString)
		{
			string[] array = quaternionString.Substring(1, quaternionString.Length - 2).Split(new char[]
			{
				','
			});
			return new Quaternion(float.Parse(array[0]), float.Parse(array[1], CultureInfo.InvariantCulture), float.Parse(array[2], CultureInfo.InvariantCulture), float.Parse(array[3], CultureInfo.InvariantCulture));
		}

		private static Matrix4x4 StringToMatrix4x4(string matrixString)
		{
			string[] array = matrixString.Split(null);
			return new Matrix4x4
			{
				m00 = float.Parse(array[0], CultureInfo.InvariantCulture),
				m01 = float.Parse(array[1], CultureInfo.InvariantCulture),
				m02 = float.Parse(array[2], CultureInfo.InvariantCulture),
				m03 = float.Parse(array[3], CultureInfo.InvariantCulture),
				m10 = float.Parse(array[4], CultureInfo.InvariantCulture),
				m11 = float.Parse(array[5], CultureInfo.InvariantCulture),
				m12 = float.Parse(array[6], CultureInfo.InvariantCulture),
				m13 = float.Parse(array[7], CultureInfo.InvariantCulture),
				m20 = float.Parse(array[8], CultureInfo.InvariantCulture),
				m21 = float.Parse(array[9], CultureInfo.InvariantCulture),
				m22 = float.Parse(array[10], CultureInfo.InvariantCulture),
				m23 = float.Parse(array[11], CultureInfo.InvariantCulture),
				m30 = float.Parse(array[12], CultureInfo.InvariantCulture),
				m31 = float.Parse(array[13], CultureInfo.InvariantCulture),
				m32 = float.Parse(array[14], CultureInfo.InvariantCulture),
				m33 = float.Parse(array[15], CultureInfo.InvariantCulture)
			};
		}

		private static Color StringToColor(string colorString)
		{
			string[] array = colorString.Substring(5, colorString.Length - 6).Split(new char[]
			{
				','
			});
			return new Color(float.Parse(array[0], CultureInfo.InvariantCulture), float.Parse(array[1], CultureInfo.InvariantCulture), float.Parse(array[2], CultureInfo.InvariantCulture), float.Parse(array[3], CultureInfo.InvariantCulture));
		}

		private static Rect StringToRect(string rectString)
		{
			string[] array = rectString.Substring(1, rectString.Length - 2).Split(new char[]
			{
				','
			});
			return new Rect(float.Parse(array[0].Substring(2, array[0].Length - 2), CultureInfo.InvariantCulture), float.Parse(array[1].Substring(3, array[1].Length - 3), CultureInfo.InvariantCulture), float.Parse(array[2].Substring(7, array[2].Length - 7), CultureInfo.InvariantCulture), float.Parse(array[3].Substring(8, array[3].Length - 8), CultureInfo.InvariantCulture));
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
					Keyframe keyframe = new Keyframe((float)Convert.ChangeType(list2[0], typeof(float), CultureInfo.InvariantCulture), (float)Convert.ChangeType(list2[1], typeof(float), CultureInfo.InvariantCulture), (float)Convert.ChangeType(list2[2], typeof(float), CultureInfo.InvariantCulture), (float)Convert.ChangeType(list2[3], typeof(float), CultureInfo.InvariantCulture));
					keyframe.tangentMode=(int)Convert.ChangeType(list2[4], typeof(int));
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
			return unityObjects[index];
		}
	}
}
