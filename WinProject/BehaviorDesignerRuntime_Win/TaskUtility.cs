// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.TaskUtility
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BehaviorDesigner.Runtime
{
  public class TaskUtility
  {
    [NonSerialized]
    private static Dictionary<string, Type> typeLookup = new Dictionary<string, Type>();
    private static List<string> loadedAssemblies = (List<string>) null;
    private static Dictionary<Type, FieldInfo[]> allFieldsLookup = new Dictionary<Type, FieldInfo[]>();
    private static Dictionary<Type, FieldInfo[]> publicFieldsLookup = new Dictionary<Type, FieldInfo[]>();
    private static Dictionary<FieldInfo, Dictionary<Type, bool>> hasFieldLookup = new Dictionary<FieldInfo, Dictionary<Type, bool>>();

    public static object CreateInstance(Type t)
    {
      if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof (Nullable<>))
        t = Nullable.GetUnderlyingType(t);
      return Activator.CreateInstance(t, true);
    }

    public static FieldInfo[] GetAllFields(Type t)
    {
      FieldInfo[] fieldInfoArray = (FieldInfo[]) null;
      if (!TaskUtility.allFieldsLookup.TryGetValue(t, out fieldInfoArray))
      {
        List<FieldInfo> fieldList = ObjectPool.Get<List<FieldInfo>>();
        fieldList.Clear();
        BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        TaskUtility.GetFields(t, ref fieldList, (int) bindingFlags);
        fieldInfoArray = fieldList.ToArray();
        ObjectPool.Return<List<FieldInfo>>(fieldList);
        TaskUtility.allFieldsLookup.Add(t, fieldInfoArray);
      }
      return fieldInfoArray;
    }

    public static FieldInfo[] GetPublicFields(Type t)
    {
      FieldInfo[] fieldInfoArray = (FieldInfo[]) null;
      if (!TaskUtility.publicFieldsLookup.TryGetValue(t, out fieldInfoArray))
      {
        List<FieldInfo> fieldList = ObjectPool.Get<List<FieldInfo>>();
        fieldList.Clear();
        BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;
        TaskUtility.GetFields(t, ref fieldList, (int) bindingFlags);
        fieldInfoArray = fieldList.ToArray();
        ObjectPool.Return<List<FieldInfo>>(fieldList);
        TaskUtility.publicFieldsLookup.Add(t, fieldInfoArray);
      }
      return fieldInfoArray;
    }

    private static void GetFields(Type t, ref List<FieldInfo> fieldList, int flags)
    {
      if (t == null || t.Equals(typeof (ParentTask)) || (t.Equals(typeof (Task)) || t.Equals(typeof (SharedVariable))))
        return;
      foreach (FieldInfo field in t.GetFields((BindingFlags) flags))
        fieldList.Add(field);
      TaskUtility.GetFields(t.BaseType, ref fieldList, flags);
    }

    public static Type GetTypeWithinAssembly(string typeName)
    {
      Type type1;
      if (TaskUtility.typeLookup.TryGetValue(typeName, out type1))
        return type1;
      Type type2 = Type.GetType(typeName);
      if (type2 == null)
      {
        if (TaskUtility.loadedAssemblies == null)
        {
          TaskUtility.loadedAssemblies = new List<string>();
          foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            TaskUtility.loadedAssemblies.Add(assembly.FullName);
        }
        for (int index = 0; index < TaskUtility.loadedAssemblies.Count; ++index)
        {
          type2 = Type.GetType(typeName + "," + TaskUtility.loadedAssemblies[index]);
          if (type2 != null)
            break;
        }
      }
      if (type2 != null)
        TaskUtility.typeLookup.Add(typeName, type2);
      return type2;
    }

    public static bool CompareType(Type t, string typeName)
    {
      Type o = Type.GetType(typeName + ", Assembly-CSharp") ?? Type.GetType(typeName + ", Assembly-CSharp-firstpass");
      return t.Equals(o);
    }

    public static bool HasAttribute(FieldInfo field, Type attribute)
    {
      if (field == null)
        return false;
      Dictionary<Type, bool> dictionary;
      if (!TaskUtility.hasFieldLookup.TryGetValue(field, out dictionary))
      {
        dictionary = new Dictionary<Type, bool>();
        TaskUtility.hasFieldLookup.Add(field, dictionary);
      }
      bool flag;
      if (!dictionary.TryGetValue(attribute, out flag))
      {
        flag = field.GetCustomAttributes(attribute, false).Length > 0;
        dictionary.Add(attribute, flag);
      }
      return flag;
    }
  }
}
