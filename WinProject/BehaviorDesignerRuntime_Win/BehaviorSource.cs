// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.BehaviorSource
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
  [Serializable]
  public class BehaviorSource : IVariableSource
  {
    public string behaviorName = "Behavior";
    public string behaviorDescription = string.Empty;
    private int behaviorID = -1;
    private Task mEntryTask;
    private Task mRootTask;
    private List<Task> mDetachedTasks;
    private List<SharedVariable> mVariables;
    private Dictionary<string, int> mSharedVariableIndex;
    [NonSerialized]
    private bool mHasSerialized;
    [SerializeField]
    private TaskSerializationData mTaskData;
    [SerializeField]
    private IBehavior mOwner;

    public BehaviorSource()
    {
    }

    public BehaviorSource(IBehavior owner)
    {
      this.Initialize(owner);
    }

    public int BehaviorID
    {
      get
      {
        return this.behaviorID;
      }
      set
      {
        this.behaviorID = value;
      }
    }

    public Task EntryTask
    {
      get
      {
        return this.mEntryTask;
      }
      set
      {
        this.mEntryTask = value;
      }
    }

    public Task RootTask
    {
      get
      {
        return this.mRootTask;
      }
      set
      {
        this.mRootTask = value;
      }
    }

    public List<Task> DetachedTasks
    {
      get
      {
        return this.mDetachedTasks;
      }
      set
      {
        this.mDetachedTasks = value;
      }
    }

    public List<SharedVariable> Variables
    {
      get
      {
        this.CheckForSerialization(false, (BehaviorSource) null);
        return this.mVariables;
      }
      set
      {
        this.mVariables = value;
        this.UpdateVariablesIndex();
      }
    }

    public bool HasSerialized
    {
      get
      {
        return this.mHasSerialized;
      }
      set
      {
        this.mHasSerialized = value;
      }
    }

    public TaskSerializationData TaskData
    {
      get
      {
        return this.mTaskData;
      }
      set
      {
        this.mTaskData = value;
      }
    }

    public IBehavior Owner
    {
      get
      {
        return this.mOwner;
      }
      set
      {
        this.mOwner = value;
      }
    }

    public void Initialize(IBehavior owner)
    {
      this.mOwner = owner;
    }

    public void Save(Task entryTask, Task rootTask, List<Task> detachedTasks)
    {
      this.mEntryTask = entryTask;
      this.mRootTask = rootTask;
      this.mDetachedTasks = detachedTasks;
    }

    public void Load(out Task entryTask, out Task rootTask, out List<Task> detachedTasks)
    {
      entryTask = this.mEntryTask;
      rootTask = this.mRootTask;
      detachedTasks = this.mDetachedTasks;
    }

    public bool CheckForSerialization(bool force, BehaviorSource behaviorSource = null)
    {
      if ((behaviorSource == null ? this.HasSerialized : behaviorSource.HasSerialized) && !force)
        return false;
      if (behaviorSource != null)
        behaviorSource.HasSerialized = true;
      else
        this.HasSerialized = true;
      if (this.mTaskData != null && !string.IsNullOrEmpty(this.mTaskData.JSONSerialization))
        JSONDeserialization.Load(this.mTaskData, behaviorSource != null ? behaviorSource : this);
      else
        BinaryDeserialization.Load(this.mTaskData, behaviorSource != null ? behaviorSource : this);
      return true;
    }

    public SharedVariable GetVariable(string name)
    {
      if (name == null)
        return (SharedVariable) null;
      this.CheckForSerialization(false, (BehaviorSource) null);
      if (this.mVariables != null)
      {
        if (this.mSharedVariableIndex == null || this.mSharedVariableIndex.Count != this.mVariables.Count)
          this.UpdateVariablesIndex();
        int index;
        if (this.mSharedVariableIndex.TryGetValue(name, out index))
          return this.mVariables[index];
      }
      return (SharedVariable) null;
    }

    public List<SharedVariable> GetAllVariables()
    {
      this.CheckForSerialization(false, (BehaviorSource) null);
      return this.mVariables;
    }

    public void SetVariable(string name, SharedVariable sharedVariable)
    {
      if (this.mVariables == null)
        this.mVariables = new List<SharedVariable>();
      else if (this.mSharedVariableIndex == null)
        this.UpdateVariablesIndex();
      sharedVariable.Name = name;
      int index;
      if (this.mSharedVariableIndex != null && this.mSharedVariableIndex.TryGetValue(name, out index))
      {
        SharedVariable mVariable = this.mVariables[index];
        if (!mVariable.GetType().Equals(typeof (SharedVariable)) && !mVariable.GetType().Equals(sharedVariable.GetType()))
          Debug.LogError((object) string.Format("Error: Unable to set SharedVariable {0} - the variable type {1} does not match the existing type {2}", (object) name, (object) mVariable.GetType(), (object) sharedVariable.GetType()));
        else if (!string.IsNullOrEmpty(sharedVariable.PropertyMapping))
        {
          mVariable.PropertyMappingOwner = sharedVariable.PropertyMappingOwner;
          mVariable.PropertyMapping = sharedVariable.PropertyMapping;
          mVariable.InitializePropertyMapping(this);
        }
        else
          mVariable.SetValue(sharedVariable.GetValue());
      }
      else
      {
        this.mVariables.Add(sharedVariable);
        this.UpdateVariablesIndex();
      }
    }

    public void UpdateVariableName(SharedVariable sharedVariable, string name)
    {
      this.CheckForSerialization(false, (BehaviorSource) null);
      sharedVariable.Name = name;
      this.UpdateVariablesIndex();
    }

    public void SetAllVariables(List<SharedVariable> variables)
    {
      this.mVariables = variables;
      this.UpdateVariablesIndex();
    }

    private void UpdateVariablesIndex()
    {
      if (this.mVariables == null)
      {
        if (this.mSharedVariableIndex == null)
          return;
        this.mSharedVariableIndex = (Dictionary<string, int>) null;
      }
      else
      {
        if (this.mSharedVariableIndex == null)
          this.mSharedVariableIndex = new Dictionary<string, int>(this.mVariables.Count);
        else
          this.mSharedVariableIndex.Clear();
        for (int index = 0; index < this.mVariables.Count; ++index)
        {
          if (this.mVariables[index] != null)
            this.mSharedVariableIndex.Add(this.mVariables[index].Name, index);
        }
      }
    }

    public override string ToString()
    {
      if (this.mOwner == null || Object.op_Equality(this.mOwner.GetObject(), (Object) null))
        return this.behaviorName;
      return string.Format("{0} - {1}", (object) this.Owner.GetOwnerName(), (object) this.behaviorName);
    }
  }
}
