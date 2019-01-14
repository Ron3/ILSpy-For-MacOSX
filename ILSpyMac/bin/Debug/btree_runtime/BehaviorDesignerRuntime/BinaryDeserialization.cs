using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

using Object = UnityEngine.Object;

public static class BinaryDeserialization
{
	private class ObjectFieldMap
	{
		public object obj;

		public FieldInfo fieldInfo;

		public ObjectFieldMap(object o, FieldInfo f)
		{
			this.obj = o;
			this.fieldInfo = f;
		}
	}

	private class ObjectFieldMapComparer : IEqualityComparer<BinaryDeserialization.ObjectFieldMap>
	{
		public bool Equals(BinaryDeserialization.ObjectFieldMap a, BinaryDeserialization.ObjectFieldMap b)
		{
			return !object.ReferenceEquals(a, null) && !object.ReferenceEquals(b, null) && a.obj.Equals(b.obj) && a.fieldInfo.Equals(b.fieldInfo);
		}

		public int GetHashCode(BinaryDeserialization.ObjectFieldMap a)
		{
			return (a == null) ? 0 : (a.obj.ToString().GetHashCode() + a.fieldInfo.ToString().GetHashCode());
		}
	}

	private static GlobalVariables globalVariables;

	private static Dictionary<BinaryDeserialization.ObjectFieldMap, List<int>> taskIDs;

	private static SHA1 shaHash;

	private static bool updatedSerialization;

	private static bool shaHashSerialization;

	private static bool strHashSerialization;

	public static void Load(BehaviorSource behaviorSource)
	{
		BinaryDeserialization.Load(behaviorSource.TaskData, behaviorSource);
	}

