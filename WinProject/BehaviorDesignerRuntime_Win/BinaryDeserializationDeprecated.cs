// Decompiled with JetBrains decompiler
// Type: BinaryDeserializationDeprecated
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

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
  private static GlobalVariables globalVariables;
  private static Dictionary<BinaryDeserializationDeprecated.ObjectFieldMap, List<int>> taskIDs;

  public static void Load(BehaviorSource behaviorSource)
  {
    BinaryDeserializationDeprecated.Load(behaviorSource.TaskData, behaviorSource);
  }

  public static void Load(TaskSerializationData taskData, BehaviorSource behaviorSource)
  {
    behaviorSource.EntryTask = (Task) null;
    behaviorSource.RootTask = (Task) null;
    behaviorSource.DetachedTasks = (List<Task>) null;
    behaviorSource.Variables = (List<SharedVariable>) null;
    TaskSerializationData taskSerializationData = taskData;
    FieldSerializationData serializationData;
    if (taskSerializationData == null || (serializationData = taskSerializationData.fieldSerializationData).byteData == null || serializationData.byteData.Count == 0)
      return;
    serializationData.byteDataArray = serializationData.byteData.ToArray();
    BinaryDeserializationDeprecated.taskIDs = (Dictionary<BinaryDeserializationDeprecated.ObjectFieldMap, List<int>>) null;
    if (taskSerializationData.variableStartIndex != null)
    {
      List<SharedVariable> sharedVariableList = new List<SharedVariable>();
      Dictionary<string, int> fieldIndexMap = ObjectPool.Get<Dictionary<string, int>>();
      for (int index1 = 0; index1 < taskSerializationData.variableStartIndex.Count; ++index1)
      {
        int num1 = taskSerializationData.variableStartIndex[index1];
        int num2 = index1 + 1 >= taskSerializationData.variableStartIndex.Count ? (taskSerializationData.startIndex == null || taskSerializationData.startIndex.Count <= 0 ? serializationData.startIndex.Count : taskSerializationData.startIndex[0]) : taskSerializationData.variableStartIndex[index1 + 1];
        fieldIndexMap.Clear();
        for (int index2 = num1; index2 < num2; ++index2)
          fieldIndexMap.Add(serializationData.typeName[index2], serializationData.startIndex[index2]);
        SharedVariable sharedVariable = BinaryDeserializationDeprecated.BytesToSharedVariable(serializationData, fieldIndexMap, serializationData.byteDataArray, taskSerializationData.variableStartIndex[index1], (IVariableSource) behaviorSource, false, string.Empty);
        if (sharedVariable != null)
          sharedVariableList.Add(sharedVariable);
      }
      ObjectPool.Return<Dictionary<string, int>>(fieldIndexMap);
      behaviorSource.Variables = sharedVariableList;
    }
    List<Task> taskList = new List<Task>();
    if (taskSerializationData.types != null)
    {
      for (int index = 0; index < taskSerializationData.types.Count; ++index)
        BinaryDeserializationDeprecated.LoadTask(taskSerializationData, serializationData, ref taskList, ref behaviorSource);
    }
    if (taskSerializationData.parentIndex.Count != taskList.Count)
    {
      Debug.LogError((object) "Deserialization Error: parent index count does not match task list count");
    }
    else
    {
      for (int index1 = 0; index1 < taskSerializationData.parentIndex.Count; ++index1)
      {
        if (taskSerializationData.parentIndex[index1] == -1)
        {
          if (behaviorSource.EntryTask == null)
          {
            behaviorSource.EntryTask = taskList[index1];
          }
          else
          {
            if (behaviorSource.DetachedTasks == null)
              behaviorSource.DetachedTasks = new List<Task>();
            behaviorSource.DetachedTasks.Add(taskList[index1]);
          }
        }
        else if (taskSerializationData.parentIndex[index1] == 0)
          behaviorSource.RootTask = taskList[index1];
        else if (taskSerializationData.parentIndex[index1] != -1)
        {
          ParentTask parentTask = taskList[taskSerializationData.parentIndex[index1]] as ParentTask;
          if (parentTask != null)
          {
            int index2 = parentTask.Children != null ? parentTask.Children.Count : 0;
            parentTask.AddChild(taskList[index1], index2);
          }
        }
      }
      if (BinaryDeserializationDeprecated.taskIDs == null)
        return;
      using (Dictionary<BinaryDeserializationDeprecated.ObjectFieldMap, List<int>>.KeyCollection.Enumerator enumerator = BinaryDeserializationDeprecated.taskIDs.Keys.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          BinaryDeserializationDeprecated.ObjectFieldMap current = enumerator.Current;
          List<int> taskId = BinaryDeserializationDeprecated.taskIDs[current];
          Type fieldType = current.fieldInfo.FieldType;
          if (typeof (IList).IsAssignableFrom(fieldType))
          {
            if (fieldType.IsArray)
            {
              Array instance = Array.CreateInstance(fieldType.GetElementType(), taskId.Count);
              for (int index = 0; index < instance.Length; ++index)
                instance.SetValue((object) taskList[taskId[index]], index);
              current.fieldInfo.SetValue(current.obj, (object) instance);
            }
            else
            {
              IList instance = TaskUtility.CreateInstance(typeof (List<>).MakeGenericType(fieldType.GetGenericArguments()[0])) as IList;
              for (int index = 0; index < taskId.Count; ++index)
                instance.Add((object) taskList[taskId[index]]);
              current.fieldInfo.SetValue(current.obj, (object) instance);
            }
          }
          else
            current.fieldInfo.SetValue(current.obj, (object) taskList[taskId[0]]);
        }
      }
    }
  }

  public static void Load(GlobalVariables globalVariables)
  {
    if (Object.op_Equality((Object) globalVariables, (Object) null))
      return;
    globalVariables.Variables = (List<SharedVariable>) null;
    FieldSerializationData serializationData;
    if (globalVariables.VariableData == null || (serializationData = globalVariables.VariableData.fieldSerializationData).byteData == null || serializationData.byteData.Count == 0)
      return;
    VariableSerializationData variableData = globalVariables.VariableData;
    serializationData.byteDataArray = serializationData.byteData.ToArray();
    if (variableData.variableStartIndex == null)
      return;
    List<SharedVariable> sharedVariableList = new List<SharedVariable>();
    Dictionary<string, int> fieldIndexMap = ObjectPool.Get<Dictionary<string, int>>();
    for (int index1 = 0; index1 < variableData.variableStartIndex.Count; ++index1)
    {
      int num1 = variableData.variableStartIndex[index1];
      int num2 = index1 + 1 >= variableData.variableStartIndex.Count ? serializationData.startIndex.Count : variableData.variableStartIndex[index1 + 1];
      fieldIndexMap.Clear();
      for (int index2 = num1; index2 < num2; ++index2)
        fieldIndexMap.Add(serializationData.typeName[index2], serializationData.startIndex[index2]);
      SharedVariable sharedVariable = BinaryDeserializationDeprecated.BytesToSharedVariable(serializationData, fieldIndexMap, serializationData.byteDataArray, variableData.variableStartIndex[index1], (IVariableSource) globalVariables, false, string.Empty);
      if (sharedVariable != null)
        sharedVariableList.Add(sharedVariable);
    }
    ObjectPool.Return<Dictionary<string, int>>(fieldIndexMap);
    globalVariables.Variables = sharedVariableList;
  }

  private static void LoadTask(
    TaskSerializationData taskSerializationData,
    FieldSerializationData fieldSerializationData,
    ref List<Task> taskList,
    ref BehaviorSource behaviorSource)
  {
    int count = taskList.Count;
    Type t = TaskUtility.GetTypeWithinAssembly(taskSerializationData.types[count]);
    if (t == null)
    {
      bool flag = false;
      for (int index = 0; index < taskSerializationData.parentIndex.Count; ++index)
      {
        if (count == taskSerializationData.parentIndex[index])
        {
          flag = true;
          break;
        }
      }
      t = !flag ? typeof (UnknownTask) : typeof (UnknownParentTask);
    }
    Task instance = TaskUtility.CreateInstance(t) as Task;
    instance.Owner = behaviorSource.Owner.GetObject() as Behavior;
    taskList.Add(instance);
    int num1 = taskSerializationData.startIndex[count];
    int num2 = count + 1 >= taskSerializationData.startIndex.Count ? fieldSerializationData.startIndex.Count : taskSerializationData.startIndex[count + 1];
    Dictionary<string, int> fieldIndexMap = ObjectPool.Get<Dictionary<string, int>>();
    fieldIndexMap.Clear();
    for (int index = num1; index < num2; ++index)
    {
      if (!fieldIndexMap.ContainsKey(fieldSerializationData.typeName[index]))
        fieldIndexMap.Add(fieldSerializationData.typeName[index], fieldSerializationData.startIndex[index]);
    }
    instance.ID = (int) BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (int), "ID", (IVariableSource) null, (object) null, (FieldInfo) null);
    instance.FriendlyName = (string) BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (string), "FriendlyName", (IVariableSource) null, (object) null, (FieldInfo) null);
    instance.IsInstant = (bool) BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (bool), "IsInstant", (IVariableSource) null, (object) null, (FieldInfo) null);
    object obj;
    if ((obj = BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (bool), "Disabled", (IVariableSource) null, (object) null, (FieldInfo) null)) != null)
      instance.Disabled = (bool) obj;
    BinaryDeserializationDeprecated.LoadNodeData(fieldSerializationData, fieldIndexMap, taskList[count]);
    if (instance.GetType().Equals(typeof (UnknownTask)) || instance.GetType().Equals(typeof (UnknownParentTask)))
    {
      if (!instance.FriendlyName.Contains("Unknown "))
        instance.FriendlyName = string.Format("Unknown {0}", (object) instance.FriendlyName);
      if (!instance.NodeData.Comment.Contains("Loaded from an unknown type. Was a task renamed or deleted?"))
        instance.NodeData.Comment = string.Format("Loaded from an unknown type. Was a task renamed or deleted?{0}", !instance.NodeData.Comment.Equals(string.Empty) ? (object) string.Format("\0{0}", (object) instance.NodeData.Comment) : (object) string.Empty);
    }
    BinaryDeserializationDeprecated.LoadFields(fieldSerializationData, fieldIndexMap, (object) taskList[count], string.Empty, (IVariableSource) behaviorSource);
    ObjectPool.Return<Dictionary<string, int>>(fieldIndexMap);
  }

  private static void LoadNodeData(
    FieldSerializationData fieldSerializationData,
    Dictionary<string, int> fieldIndexMap,
    Task task)
  {
    NodeData nodeData = new NodeData();
    nodeData.Offset = (Vector2) BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (Vector2), "NodeDataOffset", (IVariableSource) null, (object) null, (FieldInfo) null);
    nodeData.Comment = (string) BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (string), "NodeDataComment", (IVariableSource) null, (object) null, (FieldInfo) null);
    nodeData.IsBreakpoint = (bool) BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (bool), "NodeDataIsBreakpoint", (IVariableSource) null, (object) null, (FieldInfo) null);
    nodeData.Collapsed = (bool) BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (bool), "NodeDataCollapsed", (IVariableSource) null, (object) null, (FieldInfo) null);
    object obj1 = BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (int), "NodeDataColorIndex", (IVariableSource) null, (object) null, (FieldInfo) null);
    if (obj1 != null)
      nodeData.ColorIndex = (int) obj1;
    object obj2 = BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (List<string>), "NodeDataWatchedFields", (IVariableSource) null, (object) null, (FieldInfo) null);
    if (obj2 != null)
    {
      nodeData.WatchedFieldNames = new List<string>();
      nodeData.WatchedFields = new List<FieldInfo>();
      IList list = obj2 as IList;
      for (int index = 0; index < list.Count; ++index)
      {
        FieldInfo field = task.GetType().GetField((string) list[index], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
          nodeData.WatchedFieldNames.Add(field.Name);
          nodeData.WatchedFields.Add(field);
        }
      }
    }
    task.NodeData = nodeData;
  }

  private static void LoadFields(
    FieldSerializationData fieldSerializationData,
    Dictionary<string, int> fieldIndexMap,
    object obj,
    string namePrefix,
    IVariableSource variableSource)
  {
    FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
    for (int index = 0; index < allFields.Length; ++index)
    {
      if (!TaskUtility.HasAttribute(allFields[index], typeof (NonSerializedAttribute)) && (!allFields[index].IsPrivate && !allFields[index].IsFamily || TaskUtility.HasAttribute(allFields[index], typeof (SerializeField))) && (!(obj is ParentTask) || !allFields[index].Name.Equals("children")))
      {
        object objA = BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, allFields[index].FieldType, namePrefix + allFields[index].Name, variableSource, obj, allFields[index]);
        if (objA != null && !object.ReferenceEquals(objA, (object) null) && !objA.Equals((object) null))
          allFields[index].SetValue(obj, objA);
      }
    }
  }

  private static object LoadField(
    FieldSerializationData fieldSerializationData,
    Dictionary<string, int> fieldIndexMap,
    Type fieldType,
    string fieldName,
    IVariableSource variableSource,
    object obj = null,
    FieldInfo fieldInfo = null)
  {
    string str = fieldType.Name + fieldName;
    int fieldIndex;
    if (!fieldIndexMap.TryGetValue(str, out fieldIndex))
    {
      if (fieldType.IsAbstract)
        return (object) null;
      if (typeof (SharedVariable).IsAssignableFrom(fieldType))
        return TaskUtility.CreateInstance(fieldType);
      return (object) null;
    }
    object obj1 = (object) null;
    if (typeof (IList).IsAssignableFrom(fieldType))
    {
      int length = BinaryDeserializationDeprecated.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
      if (fieldType.IsArray)
      {
        Type elementType = fieldType.GetElementType();
        if (elementType == null)
          return (object) null;
        Array instance = Array.CreateInstance(elementType, length);
        for (int index = 0; index < length; ++index)
        {
          object objA = BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, elementType, str + (object) index, variableSource, obj, fieldInfo);
          instance.SetValue(object.ReferenceEquals(objA, (object) null) || objA.Equals((object) null) ? (object) null : objA, index);
        }
        obj1 = (object) instance;
      }
      else
      {
        Type type = fieldType;
        while (!type.IsGenericType)
          type = type.BaseType;
        Type genericArgument = type.GetGenericArguments()[0];
        IList instance;
        if (fieldType.IsGenericType)
          instance = TaskUtility.CreateInstance(typeof (List<>).MakeGenericType(genericArgument)) as IList;
        else
          instance = TaskUtility.CreateInstance(fieldType) as IList;
        for (int index = 0; index < length; ++index)
        {
          object objA = BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, genericArgument, str + (object) index, variableSource, obj, fieldInfo);
          instance.Add(object.ReferenceEquals(objA, (object) null) || objA.Equals((object) null) ? (object) null : objA);
        }
        obj1 = (object) instance;
      }
    }
    else if (typeof (Task).IsAssignableFrom(fieldType))
    {
      if (fieldInfo != null && TaskUtility.HasAttribute(fieldInfo, typeof (InspectTaskAttribute)))
      {
        string typeName = BinaryDeserializationDeprecated.BytesToString(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex], BinaryDeserializationDeprecated.GetFieldSize(fieldSerializationData, fieldIndex));
        if (!string.IsNullOrEmpty(typeName))
        {
          Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(typeName);
          if (typeWithinAssembly != null)
          {
            obj1 = TaskUtility.CreateInstance(typeWithinAssembly);
            BinaryDeserializationDeprecated.LoadFields(fieldSerializationData, fieldIndexMap, obj1, str, variableSource);
          }
        }
      }
      else
      {
        if (BinaryDeserializationDeprecated.taskIDs == null)
          BinaryDeserializationDeprecated.taskIDs = new Dictionary<BinaryDeserializationDeprecated.ObjectFieldMap, List<int>>((IEqualityComparer<BinaryDeserializationDeprecated.ObjectFieldMap>) new BinaryDeserializationDeprecated.ObjectFieldMapComparer());
        int num = BinaryDeserializationDeprecated.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        BinaryDeserializationDeprecated.ObjectFieldMap key = new BinaryDeserializationDeprecated.ObjectFieldMap(obj, fieldInfo);
        if (BinaryDeserializationDeprecated.taskIDs.ContainsKey(key))
          BinaryDeserializationDeprecated.taskIDs[key].Add(num);
        else
          BinaryDeserializationDeprecated.taskIDs.Add(key, new List<int>()
          {
            num
          });
      }
    }
    else if (typeof (SharedVariable).IsAssignableFrom(fieldType))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToSharedVariable(fieldSerializationData, fieldIndexMap, fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex], variableSource, true, str);
    else if (typeof (Object).IsAssignableFrom(fieldType))
      obj1 = (object) BinaryDeserializationDeprecated.IndexToUnityObject(BinaryDeserializationDeprecated.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]), fieldSerializationData);
    else if (fieldType.Equals(typeof (int)) || fieldType.IsEnum)
      obj1 = (object) BinaryDeserializationDeprecated.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (uint)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToUInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (float)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToFloat(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (double)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToDouble(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (long)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToLong(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (bool)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToBool(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (string)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToString(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex], BinaryDeserializationDeprecated.GetFieldSize(fieldSerializationData, fieldIndex));
    else if (fieldType.Equals(typeof (byte)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToByte(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (Vector2)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToVector2(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (Vector3)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToVector3(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (Vector4)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToVector4(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (Quaternion)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToQuaternion(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (Color)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToColor(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (Rect)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToRect(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (Matrix4x4)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToMatrix4x4(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (AnimationCurve)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToAnimationCurve(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (LayerMask)))
      obj1 = (object) BinaryDeserializationDeprecated.BytesToLayerMask(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.IsClass || fieldType.IsValueType && !fieldType.IsPrimitive)
    {
      object instance = TaskUtility.CreateInstance(fieldType);
      BinaryDeserializationDeprecated.LoadFields(fieldSerializationData, fieldIndexMap, instance, str, variableSource);
      return instance;
    }
    return obj1;
  }

  private static int GetFieldSize(FieldSerializationData fieldSerializationData, int fieldIndex)
  {
    return (fieldIndex + 1 >= fieldSerializationData.dataPosition.Count ? fieldSerializationData.byteData.Count : fieldSerializationData.dataPosition[fieldIndex + 1]) - fieldSerializationData.dataPosition[fieldIndex];
  }

  private static int BytesToInt(byte[] bytes, int dataPosition)
  {
    if (BitConverter.IsLittleEndian)
      Array.Reverse((Array) bytes, dataPosition, 4);
    return BitConverter.ToInt32(bytes, dataPosition);
  }

  private static uint BytesToUInt(byte[] bytes, int dataPosition)
  {
    if (BitConverter.IsLittleEndian)
      Array.Reverse((Array) bytes, dataPosition, 4);
    return BitConverter.ToUInt32(bytes, dataPosition);
  }

  private static float BytesToFloat(byte[] bytes, int dataPosition)
  {
    if (BitConverter.IsLittleEndian)
      Array.Reverse((Array) bytes, dataPosition, 4);
    return BitConverter.ToSingle(bytes, dataPosition);
  }

  private static double BytesToDouble(byte[] bytes, int dataPosition)
  {
    if (BitConverter.IsLittleEndian)
      Array.Reverse((Array) bytes, dataPosition, 8);
    return BitConverter.ToDouble(bytes, dataPosition);
  }

  private static long BytesToLong(byte[] bytes, int dataPosition)
  {
    if (BitConverter.IsLittleEndian)
      Array.Reverse((Array) bytes, dataPosition, 8);
    return BitConverter.ToInt64(bytes, dataPosition);
  }

  private static bool BytesToBool(byte[] bytes, int dataPosition)
  {
    return BitConverter.ToBoolean(bytes, dataPosition);
  }

  private static string BytesToString(byte[] bytes, int dataPosition, int dataSize)
  {
    if (dataSize == 0)
      return string.Empty;
    return Encoding.UTF8.GetString(bytes, dataPosition, dataSize);
  }

  private static byte BytesToByte(byte[] bytes, int dataPosition)
  {
    return bytes[dataPosition];
  }

  private static Color BytesToColor(byte[] bytes, int dataPosition)
  {
    Color black = Color.get_black();
    black.r = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition);
    black.g = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 4);
    black.b = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 8);
    black.a = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 12);
    return black;
  }

  private static Vector2 BytesToVector2(byte[] bytes, int dataPosition)
  {
    Vector2 zero = Vector2.get_zero();
    zero.x = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition);
    zero.y = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 4);
    return zero;
  }

  private static Vector3 BytesToVector3(byte[] bytes, int dataPosition)
  {
    Vector3 zero = Vector3.get_zero();
    zero.x = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition);
    zero.y = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 4);
    zero.z = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 8);
    return zero;
  }

  private static Vector4 BytesToVector4(byte[] bytes, int dataPosition)
  {
    Vector4 zero = Vector4.get_zero();
    zero.x = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition);
    zero.y = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 4);
    zero.z = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 8);
    zero.w = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 12);
    return zero;
  }

  private static Quaternion BytesToQuaternion(byte[] bytes, int dataPosition)
  {
    Quaternion identity = Quaternion.get_identity();
    identity.x = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition);
    identity.y = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 4);
    identity.z = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 8);
    identity.w = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 12);
    return identity;
  }

  private static Rect BytesToRect(byte[] bytes, int dataPosition)
  {
    Rect rect = (Rect) null;
    ((Rect) ref rect).set_x(BitConverter.ToSingle(bytes, dataPosition));
    ((Rect) ref rect).set_y(BitConverter.ToSingle(bytes, dataPosition + 4));
    ((Rect) ref rect).set_width(BitConverter.ToSingle(bytes, dataPosition + 8));
    ((Rect) ref rect).set_height(BitConverter.ToSingle(bytes, dataPosition + 12));
    return rect;
  }

  private static Matrix4x4 BytesToMatrix4x4(byte[] bytes, int dataPosition)
  {
    Matrix4x4 identity = Matrix4x4.get_identity();
    identity.m00 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition);
    identity.m01 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 4);
    identity.m02 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 8);
    identity.m03 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 12);
    identity.m10 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 16);
    identity.m11 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 20);
    identity.m12 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 24);
    identity.m13 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 28);
    identity.m20 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 32);
    identity.m21 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 36);
    identity.m22 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 40);
    identity.m23 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 44);
    identity.m30 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 48);
    identity.m31 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 52);
    identity.m32 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 56);
    identity.m33 = (__Null) (double) BitConverter.ToSingle(bytes, dataPosition + 60);
    return identity;
  }

  private static AnimationCurve BytesToAnimationCurve(byte[] bytes, int dataPosition)
  {
    AnimationCurve animationCurve = new AnimationCurve();
    int int32 = BitConverter.ToInt32(bytes, dataPosition);
    for (int index = 0; index < int32; ++index)
    {
      Keyframe keyframe = (Keyframe) null;
      ((Keyframe) ref keyframe).set_time(BitConverter.ToSingle(bytes, dataPosition + 4));
      ((Keyframe) ref keyframe).set_value(BitConverter.ToSingle(bytes, dataPosition + 8));
      ((Keyframe) ref keyframe).set_inTangent(BitConverter.ToSingle(bytes, dataPosition + 12));
      ((Keyframe) ref keyframe).set_outTangent(BitConverter.ToSingle(bytes, dataPosition + 16));
      ((Keyframe) ref keyframe).set_tangentMode(BitConverter.ToInt32(bytes, dataPosition + 20));
      animationCurve.AddKey(keyframe);
      dataPosition += 20;
    }
    animationCurve.set_preWrapMode((WrapMode) BitConverter.ToInt32(bytes, dataPosition + 4));
    animationCurve.set_postWrapMode((WrapMode) BitConverter.ToInt32(bytes, dataPosition + 8));
    return animationCurve;
  }

  private static LayerMask BytesToLayerMask(byte[] bytes, int dataPosition)
  {
    LayerMask layerMask = (LayerMask) null;
    ((LayerMask) ref layerMask).set_value(BinaryDeserializationDeprecated.BytesToInt(bytes, dataPosition));
    return layerMask;
  }

  private static Object IndexToUnityObject(
    int index,
    FieldSerializationData activeFieldSerializationData)
  {
    if (index < 0 || index >= activeFieldSerializationData.unityObjects.Count)
      return (Object) null;
    return activeFieldSerializationData.unityObjects[index];
  }

  private static SharedVariable BytesToSharedVariable(
    FieldSerializationData fieldSerializationData,
    Dictionary<string, int> fieldIndexMap,
    byte[] bytes,
    int dataPosition,
    IVariableSource variableSource,
    bool fromField,
    string namePrefix)
  {
    SharedVariable sharedVariable = (SharedVariable) null;
    string typeName = (string) BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (string), namePrefix + "Type", (IVariableSource) null, (object) null, (FieldInfo) null);
    if (string.IsNullOrEmpty(typeName))
      return (SharedVariable) null;
    string name = (string) BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (string), namePrefix + "Name", (IVariableSource) null, (object) null, (FieldInfo) null);
    bool boolean1 = Convert.ToBoolean(BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (bool), namePrefix + "IsShared", (IVariableSource) null, (object) null, (FieldInfo) null));
    bool boolean2 = Convert.ToBoolean(BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (bool), namePrefix + "IsGlobal", (IVariableSource) null, (object) null, (FieldInfo) null));
    if (boolean1 && fromField)
    {
      if (!boolean2)
      {
        sharedVariable = variableSource.GetVariable(name);
      }
      else
      {
        if (Object.op_Equality((Object) BinaryDeserializationDeprecated.globalVariables, (Object) null))
          BinaryDeserializationDeprecated.globalVariables = GlobalVariables.Instance;
        if (Object.op_Inequality((Object) BinaryDeserializationDeprecated.globalVariables, (Object) null))
          sharedVariable = BinaryDeserializationDeprecated.globalVariables.GetVariable(name);
      }
    }
    Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(typeName);
    if (typeWithinAssembly == null)
      return (SharedVariable) null;
    bool flag = true;
    if (sharedVariable == null || !(flag = sharedVariable.GetType().Equals(typeWithinAssembly)))
    {
      sharedVariable = TaskUtility.CreateInstance(typeWithinAssembly) as SharedVariable;
      sharedVariable.Name = name;
      sharedVariable.IsShared = boolean1;
      sharedVariable.IsGlobal = boolean2;
      sharedVariable.NetworkSync = Convert.ToBoolean(BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (bool), namePrefix + "NetworkSync", (IVariableSource) null, (object) null, (FieldInfo) null));
      if (!boolean2)
      {
        sharedVariable.PropertyMapping = (string) BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (string), namePrefix + "PropertyMapping", (IVariableSource) null, (object) null, (FieldInfo) null);
        sharedVariable.PropertyMappingOwner = (GameObject) BinaryDeserializationDeprecated.LoadField(fieldSerializationData, fieldIndexMap, typeof (GameObject), namePrefix + "PropertyMappingOwner", (IVariableSource) null, (object) null, (FieldInfo) null);
        sharedVariable.InitializePropertyMapping(variableSource as BehaviorSource);
      }
      if (!flag)
        sharedVariable.IsShared = true;
      BinaryDeserializationDeprecated.LoadFields(fieldSerializationData, fieldIndexMap, (object) sharedVariable, namePrefix, variableSource);
    }
    return sharedVariable;
  }

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
    public bool Equals(
      BinaryDeserializationDeprecated.ObjectFieldMap a,
      BinaryDeserializationDeprecated.ObjectFieldMap b)
    {
      if (object.ReferenceEquals((object) a, (object) null) || object.ReferenceEquals((object) b, (object) null) || !a.obj.Equals(b.obj))
        return false;
      return a.fieldInfo.Equals((object) b.fieldInfo);
    }

    public int GetHashCode(BinaryDeserializationDeprecated.ObjectFieldMap a)
    {
      if (a != null)
        return a.obj.ToString().GetHashCode() + a.fieldInfo.ToString().GetHashCode();
      return 0;
    }
  }
}
