using System;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime
{
	public static class ObjectPool
	{
		private static Dictionary<Type, object> poolDictionary = new Dictionary<Type, object>();

		public static T Get<T>()
		{
			if (ObjectPool.poolDictionary.ContainsKey(typeof(T)))
			{
				Stack<T> stack = ObjectPool.poolDictionary.get_Item(typeof(T)) as Stack<T>;
				if (stack.Count > 0)
				{
					return stack.Pop();
				}
			}
			return (T)((object)TaskUtility.CreateInstance(typeof(T)));
		}

		public static void Return<T>(T obj)
		{
			if (obj == null)
			{
				return;
			}
			object obj2;
			if (ObjectPool.poolDictionary.TryGetValue(typeof(T), ref obj2))
			{
				Stack<T> stack = obj2 as Stack<T>;
				stack.Push(obj);
			}
			else
			{
				Stack<T> stack2 = new Stack<T>();
				stack2.Push(obj);
				ObjectPool.poolDictionary.Add(typeof(T), stack2);
			}
		}

		public static void Clear()
		{
			ObjectPool.poolDictionary.Clear();
		}
	}
}
