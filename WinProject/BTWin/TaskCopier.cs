// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.TaskCopier
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  public class TaskCopier : UnityEditor.Editor
  {
    public TaskCopier()
    {
      base.\u002Ector();
    }

    public static TaskSerializer CopySerialized(Task task)
    {
      TaskSerializer taskSerializer = new TaskSerializer()
      {
        offset = Vector2.op_Addition((task.get_NodeData().get_NodeDesigner() as NodeDesigner).GetAbsolutePosition(), new Vector2(10f, 10f)),
        unityObjects = new List<Object>()
      };
      taskSerializer.serialization = MiniJSON.Serialize((object) JSONSerialization.SerializeTask(task, false, ref taskSerializer.unityObjects));
      return taskSerializer;
    }

    public static Task PasteTask(BehaviorSource behaviorSource, TaskSerializer serializer)
    {
      Dictionary<int, Task> dictionary = new Dictionary<int, Task>();
      JSONDeserialization.set_TaskIDs(new Dictionary<JSONDeserialization.TaskField, List<int>>());
      Task task1 = JSONDeserialization.DeserializeTask(behaviorSource, MiniJSON.Deserialize(serializer.serialization) as Dictionary<string, object>, ref dictionary, serializer.unityObjects);
      TaskCopier.CheckSharedVariables(behaviorSource, task1);
      if (JSONDeserialization.get_TaskIDs().Count > 0)
      {
        using (Dictionary<JSONDeserialization.TaskField, List<int>>.KeyCollection.Enumerator enumerator = JSONDeserialization.get_TaskIDs().Keys.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            JSONDeserialization.TaskField current = enumerator.Current;
            List<int> taskId = JSONDeserialization.get_TaskIDs()[current];
            Type fieldType = ((FieldInfo) current.fieldInfo).FieldType;
            if (((FieldInfo) current.fieldInfo).FieldType.IsArray)
            {
              int length = 0;
              for (int index = 0; index < taskId.Count; ++index)
              {
                Task task2 = TaskCopier.TaskWithID(behaviorSource, taskId[index]);
                if (task2 != null && ((object) task2).GetType().Equals(fieldType.GetElementType()) || ((object) task2).GetType().IsSubclassOf(fieldType.GetElementType()))
                  ++length;
              }
              Array instance = Array.CreateInstance(fieldType.GetElementType(), length);
              int index1 = 0;
              for (int index2 = 0; index2 < taskId.Count; ++index2)
              {
                Task task2 = TaskCopier.TaskWithID(behaviorSource, taskId[index2]);
                if (task2 != null && ((object) task2).GetType().Equals(fieldType.GetElementType()) || ((object) task2).GetType().IsSubclassOf(fieldType.GetElementType()))
                {
                  instance.SetValue((object) task2, index1);
                  ++index1;
                }
              }
              ((FieldInfo) current.fieldInfo).SetValue((object) current.task, (object) instance);
            }
            else
            {
              Task task2 = TaskCopier.TaskWithID(behaviorSource, taskId[0]);
              if (task2 != null && ((object) task2).GetType().Equals(((FieldInfo) current.fieldInfo).FieldType) || ((object) task2).GetType().IsSubclassOf(((FieldInfo) current.fieldInfo).FieldType))
                ((FieldInfo) current.fieldInfo).SetValue((object) current.task, (object) task2);
            }
          }
        }
        JSONDeserialization.set_TaskIDs((Dictionary<JSONDeserialization.TaskField, List<int>>) null);
      }
      return task1;
    }

    private static void CheckSharedVariables(BehaviorSource behaviorSource, Task task)
    {
      if (task == null)
        return;
      TaskCopier.CheckSharedVariableFields(behaviorSource, task, (object) task);
      if (!(task is ParentTask))
        return;
      ParentTask parentTask = task as ParentTask;
      if (parentTask.get_Children() == null)
        return;
      for (int index = 0; index < parentTask.get_Children().Count; ++index)
        TaskCopier.CheckSharedVariables(behaviorSource, parentTask.get_Children()[index]);
    }

    private static void CheckSharedVariableFields(
      BehaviorSource behaviorSource,
      Task task,
      object obj)
    {
      if (obj == null)
        return;
      FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
      for (int index = 0; index < allFields.Length; ++index)
      {
        if (typeof (SharedVariable).IsAssignableFrom(allFields[index].FieldType))
        {
          SharedVariable sharedVariable = allFields[index].GetValue(obj) as SharedVariable;
          if (sharedVariable != null)
          {
            if (sharedVariable.get_IsShared() && !sharedVariable.get_IsGlobal() && (!string.IsNullOrEmpty(sharedVariable.get_Name()) && behaviorSource.GetVariable(sharedVariable.get_Name()) == null))
              behaviorSource.SetVariable(sharedVariable.get_Name(), sharedVariable);
            TaskCopier.CheckSharedVariableFields(behaviorSource, task, (object) sharedVariable);
          }
        }
        else if (allFields[index].FieldType.IsClass && !allFields[index].FieldType.Equals(typeof (Type)) && !typeof (Delegate).IsAssignableFrom(allFields[index].FieldType))
          TaskCopier.CheckSharedVariableFields(behaviorSource, task, allFields[index].GetValue(obj));
      }
    }

    private static Task TaskWithID(BehaviorSource behaviorSource, int id)
    {
      Task task = (Task) null;
      if (behaviorSource.get_RootTask() != null)
        task = TaskCopier.TaskWithID(id, behaviorSource.get_RootTask());
      if (task == null && behaviorSource.get_DetachedTasks() != null)
      {
        int index = 0;
        while (index < behaviorSource.get_DetachedTasks().Count && (task = TaskCopier.TaskWithID(id, behaviorSource.get_DetachedTasks()[index])) == null)
          ++index;
      }
      return task;
    }

    private static Task TaskWithID(int id, Task task)
    {
      if (task == null)
        return (Task) null;
      if (task.get_ID() == id)
        return task;
      if (task is ParentTask)
      {
        ParentTask parentTask = task as ParentTask;
        if (parentTask.get_Children() != null)
        {
          for (int index = 0; index < parentTask.get_Children().Count; ++index)
          {
            Task task1 = TaskCopier.TaskWithID(id, parentTask.get_Children()[index]);
            if (task1 != null)
              return task1;
          }
        }
      }
      return (Task) null;
    }
  }
}
