using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public class TaskCopier : UnityEditor.Editor
	{
		public static TaskSerializer CopySerialized(Task task)
		{
			TaskSerializer taskSerializer = new TaskSerializer();
			taskSerializer.offset = (task.NodeData.NodeDesigner as NodeDesigner).GetAbsolutePosition() + new Vector2(10f, 10f);
			taskSerializer.unityObjects = new List<Object>();
			taskSerializer.serialization = MiniJSON.Serialize(JSONSerialization.SerializeTask(task, false, ref taskSerializer.unityObjects));
			return taskSerializer;
		}

		public static Task PasteTask(BehaviorSource behaviorSource, TaskSerializer serializer)
		{
			Dictionary<int, Task> dictionary = new Dictionary<int, Task>();
			JSONDeserialization.TaskIDs=new Dictionary<JSONDeserialization.TaskField, List<int>>();
			Task task = JSONDeserialization.DeserializeTask(behaviorSource, MiniJSON.Deserialize(serializer.serialization) as Dictionary<string, object>, ref dictionary, serializer.unityObjects);
			TaskCopier.CheckSharedVariables(behaviorSource, task);
			if (JSONDeserialization.TaskIDs.Count > 0)
			{
				using (Dictionary<JSONDeserialization.TaskField, List<int>>.KeyCollection.Enumerator enumerator = JSONDeserialization.TaskIDs.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						JSONDeserialization.TaskField current = enumerator.Current;
						List<int> list = JSONDeserialization.TaskIDs[current];
						Type fieldType = current.fieldInfo.FieldType;
						if (current.fieldInfo.FieldType.IsArray)
						{
							int num = 0;
							for (int i = 0; i < list.Count; i++)
							{
								Task task2 = TaskCopier.TaskWithID(behaviorSource, list[i]);
								if ((task2 != null && task2.GetType().Equals(fieldType.GetElementType())) || task2.GetType().IsSubclassOf(fieldType.GetElementType()))
								{
									num++;
								}
							}
							Array array = Array.CreateInstance(fieldType.GetElementType(), num);
							int num2 = 0;
							for (int j = 0; j < list.Count; j++)
							{
								Task task3 = TaskCopier.TaskWithID(behaviorSource, list[j]);
								if ((task3 != null && task3.GetType().Equals(fieldType.GetElementType())) || task3.GetType().IsSubclassOf(fieldType.GetElementType()))
								{
									array.SetValue(task3, num2);
									num2++;
								}
							}
							current.fieldInfo.SetValue(current.task, array);
						}
						else
						{
							Task task4 = TaskCopier.TaskWithID(behaviorSource, list[0]);
							if ((task4 != null && task4.GetType().Equals(current.fieldInfo.FieldType)) || task4.GetType().IsSubclassOf(current.fieldInfo.FieldType))
							{
								current.fieldInfo.SetValue(current.task, task4);
							}
						}
					}
				}
				JSONDeserialization.TaskIDs=null;
			}
			return task;
		}

		private static void CheckSharedVariables(BehaviorSource behaviorSource, Task task)
		{
			if (task == null)
			{
				return;
			}
			TaskCopier.CheckSharedVariableFields(behaviorSource, task, task);
			if (task is ParentTask)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						TaskCopier.CheckSharedVariables(behaviorSource, parentTask.Children[i]);
					}
				}
			}
		}

		private static void CheckSharedVariableFields(BehaviorSource behaviorSource, Task task, object obj)
		{
			if (obj == null)
			{
				return;
			}
			FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				if (typeof(SharedVariable).IsAssignableFrom(allFields[i].FieldType))
				{
					SharedVariable sharedVariable = allFields[i].GetValue(obj) as SharedVariable;
					if (sharedVariable != null)
					{
						if (sharedVariable.IsShared && !sharedVariable.IsGlobal && !string.IsNullOrEmpty(sharedVariable.Name) && behaviorSource.GetVariable(sharedVariable.Name) == null)
						{
							behaviorSource.SetVariable(sharedVariable.Name, sharedVariable);
						}
						TaskCopier.CheckSharedVariableFields(behaviorSource, task, sharedVariable);
					}
				}
				else if (allFields[i].FieldType.IsClass && !allFields[i].FieldType.Equals(typeof(Type)) && !typeof(Delegate).IsAssignableFrom(allFields[i].FieldType))
				{
					TaskCopier.CheckSharedVariableFields(behaviorSource, task, allFields[i].GetValue(obj));
				}
			}
		}

		private static Task TaskWithID(BehaviorSource behaviorSource, int id)
		{
			Task task = null;
			if (behaviorSource.RootTask != null)
			{
				task = TaskCopier.TaskWithID(id, behaviorSource.RootTask);
			}
			if (task == null && behaviorSource.DetachedTasks != null)
			{
				for (int i = 0; i < behaviorSource.DetachedTasks.Count; i++)
				{
					if ((task = TaskCopier.TaskWithID(id, behaviorSource.DetachedTasks[i])) != null)
					{
						break;
					}
				}
			}
			return task;
		}

		private static Task TaskWithID(int id, Task task)
		{
			if (task == null)
			{
				return null;
			}
			if (task.ID == id)
			{
				return task;
			}
			if (task is ParentTask)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						Task task2 = TaskCopier.TaskWithID(id, parentTask.Children[i]);
						if (task2 != null)
						{
							return task2;
						}
					}
				}
			}
			return null;
		}
	}
}
