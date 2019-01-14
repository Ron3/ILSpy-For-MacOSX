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

		private static List<string> loadedAssemblies = null;

		private static Dictionary<Type, FieldInfo[]> allFieldsLookup = new Dictionary<Type, FieldInfo[]>();

		private static Dictionary<Type, FieldInfo[]> publicFieldsLookup = new Dictionary<Type, FieldInfo[]>();

		private static Dictionary<FieldInfo, Dictionary<Type, bool>> hasFieldLookup = new Dictionary<FieldInfo, Dictionary<Type, bool>>();

		public static object CreateInstance(Type t)
		{
			if (t.get_IsGenericType() && t.GetGenericTypeDefinition() == typeof(Nullable))
			{
				t = Nullable.GetUnderlyingType(t);
			}
			return Activator.CreateInstance(t, true);
		}

		public static FieldInfo[] GetAllFields(Type t)
		{
			FieldInfo[] array = null;
			if (!TaskUtility.allFieldsLookup.TryGetValue(t, ref array))
			{
				List<FieldInfo> list = ObjectPool.Get<List<FieldInfo>>();
				list.Clear();
				BindingFlags flags = 54;
				TaskUtility.GetFields(t, ref list, flags);
				array = list.ToArray();
				ObjectPool.Return<List<FieldInfo>>(list);
				TaskUtility.allFieldsLookup.Add(t, array);
			}
			return array;
		}

		public static FieldInfo[] GetPublicFields(Type t)
		{
			FieldInfo[] array = null;
			if (!TaskUtility.publicFieldsLookup.TryGetValue(t, ref array))
			{
				List<FieldInfo> list = ObjectPool.Get<List<FieldInfo>>();
				list.Clear();
				BindingFlags flags = 22;
				TaskUtility.GetFields(t, ref list, flags);
				array = list.ToArray();
				ObjectPool.Return<List<FieldInfo>>(list);
				TaskUtility.publicFieldsLookup.Add(t, array);
			}
			return array;
		}

		private static void GetFields(Type t, ref List<FieldInfo> fieldList, int flags)
		{
			if (t == null || t.Equals(typeof(ParentTask)) || t.Equals(typeof(Task)) || t.Equals(typeof(SharedVariable)))
			{
				return;
			}
			FieldInfo[] fields = t.GetFields(flags);
			for (int i = 0; i < fields.Length; i++)
			{
				fieldList.Add(fields[i]);
			}
			TaskUtility.GetFields(t.get_BaseType(), ref fieldList, flags);
		}

		public static Type GetTypeWithinAssembly(string typeName)
		{
			Type type;
			if (TaskUtility.typeLookup.TryGetValue(typeName, ref type))
			{
				return type;
			}
			type = Type.GetType(typeName);
			if (type == null)
			{
				if (TaskUtility.loadedAssemblies == null)
				{
					TaskUtility.loadedAssemblies = new List<string>();
					Assembly[] assemblies = AppDomain.get_CurrentDomain().GetAssemblies();
					for (int i = 0; i < assemblies.Length; i++)
					{
						TaskUtility.loadedAssemblies.Add(assemblies[i].get_FullName());
					}
				}
				for (int j = 0; j < TaskUtility.loadedAssemblies.get_Count(); j++)
				{
					type = Type.GetType(typeName + "," + TaskUtility.loadedAssemblies.get_Item(j));
					if (type != null)
					{
						break;
					}
				}
			}
			if (type != null)
			{
				TaskUtility.typeLookup.Add(typeName, type);
			}
			return type;
		}

		public static bool CompareType(Type t, string typeName)
		{
			Type type = Type.GetType(typeName + ", Assembly-CSharp");
			if (type == null)
			{
				type = Type.GetType(typeName + ", Assembly-CSharp-firstpass");
			}
			return t.Equals(type);
		}

		public static bool HasAttribute(FieldInfo field, Type attribute)
		{
			if (field == null)
			{
				return false;
			}
			Dictionary<Type, bool> dictionary;
			if (!TaskUtility.hasFieldLookup.TryGetValue(field, ref dictionary))
			{
				dictionary = new Dictionary<Type, bool>();
				TaskUtility.hasFieldLookup.Add(field, dictionary);
			}
			bool flag;
			if (!dictionary.TryGetValue(attribute, ref flag))
			{
				flag = (field.GetCustomAttributes(attribute, false).Length > 0);
				dictionary.Add(attribute, flag);
			}
			return flag;
		}
	}
}