// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.JSONSerialization
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  public class JSONSerialization : Object
  {
    private static TaskSerializationData taskSerializationData;
    private static FieldSerializationData fieldSerializationData;
    private static VariableSerializationData variableSerializationData;

    public JSONSerialization()
    {
      base.\u002Ector();
    }

    public static void Save(BehaviorSource behaviorSource)
    {
      behaviorSource.CheckForSerialization(false, (BehaviorSource) null);
      JSONSerialization.taskSerializationData = new TaskSerializationData();
      JSONSerialization.fieldSerializationData = (FieldSerializationData) JSONSerialization.taskSerializationData.fieldSerializationData;
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      if (behaviorSource.get_EntryTask() != null)
      {
        // ISSUE: cast to a reference type
        dictionary.Add("EntryTask", (object) JSONSerialization.SerializeTask(behaviorSource.get_EntryTask(), true, (List<Object>&) ref JSONSerialization.fieldSerializationData.unityObjects));
      }
      if (behaviorSource.get_RootTask() != null)
      {
        // ISSUE: cast to a reference type
        dictionary.Add("RootTask", (object) JSONSerialization.SerializeTask(behaviorSource.get_RootTask(), true, (List<Object>&) ref JSONSerialization.fieldSerializationData.unityObjects));
      }
      if (behaviorSource.get_DetachedTasks() != null && behaviorSource.get_DetachedTasks().Count > 0)
      {
        Dictionary<string, object>[] dictionaryArray = new Dictionary<string, object>[behaviorSource.get_DetachedTasks().Count];
        for (int index = 0; index < behaviorSource.get_DetachedTasks().Count; ++index)
        {
          // ISSUE: cast to a reference type
          dictionaryArray[index] = JSONSerialization.SerializeTask(behaviorSource.get_DetachedTasks()[index], true, (List<Object>&) ref JSONSerialization.fieldSerializationData.unityObjects);
        }
        dictionary.Add("DetachedTasks", (object) dictionaryArray);
      }
      if (behaviorSource.get_Variables() != null && behaviorSource.get_Variables().Count > 0)
      {
        // ISSUE: cast to a reference type
        dictionary.Add("Variables", (object) JSONSerialization.SerializeVariables(behaviorSource.get_Variables(), (List<Object>&) ref JSONSerialization.fieldSerializationData.unityObjects));
      }
      JSONSerialization.taskSerializationData.Version = (__Null) "1.5.11";
      JSONSerialization.taskSerializationData.JSONSerialization = (__Null) MiniJSON.Serialize((object) dictionary);
      behaviorSource.set_TaskData(JSONSerialization.taskSerializationData);
      if (behaviorSource.get_Owner() == null || ((object) behaviorSource.get_Owner()).Equals((object) null))
        return;
      BehaviorDesignerUtility.SetObjectDirty(behaviorSource.get_Owner().GetObject());
    }

    public static void Save(GlobalVariables variables)
    {
      if (Object.op_Equality((Object) variables, (Object) null))
        return;
      JSONSerialization.variableSerializationData = new VariableSerializationData();
      JSONSerialization.fieldSerializationData = (FieldSerializationData) JSONSerialization.variableSerializationData.fieldSerializationData;
      // ISSUE: cast to a reference type
      JSONSerialization.variableSerializationData.JSONSerialization = (__Null) MiniJSON.Serialize((object) new Dictionary<string, object>()
      {
        {
          "Variables",
          (object) JSONSerialization.SerializeVariables(variables.get_Variables(), (List<Object>&) ref JSONSerialization.fieldSerializationData.unityObjects)
        }
      });
      variables.set_VariableData(JSONSerialization.variableSerializationData);
      variables.set_Version("1.5.11");
      BehaviorDesignerUtility.SetObjectDirty((Object) variables);
    }

    private static Dictionary<string, object>[] SerializeVariables(
      List<SharedVariable> variables,
      ref List<Object> unityObjects)
    {
      Dictionary<string, object>[] dictionaryArray = new Dictionary<string, object>[variables.Count];
      for (int index = 0; index < variables.Count; ++index)
        dictionaryArray[index] = JSONSerialization.SerializeVariable(variables[index], ref unityObjects);
      return dictionaryArray;
    }

    public static Dictionary<string, object> SerializeTask(
      Task task,
      bool serializeChildren,
      ref List<Object> unityObjects)
    {
      Dictionary<string, object> dict = new Dictionary<string, object>();
      dict.Add("Type", (object) ((object) task).GetType());
      dict.Add("NodeData", (object) JSONSerialization.SerializeNodeData(task.get_NodeData()));
      dict.Add("ID", (object) task.get_ID());
      dict.Add("Name", (object) task.get_FriendlyName());
      dict.Add("Instant", (object) task.get_IsInstant());
      if (task.get_Disabled())
        dict.Add("Disabled", (object) task.get_Disabled());
      JSONSerialization.SerializeFields((object) task, ref dict, ref unityObjects);
      if (serializeChildren && task is ParentTask)
      {
        ParentTask parentTask = task as ParentTask;
        if (parentTask.get_Children() != null && parentTask.get_Children().Count > 0)
        {
          Dictionary<string, object>[] dictionaryArray = new Dictionary<string, object>[parentTask.get_Children().Count];
          for (int index = 0; index < parentTask.get_Children().Count; ++index)
            dictionaryArray[index] = JSONSerialization.SerializeTask(parentTask.get_Children()[index], serializeChildren, ref unityObjects);
          dict.Add("Children", (object) dictionaryArray);
        }
      }
      return dict;
    }

    private static Dictionary<string, object> SerializeNodeData(NodeData nodeData)
    {
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      dictionary.Add("Offset", (object) nodeData.get_Offset());
      if (nodeData.get_Comment().Length > 0)
        dictionary.Add("Comment", (object) nodeData.get_Comment());
      if (nodeData.get_IsBreakpoint())
        dictionary.Add("IsBreakpoint", (object) nodeData.get_IsBreakpoint());
      if (nodeData.get_Collapsed())
        dictionary.Add("Collapsed", (object) nodeData.get_Collapsed());
      if (nodeData.get_ColorIndex() != 0)
        dictionary.Add("ColorIndex", (object) nodeData.get_ColorIndex());
      if (nodeData.get_WatchedFieldNames() != null && nodeData.get_WatchedFieldNames().Count > 0)
        dictionary.Add("WatchedFields", (object) nodeData.get_WatchedFieldNames());
      return dictionary;
    }

    private static Dictionary<string, object> SerializeVariable(
      SharedVariable sharedVariable,
      ref List<Object> unityObjects)
    {
      if (sharedVariable == null)
        return (Dictionary<string, object>) null;
      Dictionary<string, object> dict = new Dictionary<string, object>();
      dict.Add("Type", (object) ((object) sharedVariable).GetType());
      dict.Add("Name", (object) sharedVariable.get_Name());
      if (sharedVariable.get_IsShared())
        dict.Add("IsShared", (object) sharedVariable.get_IsShared());
      if (sharedVariable.get_IsGlobal())
        dict.Add("IsGlobal", (object) sharedVariable.get_IsGlobal());
      if (sharedVariable.get_NetworkSync())
        dict.Add("NetworkSync", (object) sharedVariable.get_NetworkSync());
      if (!string.IsNullOrEmpty(sharedVariable.get_PropertyMapping()))
      {
        dict.Add("PropertyMapping", (object) sharedVariable.get_PropertyMapping());
        if (!object.Equals((object) sharedVariable.get_PropertyMappingOwner(), (object) null))
        {
          dict.Add("PropertyMappingOwner", (object) unityObjects.Count);
          unityObjects.Add((Object) sharedVariable.get_PropertyMappingOwner());
        }
      }
      JSONSerialization.SerializeFields((object) sharedVariable, ref dict, ref unityObjects);
      return dict;
    }

    private static void SerializeFields(
      object obj,
      ref Dictionary<string, object> dict,
      ref List<Object> unityObjects)
    {
      FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
      for (int index1 = 0; index1 < allFields.Length; ++index1)
      {
        if (!BehaviorDesignerUtility.HasAttribute(allFields[index1], typeof (NonSerializedAttribute)) && (!allFields[index1].IsPrivate && !allFields[index1].IsFamily || BehaviorDesignerUtility.HasAttribute(allFields[index1], typeof (SerializeField))) && ((!(obj is ParentTask) || !allFields[index1].Name.Equals("children")) && allFields[index1].GetValue(obj) != null))
        {
          string key1 = (allFields[index1].FieldType.Name + allFields[index1].Name).ToString();
          if (typeof (IList).IsAssignableFrom(allFields[index1].FieldType))
          {
            IList list = allFields[index1].GetValue(obj) as IList;
            if (list != null)
            {
              List<object> objectList1 = new List<object>();
              for (int index2 = 0; index2 < list.Count; ++index2)
              {
                if (list[index2] == null)
                {
                  objectList1.Add((object) null);
                }
                else
                {
                  Type type = list[index2].GetType();
                  if (list[index2] is Task)
                  {
                    Task task = list[index2] as Task;
                    objectList1.Add((object) task.get_ID());
                  }
                  else if (list[index2] is SharedVariable)
                    objectList1.Add((object) JSONSerialization.SerializeVariable(list[index2] as SharedVariable, ref unityObjects));
                  else if (list[index2] is Object)
                  {
                    Object @object = list[index2] as Object;
                    if (!object.ReferenceEquals((object) @object, (object) null) && Object.op_Inequality(@object, (Object) null))
                    {
                      objectList1.Add((object) unityObjects.Count);
                      unityObjects.Add(@object);
                    }
                  }
                  else if (type.Equals(typeof (LayerMask)))
                  {
                    List<object> objectList2 = objectList1;
                    LayerMask layerMask = (LayerMask) list[index2];
                    // ISSUE: variable of a boxed type
                    __Boxed<int> local = (ValueType) ((LayerMask) ref layerMask).get_value();
                    objectList2.Add((object) local);
                  }
                  else if (type.IsPrimitive || type.IsEnum || (type.Equals(typeof (string)) || type.Equals(typeof (Vector2))) || (type.Equals(typeof (Vector3)) || type.Equals(typeof (Vector4)) || (type.Equals(typeof (Quaternion)) || type.Equals(typeof (Matrix4x4)))) || (type.Equals(typeof (Color)) || type.Equals(typeof (Rect))))
                  {
                    objectList1.Add(list[index2]);
                  }
                  else
                  {
                    Dictionary<string, object> dict1 = new Dictionary<string, object>();
                    JSONSerialization.SerializeFields(list[index2], ref dict1, ref unityObjects);
                    objectList1.Add((object) dict1);
                  }
                }
              }
              if (objectList1 != null)
                dict.Add(key1, (object) objectList1);
            }
          }
          else if (typeof (Task).IsAssignableFrom(allFields[index1].FieldType))
          {
            Task task = allFields[index1].GetValue(obj) as Task;
            if (task != null)
            {
              if (BehaviorDesignerUtility.HasAttribute(allFields[index1], typeof (InspectTaskAttribute)))
              {
                Dictionary<string, object> dict1 = new Dictionary<string, object>();
                dict1.Add("Type", (object) ((object) task).GetType());
                JSONSerialization.SerializeFields((object) task, ref dict1, ref unityObjects);
                dict.Add(key1, (object) dict1);
              }
              else
                dict.Add(key1, (object) task.get_ID());
            }
          }
          else if (typeof (SharedVariable).IsAssignableFrom(allFields[index1].FieldType))
          {
            if (!dict.ContainsKey(key1))
              dict.Add(key1, (object) JSONSerialization.SerializeVariable(allFields[index1].GetValue(obj) as SharedVariable, ref unityObjects));
          }
          else if (typeof (Object).IsAssignableFrom(allFields[index1].FieldType))
          {
            Object @object = allFields[index1].GetValue(obj) as Object;
            if (!object.ReferenceEquals((object) @object, (object) null) && Object.op_Inequality(@object, (Object) null))
            {
              dict.Add(key1, (object) unityObjects.Count);
              unityObjects.Add(@object);
            }
          }
          else if (allFields[index1].FieldType.Equals(typeof (LayerMask)))
          {
            Dictionary<string, object> dictionary = dict;
            string key2 = key1;
            LayerMask layerMask = (LayerMask) allFields[index1].GetValue(obj);
            // ISSUE: variable of a boxed type
            __Boxed<int> local = (ValueType) ((LayerMask) ref layerMask).get_value();
            dictionary.Add(key2, (object) local);
          }
          else if (allFields[index1].FieldType.IsPrimitive || allFields[index1].FieldType.IsEnum || (allFields[index1].FieldType.Equals(typeof (string)) || allFields[index1].FieldType.Equals(typeof (Vector2))) || (allFields[index1].FieldType.Equals(typeof (Vector3)) || allFields[index1].FieldType.Equals(typeof (Vector4)) || (allFields[index1].FieldType.Equals(typeof (Quaternion)) || allFields[index1].FieldType.Equals(typeof (Matrix4x4)))) || (allFields[index1].FieldType.Equals(typeof (Color)) || allFields[index1].FieldType.Equals(typeof (Rect))))
            dict.Add(key1, allFields[index1].GetValue(obj));
          else if (allFields[index1].FieldType.Equals(typeof (AnimationCurve)))
          {
            AnimationCurve animationCurve = allFields[index1].GetValue(obj) as AnimationCurve;
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            if (animationCurve.get_keys() != null)
            {
              Keyframe[] keys = animationCurve.get_keys();
              List<List<object>> objectListList = new List<List<object>>();
              for (int index2 = 0; index2 < keys.Length; ++index2)
                objectListList.Add(new List<object>()
                {
                  (object) ((Keyframe) ref keys[index2]).get_time(),
                  (object) ((Keyframe) ref keys[index2]).get_value(),
                  (object) ((Keyframe) ref keys[index2]).get_inTangent(),
                  (object) ((Keyframe) ref keys[index2]).get_outTangent(),
                  (object) ((Keyframe) ref keys[index2]).get_tangentMode()
                });
              dictionary.Add("Keys", (object) objectListList);
            }
            dictionary.Add("PreWrapMode", (object) animationCurve.get_preWrapMode());
            dictionary.Add("PostWrapMode", (object) animationCurve.get_postWrapMode());
            dict.Add(key1, (object) dictionary);
          }
          else
          {
            Dictionary<string, object> dict1 = new Dictionary<string, object>();
            JSONSerialization.SerializeFields(allFields[index1].GetValue(obj), ref dict1, ref unityObjects);
            dict.Add(key1, (object) dict1);
          }
        }
      }
    }
  }
}
