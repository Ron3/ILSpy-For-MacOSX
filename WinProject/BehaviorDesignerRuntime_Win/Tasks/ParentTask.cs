// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.Tasks.ParentTask
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
  public abstract class ParentTask : Task
  {
    [SerializeField]
    protected List<Task> children;

    public List<Task> Children
    {
      get
      {
        return this.children;
      }
      private set
      {
        this.children = value;
      }
    }

    public virtual int MaxChildren()
    {
      return int.MaxValue;
    }

    public virtual bool CanRunParallelChildren()
    {
      return false;
    }

    public virtual int CurrentChildIndex()
    {
      return 0;
    }

    public virtual bool CanExecute()
    {
      return true;
    }

    public virtual TaskStatus Decorate(TaskStatus status)
    {
      return status;
    }

    public virtual bool CanReevaluate()
    {
      return false;
    }

    public virtual bool OnReevaluationStarted()
    {
      return false;
    }

    public virtual void OnReevaluationEnded(TaskStatus status)
    {
    }

    public virtual void OnChildExecuted(TaskStatus childStatus)
    {
    }

    public virtual void OnChildExecuted(int childIndex, TaskStatus childStatus)
    {
    }

    public virtual void OnChildStarted()
    {
    }

    public virtual void OnChildStarted(int childIndex)
    {
    }

    public virtual TaskStatus OverrideStatus(TaskStatus status)
    {
      return status;
    }

    public virtual TaskStatus OverrideStatus()
    {
      return TaskStatus.Running;
    }

    public virtual void OnConditionalAbort(int childIndex)
    {
    }

    public override float GetUtility()
    {
      float num = 0.0f;
      if (this.children != null)
      {
        for (int index = 0; index < this.children.Count; ++index)
        {
          if (this.children[index] != null && !this.children[index].Disabled)
            num += this.children[index].GetUtility();
        }
      }
      return num;
    }

    public override void OnDrawGizmos()
    {
      if (this.children == null)
        return;
      for (int index = 0; index < this.children.Count; ++index)
      {
        if (this.children[index] != null && !this.children[index].Disabled)
          this.children[index].OnDrawGizmos();
      }
    }

    public void AddChild(Task child, int index)
    {
      if (this.children == null)
        this.children = new List<Task>();
      this.children.Insert(index, child);
    }

    public void ReplaceAddChild(Task child, int index)
    {
      if (this.children != null && index < this.children.Count)
        this.children[index] = child;
      else
        this.AddChild(child, index);
    }
  }
}
