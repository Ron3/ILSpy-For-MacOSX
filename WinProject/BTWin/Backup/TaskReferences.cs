// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.TaskReferences
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
  public class TaskReferences : MonoBehaviour
  {
    public TaskReferences()
    {
      base.\u002Ector();
    }

    public static void CheckReferences(BehaviorSource behaviorSource)
    {
      if (behaviorSource.get_RootTask() != null)
        TaskReferences.CheckReferences(behaviorSource, behaviorSource.get_RootTask());
      if (behaviorSource.get_DetachedTasks() == null)
        return;
      for (int index = 0; index < behaviorSource.get_DetachedTasks().Count; ++index)
        TaskReferences.CheckReferences(behaviorSource, behaviorSource.get_DetachedTasks()[index]);
    }

    private static void CheckReferences(BehaviorSource behaviorSource, Task task)
    {
      FieldInfo[] allFields = TaskUtility.GetAllFields(((object) task).GetType());
      for (int index1 = 0; index1 < allFields.Length; ++index1)
      {
        if (!allFields[index1].FieldType.IsArray && (allFields[index1].FieldType.Equals(typeof (Task)) || allFields[index1].FieldType.IsSubclassOf(typeof (Task))))
        {
          Task referencedTask1 = allFields[index1].GetValue((object) task) as Task;
          if (referencedTask1 != null)
          {
            Task referencedTask2 = TaskReferences.FindReferencedTask(behaviorSource, referencedTask1);
            if (referencedTask2 != null)
              allFields[index1].SetValue((object) task, (object) referencedTask2);
          }
        }
        else if (allFields[index1].FieldType.IsArray && (allFields[index1].FieldType.GetElementType().Equals(typeof (Task)) || allFields[index1].FieldType.GetElementType().IsSubclassOf(typeof (Task))))
        {
          Task[] taskArray = allFields[index1].GetValue((object) task) as Task[];
          if (taskArray != null)
          {
            IList instance1 = Activator.CreateInstance(typeof (List<>).MakeGenericType(allFields[index1].FieldType.GetElementType())) as IList;
            for (int index2 = 0; index2 < taskArray.Length; ++index2)
            {
              Task referencedTask = TaskReferences.FindReferencedTask(behaviorSource, taskArray[index2]);
              if (referencedTask != null)
                instance1.Add((object) referencedTask);
            }
            Array instance2 = Array.CreateInstance(allFields[index1].FieldType.GetElementType(), instance1.Count);
            instance1.CopyTo(instance2, 0);
            allFields[index1].SetValue((object) task, (object) instance2);
          }
        }
      }
      if (!((object) task).GetType().IsSubclassOf(typeof (ParentTask)))
        return;
      ParentTask parentTask = task as ParentTask;
      if (parentTask.get_Children() == null)
        return;
      for (int index = 0; index < parentTask.get_Children().Count; ++index)
        TaskReferences.CheckReferences(behaviorSource, parentTask.get_Children()[index]);
    }

    private static Task FindReferencedTask(BehaviorSource behaviorSource, Task referencedTask)
    {
      if (referencedTask == null)
        return (Task) null;
      int id = referencedTask.get_ID();
      Task referencedTask1;
      if (behaviorSource.get_RootTask() != null && (referencedTask1 = TaskReferences.FindReferencedTask(behaviorSource.get_RootTask(), id)) != null)
        return referencedTask1;
      if (behaviorSource.get_DetachedTasks() != null)
      {
        for (int index = 0; index < behaviorSource.get_DetachedTasks().Count; ++index)
        {
          Task referencedTask2;
          if ((referencedTask2 = TaskReferences.FindReferencedTask(behaviorSource.get_DetachedTasks()[index], id)) != null)
            return referencedTask2;
        }
      }
      return (Task) null;
    }

    private static Task FindReferencedTask(Task task, int referencedTaskID)
    {
      if (task.get_ID() == referencedTaskID)
        return task;
      if (((object) task).GetType().IsSubclassOf(typeof (ParentTask)))
      {
        ParentTask parentTask = task as ParentTask;
        if (parentTask.get_Children() != null)
        {
          for (int index = 0; index < parentTask.get_Children().Count; ++index)
          {
            Task referencedTask;
            if ((referencedTask = TaskReferences.FindReferencedTask(parentTask.get_Children()[index], referencedTaskID)) != null)
              return referencedTask;
          }
        }
      }
      return (Task) null;
    }

    public static void CheckReferences(Behavior behavior, List<Task> taskList)
    {
      for (int index = 0; index < taskList.Count; ++index)
        TaskReferences.CheckReferences(behavior, taskList[index], taskList);
    }

    private static void CheckReferences(Behavior behavior, Task task, List<Task> taskList)
    {
      if (TaskUtility.CompareType(((object) task).GetType(), "BehaviorDesigner.Runtime.Tasks.ConditionalEvaluator"))
      {
        object obj = ((object) task).GetType().GetField("conditionalTask").GetValue((object) task);
        if (obj != null)
          task = obj as Task;
      }
      FieldInfo[] allFields = TaskUtility.GetAllFields(((object) task).GetType());
      for (int index1 = 0; index1 < allFields.Length; ++index1)
      {
        if (!allFields[index1].FieldType.IsArray && (allFields[index1].FieldType.Equals(typeof (Task)) || allFields[index1].FieldType.IsSubclassOf(typeof (Task))))
        {
          Task referencedTask1 = allFields[index1].GetValue((object) task) as Task;
          if (referencedTask1 != null && !((Object) referencedTask1.get_Owner()).Equals((object) behavior))
          {
            Task referencedTask2 = TaskReferences.FindReferencedTask(referencedTask1, taskList);
            if (referencedTask2 != null)
              allFields[index1].SetValue((object) task, (object) referencedTask2);
          }
        }
        else if (allFields[index1].FieldType.IsArray && (allFields[index1].FieldType.GetElementType().Equals(typeof (Task)) || allFields[index1].FieldType.GetElementType().IsSubclassOf(typeof (Task))))
        {
          Task[] taskArray = allFields[index1].GetValue((object) task) as Task[];
          if (taskArray != null)
          {
            IList instance1 = Activator.CreateInstance(typeof (List<>).MakeGenericType(allFields[index1].FieldType.GetElementType())) as IList;
            for (int index2 = 0; index2 < taskArray.Length; ++index2)
            {
              Task referencedTask = TaskReferences.FindReferencedTask(taskArray[index2], taskList);
              if (referencedTask != null)
                instance1.Add((object) referencedTask);
            }
            Array instance2 = Array.CreateInstance(allFields[index1].FieldType.GetElementType(), instance1.Count);
            instance1.CopyTo(instance2, 0);
            allFields[index1].SetValue((object) task, (object) instance2);
          }
        }
      }
    }

    private static Task FindReferencedTask(Task referencedTask, List<Task> taskList)
    {
      int referenceId = referencedTask.get_ReferenceID();
      for (int index = 0; index < taskList.Count; ++index)
      {
        if (taskList[index].get_ReferenceID() == referenceId)
          return taskList[index];
      }
      return (Task) null;
    }
  }
}
