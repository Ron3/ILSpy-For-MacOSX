using System;

namespace BehaviorDesigner.Editor
{
	//[AttributeUsage]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class CustomObjectDrawer : Attribute
	{
		private Type type;

		public Type Type
		{
			get
			{
				return this.type;
			}
		}

		public CustomObjectDrawer(Type type)
		{
			this.type = type;
		}
	}
}
