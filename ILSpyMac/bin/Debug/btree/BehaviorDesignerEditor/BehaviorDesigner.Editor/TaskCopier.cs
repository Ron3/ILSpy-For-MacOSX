using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public class TaskCopier : Editor
	{
		public static TaskSerializer CopySerialized(Task task)
		{
			TaskSerializer taskSerializer = new TaskSerializer();
			taskSerializer.offset = (task.get_NodeData().get_NodeDesigner() as NodeDesigner).GetAbsolutePosition() + new Vector2(10f, 10f);
			taskSerializer.unityObjects = new List<Object>();
			taskSerializer.serialization = MiniJSON.Serialize(JSONSerialization.SerializeTask(task, false, ref taskSerializer.unityObjects));
			return taskSerializer;
		}

		public static Task PasteTask(BehaviorSource behaviorSource, TaskSerializer serializer)
		{
			Dictionary<int, Task> dictionary = new Dictionary<int, Task>();
			JSONDeserialization.set_TaskIDs(new Dictionary<JSONDeserialization.TaskField, List<int>>());
			Task task = JSONDeserialization.DeserializeTask(behaviorSource, MiniJSON.Deserialize(serializer.serialization) as Dictionary<string, object>, ref dictionary, serializer.unityObjects);
			TaskCopier.CheckSharedVariables(behaviorSource, task);
			if (JSONDeserialization.get_TaskIDs().get_Count() > 0)
			{
				using (Dictionary<JSONDeserialization.TaskField, List<int>>.KeyCollection.Enumerator enumerator = JSONDeserialization.get_TaskIDs().get_Keys().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						JSONDeserialization.TaskField current = enumerator.get_Current();
						List<int> list = JSONDeserialization.get_TaskIDs().get_Item(current);
						Type fieldType = current.fieldInfo.get_FieldType();
						if (current.fieldInfo.get_FieldType().get_IsArray())
						{
							int num = 0;
							for (int i = 0; i < list.get_Count(); i++)
							{
								Task task2 = TaskCopier.TaskWithID(behaviorSource, list.get_Item(i));
								if ((task2 != null && task2.GetType().Equals(fieldType.GetElementType())) || task2.GetType().IsSubclassOf(fieldType.GetElementType()))
								{
									num++;
								}
							}
							Array array = Array.CreateInstance(fieldType.GetElementType(), num);
							int num2 = 0;
							for (int j = 0; j < list.get_Count(); j++)
							{
								Task task3 = TaskCopier.TaskWithID(behaviorSource, list.get_Item(j));
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
							Task task4 = TaskCopier.TaskWithID(behaviorSource, list.get_Item(0));
							if ((task4 != null && task4.GetType().Equals(current.fieldInfo.get_FieldType())) || task4.GetType().IsSubclassOf(current.fieldInfo.get_FieldType()))
							{
								current.fieldInfo.SetValue(current.task, task4);
							}
						}
					}
				}
				JSONDeserialization.set_TaskIDs(null);
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
				if (parentTask.get_Children() != null)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						TaskCopier.CheckSharedVariables(behaviorSource, parentTask.get_Children().get_Item(i));
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
				if (typeof(SharedVariable).IsAssignableFrom(allFields[i].get_FieldType()))
				{
					SharedVariable sharedVariable = allFields[i].GetValue(obj) as SharedVariable;
					if (sharedVariable != null)
					{
						if (sharedVariable.get_IsShared() && !sharedVariable.get_IsGlobal() && !string.IsNullOrEmpty(sharedVariable.get_Name()) && behaviorSource.GetVariable(sharedVariable.get_Name()) == null)
						{
							behaviorSource.SetVariable(sharedVariable.get_Name(), sharedVariable);
						}
						TaskCopier.CheckSharedVariableFields(behaviorSource, task, sharedVariable);
					}
				}
				else if (allFields[i].get_FieldType().get_IsClass() && !allFields[i].get_FieldType().Equals(typeof(Type)) && !typeof(Delegate).IsAssignableFrom(allFields[i].get_FieldType()))
				{
					TaskCopier.CheckSharedVariableFields(behaviorSource, task, allFields[i].GetValue(obj));
				}
			}
		}

		private static Task TaskWithID(BehaviorSource behaviorSource, int id)
		{
			Task task = null;
			if (behaviorSource.get_RootTask() != null)
			{
				task = TaskCopier.TaskWithID(id, behaviorSource.get_RootTask());
			}
			if (task == null && behaviorSource.get_DetachedTasks() != null)
			{
				for (int i = 0; i < behaviorSource.get_DetachedTasks().get_Count(); i++)
				{
					if ((task = TaskCopier.TaskWithID(id, behaviorSource.get_DetachedTasks().get_Item(i))) != null)
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
			if (task.get_ID() == id)
			{
				return task;
			}
			if (task is ParentTask)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.get_Children() != null)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						Task task2 = TaskCopier.TaskWithID(id, parentTask.get_Children().get_Item(i));
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
