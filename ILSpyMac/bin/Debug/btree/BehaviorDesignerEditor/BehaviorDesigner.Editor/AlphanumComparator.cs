using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;

namespace BehaviorDesigner.Editor
{
	public class AlphanumComparator<T> : IComparer<T>
	{
		public int Compare(T x, T y)
		{
			string text = string.Empty;
			if (x.GetType().IsSubclassOf(typeof(Type)))
			{
				Type type = x as Type;
				text = this.TypePrefix(type) + "/";
				TaskCategoryAttribute[] array;
				if ((array = (type.GetCustomAttributes(typeof(TaskCategoryAttribute), false) as TaskCategoryAttribute[])).Length > 0)
				{
					text = text + array[0].get_Category() + "/";
				}
				TaskNameAttribute[] array2;
				if ((array2 = (type.GetCustomAttributes(typeof(TaskNameAttribute), false) as TaskNameAttribute[])).Length > 0)
				{
					text += array2[0].get_Name();
				}
				else
				{
					text += BehaviorDesignerUtility.SplitCamelCase(type.get_Name().ToString());
				}
			}
			else if (x.GetType().IsSubclassOf(typeof(SharedVariable)))
			{
				string text2 = x.GetType().get_Name();
				if (text2.get_Length() > 6 && text2.Substring(0, 6).Equals("Shared"))
				{
					text2 = text2.Substring(6, text2.get_Length() - 6);
				}
				text = BehaviorDesignerUtility.SplitCamelCase(text2);
			}
			else
			{
				text = BehaviorDesignerUtility.SplitCamelCase(x.ToString());
			}
			if (text == null)
			{
				return 0;
			}
			string text3 = string.Empty;
			if (y.GetType().IsSubclassOf(typeof(Type)))
			{
				Type type2 = y as Type;
				text3 = this.TypePrefix(type2) + "/";
				TaskCategoryAttribute[] array3;
				if ((array3 = (type2.GetCustomAttributes(typeof(TaskCategoryAttribute), false) as TaskCategoryAttribute[])).Length > 0)
				{
					text3 = text3 + array3[0].get_Category() + "/";
				}
				TaskNameAttribute[] array4;
				if ((array4 = (type2.GetCustomAttributes(typeof(TaskNameAttribute), false) as TaskNameAttribute[])).Length > 0)
				{
					text3 += array4[0].get_Name();
				}
				else
				{
					text3 += BehaviorDesignerUtility.SplitCamelCase(type2.get_Name().ToString());
				}
			}
			else if (y.GetType().IsSubclassOf(typeof(SharedVariable)))
			{
				string text4 = y.GetType().get_Name();
				if (text4.get_Length() > 6 && text4.Substring(0, 6).Equals("Shared"))
				{
					text4 = text4.Substring(6, text4.get_Length() - 6);
				}
				text3 = BehaviorDesignerUtility.SplitCamelCase(text4);
			}
			else
			{
				text3 = BehaviorDesignerUtility.SplitCamelCase(y.ToString());
			}
			if (text3 == null)
			{
				return 0;
			}
			int length = text.get_Length();
			int length2 = text3.get_Length();
			int num = 0;
			int num2 = 0;
			while (num < length && num2 < length2)
			{
				int num5;
				if (char.IsDigit(text.get_Chars(num)) && char.IsDigit(text.get_Chars(num2)))
				{
					string text5 = string.Empty;
					while (num < length && char.IsDigit(text.get_Chars(num)))
					{
						text5 += text.get_Chars(num);
						num++;
					}
					string text6 = string.Empty;
					while (num2 < length2 && char.IsDigit(text3.get_Chars(num2)))
					{
						text6 += text3.get_Chars(num2);
						num2++;
					}
					int num3 = 0;
					int.TryParse(text5, ref num3);
					int num4 = 0;
					int.TryParse(text6, ref num4);
					num5 = num3.CompareTo(num4);
				}
				else
				{
					num5 = text.get_Chars(num).CompareTo(text3.get_Chars(num2));
				}
				if (num5 != 0)
				{
					return num5;
				}
				num++;
				num2++;
			}
			return length - length2;
		}

		private string TypePrefix(Type t)
		{
			if (t.IsSubclassOf(typeof(Action)))
			{
				return "Action";
			}
			if (t.IsSubclassOf(typeof(Composite)))
			{
				return "Composite";
			}
			if (t.IsSubclassOf(typeof(Conditional)))
			{
				return "Conditional";
			}
			return "Decorator";
		}
	}
}
