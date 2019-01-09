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
			if (behaviorSource == null || behaviorSource.Owner == null)
			{
				return null;
			}
			List<ErrorDetails> result = null;
			ErrorCheck.fieldHashes.Clear();
			bool flag = AssetDatabase.GetAssetPath(behaviorSource.Owner.GetObject()).Length > 0;
			if (behaviorSource.EntryTask != null)
			{
				ErrorCheck.CheckTaskForErrors(behaviorSource.EntryTask, flag, ref result);
				if (behaviorSource.RootTask == null)
				{
					ErrorCheck.AddError(ref result, ErrorDetails.ErrorType.MissingChildren, behaviorSource.EntryTask, null);
				}
			}
			if (behaviorSource.RootTask != null)
			{
				ErrorCheck.CheckTaskForErrors(behaviorSource.RootTask, flag, ref result);
			}
			if (!EditorApplication.isPlaying && flag && behaviorSource.Variables != null)
			{
				for (int i = 0; i < behaviorSource.Variables.Count; i++)
				{
					object value = behaviorSource.Variables.get_Item(i).GetValue();
					if (value is Object && AssetDatabase.GetAssetPath(value as Object).Length == 0)
					{
						ErrorCheck.AddError(ref result, ErrorDetails.ErrorType.InvalidVariableReference, null, behaviorSource.Variables.get_Item(i).Name);
					}
				}
			}
			return result;
		}

		private static void CheckTaskForErrors(Task task, bool projectLevelBehavior, ref List<ErrorDetails> errorDetails)
		{
			if (task.Disabled)
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
					if (allFields[i].IsPublic || TaskUtility.HasAttribute(allFields[i], typeof(SerializableAttribute)))
					{
						ErrorCheck.CheckField(task, projectLevelBehavior, ref errorDetails, allFields[i], 0, allFields[i].GetValue(task));
					}
				}
			}
			if (task is ParentTask && task.NodeData.NodeDesigner != null && !(task.NodeData.NodeDesigner as NodeDesigner).IsEntryDisplay)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children == null || parentTask.Children.Count == 0)
				{
					ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.MissingChildren, task, null);
				}
				else
				{
					for (int j = 0; j < parentTask.Children.Count; j++)
					{
						ErrorCheck.CheckTaskForErrors(parentTask.Children.get_Item(j), projectLevelBehavior, ref errorDetails);
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
			int num = hashPrefix + field.Name.GetHashCode() + field.GetHashCode();
			if (ErrorCheck.fieldHashes.Contains(num))
			{
				return;
			}
			ErrorCheck.fieldHashes.Add(num);
			if (TaskUtility.HasAttribute(field, typeof(RequiredFieldAttribute)) && !ErrorCheck.IsRequiredFieldValid(field.FieldType, value))
			{
				ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.RequiredField, task, field.Name);
			}
			if (typeof(SharedVariable).IsAssignableFrom(field.FieldType))
			{
				SharedVariable sharedVariable = value as SharedVariable;
				if (sharedVariable != null)
				{
					if (sharedVariable.IsShared && string.IsNullOrEmpty(sharedVariable.Name) && !TaskUtility.HasAttribute(field, typeof(SharedRequiredAttribute)))
					{
						ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.SharedVariable, task, field.Name);
					}
					object value2 = sharedVariable.GetValue();
					if (!EditorApplication.isPlaying && projectLevelBehavior && !sharedVariable.IsShared && value2 is Object && AssetDatabase.GetAssetPath(value2 as Object).Length <= 0)
					{
						ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.InvalidTaskReference, task, field.Name);
					}
				}
			}
			else if (value is Object)
			{
				bool flag = AssetDatabase.GetAssetPath(value as Object).Length > 0;
				if (!EditorApplication.isPlaying && projectLevelBehavior && !flag)
				{
					ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.InvalidTaskReference, task, field.Name);
				}
			}
			else if (!typeof(Delegate).IsAssignableFrom(field.FieldType) && !typeof(Task).IsAssignableFrom(field.FieldType) && !typeof(Behavior).IsAssignableFrom(field.FieldType) && (field.FieldType.IsClass || (field.FieldType.IsValueType && !field.FieldType.IsPrimitive)))
			{
				FieldInfo[] allFields = TaskUtility.GetAllFields(field.FieldType);
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
				if (list.Count == 0)
				{
					return false;
				}
				for (int i = 0; i < list.Count; i++)
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
