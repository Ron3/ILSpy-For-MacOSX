using System;

namespace BehaviorDesigner.Runtime.Tasks
{
	[AttributeUsage]
	public class HelpURLAttribute : Attribute
	{
		private readonly string mURL;

		public string URL
		{
			get
			{
				return this.mURL;
			}
		}

		public HelpURLAttribute(string url)
		{
			this.mURL = url;
		}
	}
}
