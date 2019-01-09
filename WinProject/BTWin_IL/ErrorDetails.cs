// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.ErrorDetails
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime.Tasks;
using System;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  [Serializable]
  public class ErrorDetails
  {
    [SerializeField]
    private ErrorDetails.ErrorType mType;
    [SerializeField]
    private NodeDesigner mNodeDesigner;
    [SerializeField]
    private string mTaskFriendlyName;
    [SerializeField]
    private string mTaskType;
    [SerializeField]
    private string mFieldName;

    public ErrorDetails(ErrorDetails.ErrorType type, Task task, string fieldName)
    {
      this.mType = type;
      if (task != null)
      {
        this.mNodeDesigner = task.get_NodeData().get_NodeDesigner() as NodeDesigner;
        this.mTaskFriendlyName = task.get_FriendlyName();
        this.mTaskType = ((object) task).GetType().ToString();
      }
      this.mFieldName = fieldName;
    }

    public ErrorDetails.ErrorType Type
    {
      get
      {
        return this.mType;
      }
    }

    public NodeDesigner NodeDesigner
    {
      get
      {
        return this.mNodeDesigner;
      }
    }

    public string TaskFriendlyName
    {
      get
      {
        return this.mTaskFriendlyName;
      }
    }

    public string TaskType
    {
      get
      {
        return this.mTaskType;
      }
    }

    public string FieldName
    {
      get
      {
        return this.mFieldName;
      }
    }

    public enum ErrorType
    {
      RequiredField,
      SharedVariable,
      MissingChildren,
      UnknownTask,
      InvalidTaskReference,
      InvalidVariableReference,
    }
  }
}
