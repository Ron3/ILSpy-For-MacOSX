// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.AlphanumComparator`1
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

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
      string empty1 = string.Empty;
      string str1;
      if (x.GetType().IsSubclassOf(typeof (Type)))
      {
        Type t = (object) x as Type;
        string str2 = this.TypePrefix(t) + "/";
        TaskCategoryAttribute[] customAttributes1;
        if ((customAttributes1 = t.GetCustomAttributes(typeof (TaskCategoryAttribute), false) as TaskCategoryAttribute[]).Length > 0)
          str2 = str2 + customAttributes1[0].get_Category() + "/";
        TaskNameAttribute[] customAttributes2;
        str1 = (customAttributes2 = t.GetCustomAttributes(typeof (TaskNameAttribute), false) as TaskNameAttribute[]).Length <= 0 ? str2 + BehaviorDesignerUtility.SplitCamelCase(t.Name.ToString()) : str2 + customAttributes2[0].get_Name();
      }
      else if (x.GetType().IsSubclassOf(typeof (SharedVariable)))
      {
        string s = x.GetType().Name;
        if (s.Length > 6 && s.Substring(0, 6).Equals("Shared"))
          s = s.Substring(6, s.Length - 6);
        str1 = BehaviorDesignerUtility.SplitCamelCase(s);
      }
      else
        str1 = BehaviorDesignerUtility.SplitCamelCase(x.ToString());
      if (str1 == null)
        return 0;
      string empty2 = string.Empty;
      string str3;
      if (y.GetType().IsSubclassOf(typeof (Type)))
      {
        Type t = (object) y as Type;
        string str2 = this.TypePrefix(t) + "/";
        TaskCategoryAttribute[] customAttributes1;
        if ((customAttributes1 = t.GetCustomAttributes(typeof (TaskCategoryAttribute), false) as TaskCategoryAttribute[]).Length > 0)
          str2 = str2 + customAttributes1[0].get_Category() + "/";
        TaskNameAttribute[] customAttributes2;
        str3 = (customAttributes2 = t.GetCustomAttributes(typeof (TaskNameAttribute), false) as TaskNameAttribute[]).Length <= 0 ? str2 + BehaviorDesignerUtility.SplitCamelCase(t.Name.ToString()) : str2 + customAttributes2[0].get_Name();
      }
      else if (y.GetType().IsSubclassOf(typeof (SharedVariable)))
      {
        string s = y.GetType().Name;
        if (s.Length > 6 && s.Substring(0, 6).Equals("Shared"))
          s = s.Substring(6, s.Length - 6);
        str3 = BehaviorDesignerUtility.SplitCamelCase(s);
      }
      else
        str3 = BehaviorDesignerUtility.SplitCamelCase(y.ToString());
      if (str3 == null)
        return 0;
      int length1 = str1.Length;
      int length2 = str3.Length;
      int index1 = 0;
      for (int index2 = 0; index1 < length1 && index2 < length2; ++index2)
      {
        int num;
        if (char.IsDigit(str1[index1]) && char.IsDigit(str1[index2]))
        {
          string empty3 = string.Empty;
          for (; index1 < length1 && char.IsDigit(str1[index1]); ++index1)
            empty3 += (string) (object) str1[index1];
          string empty4 = string.Empty;
          for (; index2 < length2 && char.IsDigit(str3[index2]); ++index2)
            empty4 += (string) (object) str3[index2];
          int result1 = 0;
          int.TryParse(empty3, out result1);
          int result2 = 0;
          int.TryParse(empty4, out result2);
          num = result1.CompareTo(result2);
        }
        else
          num = str1[index1].CompareTo(str3[index2]);
        if (num != 0)
          return num;
        ++index1;
      }
      return length1 - length2;
    }

    private string TypePrefix(Type t)
    {
      if (t.IsSubclassOf(typeof (Action)))
        return "Action";
      if (t.IsSubclassOf(typeof (Composite)))
        return "Composite";
      return t.IsSubclassOf(typeof (Conditional)) ? "Conditional" : "Decorator";
    }
  }
}
