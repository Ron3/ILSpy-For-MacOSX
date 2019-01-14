using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public abstract class ExternalBehavior : ScriptableObject, IBehavior
	{
		[SerializeField]
		private BehaviorSource mBehaviorSource;

		private bool mInitialized;

		public BehaviorSource BehaviorSource
		{
			get
			{
				return this.mBehaviorSource;
			}
			set
			{
				this.mBehaviorSource = value;
			}
		}

		public bool Initialized
		{
			get
			{
				return this.mInitialized;
			}
		}

		public BehaviorSource GetBehaviorSource()
		{
			return this.mBehaviorSource;
		}

		public void SetBehaviorSource(BehaviorSource behaviorSource)
		{
			this.mBehaviorSource = behaviorSource;
		}

		public Object GetObject()
		{
			return this;
		}

		public string GetOwnerName()
		{
			return base.name;
		}

		public void Init()
		{
			this.CheckForSerialization();
			this.mInitialized = true;
		}

		public SharedVariable GetVariable(string name)
		{
			this.CheckForSerialization();
			return this.mBehaviorSource.GetVariable(name);
		}

		public void SetVariable(string name, SharedVariable item)
		{
			this.CheckForSerialization();
			this.mBehaviorSource.SetVariable(name, item);
		}

		public void SetVariableValue(string name, object value)
		{
			SharedVariable variable = this.GetVariable(name);
			if (variable != null)
			{
				variable.SetValue(value);
				variable.ValueChanged();
			}
		}

		public T FindTask<T>() where T : Task
		{
			this.CheckForSerialization();
			return this.FindTask<T>(this.mBehaviorSource.RootTask);
		}

		private T FindTask<T>(Task task) where T : Task
		{
			if (task.GetType().Equals(typeof(T)))
			{
				return (T)((object)task);
			}
			ParentTask parentTask;
			if ((parentTask = (task as ParentTask)) != null && parentTask.Children != null)
			{
				for (int i = 0; i < parentTask.Children.Count; i++)
				{
					T result = (T)((object)null);
					if ((result = this.FindTask<T>(parentTask.Children.get_Item(i))) != null)
					{
						return result;
					}
				}
			}
			return (T)((object)null);
		}

		public List<T> FindTasks<T>() where T : Task
		{
			this.CheckForSerialization();
			List<T> result = new List<T>();
			this.FindTasks<T>(this.mBehaviorSource.RootTask, ref result);
			return result;
		}

		private void FindTasks<T>(Task task, ref List<T> taskList) where T : Task
		{
			if (typeof(T).IsAssignableFrom(task.GetType()))
			{
				taskList.Add((T)((object)task));
			}
			ParentTask parentTask;
			if ((parentTask = (task as ParentTask)) != null && parentTask.Children != null)
			{
				for (int i = 0; i < parentTask.Children.Count; i++)
				{
					this.FindTasks<T>(parentTask.Children.get_Item(i), ref taskList);
				}
			}
		}

		public Task FindTaskWithName(string taskName)
		{
			this.CheckForSerialization();
			return this.FindTaskWithName(taskName, this.mBehaviorSource.RootTask);
		}

		private void CheckForSerialization()
		{
			this.mBehaviorSource.Owner = this;
			this.mBehaviorSource.CheckForSerialization(false, null);
		}

		private Task FindTaskWithName(string taskName, Task task)
		{
			if (task.FriendlyName.Equals(taskName))
			{
				return task;
			}
			ParentTask parentTask;
			if ((parentTask = (task as ParentTask)) != null && parentTask.Children != null)
			{
				for (int i = 0; i < parentTask.Children.Count; i++)
				{
					Task result;
					if ((result = this.FindTaskWithName(taskName, parentTask.Children.get_Item(i))) != null)
					{
						return result;
					}
				}
			}
			return null;
		}

		public List<Task> FindTasksWithName(string taskName)
		{
			List<Task> result = new List<Task>();
			this.FindTasksWithName(taskName, this.mBehaviorSource.RootTask, ref result);
			return result;
		}

		private void FindTasksWithName(string taskName, Task task, ref List<Task> taskList)
		{
			if (task.FriendlyName.Equals(taskName))
			{
				taskList.Add(task);
			}
			ParentTask parentTask;
			if ((parentTask = (task as ParentTask)) != null && parentTask.Children != null)
			{
				for (int i = 0; i < parentTask.Children.Count; i++)
				{
					this.FindTasksWithName(taskName, parentTask.Children.get_Item(i), ref taskList);
				}
			}
		}

		public int GetInstanceID()
		{
			return base.GetInstanceID();
		}
	}
}