	public static void Load(TaskSerializationData taskData, BehaviorSource behaviorSource)
	{
		if (taskData != null && string.IsNullOrEmpty(taskData.Version))
		{
			BinaryDeserializationDeprecated.Load(taskData, behaviorSource);
			return;
		}
		behaviorSource.EntryTask = null;
		behaviorSource.RootTask = null;
		behaviorSource.DetachedTasks = null;
		behaviorSource.Variables = null;
		FieldSerializationData fieldSerializationData;
		if (taskData == null || (fieldSerializationData = taskData.fieldSerializationData).byteData == null || fieldSerializationData.byteData.get_Count() == 0)
		{
			return;
		}
		fieldSerializationData.byteDataArray = fieldSerializationData.byteData.ToArray();
		BinaryDeserialization.taskIDs = null;
		Version version = new Version(taskData.Version);
		BinaryDeserialization.updatedSerialization = (version.CompareTo(new Version("1.5.7")) >= 0);
		BinaryDeserialization.shaHashSerialization = (BinaryDeserialization.strHashSerialization = false);
		if (BinaryDeserialization.updatedSerialization)
		{
			BinaryDeserialization.shaHashSerialization = (version.CompareTo(new Version("1.5.9")) >= 0);
			if (BinaryDeserialization.shaHashSerialization)
			{
				BinaryDeserialization.strHashSerialization = (version.CompareTo(new Version("1.5.11")) >= 0);
			}
		}
		if (taskData.variableStartIndex != null)
		{
			List<SharedVariable> list = new List<SharedVariable>();
			Dictionary<int, int> dictionary = ObjectPool.Get<Dictionary<int, int>>();
			for (int i = 0; i < taskData.variableStartIndex.get_Count(); i++)
			{
				int num = taskData.variableStartIndex.get_Item(i);
				int num2;
				if (i + 1 < taskData.variableStartIndex.get_Count())
				{
					num2 = taskData.variableStartIndex.get_Item(i + 1);
				}
				else if (taskData.startIndex != null && taskData.startIndex.get_Count() > 0)
				{
					num2 = taskData.startIndex.get_Item(0);
				}
				else
				{
					num2 = fieldSerializationData.startIndex.get_Count();
				}
				dictionary.Clear();
				for (int j = num; j < num2; j++)
				{
					dictionary.Add(fieldSerializationData.fieldNameHash.get_Item(j), fieldSerializationData.startIndex.get_Item(j));
				}
				SharedVariable sharedVariable = BinaryDeserialization.BytesToSharedVariable(fieldSerializationData, dictionary, fieldSerializationData.byteDataArray, taskData.variableStartIndex.get_Item(i), behaviorSource, false, 0);
				if (sharedVariable != null)
				{
					list.Add(sharedVariable);
				}
			}
			ObjectPool.Return<Dictionary<int, int>>(dictionary);
			behaviorSource.Variables = list;
		}
		List<Task> list2 = new List<Task>();
		if (taskData.types != null)
		{
			for (int k = 0; k < taskData.types.get_Count(); k++)
			{
				BinaryDeserialization.LoadTask(taskData, fieldSerializationData, ref list2, ref behaviorSource);
			}
		}
		if (taskData.parentIndex.get_Count() != list2.get_Count())
		{
			Debug.LogError("Deserialization Error: parent index count does not match task list count");
			return;
		}
		for (int l = 0; l < taskData.parentIndex.get_Count(); l++)
		{
			if (taskData.parentIndex.get_Item(l) == -1)
			{
				if (behaviorSource.EntryTask == null)
				{
					behaviorSource.EntryTask = list2.get_Item(l);
				}
				else
				{
					if (behaviorSource.DetachedTasks == null)
					{
						behaviorSource.DetachedTasks = new List<Task>();
					}
					behaviorSource.DetachedTasks.Add(list2.get_Item(l));
				}
			}
			else if (taskData.parentIndex.get_Item(l) == 0)
			{
				behaviorSource.RootTask = list2.get_Item(l);
			}
			else if (taskData.parentIndex.get_Item(l) != -1)
			{
				ParentTask parentTask = list2.get_Item(taskData.parentIndex.get_Item(l)) as ParentTask;
				if (parentTask != null)
				{
					int index = (parentTask.Children != null) ? parentTask.Children.get_Count() : 0;
					parentTask.AddChild(list2.get_Item(l), index);
				}
			}
		}
		if (BinaryDeserialization.taskIDs != null)
		{
			using (Dictionary<BinaryDeserialization.ObjectFieldMap, List<int>>.KeyCollection.Enumerator enumerator = BinaryDeserialization.taskIDs.get_Keys().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					BinaryDeserialization.ObjectFieldMap current = enumerator.get_Current();
					List<int> list3 = BinaryDeserialization.taskIDs.get_Item(current);
					Type fieldType = current.fieldInfo.get_FieldType();
					if (typeof(IList).IsAssignableFrom(fieldType))
					{
						if (fieldType.get_IsArray())
						{
							Type elementType = fieldType.GetElementType();
							int num3 = 0;
							for (int m = 0; m < list3.get_Count(); m++)
							{
								Task task = list2.get_Item(list3.get_Item(m));
								if (elementType.IsAssignableFrom(task.GetType()))
								{
									num3++;
								}
							}
							int num4 = 0;
							Array array = Array.CreateInstance(elementType, num3);
							for (int n = 0; n < array.get_Length(); n++)
							{
								Task task2 = list2.get_Item(list3.get_Item(n));
								if (elementType.IsAssignableFrom(task2.GetType()))
								{
									array.SetValue(task2, num4);
									num4++;
								}
							}
							current.fieldInfo.SetValue(current.obj, array);
						}
						else
						{
							Type type = fieldType.GetGenericArguments()[0];
							IList list4 = TaskUtility.CreateInstance(typeof(List).MakeGenericType(new Type[]
							{
								type
							})) as IList;
							for (int num5 = 0; num5 < list3.get_Count(); num5++)
							{
								Task task3 = list2.get_Item(list3.get_Item(num5));
								if (type.IsAssignableFrom(task3.GetType()))
								{
									list4.Add(task3);
								}
							}
							current.fieldInfo.SetValue(current.obj, list4);
						}
					}
					else
					{
						current.fieldInfo.SetValue(current.obj, list2.get_Item(list3.get_Item(0)));
					}
				}
			}
		}
	}

	public static void Load(GlobalVariables globalVariables, string version)
	{
		if (globalVariables == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(version))
		{
			BinaryDeserializationDeprecated.Load(globalVariables);
			return;
		}
		globalVariables.Variables = null;
		FieldSerializationData fieldSerializationData;
		if (globalVariables.VariableData == null || (fieldSerializationData = globalVariables.VariableData.fieldSerializationData).byteData == null || fieldSerializationData.byteData.get_Count() == 0)
		{
			return;
		}
		if (fieldSerializationData.typeName.get_Count() > 0)
		{
			BinaryDeserializationDeprecated.Load(globalVariables);
			return;
		}
		VariableSerializationData variableData = globalVariables.VariableData;
		fieldSerializationData.byteDataArray = fieldSerializationData.byteData.ToArray();
		Version version2 = new Version(globalVariables.Version);
		BinaryDeserialization.updatedSerialization = (version2.CompareTo(new Version("1.5.7")) >= 0);
		BinaryDeserialization.shaHashSerialization = (BinaryDeserialization.strHashSerialization = false);
		if (BinaryDeserialization.updatedSerialization)
		{
			BinaryDeserialization.shaHashSerialization = (version2.CompareTo(new Version("1.5.9")) >= 0);
			if (BinaryDeserialization.shaHashSerialization)
			{
				BinaryDeserialization.strHashSerialization = (version2.CompareTo(new Version("1.5.11")) >= 0);
			}
		}
		if (variableData.variableStartIndex != null)
		{
			List<SharedVariable> list = new List<SharedVariable>();
			Dictionary<int, int> dictionary = ObjectPool.Get<Dictionary<int, int>>();
			for (int i = 0; i < variableData.variableStartIndex.get_Count(); i++)
			{
				int num = variableData.variableStartIndex.get_Item(i);
				int num2;
				if (i + 1 < variableData.variableStartIndex.get_Count())
				{
					num2 = variableData.variableStartIndex.get_Item(i + 1);
				}
				else
				{
					num2 = fieldSerializationData.startIndex.get_Count();
				}
				dictionary.Clear();
				for (int j = num; j < num2; j++)
				{
					dictionary.Add(fieldSerializationData.fieldNameHash.get_Item(j), fieldSerializationData.startIndex.get_Item(j));
				}
				SharedVariable sharedVariable = BinaryDeserialization.BytesToSharedVariable(fieldSerializationData, dictionary, fieldSerializationData.byteDataArray, variableData.variableStartIndex.get_Item(i), globalVariables, false, 0);
				if (sharedVariable != null)
				{
					list.Add(sharedVariable);
				}
			}
			ObjectPool.Return<Dictionary<int, int>>(dictionary);
			globalVariables.Variables = list;
		}
	}

	public static void LoadTask(TaskSerializationData taskSerializationData, FieldSerializationData fieldSerializationData, ref List<Task> taskList, ref BehaviorSource behaviorSource)
	{
		int count = taskList.get_Count();
		int num = taskSerializationData.startIndex.get_Item(count);
		int num2;
		if (count + 1 < taskSerializationData.startIndex.get_Count())
		{
			num2 = taskSerializationData.startIndex.get_Item(count + 1);
		}
		else
		{
			num2 = fieldSerializationData.startIndex.get_Count();
		}
		Dictionary<int, int> dictionary = ObjectPool.Get<Dictionary<int, int>>();
		dictionary.Clear();
		for (int i = num; i < num2; i++)
		{
			if (!dictionary.ContainsKey(fieldSerializationData.fieldNameHash.get_Item(i)))
			{
				dictionary.Add(fieldSerializationData.fieldNameHash.get_Item(i), fieldSerializationData.startIndex.get_Item(i));
			}
		}
		Type type = TaskUtility.GetTypeWithinAssembly(taskSerializationData.types.get_Item(count));
		if (type == null)
		{
			bool flag = false;
			for (int j = 0; j < taskSerializationData.parentIndex.get_Count(); j++)
			{
				if (count == taskSerializationData.parentIndex.get_Item(j))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				type = typeof(UnknownParentTask);
			}
			else
			{
				type = typeof(UnknownTask);
			}
		}
		Task task = TaskUtility.CreateInstance(type) as Task;
		if (task is UnknownTask)
		{
			UnknownTask unknownTask = task as UnknownTask;
			for (int k = num; k < num2; k++)
			{
				unknownTask.fieldNameHash.Add(fieldSerializationData.fieldNameHash.get_Item(k));
				unknownTask.startIndex.Add(fieldSerializationData.startIndex.get_Item(k) - fieldSerializationData.startIndex.get_Item(num));
			}
			for (int l = fieldSerializationData.startIndex.get_Item(num); l <= fieldSerializationData.startIndex.get_Item(num2 - 1); l++)
			{
				unknownTask.dataPosition.Add(fieldSerializationData.dataPosition.get_Item(l) - fieldSerializationData.dataPosition.get_Item(fieldSerializationData.startIndex.get_Item(num)));
			}
			if (count + 1 < taskSerializationData.startIndex.get_Count() && taskSerializationData.startIndex.get_Item(count + 1) < fieldSerializationData.dataPosition.get_Count())
			{
				num2 = fieldSerializationData.dataPosition.get_Item(taskSerializationData.startIndex.get_Item(count + 1));
			}
			else
			{
				num2 = fieldSerializationData.byteData.get_Count();
			}
			for (int m = fieldSerializationData.dataPosition.get_Item(fieldSerializationData.startIndex.get_Item(num)); m < num2; m++)
			{
				unknownTask.byteData.Add(fieldSerializationData.byteData.get_Item(m));
			}
			unknownTask.unityObjects = fieldSerializationData.unityObjects;
		}
		task.Owner = (behaviorSource.Owner.GetObject() as Behavior);
		taskList.Add(task);
		task.ID = (int)BinaryDeserialization.LoadField(fieldSerializationData, dictionary, typeof(int), "ID", 0, null, null, null);
		task.FriendlyName = (string)BinaryDeserialization.LoadField(fieldSerializationData, dictionary, typeof(string), "FriendlyName", 0, null, null, null);
		task.IsInstant = (bool)BinaryDeserialization.LoadField(fieldSerializationData, dictionary, typeof(bool), "IsInstant", 0, null, null, null);
		object obj;
		if ((obj = BinaryDeserialization.LoadField(fieldSerializationData, dictionary, typeof(bool), "Disabled", 0, null, null, null)) != null)
		{
			task.Disabled = (bool)obj;
		}
		BinaryDeserialization.LoadNodeData(fieldSerializationData, dictionary, taskList.get_Item(count));
		if (task.GetType().Equals(typeof(UnknownTask)) || task.GetType().Equals(typeof(UnknownParentTask)))
		{
			if (!task.FriendlyName.Contains("Unknown "))
			{
				task.FriendlyName = string.Format("Unknown {0}", task.FriendlyName);
			}
			task.NodeData.Comment = "Unknown Task. Right click and Replace to locate new task.";
		}
		BinaryDeserialization.LoadFields(fieldSerializationData, dictionary, taskList.get_Item(count), 0, behaviorSource);
		ObjectPool.Return<Dictionary<int, int>>(dictionary);
	}

	private static void LoadNodeData(FieldSerializationData fieldSerializationData, Dictionary<int, int> fieldIndexMap, Task task)
	{
		NodeData nodeData = new NodeData();
		nodeData.Offset = (Vector2)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(Vector2), "NodeDataOffset", 0, null, null, null);
		nodeData.Comment = (string)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "NodeDataComment", 0, null, null, null);
		nodeData.IsBreakpoint = (bool)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "NodeDataIsBreakpoint", 0, null, null, null);
		nodeData.Collapsed = (bool)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "NodeDataCollapsed", 0, null, null, null);
		object obj = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(int), "NodeDataColorIndex", 0, null, null, null);
		if (obj != null)
		{
			nodeData.ColorIndex = (int)obj;
		}
		obj = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(List<string>), "NodeDataWatchedFields", 0, null, null, null);
		if (obj != null)
		{
			nodeData.WatchedFieldNames = new List<string>();
			nodeData.WatchedFields = new List<FieldInfo>();
			IList list = obj as IList;
			for (int i = 0; i < list.get_Count(); i++)
			{
				FieldInfo field = task.GetType().GetField((string)list.get_Item(i), 52);
				if (field != null)
				{
					nodeData.WatchedFieldNames.Add(field.get_Name());
					nodeData.WatchedFields.Add(field);
				}
			}
		}
		task.NodeData = nodeData;
	}

	private static void LoadFields(FieldSerializationData fieldSerializationData, Dictionary<int, int> fieldIndexMap, object obj, int hashPrefix, IVariableSource variableSource)
	{
		FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
		for (int i = 0; i < allFields.Length; i++)
		{
			if (!TaskUtility.HasAttribute(allFields[i], typeof(NonSerializedAttribute)) && ((!allFields[i].get_IsPrivate() && !allFields[i].get_IsFamily()) || TaskUtility.HasAttribute(allFields[i], typeof(SerializeField))) && (!(obj is ParentTask) || !allFields[i].get_Name().Equals("children")))
			{
				object obj2 = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, allFields[i].get_FieldType(), allFields[i].get_Name(), hashPrefix, variableSource, obj, allFields[i]);
				if (obj2 != null && !object.ReferenceEquals(obj2, null) && !obj2.Equals(null))
				{
					allFields[i].SetValue(obj, obj2);
				}
			}
		}
	}

	private static object LoadField(FieldSerializationData fieldSerializationData, Dictionary<int, int> fieldIndexMap, Type fieldType, string fieldName, int hashPrefix, IVariableSource variableSource, object obj = null, FieldInfo fieldInfo = null)
	{
		int num;
		if (BinaryDeserialization.shaHashSerialization)
		{
			num = hashPrefix + (BinaryDeserialization.StringHash(fieldType.get_Name().ToString(), BinaryDeserialization.strHashSerialization) + BinaryDeserialization.StringHash(fieldName, BinaryDeserialization.strHashSerialization));
		}
		else
		{
			num = hashPrefix + (fieldType.get_Name().GetHashCode() + fieldName.GetHashCode());
		}
		int num2;
		if (fieldIndexMap.TryGetValue(num, ref num2))
		{
			object obj2 = null;
			if (typeof(IList).IsAssignableFrom(fieldType))
			{
				int num3 = BinaryDeserialization.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
				if (fieldType.get_IsArray())
				{
					Type elementType = fieldType.GetElementType();
					if (elementType == null)
					{
						return null;
					}
					Array array = Array.CreateInstance(elementType, num3);
					for (int i = 0; i < num3; i++)
					{
						object obj3 = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, elementType, i.ToString(), num / ((!BinaryDeserialization.updatedSerialization) ? 1 : (i + 1)), variableSource, obj, fieldInfo);
						array.SetValue((!object.ReferenceEquals(obj3, null) && !obj3.Equals(null)) ? obj3 : null, i);
					}
					obj2 = array;
				}
				else
				{
					Type type = fieldType;
					while (!type.get_IsGenericType())
					{
						type = type.get_BaseType();
					}
					Type type2 = type.GetGenericArguments()[0];
					IList list;
					if (fieldType.get_IsGenericType())
					{
						list = (TaskUtility.CreateInstance(typeof(List).MakeGenericType(new Type[]
						{
							type2
						})) as IList);
					}
					else
					{
						list = (TaskUtility.CreateInstance(fieldType) as IList);
					}
					for (int j = 0; j < num3; j++)
					{
						object obj4 = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, type2, j.ToString(), num / ((!BinaryDeserialization.updatedSerialization) ? 1 : (j + 1)), variableSource, obj, fieldInfo);
						list.Add((!object.ReferenceEquals(obj4, null) && !obj4.Equals(null)) ? obj4 : null);
					}
					obj2 = list;
				}
			}
			else if (typeof(Task).IsAssignableFrom(fieldType))
			{
				if (fieldInfo != null && TaskUtility.HasAttribute(fieldInfo, typeof(InspectTaskAttribute)))
				{
					string text = BinaryDeserialization.BytesToString(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2), BinaryDeserialization.GetFieldSize(fieldSerializationData, num2));
					if (!string.IsNullOrEmpty(text))
					{
						Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(text);
						if (typeWithinAssembly != null)
						{
							obj2 = TaskUtility.CreateInstance(typeWithinAssembly);
							BinaryDeserialization.LoadFields(fieldSerializationData, fieldIndexMap, obj2, num, variableSource);
						}
					}
				}
				else
				{
					if (BinaryDeserialization.taskIDs == null)
					{
						BinaryDeserialization.taskIDs = new Dictionary<BinaryDeserialization.ObjectFieldMap, List<int>>(new BinaryDeserialization.ObjectFieldMapComparer());
					}
					int num4 = BinaryDeserialization.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
					BinaryDeserialization.ObjectFieldMap objectFieldMap = new BinaryDeserialization.ObjectFieldMap(obj, fieldInfo);
					if (BinaryDeserialization.taskIDs.ContainsKey(objectFieldMap))
					{
						BinaryDeserialization.taskIDs.get_Item(objectFieldMap).Add(num4);
					}
					else
					{
						List<int> list2 = new List<int>();
						list2.Add(num4);
						BinaryDeserialization.taskIDs.Add(objectFieldMap, list2);
					}
				}
			}
			else if (typeof(SharedVariable).IsAssignableFrom(fieldType))
			{
				obj2 = BinaryDeserialization.BytesToSharedVariable(fieldSerializationData, fieldIndexMap, fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2), variableSource, true, num);
			}
			else if (typeof(Object).IsAssignableFrom(fieldType))
			{
				int index = BinaryDeserialization.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
				obj2 = BinaryDeserialization.IndexToUnityObject(index, fieldSerializationData);
			}
			else if (fieldType.Equals(typeof(int)) || fieldType.get_IsEnum())
			{
				obj2 = BinaryDeserialization.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(uint)))
			{
				obj2 = BinaryDeserialization.BytesToUInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(float)))
			{
				obj2 = BinaryDeserialization.BytesToFloat(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(double)))
			{
				obj2 = BinaryDeserialization.BytesToDouble(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(long)))
			{
				obj2 = BinaryDeserialization.BytesToLong(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(bool)))
			{
				obj2 = BinaryDeserialization.BytesToBool(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(string)))
			{
				obj2 = BinaryDeserialization.BytesToString(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2), BinaryDeserialization.GetFieldSize(fieldSerializationData, num2));
			}
			else if (fieldType.Equals(typeof(byte)))
			{
				obj2 = BinaryDeserialization.BytesToByte(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(Vector2)))
			{
				obj2 = BinaryDeserialization.BytesToVector2(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(Vector3)))
			{
				obj2 = BinaryDeserialization.BytesToVector3(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(Vector4)))
			{
				obj2 = BinaryDeserialization.BytesToVector4(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(Quaternion)))
			{
				obj2 = BinaryDeserialization.BytesToQuaternion(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(Color)))
			{
				obj2 = BinaryDeserialization.BytesToColor(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(Rect)))
			{
				obj2 = BinaryDeserialization.BytesToRect(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(Matrix4x4)))
			{
				obj2 = BinaryDeserialization.BytesToMatrix4x4(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(AnimationCurve)))
			{
				obj2 = BinaryDeserialization.BytesToAnimationCurve(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.Equals(typeof(LayerMask)))
			{
				obj2 = BinaryDeserialization.BytesToLayerMask(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num2));
			}
			else if (fieldType.get_IsClass() || (fieldType.get_IsValueType() && !fieldType.get_IsPrimitive()))
			{
				obj2 = TaskUtility.CreateInstance(fieldType);
				BinaryDeserialization.LoadFields(fieldSerializationData, fieldIndexMap, obj2, num, variableSource);
				return obj2;
			}
			return obj2;
		}
		if (fieldType.get_IsAbstract())
		{
			return null;
		}
		if (typeof(SharedVariable).IsAssignableFrom(fieldType))
		{
			SharedVariable sharedVariable = TaskUtility.CreateInstance(fieldType) as SharedVariable;
			SharedVariable sharedVariable2 = fieldInfo.GetValue(obj) as SharedVariable;
			if (sharedVariable2 != null)
			{
				sharedVariable.SetValue(sharedVariable2.GetValue());
			}
			return sharedVariable;
		}
		return null;
	}

	public static int StringHash(string value, bool fastHash)
	{
		if (string.IsNullOrEmpty(value))
		{
			return 0;
		}
		if (fastHash)
		{
			int num = 23;
			int length = value.get_Length();
			for (int i = 0; i < length; i++)
			{
				num = num * 31 + (int)value.get_Chars(i);
			}
			return num;
		}
		byte[] bytes = Encoding.get_UTF8().GetBytes(value);
		if (BinaryDeserialization.shaHash == null)
		{
			BinaryDeserialization.shaHash = new SHA1Managed();
		}
		byte[] array = BinaryDeserialization.shaHash.ComputeHash(bytes);
		return BitConverter.ToInt32(array, 0);
	}

	private static int GetFieldSize(FieldSerializationData fieldSerializationData, int fieldIndex)
	{
		return ((fieldIndex + 1 >= fieldSerializationData.dataPosition.get_Count()) ? fieldSerializationData.byteData.get_Count() : fieldSerializationData.dataPosition.get_Item(fieldIndex + 1)) - fieldSerializationData.dataPosition.get_Item(fieldIndex);
	}

	private static int BytesToInt(byte[] bytes, int dataPosition)
	{
		return BitConverter.ToInt32(bytes, dataPosition);
	}

	private static uint BytesToUInt(byte[] bytes, int dataPosition)
	{
		return BitConverter.ToUInt32(bytes, dataPosition);
	}

	private static float BytesToFloat(byte[] bytes, int dataPosition)
	{
		return BitConverter.ToSingle(bytes, dataPosition);
	}

	private static double BytesToDouble(byte[] bytes, int dataPosition)
	{
		return BitConverter.ToDouble(bytes, dataPosition);
	}

	private static long BytesToLong(byte[] bytes, int dataPosition)
	{
		return BitConverter.ToInt64(bytes, dataPosition);
	}

	private static bool BytesToBool(byte[] bytes, int dataPosition)
	{
		return BitConverter.ToBoolean(bytes, dataPosition);
	}

	private static string BytesToString(byte[] bytes, int dataPosition, int dataSize)
	{
		if (dataSize == 0)
		{
			return string.Empty;
		}
		return Encoding.get_UTF8().GetString(bytes, dataPosition, dataSize);
	}

	private static byte BytesToByte(byte[] bytes, int dataPosition)
	{
		return bytes[dataPosition];
	}

	private static Color BytesToColor(byte[] bytes, int dataPosition)
	{
		Color black = Color.get_black();
		black.r = BitConverter.ToSingle(bytes, dataPosition);
		black.g = BitConverter.ToSingle(bytes, dataPosition + 4);
		black.b = BitConverter.ToSingle(bytes, dataPosition + 8);
		black.a = BitConverter.ToSingle(bytes, dataPosition + 12);
		return black;
	}

	private static Vector2 BytesToVector2(byte[] bytes, int dataPosition)
	{
		Vector2 zero = Vector2.get_zero();
		zero.x = BitConverter.ToSingle(bytes, dataPosition);
		zero.y = BitConverter.ToSingle(bytes, dataPosition + 4);
		return zero;
	}

	private static Vector3 BytesToVector3(byte[] bytes, int dataPosition)
	{
		Vector3 zero = Vector3.get_zero();
		zero.x = BitConverter.ToSingle(bytes, dataPosition);
		zero.y = BitConverter.ToSingle(bytes, dataPosition + 4);
		zero.z = BitConverter.ToSingle(bytes, dataPosition + 8);
		return zero;
	}

	private static Vector4 BytesToVector4(byte[] bytes, int dataPosition)
	{
		Vector4 zero = Vector4.get_zero();
		zero.x = BitConverter.ToSingle(bytes, dataPosition);
		zero.y = BitConverter.ToSingle(bytes, dataPosition + 4);
		zero.z = BitConverter.ToSingle(bytes, dataPosition + 8);
		zero.w = BitConverter.ToSingle(bytes, dataPosition + 12);
		return zero;
	}

	private static Quaternion BytesToQuaternion(byte[] bytes, int dataPosition)
	{
		Quaternion identity = Quaternion.get_identity();
		identity.x = BitConverter.ToSingle(bytes, dataPosition);
		identity.y = BitConverter.ToSingle(bytes, dataPosition + 4);
		identity.z = BitConverter.ToSingle(bytes, dataPosition + 8);
		identity.w = BitConverter.ToSingle(bytes, dataPosition + 12);
		return identity;
	}

	private static Rect BytesToRect(byte[] bytes, int dataPosition)
	{
		Rect result = default(Rect);
		result.set_x(BitConverter.ToSingle(bytes, dataPosition));
		result.set_y(BitConverter.ToSingle(bytes, dataPosition + 4));
		result.set_width(BitConverter.ToSingle(bytes, dataPosition + 8));
		result.set_height(BitConverter.ToSingle(bytes, dataPosition + 12));
		return result;
	}

	private static Matrix4x4 BytesToMatrix4x4(byte[] bytes, int dataPosition)
	{
		Matrix4x4 identity = Matrix4x4.get_identity();
		identity.m00 = BitConverter.ToSingle(bytes, dataPosition);
		identity.m01 = BitConverter.ToSingle(bytes, dataPosition + 4);
		identity.m02 = BitConverter.ToSingle(bytes, dataPosition + 8);
		identity.m03 = BitConverter.ToSingle(bytes, dataPosition + 12);
		identity.m10 = BitConverter.ToSingle(bytes, dataPosition + 16);
		identity.m11 = BitConverter.ToSingle(bytes, dataPosition + 20);
		identity.m12 = BitConverter.ToSingle(bytes, dataPosition + 24);
		identity.m13 = BitConverter.ToSingle(bytes, dataPosition + 28);
		identity.m20 = BitConverter.ToSingle(bytes, dataPosition + 32);
		identity.m21 = BitConverter.ToSingle(bytes, dataPosition + 36);
		identity.m22 = BitConverter.ToSingle(bytes, dataPosition + 40);
		identity.m23 = BitConverter.ToSingle(bytes, dataPosition + 44);
		identity.m30 = BitConverter.ToSingle(bytes, dataPosition + 48);
		identity.m31 = BitConverter.ToSingle(bytes, dataPosition + 52);
		identity.m32 = BitConverter.ToSingle(bytes, dataPosition + 56);
		identity.m33 = BitConverter.ToSingle(bytes, dataPosition + 60);
		return identity;
	}

	private static AnimationCurve BytesToAnimationCurve(byte[] bytes, int dataPosition)
	{
		AnimationCurve animationCurve = new AnimationCurve();
		int num = BitConverter.ToInt32(bytes, dataPosition);
		for (int i = 0; i < num; i++)
		{
			Keyframe keyframe = default(Keyframe);
			keyframe.set_time(BitConverter.ToSingle(bytes, dataPosition + 4));
			keyframe.set_value(BitConverter.ToSingle(bytes, dataPosition + 8));
			keyframe.set_inTangent(BitConverter.ToSingle(bytes, dataPosition + 12));
			keyframe.set_outTangent(BitConverter.ToSingle(bytes, dataPosition + 16));
			keyframe.set_tangentMode(BitConverter.ToInt32(bytes, dataPosition + 20));
			animationCurve.AddKey(keyframe);
			dataPosition += 20;
		}
		animationCurve.set_preWrapMode(BitConverter.ToInt32(bytes, dataPosition + 4));
		animationCurve.set_postWrapMode(BitConverter.ToInt32(bytes, dataPosition + 8));
		return animationCurve;
	}

	private static LayerMask BytesToLayerMask(byte[] bytes, int dataPosition)
	{
		LayerMask result = default(LayerMask);
		result.set_value(BinaryDeserialization.BytesToInt(bytes, dataPosition));
		return result;
	}

	private static Object IndexToUnityObject(int index, FieldSerializationData activeFieldSerializationData)
	{
		if (index < 0 || index >= activeFieldSerializationData.unityObjects.get_Count())
		{
			return null;
		}
		return activeFieldSerializationData.unityObjects.get_Item(index);
	}

	private static SharedVariable BytesToSharedVariable(FieldSerializationData fieldSerializationData, Dictionary<int, int> fieldIndexMap, byte[] bytes, int dataPosition, IVariableSource variableSource, bool fromField, int hashPrefix)
	{
		SharedVariable sharedVariable = null;
		string text = (string)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "Type", hashPrefix, null, null, null);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		string name = (string)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "Name", hashPrefix, null, null, null);
		bool flag = Convert.ToBoolean(BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "IsShared", hashPrefix, null, null, null));
		bool flag2 = Convert.ToBoolean(BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "IsGlobal", hashPrefix, null, null, null));
		if (flag && fromField)
		{
			if (!flag2)
			{
				sharedVariable = variableSource.GetVariable(name);
			}
			else
			{
				if (BinaryDeserialization.globalVariables == null)
				{
					BinaryDeserialization.globalVariables = GlobalVariables.Instance;
				}
				if (BinaryDeserialization.globalVariables != null)
				{
					sharedVariable = BinaryDeserialization.globalVariables.GetVariable(name);
				}
			}
		}
		Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(text);
		if (typeWithinAssembly == null)
		{
			return null;
		}
		bool flag3 = true;
		if (sharedVariable == null || !(flag3 = sharedVariable.GetType().Equals(typeWithinAssembly)))
		{
			sharedVariable = (TaskUtility.CreateInstance(typeWithinAssembly) as SharedVariable);
			sharedVariable.Name = name;
			sharedVariable.IsShared = flag;
			sharedVariable.IsGlobal = flag2;
			sharedVariable.NetworkSync = Convert.ToBoolean(BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "NetworkSync", hashPrefix, null, null, null));
			if (!flag2)
			{
				sharedVariable.PropertyMapping = (string)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "PropertyMapping", hashPrefix, null, null, null);
				sharedVariable.PropertyMappingOwner = (GameObject)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(GameObject), "PropertyMappingOwner", hashPrefix, null, null, null);
				sharedVariable.InitializePropertyMapping(variableSource as BehaviorSource);
			}
			if (!flag3)
			{
				sharedVariable.IsShared = true;
			}
			BinaryDeserialization.LoadFields(fieldSerializationData, fieldIndexMap, sharedVariable, hashPrefix, variableSource);
		}
		return sharedVariable;
	}
}
