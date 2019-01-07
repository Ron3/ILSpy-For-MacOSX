using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public static class ErrorCheck
	{
		private static HashSet<int> fieldHashes = new HashSet<int>();

		public static List<ErrorDetails> CheckForErrors(BehaviorSource behaviorSource)
		{
			if (behaviorSource == null || behaviorSource.get_Owner() == null)
			{
				return null;
			}
			List<ErrorDetails> result = null;
			ErrorCheck.fieldHashes.Clear();
			bool flag = AssetDatabase.GetAssetPath(behaviorSource.get_Owner().GetObject()).get_Length() > 0;
			if (behaviorSource.get_EntryTask() != null)
			{
				ErrorCheck.CheckTaskForErrors(behaviorSource.get_EntryTask(), flag, ref result);
				if (behaviorSource.get_RootTask() == null)
				{
					ErrorCheck.AddError(ref result, ErrorDetails.ErrorType.MissingChildren, behaviorSource.get_EntryTask(), null);
				}
			}
			if (behaviorSource.get_RootTask() != null)
			{
				ErrorCheck.CheckTaskForErrors(behaviorSource.get_RootTask(), flag, ref result);
			}
			if (!EditorApplication.get_isPlaying() && flag && behaviorSource.get_Variables() != null)
			{
				for (int i = 0; i < behaviorSource.get_Variables().get_Count(); i++)
				{
					object value = behaviorSource.get_Variables().get_Item(i).GetValue();
					if (value is Object && AssetDatabase.GetAssetPath(value as Object).get_Length() == 0)
					{
						ErrorCheck.AddError(ref result, ErrorDetails.ErrorType.InvalidVariableReference, null, behaviorSource.get_Variables().get_Item(i).get_Name());
					}
				}
			}
			return result;
		}

		private static void CheckTaskForErrors(Task task, bool projectLevelBehavior, ref List<ErrorDetails> errorDetails)
		{
			if (task.get_Disabled())
			{
				return;
			}
			if (task is UnknownTask || task is UnknownParentTask)
			{
				ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.UnknownTask, task, null);
			}
			if (task.GetType().GetCustomAttributes(typeof(SkipErrorCheckAttribute), false).Length == 0)
			{
				FieldInfo[] allFields = TaskUtility.GetAllFields(task.GetType());
				for (int i = 0; i < allFields.Length; i++)
				{
					if (allFields[i].get_IsPublic() || TaskUtility.HasAttribute(allFields[i], typeof(SerializableAttribute)))
					{
						ErrorCheck.CheckField(task, projectLevelBehavior, ref errorDetails, allFields[i], 0, allFields[i].GetValue(task));
					}
				}
			}
			if (task is ParentTask && task.get_NodeData().get_NodeDesigner() != null && !(task.get_NodeData().get_NodeDesigner() as NodeDesigner).IsEntryDisplay)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.get_Children() == null || parentTask.get_Children().get_Count() == 0)
				{
					ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.MissingChildren, task, null);
				}
				else
				{
					for (int j = 0; j < parentTask.get_Children().get_Count(); j++)
					{
						ErrorCheck.CheckTaskForErrors(parentTask.get_Children().get_Item(j), projectLevelBehavior, ref errorDetails);
					}
				}
			}
		}

		private static void CheckField(Task task, bool projectLevelBehavior, ref List<ErrorDetails> errorDetails, FieldInfo field, int hashPrefix, object value)
		{
			if (value == null)
			{
				return;
			}
			int num = hashPrefix + field.get_Name().GetHashCode() + field.GetHashCode();
			if (ErrorCheck.fieldHashes.Contains(num))
			{
				return;
			}
			ErrorCheck.fieldHashes.Add(num);
			if (TaskUtility.HasAttribute(field, typeof(RequiredFieldAttribute)) && !ErrorCheck.IsRequiredFieldValid(field.get_FieldType(), value))
			{
				ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.RequiredField, task, field.get_Name());
			}
			if (typeof(SharedVariable).IsAssignableFrom(field.get_FieldType()))
			{
				SharedVariable sharedVariable = value as SharedVariable;
				if (sharedVariable != null)
				{
					if (sharedVariable.get_IsShared() && string.IsNullOrEmpty(sharedVariable.get_Name()) && !TaskUtility.HasAttribute(field, typeof(SharedRequiredAttribute)))
					{
						ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.SharedVariable, task, field.get_Name());
					}
					object value2 = sharedVariable.GetValue();
					if (!EditorApplication.get_isPlaying() && projectLevelBehavior && !sharedVariable.get_IsShared() && value2 is Object && AssetDatabase.GetAssetPath(value2 as Object).get_Length() <= 0)
					{
						ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.InvalidTaskReference, task, field.get_Name());
					}
				}
			}
			else if (value is Object)
			{
				bool flag = AssetDatabase.GetAssetPath(value as Object).get_Length() > 0;
				if (!EditorApplication.get_isPlaying() && projectLevelBehavior && !flag)
				{
					ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.InvalidTaskReference, task, field.get_Name());
				}
			}
			else if (!typeof(Delegate).IsAssignableFrom(field.get_FieldType()) && !typeof(Task).IsAssignableFrom(field.get_FieldType()) && !typeof(Behavior).IsAssignableFrom(field.get_FieldType()) && (field.get_FieldType().get_IsClass() || (field.get_FieldType().get_IsValueType() && !field.get_FieldType().get_IsPrimitive())))
			{
				FieldInfo[] allFields = TaskUtility.GetAllFields(field.get_FieldType());
				for (int i = 0; i < allFields.Length; i++)
				{
					ErrorCheck.CheckField(task, projectLevelBehavior, ref errorDetails, allFields[i], num, allFields[i].GetValue(value));
				}
			}
		}

		private static void AddError(ref List<ErrorDetails> errorDetails, ErrorDetails.ErrorType type, Task task, string fieldName)
		{
			if (errorDetails == null)
			{
				errorDetails = new List<ErrorDetails>();
			}
			errorDetails.Add(new ErrorDetails(type, task, fieldName));
		}

		public static bool IsRequiredFieldValid(Type fieldType, object value)
		{
			if (value == null || value.Equals(null))
			{
				return false;
			}
			if (typeof(IList).IsAssignableFrom(fieldType))
			{
				IList list = value as IList;
				if (list.get_Count() == 0)
				{
					return false;
				}
				for (int i = 0; i < list.get_Count(); i++)
				{
					if (list.get_Item(i) == null || list.get_Item(i).Equals(null))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
