// Decompiled with JetBrains decompiler
// Type: BinaryDeserialization
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class BinaryDeserialization
{
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
    }
    else
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
      BinaryDeserialization.taskIDs = (Dictionary<BinaryDeserialization.ObjectFieldMap, List<int>>) null;
      Version version = new Version(taskData.Version);
      BinaryDeserialization.updatedSerialization = version.CompareTo(new Version("1.5.7")) >= 0;
      BinaryDeserialization.shaHashSerialization = BinaryDeserialization.strHashSerialization = false;
      if (BinaryDeserialization.updatedSerialization)
      {
        BinaryDeserialization.shaHashSerialization = version.CompareTo(new Version("1.5.9")) >= 0;
        if (BinaryDeserialization.shaHashSerialization)
          BinaryDeserialization.strHashSerialization = version.CompareTo(new Version("1.5.11")) >= 0;
      }
      if (taskSerializationData.variableStartIndex != null)
      {
        List<SharedVariable> sharedVariableList = new List<SharedVariable>();
        Dictionary<int, int> fieldIndexMap = ObjectPool.Get<Dictionary<int, int>>();
        for (int index1 = 0; index1 < taskSerializationData.variableStartIndex.Count; ++index1)
        {
          int num1 = taskSerializationData.variableStartIndex[index1];
          int num2 = index1 + 1 >= taskSerializationData.variableStartIndex.Count ? (taskSerializationData.startIndex == null || taskSerializationData.startIndex.Count <= 0 ? serializationData.startIndex.Count : taskSerializationData.startIndex[0]) : taskSerializationData.variableStartIndex[index1 + 1];
          fieldIndexMap.Clear();
          for (int index2 = num1; index2 < num2; ++index2)
            fieldIndexMap.Add(serializationData.fieldNameHash[index2], serializationData.startIndex[index2]);
          SharedVariable sharedVariable = BinaryDeserialization.BytesToSharedVariable(serializationData, fieldIndexMap, serializationData.byteDataArray, taskSerializationData.variableStartIndex[index1], (IVariableSource) behaviorSource, false, 0);
          if (sharedVariable != null)
            sharedVariableList.Add(sharedVariable);
        }
        ObjectPool.Return<Dictionary<int, int>>(fieldIndexMap);
        behaviorSource.Variables = sharedVariableList;
      }
      List<Task> taskList = new List<Task>();
      if (taskSerializationData.types != null)
      {
        for (int index = 0; index < taskSerializationData.types.Count; ++index)
          BinaryDeserialization.LoadTask(taskSerializationData, serializationData, ref taskList, ref behaviorSource);
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
        if (BinaryDeserialization.taskIDs == null)
          return;
        using (Dictionary<BinaryDeserialization.ObjectFieldMap, List<int>>.KeyCollection.Enumerator enumerator = BinaryDeserialization.taskIDs.Keys.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            BinaryDeserialization.ObjectFieldMap current = enumerator.Current;
            List<int> taskId = BinaryDeserialization.taskIDs[current];
            Type fieldType = current.fieldInfo.FieldType;
            if (typeof (IList).IsAssignableFrom(fieldType))
            {
              if (fieldType.IsArray)
              {
                Type elementType = fieldType.GetElementType();
                int length = 0;
                for (int index = 0; index < taskId.Count; ++index)
                {
                  Task task = taskList[taskId[index]];
                  if (elementType.IsAssignableFrom(task.GetType()))
                    ++length;
                }
                int index1 = 0;
                Array instance = Array.CreateInstance(elementType, length);
                for (int index2 = 0; index2 < instance.Length; ++index2)
                {
                  Task task = taskList[taskId[index2]];
                  if (elementType.IsAssignableFrom(task.GetType()))
                  {
                    instance.SetValue((object) task, index1);
                    ++index1;
                  }
                }
                current.fieldInfo.SetValue(current.obj, (object) instance);
              }
              else
              {
                Type genericArgument = fieldType.GetGenericArguments()[0];
                IList instance = TaskUtility.CreateInstance(typeof (List<>).MakeGenericType(genericArgument)) as IList;
                for (int index = 0; index < taskId.Count; ++index)
                {
                  Task task = taskList[taskId[index]];
                  if (genericArgument.IsAssignableFrom(task.GetType()))
                    instance.Add((object) task);
                }
                current.fieldInfo.SetValue(current.obj, (object) instance);
              }
            }
            else
              current.fieldInfo.SetValue(current.obj, (object) taskList[taskId[0]]);
          }
        }
      }
    }
  }

  public static void Load(GlobalVariables globalVariables, string version)
  {
    if (Object.op_Equality((Object) globalVariables, (Object) null))
      return;
    if (string.IsNullOrEmpty(version))
    {
      BinaryDeserializationDeprecated.Load(globalVariables);
    }
    else
    {
      globalVariables.Variables = (List<SharedVariable>) null;
      FieldSerializationData serializationData;
      if (globalVariables.VariableData == null || (serializationData = globalVariables.VariableData.fieldSerializationData).byteData == null || serializationData.byteData.Count == 0)
        return;
      if (serializationData.typeName.Count > 0)
      {
        BinaryDeserializationDeprecated.Load(globalVariables);
      }
      else
      {
        VariableSerializationData variableData = globalVariables.VariableData;
        serializationData.byteDataArray = serializationData.byteData.ToArray();
        Version version1 = new Version(globalVariables.Version);
        BinaryDeserialization.updatedSerialization = version1.CompareTo(new Version("1.5.7")) >= 0;
        BinaryDeserialization.shaHashSerialization = BinaryDeserialization.strHashSerialization = false;
        if (BinaryDeserialization.updatedSerialization)
        {
          BinaryDeserialization.shaHashSerialization = version1.CompareTo(new Version("1.5.9")) >= 0;
          if (BinaryDeserialization.shaHashSerialization)
            BinaryDeserialization.strHashSerialization = version1.CompareTo(new Version("1.5.11")) >= 0;
        }
        if (variableData.variableStartIndex == null)
          return;
        List<SharedVariable> sharedVariableList = new List<SharedVariable>();
        Dictionary<int, int> fieldIndexMap = ObjectPool.Get<Dictionary<int, int>>();
        for (int index1 = 0; index1 < variableData.variableStartIndex.Count; ++index1)
        {
          int num1 = variableData.variableStartIndex[index1];
          int num2 = index1 + 1 >= variableData.variableStartIndex.Count ? serializationData.startIndex.Count : variableData.variableStartIndex[index1 + 1];
          fieldIndexMap.Clear();
          for (int index2 = num1; index2 < num2; ++index2)
            fieldIndexMap.Add(serializationData.fieldNameHash[index2], serializationData.startIndex[index2]);
          SharedVariable sharedVariable = BinaryDeserialization.BytesToSharedVariable(serializationData, fieldIndexMap, serializationData.byteDataArray, variableData.variableStartIndex[index1], (IVariableSource) globalVariables, false, 0);
          if (sharedVariable != null)
            sharedVariableList.Add(sharedVariable);
        }
        ObjectPool.Return<Dictionary<int, int>>(fieldIndexMap);
        globalVariables.Variables = sharedVariableList;
      }
    }
  }

  public static void LoadTask(
    TaskSerializationData taskSerializationData,
    FieldSerializationData fieldSerializationData,
    ref List<Task> taskList,
    ref BehaviorSource behaviorSource)
  {
    int count = taskList.Count;
    int index1 = taskSerializationData.startIndex[count];
    int num1 = count + 1 >= taskSerializationData.startIndex.Count ? fieldSerializationData.startIndex.Count : taskSerializationData.startIndex[count + 1];
    Dictionary<int, int> fieldIndexMap = ObjectPool.Get<Dictionary<int, int>>();
    fieldIndexMap.Clear();
    for (int index2 = index1; index2 < num1; ++index2)
    {
      if (!fieldIndexMap.ContainsKey(fieldSerializationData.fieldNameHash[index2]))
        fieldIndexMap.Add(fieldSerializationData.fieldNameHash[index2], fieldSerializationData.startIndex[index2]);
    }
    Type t = TaskUtility.GetTypeWithinAssembly(taskSerializationData.types[count]);
    if (t == null)
    {
      bool flag = false;
      for (int index2 = 0; index2 < taskSerializationData.parentIndex.Count; ++index2)
      {
        if (count == taskSerializationData.parentIndex[index2])
        {
          flag = true;
          break;
        }
      }
      t = !flag ? typeof (UnknownTask) : typeof (UnknownParentTask);
    }
    Task instance = TaskUtility.CreateInstance(t) as Task;
    if (instance is UnknownTask)
    {
      UnknownTask unknownTask = instance as UnknownTask;
      for (int index2 = index1; index2 < num1; ++index2)
      {
        unknownTask.fieldNameHash.Add(fieldSerializationData.fieldNameHash[index2]);
        unknownTask.startIndex.Add(fieldSerializationData.startIndex[index2] - fieldSerializationData.startIndex[index1]);
      }
      for (int index2 = fieldSerializationData.startIndex[index1]; index2 <= fieldSerializationData.startIndex[num1 - 1]; ++index2)
        unknownTask.dataPosition.Add(fieldSerializationData.dataPosition[index2] - fieldSerializationData.dataPosition[fieldSerializationData.startIndex[index1]]);
      int num2 = count + 1 >= taskSerializationData.startIndex.Count || taskSerializationData.startIndex[count + 1] >= fieldSerializationData.dataPosition.Count ? fieldSerializationData.byteData.Count : fieldSerializationData.dataPosition[taskSerializationData.startIndex[count + 1]];
      for (int index2 = fieldSerializationData.dataPosition[fieldSerializationData.startIndex[index1]]; index2 < num2; ++index2)
        unknownTask.byteData.Add(fieldSerializationData.byteData[index2]);
      unknownTask.unityObjects = fieldSerializationData.unityObjects;
    }
    instance.Owner = behaviorSource.Owner.GetObject() as Behavior;
    taskList.Add(instance);
    instance.ID = (int) BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (int), "ID", 0, (IVariableSource) null, (object) null, (FieldInfo) null);
    instance.FriendlyName = (string) BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (string), "FriendlyName", 0, (IVariableSource) null, (object) null, (FieldInfo) null);
    instance.IsInstant = (bool) BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (bool), "IsInstant", 0, (IVariableSource) null, (object) null, (FieldInfo) null);
    object obj;
    if ((obj = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (bool), "Disabled", 0, (IVariableSource) null, (object) null, (FieldInfo) null)) != null)
      instance.Disabled = (bool) obj;
    BinaryDeserialization.LoadNodeData(fieldSerializationData, fieldIndexMap, taskList[count]);
    if (instance.GetType().Equals(typeof (UnknownTask)) || instance.GetType().Equals(typeof (UnknownParentTask)))
    {
      if (!instance.FriendlyName.Contains("Unknown "))
        instance.FriendlyName = string.Format("Unknown {0}", (object) instance.FriendlyName);
      instance.NodeData.Comment = "Unknown Task. Right click and Replace to locate new task.";
    }
    BinaryDeserialization.LoadFields(fieldSerializationData, fieldIndexMap, (object) taskList[count], 0, (IVariableSource) behaviorSource);
    ObjectPool.Return<Dictionary<int, int>>(fieldIndexMap);
  }

  private static void LoadNodeData(
    FieldSerializationData fieldSerializationData,
    Dictionary<int, int> fieldIndexMap,
    Task task)
  {
    NodeData nodeData = new NodeData();
    nodeData.Offset = (Vector2) BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (Vector2), "NodeDataOffset", 0, (IVariableSource) null, (object) null, (FieldInfo) null);
    nodeData.Comment = (string) BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (string), "NodeDataComment", 0, (IVariableSource) null, (object) null, (FieldInfo) null);
    nodeData.IsBreakpoint = (bool) BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (bool), "NodeDataIsBreakpoint", 0, (IVariableSource) null, (object) null, (FieldInfo) null);
    nodeData.Collapsed = (bool) BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (bool), "NodeDataCollapsed", 0, (IVariableSource) null, (object) null, (FieldInfo) null);
    object obj1 = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (int), "NodeDataColorIndex", 0, (IVariableSource) null, (object) null, (FieldInfo) null);
    if (obj1 != null)
      nodeData.ColorIndex = (int) obj1;
    object obj2 = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (List<string>), "NodeDataWatchedFields", 0, (IVariableSource) null, (object) null, (FieldInfo) null);
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
    Dictionary<int, int> fieldIndexMap,
    object obj,
    int hashPrefix,
    IVariableSource variableSource)
  {
    FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
    for (int index = 0; index < allFields.Length; ++index)
    {
      if (!TaskUtility.HasAttribute(allFields[index], typeof (NonSerializedAttribute)) && (!allFields[index].IsPrivate && !allFields[index].IsFamily || TaskUtility.HasAttribute(allFields[index], typeof (SerializeField))) && (!(obj is ParentTask) || !allFields[index].Name.Equals("children")))
      {
        object objA = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, allFields[index].FieldType, allFields[index].Name, hashPrefix, variableSource, obj, allFields[index]);
        if (objA != null && !object.ReferenceEquals(objA, (object) null) && !objA.Equals((object) null))
          allFields[index].SetValue(obj, objA);
      }
    }
  }

  private static object LoadField(
    FieldSerializationData fieldSerializationData,
    Dictionary<int, int> fieldIndexMap,
    Type fieldType,
    string fieldName,
    int hashPrefix,
    IVariableSource variableSource,
    object obj = null,
    FieldInfo fieldInfo = null)
  {
    int num1 = hashPrefix;
    int num2 = !BinaryDeserialization.shaHashSerialization ? num1 + (fieldType.Name.GetHashCode() + fieldName.GetHashCode()) : num1 + (BinaryDeserialization.StringHash(fieldType.Name.ToString(), BinaryDeserialization.strHashSerialization) + BinaryDeserialization.StringHash(fieldName, BinaryDeserialization.strHashSerialization));
    int fieldIndex;
    if (!fieldIndexMap.TryGetValue(num2, out fieldIndex))
    {
      if (fieldType.IsAbstract)
        return (object) null;
      if (!typeof (SharedVariable).IsAssignableFrom(fieldType))
        return (object) null;
      SharedVariable instance = TaskUtility.CreateInstance(fieldType) as SharedVariable;
      SharedVariable sharedVariable = fieldInfo.GetValue(obj) as SharedVariable;
      if (sharedVariable != null)
        instance.SetValue(sharedVariable.GetValue());
      return (object) instance;
    }
    object obj1 = (object) null;
    if (typeof (IList).IsAssignableFrom(fieldType))
    {
      int length = BinaryDeserialization.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
      if (fieldType.IsArray)
      {
        Type elementType = fieldType.GetElementType();
        if (elementType == null)
          return (object) null;
        Array instance = Array.CreateInstance(elementType, length);
        for (int index = 0; index < length; ++index)
        {
          object objA = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, elementType, index.ToString(), num2 / (!BinaryDeserialization.updatedSerialization ? 1 : index + 1), variableSource, obj, fieldInfo);
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
          object objA = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, genericArgument, index.ToString(), num2 / (!BinaryDeserialization.updatedSerialization ? 1 : index + 1), variableSource, obj, fieldInfo);
          instance.Add(object.ReferenceEquals(objA, (object) null) || objA.Equals((object) null) ? (object) null : objA);
        }
        obj1 = (object) instance;
      }
    }
    else if (typeof (Task).IsAssignableFrom(fieldType))
    {
      if (fieldInfo != null && TaskUtility.HasAttribute(fieldInfo, typeof (InspectTaskAttribute)))
      {
        string typeName = BinaryDeserialization.BytesToString(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex], BinaryDeserialization.GetFieldSize(fieldSerializationData, fieldIndex));
        if (!string.IsNullOrEmpty(typeName))
        {
          Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(typeName);
          if (typeWithinAssembly != null)
          {
            obj1 = TaskUtility.CreateInstance(typeWithinAssembly);
            BinaryDeserialization.LoadFields(fieldSerializationData, fieldIndexMap, obj1, num2, variableSource);
          }
        }
      }
      else
      {
        if (BinaryDeserialization.taskIDs == null)
          BinaryDeserialization.taskIDs = new Dictionary<BinaryDeserialization.ObjectFieldMap, List<int>>((IEqualityComparer<BinaryDeserialization.ObjectFieldMap>) new BinaryDeserialization.ObjectFieldMapComparer());
        int num3 = BinaryDeserialization.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        BinaryDeserialization.ObjectFieldMap key = new BinaryDeserialization.ObjectFieldMap(obj, fieldInfo);
        if (BinaryDeserialization.taskIDs.ContainsKey(key))
          BinaryDeserialization.taskIDs[key].Add(num3);
        else
          BinaryDeserialization.taskIDs.Add(key, new List<int>()
          {
            num3
          });
      }
    }
    else if (typeof (SharedVariable).IsAssignableFrom(fieldType))
      obj1 = (object) BinaryDeserialization.BytesToSharedVariable(fieldSerializationData, fieldIndexMap, fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex], variableSource, true, num2);
    else if (typeof (Object).IsAssignableFrom(fieldType))
      obj1 = (object) BinaryDeserialization.IndexToUnityObject(BinaryDeserialization.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]), fieldSerializationData);
    else if (fieldType.Equals(typeof (int)) || fieldType.IsEnum)
      obj1 = (object) BinaryDeserialization.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (uint)))
      obj1 = (object) BinaryDeserialization.BytesToUInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (float)))
      obj1 = (object) BinaryDeserialization.BytesToFloat(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (double)))
      obj1 = (object) BinaryDeserialization.BytesToDouble(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (long)))
      obj1 = (object) BinaryDeserialization.BytesToLong(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (bool)))
      obj1 = (object) BinaryDeserialization.BytesToBool(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (string)))
      obj1 = (object) BinaryDeserialization.BytesToString(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex], BinaryDeserialization.GetFieldSize(fieldSerializationData, fieldIndex));
    else if (fieldType.Equals(typeof (byte)))
      obj1 = (object) BinaryDeserialization.BytesToByte(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (Vector2)))
      obj1 = (object) BinaryDeserialization.BytesToVector2(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (Vector3)))
      obj1 = (object) BinaryDeserialization.BytesToVector3(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (Vector4)))
      obj1 = (object) BinaryDeserialization.BytesToVector4(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (Quaternion)))
      obj1 = (object) BinaryDeserialization.BytesToQuaternion(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (Color)))
      obj1 = (object) BinaryDeserialization.BytesToColor(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (Rect)))
      obj1 = (object) BinaryDeserialization.BytesToRect(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (Matrix4x4)))
      obj1 = (object) BinaryDeserialization.BytesToMatrix4x4(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (AnimationCurve)))
      obj1 = (object) BinaryDeserialization.BytesToAnimationCurve(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.Equals(typeof (LayerMask)))
      obj1 = (object) BinaryDeserialization.BytesToLayerMask(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
    else if (fieldType.IsClass || fieldType.IsValueType && !fieldType.IsPrimitive)
    {
      object instance = TaskUtility.CreateInstance(fieldType);
      BinaryDeserialization.LoadFields(fieldSerializationData, fieldIndexMap, instance, num2, variableSource);
      return instance;
    }
    return obj1;
  }

  public static int StringHash(string value, bool fastHash)
  {
    if (string.IsNullOrEmpty(value))
      return 0;
    if (fastHash)
    {
      int num = 23;
      int length = value.Length;
      for (int index = 0; index < length; ++index)
        num = num * 31 + (int) value[index];
      return num;
    }
    byte[] bytes = Encoding.UTF8.GetBytes(value);
    if (BinaryDeserialization.shaHash == null)
      BinaryDeserialization.shaHash = (SHA1) new SHA1Managed();
    return BitConverter.ToInt32(BinaryDeserialization.shaHash.ComputeHash(bytes), 0);
  }

  private static int GetFieldSize(FieldSerializationData fieldSerializationData, int fieldIndex)
  {
    return (fieldIndex + 1 >= fieldSerializationData.dataPosition.Count ? fieldSerializationData.byteData.Count : fieldSerializationData.dataPosition[fieldIndex + 1]) - fieldSerializationData.dataPosition[fieldIndex];
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
    ((LayerMask) ref layerMask).set_value(BinaryDeserialization.BytesToInt(bytes, dataPosition));
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
    Dictionary<int, int> fieldIndexMap,
    byte[] bytes,
    int dataPosition,
    IVariableSource variableSource,
    bool fromField,
    int hashPrefix)
  {
    SharedVariable sharedVariable = (SharedVariable) null;
    string typeName = (string) BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (string), "Type", hashPrefix, (IVariableSource) null, (object) null, (FieldInfo) null);
    if (string.IsNullOrEmpty(typeName))
      return (SharedVariable) null;
    string name = (string) BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (string), "Name", hashPrefix, (IVariableSource) null, (object) null, (FieldInfo) null);
    bool boolean1 = Convert.ToBoolean(BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (bool), "IsShared", hashPrefix, (IVariableSource) null, (object) null, (FieldInfo) null));
    bool boolean2 = Convert.ToBoolean(BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (bool), "IsGlobal", hashPrefix, (IVariableSource) null, (object) null, (FieldInfo) null));
    if (boolean1 && fromField)
    {
      if (!boolean2)
      {
        sharedVariable = variableSource.GetVariable(name);
      }
      else
      {
        if (Object.op_Equality((Object) BinaryDeserialization.globalVariables, (Object) null))
          BinaryDeserialization.globalVariables = GlobalVariables.Instance;
        if (Object.op_Inequality((Object) BinaryDeserialization.globalVariables, (Object) null))
          sharedVariable = BinaryDeserialization.globalVariables.GetVariable(name);
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
      sharedVariable.NetworkSync = Convert.ToBoolean(BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (bool), "NetworkSync", hashPrefix, (IVariableSource) null, (object) null, (FieldInfo) null));
      if (!boolean2)
      {
        sharedVariable.PropertyMapping = (string) BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (string), "PropertyMapping", hashPrefix, (IVariableSource) null, (object) null, (FieldInfo) null);
        sharedVariable.PropertyMappingOwner = (GameObject) BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof (GameObject), "PropertyMappingOwner", hashPrefix, (IVariableSource) null, (object) null, (FieldInfo) null);
        sharedVariable.InitializePropertyMapping(variableSource as BehaviorSource);
      }
      if (!flag)
        sharedVariable.IsShared = true;
      BinaryDeserialization.LoadFields(fieldSerializationData, fieldIndexMap, (object) sharedVariable, hashPrefix, variableSource);
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

  private class ObjectFieldMapComparer : IEqualityComparer<BinaryDeserialization.ObjectFieldMap>
  {
    public bool Equals(
      BinaryDeserialization.ObjectFieldMap a,
      BinaryDeserialization.ObjectFieldMap b)
    {
      if (object.ReferenceEquals((object) a, (object) null) || object.ReferenceEquals((object) b, (object) null) || !a.obj.Equals(b.obj))
        return false;
      return a.fieldInfo.Equals((object) b.fieldInfo);
    }

    public int GetHashCode(BinaryDeserialization.ObjectFieldMap a)
    {
      if (a != null)
        return a.obj.ToString().GetHashCode() + a.fieldInfo.ToString().GetHashCode();
      return 0;
    }
  }
}
