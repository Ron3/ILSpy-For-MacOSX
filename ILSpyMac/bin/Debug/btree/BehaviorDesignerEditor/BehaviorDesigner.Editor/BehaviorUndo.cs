using System;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public class BehaviorUndo
	{
		public static void RegisterUndo(string undoName, Object undoObject)
		{
			Undo.RecordObject(undoObject, undoName);
		}

		public static Component AddComponent(GameObject undoObject, Type type)
		{
			return Undo.AddComponent(undoObject, type);
		}

		public static void DestroyObject(Object undoObject, bool registerScene)
		{
			Undo.DestroyObjectImmediate(undoObject);
		}
	}
}
