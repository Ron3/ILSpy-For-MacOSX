// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.IVariableSource
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System.Collections.Generic;

namespace BehaviorDesigner.Runtime
{
  public interface IVariableSource
  {
    SharedVariable GetVariable(string name);

    List<SharedVariable> GetAllVariables();

    void SetVariable(string name, SharedVariable sharedVariable);

    void UpdateVariableName(SharedVariable sharedVariable, string name);

    void SetAllVariables(List<SharedVariable> variables);
  }
}
