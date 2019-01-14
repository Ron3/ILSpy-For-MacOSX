using System;

namespace BehaviorDesigner.Runtime.Tasks
{
	//[AttributeUsage]
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public abstract class ObjectDrawerAttribute : Attribute
	{
	}
}
