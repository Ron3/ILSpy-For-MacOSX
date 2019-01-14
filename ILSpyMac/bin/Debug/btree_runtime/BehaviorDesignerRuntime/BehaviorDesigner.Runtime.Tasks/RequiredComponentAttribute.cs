using System;

namespace BehaviorDesigner.Runtime.Tasks
{
	[AttributeUsage]
	public class RequiredComponentAttribute : Attribute
	{
		public readonly Type mComponentType;

		public Type ComponentType
		{
			get
			{
				return this.mComponentType;
			}
		}

		public RequiredComponentAttribute(Type componentType)
		{
			this.mComponentType = componentType;
		}
	}
}
