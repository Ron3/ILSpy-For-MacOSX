// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.GlobalVariables
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
  public class GlobalVariables : ScriptableObject, IVariableSource
  {
    private static GlobalVariables instance;
    [SerializeField]
    private List<SharedVariable> mVariables;
    private Dictionary<string, int> mSharedVariableIndex;
    [SerializeField]
    private VariableSerializationData mVariableData;
    [SerializeField]
    private string mVersion;

    public GlobalVariables()
    {
      base.\u002Ector();
    }

    public static GlobalVariables Instance
    {
      get
      {
        if (Object.op_Equality((Object) GlobalVariables.instance, (Object) null))
        {
          GlobalVariables.instance = Resources.Load("BehaviorDesignerGlobalVariables", typeof (GlobalVariables)) as GlobalVariables;
          if (Object.op_Inequality((Object) GlobalVariables.instance, (Object) null))
            GlobalVariables.instance.CheckForSerialization(false);
        }
        return GlobalVariables.instance;
      }
    }

    public List<SharedVariable> Variables
    {
      get
      {
        return this.mVariables;
      }
      set
      {
        this.mVariables = value;
        this.UpdateVariablesIndex();
      }
    }

    public VariableSerializationData VariableData
    {
      get
      {
        return this.mVariableData;
      }
      set
      {
        this.mVariableData = value;
      }
    }

    public string Version
    {
      get
      {
        return this.mVersion;
      }
      set
      {
        this.mVersion = value;
      }
    }

    public void CheckForSerialization(bool force)
    {
      if (!force && this.mVariables != null && (this.mVariables.Count <= 0 || this.mVariables[0] != null))
        return;
      if (this.VariableData != null && !string.IsNullOrEmpty(this.VariableData.JSONSerialization))
        JSONDeserialization.Load(this.VariableData.JSONSerialization, this, this.mVersion);
      else
        BinaryDeserialization.Load(this, this.mVersion);
    }

    public SharedVariable GetVariable(string name)
    {
      if (name == null)
        return (SharedVariable) null;
      this.CheckForSerialization(false);
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
      this.CheckForSerialization(false);
      return this.mVariables;
    }

    public void SetVariable(string name, SharedVariable sharedVariable)
    {
      this.CheckForSerialization(false);
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
        else
          mVariable.SetValue(sharedVariable.GetValue());
      }
      else
      {
        this.mVariables.Add(sharedVariable);
        this.UpdateVariablesIndex();
      }
    }

    public void SetVariableValue(string name, object value)
    {
      SharedVariable variable = this.GetVariable(name);
      if (variable == null)
        return;
      variable.SetValue(value);
      variable.ValueChanged();
    }

    public void UpdateVariableName(SharedVariable sharedVariable, string name)
    {
      this.CheckForSerialization(false);
      sharedVariable.Name = name;
      this.UpdateVariablesIndex();
    }

    public void SetAllVariables(List<SharedVariable> variables)
    {
      this.mVariables = variables;
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
  }
}
