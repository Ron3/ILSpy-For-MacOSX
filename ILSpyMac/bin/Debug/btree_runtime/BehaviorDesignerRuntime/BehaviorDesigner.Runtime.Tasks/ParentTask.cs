using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	public abstract class ParentTask : Task
	{
		[SerializeField]
		protected List<Task> children;

		public List<Task> Children
		{
			get
			{
				return this.children;
			}
			private set
			{
				this.children = value;
			}
		}

		public virtual int MaxChildren()
		{
			return 2147483647;
		}

		public virtual bool CanRunParallelChildren()
		{
			return false;
		}

		public virtual int CurrentChildIndex()
		{
			return 0;
		}

		public virtual bool CanExecute()
		{
			return true;
		}

		public virtual TaskStatus Decorate(TaskStatus status)
		{
			return status;
		}

		public virtual bool CanReevaluate()
		{
			return false;
		}

		public virtual bool OnReevaluationStarted()
		{
			return false;
		}

		public virtual void OnReevaluationEnded(TaskStatus status)
		{
		}

		public virtual void OnChildExecuted(TaskStatus childStatus)
		{
		}

		public virtual void OnChildExecuted(int childIndex, TaskStatus childStatus)
		{
		}

		public virtual void OnChildStarted()
		{
		}

		public virtual void OnChildStarted(int childIndex)
		{
		}

		public virtual TaskStatus OverrideStatus(TaskStatus status)
		{
			return status;
		}

		public virtual TaskStatus OverrideStatus()
		{
			return TaskStatus.Running;
		}

		public virtual void OnConditionalAbort(int childIndex)
		{
		}

		public override float GetUtility()
		{
			float num = 0f;
			if (this.children != null)
			{
				for (int i = 0; i < this.children.Count; i++)
				{
					if (this.children.get_Item(i) != null && !this.children.get_Item(i).Disabled)
					{
						num += this.children.get_Item(i).GetUtility();
					}
				}
			}
			return num;
		}

		public override void OnDrawGizmos()
		{
			if (this.children != null)
			{
				for (int i = 0; i < this.children.Count; i++)
				{
					if (this.children.get_Item(i) != null && !this.children.get_Item(i).Disabled)
					{
						this.children.get_Item(i).OnDrawGizmos();
					}
				}
			}
		}

		public void AddChild(Task child, int index)
		{
			if (this.children == null)
			{
				this.children = new List<Task>();
			}
			this.children.Insert(index, child);
		}

		public void ReplaceAddChild(Task child, int index)
		{
			if (this.children != null && index < this.children.Count)
			{
				//this.children.Item=index, child;
				this.children[index] = child;
			}
			else
			{
				this.AddChild(child, index);
			}
		}
	}
}
