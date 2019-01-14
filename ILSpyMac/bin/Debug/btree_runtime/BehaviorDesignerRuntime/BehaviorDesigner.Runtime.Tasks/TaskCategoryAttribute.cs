using System;

namespace BehaviorDesigner.Runtime.Tasks
{
	[AttributeUsage]
	public class TaskCategoryAttribute : Attribute
	{
		public readonly string mCategory;

		public string Category
		{
			get
			{
				return this.mCategory;
			}
		}

		public TaskCategoryAttribute(string category)
		{
			this.mCategory = category;
		}
	}
}
