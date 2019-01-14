// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.ObjectPool
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime
{
  public static class ObjectPool
  {
    private static Dictionary<Type, object> poolDictionary = new Dictionary<Type, object>();

    public static T Get<T>()
    {
      if (ObjectPool.poolDictionary.ContainsKey(typeof (T)))
      {
        Stack<T> pool = ObjectPool.poolDictionary[typeof (T)] as Stack<T>;
        if (pool.Count > 0)
          return pool.Pop();
      }
      return (T) TaskUtility.CreateInstance(typeof (T));
    }

    public static void Return<T>(T obj)
    {
      if ((object) obj == null)
        return;
      object obj1;
      if (ObjectPool.poolDictionary.TryGetValue(typeof (T), out obj1))
      {
        (obj1 as Stack<T>).Push(obj);
      }
      else
      {
        Stack<T> objStack = new Stack<T>();
        objStack.Push(obj);
        ObjectPool.poolDictionary.Add(typeof (T), (object) objStack);
      }
    }

    public static void Clear()
    {
      ObjectPool.poolDictionary.Clear();
    }
  }
}
