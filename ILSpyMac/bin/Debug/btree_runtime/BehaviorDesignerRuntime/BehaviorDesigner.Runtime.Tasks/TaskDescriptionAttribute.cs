using System;

namespace BehaviorDesigner.Runtime.Tasks
{
	[AttributeUsage]
	public class TaskDescriptionAttribute : Attribute
	{
		public readonly string mDescription;

		public string Description
		{
			get
			{
				return this.mDescription;
			}
		}

		public TaskDescriptionAttribute(string description)
		{
			this.mDescription = description;
		}
	}
}
