using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class BinaryDeserializationDeprecated
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

	private class ObjectFieldMapComparer : IEqualityComparer<BinaryDeserializationDeprecated.ObjectFieldMap>
	{
		public bool Equals(BinaryDeserializationDeprecated.ObjectFieldMap a, BinaryDeserializationDeprecated.ObjectFieldMap b)
		{
			return !object.ReferenceEquals(a, null) && !object.ReferenceEquals(b, null) && a.obj.Equals(b.obj) && a.fieldInfo.Equals(b.fieldInfo);
		}

		public int GetHashCode(BinaryDeserializationDeprecated.ObjectFieldMap a)
		{
			return (a == null) ? 0 : (a.obj.ToString().GetHashCode() + a.fieldInfo.ToString().GetHashCode());
		}
	}

	private static GlobalVariables globalVariables;

	private static Dictionary<BinaryDeserializationDeprecated.ObjectFieldMap, List<int>> taskIDs;

	public static void Load(BehaviorSource behaviorSource)
	{
		BinaryDeserializationDeprecated.Load(behaviorSource.TaskData, behaviorSource);
	}

	public static void Load(TaskSerializationData taskData, BehaviorSource behaviorSource)
	{
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
		BinaryDeserializationDeprecated.taskIDs = null;
		if (taskData.variableStartIndex != null)
		{
			List<SharedVariable> list = new List<SharedVariable>();
			Dictionary<string, int> dictionary = ObjectPool.Get<Dictionary<string, int>>();
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
					dictionary.Add(fieldSerializationData.typeName.get_Item(j), fieldSerializationData.startIndex.get_Item(j));
				}
				SharedVariable sharedVariable = BinaryDeserializationDeprecated.BytesToSharedVariable(fieldSerializationData, dictionary, fieldSerializationData.byteDataArray, taskData.variableStartIndex.get_Item(i), behaviorSource, false, string.Empty);
				if (sharedVariable != null)
				{
					list.Add(sharedVariable);
				}
			}
			ObjectPool.Return<Dictionary<string, int>>(dictionary);
			behaviorSource.Variables = list;
		}
		List<Task> list2 = new List<Task>();
		if (taskData.types != null)
		{
			for (int k = 0; k < taskData.types.get_Count(); k++)
			{
				BinaryDeserializationDeprecated.LoadTask(taskData, fieldSerializationData, ref list2, ref behaviorSource);
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
		if (BinaryDeserializationDeprecated.taskIDs != null)
		{
			using (Dictionary<BinaryDeserializationDeprecated.ObjectFieldMap, List<int>>.KeyCollection.Enumerator enumerator = BinaryDeserializationDeprecated.taskIDs.get_Keys().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					BinaryDeserializationDeprecated.ObjectFieldMap current = enumerator.get_Current();
					List<int> list3 = BinaryDeserializationDeprecated.taskIDs.get_Item(current);
					Type fieldType = current.fieldInfo.get_FieldType();
					if (typeof(IList).IsAssignableFrom(fieldType))
					{
						if (fieldType.get_IsArray())
						{
							Type elementType = fieldType.GetElementType();
							Array array = Array.CreateInstance(elementType, list3.get_Count());
							for (int m = 0; m < array.get_Length(); m++)
							{
								array.SetValue(list2.get_Item(list3.get_Item(m)), m);
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
							for (int n = 0; n < list3.get_Count(); n++)
							{
								list4.Add(list2.get_Item(list3.get_Item(n)));
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

	public static void Load(GlobalVariables globalVariables)
	{
		if (globalVariables == null)
		{
			return;
		}
		globalVariables.Variables = null;
		FieldSerializationData fieldSerializationData;
		if (globalVariables.VariableData == null || (fieldSerializationData = globalVariables.VariableData.fieldSerializationData).byteData == null || fieldSerializationData.byteData.get_Count() == 0)
		{
			return;
		}
		VariableSerializationData variableData = globalVariables.VariableData;
		fieldSerializationData.byteDataArray = fieldSerializationData.byteData.ToArray();
		if (variableData.variableStartIndex != null)
		{
			List<SharedVariable> list = new List<SharedVariable>();
			Dictionary<string, int> dictionary = ObjectPool.Get<Dictionary<string, int>>();
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
					dictionary.Add(fieldSerializationData.typeName.get_Item(j), fieldSerializationData.startIndex.get_Item(j));
				}
				SharedVariable sharedVariable = BinaryDeserializationDeprecated.BytesToSharedVariable(fieldSerializationData, dictionary, fieldSerializationData.byteDataArray, variableData.variableStartIndex.get_Item(i), globalVariables, false, string.Empty);
				if (sharedVariable != null)
				{
					list.Add(sharedVariable);
				}
			}
			ObjectPool.Return<Dictionary<string, int>>(dictionary);
			globalVariables.Variables = list;
		}
	}

	private static void LoadTask(TaskSerializationData taskSerializationData, FieldSerializationData fieldSerializationData, ref List<Task> taskList, ref BehaviorSource behaviorSource)
	{
		int count = taskList.get_Count();
		Type type = TaskUtility.GetTypeWithinAssembly(taskSerializationData.types.get_Item(count));
		if (type == null)
		{
			bool flag = false;
			for (int i = 0; i < taskSerializationData.parentIndex.get_Count(); i++)
			{
				if (count == taskSerializationData.parentIndex.get_Item(i))
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
		task.Owner = (behaviorSource.Owner.GetObject() as Behavior);
		taskList.Add(task);
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
		Dictionary<string, int> dictionary = ObjectPool.Get<Dictionary<string, int>>();
		dictionary.Clear();
		for (int j = num; j < num2; j++)
		{
			if (!dictionary.ContainsKey(fieldSerializationData.typeName.get_Item(j)))
			{
				dictionary.Add(fieldSerializationData.typeName.get_Item(j), fieldSerializationData.startIndex.get_Item(j));
			}
		}
		task.ID = (int)BinaryDeserializationDeprecated.LoadField(fieldSerializationData, dictionary, typeof(int), "ID", null, null, null);
		task.FriendlyName = (string)BinaryDeserializationDeprecated.LoadField(fieldSerializationData, dictionary, typeof(string), "FriendlyName", null, null, null);
		task.IsInstant = (bool)BinaryDeserializationDeprecated.LoadField(fieldSerializationData, dictionary, typeof(bool), "IsInstant", null, null, null);
		object obj;
		if ((obj = BinaryDeserializationDeprecated.LoadField(fieldSerializationData, dictionary, typeof(bool), "Disabled", null, null, null)) != null)
		{
			task.Disabled = (bool)obj;
		}
		BinaryDeserializationDeprecated.LoadNodeData(fieldSerializationData, dictionary, taskList.get_Item(count));
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
		BinaryDeserializationDeprecated.LoadFields(fieldSerializationData, dictionary, taskList.get_Item(count), string.Empty, behaviorSource);
		ObjectPool.Return<Dictionary<string, int>>(dictionary);
	}

	private static void LoadNodeData(FieldSerializationData fieldSerializationData, Dictionary<string, int> fieldIndexMap, Task task)
	{
		NodeData nodeData = new NodeData();
		nodeData.Offset = (Vector2)BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof(Vector2), "NodeDataOffset", null, null, null);
		nodeData.Comment = (string)BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "NodeDataComment", null, null, null);
		nodeData.IsBreakpoint = (bool)BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "NodeDataIsBreakpoint", null, null, null);
		nodeData.Collapsed = (bool)BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "NodeDataCollapsed", null, null, null);
		object obj = BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof(int), "NodeDataColorIndex", null, null, null);
		if (obj != null)
		{
			nodeData.ColorIndex = (int)obj;
		}
		obj = BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof(List<string>), "NodeDataWatchedFields", null, null, null);
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

	private static void LoadFields(FieldSerializationData fieldSerializationData, Dictionary<string, int> fieldIndexMap, object obj, string namePrefix, IVariableSource variableSource)
	{
		FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
		for (int i = 0; i < allFields.Length; i++)
		{
			if (!TaskUtility.HasAttribute(allFields[i], typeof(NonSerializedAttribute)) && ((!allFields[i].get_IsPrivate() && !allFields[i].get_IsFamily()) || TaskUtility.HasAttribute(allFields[i], typeof(SerializeField))) && (!(obj is ParentTask) || !allFields[i].get_Name().Equals("children")))
			{
				object obj2 = BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, allFields[i].get_FieldType(), namePrefix + allFields[i].get_Name(), variableSource, obj, allFields[i]);
				if (obj2 != null && !object.ReferenceEquals(obj2, null) && !obj2.Equals(null))
				{
					allFields[i].SetValue(obj, obj2);
				}
			}
		}
	}

	private static object LoadField(FieldSerializationData fieldSerializationData, Dictionary<string, int> fieldIndexMap, Type fieldType, string fieldName, IVariableSource variableSource, object obj = null, FieldInfo fieldInfo = null)
	{
		string text = fieldType.get_Name() + fieldName;
		int num;
		if (fieldIndexMap.TryGetValue(text, ref num))
		{
			object obj2 = null;
			if (typeof(IList).IsAssignableFrom(fieldType))
			{
				int num2 = BinaryDeserializationDeprecated.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
				if (fieldType.get_IsArray())
				{
					Type elementType = fieldType.GetElementType();
					if (elementType == null)
					{
						return null;
					}
					Array array = Array.CreateInstance(elementType, num2);
					for (int i = 0; i < num2; i++)
					{
						object obj3 = BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, elementType, text + i, variableSource, obj, fieldInfo);
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
					for (int j = 0; j < num2; j++)
					{
						object obj4 = BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, type2, text + j, variableSource, obj, fieldInfo);
						list.Add((!object.ReferenceEquals(obj4, null) && !obj4.Equals(null)) ? obj4 : null);
					}
					obj2 = list;
				}
			}
			else if (typeof(Task).IsAssignableFrom(fieldType))
			{
				if (fieldInfo != null && TaskUtility.HasAttribute(fieldInfo, typeof(InspectTaskAttribute)))
				{
					string text2 = BinaryDeserializationDeprecated.BytesToString(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num), BinaryDeserializationDeprecated.GetFieldSize(fieldSerializationData, num));
					if (!string.IsNullOrEmpty(text2))
					{
						Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(text2);
						if (typeWithinAssembly != null)
						{
							obj2 = TaskUtility.CreateInstance(typeWithinAssembly);
							BinaryDeserializationDeprecated.LoadFields(fieldSerializationData, fieldIndexMap, obj2, text, variableSource);
						}
					}
				}
				else
				{
					if (BinaryDeserializationDeprecated.taskIDs == null)
					{
						BinaryDeserializationDeprecated.taskIDs = new Dictionary<BinaryDeserializationDeprecated.ObjectFieldMap, List<int>>(new BinaryDeserializationDeprecated.ObjectFieldMapComparer());
					}
					int num3 = BinaryDeserializationDeprecated.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
					BinaryDeserializationDeprecated.ObjectFieldMap objectFieldMap = new BinaryDeserializationDeprecated.ObjectFieldMap(obj, fieldInfo);
					if (BinaryDeserializationDeprecated.taskIDs.ContainsKey(objectFieldMap))
					{
						BinaryDeserializationDeprecated.taskIDs.get_Item(objectFieldMap).Add(num3);
					}
					else
					{
						List<int> list2 = new List<int>();
						list2.Add(num3);
						BinaryDeserializationDeprecated.taskIDs.Add(objectFieldMap, list2);
					}
				}
			}
			else if (typeof(SharedVariable).IsAssignableFrom(fieldType))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToSharedVariable(fieldSerializationData, fieldIndexMap, fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num), variableSource, true, text);
			}
			else if (typeof(Object).IsAssignableFrom(fieldType))
			{
				int index = BinaryDeserializationDeprecated.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
				obj2 = BinaryDeserializationDeprecated.IndexToUnityObject(index, fieldSerializationData);
			}
			else if (fieldType.Equals(typeof(int)) || fieldType.get_IsEnum())
			{
				obj2 = BinaryDeserializationDeprecated.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(uint)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToUInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(float)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToFloat(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(double)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToDouble(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(long)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToLong(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(bool)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToBool(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(string)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToString(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num), BinaryDeserializationDeprecated.GetFieldSize(fieldSerializationData, num));
			}
			else if (fieldType.Equals(typeof(byte)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToByte(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(Vector2)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToVector2(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(Vector3)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToVector3(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(Vector4)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToVector4(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(Quaternion)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToQuaternion(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(Color)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToColor(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(Rect)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToRect(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(Matrix4x4)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToMatrix4x4(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(AnimationCurve)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToAnimationCurve(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.Equals(typeof(LayerMask)))
			{
				obj2 = BinaryDeserializationDeprecated.BytesToLayerMask(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition.get_Item(num));
			}
			else if (fieldType.get_IsClass() || (fieldType.get_IsValueType() && !fieldType.get_IsPrimitive()))
			{
				obj2 = TaskUtility.CreateInstance(fieldType);
				BinaryDeserializationDeprecated.LoadFields(fieldSerializationData, fieldIndexMap, obj2, text, variableSource);
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
			return TaskUtility.CreateInstance(fieldType);
		}
		return null;
	}

	private static int GetFieldSize(FieldSerializationData fieldSerializationData, int fieldIndex)
	{
		return ((fieldIndex + 1 >= fieldSerializationData.dataPosition.get_Count()) ? fieldSerializationData.byteData.get_Count() : fieldSerializationData.dataPosition.get_Item(fieldIndex + 1)) - fieldSerializationData.dataPosition.get_Item(fieldIndex);
	}

	private static int BytesToInt(byte[] bytes, int dataPosition)
	{
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes, dataPosition, 4);
		}
		return BitConverter.ToInt32(bytes, dataPosition);
	}

	private static uint BytesToUInt(byte[] bytes, int dataPosition)
	{
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes, dataPosition, 4);
		}
		return BitConverter.ToUInt32(bytes, dataPosition);
	}

	private static float BytesToFloat(byte[] bytes, int dataPosition)
	{
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes, dataPosition, 4);
		}
		return BitConverter.ToSingle(bytes, dataPosition);
	}

	private static double BytesToDouble(byte[] bytes, int dataPosition)
	{
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes, dataPosition, 8);
		}
		return BitConverter.ToDouble(bytes, dataPosition);
	}

	private static long BytesToLong(byte[] bytes, int dataPosition)
	{
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes, dataPosition, 8);
		}
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
		result.set_value(BinaryDeserializationDeprecated.BytesToInt(bytes, dataPosition));
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

	private static SharedVariable BytesToSharedVariable(FieldSerializationData fieldSerializationData, Dictionary<string, int> fieldIndexMap, byte[] bytes, int dataPosition, IVariableSource variableSource, bool fromField, string namePrefix)
	{
		SharedVariable sharedVariable = null;
		string text = (string)BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof(string), namePrefix + "Type", null, null, null);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		string name = (string)BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof(string), namePrefix + "Name", null, null, null);
		bool flag = Convert.ToBoolean(BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), namePrefix + "IsShared", null, null, null));
		bool flag2 = Convert.ToBoolean(BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), namePrefix + "IsGlobal", null, null, null));
		if (flag && fromField)
		{
			if (!flag2)
			{
				sharedVariable = variableSource.GetVariable(name);
			}
			else
			{
				if (BinaryDeserializationDeprecated.globalVariables == null)
				{
					BinaryDeserializationDeprecated.globalVariables = GlobalVariables.Instance;
				}
				if (BinaryDeserializationDeprecated.globalVariables != null)
				{
					sharedVariable = BinaryDeserializationDeprecated.globalVariables.GetVariable(name);
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
			sharedVariable.NetworkSync = Convert.ToBoolean(BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), namePrefix + "NetworkSync", null, null, null));
			if (!flag2)
			{
				sharedVariable.PropertyMapping = (string)BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof(string), namePrefix + "PropertyMapping", null, null, null);
				sharedVariable.PropertyMappingOwner = (GameObject)BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof(GameObject), namePrefix + "PropertyMappingOwner", null, null, null);
				sharedVariable.InitializePropertyMapping(variableSource as BehaviorSource);
			}
			if (!flag3)
			{
				sharedVariable.IsShared = true;
			}
			BinaryDeserializationDeprecated.LoadFields(fieldSerializationData, fieldIndexMap, sharedVariable, namePrefix, variableSource);
		}
		return sharedVariable;
	}
}
