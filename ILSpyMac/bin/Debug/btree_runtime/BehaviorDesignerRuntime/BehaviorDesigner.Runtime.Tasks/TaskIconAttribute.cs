using System;

namespace BehaviorDesigner.Runtime.Tasks
{
	[AttributeUsage]
	public class TaskIconAttribute : Attribute
	{
		public readonly string mIconPath;

		public string IconPath
		{
			get
			{
				return this.mIconPath;
			}
		}

		public TaskIconAttribute(string iconPath)
		{
			this.mIconPath = iconPath;
		}
	}
}
