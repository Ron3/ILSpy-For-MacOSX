// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.ErrorCheck
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

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
        return (List<ErrorDetails>) null;
      List<ErrorDetails> errorDetails = (List<ErrorDetails>) null;
      ErrorCheck.fieldHashes.Clear();
      bool projectLevelBehavior = AssetDatabase.GetAssetPath(behaviorSource.get_Owner().GetObject()).Length > 0;
      if (behaviorSource.get_EntryTask() != null)
      {
        ErrorCheck.CheckTaskForErrors(behaviorSource.get_EntryTask(), projectLevelBehavior, ref errorDetails);
        if (behaviorSource.get_RootTask() == null)
          ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.MissingChildren, behaviorSource.get_EntryTask(), (string) null);
      }
      if (behaviorSource.get_RootTask() != null)
        ErrorCheck.CheckTaskForErrors(behaviorSource.get_RootTask(), projectLevelBehavior, ref errorDetails);
      if (!EditorApplication.get_isPlaying() && projectLevelBehavior && behaviorSource.get_Variables() != null)
      {
        for (int index = 0; index < behaviorSource.get_Variables().Count; ++index)
        {
          object obj = behaviorSource.get_Variables()[index].GetValue();
          if (obj is Object && AssetDatabase.GetAssetPath(obj as Object).Length == 0)
            ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.InvalidVariableReference, (Task) null, behaviorSource.get_Variables()[index].get_Name());
        }
      }
      return errorDetails;
    }

    private static void CheckTaskForErrors(
      Task task,
      bool projectLevelBehavior,
      ref List<ErrorDetails> errorDetails)
    {
      if (task.get_Disabled())
        return;
      if (task is UnknownTask || task is UnknownParentTask)
        ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.UnknownTask, task, (string) null);
      if (((object) task).GetType().GetCustomAttributes(typeof (SkipErrorCheckAttribute), false).Length == 0)
      {
        FieldInfo[] allFields = TaskUtility.GetAllFields(((object) task).GetType());
        for (int index = 0; index < allFields.Length; ++index)
        {
          if (allFields[index].IsPublic || TaskUtility.HasAttribute(allFields[index], typeof (SerializableAttribute)))
            ErrorCheck.CheckField(task, projectLevelBehavior, ref errorDetails, allFields[index], 0, allFields[index].GetValue((object) task));
        }
      }
      if (!(task is ParentTask) || task.get_NodeData().get_NodeDesigner() == null || (task.get_NodeData().get_NodeDesigner() as NodeDesigner).IsEntryDisplay)
        return;
      ParentTask parentTask = task as ParentTask;
      if (parentTask.get_Children() == null || parentTask.get_Children().Count == 0)
      {
        ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.MissingChildren, task, (string) null);
      }
      else
      {
        for (int index = 0; index < parentTask.get_Children().Count; ++index)
          ErrorCheck.CheckTaskForErrors(parentTask.get_Children()[index], projectLevelBehavior, ref errorDetails);
      }
    }

    private static void CheckField(
      Task task,
      bool projectLevelBehavior,
      ref List<ErrorDetails> errorDetails,
      FieldInfo field,
      int hashPrefix,
      object value)
    {
      if (value == null)
        return;
      int hashPrefix1 = hashPrefix + field.Name.GetHashCode() + field.GetHashCode();
      if (ErrorCheck.fieldHashes.Contains(hashPrefix1))
        return;
      ErrorCheck.fieldHashes.Add(hashPrefix1);
      if (TaskUtility.HasAttribute(field, typeof (RequiredFieldAttribute)) && !ErrorCheck.IsRequiredFieldValid(field.FieldType, value))
        ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.RequiredField, task, field.Name);
      if (typeof (SharedVariable).IsAssignableFrom(field.FieldType))
      {
        SharedVariable sharedVariable = value as SharedVariable;
        if (sharedVariable == null)
          return;
        if (sharedVariable.get_IsShared() && string.IsNullOrEmpty(sharedVariable.get_Name()) && !TaskUtility.HasAttribute(field, typeof (SharedRequiredAttribute)))
          ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.SharedVariable, task, field.Name);
        object obj = sharedVariable.GetValue();
        if (EditorApplication.get_isPlaying() || !projectLevelBehavior || (sharedVariable.get_IsShared() || !(obj is Object)) || AssetDatabase.GetAssetPath(obj as Object).Length > 0)
          return;
        ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.InvalidTaskReference, task, field.Name);
      }
      else if (value is Object)
      {
        bool flag = AssetDatabase.GetAssetPath(value as Object).Length > 0;
        if (EditorApplication.get_isPlaying() || !projectLevelBehavior || flag)
          return;
        ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.InvalidTaskReference, task, field.Name);
      }
      else
      {
        if (typeof (Delegate).IsAssignableFrom(field.FieldType) || typeof (Task).IsAssignableFrom(field.FieldType) || typeof (Behavior).IsAssignableFrom(field.FieldType) || !field.FieldType.IsClass && (!field.FieldType.IsValueType || field.FieldType.IsPrimitive))
          return;
        FieldInfo[] allFields = TaskUtility.GetAllFields(field.FieldType);
        for (int index = 0; index < allFields.Length; ++index)
          ErrorCheck.CheckField(task, projectLevelBehavior, ref errorDetails, allFields[index], hashPrefix1, allFields[index].GetValue(value));
      }
    }

    private static void AddError(
      ref List<ErrorDetails> errorDetails,
      ErrorDetails.ErrorType type,
      Task task,
      string fieldName)
    {
      if (errorDetails == null)
        errorDetails = new List<ErrorDetails>();
      errorDetails.Add(new ErrorDetails(type, task, fieldName));
    }

    public static bool IsRequiredFieldValid(Type fieldType, object value)
    {
      if (value == null || value.Equals((object) null))
        return false;
      if (typeof (IList).IsAssignableFrom(fieldType))
      {
        IList list = value as IList;
        if (list.Count == 0)
          return false;
        for (int index = 0; index < list.Count; ++index)
        {
          if (list[index] == null || list[index].Equals((object) null))
            return false;
        }
      }
      return true;
    }
  }
}
