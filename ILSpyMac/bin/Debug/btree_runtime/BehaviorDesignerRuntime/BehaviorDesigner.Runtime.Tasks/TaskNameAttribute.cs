using System;

namespace BehaviorDesigner.Runtime.Tasks
{
	[AttributeUsage]
	public class TaskNameAttribute : Attribute
	{
		public readonly string mName;

		public string Name
		{
			get
			{
				return this.mName;
			}
		}

		public TaskNameAttribute(string name)
		{
			this.mName = name;
		}
	}
}
