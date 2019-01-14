// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.JSONDeserializationDeprecated
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
  public class JSONDeserializationDeprecated : Object
  {
    private static Dictionary<JSONDeserializationDeprecated.TaskField, List<int>> taskIDs = (Dictionary<JSONDeserializationDeprecated.TaskField, List<int>>) null;
    private static GlobalVariables globalVariables = (GlobalVariables) null;
    private static Dictionary<int, Dictionary<string, object>> serializationCache = new Dictionary<int, Dictionary<string, object>>();

    public JSONDeserializationDeprecated()
    {
      base.\u002Ector();
    }

    public static void Load(TaskSerializationData taskData, BehaviorSource behaviorSource)
    {
      behaviorSource.EntryTask = (Task) null;
      behaviorSource.RootTask = (Task) null;
      behaviorSource.DetachedTasks = (List<Task>) null;
      behaviorSource.Variables = (List<SharedVariable>) null;
      Dictionary<string, object> dict1;
      if (!JSONDeserializationDeprecated.serializationCache.TryGetValue(taskData.JSONSerialization.GetHashCode(), out dict1))
      {
        dict1 = MiniJSON.Deserialize(taskData.JSONSerialization) as Dictionary<string, object>;
        JSONDeserializationDeprecated.serializationCache.Add(taskData.JSONSerialization.GetHashCode(), dict1);
      }
      if (dict1 == null)
      {
        Debug.Log((object) "Failed to deserialize");
      }
      else
      {
        JSONDeserializationDeprecated.taskIDs = new Dictionary<JSONDeserializationDeprecated.TaskField, List<int>>();
        Dictionary<int, Task> IDtoTask = new Dictionary<int, Task>();
        JSONDeserializationDeprecated.DeserializeVariables((IVariableSource) behaviorSource, dict1, taskData.fieldSerializationData.unityObjects);
        if (dict1.ContainsKey("EntryTask"))
          behaviorSource.EntryTask = JSONDeserializationDeprecated.DeserializeTask(behaviorSource, dict1["EntryTask"] as Dictionary<string, object>, ref IDtoTask, taskData.fieldSerializationData.unityObjects);
        if (dict1.ContainsKey("RootTask"))
          behaviorSource.RootTask = JSONDeserializationDeprecated.DeserializeTask(behaviorSource, dict1["RootTask"] as Dictionary<string, object>, ref IDtoTask, taskData.fieldSerializationData.unityObjects);
        if (dict1.ContainsKey("DetachedTasks"))
        {
          List<Task> taskList = new List<Task>();
          foreach (Dictionary<string, object> dict2 in dict1["DetachedTasks"] as IEnumerable)
            taskList.Add(JSONDeserializationDeprecated.DeserializeTask(behaviorSource, dict2, ref IDtoTask, taskData.fieldSerializationData.unityObjects));
          behaviorSource.DetachedTasks = taskList;
        }
        if (JSONDeserializationDeprecated.taskIDs == null || JSONDeserializationDeprecated.taskIDs.Count <= 0)
          return;
        using (Dictionary<JSONDeserializationDeprecated.TaskField, List<int>>.KeyCollection.Enumerator enumerator = JSONDeserializationDeprecated.taskIDs.Keys.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            JSONDeserializationDeprecated.TaskField current = enumerator.Current;
            List<int> taskId = JSONDeserializationDeprecated.taskIDs[current];
            Type fieldType = current.fieldInfo.FieldType;
            if (current.fieldInfo.FieldType.IsArray)
            {
              int length = 0;
              for (int index = 0; index < taskId.Count; ++index)
              {
                Task task = IDtoTask[taskId[index]];
                if (task.GetType().Equals(fieldType.GetElementType()) || task.GetType().IsSubclassOf(fieldType.GetElementType()))
                  ++length;
              }
              Array instance = Array.CreateInstance(fieldType.GetElementType(), length);
              int index1 = 0;
              for (int index2 = 0; index2 < taskId.Count; ++index2)
              {
                Task task = IDtoTask[taskId[index2]];
                if (task.GetType().Equals(fieldType.GetElementType()) || task.GetType().IsSubclassOf(fieldType.GetElementType()))
                {
                  instance.SetValue((object) task, index1);
                  ++index1;
                }
              }
              current.fieldInfo.SetValue((object) current.task, (object) instance);
            }
            else
            {
              Task task = IDtoTask[taskId[0]];
              if (task.GetType().Equals(current.fieldInfo.FieldType) || task.GetType().IsSubclassOf(current.fieldInfo.FieldType))
                current.fieldInfo.SetValue((object) current.task, (object) task);
            }
          }
        }
        JSONDeserializationDeprecated.taskIDs = (Dictionary<JSONDeserializationDeprecated.TaskField, List<int>>) null;
      }
    }

    public static void Load(string serialization, GlobalVariables globalVariables)
    {
      if (Object.op_Equality((Object) globalVariables, (Object) null))
        return;
      Dictionary<string, object> dict = MiniJSON.Deserialize(serialization) as Dictionary<string, object>;
      if (dict == null)
      {
        Debug.Log((object) "Failed to deserialize");
      }
      else
      {
        if (globalVariables.VariableData == null)
          globalVariables.VariableData = new VariableSerializationData();
        JSONDeserializationDeprecated.DeserializeVariables((IVariableSource) globalVariables, dict, globalVariables.VariableData.fieldSerializationData.unityObjects);
      }
    }

    private static void DeserializeVariables(
      IVariableSource variableSource,
      Dictionary<string, object> dict,
      List<Object> unityObjects)
    {
      object obj;
      if (!dict.TryGetValue("Variables", out obj))
        return;
      List<SharedVariable> variables = new List<SharedVariable>();
      IList list = obj as IList;
      for (int index = 0; index < list.Count; ++index)
      {
        SharedVariable sharedVariable = JSONDeserializationDeprecated.DeserializeSharedVariable(list[index] as Dictionary<string, object>, variableSource, true, unityObjects);
        variables.Add(sharedVariable);
      }
      variableSource.SetAllVariables(variables);
    }

    public static Task DeserializeTask(
      BehaviorSource behaviorSource,
      Dictionary<string, object> dict,
      ref Dictionary<int, Task> IDtoTask,
      List<Object> unityObjects)
    {
      Task task = (Task) null;
      try
      {
        task = TaskUtility.CreateInstance(TaskUtility.GetTypeWithinAssembly(dict["ObjectType"] as string) ?? (!dict.ContainsKey("Children") ? typeof (UnknownTask) : typeof (UnknownParentTask))) as Task;
      }
      catch (Exception ex)
      {
      }
      if (task == null)
        return (Task) null;
      task.Owner = behaviorSource.Owner.GetObject() as Behavior;
      task.ID = Convert.ToInt32(dict["ID"]);
      object obj;
      if (dict.TryGetValue("Name", out obj))
        task.FriendlyName = (string) obj;
      if (dict.TryGetValue("Instant", out obj))
        task.IsInstant = Convert.ToBoolean(obj);
      if (dict.TryGetValue("Disabled", out obj))
        task.Disabled = Convert.ToBoolean(obj);
      IDtoTask.Add(task.ID, task);
      task.NodeData = JSONDeserializationDeprecated.DeserializeNodeData(dict["NodeData"] as Dictionary<string, object>, task);
      if (task.GetType().Equals(typeof (UnknownTask)) || task.GetType().Equals(typeof (UnknownParentTask)))
      {
        if (!task.FriendlyName.Contains("Unknown "))
          task.FriendlyName = string.Format("Unknown {0}", (object) task.FriendlyName);
        if (!task.NodeData.Comment.Contains("Loaded from an unknown type. Was a task renamed or deleted?"))
          task.NodeData.Comment = string.Format("Loaded from an unknown type. Was a task renamed or deleted?{0}", !task.NodeData.Comment.Equals(string.Empty) ? (object) string.Format("\0{0}", (object) task.NodeData.Comment) : (object) string.Empty);
      }
      JSONDeserializationDeprecated.DeserializeObject(task, (object) task, dict, (IVariableSource) behaviorSource, unityObjects);
      if (task is ParentTask && dict.TryGetValue("Children", out obj))
      {
        ParentTask parentTask = task as ParentTask;
        if (parentTask != null)
        {
          foreach (Dictionary<string, object> dict1 in obj as IEnumerable)
          {
            Task child = JSONDeserializationDeprecated.DeserializeTask(behaviorSource, dict1, ref IDtoTask, unityObjects);
            int index = parentTask.Children != null ? parentTask.Children.Count : 0;
            parentTask.AddChild(child, index);
          }
        }
      }
      return task;
    }

    private static NodeData DeserializeNodeData(Dictionary<string, object> dict, Task task)
    {
      NodeData nodeData = new NodeData();
      object obj;
      if (dict.TryGetValue("Offset", out obj))
        nodeData.Offset = JSONDeserializationDeprecated.StringToVector2((string) obj);
      if (dict.TryGetValue("FriendlyName", out obj))
        task.FriendlyName = (string) obj;
      if (dict.TryGetValue("Comment", out obj))
        nodeData.Comment = (string) obj;
      if (dict.TryGetValue("IsBreakpoint", out obj))
        nodeData.IsBreakpoint = Convert.ToBoolean(obj);
      if (dict.TryGetValue("Collapsed", out obj))
        nodeData.Collapsed = Convert.ToBoolean(obj);
      if (dict.TryGetValue("ColorIndex", out obj))
        nodeData.ColorIndex = Convert.ToInt32(obj);
      if (dict.TryGetValue("WatchedFields", out obj))
      {
        nodeData.WatchedFieldNames = new List<string>();
        nodeData.WatchedFields = new List<FieldInfo>();
        IList list = obj as IList;
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
      return nodeData;
    }

    private static SharedVariable DeserializeSharedVariable(
      Dictionary<string, object> dict,
      IVariableSource variableSource,
      bool fromSource,
      List<Object> unityObjects)
    {
      if (dict == null)
        return (SharedVariable) null;
      SharedVariable sharedVariable = (SharedVariable) null;
      object obj1;
      if (!fromSource && variableSource != null && dict.TryGetValue("Name", out obj1))
      {
        object obj2;
        dict.TryGetValue("IsGlobal", out obj2);
        if (!dict.TryGetValue("IsGlobal", out obj2) || !Convert.ToBoolean(obj2))
        {
          sharedVariable = variableSource.GetVariable(obj1 as string);
        }
        else
        {
          if (Object.op_Equality((Object) JSONDeserializationDeprecated.globalVariables, (Object) null))
            JSONDeserializationDeprecated.globalVariables = GlobalVariables.Instance;
          if (Object.op_Inequality((Object) JSONDeserializationDeprecated.globalVariables, (Object) null))
            sharedVariable = JSONDeserializationDeprecated.globalVariables.GetVariable(obj1 as string);
        }
      }
      Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(dict["Type"] as string);
      if (typeWithinAssembly == null)
        return (SharedVariable) null;
      bool flag = true;
      if (sharedVariable == null || !(flag = sharedVariable.GetType().Equals(typeWithinAssembly)))
      {
        sharedVariable = TaskUtility.CreateInstance(typeWithinAssembly) as SharedVariable;
        sharedVariable.Name = dict["Name"] as string;
        object obj2;
        if (dict.TryGetValue("IsShared", out obj2))
          sharedVariable.IsShared = Convert.ToBoolean(obj2);
        if (dict.TryGetValue("IsGlobal", out obj2))
          sharedVariable.IsGlobal = Convert.ToBoolean(obj2);
        if (dict.TryGetValue("NetworkSync", out obj2))
          sharedVariable.NetworkSync = Convert.ToBoolean(obj2);
        if (!sharedVariable.IsGlobal && dict.TryGetValue("PropertyMapping", out obj2))
        {
          sharedVariable.PropertyMapping = obj2 as string;
          if (dict.TryGetValue("PropertyMappingOwner", out obj2))
            sharedVariable.PropertyMappingOwner = JSONDeserializationDeprecated.IndexToUnityObject(Convert.ToInt32(obj2), unityObjects) as GameObject;
          sharedVariable.InitializePropertyMapping(variableSource as BehaviorSource);
        }
        if (!flag)
          sharedVariable.IsShared = true;
        JSONDeserializationDeprecated.DeserializeObject((Task) null, (object) sharedVariable, dict, variableSource, unityObjects);
      }
      return sharedVariable;
    }

    private static void DeserializeObject(
      Task task,
      object obj,
      Dictionary<string, object> dict,
      IVariableSource variableSource,
      List<Object> unityObjects)
    {
      if (dict == null)
        return;
      FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
      for (int index1 = 0; index1 < allFields.Length; ++index1)
      {
        object obj1;
        if (dict.TryGetValue(allFields[index1].FieldType.ToString() + "," + allFields[index1].Name, out obj1) || dict.TryGetValue(allFields[index1].Name, out obj1))
        {
          if (typeof (IList).IsAssignableFrom(allFields[index1].FieldType))
          {
            IList list = obj1 as IList;
            if (list != null)
            {
              Type type1;
              if (allFields[index1].FieldType.IsArray)
              {
                type1 = allFields[index1].FieldType.GetElementType();
              }
              else
              {
                Type type2 = allFields[index1].FieldType;
                while (!type2.IsGenericType)
                  type2 = type2.BaseType;
                type1 = type2.GetGenericArguments()[0];
              }
              if (type1.Equals(typeof (Task)) || type1.IsSubclassOf(typeof (Task)))
              {
                if (JSONDeserializationDeprecated.taskIDs != null)
                {
                  List<int> intList = new List<int>();
                  for (int index2 = 0; index2 < list.Count; ++index2)
                    intList.Add(Convert.ToInt32(list[index2]));
                  JSONDeserializationDeprecated.taskIDs.Add(new JSONDeserializationDeprecated.TaskField(task, allFields[index1]), intList);
                }
              }
              else if (allFields[index1].FieldType.IsArray)
              {
                Array instance = Array.CreateInstance(type1, list.Count);
                for (int index2 = 0; index2 < list.Count; ++index2)
                  instance.SetValue(JSONDeserializationDeprecated.ValueToObject(task, type1, list[index2], variableSource, unityObjects), index2);
                allFields[index1].SetValue(obj, (object) instance);
              }
              else
              {
                IList instance;
                if (allFields[index1].FieldType.IsGenericType)
                  instance = TaskUtility.CreateInstance(typeof (List<>).MakeGenericType(type1)) as IList;
                else
                  instance = TaskUtility.CreateInstance(allFields[index1].FieldType) as IList;
                for (int index2 = 0; index2 < list.Count; ++index2)
                  instance.Add(JSONDeserializationDeprecated.ValueToObject(task, type1, list[index2], variableSource, unityObjects));
                allFields[index1].SetValue(obj, (object) instance);
              }
            }
          }
          else
          {
            Type fieldType = allFields[index1].FieldType;
            if (fieldType.Equals(typeof (Task)) || fieldType.IsSubclassOf(typeof (Task)))
            {
              if (TaskUtility.HasAttribute(allFields[index1], typeof (InspectTaskAttribute)))
              {
                Dictionary<string, object> dict1 = obj1 as Dictionary<string, object>;
                Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(dict1["ObjectType"] as string);
                if (typeWithinAssembly != null)
                {
                  Task instance = TaskUtility.CreateInstance(typeWithinAssembly) as Task;
                  JSONDeserializationDeprecated.DeserializeObject(instance, (object) instance, dict1, variableSource, unityObjects);
                  allFields[index1].SetValue((object) task, (object) instance);
                }
              }
              else if (JSONDeserializationDeprecated.taskIDs != null)
                JSONDeserializationDeprecated.taskIDs.Add(new JSONDeserializationDeprecated.TaskField(task, allFields[index1]), new List<int>()
                {
                  Convert.ToInt32(obj1)
                });
            }
            else
              allFields[index1].SetValue(obj, JSONDeserializationDeprecated.ValueToObject(task, fieldType, obj1, variableSource, unityObjects));
          }
        }
        else if (typeof (SharedVariable).IsAssignableFrom(allFields[index1].FieldType) && !allFields[index1].FieldType.IsAbstract)
        {
          if (dict.TryGetValue(allFields[index1].FieldType.ToString() + "," + allFields[index1].Name, out obj1))
          {
            SharedVariable instance = TaskUtility.CreateInstance(allFields[index1].FieldType) as SharedVariable;
            instance.SetValue(JSONDeserializationDeprecated.ValueToObject(task, allFields[index1].FieldType, obj1, variableSource, unityObjects));
            allFields[index1].SetValue(obj, (object) instance);
          }
          else
          {
            SharedVariable instance = TaskUtility.CreateInstance(allFields[index1].FieldType) as SharedVariable;
            allFields[index1].SetValue(obj, (object) instance);
          }
        }
      }
    }

    private static object ValueToObject(
      Task task,
      Type type,
      object obj,
      IVariableSource variableSource,
      List<Object> unityObjects)
    {
      if (type.Equals(typeof (SharedVariable)) || type.IsSubclassOf(typeof (SharedVariable)))
        return (object) (JSONDeserializationDeprecated.DeserializeSharedVariable(obj as Dictionary<string, object>, variableSource, false, unityObjects) ?? TaskUtility.CreateInstance(type) as SharedVariable);
      if (type.Equals(typeof (Object)) || type.IsSubclassOf(typeof (Object)))
        return (object) JSONDeserializationDeprecated.IndexToUnityObject(Convert.ToInt32(obj), unityObjects);
      if (!type.IsPrimitive)
      {
        if (!type.Equals(typeof (string)))
        {
          if (type.IsSubclassOf(typeof (Enum)))
          {
            try
            {
              return Enum.Parse(type, (string) obj);
            }
            catch (Exception ex)
            {
              return (object) null;
            }
          }
          else
          {
            if (type.Equals(typeof (Vector2)))
              return (object) JSONDeserializationDeprecated.StringToVector2((string) obj);
            if (type.Equals(typeof (Vector3)))
              return (object) JSONDeserializationDeprecated.StringToVector3((string) obj);
            if (type.Equals(typeof (Vector4)))
              return (object) JSONDeserializationDeprecated.StringToVector4((string) obj);
            if (type.Equals(typeof (Quaternion)))
              return (object) JSONDeserializationDeprecated.StringToQuaternion((string) obj);
            if (type.Equals(typeof (Matrix4x4)))
              return (object) JSONDeserializationDeprecated.StringToMatrix4x4((string) obj);
            if (type.Equals(typeof (Color)))
              return (object) JSONDeserializationDeprecated.StringToColor((string) obj);
            if (type.Equals(typeof (Rect)))
              return (object) JSONDeserializationDeprecated.StringToRect((string) obj);
            if (type.Equals(typeof (LayerMask)))
              return (object) JSONDeserializationDeprecated.ValueToLayerMask(Convert.ToInt32(obj));
            if (type.Equals(typeof (AnimationCurve)))
              return (object) JSONDeserializationDeprecated.ValueToAnimationCurve((Dictionary<string, object>) obj);
            object instance = TaskUtility.CreateInstance(type);
            JSONDeserializationDeprecated.DeserializeObject(task, instance, obj as Dictionary<string, object>, variableSource, unityObjects);
            return instance;
          }
        }
      }
      try
      {
        return Convert.ChangeType(obj, type);
      }
      catch (Exception ex)
      {
        return (object) null;
      }
    }

    private static Vector2 StringToVector2(string vector2String)
    {
      string[] strArray = vector2String.Substring(1, vector2String.Length - 2).Split(',');
      return new Vector2(float.Parse(strArray[0]), float.Parse(strArray[1]));
    }

    private static Vector3 StringToVector3(string vector3String)
    {
      string[] strArray = vector3String.Substring(1, vector3String.Length - 2).Split(',');
      return new Vector3(float.Parse(strArray[0]), float.Parse(strArray[1]), float.Parse(strArray[2]));
    }

    private static Vector4 StringToVector4(string vector4String)
    {
      string[] strArray = vector4String.Substring(1, vector4String.Length - 2).Split(',');
      return new Vector4(float.Parse(strArray[0]), float.Parse(strArray[1]), float.Parse(strArray[2]), float.Parse(strArray[3]));
    }

    private static Quaternion StringToQuaternion(string quaternionString)
    {
      string[] strArray = quaternionString.Substring(1, quaternionString.Length - 2).Split(',');
      return new Quaternion(float.Parse(strArray[0]), float.Parse(strArray[1]), float.Parse(strArray[2]), float.Parse(strArray[3]));
    }

    private static Matrix4x4 StringToMatrix4x4(string matrixString)
    {
      string[] strArray = matrixString.Split((char[]) null);
      Matrix4x4 matrix4x4 = (Matrix4x4) null;
      matrix4x4.m00 = (__Null) (double) float.Parse(strArray[0]);
      matrix4x4.m01 = (__Null) (double) float.Parse(strArray[1]);
      matrix4x4.m02 = (__Null) (double) float.Parse(strArray[2]);
      matrix4x4.m03 = (__Null) (double) float.Parse(strArray[3]);
      matrix4x4.m10 = (__Null) (double) float.Parse(strArray[4]);
      matrix4x4.m11 = (__Null) (double) float.Parse(strArray[5]);
      matrix4x4.m12 = (__Null) (double) float.Parse(strArray[6]);
      matrix4x4.m13 = (__Null) (double) float.Parse(strArray[7]);
      matrix4x4.m20 = (__Null) (double) float.Parse(strArray[8]);
      matrix4x4.m21 = (__Null) (double) float.Parse(strArray[9]);
      matrix4x4.m22 = (__Null) (double) float.Parse(strArray[10]);
      matrix4x4.m23 = (__Null) (double) float.Parse(strArray[11]);
      matrix4x4.m30 = (__Null) (double) float.Parse(strArray[12]);
      matrix4x4.m31 = (__Null) (double) float.Parse(strArray[13]);
      matrix4x4.m32 = (__Null) (double) float.Parse(strArray[14]);
      matrix4x4.m33 = (__Null) (double) float.Parse(strArray[15]);
      return matrix4x4;
    }

    private static Color StringToColor(string colorString)
    {
      string[] strArray = colorString.Substring(5, colorString.Length - 6).Split(',');
      return new Color(float.Parse(strArray[0]), float.Parse(strArray[1]), float.Parse(strArray[2]), float.Parse(strArray[3]));
    }

    private static Rect StringToRect(string rectString)
    {
      string[] strArray = rectString.Substring(1, rectString.Length - 2).Split(',');
      return new Rect(float.Parse(strArray[0].Substring(2, strArray[0].Length - 2)), float.Parse(strArray[1].Substring(3, strArray[1].Length - 3)), float.Parse(strArray[2].Substring(7, strArray[2].Length - 7)), float.Parse(strArray[3].Substring(8, strArray[3].Length - 8)));
    }

    private static LayerMask ValueToLayerMask(int value)
    {
      LayerMask layerMask = (LayerMask) null;
      ((LayerMask) ref layerMask).set_value(value);
      return layerMask;
    }

    private static AnimationCurve ValueToAnimationCurve(
      Dictionary<string, object> value)
    {
      AnimationCurve animationCurve = new AnimationCurve();
      object obj;
      if (value.TryGetValue("Keys", out obj))
      {
        List<object> objectList1 = obj as List<object>;
        for (int index = 0; index < objectList1.Count; ++index)
        {
          List<object> objectList2 = objectList1[index] as List<object>;
          Keyframe keyframe;
          ((Keyframe) ref keyframe).\u002Ector((float) Convert.ChangeType(objectList2[0], typeof (float)), (float) Convert.ChangeType(objectList2[1], typeof (float)), (float) Convert.ChangeType(objectList2[2], typeof (float)), (float) Convert.ChangeType(objectList2[3], typeof (float)));
          ((Keyframe) ref keyframe).set_tangentMode((int) Convert.ChangeType(objectList2[4], typeof (int)));
          animationCurve.AddKey(keyframe);
        }
      }
      if (value.TryGetValue("PreWrapMode", out obj))
        animationCurve.set_preWrapMode((WrapMode) (int) Enum.Parse(typeof (WrapMode), (string) obj));
      if (value.TryGetValue("PostWrapMode", out obj))
        animationCurve.set_postWrapMode((WrapMode) (int) Enum.Parse(typeof (WrapMode), (string) obj));
      return animationCurve;
    }

    private static Object IndexToUnityObject(int index, List<Object> unityObjects)
    {
      if (index < 0 || index >= unityObjects.Count)
        return (Object) null;
      return unityObjects[index];
    }

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
  }
}
