// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.SharedNamedVariable
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System;

namespace BehaviorDesigner.Runtime
{
  [Serializable]
  public class SharedNamedVariable : SharedVariable<NamedVariable>
  {
    public SharedNamedVariable()
    {
      this.mValue = new NamedVariable();
    }

    public static implicit operator SharedNamedVariable(NamedVariable value)
    {
      SharedNamedVariable sharedNamedVariable = new SharedNamedVariable();
      sharedNamedVariable.mValue = value;
      return sharedNamedVariable;
    }
  }
}
