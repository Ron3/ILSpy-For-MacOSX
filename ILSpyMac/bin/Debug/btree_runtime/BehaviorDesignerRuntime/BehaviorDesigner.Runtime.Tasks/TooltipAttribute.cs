using System;

namespace BehaviorDesigner.Runtime.Tasks
{
	[AttributeUsage]
	public class TooltipAttribute : Attribute
	{
		public readonly string mTooltip;

		public string Tooltip
		{
			get
			{
				return this.mTooltip;
			}
		}

		public TooltipAttribute(string tooltip)
		{
			this.mTooltip = tooltip;
		}
	}
}
